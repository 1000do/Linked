namespace CourseMarketplaceFE.Models
{
    public class InstructorApprovalViewModel
    {
        public int InstructorId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!; // Thêm dòng này
        public string? ProfessionalTitle { get; set; }
        public string? DocumentUrl { get; set; }
        public string? ApprovalStatus { get; set; }
        public string? ExpertiseCategories { get; set; }
    }
}
