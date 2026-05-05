namespace CourseMarketplaceBE.Application.DTOs
{
    public class UpdateCouponRequest
    {
        public string? CouponCode { get; set; }
        public string? CouponType { get; set; }
        public decimal? DiscountValue { get; set; }
        /// <summary>Giá trị đơn tối thiểu để áp dụng coupon. Null = giữ nguyên giá trị cũ.</summary>
        public decimal? MinOrderValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? UsageLimit { get; set; }
        public bool? IsActive { get; set; }
    }
}
