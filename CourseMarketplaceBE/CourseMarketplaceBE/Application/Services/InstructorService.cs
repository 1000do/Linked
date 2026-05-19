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
    private readonly IAdminFinanceRepository _financeRepo;

    public InstructorService(IInstructorRepository repo, IFileUploadService uploadService, IAdminFinanceRepository financeRepo)
    {
        _repo          = repo;
        _uploadService = uploadService;
        _financeRepo   = financeRepo;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 1. SUBMIT APPLICATION — Learner nộp đơn đăng ký Giảng viên
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<string> SubmitApplicationAsync(int userId, InstructorApplicationRequest request)
    {
        // Kiểm tra account tồn tại và đã xác thực email
        var account = await _repo.GetAccountByIdAsync(userId);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
        if (!account.IsVerified)
            throw new InvalidOperationException("EMAIL_NOT_VERIFIED");

        var existing = await _repo.GetByIdAsync(userId);

        if (existing != null)
        {
            // Chỉ cho phép nộp lại nếu đơn đang ở trạng thái 'Rejected'
            if (existing.ApprovalStatus != "Rejected")
                throw new InvalidOperationException("You have already submitted an instructor application.");

            // Upload file mới nếu có, không thì giữ file cũ
            if (request.DocumentFile != null && request.DocumentFile.Length > 0)
                existing.DocumentUrl = await _uploadService.UploadImageAsync(request.DocumentFile);
            else if (string.IsNullOrEmpty(existing.DocumentUrl))
                throw new InvalidOperationException("Please upload your CV / Certificates / ID card.");

            existing.ProfessionalTitle   = request.ProfessionalTitle;
            existing.ExpertiseCategories = request.ExpertiseCategories;
            existing.LinkedinUrl         = request.LinkedinUrl;
            existing.YoutubeUrl          = request.YoutubeUrl;
            existing.FacebookUrl         = request.FacebookUrl;
            existing.StripeCountry       = request.StripeCountry.ToUpper();
            existing.ApprovalStatus      = "Pending";

            await _repo.SaveChangesAsync();
            return "Your application has been resubmitted. Please wait for admin approval.";
        }

        // ★ CV/Document bắt buộc cho đơn mới
        if (request.DocumentFile == null || request.DocumentFile.Length == 0)
            throw new InvalidOperationException("Please upload your CV / Certificates / ID card.");

        string? documentUrl = await _uploadService.UploadImageAsync(request.DocumentFile);

        var instructor = new Instructor
        {
            InstructorId         = userId,
            ProfessionalTitle    = request.ProfessionalTitle,
            ExpertiseCategories  = request.ExpertiseCategories,
            LinkedinUrl          = request.LinkedinUrl,
            YoutubeUrl           = request.YoutubeUrl,
            FacebookUrl          = request.FacebookUrl,
            DocumentUrl          = documentUrl,
            ApprovalStatus       = "Pending",
            StripeCountry        = request.StripeCountry.ToUpper(),
            StripeAccountId      = null,
            StripeOnboardingStatus = null,
            PayoutsEnabled       = false,
            ChargesEnabled       = false
        };

        await _repo.AddAsync(instructor);
        await _repo.SaveChangesAsync();

        return "Your application has been submitted. Please wait for admin approval.";
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 2. APPROVE APPLICATION — Admin duyệt đơn
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<bool> ApproveApplicationAsync(int instructorId, string status)
    {
        var instructor = await _repo.GetByIdAsync(instructorId);
        if (instructor == null)
            throw new InvalidOperationException("Application not found.");

        if (status != "Approved" && status != "Rejected")
            throw new InvalidOperationException("Invalid status. Only 'Approved' or 'Rejected' are allowed.");

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
            throw new InvalidOperationException("You have not submitted an instructor application.");

        if (instructor.ApprovalStatus != "Approved")
            throw new InvalidOperationException("Application is not approved yet. Please wait for admin approval.");

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

        // Lấy quốc gia mà giảng viên đã chọn khi đăng ký (từ DB)
        var country = !string.IsNullOrEmpty(instructor.StripeCountry)
            ? instructor.StripeCountry
            : "SG"; // Fallback mặc định nếu chưa chọn

        // Tạo Stripe Express account mới
        var accountService  = new AccountService();
        var stripeAccount   = await accountService.CreateAsync(new AccountCreateOptions
        {
            Type    = "express",
            // ★ Country được lấy từ bảng instructors.stripe_country (giảng viên tự chọn)
            Country = country,
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
            throw new InvalidOperationException("Instructor not found.");

        if (string.IsNullOrEmpty(instructor.StripeAccountId))
            throw new InvalidOperationException("Instructor does not have a Stripe account.");

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

    // ═══════════════════════════════════════════════════════════════════════
    // 8. RESET STRIPE ACCOUNT — Xóa Connected Account cũ, cho phép tạo lại
    //    Dùng khi: Platform ở AU, Connected Account lỗi ở US → không Transfer được
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<string> ResetStripeAccountAsync(int instructorId)
    {
        var instructor = await _repo.GetByIdAsync(instructorId);
        if (instructor == null)
            throw new InvalidOperationException("Instructor not found.");

        if (string.IsNullOrEmpty(instructor.StripeAccountId))
            throw new InvalidOperationException("Instructor does not have a Stripe account.");

        // ★ Xóa Stripe Connected Account trên Stripe
        try
        {
            var accountService = new AccountService();
            await accountService.DeleteAsync(instructor.StripeAccountId);
            Console.WriteLine($"[STRIPE-RESET] ✅ Deleted Stripe account {instructor.StripeAccountId} for instructor {instructorId}");
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"[STRIPE-RESET] ⚠️ Stripe delete failed (may already be deleted): {ex.Message}");
        }

        // ★ Reset DB fields → Instructor phải onboard lại
        instructor.StripeAccountId = null;
        instructor.StripeOnboardingStatus = null;
        instructor.PayoutsEnabled = false;
        instructor.ChargesEnabled = false;
        await _repo.SaveChangesAsync();

        return $"Stripe account for instructor {instructorId} has been reset. The instructor needs to set up Stripe again.";
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 8. SET STRIPE COUNTRY — Giảng viên chọn quốc gia Stripe Connect
    // ═══════════════════════════════════════════════════════════════════════
    public async Task SetStripeCountryAsync(int userId, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
            throw new InvalidOperationException("Invalid country code (must be exactly 2 ISO characters, e.g. SG, AU, US).");

        var instructor = await _repo.GetByIdAsync(userId);
        if (instructor == null)
            throw new InvalidOperationException("You have not submitted an instructor application.");

        // Nếu đã có Stripe account rồi → không cho đổi country nữa
        if (!string.IsNullOrEmpty(instructor.StripeAccountId))
            throw new InvalidOperationException("Cannot change country once a Stripe account exists. Please reset Stripe first.");

        instructor.StripeCountry = countryCode.ToUpper();
        await _repo.SaveChangesAsync();
    }

    public async Task<InstructorPayoutPagedDto> GetPayoutsAsync(int userId, int page = 1, int pageSize = 10, string? keyword = null, string? sortBy = "date_desc", string? status = null)
    {
        return await _repo.GetPayoutsAsync(userId, page, pageSize, keyword, sortBy, status);
    }

    public async Task SyncPayoutsWithStripeAsync(int userId)
    {
        var instructor = await _repo.GetByIdAsync(userId);
        if (instructor == null || string.IsNullOrEmpty(instructor.StripeAccountId))
            return;

        // 1. Lấy danh sách Payouts gần đây từ Stripe cho Connected Account này
        var payoutService = new PayoutService();
        var stripePayouts = await payoutService.ListAsync(new PayoutListOptions
        {
            Limit = 20,
            Expand = new List<string> { "data.destination" }
        }, new RequestOptions
        {
            StripeAccount = instructor.StripeAccountId
        });

        if (!stripePayouts.Any()) return;

        // 2. Duyệt qua từng Payout từ Stripe để đồng bộ
        foreach (var sp in stripePayouts)
        {
            // Kiểm tra xem mã Payout này đã có trong DB chưa
            var dbPayouts = await _financeRepo.GetPayoutsByStripePayoutIdAsync(sp.Id);

            // Nếu CHƯA có mã Payout trong DB -> Ta cần tìm xem Payout này chứa những Transfer nào của ta
            if (!dbPayouts.Any())
            {
                // Truy vấn Balance Transactions của Payout này để tìm các TransferId (tr_xxx)
                var btService = new BalanceTransactionService();
                var balanceTransactions = await btService.ListAsync(new BalanceTransactionListOptions
                {
                    Payout = sp.Id,
                    Limit = 100
                }, new RequestOptions
                {
                    StripeAccount = instructor.StripeAccountId
                });

                foreach (var bt in balanceTransactions)
                {
                    // Chấp nhận cả 'payment' (py_xxx) và 'transfer' (tr_xxx)
                    if ((bt.Type != "transfer" && bt.Type != "payment") || string.IsNullOrEmpty(bt.SourceId)) continue;

                    var localPayout = await _financeRepo.GetPayoutByTransferIdAsync(bt.SourceId);
                    if (localPayout != null)
                    {
                        // Khớp lệnh thành công -> Gắn mã Payout và cập nhật trạng thái
                        localPayout.StripePayoutId = sp.Id;
                        UpdatePayoutStatusFromStripe(localPayout, sp);
                    }
                }
            }
            else
            {
                // Nếu ĐÃ có mã Payout -> Cập nhật trạng thái mới nhất (VD: từ in_transit sang paid)
                foreach (var dbp in dbPayouts)
                {
                    UpdatePayoutStatusFromStripe(dbp, sp);
                }
            }
        }

        await _repo.SaveChangesAsync();
    }

    private void UpdatePayoutStatusFromStripe(InstructorPayout dbp, Payout sp)
    {
        if (sp.Status == "paid")
        {
            dbp.PayoutStatus = "paid";
            dbp.IsPaid = true;
            dbp.PaidToBankAt = sp.ArrivalDate;
        }
        else if (sp.Status == "in_transit" || sp.Status == "pending")
        {
            dbp.PayoutStatus = "in_transit";
        }
        else if (sp.Status == "failed" || sp.Status == "canceled")
        {
            dbp.PayoutStatus = "failed";
            dbp.IsPaid = false;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // PUBLIC PROFILE — Trang thông tin công khai của giảng viên
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<InstructorPublicProfileDto?> GetPublicProfileAsync(int instructorId)
    {
        var instructor = await _repo.GetByIdWithNavigationAsync(instructorId);
        if (instructor == null || instructor.ApprovalStatus != "Approved")
            return null;

        var user = instructor.InstructorNavigation;
        var stats = await _repo.GetStatsAsync(instructorId);
        var activeCourses = await _repo.CountActiveCoursesAsync(instructorId);

        // Lấy danh sách khóa học published
        var courses = instructor.Courses
            .Where(c => c.CourseStatus == "published")
            .Select(c => new InstructorCourseDto
            {
                CourseId = c.CourseId,
                Title = c.Title,
                ThumbnailUrl = c.CourseThumbnailUrl,
                Price = c.Price,
                Rating = 0, // Sẽ lấy từ view nếu cần
                TotalStudents = c.Enrollments?.Count ?? 0
            }).ToList();

        return new InstructorPublicProfileDto
        {
            InstructorId = instructorId,
            FullName = user?.FullName,
            AvatarUrl = user?.UserNavigation?.AvatarUrl,
            ProfessionalTitle = instructor.ProfessionalTitle,
            ExpertiseCategories = instructor.ExpertiseCategories,
            Bio = user?.Bio,
            LinkedinUrl = instructor.LinkedinUrl,
            YoutubeUrl = instructor.YoutubeUrl,
            FacebookUrl = instructor.FacebookUrl,
            TotalStudents = stats?.TotalStudentsCount ?? 0,
            TotalCourses = activeCourses,
            AverageRating = (decimal)(stats?.InstructorRating ?? 0),
            TotalReviews = 0, // Sẽ bổ sung sau nếu cần
            Courses = courses
        };
    }
}
