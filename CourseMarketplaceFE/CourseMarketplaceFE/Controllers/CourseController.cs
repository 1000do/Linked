using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CourseMarketplaceFE.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApiClient _apiClient;

        public CourseController(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Details(int id)
        {
            var response = await _apiClient.GetAsync($"public/courses/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var data = json.RootElement.GetProperty("data").ToString();
                var course = JsonSerializer.Deserialize<CourseDetailViewModel>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(course);
            }
            return NotFound();
        }

        public async Task<IActionResult> Learn(int id)
        {
            var response = await _apiClient.GetAsync($"public/courses/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var data = json.RootElement.GetProperty("data").ToString();
                var course = JsonSerializer.Deserialize<CourseDetailViewModel>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(course);
            }
            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> DownloadMaterial(string url, string fileName)
        {
            if (string.IsNullOrEmpty(url)) return BadRequest("URL is missing.");
            
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Could not fetch the file from the remote server.");
                }

                var stream = await response.Content.ReadAsStreamAsync();
                var extension = Path.GetExtension(new Uri(url).AbsolutePath);
                if (string.IsNullOrEmpty(extension)) extension = ".pdf";
                
                var finalName = string.IsNullOrEmpty(fileName) ? "document" : fileName;
                if (!finalName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    finalName += extension;
                }

                return File(stream, "application/octet-stream", finalName);
            }
            catch (Exception)
            {
                return BadRequest("An error occurred while downloading the file.");
            }
        }
    }
}
