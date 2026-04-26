using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class LessonExt
{
    public int LessonId { get; set; }

    public string? TitleHash { get; set; }

    public string? DescriptionHash { get; set; }

    public string? ThumbnailHash { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;
}
