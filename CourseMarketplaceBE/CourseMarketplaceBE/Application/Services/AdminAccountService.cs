using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.Services
{
    public class AdminAccountService : IAdminAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly IReportRepository _reportRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IChatRepository _chatRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILockoutRepository _lockoutRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IManagerRepository _managerRepository;
        private readonly INotificationService _notificationService;

        public AdminAccountService(
            IUserRepository userRepository,
            IReportRepository reportRepository,
            INotificationRepository notificationRepository,
            IChatRepository chatRepository,
            ITransactionRepository transactionRepository,
            ILockoutRepository lockoutRepository,
            IEnrollmentRepository enrollmentRepository,
            IManagerRepository managerRepository,
            INotificationService notificationService)
        {
            _userRepository = userRepository;
            _reportRepository = reportRepository;
            _notificationRepository = notificationRepository;
            _chatRepository = chatRepository;
            _transactionRepository = transactionRepository;
            _lockoutRepository = lockoutRepository;
            _enrollmentRepository = enrollmentRepository;
            _managerRepository = managerRepository;
            _notificationService = notificationService;
        }

        public async Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<AdminAccountListDto>> GetAccountsPagedAsync(string? keyword, string? role, int page, int pageSize)
        {
            var (items, totalCount) = await _userRepository.GetAccountsPagedAsync(keyword, role, page, pageSize);

            var dtos = items.Select(a =>
            {
                string? roleName = "user";
                string? fullName = a.User?.FullName;

                if (a.Manager != null)
                {
                    roleName = a.Manager.Role;
                    fullName = a.Manager.FullName ?? a.Manager.DisplayName;
                }
                else if (a.User?.Instructor != null)
                {
                    roleName = "instructor";
                }

                return new AdminAccountListDto
                {
                    AccountId = a.AccountId,
                    Email = a.Email,
                    FullName = fullName ?? "No Name",
                    Role = roleName,
                    AccountStatus = a.AccountStatus ?? AccountStatus.Active.ToValue(),
                    AccountCreatedAt = a.AccountCreatedAt,
                    AccountLastLoginAt = a.AccountLastLoginAt,
                    PhoneNumber = a.Manager?.PhoneNumber ?? a.PhoneNumber,
                    AvatarUrl = a.Manager?.AvatarUrl ?? a.AvatarUrl,
                    AccountFlagCount = a.AccountFlagCount ?? 0
                };
            }).ToList();

            return new CourseMarketplaceBE.Application.DTOs.Common.PagedResult<AdminAccountListDto>(dtos, totalCount, page, pageSize);
        }

        public async Task<AdminAccountDetailDto?> GetAccountDetailAsync(int id)
        {
            var account = await _userRepository.GetAccountByIdAsync(id);
            if (account == null) return null;

            string? roleName = "user";
            string? fullName = account.User?.FullName;

            if (account.Manager != null)
            {
                roleName = account.Manager.Role;
                fullName = account.Manager.FullName ?? account.Manager.DisplayName;
            }
            else if (account.User?.Instructor != null)
            {
                roleName = "instructor";
            }

            var activeLockout = await _lockoutRepository.GetActiveLockoutAsync(id, "account");

            var dto = new AdminAccountDetailDto
            {
                AccountId = account.AccountId,
                Email = account.Email,
                FullName = fullName ?? "No Name",
                PhoneNumber = account.Manager?.PhoneNumber ?? account.PhoneNumber,
                AccountStatus = account.AccountStatus ?? AccountStatus.Active.ToValue(),
                AccountCreatedAt = account.AccountCreatedAt,
                AccountLastLoginAt = account.AccountLastLoginAt,
                AvatarUrl = account.Manager?.AvatarUrl ?? account.AvatarUrl,
                AccountFlagCount = account.AccountFlagCount ?? 0,
                Role = roleName,
                Bio = account.Manager?.Bio,
                LockoutStart = activeLockout?.LockoutStart,
                LockoutEnd = activeLockout?.LockoutEnd
            };

            // Nếu là Staff hoặc Admin (Manager)
            if (account.Manager != null)
            {
                dto.ResolvedReportsCount = await _reportRepository.GetResolvedReportsCountAsync(id);
                dto.SentNotificationsCount = await _notificationRepository.GetSentNotificationsCountAsync(id);
                dto.ActiveChatsCount = await _chatRepository.GetActiveChatsCountAsync(id);
            }

            // Nếu là Học viên
            if (account.User != null)
            {
                dto.TotalSpent = await _transactionRepository.GetTotalSpentAsync(id);
                dto.EnrolledCoursesCount = (await _enrollmentRepository.GetMyEnrolledCoursesAsync(id)).Count;

                // Nếu là Giảng viên
                var inst = account.User.Instructor;
                if (inst != null)
                {
                    dto.IsInstructor = true;
                    dto.ProfessionalTitle = inst.ProfessionalTitle;
                    dto.ExpertiseCategories = inst.ExpertiseCategories;
                    dto.LinkedinUrl = inst.LinkedinUrl;
                    dto.StripeAccountId = inst.StripeAccountId;
                    dto.StripeOnboardingStatus = inst.StripeOnboardingStatus;
                    dto.PayoutsEnabled = inst.PayoutsEnabled;
                    dto.ChargesEnabled = inst.ChargesEnabled;

                    dto.TotalRevenue = await _transactionRepository.GetInstructorTotalRevenueAsync(id);
                    dto.TotalWithdrawn = await _transactionRepository.GetInstructorTotalWithdrawnAsync(id);
                    dto.AvailableBalance = dto.TotalRevenue - dto.TotalWithdrawn;
                }
            }

            return dto;
        }

        public async Task<AccountTransactionSummaryDto?> GetAccountTransactionsAsync(int id)
        {
            var account = await _userRepository.GetAccountByIdAsync(id);
            if (account == null) return null;

            return await _transactionRepository.GetAccountTransactionsSummaryAsync(id);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _userRepository.IsEmailExistsAsync(email);
        }

        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            return await _userRepository.IsUsernameExistsAsync(username);
        }

        public async Task CreateStaffAsync(CreateStaffRequest request)
        {
            var newAccount = new Account
            {
                Username = request.Username,
                Email = request.Username, // Use username as email placeholder for staff
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber,
                AvatarUrl = request.AvatarUrl,
                AccountStatus = AccountStatus.Active.ToValue(),
                AccountCreatedAt = DateTime.UtcNow,
                IsVerified = true // Auto-verify admin-created staff
            };

            var newManager = new Manager
            {
                Role = "staff",
                DisplayName = request.DisplayName,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                AvatarUrl = request.AvatarUrl,
                Bio = request.Bio
            };

            bool success = await _userRepository.RegisterManagerAsync(newAccount, newManager);
            if (!success)
                throw new InvalidOperationException("Failed to save changes when creating staff account.");
        }

        public async Task<bool> UpdateStaffAsync(int id, UpdateStaffRequest request)
        {
            var account = await _userRepository.GetAccountByIdAsync(id);
            if (account == null || account.Manager == null || account.Manager.Role != "staff")
            {
                return false;
            }

            account.Manager.DisplayName = request.DisplayName;
            account.Manager.FullName = request.FullName;
            account.Manager.PhoneNumber = request.PhoneNumber;
            account.Manager.AvatarUrl = request.AvatarUrl;
            account.Manager.Bio = request.Bio;

            account.PhoneNumber = request.PhoneNumber;
            if (request.AvatarUrl != null)
            {
                account.AvatarUrl = request.AvatarUrl;
            }

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            if (!string.IsNullOrWhiteSpace(request.AccountStatus))
            {
                var oldStatus = account.AccountStatus;
                account.AccountStatus = request.AccountStatus;
                if (request.AccountStatus == AccountStatus.Banned.ToValue() && oldStatus != AccountStatus.Banned.ToValue())
                {
                    var lockout = new Lockout
                    {
                        AccountId = account.AccountId,
                        LockoutType = "account",
                        LockoutLevel = "severe",
                        LockoutStart = DateTime.UtcNow,
                        LockoutEnd = DateTime.UtcNow.AddYears(100)
                    };
                    await _lockoutRepository.AddAsync(lockout);
                }
                else if (request.AccountStatus != AccountStatus.Banned.ToValue() && oldStatus == AccountStatus.Banned.ToValue())
                {
                    await _lockoutRepository.RemoveAccountLockoutsAsync(account.AccountId);
                }
            }

            int rows3 = await _userRepository.SaveChangesAsync();
            /* zero rows exception removed */
            return true;
        }

        public async Task<string?> ToggleBanAsync(int id, int adminId)
        {
            var account = await _userRepository.GetAccountByIdAsync(id);
            if (account == null) return null;

            // Không cho phép tự ban chính mình (Super Admin)
            if (account.Manager != null && account.Manager.Role == "admin")
            {
                throw new InvalidOperationException("Cannot ban the Super Admin account.");
            }

            if (account.AccountStatus == AccountStatus.Banned.ToValue())
            {
                account.AccountStatus = AccountStatus.Active.ToValue();
                await _lockoutRepository.RemoveAccountLockoutsAsync(account.AccountId);
            }
            else
            {
                account.AccountStatus = AccountStatus.Banned.ToValue();
                var lockout = new Lockout
                {
                    AccountId = account.AccountId,
                    LockoutType = "account",
                    LockoutLevel = "severe",
                    LockoutStart = DateTime.UtcNow,
                    LockoutEnd = DateTime.UtcNow.AddYears(100)
                };
                await _lockoutRepository.AddAsync(lockout);
            }

            int rows4 = await _userRepository.SaveChangesAsync();
            /* zero rows exception removed */
            return account.AccountStatus;
        }

        public async Task<(bool Success, int CurrentFlags, string? NewStatus, string? ErrorMessage)> FlagAccountAsync(int id, string reason)
        {
            var account = await _userRepository.GetAccountByIdAsync(id);
            if (account == null)
            {
                return (false, 0, null, "Account not found.");
            }

            var currentFlags = account.AccountFlagCount ?? 0;
            if (currentFlags >= 3 || account.AccountStatus == AccountStatus.Banned.ToValue())
            {
                return (false, currentFlags, account.AccountStatus, "Account is already banned or has reached the maximum flag limit of 3.");
            }

            var newFlags = currentFlags + 1;
            account.AccountFlagCount = newFlags;

            if (newFlags == 1)
            {
                account.AccountStatus = AccountStatus.Flagged1.ToValue();
            }
            else if (newFlags == 2)
            {
                account.AccountStatus = AccountStatus.Flagged2.ToValue();
            }
            else if (newFlags >= 3)
            {
                account.AccountStatus = AccountStatus.Banned.ToValue();
                var lockout = new Lockout
                {
                    AccountId = account.AccountId,
                    LockoutType = "account",
                    LockoutLevel = "severe",
                    LockoutStart = DateTime.UtcNow,
                    LockoutEnd = DateTime.UtcNow.AddDays(30)
                };
                await _lockoutRepository.AddAsync(lockout);
            }

            int rows5 = await _userRepository.SaveChangesAsync();
            /* zero rows exception removed */

            // Send notification
            string reasonStr = string.IsNullOrWhiteSpace(reason) ? "No reason specified" : reason;
            string notiTitle = "Account Flagged Warning";
            string notiContent = $"Your account has been flagged. Reason: {reasonStr}. Current flags: {newFlags}/3.";
            if (newFlags >= 3)
            {
                notiTitle = "Account Banned Notification";
                notiContent = $"Your account has been banned due to repeated violations. Reason: {reasonStr}.";
            }

            try
            {
                await _notificationService.SendNotificationAsync(account.AccountId, notiTitle, notiContent, "/profile");
            }
            catch (Exception notiEx)
            {
                Console.WriteLine($"Error sending notification: {notiEx.Message}");
            }

            return (true, newFlags, account.AccountStatus, null);
        }
    }
}
