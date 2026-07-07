using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatAttachmentController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;
    private readonly IConfiguration _configuration;

    public ChatAttachmentController(IFileUploadService fileUploadService, IConfiguration configuration)
    {
        _fileUploadService = fileUploadService;
        _configuration = configuration;
    }

    [HttpPost("upload")]
    [Authorize(Roles = "user,instructor,admin,staff")]
    public async Task<IActionResult> UploadAttachment(IFormFile file)
    {
        if (!_configuration.GetValue<bool>("ChatSettings:EnableAttachments"))
            return BadRequest(new { message = "File attachment feature is disabled." });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Please choose a file." });

        // Giới hạn 10MB
        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { message = "File size must not exceed 10MB." });

        string? url = null;
        var contentType = file.ContentType.ToLower();

        if (contentType.Contains("image"))
            url = await _fileUploadService.UploadImageAsync(file);
        else if (contentType.Contains("video"))
            url = await _fileUploadService.UploadVideoAsync(file);
        else
            url = await _fileUploadService.UploadFileAsync(file);

        if (string.IsNullOrEmpty(url))
            return StatusCode(500, new { message = "Error uploading file to Cloudinary." });

        return Ok(new
        {
            FileUrl = url,
            FileName = file.FileName,
            FileType = contentType.Contains("image") ? "image" : (contentType.Contains("video") ? "video" : "document"),
            FileSize = file.Length
        });
    }
}
