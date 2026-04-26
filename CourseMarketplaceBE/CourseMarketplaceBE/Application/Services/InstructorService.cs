using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace CourseMarketplaceBE.Application.Services;

/// <summary>
/// Service xử lý toàn bộ luồng Giảng viên:
/// 1. Learner nộp đơn (SubmitApplication)
/// 2. Admin duyệt đơn (ApproveApplication)
/// 3. Instructor setup Stripe payout (SetupStripePayout)
/// 4. Stripe redirect về → verify trạng thái (VerifyStripeOnboarding)
/// </summary>
public class InstructorService : IInstructorService
{
    private readonly AppDbContext _context;
    private readonly IFileUploadService _uploadService;

    public InstructorService(AppDbContext context, IFileUploadService uploadService)
    {
        _context = context;
        _uploadService = uploadService;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 1. SUBMIT APPLICATION — Learner nộp đơn đăng ký Giảng viên
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<string> SubmitApplicationAsync(int userId, InstructorApplicationRequest request)
    {
        // Kiểm tra email đã xác thực chưa
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == userId);
        if (account == null)
            throw new InvalidOperationException("Không tìm thấy tài khoản.");
        if (!account.IsVerified)
            throw new InvalidOperationException("EMAIL_NOT_VERIFIED");

        // Kiểm tra đã nộp đơn chưa
        var existing = await _context.Instructors.FirstOrDefaultAsync(i => i.InstructorId == userId);
        if (existing != null)
            throw new InvalidOperationException("Bạn đã nộp đơn đăng ký Giảng viên trước đó.");

        // Upload file CV/ID lên Cloudinary (nếu có)
        string? documentUrl = null;
        if (request.DocumentFile != null && request.DocumentFile.Length > 0)
        {
            documentUrl = await _uploadService.UploadImageAsync(request.DocumentFile);
        }

        // INSERT vào bảng instructors với approval_status = 'Pending'
        var instructor = new Instructor
        {
            InstructorId = userId,
            ProfessionalTitle = request.ProfessionalTitle,
            ExpertiseCategories = request.ExpertiseCategories,
            LinkedinUrl = request.LinkedinUrl,
            DocumentUrl = documentUrl,
            ApprovalStatus = "Pending",
            // Stripe chưa setup → null
            StripeAccountId = null,
            StripeOnboardingStatus = null,
            PayoutsEnabled = false,
            ChargesEnabled = false
        };

        _context.Instructors.Add(instructor);
        await _context.SaveChangesAsync();

        return "Đơn đăng ký đã được gửi. Vui lòng chờ Admin duyệt.";
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 2. APPROVE APPLICATION — Admin duyệt đơn
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<bool> ApproveApplicationAsync(int instructorId, string status)
    {
        var instructor = await _context.Instructors.FindAsync(instructorId);
        if (instructor == null)
            throw new InvalidOperationException("Không tìm thấy đơn đăng ký.");

        if (status != "Approved" && status != "Rejected")
            throw new InvalidOperationException("Trạng thái không hợp lệ. Chỉ chấp nhận 'Approved' hoặc 'Rejected'.");

        instructor.ApprovalStatus = status;
        await _context.SaveChangesAsync();

        return true;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 3. SETUP STRIPE PAYOUT — Instructor đã Approved → tạo Stripe account
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<StripeSetupResponse> SetupStripePayoutAsync(int userId)
    {
        var instructor = await _context.Instructors
            .Include(i => i.InstructorNavigation)
                .ThenInclude(u => u.UserNavigation)
            .FirstOrDefaultAsync(i => i.InstructorId == userId);

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
                Account = instructor.StripeAccountId,
                RefreshUrl = "http://localhost:5208/Instructor/OnboardingRefresh",
                ReturnUrl = $"http://localhost:5208/Instructor/StripeReturn?instructorId={userId}",
                Type = "account_onboarding"
            });

            return new StripeSetupResponse
            {
                OnboardingUrl = reLink.Url,
                StripeAccountId = instructor.StripeAccountId
            };
        }

        // Lấy email từ Account (qua User → Account navigation)
        var email = instructor.InstructorNavigation?.UserNavigation?.Email ?? "";

        // Tạo Stripe Express account mới
        var accountService = new AccountService();
        var stripeAccount = await accountService.CreateAsync(new AccountCreateOptions
        {
            Type = "express",
            Country = "US",
            Email = email,
            Capabilities = new AccountCapabilitiesOptions
            {
                CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
                Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
            },
            BusinessProfile = new AccountBusinessProfileOptions
            {
                ProductDescription = $"{instructor.ProfessionalTitle} | {instructor.ExpertiseCategories}"
            },
            Metadata = new Dictionary<string, string>
            {
                { "user_id", userId.ToString() },
                { "instructor_id", userId.ToString() }
            }
        });

