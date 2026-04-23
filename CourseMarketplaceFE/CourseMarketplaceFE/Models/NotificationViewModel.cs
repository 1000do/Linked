namespace CourseMarketplaceFE.Models
{
    public class NotificationViewModel
    {
        public int NotificationId { get; set; }
        public int ReceiverId { get; set; }
        public string? ReceiverFullName { get; set; }
        public string? ReceiverEmail { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? LinkAction { get; set; }
        public bool IsRead { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
