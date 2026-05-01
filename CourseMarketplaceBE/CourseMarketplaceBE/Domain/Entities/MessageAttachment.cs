using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class MessageAttachment
{
    public int AttachmentId { get; set; }

    public int? MessageId { get; set; }

    public string FileUrl { get; set; } = null!;

    public string? FileName { get; set; }

    public string? FileType { get; set; }

    public long? FileSize { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Message? Message { get; set; }
}
