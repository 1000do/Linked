using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceFE.Controllers
{
    public class QuizController : Controller
    {
        private readonly ApiClient _api;
        private readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

        public QuizController(ApiClient api)
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

        // ─── INDEX ────────────────────────────────────────────────────────
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            var viewModel = new QuizListViewModel
            {
                CurrentPage = page,
                SearchTerm = searchTerm
            };

            try
            {
                // Fetch Quizzes from API
                var resp = await _api.GetAsync("quizzes");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Array)
                    {
                        var quizzes = JsonSerializer.Deserialize<List<QuizSummaryItem>>(dataEl.GetRawText(), _jsonOpts);
                        if (quizzes != null)
                        {
                            // simple client side filtering for now
                            if (!string.IsNullOrEmpty(searchTerm))
                            {
                                quizzes = quizzes.Where(q => q.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                            }
                            viewModel.Quizzes = quizzes;
                            viewModel.TotalCount = quizzes.Count;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Failed to load quizzes: " + ex.Message;
            }

            try
            {
                var coursesResp = await _api.GetAsync("courses/my-courses");
                if (coursesResp.IsSuccessStatusCode)
                {
                    var json = await coursesResp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("data", out var dataEl) && dataEl.TryGetProperty("items", out var itemsEl) && itemsEl.ValueKind == JsonValueKind.Array)
                    {
                        var courses = JsonSerializer.Deserialize<List<CourseListViewModel>>(itemsEl.GetRawText(), _jsonOpts);
                        if (courses != null)
                        {
                            viewModel.AvailableCourses = courses;
                        }
                    }
                }
            }
            catch (Exception) { /* ignore */ }

            return View(viewModel);
        }

        // ─── CREATE (AJAX) ────────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> Create([FromBody] QuizCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid input data." });
            }

            try
            {
                Console.WriteLine("DEBUG FE - CourseId: " + model.CourseId + " Title: " + model.Title);
                var resp = await _api.PostJsonAsync("quizzes", model);
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("data", out var dataEl))
                    {
                        var quizId = dataEl.GetProperty("quizId").GetInt32();
                        return Json(new { success = true, quizId = quizId, message = "Quiz created successfully." });
                    }
                }
                var errorBody = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = errorBody });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "System error: " + ex.Message });
            }
        }

        [HttpGet("Quiz/QuestionPool/{id:int}")]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> QuestionPool(int id)
        {
            try
            {
                var resp = await _api.GetAsync($"quizzes/{id}/question-pool");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    return Content(json, "application/json");
                }
                var errorBody = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = errorBody });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "System error: " + ex.Message });
            }
        }

        // ─── SETTINGS (PATCH) ────────────────────────────────────────────────
        [HttpPatch("Quiz/Update/{id:int}")]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> Update(int id, [FromBody] QuizUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid input data." });
            }

            try
            {
                var resp = await _api.PatchJsonAsync($"quizzes/{id}", model);
                if (resp.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Settings updated successfully." });
                }
                var errorBody = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = errorBody });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "System error: " + ex.Message });
            }
        }

        // ─── DELETE (AJAX) ────────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var resp = await _api.DeleteAsync($"quizzes/{id}");
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

        // ─── TOGGLE HIDDEN (AJAX) ─────────────────────────────────────────
        public class ToggleHiddenRequest
        {
            public int QuizId { get; set; }
            public bool IsHidden { get; set; }
        }

        [HttpPost]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> ToggleHidden([FromBody] ToggleHiddenRequest request)
        {
            try
            {
                var resp = await _api.PatchAsync($"quizzes/{request.QuizId}/hide?hide={request.IsHidden.ToString().ToLower()}");
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

        // ─── EDITOR (GET) ────────────────────────────────────────────────
        [HttpGet("Quiz/Editor/{id:int}")]
        [Authorize(Roles = "instructor,admin,staff")]
        public async Task<IActionResult> Editor(int id)
        {
            var viewModel = new QuizEditorViewModel { QuizId = id };
            try
            {
                var resp = await _api.GetAsync($"quizzes/{id}");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("data", out var dataEl))
                    {
                        viewModel = JsonSerializer.Deserialize<QuizEditorViewModel>(dataEl.GetRawText(), _jsonOpts) ?? viewModel;
                    }
                }
                else if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }

                // Load Available Courses that the instructor owns
                var coursesResp = await _api.GetAsync("courses/my-courses?pageSize=100");
                if (coursesResp.IsSuccessStatusCode)
                {
                    var coursesJson = await coursesResp.Content.ReadAsStringAsync();
                    using var coursesDoc = JsonDocument.Parse(coursesJson);
                    if (coursesDoc.RootElement.TryGetProperty("data", out var cData) && cData.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
                    {
                        var allCourses = new List<CourseListViewModel>();
                        foreach (var item in items.EnumerateArray())
                        {
                            var status = item.TryGetProperty("courseStatus", out var s) ? s.GetString() : null;
                            if (status == "published" || status == "draft")
                            {
                                allCourses.Add(new CourseListViewModel
                                {
                                    Id = item.TryGetProperty("courseId", out var cId) ? cId.GetInt32() : 0,
                                    Title = item.TryGetProperty("title", out var title) ? title.GetString() ?? "Untitled" : "Untitled",
                                    Status = status ?? "draft"
                                });
                            }
                        }
                        viewModel.AvailableCourses = allCourses;
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading quiz details: " + ex.Message;
            }
            return View(viewModel);
        }

        // ─── UPDATE QUIZ SETTINGS (AJAX) ─────────────────────────────────
        [HttpPatch("Quiz/Settings/{id:int}")]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> UpdateSettings(int id, [FromBody] QuizUpdateViewModel model)
        {
            if (!ModelState.IsValid) return Json(new { success = false, message = "Invalid input." });
            try
            {
                var resp = await _api.PatchJsonAsync($"quizzes/{id}", model);
                if (resp.IsSuccessStatusCode) return Json(new { success = true });
                return Json(new { success = false, message = await resp.Content.ReadAsStringAsync() });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ─── ADD QUESTION (AJAX) ─────────────────────────────────────────
        [HttpPost("Quiz/{id:int}/Questions")]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> AddQuestion(int id, [FromBody] object payload)
        {
            try
            {
                var resp = await _api.PostJsonAsync($"quizzes/{id}/questions", payload);
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var data = doc.RootElement.GetProperty("data");
                    return Json(new { success = true, data = data.Clone() });
                }
                return Json(new { success = false, message = await resp.Content.ReadAsStringAsync() });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ─── ADD TO COURSE (AJAX) ────────────────────────────────────────
        [HttpPost("Quiz/{id:int}/AddToCourse")]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> AddToCourse(int id, [FromBody] JsonElement payload)
        {
            int courseId = payload.TryGetProperty("courseId", out var cProp) && cProp.ValueKind == JsonValueKind.Number 
                ? cProp.GetInt32() : 0;
            
            if (courseId <= 0) return Json(new { success = false, message = "Invalid course ID" });

            try
            {
                var reqBody = new { courseId = courseId, quizId = id };
                var resp = await _api.PostJsonAsync($"courses/{courseId}/quizzes", reqBody);
                if (resp.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                var err = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = err });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── DELETE QUESTION (AJAX) ──────────────────────────────────────
        [HttpDelete("Quiz/Questions/{questionId:int}")]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            try
            {
                var resp = await _api.DeleteAsync($"quizzes/questions/{questionId}");
                if (resp.IsSuccessStatusCode) return Json(new { success = true });
                return Json(new { success = false, message = await resp.Content.ReadAsStringAsync() });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ─── UPDATE QUESTION (AJAX) ──────────────────────────────────────
        [HttpPut("Quiz/Questions/{questionId:int}")]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> UpdateQuestion(int questionId, [FromBody] object payload)
        {
            try
            {
                var resp = await _api.PutJsonAsync($"quizzes/questions/{questionId}", payload);
                if (resp.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = await resp.Content.ReadAsStringAsync() });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ─── ATTEMPTS HISTORY VIEW ─────────────────────────────────────────
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> AttemptsHistory()
        {
            var viewModel = new QuizListViewModel();
            try
            {
                var resp = await _api.GetAsync("quizzes");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Array)
                    {
                        var quizzes = JsonSerializer.Deserialize<List<QuizSummaryItem>>(dataEl.GetRawText(), _jsonOpts);
                        if (quizzes != null) viewModel.Quizzes = quizzes;
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Failed to load quizzes: " + ex.Message;
            }
            return View(viewModel);
        }

        // ─── STUDENT ATTEMPTS AJAX ─────────────────────────────────────────
        [HttpGet("Quiz/StudentAttempts/{quizId}")]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> StudentAttempts(int quizId, int page = 1, int pageSize = 5)
        {
            try
            {
                var resp = await _api.GetAsync($"quizzes/{quizId}/attempts?page={page}&pageSize={pageSize}");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    return Json(new { success = true, data = doc.RootElement.GetProperty("data").Clone() });
                }
                return Json(new { success = false, message = await resp.Content.ReadAsStringAsync() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
