using System;
using System.Collections.Generic;

namespace CourseMarketplaceFE.Models
{
    public class TransactionListItemVM
    {
        public int TransactionId { get; set; }
        public string? StripeSessionId { get; set; }
        public DateTime? Date { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public string BuyerName { get; set; } = "";
        public string CourseTitle { get; set; } = "";
        public string InstructorName { get; set; } = "";
        public string? Currency { get; set; }
        public string? PayoutCurrency { get; set; }
        public string? RefundReason { get; set; }
        public string? RefundAdminNote { get; set; }
        public DateTime? RefundRequestedAt { get; set; }
        public bool IsGift { get; set; }
        public string? RecipientEmail { get; set; }
        public bool IsGiftClaimed { get; set; }
    }

    public class TransactionPagedVM
    {
        public List<TransactionListItemVM> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class InstructorFinancePageVM
    {
        public TransactionPagedVM Transactions { get; set; } = new();
        public InstructorPayoutPagedViewModel Payouts { get; set; } = new();
        public List<InstructorCourseRevenueResponse> CourseRevenues { get; set; } = new();
    }

    public class TransactionDetailVM
    {
        public int TransactionId { get; set; }
        public string? StripeSessionId { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public DateTime? Date { get; set; }
        public string? Status { get; set; }
        public string? Currency { get; set; }
        public string BuyerName { get; set; } = "";
        public string BuyerEmail { get; set; } = "";
        public string CourseTitle { get; set; } = "";
        public string? CourseThumbnail { get; set; }
        public string InstructorName { get; set; } = "";
        public string InstructorEmail { get; set; } = "";
        public decimal GrossAmount { get; set; }

        // ★ Mới thêm: Thông tin Coupon
        public decimal OriginalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public bool CouponUsed { get; set; }
        public string? CouponCode { get; set; }
        public string? CouponType { get; set; }
        public decimal? CouponDiscountValue { get; set; }

        public decimal TransferRate { get; set; }
        public decimal InstructorPayout { get; set; }
        public decimal PlatformProfit { get; set; }
        public bool IsPaid { get; set; }
        public string? RefundReason { get; set; }
        public string? RefundAdminNote { get; set; }
        public DateTime? RefundRequestedAt { get; set; }
    }

    public class ApiResp<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }

    public class RefundResultDto
    {
        public bool IsAutoRejected { get; set; }
        public string? RejectReason { get; set; }
    }
}
