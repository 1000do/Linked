using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class ChatParticipant
{
    public int ChatId { get; set; }

    public int AccountId { get; set; }

    public string Role { get; set; } = "member";

    public DateTime? JoinedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Chat Chat { get; set; } = null!;
}
