using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Chat
{
    public int ChatId { get; set; }

    public string? ChatName { get; set; }

    public string ChatType { get; set; } = "private";

    public string? ContextType { get; set; }

    public int? ContextId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public virtual ICollection<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<UserReport> UserReports { get; set; } = new List<UserReport>();
}
