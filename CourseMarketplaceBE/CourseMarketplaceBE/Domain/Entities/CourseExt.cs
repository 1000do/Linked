using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class CourseExt
{
    public int CourseId { get; set; }

    public string? TitleHash { get; set; }

    public string? DescriptionHash { get; set; }

    public string? ThumbnailHash { get; set; }

    public virtual Course Course { get; set; } = null!;
}
