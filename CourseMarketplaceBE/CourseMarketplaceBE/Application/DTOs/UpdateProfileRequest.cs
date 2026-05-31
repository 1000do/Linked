using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CourseMarketplaceBE.Application.DTOs;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "First & Middle Name is required.")]
    [StringLength(50, ErrorMessage = "First & Middle Name cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\p{L}]+$", ErrorMessage = "First & Middle Name can only contain letters and spaces.")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last Name is required.")]
    [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\p{L}]+$", ErrorMessage = "Last Name can only contain letters and spaces.")]
    public string LastName { get; set; } = null!;

    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Only @gmail.com email addresses are accepted.")]
    public string? Email { get; set; }

    [StringLength(500, ErrorMessage = "Bio description cannot exceed 500 characters.")]
    public string? Bio { get; set; }

    public string? DateOfBirth { get; set; } // YYYY-MM-DD

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Phone number must be exactly 10 or 11 digits.")]
    public string? PhoneNumber { get; set; }

    public IFormFile? AvatarFile { get; set; }

    // Instructor specific fields
    public string? ProfessionalTitle { get; set; }
    public string? ExpertiseCategories { get; set; }
    public string? LinkedinUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? FacebookUrl { get; set; }
}
