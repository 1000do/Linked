using System.ComponentModel.DataAnnotations;

namespace LinkedLearn.Models.UserVM
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter your full name")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Please enter your Email")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Only @gmail.com addresses are accepted")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Please enter your username")]
        [RegularExpression(@"^[a-zA-Z0-9_]{3,30}$", ErrorMessage = "Username must be 3-30 characters, alphanumeric or underscores only")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Please enter your password")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = null!;
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your Username or Email")]
        public string UsernameOrEmail { get; set; } = null!;

        [Required(ErrorMessage = "Please enter your password")]
        public string Password { get; set; } = null!;
    }

    public class LoginResponse
    {
        public int Status { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int AccountId { get; set; }
        public string? Message { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        /// <summary>"user" | "manager" — dùng để redirect sau login</summary>
        public string? Role { get; set; }
        public bool IsVerified { get; set; }
    }

    public class ApiResponse
    {
        public string? Message { get; set; } = null!;
        public int Status { get; set; }
    }
}