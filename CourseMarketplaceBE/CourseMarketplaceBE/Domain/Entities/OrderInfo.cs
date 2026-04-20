using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class OrderInfo
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? OrderStatus { get; set; }

    public string? PaymentMethod { get; set; }

    // TotalAmount & DiscountAmount đã bị xóa trong SQL v2
    // Dùng VIEW view_order_stats để tính: SUM(order_items.purchase_price)

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User? User { get; set; }
}
