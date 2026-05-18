namespace CourseMarketplaceBE.Application.DTOs
{
    /// <summary>
    /// UC-64: Chỉ cho phép sửa end_date và usage_limit.
    /// coupon_code, coupon_type, discount_value đã bị khóa (read-only) 
    /// để không phá vỡ lịch sử kế toán đơn hàng cũ.
    /// </summary>
    public class UpdateCouponRequest
    {
        /// <summary>Ngày kết thúc mới (null = giữ nguyên giá trị cũ).</summary>
        public DateTime? EndDate { get; set; }

        /// <summary>Số lượng giới hạn mới (null = giữ nguyên giá trị cũ).</summary>
        public int? UsageLimit { get; set; }

        /// <summary>Bật / tắt trạng thái hoạt động (null = giữ nguyên).</summary>
        public bool? IsActive { get; set; }
    }
}
