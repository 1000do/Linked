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
    }

    // Class hứng dữ liệu bọc ngoài từ API (status, data)
    public class UserProfileApiResponse
    {
        public int Status { get; set; }
        public UserProfileViewModel Data { get; set; }
    }
}