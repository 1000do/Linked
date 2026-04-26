namespace CourseMarketplaceBE.Application.DTOs
{
    public class UpdateCouponRequest
    {
        public string? CouponCode { get; set; }
        public string? CouponType { get; set; }
        public decimal? DiscountValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? UsageLimit { get; set; }
        public bool? IsActive { get; set; }
    }
}
