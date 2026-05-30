using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stripe;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace CourseMarketplaceBE.Presentation.Controllers;

/// <summary>
/// Stripe Connect Webhook Controller.
///
/// Nhận và xử lý các sự kiện từ Stripe Dashboard → Developers → Webhooks.
///
/// SOLID — SRP: Controller chỉ parse + route event. Không chứa business logic.
/// SOLID — DIP: Phụ thuộc IStripeWebhookService qua constructor injection.
/// </summary>
[Route("api/webhooks")]
[ApiController]
public class StripeWebhookController : ControllerBase
{
    private readonly IStripeWebhookService _webhookService;
    private readonly ILogger<StripeWebhookController> _logger;
    private readonly string _connectWebhookSecret;

    public StripeWebhookController(
        IStripeWebhookService webhookService,
        ILogger<StripeWebhookController> logger,
        IConfiguration configuration)
    {
        _webhookService = webhookService;
        _logger = logger;
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
            case "charge.refunded":
                await OnChargeRefundedAsync(e);
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

        await _webhookService.OnPayoutPaidAsync(stripePayout.Id, e.Account!);
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

        await _webhookService.OnPayoutFailedAsync(stripePayout.Id, stripePayout.FailureMessage, e.Account!);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // payout.created → Stripe vừa tạo lệnh chuyển về ngân hàng (in_transit)
    // ═══════════════════════════════════════════════════════════════════════
    private async Task OnPayoutCreatedAsync(Event e)
    {
        var stripePayout = e.Data.Object as Payout;
        if (stripePayout is null) return;

        _logger.LogInformation(
            "payout.created | StripePayoutId={Id} | Amount={Amt} | Account={Acc}",
            stripePayout.Id, stripePayout.Amount / 100.0, e.Account);

        await _webhookService.OnPayoutCreatedAsync(stripePayout.Id, e.Account!);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // charge.refunded → Stripe xác nhận refund đã hoàn tất
    // ═══════════════════════════════════════════════════════════════════════
    private async Task OnChargeRefundedAsync(Event e)
    {
        var charge = e.Data.Object as Charge;
        if (charge is null) return;

        _logger.LogInformation(
            "charge.refunded | ChargeId={Id} | PaymentIntent={PI} | Amount={Amt}",
            charge.Id, charge.PaymentIntentId, charge.AmountRefunded / 100.0);

        await _webhookService.OnChargeRefundedAsync(charge.PaymentIntentId, charge.AmountRefunded / 100.0);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Endpoint TEST: Ép trạng thái sang PAID (Dùng để test giao diện nhảy chữ)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("test-force-paid")]
    public async Task<IActionResult> TestForcePaid(string payoutId)
    {
        try
        {
            await _webhookService.ForcePayoutPaidAsync(payoutId);
            return Ok("Successfully forced transactions to PAID status!");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
