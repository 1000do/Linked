using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stripe;

namespace CourseMarketplaceBE.Presentation.Controllers;

/// <summary>
/// Stripe Connect Webhook Controller.
///
/// Nhận và xử lý các sự kiện từ Stripe Dashboard → Developers → Webhooks.
///
/// ★ QUAN TRỌNG (Production Checklist):
///   1. Tạo Webhook Endpoint trên Stripe Dashboard:
///      URL: https://your-domain.com/api/webhooks/stripe-connect
///      Events cần đăng ký (chọn "Events on Connected accounts"):
///        - payout.paid          → Tiền đã về ngân hàng giảng viên ✅
///        - payout.failed        → Lỗi chuyển tiền về ngân hàng ❌
///        - payout.created       → Stripe bắt đầu xử lý lệnh chuyển tiền 🔵
///
///   2. Set biến môi trường:
///      STRIPE_CONNECT_WEBHOOK_SECRET=whsec_xxxx
///
///   3. Test local với Stripe CLI:
///      stripe listen --forward-connect-to localhost:5001/api/webhooks/stripe-connect
///
/// SOLID — SRP: Controller chỉ parse + route event. Không chứa business logic.
/// SOLID — DIP: Phụ thuộc IAdminFinanceRepository qua constructor injection.
/// </summary>
[Route("api/webhooks")]
[ApiController]
public class StripeWebhookController : ControllerBase
{
    private readonly IAdminFinanceRepository _financeRepo;
    private readonly ILogger<StripeWebhookController> _logger;
    private readonly IHubContext<FinanceHub> _hubContext;
    private readonly string _connectWebhookSecret;

    public StripeWebhookController(
        IAdminFinanceRepository financeRepo,
        ILogger<StripeWebhookController> logger,
        IConfiguration configuration,
        IHubContext<FinanceHub> hubContext)
    {
        _financeRepo = financeRepo;
        _logger = logger;
        _hubContext = hubContext;
        _connectWebhookSecret = Environment.GetEnvironmentVariable("STRIPE_CONNECT_WEBHOOK_SECRET")
            ?? configuration["Stripe:ConnectWebhookSecret"]
            ?? string.Empty;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/webhooks/stripe-connect
    // ★ KHÔNG có [Authorize] — Stripe gọi trực tiếp, không dùng JWT
    // ★ PHẢI disable request body buffering để đọc raw bytes (yêu cầu của Stripe)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("stripe-connect")]
    public async Task<IActionResult> HandleConnectEvent()
    {
        // 1. Đọc raw body (Stripe yêu cầu original bytes để verify chữ ký)
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        // 2. Kiểm tra header chữ ký
        if (!Request.Headers.TryGetValue("Stripe-Signature", out var stripeSignature))
        {
            _logger.LogWarning("Webhook received without Stripe-Signature header.");
            return BadRequest("Missing Stripe-Signature header.");
        }

        // 3. Verify chữ ký — bảo vệ khỏi giả mạo request
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                _connectWebhookSecret,
                throwOnApiVersionMismatch: false
            );
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature verification FAILED.");
            return BadRequest($"Signature verification failed: {ex.Message}");
        }

        _logger.LogInformation(
            "Stripe Connect Webhook: Type={Type} | EventId={Id} | Account={Account}",
            stripeEvent.Type, stripeEvent.Id, stripeEvent.Account);

        // 4. Dispatch — xử lý lỗi nội bộ, luôn trả 200 để Stripe không retry
        try
        {
            await DispatchAsync(stripeEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Stripe event {Type} [{Id}]",
                stripeEvent.Type, stripeEvent.Id);
        }

