using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CourseMarketplaceBE.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Background Service tự động quét lịch thanh toán và thực hiện chuyển tiền Stripe.
    /// Chạy định kỳ mỗi 1 tiếng.
    /// </summary>
    public class PayoutScheduleTask : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PayoutScheduleTask> _logger;

        public PayoutScheduleTask(IServiceProvider serviceProvider, ILogger<PayoutScheduleTask> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Payout Background Task started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessScheduledPayoutsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Payout Background Task.");
                }

                // Chờ 1 tiếng (hoặc cấu hình tùy ý)
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task ProcessScheduledPayoutsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var financeService = scope.ServiceProvider.GetRequiredService<IAdminFinanceService>();
            var configRepo = scope.ServiceProvider.GetRequiredService<ISystemConfigRepository>();

            // 1. Lấy cấu hình ngày thanh toán (Mặc định là ngày 15 hàng tháng nếu DB trống)
            var payoutDaysStr = await configRepo.GetValueAsync("PayoutDays") ?? "15";
            if (string.IsNullOrWhiteSpace(payoutDaysStr)) payoutDaysStr = "15";

            var today = DateTime.Today.Day;
            var allowedDays = payoutDaysStr.Split(',')
                .Select(s => int.TryParse(s.Trim(), out var d) ? d : 0)
                .Where(d => d > 0)
                .ToList();

            // 2. Kiểm tra xem hôm nay có phải ngày thanh toán không
            if (allowedDays.Contains(today))
            {
                // Kiểm tra xem hôm nay đã chạy chưa để tránh double-run (Worker quét mỗi tiếng)
                var todayStr = DateTime.Today.ToString("yyyy-MM-dd");
                var lastPayoutDateStr = await configRepo.GetValueAsync("LastBulkPayoutDate") ?? "";
                
                if (lastPayoutDateStr == todayStr)
                {
                    _logger.LogInformation("Bulk payout already executed today ({Today}). Skipping to prevent double-run.", todayStr);
                    return;
                }

                _logger.LogInformation("Today is payout day ({Day}). Starting bulk payout...", today);
                
                var result = await financeService.BulkPayAllViaStripeAsync();
                
                // Lưu lại ngày chạy hôm nay để đánh dấu hoàn tất
                await configRepo.UpsertConfigAsync("LastBulkPayoutDate", todayStr, "The last date when bulk payout was successfully executed.");
                
                _logger.LogInformation("Bulk payout finished. Success: {Success}, Fail: {Fail}", 
                    result.SuccessCount, result.FailCount);
            }
        }
    }
}
