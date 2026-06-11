using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Application.DTOs;

public class AdminAccountListDto
{
    public int AccountId { get; set; }
    public string Email { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Role { get; set; } // admin, staff, instructor, user
    public string? AccountStatus { get; set; }
    public DateTime? AccountCreatedAt { get; set; }
    public DateTime? AccountLastLoginAt { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public int? AccountFlagCount { get; set; }
}

public class CreateStaffRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
}

public class UpdateStaffRequest
{
    public string DisplayName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; } // Optional password reset
    public string? AccountStatus { get; set; }
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
}

public class AdminAccountDetailDto
{
    public int AccountId { get; set; }
    public string Email { get; set; } = null!;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AccountStatus { get; set; }
    public DateTime? AccountCreatedAt { get; set; }
    public DateTime? AccountLastLoginAt { get; set; }
    public string? AvatarUrl { get; set; }
    public int? AccountFlagCount { get; set; }
    public string? Role { get; set; }
    public string? Bio { get; set; }
    public DateTime? LockoutStart { get; set; }
    public DateTime? LockoutEnd { get; set; }

    // User-specific details (Students/Instructors)
    public decimal TotalSpent { get; set; }
    public int EnrolledCoursesCount { get; set; }

    // Instructor-specific details
    public bool IsInstructor { get; set; }
    public string? ProfessionalTitle { get; set; }
    public string? ExpertiseCategories { get; set; }
    public string? LinkedinUrl { get; set; }
    public string? StripeAccountId { get; set; }
    public string? StripeOnboardingStatus { get; set; }
    public bool? PayoutsEnabled { get; set; }
    public bool? ChargesEnabled { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalWithdrawn { get; set; }
    public decimal AvailableBalance { get; set; } // TotalRevenue - TotalWithdrawn
}

public class PaymentDto
{
    public int TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? StudentName { get; set; }
    public string? CourseTitle { get; set; }
}

public class TransferDto
{
    public int TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public decimal TransferRate { get; set; }
    public string? CourseTitle { get; set; }
}

public class PayoutDto
{
    public int PayoutId { get; set; }
    public decimal PayoutAmount { get; set; }
    public string PayoutStatus { get; set; } = "pending";
    public DateTime PayoutDate { get; set; }
    public string? StripeTransferId { get; set; }
    public string? StripePayoutId { get; set; }
    public DateTime? PaidToBankAt { get; set; }
}

public class AccountTransactionSummaryDto
{
    public List<PaymentDto> Payments { get; set; } = new();
    public List<TransferDto> Transfers { get; set; } = new();
    public List<PayoutDto> Payouts { get; set; } = new();
}
