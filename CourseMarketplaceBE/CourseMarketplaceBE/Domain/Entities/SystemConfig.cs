using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class SystemConfig
{
    public int ConfigId { get; set; }

    public int? ManagerId { get; set; }

    public string ConfigKey { get; set; } = null!;

    public string? ConfigValue { get; set; }

    public string? Description { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Manager? Manager { get; set; }
}
