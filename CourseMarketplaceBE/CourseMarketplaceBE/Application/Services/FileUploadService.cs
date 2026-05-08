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
    private readonly string? _notificationUrl;
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
        _notificationUrl = config["CloudinarySettings:NotificationUrl"];

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
                _logger.LogError("❌ Upload lỗi: {msg}", result.Error.Message);
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

            if (!string.IsNullOrWhiteSpace(_notificationUrl))
            {
                uploadParams.NotificationUrl = _notificationUrl;
            }

            var result = await _cloudinary.UploadLargeAsync(uploadParams);

            if (result?.Error != null)
            {
                _logger.LogError("❌ Upload video lỗi: {msg}", result.Error.Message);
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

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var publicId = GetPublicIdFromUrl(fileUrl);
            if (publicId == null) return false;

            var resourceType = fileUrl.Contains("/video/") ? ResourceType.Video : ResourceType.Image;
            var deletionParams = new DeletionParams(publicId) { ResourceType = resourceType };

            var result = await _cloudinary.DestroyAsync(deletionParams);
            return result.Result == "ok";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🔥 Lỗi khi xóa file trên Cloudinary: {url}", fileUrl);
            return false;
        }
    }

    public async Task<string?> MoveToTrashAsync(string fileUrl)
    {
        try
        {
            var publicId = GetPublicIdFromUrl(fileUrl);
            if (publicId == null) return null;

            var resourceType = fileUrl.Contains("/video/") ? ResourceType.Video : ResourceType.Image;
            
            var newPublicId = $"trash/{publicId}";
            var renameParams = new RenameParams(publicId, newPublicId)
            {
                ResourceType = resourceType,
                Overwrite = true
            };

            var result = await _cloudinary.RenameAsync(renameParams);
            
            if (result.Error != null)
            {
                _logger.LogError("❌ Rename to trash lỗi: {msg}", result.Error.Message);
                return null;
            }

            return result.SecureUrl?.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🔥 Exception khi move file tới trash: {url}", fileUrl);
            return null;
        }
    }

    public string? GetPublicIdFromUrl(string fileUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return null;

            var uri = new Uri(fileUrl);
            var path = uri.AbsolutePath;
            var segments = path.Split('/');

            int uploadIndex = Array.IndexOf(segments, "upload");
            if (uploadIndex == -1) return null;

            var publicIdSegments = segments.Skip(uploadIndex + 2).ToList();
            var lastSegment = publicIdSegments.Last();
            var dotIndex = lastSegment.LastIndexOf('.');
            if (dotIndex != -1)
            {
                publicIdSegments[publicIdSegments.Count - 1] = lastSegment.Substring(0, dotIndex);
            }

            return string.Join("/", publicIdSegments);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteFileByPublicIdAsync(string publicId, string resourceType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(publicId)) return false;

            var resType = resourceType.ToLower() == "video" ? ResourceType.Video : ResourceType.Image;
            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = resType
            };

            var result = await _cloudinary.DestroyAsync(deletionParams);
            return result.Result == "ok";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🔥 Lỗi khi xóa file bằng publicId: {id}", publicId);
            return false;
        }
    }

    public async Task<string?> UploadFileAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0) return null;
            await using var stream = file.OpenReadStream();
            var uploadParams = new CloudinaryDotNet.Actions.RawUploadParams { File = new CloudinaryDotNet.FileDescription(file.FileName, stream) };
            var result = await _cloudinary.UploadAsync(uploadParams);
            return result?.SecureUrl?.ToString();
        }
        catch { return null; }
    }
    public async Task<string?> RestoreFromTrashAsync(string publicId, string resourceType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(publicId)) return null;

            var resType = resourceType.ToLower() == "video" ? ResourceType.Video : ResourceType.Image;
            
            // The file is currently at trash/{publicId}
            var currentPublicId = $"trash/{publicId}";
            var newPublicId = publicId;

            var renameParams = new RenameParams(currentPublicId, newPublicId)
            {
                ResourceType = resType,
                Overwrite = true
            };

            var result = await _cloudinary.RenameAsync(renameParams);
            
            if (result.Error != null)
            {
                _logger.LogError("❌ Restore from trash lỗi: {msg}", result.Error.Message);
                return null;
            }

            return result.SecureUrl?.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🔥 Exception khi restore file từ trash: {id}", publicId);
            return null;
        }
    }
}

