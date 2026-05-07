using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace CourseMarketplaceFE.Controllers
{
    public class AdminModerationController : Controller
    {
        private readonly ApiClient _apiClient;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public AdminModerationController(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Courses(string? search, string? category, string? status, string? sortBy)
        {
            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Status = status ?? "pending";
            ViewBag.SortBy = sortBy ?? "oldest";

            // Fetch categories
            var catRes = await _apiClient.GetAsync("/api/public/categories");
            if (catRes.IsSuccessStatusCode)
            {
                var catContent = await catRes.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(catContent);
                ViewBag.Categories = json.RootElement.GetProperty("data").ToString();
            }

            var url = $"/api/admin/moderation/courses/pending?search={search}&category={category}&status={status}&sortBy={sortBy}";
            var response = await _apiClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var courses = JsonSerializer.Deserialize<List<CourseModerationViewModel>>(content, _jsonOptions);
                return View(courses);
            }
            return View(new List<CourseModerationViewModel>());
        }

        [HttpPost]
        public async Task<IActionResult> ApproveCourse(int id, string? feedback)
        {
            var response = await _apiClient.PostJsonAsync($"/api/admin/moderation/courses/approve/{id}", feedback);
            return Json(new { success = response.IsSuccessStatusCode });
        }

        [HttpPost]
        public async Task<IActionResult> RejectCourse(int id, string reason)
        {
            var response = await _apiClient.PostJsonAsync($"/api/admin/moderation/courses/reject/{id}", reason);
            return Json(new { success = response.IsSuccessStatusCode });
        }

        [HttpPost]
        public async Task<IActionResult> FlagCourse(int id, string reason)
        {
            var response = await _apiClient.PostJsonAsync($"/api/admin/moderation/courses/flag/{id}", reason);
            return Json(new { success = response.IsSuccessStatusCode });
        }

        [HttpPost]
        public async Task<IActionResult> RejectCourseDetailed([FromBody] System.Text.Json.JsonElement body)
        {
            var response = await _apiClient.PostJsonAsync("/api/admin/moderation/courses/reject-detailed", body);
            return Json(new { success = response.IsSuccessStatusCode });
        }

        [HttpGet]
        public async Task<IActionResult> GetCourseDetails(int id)
        {
            var response = await _apiClient.GetAsync($"public/courses/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            return Json(new { success = false });
        }

        public async Task<IActionResult> Reports()
        {
            var response = await _apiClient.GetAsync("/api/admin/moderation/reports");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var reports = JsonSerializer.Deserialize<List<UserReportModerationViewModel>>(content, _jsonOptions);
                return View(reports);
            }
            return View(new List<UserReportModerationViewModel>());
        }

        [HttpPost]
        public async Task<IActionResult> ResolveReport(int reportId, string status, string note)
        {
            var payload = new { reportId, status, resolutionNote = note };
            var response = await _apiClient.PostJsonAsync("/api/admin/moderation/reports/resolve", payload);
            return Json(new { success = response.IsSuccessStatusCode });
        }
    }
}
