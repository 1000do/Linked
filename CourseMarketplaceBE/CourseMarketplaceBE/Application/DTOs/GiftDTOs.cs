using System.ComponentModel.DataAnnotations;

namespace CourseMarketplaceBE.Application.DTOs;

public class GiftPurchaseRequest
{
    [Required(ErrorMessage = "Recipient email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string RecipientEmail { get; set; } = null!;

    public string? RecipientName { get; set; }

    [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
    public string? GiftMessage { get; set; }

    public string CardTheme { get; set; } = "classic"; // classic, birthday, thanks, congrats
}

public class GiftClaimRequest
{
    [Required(ErrorMessage = "Redemption token is required.")]
    public string RedemptionToken { get; set; } = null!;
}

public class GiftValidationResponse
{
    public int GiftId { get; set; }
    public string SenderName { get; set; } = null!;
    public string? RecipientName { get; set; }
    public string? GiftMessage { get; set; }
    public string CardTheme { get; set; } = "classic";
    public string CourseTitle { get; set; } = null!;
    public string? CourseThumbnailUrl { get; set; }
    public bool IsClaimed { get; set; }
}

public class GiftCheckoutRequest
{
    [Required(ErrorMessage = "Course ID is required.")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Recipient email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string RecipientEmail { get; set; } = null!;

    public string? RecipientName { get; set; }

    [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
    public string? GiftMessage { get; set; }

    public string CardTheme { get; set; } = "classic";

    [Required(ErrorMessage = "Success URL is required.")]
    public string SuccessUrl { get; set; } = null!;

    [Required(ErrorMessage = "Cancel URL is required.")]
    public string CancelUrl { get; set; } = null!;
}
