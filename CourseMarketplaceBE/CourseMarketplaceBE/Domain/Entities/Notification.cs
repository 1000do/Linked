using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int? SenderId { get; set; }

    public int? ReceiverId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public string? LinkAction { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account? Receiver { get; set; }

    public virtual Account? Sender { get; set; }
}
