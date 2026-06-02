namespace CourseMarketplaceBE.Application.DTOs
{
    public class NotificationBulkDto
    {
        public int ReceiverId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? LinkAction { get; set; }
    }
}
