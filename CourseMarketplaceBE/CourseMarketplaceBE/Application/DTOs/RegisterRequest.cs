namespace CourseMarketplaceBE.Application.DTOs;

    public class RegisterRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
        public string FullName { get; set; } = null!;
    }