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
}
