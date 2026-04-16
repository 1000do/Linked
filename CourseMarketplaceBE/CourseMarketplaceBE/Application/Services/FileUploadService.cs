using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public class CloudinaryUploadService : IFileUploadService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryUploadService(IConfiguration config)
    {
        var acc = new CloudinaryDotNet.Account(
            config["CloudinarySettings:CloudName"],
            config["CloudinarySettings:ApiKey"],
            config["CloudinarySettings:ApiSecret"]
        );

        _cloudinary = new Cloudinary(acc);
    }

    public async Task<string?> UploadImageAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            UploadPreset = "ml_default"
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
        {
            Console.WriteLine(result.Error.Message);
            return null;
        }

        return result.SecureUrl.ToString();
    }
}