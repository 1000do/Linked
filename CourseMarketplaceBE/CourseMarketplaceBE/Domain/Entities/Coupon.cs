using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Coupon
{
    public int CouponId { get; set; }

    public int? ManagerId { get; set; }

    public string CouponCode { get; set; } = null!;

    public string? CouponType { get; set; }

    public decimal DiscountValue { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? UsageLimit { get; set; }

    public int? UsedCount { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual Manager? Manager { get; set; }
}
