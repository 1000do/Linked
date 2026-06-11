using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Manager
{
    public int ManagerId { get; set; }

    public string? Role { get; set; }

    public string DisplayName { get; set; } = null!;

    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();

    public virtual Account ManagerNavigation { get; set; } = null!;

    public virtual ICollection<SystemConfig> SystemConfigs { get; set; } = new List<SystemConfig>();
}
