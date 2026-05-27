namespace CourseMarketplaceBE.Application.DTOs
{
    public class InstructorApprovalDto
    {
        public int InstructorId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? AvatarUrl { get; set; } // Thêm Avatar
        public string? ProfessionalTitle { get; set; }
        public string? DocumentUrl { get; set; } // Link CV (PDF)
        public string? LinkedInUrl { get; set; } // Link m?ng xã h?i
        public string? ApprovalStatus { get; set; }
        public string? ExpertiseCategories { get; set; }
    }

    public class UpdateApprovalStatusDto
    {
        public int InstructorId { get; set; }
        public string Status { get; set; } = null!; // Approved ho?c Rejected
    }
}
