using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using CourseMarketplaceFE.Models.Common;

namespace CourseMarketplaceFE.Controllers
{
    [Authorize(Roles = "admin,staff")]
    public class AdminModerationController : Controller
    {
        private readonly ApiClient _apiClient;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public AdminModerationController(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Courses(string? search, string? category, string? status, string? sortBy, int page = 1, int pageSize = 10)
        {
            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Status = status ?? "all";
            ViewBag.SortBy = sortBy ?? "threat_desc";
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            // Fetch categories
            var catRes = await _apiClient.GetAsync("public/courses/categories");
            if (catRes.IsSuccessStatusCode)
            {
                var catContent = await catRes.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(catContent);
                ViewBag.Categories = json.RootElement.GetProperty("data").ToString();
            }

            var url = $"/api/admin/moderation/courses/pending?search={search}&category={category}&status={status}&sortBy={sortBy}&page={page}&pageSize={pageSize}";
            var response = await _apiClient.GetAsync(url);
            
            var statsUrl = "/api/admin/moderation/courses/stats";
            var statsRes = await _apiClient.GetAsync(statsUrl);
            if (statsRes.IsSuccessStatusCode)
            {
                var statsContent = await statsRes.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(statsContent);
                var data = json.RootElement.GetProperty("data");
                ViewBag.PendingCount = data.GetProperty("pendingCount").GetInt32();
                ViewBag.ResolvedTodayCount = data.GetProperty("resolvedTodayCount").GetInt32();
            }
            else 
            {
                ViewBag.PendingCount = 0;
                ViewBag.ResolvedTodayCount = 0;
            }
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var pagedResult = JsonSerializer.Deserialize<PagedResult<CourseModerationViewModel>>(content, _jsonOptions);
                return View(pagedResult ?? new PagedResult<CourseModerationViewModel>());
            }
            return View(new PagedResult<CourseModerationViewModel>());
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
        public async Task<IActionResult> UnflagCourse(int id)
        {
            var response = await _apiClient.PostJsonAsync($"/api/admin/moderation/courses/unflag/{id}", new { });
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
        public async Task<IActionResult> Reports(int coursePage = 1, int cReviewPage = 1, int lReviewPage = 1)
        {
            var model = new ReportModerationPageViewModel();

            try
            {
                var statsTask = _apiClient.GetAsync("/api/admin/moderation/reports/stats");
                var coursesTask = _apiClient.GetAsync($"/api/admin/moderation/reports/courses?page={coursePage}&pageSize=10");
                var cReviewsTask = _apiClient.GetAsync($"/api/admin/moderation/reports/course-reviews?page={cReviewPage}&pageSize=10");
                var lReviewsTask = _apiClient.GetAsync($"/api/admin/moderation/reports/lesson-reviews?page={lReviewPage}&pageSize=10");

                await Task.WhenAll(statsTask, coursesTask, cReviewsTask, lReviewsTask);

                var statsResp = await statsTask;
                if (statsResp.IsSuccessStatusCode)
                {
                    var json = await statsResp.Content.ReadAsStringAsync();
                    var parsed = JsonSerializer.Deserialize<ApiResp<ReportStatsViewModel>>(json, _jsonOptions);
                    if (parsed?.Data != null) model.Stats = parsed.Data;
                }

                var coursesResp = await coursesTask;
                if (coursesResp.IsSuccessStatusCode)
                {
                    var json = await coursesResp.Content.ReadAsStringAsync();
                    var parsed = JsonSerializer.Deserialize<BaseApiResponse<PagedResult<CourseReportDetailViewModel>>>(json, _jsonOptions);
                    if (parsed?.Data != null) model.CourseReports = parsed.Data;
                }

                var cReviewsResp = await cReviewsTask;
                if (cReviewsResp.IsSuccessStatusCode)
                {
                    var json = await cReviewsResp.Content.ReadAsStringAsync();
                    var parsed = JsonSerializer.Deserialize<BaseApiResponse<PagedResult<ReviewReportDetailViewModel>>>(json, _jsonOptions);
                    if (parsed?.Data != null) model.CourseReviewReports = parsed.Data;
                }

                var lReviewsResp = await lReviewsTask;
                if (lReviewsResp.IsSuccessStatusCode)
                {
                    var json = await lReviewsResp.Content.ReadAsStringAsync();
                    var parsed = JsonSerializer.Deserialize<BaseApiResponse<PagedResult<ReviewReportDetailViewModel>>>(json, _jsonOptions);
                    if (parsed?.Data != null) model.LessonReviewReports = parsed.Data;
                }
            }
            catch { }

            return View(model);
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
        public async Task<IActionResult> GetCourseReports(string? status, int page = 1)
        {
            var url = $"/api/admin/moderation/reports/courses?page={page}&pageSize=10";
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                url += $"&status={status}";
            }
            var response = await _apiClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var parsed = JsonSerializer.Deserialize<BaseApiResponse<PagedResult<CourseReportDetailViewModel>>>(content, _jsonOptions);
                return PartialView("_CourseReportsPartial", parsed?.Data ?? new PagedResult<CourseReportDetailViewModel>());
            }
            return PartialView("_CourseReportsPartial", new PagedResult<CourseReportDetailViewModel>());
        }

        // API AJAX lấy báo cáo đánh giá khóa học
        [HttpGet]
        public async Task<IActionResult> GetCourseReviewReports(string? status, int page = 1)
        {
            var url = $"/api/admin/moderation/reports/course-reviews?page={page}&pageSize=10";
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                url += $"&status={status}";
            }
            var response = await _apiClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var parsed = JsonSerializer.Deserialize<BaseApiResponse<PagedResult<ReviewReportDetailViewModel>>>(content, _jsonOptions);
                return PartialView("_ReviewReportsPartial", parsed?.Data ?? new PagedResult<ReviewReportDetailViewModel>());
            }
            return PartialView("_ReviewReportsPartial", new PagedResult<ReviewReportDetailViewModel>());
        }

        // API AJAX lấy báo cáo đánh giá bài học
        [HttpGet]
        public async Task<IActionResult> GetLessonReviewReports(string? status, int page = 1)
        {
            var url = $"/api/admin/moderation/reports/lesson-reviews?page={page}&pageSize=10";
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                url += $"&status={status}";
            }
            var response = await _apiClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var parsed = JsonSerializer.Deserialize<BaseApiResponse<PagedResult<ReviewReportDetailViewModel>>>(content, _jsonOptions);
                return PartialView("_ReviewReportsPartial", parsed?.Data ?? new PagedResult<ReviewReportDetailViewModel>());
            }
            return PartialView("_ReviewReportsPartial", new PagedResult<ReviewReportDetailViewModel>());
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
