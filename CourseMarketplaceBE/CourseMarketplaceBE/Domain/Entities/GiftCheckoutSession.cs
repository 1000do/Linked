using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseMarketplaceBE.Domain.Entities;

[Table("gift_checkout_sessions")]
public class GiftCheckoutSession
{
    [Key]
    [Column("gift_session_id")]
    [StringLength(50)]
    public string GiftCheckoutSessionId { get; set; } = null!;

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("recipient_email")]
    public string RecipientEmail { get; set; } = null!;

    [Column("recipient_name")]
    public string? RecipientName { get; set; }

    [Column("gift_message")]
    public string? GiftMessage { get; set; }

    [Column("card_theme")]
    public string CardTheme { get; set; } = "classic";

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

    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;
}
