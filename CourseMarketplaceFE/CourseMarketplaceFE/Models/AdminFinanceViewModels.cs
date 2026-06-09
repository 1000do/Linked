using System;
using System.Collections.Generic;

namespace CourseMarketplaceFE.Models
{
    public class FinancialSummaryVM
    {
        public decimal GrossRevenue { get; set; }
        public decimal TotalPaidOut { get; set; }
        public decimal PendingEscrow { get; set; }
        public decimal MaturedEscrow { get; set; }
        public decimal PlatformNetProfit { get; set; }
        public decimal CurrentTransferRate { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalRefunded { get; set; }
    }

    public class PayoutDetailVM
    {
        public int PayoutId { get; set; }
        public int TransactionId { get; set; }
        public string InstructorName { get; set; } = "";
        public string InstructorEmail { get; set; } = "";
        public string CourseTitle { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public decimal InstructorReceived { get; set; }
        public decimal PlatformReceived { get; set; }
        public decimal TransferRate { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime PayoutDate { get; set; }

        // ★ Webhook tracking fields
        /// <summary>pending | transferred | in_transit | paid | failed</summary>
        public string? PayoutStatus { get; set; }
        public string? StripeTransferId { get; set; }
        public string? StripePayoutId { get; set; }
        public DateTime? PaidToBankAt { get; set; }
    }

    public class FinanceDashboardVM
    {
        public FinancialSummaryVM Summary { get; set; } = new();
        public List<PayoutDetailVM> Payouts { get; set; } = new();
    }

    public class PlatformBalanceVM
    {
        public decimal Available { get; set; }
        public decimal Incoming { get; set; }
        public decimal Total { get; set; }
        public string Currency { get; set; } = "usd";
        public string PayoutScheduleInterval { get; set; } = "manual";
        public string? PayoutScheduleAnchor { get; set; }
    }

    public class WithdrawalHistoryVM
    {
        public int WithdrawalId { get; set; }
        public string? ManagerName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string? StripePayoutId { get; set; }
        public string Status { get; set; } = "pending";
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ArrivedAt { get; set; }
    }

    public class WithdrawPageVM
    {
        public PlatformBalanceVM Balance { get; set; } = new();
        public List<WithdrawalHistoryVM> History { get; set; } = new();
    }

    public class RefundRequestVM
    {
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        
        private string? _refundReason;
        public string? RefundReason
        {
            get => _refundReason ?? TransactionExt?.RefundReason;
            set => _refundReason = value;
        }

        private DateTime? _refundRequestedAt;
        public DateTime? RefundRequestedAt
        {
            get => _refundRequestedAt ?? TransactionExt?.RefundRequestedAt;
            set => _refundRequestedAt = value;
        }

        public DateTime? TransactionCreatedAt { get; set; }

        public OrderItemDto? OrderItem { get; set; }
        public AccountDto? AccountFromNavigation { get; set; }
        public TransactionExtDto? TransactionExt { get; set; }

        public string CourseTitle => OrderItem?.Course?.Title ?? "N/A";
        public string BuyerName => AccountFromNavigation?.User?.FullName ?? "N/A";
    }

    public class TransactionExtDto
    {
        public int TransactionId { get; set; }
        public string? RefundReason { get; set; }
        public string? RefundAdminNote { get; set; }
        public DateTime? RefundRequestedAt { get; set; }
    }

    public class OrderItemDto
    {
        public CourseDto? Course { get; set; }
    }

    public class CourseDto
    {
        public string Title { get; set; } = "";
    }

    public class AccountDto
    {
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public string FullName { get; set; } = "";
    }

    public class InstructorCourseRevenueResponse
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public int SalesCount { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal PreviousMonthRevenue { get; set; }
        public decimal YearlyRevenue { get; set; }
        public decimal LifetimeRevenue { get; set; }
    }

    public class AdminFinanceUnifiedVM
    {
        public FinanceDashboardVM Dashboard { get; set; } = new();
        public WithdrawPageVM Withdraw { get; set; } = new();
        public TransactionPagedVM Transactions { get; set; } = new();
        public PagedResult<PayoutDetailVM> Payouts { get; set; } = new();
        public PagedResult<WithdrawalHistoryVM> Withdrawals { get; set; } = new();
        public PagedResult<RefundRequestVM> PendingRefunds { get; set; } = new();
        public string PayoutDays { get; set; } = "15";
        public List<InstructorCourseRevenueResponse> CourseRevenues { get; set; } = new();
        public Dictionary<int, FinancialSummaryVM> YearlySummaries { get; set; } = new();
    }

    public class InstructorCourseRevenuesPageVM
    {
        public List<InstructorCourseRevenueResponse> Revenues { get; set; } = new();
        public int TotalActiveCourses { get; set; }
        public string TopSellingCourseTitle { get; set; } = "N/A";
        public int TopSellingCourseSales { get; set; }
        public string HighestEarningCourseTitle { get; set; } = "N/A";
        public decimal HighestEarningCourseRevenue { get; set; }

        public Dictionary<int, decimal[]> YearlyMonthlyRevenue { get; set; } = new();
        public decimal TotalGrossRevenue { get; set; }
        public int TotalCoursesSold { get; set; }
        public decimal PlatformNetProfit { get; set; }
        public int TotalNewLearners { get; set; }
    }
}
