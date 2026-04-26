namespace CourseMarketplaceBE.Application.DTOs
{
    public class CouponResponse
    {
        public int CouponId { get; set; }
        public string CouponCode { get; set; } = null!;
        public string CouponType { get; set; } = null!;
        public decimal DiscountValue { get; set; }
        public DateTime? StartDate { get; set; } 
        public DateTime? EndDate { get; set; }  
        public int? UsageLimit { get; set; }    
        public bool? IsActive { get; set; }
    }
}
