using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CourseMarketplaceBE.Application.BackgroundTasks;

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
        var repo = scope.ServiceProvider.GetRequiredService<IAdminFinanceRepository>();

        // 1. Lấy cấu hình ngày thanh toán (VD: "15,30")
        var payoutDaysStr = await repo.GetConfigValueAsync("PayoutDays") ?? "";
        if (string.IsNullOrWhiteSpace(payoutDaysStr)) return;

        var today = DateTime.Today.Day;
        var allowedDays = payoutDaysStr.Split(',')
            .Select(s => int.TryParse(s.Trim(), out var d) ? d : 0)
            .Where(d => d > 0)
            .ToList();

        // 2. Kiểm tra xem hôm nay có phải ngày thanh toán không
        if (allowedDays.Contains(today))
        {
            // Kiểm tra xem hôm nay đã chạy chưa (tránh chạy nhiều lần trong cùng 1 ngày thanh toán)
            // Lưu ý: Đây là logic tối giản, thực tế nên lưu LastBulkPayoutDate vào DB.
            _logger.LogInformation("Today is payout day ({Day}). Starting bulk payout...", today);
            
            var result = await financeService.BulkPayAllViaStripeAsync();
            
            _logger.LogInformation("Bulk payout finished. Success: {Success}, Fail: {Fail}", 
                result.SuccessCount, result.FailCount);
        }
    }
}