        // 5. Luôn trả 200 OK
        return Ok();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Dispatcher: route đến handler tương ứng
    // ═══════════════════════════════════════════════════════════════════════
    private async Task DispatchAsync(Event e)
    {
        switch (e.Type)
        {
            case "payout.paid":
                await OnPayoutPaidAsync(e);
                break;
            case "payout.failed":
                await OnPayoutFailedAsync(e);
                break;
            case "payout.created":
                await OnPayoutCreatedAsync(e);
                break;
            default:
                _logger.LogDebug("Unhandled event type: {Type}", e.Type);
                break;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // payout.paid → Tiền đã về ngân hàng của giảng viên ✅
    // ═══════════════════════════════════════════════════════════════════════
    private async Task OnPayoutPaidAsync(Event e)
    {
        var stripePayout = e.Data.Object as Payout;
        if (stripePayout is null) return;

        _logger.LogInformation(
            "payout.paid | StripePayoutId={Id} | Amount={Amt} {Curr} | Account={Acc}",
            stripePayout.Id, stripePayout.Amount / 100.0,
            stripePayout.Currency.ToUpper(), e.Account);

        // Tìm các bản ghi DB đã được lưu stripe_payout_id lúc payout.created
        var dbPayouts = await _financeRepo.GetPayoutsByStripePayoutIdAsync(stripePayout.Id);
        if (dbPayouts == null || !dbPayouts.Any())
        {
            _logger.LogWarning("payout.paid: No DB record found for StripePayoutId={Id}", stripePayout.Id);
            return;
        }

        // ★ Tiền đã về ngân hàng — cập nhật trạng thái cuối cùng cho tất cả
        foreach (var p in dbPayouts)
        {
            p.PayoutStatus = "paid";
            p.PaidToBankAt = DateTime.UtcNow;
        }
        await _financeRepo.SaveChangesAsync();

        // 🔥 Gửi SignalR báo cho Admin (refresh toàn bảng)
        await _hubContext.Clients.Group("AdminFinance").SendAsync("UpdatePayoutStatus", new {
            refresh = true
        });

        _logger.LogInformation(
            "✅ {Count} InstructorPayouts → PAID TO BANK at {Time}",
            dbPayouts.Count, DateTime.UtcNow);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // payout.failed → Stripe không thể chuyển về ngân hàng ❌
    // ═══════════════════════════════════════════════════════════════════════
    private async Task OnPayoutFailedAsync(Event e)
    {
        var stripePayout = e.Data.Object as Payout;
        if (stripePayout is null) return;

        _logger.LogError(
            "payout.failed | StripePayoutId={Id} | Reason={Reason} | Account={Acc}",
            stripePayout.Id, stripePayout.FailureMessage, e.Account);

        var dbPayouts = await _financeRepo.GetPayoutsByStripePayoutIdAsync(stripePayout.Id);
        if (dbPayouts == null || !dbPayouts.Any()) return;

        foreach (var p in dbPayouts)
        {
            p.PayoutStatus = "failed";
        }
        await _financeRepo.SaveChangesAsync();

        _logger.LogError("❌ {Count} InstructorPayouts → FAILED", dbPayouts.Count);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // payout.created → Stripe vừa tạo lệnh chuyển về ngân hàng (in_transit)
    // ★ Lưu stripe_payout_id để match khi payout.paid đến
    // ═══════════════════════════════════════════════════════════════════════
    private async Task OnPayoutCreatedAsync(Event e)
    {
        var stripePayout = e.Data.Object as Payout;
        if (stripePayout is null) return;

        _logger.LogInformation(
            "payout.created | StripePayoutId={Id} | Amount={Amt} | Account={Acc}",
            stripePayout.Id, stripePayout.Amount / 100.0, e.Account);

        // Tìm tất cả các khoản đang ở trạng thái 'transferred' thuộc account này
        var dbPayouts = await _financeRepo.GetTransferredPayoutsByAccountAsync(e.Account!);
        if (dbPayouts == null || !dbPayouts.Any())
        {
            _logger.LogWarning(
                "payout.created: No 'transferred' payouts found for Account={Acc}", e.Account);
            return;
        }

        // ★ Lưu stripe_payout_id, chuyển sang in_transit cho TẤT CẢ các khoản
        foreach (var p in dbPayouts)
        {
            p.StripePayoutId = stripePayout.Id;
            p.PayoutStatus = "in_transit";
        }
        await _financeRepo.SaveChangesAsync();

        // 🔥 Gửi SignalR báo cho Admin (báo refresh lại toàn bảng)
        await _hubContext.Clients.Group("AdminFinance").SendAsync("UpdatePayoutStatus", new {
            refresh = true
        });

        _logger.LogInformation(
            "🔵 {Count} InstructorPayouts → IN TRANSIT | StripePayoutId={PoId}",
            dbPayouts.Count, stripePayout.Id);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Endpoint TEST: Ép trạng thái sang PAID (Dùng để test giao diện nhảy chữ)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("test-force-paid")]
    public async Task<IActionResult> TestForcePaid(string payoutId)
    {
        var dbPayouts = await _financeRepo.GetPayoutsByStripePayoutIdAsync(payoutId);
        if (dbPayouts == null || !dbPayouts.Any()) return NotFound("Không tìm thấy Payout ID này trong DB.");

        foreach (var p in dbPayouts)
        {
            p.PayoutStatus = "paid";
            p.PaidToBankAt = DateTime.UtcNow;
            p.IsPaid = true;
        }
        await _financeRepo.SaveChangesAsync();

        // 🔥 Bắn SignalR để Web tự nhảy chữ
        await _hubContext.Clients.Group("AdminFinance").SendAsync("UpdatePayoutStatus", new {
            refresh = true
        });

        return Ok($"Đã ép {dbPayouts.Count} giao dịch sang trạng thái PAID!");
    }
}
