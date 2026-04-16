using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface IFileUploadService
{
    Task<string?> UploadImageAsync(IFormFile file);
}