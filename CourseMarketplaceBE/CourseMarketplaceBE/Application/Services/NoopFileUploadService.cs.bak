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

    public Task<bool> DeleteFileAsync(string fileUrl)
    {
        return Task.FromResult(true);
    }

    public Task<string?> MoveToTrashAsync(string fileUrl)
    {
        return Task.FromResult<string?>(null);
    }

    public string? GetPublicIdFromUrl(string fileUrl)
    {
        return null;
    }

    public Task<bool> DeleteFileByPublicIdAsync(string publicId, string resourceType)
    {
        return Task.FromResult(true);
    }

    public Task<string?> UploadFileAsync(IFormFile file)
    {
        return Task.FromResult<string?>(null);
    }

    public Task<string?> RestoreFromTrashAsync(string publicId, string resourceType)
    {
        return Task.FromResult<string?>(null);
    }
}
