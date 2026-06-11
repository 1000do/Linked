using Microsoft.AspNetCore.Http;
using System;

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
        public string DisplayName { get; set; } = null!;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Bio { get; set; }
        public IFormFile? AvatarFile { get; set; }
    }
}
