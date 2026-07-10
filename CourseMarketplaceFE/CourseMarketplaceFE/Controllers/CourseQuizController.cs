using System;
using System.Threading.Tasks;
using CourseMarketplaceFE.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceFE.Controllers
{
    public class CourseQuizController : Controller
    {
        private readonly ApiClient _api;

        public CourseQuizController(ApiClient api)
        {
            _api = api;
        }

        // ─── Auth guard
        public override async Task OnActionExecutionAsync(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context, Microsoft.AspNetCore.Mvc.Filters.ActionExecutionDelegate next)
        {
            if (!Request.Cookies.ContainsKey("AccessToken"))
            {
                context.Result = RedirectToAction("Login", "Account");
                return;
            }

            var statusCookie = Request.Cookies["InstructorApprovalStatus"];
            if (statusCookie != "Approved")
            {
                context.Result = RedirectToAction("Index", "Home");
                return;
            }

            await next();
        }

        // ─── REMOVE FROM COURSE (AJAX) ───────────────────────────────────
        [HttpDelete("CourseQuiz/Remove")]
        public async Task<IActionResult> Remove(int courseId, int quizId)
        {
            try
            {
                // This will map to BE DELETE api/courses/{courseId}/quizzes/{quizId}
                var resp = await _api.DeleteAsync($"courses/{courseId}/quizzes/{quizId}");
                if (resp.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                var errorBody = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = errorBody });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "System error: " + ex.Message });
            }
        }

        // ─── ADD TO COURSE (AJAX) ───────────────────────────────────
        public class CourseQuizAddRequest
        {
            public int CourseId { get; set; }
            public int QuizId { get; set; }
            public int OrderIndex { get; set; }
        }

        [HttpPost("CourseQuiz/Add")]
        public async Task<IActionResult> Add([FromBody] CourseQuizAddRequest req)
        {
            try
            {
                var resp = await _api.PostJsonAsync($"courses/{req.CourseId}/quizzes", req);
                if (resp.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                var errorBody = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = errorBody });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "System error: " + ex.Message });
            }
        }

        // ─── TOGGLE VISIBILITY (AJAX) ───────────────────────────────────
        [HttpPatch("CourseQuiz/Hide")]
        public async Task<IActionResult> Hide(int courseId, int quizId, bool hide)
        {
            try
            {
                // This will map to BE PATCH api/courses/{courseId}/quizzes/{quizId}/hide?hide={hide}
                var resp = await _api.PatchAsync($"courses/{courseId}/quizzes/{quizId}/hide?hide={hide}");
                if (resp.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                var errorBody = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = errorBody });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "System error: " + ex.Message });
            }
        }

        // ─── GET AVAILABLE QUIZZES (AJAX) ───────────────────────────────────
        [HttpGet("CourseQuiz/Available/{courseId}")]
        public async Task<IActionResult> GetAvailableQuizzes(int courseId)
        {
            try
            {
                // We'll fetch all quizzes, then filter to only those belonging to courseId
                // The FE quiz view model API has an endpoint to get all quizzes.
                var resp = await _api.GetAsync("quizzes?page=1&pageSize=1000");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = System.Text.Json.JsonDocument.Parse(json);
                    var items = doc.RootElement.GetProperty("data").EnumerateArray();
                    
                    var list = new System.Collections.Generic.List<object>();
                    foreach (var item in items)
                    {
                        if (item.TryGetProperty("courseId", out var cIdProp))
                        {
                            if (cIdProp.GetInt32() != courseId)
                            {
                                continue;
                            }
                        }

                        list.Add(new {
                            quizId = item.GetProperty("quizId").GetInt32(),
                            title = item.GetProperty("title").GetString(),
                            questionCount = item.GetProperty("questionCount").GetInt32()
                        });
                    }
                    return Json(new { success = true, data = list });
                }
                var errorBody = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = errorBody });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "System error: " + ex.Message });
            }
        }
    }
}
