using CourseMarketplaceBE.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace CourseMarketplaceBE.Application.IServices;

public class NoopFileUploadService : IFileUploadService
{
    // Safe fallback when Cloudinary is not configured.
    // Returns null to indicate no remote URL was produced.
    public Task<string?> UploadImageAsync(IFormFile file)
    {
        // Optionally log here.
        return Task.FromResult<string?>(null);
    }

    public Task<string?> UploadVideoAsync(IFormFile file)
    {
        return Task.FromResult<string?>(null);
    }
}