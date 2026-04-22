using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Instructor
{
    public int InstructorId { get; set; }

    public string? StripeAccountId { get; set; }

    public string? StripeOnboardingStatus { get; set; }

    public bool? PayoutsEnabled { get; set; }

    public bool? ChargesEnabled { get; set; }

    //public float? InstructorRating { get; set; }

    //public decimal? TotalRevenue { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<InstructorPayout> InstructorPayouts { get; set; } = new List<InstructorPayout>();

    public virtual User InstructorNavigation { get; set; } = null!;
}
