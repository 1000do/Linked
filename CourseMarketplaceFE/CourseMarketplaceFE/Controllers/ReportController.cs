using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using CourseMarketplaceFE.Models.Common;

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
        [Authorize(Roles = "user,instructor")]
        public async Task<IActionResult> ReportCourse([FromBody] CreateCourseReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { Message = "Validation failed", Errors = errors });
            }

            var response = await _apiClient.PostJsonAsync("/api/report/courses", model);
            var content = await response.Content.ReadAsStringAsync();
            var jsonContent = JsonSerializer.Deserialize<object>(content, _jsonOptions);
            return StatusCode((int)response.StatusCode, jsonContent);
        }

        // ── Gửi báo cáo đánh giá khóa học ───────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "user,instructor")]
        public async Task<IActionResult> ReportCourseReview([FromBody] CreateReviewReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { Message = "Validation failed", Errors = errors });
            }

            var payload = new
            {
                CourseReviewId = model.ReviewId,
                Reason = model.Reason,
                Description = model.Description
            };

            var response = await _apiClient.PostJsonAsync("/api/report/course-reviews", payload);
            var content = await response.Content.ReadAsStringAsync();
            var jsonContent = JsonSerializer.Deserialize<object>(content, _jsonOptions);
            return StatusCode((int)response.StatusCode, jsonContent);
        }

        // ── Gửi báo cáo đánh giá bài học ────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "user,instructor")]
        public async Task<IActionResult> ReportLessonReview([FromBody] CreateReviewReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { Message = "Validation failed", Errors = errors });
            }

            var payload = new
            {
                LessonReviewId = model.ReviewId,
                Reason = model.Reason,
                Description = model.Description
            };

            var response = await _apiClient.PostJsonAsync("/api/report/lesson-reviews", payload);
            var content = await response.Content.ReadAsStringAsync();
            var jsonContent = JsonSerializer.Deserialize<object>(content, _jsonOptions);
            return StatusCode((int)response.StatusCode, jsonContent);
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
