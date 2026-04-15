using Microsoft.AspNetCore.Http;

namespace CourseMarketplaceBE.Application.DTOs;

public class UpdateProfileRequest
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Bio { get; set; }
    public string? DateOfBirth { get; set; } // Định dạng YYYY-MM-DD
    public string? PhoneNumber { get; set; }
    public IFormFile? AvatarFile { get; set; }
}