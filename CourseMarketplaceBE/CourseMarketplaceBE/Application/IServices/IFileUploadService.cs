using CourseMarketplaceBE.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace CourseMarketplaceBE.Application.IServices;

public interface IFileUploadService
{
    Task<string?> UploadImageAsync(IFormFile file);
    Task<string?> UploadVideoAsync(IFormFile file);
}