using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class TransactionExt
{
    public int TransactionId { get; set; }
    
    public string? RefundReason { get; set; }
    
    public string? RefundAdminNote { get; set; }
    
    public DateTime? RefundRequestedAt { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}
