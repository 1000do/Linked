namespace CourseMarketplaceBE.Application.DTOs
{
    public class CreateCouponRequest
    {
        public string CouponCode { get; set; } = null!;
        public string? CouponType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? UsageLimit { get; set; }
    }
}
