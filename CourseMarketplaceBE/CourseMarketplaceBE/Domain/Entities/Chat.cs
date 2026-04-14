using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Chat
{
    public int ChatId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
