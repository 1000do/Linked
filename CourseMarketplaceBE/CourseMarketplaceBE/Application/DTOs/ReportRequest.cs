namespace CourseMarketplaceBE.Application.DTOs
{
    public class ReportRequest
    {
        public int ReviewId { get; set; }
        public string Type { get; set; } = null!; // "course" or "lesson"
        public string Reason { get; set; } = null!;
    }
}
