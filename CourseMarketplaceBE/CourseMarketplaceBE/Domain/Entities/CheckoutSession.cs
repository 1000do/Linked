using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseMarketplaceBE.Domain.Entities;

[Table("checkout_sessions")]
public class CheckoutSession
{
    [Key]
    [Column("checkout_session_id")]
    [StringLength(50)]
    public string CheckoutSessionId { get; set; } = null!;

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    [Column("total_amount")]
    public decimal TotalAmount { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
