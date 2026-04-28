namespace LinkedLearn.Models.UserVM
{
    public class UserProfileViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Bio { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? AvatarUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsVerified { get; set; }

        // ── Thông tin Giảng viên ────────────────────────────
        public bool IsInstructor { get; set; }
        public string? ProfessionalTitle { get; set; }
        public string? ExpertiseCategories { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? ApprovalStatus { get; set; }
    }

    // Class hứng dữ liệu bọc ngoài từ API (status, data)
    public class UserProfileApiResponse
    {
        public int Status { get; set; }
        public UserProfileViewModel Data { get; set; }
    }
}