using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Presentation.Controllers;

[Route("api/admin/accounts")]
[ApiController]
[Authorize(Roles = "admin")]
public class AdminAccountController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public AdminAccountController(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/admin/accounts
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> GetAccounts(
        [FromQuery] string? keyword,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Accounts
                .Include(a => a.Manager)
                .Include(a => a.User)
                    .ThenInclude(u => u!.Instructor)
                .AsQueryable();

            // Lọc theo vai trò (Role)
            if (!string.IsNullOrWhiteSpace(role) && role.ToLower() != "all")
            {
                var roleLower = role.ToLower();
                if (roleLower == "staff")
                {
                    query = query.Where(a => a.Manager != null && a.Manager.Role == "staff");
                }
                else if (roleLower == "registered_staff")
                {
                    // Lấy toàn bộ staff đã được kích hoạt hoặc đã login ít nhất 1 lần
                    query = query.Where(a => a.Manager != null && a.Manager.Role == "staff" && a.IsVerified);
                }
                else if (roleLower == "user")
                {
                    // Học viên hoặc giảng viên (Không có bản ghi Manager)
                    query = query.Where(a => a.Manager == null);
                }
                else if (roleLower == "instructor")
                {
                    query = query.Where(a => a.User != null && a.User.Instructor != null);
                }
            }

            // Tìm kiếm keyword (Email, Name, Phone)
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.ToLower();
                query = query.Where(a =>
                    a.Email.ToLower().Contains(kw) ||
                    (a.PhoneNumber != null && a.PhoneNumber.Contains(kw)) ||
                    (a.Manager != null && a.Manager.DisplayName.ToLower().Contains(kw)) ||
                    (a.User != null && a.User.FullName.ToLower().Contains(kw))
                );
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(a => a.AccountCreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(a =>
            {
                string? roleName = "user";
                string? fullName = a.User?.FullName;

                if (a.Manager != null)
                {
                    roleName = a.Manager.Role;
                    fullName = a.Manager.DisplayName;
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
                    AccountStatus = a.AccountStatus ?? "Active",
                    AccountCreatedAt = a.AccountCreatedAt,
                    AccountLastLoginAt = a.AccountLastLoginAt,
                    PhoneNumber = a.PhoneNumber,
                    AvatarUrl = a.AvatarUrl,
                    AccountFlagCount = a.AccountFlagCount ?? 0
                };
            }).ToList();

            return Ok(new
            {
                status = 200,
                message = "Accounts retrieved successfully.",
                data = dtos,
                totalCount = totalCount,
                page = page,
                pageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Internal server error: {ex.Message}" });
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/admin/accounts/{id}
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountDetail(int id)
    {
        try
        {
            var account = await _context.Accounts
                .Include(a => a.Manager)
                .Include(a => a.User)
                    .ThenInclude(u => u!.Instructor)
                .FirstOrDefaultAsync(a => a.AccountId == id);

            if (account == null)
            {
                return NotFound(new { status = 404, message = "Account not found." });
            }

            string? roleName = "user";
            string? fullName = account.User?.FullName;

            if (account.Manager != null)
            {
                roleName = account.Manager.Role;
                fullName = account.Manager.DisplayName;
            }
            else if (account.User?.Instructor != null)
            {
                roleName = "instructor";
            }

            var dto = new AdminAccountDetailDto
            {
                AccountId = account.AccountId,
                Email = account.Email,
                FullName = fullName ?? "No Name",
                PhoneNumber = account.PhoneNumber,
                AccountStatus = account.AccountStatus ?? "Active",
                AccountCreatedAt = account.AccountCreatedAt,
                AccountLastLoginAt = account.AccountLastLoginAt,
                AvatarUrl = account.AvatarUrl,
                AccountFlagCount = account.AccountFlagCount ?? 0,
                Role = roleName
            };

            // Nếu là Học viên
            if (account.User != null)
            {
                // Tính tổng tiền chi tiêu mua khóa học (Chỉ lấy giao dịch Thành công của User này)
                dto.TotalSpent = await _context.Transactions
                    .Where(t => t.AccountFrom == account.AccountId && t.TransactionsStatus == "Success")
                    .SumAsync(t => t.Amount);

                dto.EnrolledCoursesCount = await _context.Enrollments
                    .CountAsync(e => e.UserId == account.AccountId);

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

                    // Doanh thu giảng viên: Tổng tiền các Transactions thành công được chuyển cho giảng viên này
                    dto.TotalRevenue = await _context.Transactions
                        .Where(t => t.AccountTo == account.AccountId && t.TransactionsStatus == "Success")
                        .SumAsync(t => t.Amount);

                    // Số tiền đã rút (payout): Tổng tiền InstructorPayouts có PayoutStatus == "paid"
                    dto.TotalWithdrawn = await _context.InstructorPayouts
                        .Where(p => p.InstructorId == account.AccountId && p.PayoutStatus == "paid")
                        .SumAsync(p => p.PayoutAmount);

                    dto.AvailableBalance = dto.TotalRevenue - dto.TotalWithdrawn;
                }
            }

            return Ok(new { status = 200, message = "Account details retrieved successfully.", data = dto });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Internal server error: {ex.Message}" });
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/admin/accounts/{id}/transactions
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("{id}/transactions")]
    public async Task<IActionResult> GetAccountTransactions(int id)
    {
        try
        {
            var account = await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AccountId == id);

            if (account == null)
            {
                return NotFound(new { status = 404, message = "Account not found." });
            }

            var summary = new AccountTransactionSummaryDto();

            // 1. Payments: Giao dịch thanh toán mua hàng của học viên (hoặc mua khóa học của giảng viên)
            var payments = await _context.Transactions
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi!.Course)
                .Include(t => t.AccountFromNavigation)
                    .ThenInclude(af => af!.User)
                .Where(t => (t.AccountFrom == id || t.AccountTo == id) && (t.TransactionType == "payment" || t.TransactionType == null))
                .OrderByDescending(t => t.TransactionCreatedAt)
                .ToListAsync();

            summary.Payments = payments.Select(t => new PaymentDto
            {
                TransactionId = t.TransactionId,
                Amount = t.Amount,
                Status = t.TransactionsStatus,
                CreatedAt = t.TransactionCreatedAt,
                StudentName = t.AccountFromNavigation?.User?.FullName ?? t.AccountFromNavigation?.Email ?? "System/Guest",
                CourseTitle = t.OrderItem?.Course?.Title ?? "Direct / Platform Deposit"
            }).ToList();

            // 2. Transfers: Giao dịch chuyển đổi nội bộ (sàn chia doanh thu cho giảng viên)
            var transfers = await _context.Transactions
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi!.Course)
                .Where(t => t.AccountTo == id && t.TransactionType == "transfer")
                .OrderByDescending(t => t.TransactionCreatedAt)
                .ToListAsync();

            summary.Transfers = transfers.Select(t => new TransferDto
            {
                TransactionId = t.TransactionId,
                Amount = t.Amount,
                Status = t.TransactionsStatus,
                CreatedAt = t.TransactionCreatedAt,
                TransferRate = t.TransferRate,
                CourseTitle = t.OrderItem?.Course?.Title ?? "Platform Split Share"
            }).ToList();

            // 3. Payouts: Lịch sử rút tiền về tài khoản ngân hàng của giảng viên (instructor)
            var payouts = await _context.InstructorPayouts
                .Where(p => p.InstructorId == id)
                .OrderByDescending(p => p.PayoutDate)
                .ToListAsync();

            summary.Payouts = payouts.Select(p => new PayoutDto
            {
                PayoutId = p.PayoutId,
                PayoutAmount = p.PayoutAmount,
                PayoutStatus = p.PayoutStatus,
                PayoutDate = p.PayoutDate,
                StripeTransferId = p.StripeTransferId,
                StripePayoutId = p.StripePayoutId,
                PaidToBankAt = p.PaidToBankAt
            }).ToList();

            return Ok(new { status = 200, message = "Account transactions retrieved successfully.", data = summary });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Internal server error: {ex.Message}" });
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/admin/accounts/staff (Tạo Staff)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("staff")]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.DisplayName))
            {
                return BadRequest(new { status = 400, message = "Email, password, and display name are required." });
            }

            var existing = await _context.Accounts.AnyAsync(a => a.Email.ToLower() == request.Email.ToLower());
            if (existing)
            {
                return BadRequest(new { status = 400, message = "Email is already registered." });
            }

            var newAccount = new Account
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber,
                AccountStatus = "Active",
                AccountCreatedAt = DateTime.UtcNow,
                IsVerified = true // Auto-verify admin-created staff
            };

            await _context.Accounts.AddAsync(newAccount);
            await _context.SaveChangesAsync();

            var newManager = new Manager
            {
                ManagerId = newAccount.AccountId,
                Role = "staff",
                DisplayName = request.DisplayName
            };

            await _context.Managers.AddAsync(newManager);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Staff account created successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Internal server error: {ex.Message}" });
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // PUT /api/admin/accounts/staff/{id} (Cập nhật Staff)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPut("staff/{id}")]
    public async Task<IActionResult> UpdateStaff(int id, [FromBody] UpdateStaffRequest request)
    {
        try
        {
            var account = await _context.Accounts
                .Include(a => a.Manager)
                .FirstOrDefaultAsync(a => a.AccountId == id);

            if (account == null || account.Manager == null || account.Manager.Role != "staff")
            {
                return NotFound(new { status = 404, message = "Staff account not found." });
            }

            if (string.IsNullOrWhiteSpace(request.DisplayName))
            {
                return BadRequest(new { status = 400, message = "Display name is required." });
            }

            account.Manager.DisplayName = request.DisplayName;
            account.PhoneNumber = request.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            if (!string.IsNullOrWhiteSpace(request.AccountStatus))
            {
                account.AccountStatus = request.AccountStatus;
            }

            await _context.SaveChangesAsync();
            return Ok(new { status = 200, message = "Staff account updated successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Internal server error: {ex.Message}" });
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/admin/accounts/{id}/toggle-ban (Khoá/mở khoá)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("{id}/toggle-ban")]
    public async Task<IActionResult> ToggleBan(int id)
    {
        try
        {
            var account = await _context.Accounts
                .Include(a => a.Manager)
                .FirstOrDefaultAsync(a => a.AccountId == id);

            if (account == null)
            {
                return NotFound(new { status = 404, message = "Account not found." });
            }

            // Không cho phép tự ban chính mình (Super Admin)
            if (account.Manager != null && account.Manager.Role == "admin")
            {
                return BadRequest(new { status = 400, message = "Cannot ban the Super Admin account." });
            }

            if (account.AccountStatus == "Banned")
            {
                account.AccountStatus = "Active";
            }
            else
            {
                account.AccountStatus = "Banned";
            }

            await _context.SaveChangesAsync();
            return Ok(new { status = 200, message = $"Account status changed to {account.AccountStatus} successfully.", currentStatus = account.AccountStatus });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Internal server error: {ex.Message}" });
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/admin/accounts/{id}/flag (Tăng flag count)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("{id}/flag")]
    public async Task<IActionResult> FlagAccount(int id, [FromBody] FlagAccountRequest request)
    {
        try
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound(new { status = 404, message = "Account not found." });
            }

            var currentFlags = account.AccountFlagCount ?? 0;
            if (currentFlags >= 3 || account.AccountStatus == "Banned")
            {
                return BadRequest(new { status = 400, message = "Account is already banned or has reached the maximum flag limit of 3." });
            }

            var newFlags = currentFlags + 1;
            account.AccountFlagCount = newFlags;

            if (newFlags == 1)
            {
                account.AccountStatus = "Flagged_1";
            }
            else if (newFlags == 2)
            {
                account.AccountStatus = "Flagged_2";
            }
            else if (newFlags >= 3)
            {
                account.AccountStatus = "Banned";
            }

            await _context.SaveChangesAsync();

            // Send notification
            string reason = string.IsNullOrWhiteSpace(request?.Reason) ? "No reason specified" : request.Reason;
            string notiTitle = "Account Flagged Warning";
            string notiContent = $"Your account has been flagged. Reason: {reason}. Current flags: {newFlags}/3.";
            if (newFlags >= 3)
            {
                notiTitle = "Account Banned Notification";
                notiContent = $"Your account has been banned due to repeated violations. Reason: {reason}.";
            }

            try
            {
                await _notificationService.SendNotificationAsync(account.AccountId, notiTitle, notiContent, "/profile");
            }
            catch (Exception notiEx)
            {
                Console.WriteLine($"Error sending notification: {notiEx.Message}");
            }

            return Ok(new { status = 200, message = "Account flagged successfully.", currentFlags = newFlags, newStatus = account.AccountStatus });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Internal server error: {ex.Message}" });
        }
    }
}

public class FlagAccountRequest
{
    public string Reason { get; set; } = string.Empty;
}
