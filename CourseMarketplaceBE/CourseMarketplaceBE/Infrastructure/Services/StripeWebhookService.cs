using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Infrastructure.Services
{
    public class StripeWebhookService : IStripeWebhookService
    {
        private readonly IAdminFinanceRepository _financeRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IHubContext<FinanceHub> _hubContext;
        private readonly ILogger<StripeWebhookService> _logger;

        public StripeWebhookService(
            IAdminFinanceRepository financeRepo,
            ICourseRepository courseRepo,
            IHubContext<FinanceHub> hubContext,
            ILogger<StripeWebhookService> logger)
        {
            _financeRepo = financeRepo;
            _courseRepo = courseRepo;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task OnPayoutPaidAsync(string stripePayoutId, string stripeAccountId)
        {
            // Tìm các bản ghi DB đã được lưu stripe_payout_id lúc payout.created
            var dbPayouts = await _financeRepo.GetPayoutsByStripePayoutIdAsync(stripePayoutId);
            if (dbPayouts == null || !dbPayouts.Any())
            {
                _logger.LogWarning("payout.paid: No DB record found for StripePayoutId={Id}", stripePayoutId);
                return;
            }

            // ★ Tiền đã về ngân hàng — cập nhật trạng thái cuối cùng cho tất cả
            foreach (var p in dbPayouts)
            {
                p.PayoutStatus = "paid";
                p.PaidToBankAt = DateTime.UtcNow;
            }
            await _financeRepo.SaveChangesAsync();

            // 🔥 Gửi SignalR báo cho Admin và Instructor (refresh toàn bảng)
            var instructorId = dbPayouts.First().InstructorId;
            await _hubContext.Clients.Group("AdminFinance").SendAsync("UpdatePayoutStatus", new {
                refresh = true
            });
            if (instructorId.HasValue)
            {
                await _hubContext.Clients.Group($"InstructorFinance_{instructorId.Value}").SendAsync("UpdatePayoutStatus", new {
                    refresh = true
                });
            }

            _logger.LogInformation(
                "✅ {Count} InstructorPayouts → PAID TO BANK at {Time}",
                dbPayouts.Count, DateTime.UtcNow);
        }

        public async Task OnPayoutFailedAsync(string stripePayoutId, string? failureMessage, string stripeAccountId)
        {
            var dbPayouts = await _financeRepo.GetPayoutsByStripePayoutIdAsync(stripePayoutId);
            if (dbPayouts == null || !dbPayouts.Any()) return;

            foreach (var p in dbPayouts)
            {
                p.PayoutStatus = "failed";
            }
            await _financeRepo.SaveChangesAsync();

            _logger.LogError("❌ {Count} InstructorPayouts → FAILED. Reason: {Reason}", dbPayouts.Count, failureMessage);
        }

        public async Task OnPayoutCreatedAsync(string stripePayoutId, string stripeAccountId)
        {
            // Tìm tất cả các khoản đang ở trạng thái 'transferred' thuộc account này
            var dbPayouts = await _financeRepo.GetTransferredPayoutsByAccountAsync(stripeAccountId);
            if (dbPayouts == null || !dbPayouts.Any())
            {
                _logger.LogWarning(
                    "payout.created: No 'transferred' payouts found for Account={Acc}", stripeAccountId);
                return;
            }

            // ★ Lưu stripe_payout_id, chuyển sang in_transit cho TẤT CẢ các khoản
            foreach (var p in dbPayouts)
            {
                p.StripePayoutId = stripePayoutId;
                p.PayoutStatus = "in_transit";
            }
            await _financeRepo.SaveChangesAsync();

            // 🔥 Gửi SignalR báo cho Admin và Instructor
            var instructorId = dbPayouts.First().InstructorId;
            await _hubContext.Clients.Group("AdminFinance").SendAsync("UpdatePayoutStatus", new {
                refresh = true
            });
            if (instructorId.HasValue)
            {
                await _hubContext.Clients.Group($"InstructorFinance_{instructorId.Value}").SendAsync("UpdatePayoutStatus", new {
                    refresh = true
                });
            }

            _logger.LogInformation(
                "🔵 {Count} InstructorPayouts → IN TRANSIT | StripePayoutId={PoId}",
                dbPayouts.Count, stripePayoutId);
        }

        public async Task OnChargeRefundedAsync(string paymentIntentId, double amountRefunded)
        {
            // 1. Tìm transaction theo PaymentIntentId (pi_xxx)
            var txn = await _financeRepo.GetTransactionByPaymentIntentIdAsync(paymentIntentId);
            
            if (txn != null && txn.TransactionsStatus != "refunded")
            {
                _logger.LogInformation("🔄 Syncing refund from Stripe Dashboard for TxnId={Id}", txn.TransactionId);
                
                // 2. Cập nhật trạng thái
                txn.TransactionsStatus = "refunded";
                
                // 3. Cập nhật payout liên quan
                var payout = txn.InstructorPayouts.FirstOrDefault();
                if (payout != null)
                {
                    _financeRepo.RemoveInstructorPayout(payout);
                }

                // 4. Thu hồi Enrollment
                var buyerUserId = txn.AccountFromNavigation?.User?.UserId;
                var courseId = txn.OrderItem?.CourseId;
                if (buyerUserId.HasValue && courseId.HasValue)
                {
                    var enrollment = await _courseRepo.GetActiveEnrollmentAsync(buyerUserId.Value, courseId.Value);
                    if (enrollment != null)
                    {
                        enrollment.EnrollmentStatus = "revoked";
                        _logger.LogInformation("🚫 Enrollment revoked via Webhook sync");
                    }
                }

                await _financeRepo.SaveChangesAsync();
            }

            // 🔥 Gửi SignalR báo cho Admin để refresh UI
            await _hubContext.Clients.Group("AdminFinance").SendAsync("UpdatePayoutStatus", new
            {
                refresh = true
            });
        }

        public async Task ForcePayoutPaidAsync(string stripePayoutId)
        {
            var dbPayouts = await _financeRepo.GetPayoutsByStripePayoutIdAsync(stripePayoutId);
            if (dbPayouts == null || !dbPayouts.Any())
            {
                throw new InvalidOperationException("This Payout ID was not found in the DB.");
            }

            foreach (var p in dbPayouts)
            {
                p.PayoutStatus = "paid";
                p.PaidToBankAt = DateTime.UtcNow;
                p.IsPaid = true;
            }
            await _financeRepo.SaveChangesAsync();

            // 🔥 Bắn SignalR để Web tự nhảy chữ
            var instructorId = dbPayouts.First().InstructorId;
            await _hubContext.Clients.Group("AdminFinance").SendAsync("UpdatePayoutStatus", new {
                refresh = true
            });
            if (instructorId.HasValue)
            {
                await _hubContext.Clients.Group($"InstructorFinance_{instructorId.Value}").SendAsync("UpdatePayoutStatus", new {
                    refresh = true
                });
            }
        }
    }
}
