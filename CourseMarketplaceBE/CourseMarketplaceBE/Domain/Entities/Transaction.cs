using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int? OrderId { get; set; }

    public decimal Amount { get; set; }

    public string? StripeSessionId { get; set; }

    public string? StripePaymentintentId { get; set; }

    public string? Currency { get; set; }

    public string? TransactionsStatus { get; set; }

    public string? TransactionType { get; set; }

    public DateTime? TransactionCreatedAt { get; set; }

    public virtual OrderInfo? Order { get; set; }
}
