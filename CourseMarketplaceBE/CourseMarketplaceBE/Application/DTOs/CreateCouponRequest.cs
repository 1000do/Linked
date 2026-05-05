namespace CourseMarketplaceBE.Application.DTOs
{
    public class CreateCouponRequest
    {
        public string CouponCode { get; set; } = null!;
        public string? CouponType { get; set; }
        public decimal DiscountValue { get; set; }
        /// <summary>Giá trị đơn tối thiểu để áp dụng coupon (0 = không yêu cầu).</summary>
        public decimal MinOrderValue { get; set; } = 0;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? UsageLimit { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
