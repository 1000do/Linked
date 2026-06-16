using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace CourseMarketplaceBE.Application.DTOs
{
    public class ManagerProfileResponse
    {
        public int ManagerId { get; set; }
        public string? Role { get; set; }
        public string DisplayName { get; set; } = null!;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public DateTime? AccountCreatedAt { get; set; }
        public DateTime? LockoutStart { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string Email { get; set; } = null!;
    }

    public class UpdateManagerProfileRequest
    {
        [Required(ErrorMessage = "Display name is required.")]
        [MaxLength(255, ErrorMessage = "Display name cannot exceed 255 characters.")]
        public string DisplayName { get; set; } = null!;

        [MaxLength(255, ErrorMessage = "Full name cannot exceed 255 characters.")]
        public string? FullName { get; set; }

        [MaxLength(50, ErrorMessage = "Phone number cannot exceed 50 characters.")]
        [RegularExpression(@"^\+?[0-9\s-]{9,15}$", ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }

        [MaxLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters.")]
        public string? Bio { get; set; }

        public IFormFile? AvatarFile { get; set; }
    }
}
