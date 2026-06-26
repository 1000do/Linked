using System.ComponentModel.DataAnnotations;

namespace CourseMarketplaceFE.Models
{
    public class GiftSetupViewModel
    {
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Recipient email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [Display(Name = "Recipient's Email")]
        public string RecipientEmail { get; set; } = null!;

        [Display(Name = "Recipient's Name (Optional)")]
        public string? RecipientName { get; set; }

        [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
        [Display(Name = "Gift Message (Optional)")]
        public string? GiftMessage { get; set; }

        [Display(Name = "Card Design Theme")]
        public string CardTheme { get; set; } = "classic"; // classic, birthday, thanks, congrats
    }

    public class GiftValidationViewModel
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
}
