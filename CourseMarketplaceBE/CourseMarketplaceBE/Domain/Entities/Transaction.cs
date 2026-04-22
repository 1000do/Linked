using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int? OrderItemId { get; set; }

    public int? AccountFrom { get; set; }

    public int? AccountTo { get; set; }

    public decimal Amount { get; set; }

    public decimal TransferRate { get; set; } = 100m;

    public string? StripeSessionId { get; set; }

    public string? StripePaymentintentId { get; set; }

    public string? Currency { get; set; }

    public string? TransactionsStatus { get; set; }

    public string? TransactionType { get; set; }

    public DateTime? TransactionCreatedAt { get; set; }

    public virtual OrderItem? OrderItem { get; set; }

    public virtual Account? AccountFromNavigation { get; set; }

    public virtual Account? AccountToNavigation { get; set; }

    public virtual ICollection<InstructorPayout> InstructorPayouts { get; set; } = new List<InstructorPayout>();
}
