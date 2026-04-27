using Microsoft.AspNetCore.Mvc;
using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using System.Text.Json;

namespace CourseMarketplaceFE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiClient _apiClient;

        public HomeController(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }
        public async Task<IActionResult> Index()
        {
            var response = await _apiClient.GetAsync("public/courses");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var data = json.RootElement.GetProperty("data").ToString();
                var courses = JsonSerializer.Deserialize<List<PublicCourseViewModel>>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(courses);
            }
            return View(new List<PublicCourseViewModel>());
        }
    }
}