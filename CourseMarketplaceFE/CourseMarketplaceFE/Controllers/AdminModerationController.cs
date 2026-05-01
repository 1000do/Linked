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

        public async Task<IActionResult> Courses()
        {
            var response = await _apiClient.GetAsync("/api/admin/moderation/courses/pending");
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