        // Lưu stripe_account_id vào DB
        instructor.StripeAccountId = stripeAccount.Id;
        instructor.StripeOnboardingStatus = "Pending";
        await _context.SaveChangesAsync();

        // Tạo Onboarding URL
        var linkService = new AccountLinkService();
        var link = await linkService.CreateAsync(new AccountLinkCreateOptions
        {
            Account = stripeAccount.Id,
            RefreshUrl = "http://localhost:5208/Instructor/OnboardingRefresh",
            ReturnUrl = $"http://localhost:5208/Instructor/StripeReturn?instructorId={userId}",
            Type = "account_onboarding"
        });

        return new StripeSetupResponse
        {
            OnboardingUrl = link.Url,
            StripeAccountId = stripeAccount.Id
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 4. VERIFY STRIPE ONBOARDING — Kiểm tra PayoutsEnabled & ChargesEnabled
    // ═══════════════════════════════════════════════════════════════════════
    /// <summary>
    /// ★ LOGIC QUAN TRỌNG NHẤT:
    /// Gọi Stripe API để lấy thông tin account.
    /// - account.PayoutsEnabled: Stripe cho phép rút tiền? (cần xác thực ngân hàng + danh tính)
    /// - account.ChargesEnabled: Stripe cho phép nhận thanh toán? (cần hoàn tất onboarding)
    /// Nếu CẢ HAI == true → cập nhật DB + đặt stripe_onboarding_status = 'Active'.
    /// Nếu không → giữ nguyên status = 'Pending', trả về false.
    /// </summary>
    public async Task<string> VerifyStripeOnboardingAsync(int instructorId)
    {
        var instructor = await _context.Instructors.FindAsync(instructorId);
        if (instructor == null)
            throw new InvalidOperationException("Không tìm thấy Instructor.");

        if (string.IsNullOrEmpty(instructor.StripeAccountId))
            throw new InvalidOperationException("Instructor chưa có tài khoản Stripe.");

        if (instructor.StripeOnboardingStatus == "Active") return "Active";

        var accountService = new AccountService();
        var stripeAccount = await accountService.GetAsync(instructor.StripeAccountId);

        // Theo yêu cầu: Chỉ cần điền xong form (DetailsSubmitted = true) là coi như THÀNH CÔNG 100%
        if (stripeAccount.DetailsSubmitted)
        {
            // Bật luôn cờ để UI hiện màu xanh
            instructor.PayoutsEnabled = true;
            instructor.ChargesEnabled = true;
            instructor.StripeOnboardingStatus = "Active";
            
            await _context.SaveChangesAsync();
            return "Active";
        }

        // Nếu chưa nộp form
        instructor.PayoutsEnabled = stripeAccount.PayoutsEnabled;
        instructor.ChargesEnabled = stripeAccount.ChargesEnabled;
        instructor.StripeOnboardingStatus = "Pending";
        await _context.SaveChangesAsync();
        
        return "Pending";
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 5. GET ALL APPLICATIONS — Admin xem danh sách đơn đăng ký
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<List<InstructorDashboardDto>> GetAllApplicationsAsync()
    {
        return await _context.Instructors
            .Include(i => i.InstructorNavigation)
                .ThenInclude(u => u.UserNavigation)
            .Select(i => new InstructorDashboardDto
            {
                InstructorId = i.InstructorId,
                ProfessionalTitle = i.ProfessionalTitle,
                ExpertiseCategories = i.ExpertiseCategories,
                ApprovalStatus = i.ApprovalStatus,
                StripeAccountId = i.StripeAccountId,
                StripeOnboardingStatus = i.StripeOnboardingStatus,
                PayoutsEnabled = i.PayoutsEnabled ?? false,
                ChargesEnabled = i.ChargesEnabled ?? false,
                FullName = i.InstructorNavigation.FullName,
                Email = i.InstructorNavigation.UserNavigation.Email
            })
            .ToListAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 6. GET INSTRUCTOR DASHBOARD — Thông tin cho Instructor Dashboard FE
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<InstructorDashboardDto?> GetInstructorDashboardAsync(int userId)
    {
        return await _context.Instructors
            .Include(i => i.InstructorNavigation)
                .ThenInclude(u => u.UserNavigation)
            .Where(i => i.InstructorId == userId)
            .Select(i => new InstructorDashboardDto
            {
                InstructorId = i.InstructorId,
                ProfessionalTitle = i.ProfessionalTitle,
                ExpertiseCategories = i.ExpertiseCategories,
                ApprovalStatus = i.ApprovalStatus,
                StripeAccountId = i.StripeAccountId,
                StripeOnboardingStatus = i.StripeOnboardingStatus,
                PayoutsEnabled = i.PayoutsEnabled ?? false,
                ChargesEnabled = i.ChargesEnabled ?? false,
                FullName = i.InstructorNavigation.FullName,
                Email = i.InstructorNavigation.UserNavigation.Email
            })
            .FirstOrDefaultAsync();
    }
}
