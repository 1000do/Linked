using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services
{
    /// <summary>
    /// Service xử lý toàn bộ luồng Giảng viên.
    /// Tuân thủ DIP (SOLID): chỉ phụ thuộc IInstructorRepository + IFileUploadService.
    /// AppDbContext bị đóng gói hoàn toàn ở Infrastructure layer.
    /// </summary>
    public class InstructorService : IInstructorService
    {
        private readonly IInstructorRepository _repo;
        private readonly IFileUploadService _uploadService;
        private readonly IAdminFinanceRepository _financeRepo;
        private readonly IUserRepository _userRepo;
        private readonly IStripeConnectService _stripeConnect;
        private readonly ICourseRepository _courseRepo;

        public InstructorService(
            IInstructorRepository repo,
            IFileUploadService uploadService,
            IAdminFinanceRepository financeRepo,
            IUserRepository userRepo,
            IStripeConnectService stripeConnect,
            ICourseRepository courseRepo)
        {
            _repo = repo;
            _uploadService = uploadService;
            _financeRepo = financeRepo;
            _userRepo = userRepo;
            _stripeConnect = stripeConnect;
            _courseRepo = courseRepo;
        }

        // ═══════════════════════════════════════════════════════════════════════
        // 1. SUBMIT APPLICATION — Learner nộp đơn đăng ký Giảng viên
        // ═══════════════════════════════════════════════════════════════════════
        public async Task<string> SubmitApplicationAsync(int userId, InstructorApplicationRequest request)
        {
            // Kiểm tra account tồn tại và đã xác thực email
            var account = await _userRepo.GetAccountByIdAsync(userId);
            if (account == null)
                throw new InvalidOperationException("Account not found.");
            if (!account.IsVerified)
                throw new InvalidOperationException("EMAIL_NOT_VERIFIED");

            var existing = await _repo.GetByIdAsync(userId);

            // Thu thập các file tải lên từ cả DocumentFiles và DocumentFile (để tương thích ngược)
            var filesToUpload = new List<Microsoft.AspNetCore.Http.IFormFile>();
            if (request.DocumentFiles != null && request.DocumentFiles.Count > 0)
            {
                filesToUpload.AddRange(request.DocumentFiles.Where(f => f != null && f.Length > 0));
            }
            else if (request.DocumentFile != null && request.DocumentFile.Length > 0)
            {
                filesToUpload.Add(request.DocumentFile);
            }

            int retainedCount = request.RetainedDocumentUrls?.Count ?? 0;
            int totalFiles = retainedCount + filesToUpload.Count;

            if (totalFiles < 1)
                throw new InvalidOperationException("Please upload at least 1 document/certificate file.");

            if (totalFiles > 3)
                throw new InvalidOperationException("You can upload a maximum of 3 document/certificate files.");

            if (existing != null)
            {
                // Chỉ cho phép nộp lại nếu đơn đang ở trạng thái 'Rejected'
                if (existing.ApprovalStatus != InstructorApprovalStatus.Rejected.ToValue())
                    throw new InvalidOperationException("You have already submitted an instructor application.");

                // Chỉ giữ lại những URL thực sự thuộc về documentUrl cũ của chính user này
                var previousUrls = existing.DocumentUrl?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                var validRetained = request.RetainedDocumentUrls?
                    .Where(url => previousUrls.Contains(url))
                    .ToList() ?? new List<string>();

                var uploadedUrls = new List<string>();
                if (filesToUpload.Count > 0)
                {
                    foreach (var file in filesToUpload)
                    {
                        var url = await _uploadService.UploadImageAsync(file);
                        if (!string.IsNullOrEmpty(url))
                            uploadedUrls.Add(url);
                    }
                }

                var finalUrls = new List<string>(validRetained);
                finalUrls.AddRange(uploadedUrls);

                if (finalUrls.Count < 1)
                    throw new InvalidOperationException("Please upload at least 1 document/certificate file.");
                if (finalUrls.Count > 3)
                    throw new InvalidOperationException("You can upload a maximum of 3 document/certificate files.");

                existing.DocumentUrl = string.Join(";", finalUrls);

                existing.ProfessionalTitle = request.ProfessionalTitle;
                existing.ExpertiseCategories = request.ExpertiseCategories;
                existing.LinkedinUrl = request.LinkedinUrl;
                existing.YoutubeUrl = request.YoutubeUrl;
                existing.FacebookUrl = request.FacebookUrl;
                existing.StripeCountry = request.StripeCountry.ToUpper();
                existing.ApprovalStatus = InstructorApprovalStatus.Pending.ToValue();

                int rowsResubmit = await _repo.SaveChangesAsync();
                /* zero rows exception removed */
                return "Your application has been resubmitted. Please wait for admin approval.";
            }

            // ★ CV/Document bắt buộc cho đơn mới
            if (filesToUpload.Count == 0)
                throw new InvalidOperationException("Please upload at least 1 document/certificate file.");

            var uploadedUrlsNew = new List<string>();
            foreach (var file in filesToUpload)
            {
                var url = await _uploadService.UploadImageAsync(file);
                if (!string.IsNullOrEmpty(url))
                    uploadedUrlsNew.Add(url);
            }

            if (uploadedUrlsNew.Count == 0)
                throw new InvalidOperationException("Failed to upload document files.");

            string documentUrl = string.Join(";", uploadedUrlsNew);

            var instructor = new Instructor
            {
                InstructorId = userId,
                ProfessionalTitle = request.ProfessionalTitle,
                ExpertiseCategories = request.ExpertiseCategories,
                LinkedinUrl = request.LinkedinUrl,
                YoutubeUrl = request.YoutubeUrl,
                FacebookUrl = request.FacebookUrl,
                DocumentUrl = documentUrl,
                ApprovalStatus = InstructorApprovalStatus.Pending.ToValue(),
                StripeCountry = request.StripeCountry.ToUpper(),
                StripeAccountId = null,
                StripeOnboardingStatus = null,
                PayoutsEnabled = false,
                ChargesEnabled = false
            };

            await _repo.AddAsync(instructor);
            int rowsSubmit = await _repo.SaveChangesAsync();
            /* zero rows exception removed */

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

            if (status != InstructorApprovalStatus.Approved.ToValue() && status != InstructorApprovalStatus.Rejected.ToValue())
                throw new InvalidOperationException("Invalid status. Only 'Approved' or 'Rejected' are allowed.");

            instructor.ApprovalStatus = status;
            int rowsApprove = await _repo.SaveChangesAsync();
            /* zero rows exception removed */

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

            if (instructor.ApprovalStatus != InstructorApprovalStatus.Approved.ToValue())
                throw new InvalidOperationException("Application is not approved yet. Please wait for admin approval.");

            // Lấy email từ Account (qua User → Account navigation)
            var email = instructor.InstructorNavigation?.UserNavigation?.Email ?? "";

            // Lấy quốc gia mà giảng viên đã chọn khi đăng ký (từ DB)
            var country = !string.IsNullOrEmpty(instructor.StripeCountry)
                ? instructor.StripeCountry
                : "SG"; // Fallback mặc định nếu chưa chọn

            // Gọi Stripe Connect setup service
            var setupResult = await _stripeConnect.SetupExpressAccountAsync(
                userId,
                email,
                country,
                instructor.ProfessionalTitle ?? "",
                instructor.ExpertiseCategories ?? "",
                instructor.StripeAccountId
            );

            // Lưu stripe_account_id vào DB nếu chưa có hoặc có sự thay đổi
            if (instructor.StripeAccountId != setupResult.StripeAccountId)
            {
                instructor.StripeAccountId = setupResult.StripeAccountId;
                instructor.StripeOnboardingStatus = StripeOnboardingStatus.Pending.ToValue();
                int rowsSetup = await _repo.SaveChangesAsync();
                /* zero rows exception removed */
            }

            return new StripeSetupResponse
            {
                OnboardingUrl = setupResult.OnboardingUrl,
                StripeAccountId = setupResult.StripeAccountId
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

            if (instructor.StripeOnboardingStatus == StripeOnboardingStatus.Active.ToValue()) return StripeOnboardingStatus.Active.ToValue();

            var stripeAccountStatus = await _stripeConnect.GetAccountStatusAsync(instructor.StripeAccountId);

            // Chỉ cần điền xong form (DetailsSubmitted = true) → coi như THÀNH CÔNG 100%
            if (stripeAccountStatus.DetailsSubmitted)
            {
                instructor.PayoutsEnabled = true;
                instructor.ChargesEnabled = true;
                instructor.StripeOnboardingStatus = StripeOnboardingStatus.Active.ToValue();
                await _repo.SaveChangesAsync();
                return "Active";
            }

            // Nếu chưa nộp form
            instructor.PayoutsEnabled = stripeAccountStatus.PayoutsEnabled;
            instructor.ChargesEnabled = stripeAccountStatus.ChargesEnabled;
            instructor.StripeOnboardingStatus = StripeOnboardingStatus.Pending.ToValue();
            await _repo.SaveChangesAsync();

            return StripeOnboardingStatus.Pending.ToValue();
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
                dto.TotalStudents = stats.TotalStudentsCount;
                dto.AverageRating = (decimal)stats.InstructorRating;
                dto.TotalRevenue = stats.TotalRevenue;
            }

            var statusCounts = await _courseRepo.CountCoursesByStatusAsync(userId);
            dto.ActiveCoursesCount = statusCounts.GetValueOrDefault(CourseStatus.Published.ToValue(), 0);
            dto.PendingCoursesCount = statusCounts.GetValueOrDefault(CourseStatus.Pending.ToValue(), 0);
            dto.DraftCoursesCount = statusCounts.GetValueOrDefault(CourseStatus.Draft.ToValue(), 0);

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
                await _stripeConnect.DeleteAccountAsync(instructor.StripeAccountId);
                Console.WriteLine($"[STRIPE-RESET] ✅ Deleted Stripe account {instructor.StripeAccountId} for instructor {instructorId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[STRIPE-RESET] ⚠️ Stripe delete failed (may already be deleted): {ex.Message}");
            }

            // ★ Reset DB fields → Instructor phải onboard lại
            instructor.StripeAccountId = null;
            instructor.StripeOnboardingStatus = null;
            instructor.PayoutsEnabled = false;
            instructor.ChargesEnabled = false;
            int rowsReset = await _repo.SaveChangesAsync();
            /* zero rows exception removed */

            return $"Stripe account for instructor {instructorId} has been reset. The instructor needs to set up Stripe again.";
        }

        public async Task<string> GetStripeLoginLinkAsync(int userId)
        {
            var instructor = await _repo.GetByIdAsync(userId);
            if (instructor == null)
                throw new InvalidOperationException("Instructor not found.");

            if (string.IsNullOrEmpty(instructor.StripeAccountId))
                throw new InvalidOperationException("You do not have an active Stripe account. Please connect to Stripe first.");

            return await _stripeConnect.GetLoginLinkAsync(instructor.StripeAccountId);
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
            int rowsCountry = await _repo.SaveChangesAsync();
            /* zero rows exception removed */
        }

        public async Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseMarketplaceBE.Application.DTOs.InstructorPayoutDto>> GetPayoutsAsync(int userId, int page = 1, int pageSize = 10, string? keyword = null, string? sortBy = "date_desc", string? status = null, int? year = null, int? month = null)
        {
            return await _repo.GetPayoutsAsync(userId, page, pageSize, keyword, sortBy, status, year, month);
        }

        public async Task SyncPayoutsWithStripeAsync(int userId)
        {
            var instructor = await _repo.GetByIdAsync(userId);
            if (instructor == null || string.IsNullOrEmpty(instructor.StripeAccountId))
                return;

            // 1. Lấy danh sách Payouts gần đây từ Stripe cho Connected Account này
            var stripePayouts = await _stripeConnect.ListPayoutsAsync(instructor.StripeAccountId);

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
                    var balanceTransactions = await _stripeConnect.ListBalanceTransactionsAsync(instructor.StripeAccountId, sp.Id);

                    foreach (var bt in balanceTransactions)
                    {
                        // Chấp nhận cả 'payment' (py_xxx) và 'transfer' (tr_xxx)
                        if ((bt.Type != "transfer" && bt.Type != "payment") || string.IsNullOrEmpty(bt.SourceId)) continue;

                        var localPayout = await _financeRepo.GetPayoutByTransferIdAsync(bt.SourceId);
                        if (localPayout != null)
                        {
                            // Khớp lệnh thành công -> Gắn mã Payout và cập nhật trạng thái
                            localPayout.StripePayoutId = sp.Id;
                            UpdatePayoutStatusFromStripe(localPayout, sp.Status, sp.ArrivalDate);
                        }
                    }
                }
                else
                {
                    // Nếu ĐÃ có mã Payout -> Cập nhật trạng thái mới nhất (VD: từ in_transit sang paid)
                    foreach (var dbp in dbPayouts)
                    {
                        UpdatePayoutStatusFromStripe(dbp, sp.Status, sp.ArrivalDate);
                    }
                }
            }

            await _repo.SaveChangesAsync();
        }

        private void UpdatePayoutStatusFromStripe(InstructorPayout dbp, string status, DateTime? arrivalDate)
        {
            if (dbp.PayoutStatus == PayoutStatus.Refunded.ToValue())
            {
                return;
            }
            var statusLower = status.ToLower();
            if (statusLower == "paid")
            {
                dbp.PayoutStatus = PayoutStatus.Paid.ToValue();
                dbp.IsPaid = true;
                dbp.PaidToBankAt = arrivalDate;
            }
            else if (statusLower == "in_transit" || statusLower == "pending")
            {
                dbp.PayoutStatus = PayoutStatus.InTransit.ToValue();
            }
            else if (statusLower == "failed" || statusLower == "canceled")
            {
                dbp.PayoutStatus = PayoutStatus.Failed.ToValue();
                dbp.IsPaid = false;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // PUBLIC PROFILE — Trang thông tin công khai của giảng viên
        // ═══════════════════════════════════════════════════════════════════════
        public async Task<InstructorPublicProfileDto?> GetPublicProfileAsync(int instructorId)
        {
            var instructor = await _repo.GetByIdWithNavigationAsync(instructorId);
            if (instructor == null || instructor.ApprovalStatus != InstructorApprovalStatus.Approved.ToValue())
                return null;

            var user = instructor.InstructorNavigation;
            var stats = await _repo.GetStatsAsync(instructorId);
            var activeCourses = await _repo.CountActiveCoursesAsync(instructorId);

            // Lấy danh sách khóa học published
            var courses = instructor.Courses
                .Where(c => c.CourseStatus == CourseStatus.Published.ToValue())
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
}
