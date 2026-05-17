using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class OrderItem
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? CourseId { get; set; }

    public decimal PurchasePrice { get; set; }

    public bool? CouponUsed { get; set; }

    // ★ Snapshot giá gốc & coupon tại thời điểm mua
    [System.ComponentModel.DataAnnotations.Schema.Column("original_price")]
    public decimal? OriginalPrice { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.Column("coupon_code")]
    public string? CouponCode { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.Column("coupon_type")]
    public string? CouponType { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.Column("discount_amount")]
    public decimal DiscountAmount { get; set; }

    public virtual Course? Course { get; set; }

    public virtual OrderInfo? Order { get; set; }

    /// <summary>Mỗi order_item có thể có nhiều transactions (payment, refund...)</summary>
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
