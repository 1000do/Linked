using System.ComponentModel.DataAnnotations;

namespace CourseMarketplaceBE.Application.DTOs
{
    public class NotificationSendDto
    {
        public int ReceiverId { get; set; }
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Content { get; set; } = string.Empty;
    }
    public class NotificationAdvancedDto
    {
        public string TargetType { get; set; } = "SINGLE"; // "ALL" ho?c "SINGLE"
        public List<string>? Emails { get; set; } // Dùng Email thay vì Username
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = null!;
        [Required]
        public string Content { get; set; } = null!;
        public string? LinkAction { get; set; }
    }

    public class NotificationResponseDto
    {
        public int NotificationId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? LinkAction { get; set; }
        public bool? IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
