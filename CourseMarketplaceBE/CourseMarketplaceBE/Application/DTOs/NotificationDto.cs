namespace CourseMarketplaceBE.Application.DTOs
{
    public class NotificationSendDto
    {
        public int ReceiverId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
    public class NotificationAdvancedDto
    {
        public string TargetType { get; set; } = "SINGLE"; // "ALL" ho?c "SINGLE"
        public List<string>? Emails { get; set; } // Dùng Email thay vì Username
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
    }
}
