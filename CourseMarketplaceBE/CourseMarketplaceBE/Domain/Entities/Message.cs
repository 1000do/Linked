using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Message
{
    public int MessageId { get; set; }

    public int ChatId { get; set; }

    public int? SenderId { get; set; }

    public string Content { get; set; } = null!;

    public bool IsSeen { get; set; }

    public string MessageStatus { get; set; } = "ok";

    public DateTime? SentAt { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public virtual Chat Chat { get; set; } = null!;

    public virtual Account? Sender { get; set; }

    public virtual ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
}
