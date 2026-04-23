using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CourseMarketplaceBE.Infrastructure.Services;

public class CloudinaryUploadService : IFileUploadService
{
    private readonly Cloudinary _cloudinary;
    private readonly string? _uploadPreset;
    private readonly ILogger<CloudinaryUploadService> _logger;

    public CloudinaryUploadService(
        IConfiguration config,
        ILogger<CloudinaryUploadService> logger)
    {
        _logger = logger;

        var cloudName = config["CloudinarySettings:CloudName"];
        var apiKey = config["CloudinarySettings:ApiKey"];
        var apiSecret = config["CloudinarySettings:ApiSecret"];
        _uploadPreset = config["CloudinarySettings:UploadPreset"];

        // 🔥 Validate config (tránh null crash)
        if (string.IsNullOrWhiteSpace(cloudName) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(apiSecret))
        {
            _logger.LogError("❌ Cloudinary config missing!");
            throw new InvalidOperationException("Cloudinary is not configured properly.");
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string?> UploadImageAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("⚠️ File rỗng hoặc null");
                return null;
            }

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream)
            };

            // ✅ chỉ set nếu có preset
            if (!string.IsNullOrWhiteSpace(_uploadPreset))
            {
                uploadParams.UploadPreset = _uploadPreset;
            }

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result?.Error != null)
            {
                _logger.LogError("❌ Upload lỗi: {msg} | {desc}",
                    result.Error.Message
                   );
                return null;
            }

            _logger.LogInformation("✅ Upload thành công: {url}", result?.SecureUrl);

            return result?.SecureUrl?.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🔥 Exception khi upload ảnh");
            return null;
        }
    }

    public async Task<string?> UploadVideoAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("⚠️ Video rỗng hoặc null");
                return null;
            }

            await using var stream = file.OpenReadStream();

            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream)
            };

            if (!string.IsNullOrWhiteSpace(_uploadPreset))
            {
                uploadParams.UploadPreset = _uploadPreset;
            }

            var result = await _cloudinary.UploadLargeAsync(uploadParams);

            if (result?.Error != null)
            {
                _logger.LogError("❌ Upload video lỗi: {msg} | {desc}",
                    result.Error.Message
                   );
                return null;
            }

            _logger.LogInformation("✅ Upload video thành công: {url}", result?.SecureUrl);

            return result?.SecureUrl?.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🔥 Exception khi upload video");
            return null;
        }
    }
}