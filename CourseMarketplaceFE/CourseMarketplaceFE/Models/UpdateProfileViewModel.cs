using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LinkedLearn.Models.UserVM
{
    public class UpdateProfileViewModel
    {
        [Required(ErrorMessage = "Please enter First & Middle Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter Last Name")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Only @gmail.com addresses are accepted")]
        public string? Email { get; set; }

        public string? Username { get; set; }

        public string? AuthProvider { get; set; }

        public string? Bio { get; set; }

        public string? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Phone number must be 10-11 digits and contain no special characters.")]
        public string? PhoneNumber { get; set; }

        public IFormFile? AvatarFile { get; set; }
        public string? AvatarUrl { get; set; }

        // Instructor specific fields
        public bool IsInstructor { get; set; }
        public string? ProfessionalTitle { get; set; }
        public string? ExpertiseCategories { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? YoutubeUrl { get; set; }
        public string? FacebookUrl { get; set; }
    }
}