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
            ViewBag.Status = status ?? "all";
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

        // ── Danh sách báo cáo chính thức (FE) ───────────────────────────────
        public async Task<IActionResult> Reports()
        {
            var stats = new ReportStatsViewModel();
            var response = await _apiClient.GetAsync("/api/admin/moderation/reports/stats");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("data", out var dataEl))
                    {
                        stats = JsonSerializer.Deserialize<ReportStatsViewModel>(dataEl.ToString(), _jsonOptions) ?? new ReportStatsViewModel();
                    }
                }
                catch { }
            }
            return View(stats);
        }

        // API AJAX lấy thống kê report
        [HttpGet]
        public async Task<IActionResult> GetReportStats()
        {
            var response = await _apiClient.GetAsync("/api/admin/moderation/reports/stats");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            return Json(new { success = false });
        }

        // API AJAX lấy báo cáo khóa học
        [HttpGet]
        public async Task<IActionResult> GetCourseReports(string? status)
        {
            var url = "/api/admin/moderation/reports/courses";
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                url += $"?status={status}";
            }
            var response = await _apiClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            return Json(new { success = false });
        }

        // API AJAX lấy báo cáo đánh giá khóa học
        [HttpGet]
        public async Task<IActionResult> GetCourseReviewReports(string? status)
        {
            var url = "/api/admin/moderation/reports/course-reviews";
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                url += $"?status={status}";
            }
            var response = await _apiClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            return Json(new { success = false });
        }

        // API AJAX lấy báo cáo đánh giá bài học
        [HttpGet]
        public async Task<IActionResult> GetLessonReviewReports(string? status)
        {
            var url = "/api/admin/moderation/reports/lesson-reviews";
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                url += $"?status={status}";
            }
            var response = await _apiClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            return Json(new { success = false });
        }

        // Resolve course report
        [HttpPost]
        public async Task<IActionResult> ResolveCourseReport(int reportId, [FromBody] ResolveReportViewModel model)
        {
            var response = await _apiClient.PatchJsonAsync($"/api/admin/moderation/reports/courses/{reportId}", model);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        // Resolve course review report
        [HttpPost]
        public async Task<IActionResult> ResolveCourseReviewReport(int reportId, [FromBody] ResolveReportViewModel model)
        {
            var response = await _apiClient.PatchJsonAsync($"/api/admin/moderation/reports/course-reviews/{reportId}", model);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        // Resolve lesson review report
        [HttpPost]
        public async Task<IActionResult> ResolveLessonReviewReport(int reportId, [FromBody] ResolveReportViewModel model)
        {
            var response = await _apiClient.PatchJsonAsync($"/api/admin/moderation/reports/lesson-reviews/{reportId}", model);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        // Admin only: remove course
        [HttpPost]
        public async Task<IActionResult> RemoveCourse(int courseId)
        {
            var response = await _apiClient.DeleteAsync($"/api/admin/moderation/courses/{courseId}/remove");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        // AJAX: Lấy báo cáo chat / user
        [HttpGet]
        public async Task<IActionResult> GetUserReports()
        {
            var response = await _apiClient.GetAsync("/api/admin/moderation/reports");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            return Json(new { success = false });
        }

        // AJAX: Xử lý báo cáo chat / user
        [HttpPost]
        public async Task<IActionResult> ResolveUserReport(int reportId, string status, string? resolutionNote)
        {
            var payload = new { reportId, status, resolutionNote };
            var response = await _apiClient.PostJsonAsync("/api/admin/moderation/reports/resolve", payload);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        // Legacy action (giữ lại phòng trường hợp code cũ gọi)
        [HttpPost]
        public async Task<IActionResult> ResolveReport(int reportId, string status, string note)
        {
            var payload = new { reportId, status, resolutionNote = note };
            var response = await _apiClient.PostJsonAsync("/api/admin/moderation/reports/resolve", payload);
            return Json(new { success = response.IsSuccessStatusCode });
        }
    }
}
