namespace CourseMarketplaceBE.Application.DTOs
{
    public class NotificationAdminResponseDto
    {
        public int NotificationId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Hai tru?ng quan tr?ng manager c?n quan sát
        public string? ReceiverEmail { get; set; }
        public string? ReceiverFullName { get; set; }
    }
}
