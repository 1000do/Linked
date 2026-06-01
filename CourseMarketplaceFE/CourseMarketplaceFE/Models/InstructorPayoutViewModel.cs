using System;
using System.Collections.Generic;

namespace CourseMarketplaceFE.Models
{
    public class InstructorPayoutViewModel
    {
        public int PayoutId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PayoutDate { get; set; }
        public bool IsPaid { get; set; }
        public string CourseTitle { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public string? PayoutStatus { get; set; }
        public DateTime? PaidToBankAt { get; set; }
        public string? StripeTransferId { get; set; }
        public string? StripePayoutId { get; set; }
    }

    public class InstructorPayoutPagedViewModel
    {
        public List<InstructorPayoutViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
