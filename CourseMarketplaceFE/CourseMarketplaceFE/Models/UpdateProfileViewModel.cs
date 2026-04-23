using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LinkedLearn.Models.UserVM
{
    public class UpdateProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Họ & Tên đệm")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Tên")]
        public string LastName { get; set; }

        public string? Bio { get; set; }

        public string? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Số điện thoại phải từ 10 đến 11 chữ số và không chứa ký tự đặc biệt.")]
        public string? PhoneNumber { get; set; }

        public IFormFile? AvatarFile { get; set; }
        public string? AvatarUrl { get; set; }
    }
}