using System;

namespace CourseMarketplaceBE.Application.DTOs
{
    public class PayoutDetailProjection
    {
        public int PayoutId { get; set; }
        public int TransactionId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string InstructorEmail { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal InstructorReceived { get; set; }
        public decimal TransferRate { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime PayoutDate { get; set; }

        // ★ Cột bổ sung cho Webhook tracking
        public string? PayoutStatus { get; set; }
        public string? StripeTransferId { get; set; }
        public string? StripePayoutId { get; set; }
        public DateTime? PaidToBankAt { get; set; }
    }
}
