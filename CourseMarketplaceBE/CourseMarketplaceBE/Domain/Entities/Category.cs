using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoriesName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CategoryStatus { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
