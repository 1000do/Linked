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
    /// Background Service tự động quét các coupon đã hết hạn (EndDate < Now) nhưng vẫn đang có IsActive = true.
    /// - Đổi IsActive = false để vô hiệu hóa coupon.
    /// - Gửi thông báo đến tất cả giảng viên có khóa học đang sử dụng coupon này.
    /// Chạy định kỳ 1 ngày 1 lần.
    /// </summary>
    public class CouponExpirationTask : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CouponExpirationTask> _logger;

        public CouponExpirationTask(IServiceProvider serviceProvider, ILogger<CouponExpirationTask> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Coupon Expiration Task started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiredCouponsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Coupon Expiration Task.");
                }

                // Chạy mỗi 30 giây để test (Sau khi test xong nhớ đổi lại thành FromDays(1))
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task ProcessExpiredCouponsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var couponRepo = scope.ServiceProvider.GetRequiredService<ICouponRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var now = DateTime.UtcNow;
            
            // Lấy danh sách các coupon đã hết hạn nhưng vẫn đang active, kèm theo khóa học liên quan
            var expiredCoupons = await couponRepo.GetExpiredActiveCouponsWithCoursesAsync(now);

            if (!expiredCoupons.Any())
            {
                _logger.LogInformation("No expired active coupons found.");
                return;
            }

            _logger.LogInformation($"Found {expiredCoupons.Count} expired active coupons to process.");

            foreach (var coupon in expiredCoupons)
            {
                // Tắt trạng thái hoạt động của coupon
                coupon.IsActive = false;
                couponRepo.Update(coupon);

                // Lấy danh sách giảng viên duy nhất (InstructorId) từ các khóa học đang dùng coupon này
                // Các khóa học có InstructorId hợp lệ
                var instructorIds = coupon.Courses
                    .Where(c => c.InstructorId.HasValue)
                    .Select(c => c.InstructorId!.Value)
                    .Distinct()
                    .ToList();

                foreach (var instructorId in instructorIds)
                {
                    var title = "Coupon Expired";
                    var content = $"The discount code {coupon.CouponCode} attached to your course has expired. Please review your course pricing.";
                    var linkAction = "/instructor/courses"; // Điều hướng giảng viên về trang khóa học của họ

                    await notificationService.SendNotificationAsync(instructorId, title, content, linkAction);
                }

                _logger.LogInformation($"Processed expired coupon {coupon.CouponCode}. Notified {instructorIds.Count} instructors.");
            }

            // Lưu thay đổi vào DB
            await couponRepo.SaveChangesAsync();
            _logger.LogInformation("Coupon expiration processing completed and saved to DB.");
        }
    }
}
