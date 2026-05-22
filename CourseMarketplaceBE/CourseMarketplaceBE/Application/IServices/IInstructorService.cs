using CourseMarketplaceBE.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace CourseMarketplaceBE.Application.IServices;

/// <summary>
/// Interface xử lý nghiệp vụ Giảng viên:
/// 1. Learner nộp đơn → 2. Admin duyệt → 3. Instructor setup Stripe → 4. Verify Stripe Onboarding.
/// </summary>
public interface IInstructorService
{
    /// <summary>
    /// Learner nộp đơn: Upload file Cloudinary + INSERT vào instructors (approval_status = Pending).
    /// </summary>
    Task<string> SubmitApplicationAsync(int userId, InstructorApplicationRequest request);

    /// <summary>
    /// Admin duyệt đơn: UPDATE approval_status = 'Approved' hoặc 'Rejected'.
    /// </summary>
    Task<bool> ApproveApplicationAsync(int instructorId, string status);

    /// <summary>
    /// Instructor (đã Approved) setup Stripe: Tạo Express account + trả URL Onboarding.
    /// </summary>
    Task<StripeSetupResponse> SetupStripePayoutAsync(int userId);

    /// <summary>
    /// Stripe redirect về: Verify PayoutsEnabled & ChargesEnabled từ Stripe API.
    /// Trả về "Active", "Processing", hoặc "Pending"
    /// </summary>
    Task<string> VerifyStripeOnboardingAsync(int instructorId);

    /// <summary>
    /// Lấy danh sách tất cả đơn đăng ký (Admin dùng).
    /// </summary>
    Task<List<InstructorDashboardDto>> GetAllApplicationsAsync();

    /// <summary>
    /// Lấy thông tin đơn cũ của user (chỉ khi status = Rejected), dùng để điền sẵn vào form nộp lại.
    /// </summary>
    Task<InstructorDashboardDto?> GetRejectedApplicationInfoAsync(int userId);

    /// <summary>
    /// Lấy thông tin instructor dashboard cho user.
    /// </summary>
    Task<InstructorDashboardDto?> GetInstructorDashboardAsync(int userId);

    /// <summary>
    /// ★ Reset Stripe Connected Account (xóa account cũ, cho phép tạo lại).
    /// Dùng khi account cũ bị lỗi region mismatch.
    /// </summary>
    Task<string> ResetStripeAccountAsync(int instructorId);

    /// <summary>
    /// Tạo đường dẫn đăng nhập bảo mật (Stripe Login Link) cho giảng viên tự quản lý Express Dashboard.
    /// </summary>
    Task<string> GetStripeLoginLinkAsync(int userId);

    /// <summary>
    /// Giảng viên chọn quốc gia cho tài khoản Stripe Connect.
    /// Lưu vào cột stripe_country trong bảng instructors.
    /// </summary>
    Task SetStripeCountryAsync(int userId, string countryCode);

    /// <summary>
    /// Lấy danh sách lịch sử thanh toán của giảng viên (có phân trang, tìm kiếm, lọc).
    /// </summary>
    Task<InstructorPayoutPagedDto> GetPayoutsAsync(int userId, int page = 1, int pageSize = 10, string? keyword = null, string? sortBy = "date_desc", string? status = null, int? year = null, int? month = null);

    /// <summary>
    /// Chủ động gọi Stripe để đồng bộ trạng thái Payouts (giải pháp cho việc lỡ mất Webhook).
    /// </summary>
    Task SyncPayoutsWithStripeAsync(int userId);

    /// <summary>
    /// Lấy thông tin Public Profile của giảng viên (cho trang Instructor Profile công khai).
    /// </summary>
    Task<InstructorPublicProfileDto?> GetPublicProfileAsync(int instructorId);
}
