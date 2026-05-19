using System.ComponentModel.DataAnnotations;

namespace CourseMarketplaceBE.Application.DTOs;

public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Please enter the current password.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the new password.")]
    [MinLength(6, ErrorMessage = "New password must be at least 6 characters long.")]
    public string NewPassword { get; set; } = string.Empty;
}
