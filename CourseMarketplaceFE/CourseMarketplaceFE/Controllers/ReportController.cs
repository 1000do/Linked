using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace CourseMarketplaceFE.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApiClient _apiClient;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public ReportController(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // ── Gửi báo cáo khóa học ──────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ReportCourse([FromBody] CreateCourseReportViewModel model)
        {
            if (model == null || model.CourseId <= 0 || string.IsNullOrWhiteSpace(model.Reason))
            {
                return Json(new { success = false, message = "Please select a reason for your report." });
            }

            var response = await _apiClient.PostJsonAsync("/api/report/courses", model);
            var content = await response.Content.ReadAsStringAsync();
            
            string? message = null;
            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("message", out var msgEl))
                {
                    message = msgEl.GetString();
                }
            }
            catch { }

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = message ?? "Your report has been submitted and will be reviewed by our moderation team." });
            }

            return Json(new { success = false, message = message ?? "An unexpected error occurred. Please try again later." });
        }

        // ── Gửi báo cáo đánh giá khóa học ───────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ReportCourseReview([FromBody] CreateReviewReportViewModel model)
        {
            if (model == null || model.ReviewId <= 0 || string.IsNullOrWhiteSpace(model.Reason))
            {
                return Json(new { success = false, message = "Please select a reason for your report." });
            }

            var payload = new
            {
                CourseReviewId = model.ReviewId,
                Reason = model.Reason,
                Description = model.Description
            };

            var response = await _apiClient.PostJsonAsync("/api/report/course-reviews", payload);
            var content = await response.Content.ReadAsStringAsync();

            string? message = null;
            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("message", out var msgEl))
                {
                    message = msgEl.GetString();
                }
            }
            catch { }

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = message ?? "Your report has been submitted and will be reviewed by our moderation team." });
            }

            return Json(new { success = false, message = message ?? "An unexpected error occurred. Please try again later." });
        }

        // ── Gửi báo cáo đánh giá bài học ────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ReportLessonReview([FromBody] CreateReviewReportViewModel model)
        {
            if (model == null || model.ReviewId <= 0 || string.IsNullOrWhiteSpace(model.Reason))
            {
                return Json(new { success = false, message = "Please select a reason for your report." });
            }

            var payload = new
            {
                LessonReviewId = model.ReviewId,
                Reason = model.Reason,
                Description = model.Description
            };

            var response = await _apiClient.PostJsonAsync("/api/report/lesson-reviews", payload);
            var content = await response.Content.ReadAsStringAsync();

            string? message = null;
            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("message", out var msgEl))
                {
                    message = msgEl.GetString();
                }
            }
            catch { }

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = message ?? "Your report has been submitted and will be reviewed by our moderation team." });
            }

            return Json(new { success = false, message = message ?? "An unexpected error occurred. Please try again later." });
        }

        // ── Xem lịch sử báo cáo của tôi ─────────────────────────────────────────────
        [HttpGet]
        [Route("Account/MyReports")]
        [Route("Report/MyReports")]
        public async Task<IActionResult> MyReports()
        {
            if (!Request.Cookies.ContainsKey("AccessToken"))
            {
                return RedirectToAction("Login", "Account");
            }

            var courseReports = new List<MyCourseReportViewModel>();
            var reviewReports = new List<MyReviewReportViewModel>();

            // Lấy danh sách báo cáo khóa học
            var courseRes = await _apiClient.GetAsync("/api/report/my/courses");
            if (courseRes.IsSuccessStatusCode)
            {
                var content = await courseRes.Content.ReadAsStringAsync();
                try
                {
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("data", out var dataEl))
                    {
                        courseReports = JsonSerializer.Deserialize<List<MyCourseReportViewModel>>(dataEl.ToString(), _jsonOptions) ?? new List<MyCourseReportViewModel>();
                    }
                }
                catch { }
            }

            // Lấy danh sách báo cáo review
            var reviewRes = await _apiClient.GetAsync("/api/report/my/reviews");
            if (reviewRes.IsSuccessStatusCode)
            {
                var content = await reviewRes.Content.ReadAsStringAsync();
                try
                {
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("data", out var dataEl))
                    {
                        reviewReports = JsonSerializer.Deserialize<List<MyReviewReportViewModel>>(dataEl.ToString(), _jsonOptions) ?? new List<MyReviewReportViewModel>();
                    }
                }
                catch { }
            }

            ViewBag.CourseReports = courseReports;
            ViewBag.ReviewReports = reviewReports;

            return View("MyReports");
        }
    }
}
