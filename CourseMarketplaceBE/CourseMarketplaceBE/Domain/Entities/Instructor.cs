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

    // instructor_rating & total_revenue đã bị xóa khỏi bảng instructors trong SQL v2
    // Dùng VIEW view_instructor_stats để lấy các giá trị này (tính động qua SQL)

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<InstructorPayout> InstructorPayouts { get; set; } = new List<InstructorPayout>();

    public virtual User InstructorNavigation { get; set; } = null!;
}
