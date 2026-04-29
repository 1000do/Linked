using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

/// <summary>
/// SOLID — ISP: Interface chỉ chứa nghiệp vụ Admin Finance.
/// SOLID — DIP: Controller phụ thuộc interface, không biết implementation.
/// </summary>
public interface IAdminFinanceService
{
    /// <summary>
    /// UC-120: Cập nhật tỷ lệ chia sẻ doanh thu.
    /// Giá trị từ 1-100 (VD: 70 = 70% cho giảng viên, 30% cho sàn).
    /// </summary>
    Task SetTransferRateAsync(decimal rate);

    /// <summary>
    /// UC-112: Lấy tổng quan tài chính hệ thống.
    /// Bao gồm: Gross Revenue, Net Profit, Pending Escrow, Transfer Rate.
    /// </summary>
    Task<FinancialSummaryResponse> GetFinancialSummaryAsync();

    /// <summary>
    /// Lấy danh sách chi tiết chia tiền giảng viên.
    /// JOIN: instructor_payouts → transactions → courses → users.
    /// </summary>
    Task<List<PayoutDetailResponse>> GetInstructorPayoutsAsync();

    /// <summary>
    /// Lấy tỷ lệ chia sẻ hiện tại từ system_configs.
    /// </summary>
    Task<decimal> GetCurrentTransferRateAsync();
}
