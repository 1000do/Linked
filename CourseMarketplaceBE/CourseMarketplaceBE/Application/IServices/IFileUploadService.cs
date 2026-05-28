using CourseMarketplaceBE.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace CourseMarketplaceBE.Application.IServices;

public interface IFileUploadService
{
    Task<string?> UploadImageAsync(IFormFile file);
    Task<string?> UploadVideoAsync(IFormFile file);
    Task<bool> DeleteFileAsync(string fileUrl);
    Task<string?> MoveToTrashAsync(string fileUrl);
    string? GetPublicIdFromUrl(string fileUrl);
    Task<bool> DeleteFileByPublicIdAsync(string publicId, string resourceType);
    Task<string?> UploadFileAsync(IFormFile file);
    Task<string?> RestoreFromTrashAsync(string publicId, string resourceType);
}
