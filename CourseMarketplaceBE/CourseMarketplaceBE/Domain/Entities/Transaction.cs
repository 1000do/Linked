using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Transaction
{
    public int TransactionId { get; set; }

    /// <summary>FK sang order_items (mỗi transaction ứng với 1 item, không phải cả order)</summary>
    public int? OrderItemId { get; set; }

    /// <summary>Tài khoản chuyển tiền (người mua)</summary>
    public int? AccountFrom { get; set; }

    /// <summary>Tài khoản nhận tiền (instructor hoặc sàn)</summary>
    public int? AccountTo { get; set; }

    public decimal Amount { get; set; }

    /// <summary>Phần trăm instructor nhận được (mặc định 100%)</summary>
    public decimal TransferRate { get; set; }

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
