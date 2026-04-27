using System.ComponentModel.DataAnnotations.Schema;

namespace CourseMarketplaceBE.Domain.Entities;

/// <summary>
/// Keyless entity for mapping view_course_stats
/// </summary>
public class CourseStats
{
    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("rating_average")]
    public double RatingAverage { get; set; }

    [Column("total_students")]
    public int TotalStudents { get; set; }

    [Column("total_reviews")]
    public int TotalReviews { get; set; }

    [Column("total_lessons")]
    public int TotalLessons { get; set; }

    [Column("total_materials")]
    public int TotalMaterials { get; set; }

    [Column("total_duration")]
    public int TotalDuration { get; set; }
}
