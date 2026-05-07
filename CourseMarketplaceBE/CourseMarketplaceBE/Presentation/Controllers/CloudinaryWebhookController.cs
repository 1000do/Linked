using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Presentation.Controllers
{
    [ApiController]
    [Route("api/webhooks/cloudinary")]
    public class CloudinaryWebhookController : ControllerBase
    {
        private readonly ILogger<CloudinaryWebhookController> _logger;

        public CloudinaryWebhookController(ILogger<CloudinaryWebhookController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> HandleNotification()
        {
            try
            {
                using var reader = new System.IO.StreamReader(Request.Body);
                var json = await reader.ReadToEndAsync();
                
                _logger.LogInformation("☁️ Cloudinary Webhook received: {json}", json);

                // Parse json để xử lý các event: upload, delete, eager (processing)
                // Ví dụ: cập nhật trạng thái "Sẵn sàng" cho video sau khi convert xong
                
                return Ok(new { message = "Webhook processed" });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "🔥 Error processing Cloudinary Webhook");
                return StatusCode(500);
            }
        }

        [HttpGet("test-simulation")]
        public IActionResult TestSimulation()
        {
            var mockPayload = new
            {
                notification_type = "upload",
                public_id = "test_video_id",
                secure_url = "https://res.cloudinary.com/demo/video/upload/v1/sample.mp4",
                status = "success"
            };

            _logger.LogInformation("🛠️ Simulating Cloudinary Webhook: {payload}", JsonSerializer.Serialize(mockPayload));
            return Ok(new { message = "Simulation successful", data = mockPayload });
        }
    }
}
