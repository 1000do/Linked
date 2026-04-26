namespace CourseMarketplaceBE.Application.DTOs
{
    public class InstructorApprovalDto
    {
        public int InstructorId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!; // Thêm Email
        public string? ProfessionalTitle { get; set; }
        public string? DocumentUrl { get; set; }
        public string? ApprovalStatus { get; set; } // Pending, Approved, Rejected
        public string? ExpertiseCategories { get; set; }
    }

    public class UpdateApprovalStatusDto
    {
        public int InstructorId { get; set; }
        public string Status { get; set; } = null!; // Approved hoặc Rejected
    }
}
