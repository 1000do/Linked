using System;
using System.Collections.Generic;

namespace CourseMarketplaceFE.Models;

public class AdminAccountListItem
{
    public int AccountId { get; set; }
    public string Email { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public string? AccountStatus { get; set; }
    public DateTime? AccountCreatedAt { get; set; }
    public DateTime? AccountLastLoginAt { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public int AccountFlagCount { get; set; }
}

public class AdminAccountListViewModel
{
    public List<AdminAccountListItem> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? Keyword { get; set; }
    public string? Role { get; set; }
}

public class AdminAccountDetailViewModel
{
    public int AccountId { get; set; }
    public string Email { get; set; } = null!;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AccountStatus { get; set; }
    public DateTime? AccountCreatedAt { get; set; }
    public DateTime? AccountLastLoginAt { get; set; }
    public string? AvatarUrl { get; set; }
    public int AccountFlagCount { get; set; }
    public string? Role { get; set; }
    public string? Bio { get; set; }
    public DateTime? LockoutStart { get; set; }
    public DateTime? LockoutEnd { get; set; }

    // User-specific details
    public decimal TotalSpent { get; set; }
    public int EnrolledCoursesCount { get; set; }

    // Manager-specific details (Staff/Admin)
    public int ResolvedReportsCount { get; set; }
    public int SentNotificationsCount { get; set; }
    public int ActiveChatsCount { get; set; }

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
    public decimal AvailableBalance { get; set; }
}

public class PaymentViewModel
{
    public int TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? StudentName { get; set; }
    public string? CourseTitle { get; set; }
}

public class TransferViewModel
{
    public int TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public decimal TransferRate { get; set; }
    public string? CourseTitle { get; set; }
}

public class PayoutViewModel
{
    public int PayoutId { get; set; }
    public decimal PayoutAmount { get; set; }
    public string PayoutStatus { get; set; } = "pending";
    public DateTime PayoutDate { get; set; }
    public string? StripeTransferId { get; set; }
    public string? StripePayoutId { get; set; }
    public DateTime? PaidToBankAt { get; set; }
    public bool IsPaid { get; set; }
}

public class AccountTransactionSummaryViewModel
{
    public List<PaymentViewModel> Payments { get; set; } = new();
    public List<TransferViewModel> Transfers { get; set; } = new();
    public List<PayoutViewModel> Payouts { get; set; } = new();
}

public class AdminAccountDetailPageViewModel
{
    public AdminAccountDetailViewModel Account { get; set; } = null!;
    public AccountTransactionSummaryViewModel Transactions { get; set; } = null!;
}

public class CreateStaffFERequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
}

public class UpdateStaffFERequest
{
    public string DisplayName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public string? AccountStatus { get; set; }
}

public class FlagAccountFERequest
{
    public string Reason { get; set; } = string.Empty;
}
