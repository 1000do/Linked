using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Message
{
    public int MessageId { get; set; }

    public int? ChatId { get; set; }

    public int? SenderId { get; set; }

    public int? ReceiverId { get; set; }

    public string Content { get; set; } = null!;

    public bool? IsSeen { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public virtual Chat? Chat { get; set; }

    public virtual Account? Receiver { get; set; }

    public virtual Account? Sender { get; set; }
}
