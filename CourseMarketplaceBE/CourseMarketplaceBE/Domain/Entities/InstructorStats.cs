using System.ComponentModel.DataAnnotations.Schema;

namespace CourseMarketplaceBE.Domain.Entities;

/// <summary>
/// Keyless entity for mapping view_instructor_stats
/// </summary>
public class InstructorStats
{
    [Column("instructor_id")]
    public int InstructorId { get; set; }

    [Column("instructor_rating")]
    public double InstructorRating { get; set; }

    [Column("total_revenue")]
    public decimal TotalRevenue { get; set; }

    [Column("total_students_count")]
    public int TotalStudentsCount { get; set; }
}
