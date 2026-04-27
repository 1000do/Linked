using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using Stripe;

namespace CourseMarketplaceBE.Application.Services;

/// <summary>
/// Service xử lý toàn bộ luồng Giảng viên.
/// Tuân thủ DIP (SOLID): chỉ phụ thuộc IInstructorRepository + IFileUploadService.
/// AppDbContext bị đóng gói hoàn toàn ở Infrastructure layer.
/// </summary>
public class InstructorService : IInstructorService
{
    private readonly IInstructorRepository _repo;
    private readonly IFileUploadService    _uploadService;

    public InstructorService(IInstructorRepository repo, IFileUploadService uploadService)
    {
        _repo          = repo;
        _uploadService = uploadService;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 1. SUBMIT APPLICATION — Learner nộp đơn đăng ký Giảng viên
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<string> SubmitApplicationAsync(int userId, InstructorApplicationRequest request)
    {
        // Kiểm tra account tồn tại và đã xác thực email
        var account = await _repo.GetAccountByIdAsync(userId);
        if (account == null)
            throw new InvalidOperationException("Không tìm thấy tài khoản.");
        if (!account.IsVerified)
            throw new InvalidOperationException("EMAIL_NOT_VERIFIED");

        var existing = await _repo.GetByIdAsync(userId);

        if (existing != null)
        {
            // Chỉ cho phép nộp lại nếu đơn đang ở trạng thái 'Rejected'
            if (existing.ApprovalStatus != "Rejected")
                throw new InvalidOperationException("Bạn đã nộp đơn đăng ký Giảng viên trước đó.");

            // Upload file mới nếu có, không thì giữ file cũ
            if (request.DocumentFile != null && request.DocumentFile.Length > 0)
                existing.DocumentUrl = await _uploadService.UploadImageAsync(request.DocumentFile);

            existing.ProfessionalTitle   = request.ProfessionalTitle;
            existing.ExpertiseCategories = request.ExpertiseCategories;
            existing.LinkedinUrl         = request.LinkedinUrl;
            existing.ApprovalStatus      = "Pending";

            await _repo.SaveChangesAsync();
            return "Đơn đăng ký đã được gửi lại. Vui lòng chờ Admin xét duyệt.";
        }

        // Upload file CV/ID lên Cloudinary (nếu có)
        string? documentUrl = null;
        if (request.DocumentFile != null && request.DocumentFile.Length > 0)
            documentUrl = await _uploadService.UploadImageAsync(request.DocumentFile);

        var instructor = new Instructor
        {
            InstructorId         = userId,
            ProfessionalTitle    = request.ProfessionalTitle,
            ExpertiseCategories  = request.ExpertiseCategories,
            LinkedinUrl          = request.LinkedinUrl,
            DocumentUrl          = documentUrl,
            ApprovalStatus       = "Pending",
            StripeAccountId      = null,
            StripeOnboardingStatus = null,
            PayoutsEnabled       = false,
            ChargesEnabled       = false
        };

        await _repo.AddAsync(instructor);
        await _repo.SaveChangesAsync();

        return "Đơn đăng ký đã được gửi. Vui lòng chờ Admin duyệt.";
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 2. APPROVE APPLICATION — Admin duyệt đơn
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<bool> ApproveApplicationAsync(int instructorId, string status)
    {
        var instructor = await _repo.GetByIdAsync(instructorId);
        if (instructor == null)
            throw new InvalidOperationException("Không tìm thấy đơn đăng ký.");

        if (status != "Approved" && status != "Rejected")
            throw new InvalidOperationException("Trạng thái không hợp lệ. Chỉ chấp nhận 'Approved' hoặc 'Rejected'.");

        instructor.ApprovalStatus = status;
        await _repo.SaveChangesAsync();

        return true;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 3. SETUP STRIPE PAYOUT — Instructor đã Approved → tạo Stripe account
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<StripeSetupResponse> SetupStripePayoutAsync(int userId)
    {
        var instructor = await _repo.GetByIdWithNavigationAsync(userId);

        if (instructor == null)
            throw new InvalidOperationException("Bạn chưa nộp đơn đăng ký Giảng viên.");

        if (instructor.ApprovalStatus != "Approved")
            throw new InvalidOperationException("Đơn đăng ký chưa được duyệt. Vui lòng chờ Admin xét duyệt.");

        // Nếu đã có Stripe account → tạo lại Link (tái sử dụng account cũ)
        if (!string.IsNullOrEmpty(instructor.StripeAccountId))
        {
            var reLinkService = new AccountLinkService();
            var reLink = await reLinkService.CreateAsync(new AccountLinkCreateOptions
            {
                Account    = instructor.StripeAccountId,
                RefreshUrl = "http://localhost:5208/Instructor/OnboardingRefresh",
                ReturnUrl  = $"http://localhost:5208/Instructor/StripeReturn?instructorId={userId}",
                Type       = "account_onboarding"
            });

            return new StripeSetupResponse
            {
                OnboardingUrl   = reLink.Url,
                StripeAccountId = instructor.StripeAccountId
            };
        }

        // Lấy email từ Account (qua User → Account navigation)
        var email = instructor.InstructorNavigation?.UserNavigation?.Email ?? "";

        // Tạo Stripe Express account mới
        var accountService  = new AccountService();
        var stripeAccount   = await accountService.CreateAsync(new AccountCreateOptions
        {
            Type    = "express",
            Country = "US",
            Email   = email,
            Capabilities = new AccountCapabilitiesOptions
            {
                CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
                Transfers    = new AccountCapabilitiesTransfersOptions    { Requested = true }
            },
            BusinessProfile = new AccountBusinessProfileOptions
            {
                ProductDescription = $"{instructor.ProfessionalTitle} | {instructor.ExpertiseCategories}"
            },
            Metadata = new Dictionary<string, string>
            {
                { "user_id",       userId.ToString() },
                { "instructor_id", userId.ToString() }
            }
        });

        // Lưu stripe_account_id vào DB
        instructor.StripeAccountId         = stripeAccount.Id;
        instructor.StripeOnboardingStatus  = "Pending";
        await _repo.SaveChangesAsync();

        // Tạo Onboarding URL
        var linkService = new AccountLinkService();
        var link = await linkService.CreateAsync(new AccountLinkCreateOptions
        {
            Account    = stripeAccount.Id,
            RefreshUrl = "http://localhost:5208/Instructor/OnboardingRefresh",
            ReturnUrl  = $"http://localhost:5208/Instructor/StripeReturn?instructorId={userId}",
            Type       = "account_onboarding"
        });

        return new StripeSetupResponse
        {
            OnboardingUrl   = link.Url,
            StripeAccountId = stripeAccount.Id
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 4. VERIFY STRIPE ONBOARDING — Kiểm tra PayoutsEnabled & ChargesEnabled
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<string> VerifyStripeOnboardingAsync(int instructorId)
    {
        var instructor = await _repo.GetByIdAsync(instructorId);
        if (instructor == null)
            throw new InvalidOperationException("Không tìm thấy Instructor.");

        if (string.IsNullOrEmpty(instructor.StripeAccountId))
            throw new InvalidOperationException("Instructor chưa có tài khoản Stripe.");

        if (instructor.StripeOnboardingStatus == "Active") return "Active";

        var accountService  = new AccountService();
        var stripeAccount   = await accountService.GetAsync(instructor.StripeAccountId);

        // Chỉ cần điền xong form (DetailsSubmitted = true) → coi như THÀNH CÔNG 100%
        if (stripeAccount.DetailsSubmitted)
        {
            instructor.PayoutsEnabled         = true;
            instructor.ChargesEnabled         = true;
            instructor.StripeOnboardingStatus = "Active";
            await _repo.SaveChangesAsync();
            return "Active";
        }

        // Nếu chưa nộp form
        instructor.PayoutsEnabled         = stripeAccount.PayoutsEnabled;
        instructor.ChargesEnabled         = stripeAccount.ChargesEnabled;
        instructor.StripeOnboardingStatus = "Pending";
        await _repo.SaveChangesAsync();

        return "Pending";
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 5. GET ALL APPLICATIONS — Admin xem danh sách đơn đăng ký
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<List<InstructorDashboardDto>> GetAllApplicationsAsync()
        => await _repo.GetAllApplicationsDtoAsync();

    // ═══════════════════════════════════════════════════════════════════════
    // 6. GET INSTRUCTOR DASHBOARD — Thông tin cho Instructor Dashboard FE
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<InstructorDashboardDto?> GetInstructorDashboardAsync(int userId)
    {
        var dto = await _repo.GetDashboardDtoAsync(userId);
        if (dto == null) return null;

        // Fetch dynamic stats từ view
        var stats = await _repo.GetStatsAsync(userId);
        if (stats != null)
        {
            dto.TotalStudents  = stats.TotalStudentsCount;
            dto.AverageRating  = (decimal)stats.InstructorRating;
            dto.TotalRevenue   = stats.TotalRevenue;
        }

        dto.ActiveCoursesCount = await _repo.CountActiveCoursesAsync(userId);

        return dto;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 7. GET REJECTED APPLICATION INFO
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<InstructorDashboardDto?> GetRejectedApplicationInfoAsync(int userId)
        => await _repo.GetRejectedApplicationDtoAsync(userId);
}
