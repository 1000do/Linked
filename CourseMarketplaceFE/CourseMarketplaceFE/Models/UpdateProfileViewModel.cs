using Microsoft.AspNetCore.Http;

namespace LinkedLearn.Models.UserVM
{
    public class UpdateProfileViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Bio { get; set; }
        public string? DateOfBirth { get; set; } // Dạng chuỗi để dễ parse
        public string? PhoneNumber { get; set; }
        public IFormFile? AvatarFile { get; set; }
        public string? AvatarUrl { get; set; }


    }
}