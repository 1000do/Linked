using System.ComponentModel.DataAnnotations;

namespace LinkedLearn.Models.UserVM
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Chỉ chấp nhận địa chỉ @gmail.com")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
        public string ConfirmPassword { get; set; } = null!;
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Gmail")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Tài khoản phải là @gmail.com")]
        public string Identifier { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; } = null!;
    }

    public class LoginResponse
    {
        public int Status { get; set; }
        public string? Token { get; set; }
        public string? Message { get; set; }
        public string? FullName { get; set; } // Thêm trường này
        public string AvatarUrl { get; set; }
    }   

    public class ApiResponse
    {
        public string? Message { get; set; } = null!;
        public int Status { get; set; }
    }
}