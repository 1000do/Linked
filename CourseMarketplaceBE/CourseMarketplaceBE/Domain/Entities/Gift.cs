using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Gift
{
    public int GiftId { get; set; }

    public int OrderItemId { get; set; }

    public int? SenderId { get; set; }

    public string RecipientEmail { get; set; } = null!;

    public string? RecipientName { get; set; }

    public string? GiftMessage { get; set; }

    public string? CardTheme { get; set; }

    public string RedemptionToken { get; set; } = null!;

    public bool IsClaimed { get; set; }

    public int? ClaimedByUserId { get; set; }

    public DateTime? ClaimedAt { get; set; }

    public string? DeliveryStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public virtual OrderItem OrderItem { get; set; } = null!;

    public virtual Account? Sender { get; set; }

    public virtual User? ClaimedByUser { get; set; }
}
