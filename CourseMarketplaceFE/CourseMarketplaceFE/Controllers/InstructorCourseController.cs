using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseMarketplaceFE.Models;
using CourseMarketplaceFE.Helpers;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CourseMarketplaceFE.Controllers
{
    // [Authorize(Roles = "Instructor")] // TODO: Enable when FE auth is configured
    public class InstructorCourseController : Controller
    {
        private readonly ApiClient _api;
        private readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

        public InstructorCourseController(ApiClient api)
        {
            _api = api;
        }

        // ─── Auth guard: redirect to Login if no AccessToken cookie ────
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            if (!Request.Cookies.ContainsKey("AccessToken"))
            {
                context.Result = RedirectToAction("Login", "Account");
            }
        }

        // ─── LIST COURSES ─────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var viewModel = new InstructorStudioViewModel();

            try
            {
                // 1. Fetch Stats
                var statsResp = await _api.GetAsync("instructor/dashboard");
                if (statsResp.IsSuccessStatusCode)
                {
                    var statsJson = await statsResp.Content.ReadAsStringAsync();
                    using var statsDoc = JsonDocument.Parse(statsJson);
                    var statsData = statsDoc.RootElement.GetProperty("data");
                    
                    viewModel.TotalStudents = statsData.GetProperty("totalStudents").GetInt32();
                    viewModel.AverageRating = (double)statsData.GetProperty("averageRating").GetDecimal();
                    viewModel.ActiveCoursesCount = statsData.GetProperty("activeCoursesCount").GetInt32();
                    viewModel.TotalRevenue = statsData.GetProperty("totalRevenue").GetDecimal();
                }

                // 2. Fetch Courses
                var resp = await _api.GetAsync("courses/my-courses");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in dataEl.EnumerateArray())
                        {
                            viewModel.Courses.Add(new CourseListViewModel
                            {
                                Id = item.GetProperty("courseId").GetInt32(),
                                Title = item.GetProperty("title").GetString() ?? "Untitled",
                                Students = item.TryGetProperty("totalStudents", out var s) ? s.GetInt32() : 0, 
                                Rating = item.TryGetProperty("ratingAverage", out var r) ? (double)r.GetDecimal() : 0,
                                Status = item.GetProperty("courseStatus").GetString() ?? "Draft",
                                ThumbnailUrl = item.TryGetProperty("courseThumbnailUrl", out var t) ? t.GetString() ?? "" : "",
                                UpdatedAt = item.TryGetProperty("updatedAt", out var u) && u.ValueKind != JsonValueKind.Null
                                    ? u.GetDateTime() : DateTime.Now
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Failed to load dashboard: " + ex.Message;
            }

            return View(viewModel);
        }

        // ─── CREATE (GET) ─────────────────────────────────────────────────
        public IActionResult Create()
        {
            return View(new CreateCourseViewModel());
        }

        // ─── CREATE (POST) ────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create(CreateCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");
                formData.Add(new StringContent(model.Title), "Title");
                formData.Add(new StringContent(model.Description ?? ""), "Description");
                formData.Add(new StringContent(model.Price.ToString()), "Price");
                if (!string.IsNullOrEmpty(model.CourseThumbnailUrl))
                {
                    formData.Add(new StringContent(model.CourseThumbnailUrl), "CourseThumbnailUrl");
                }

                var resp = await _api.PostFormDataAsync("courses", formData);

                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    // Get the new courseId and redirect to editor
                    if (root.TryGetProperty("data", out var dataEl) &&
                        dataEl.TryGetProperty("courseId", out var idEl))
                    {
                        return RedirectToAction("Editor", new { id = idEl.GetInt32() });
                    }
                }
                else
                {
                    var errorBody = await resp.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"API Error ({resp.StatusCode}): {errorBody}");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to create course: " + ex.Message);
            }

            return View(model);
        }

        // ─── EDITOR ──────────────────────────────────────────────────────
        public async Task<IActionResult> Editor(int id)
        {
            ViewBag.CourseId = id;

            try
            {
                var resp = await _api.GetAsync($"courses/{id}");
                Console.WriteLine($"[Editor] GET courses/{id} => StatusCode: {(int)resp.StatusCode}");

                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("data", out var data))
                    {
                        ViewBag.CourseTitle = data.GetProperty("title").GetString();
                        ViewBag.CourseStatus = data.TryGetProperty("courseStatus", out var s) ? s.GetString() : "Draft";

                        // Parse lessons
                        if (data.TryGetProperty("lessons", out var lessonsEl) && lessonsEl.ValueKind == JsonValueKind.Array)
                        {
                            var rawJson = lessonsEl.GetRawText();
                            Console.WriteLine("[Editor] LESSONS JSON length: " + rawJson.Length);
                            ViewBag.LessonsJson = rawJson;
                        }
                        else
                        {
                            Console.WriteLine("[Editor] WARNING: 'lessons' property not found in API response.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[Editor] WARNING: 'data' property not found in API response.");
                    }
                }
                else
                {
                    var errorBody = await resp.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Editor] API FAILED: {(int)resp.StatusCode} - {errorBody}");
                    
                    // If 401/403 after auto-refresh attempt, session is dead — force re-login
                    if ((int)resp.StatusCode == 401 || (int)resp.StatusCode == 403)
                    {
                        Response.Cookies.Delete("AccessToken", new CookieOptions { Path = "/" });
                        return RedirectToAction("Login", "Account");
                    }
                    
                    ViewBag.ApiError = $"API returned {(int)resp.StatusCode}.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Editor] EXCEPTION: {ex.Message}");
                ViewBag.CourseTitle = "Course #" + id;
            }

            return View();
        }

        // ─── DELETE COURSE (AJAX) ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var resp = await _api.DeleteAsync($"courses/{id}");
                if (resp.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                var error = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // ─── ADD LESSON (AJAX) ────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> AddLesson([FromForm] int courseId, [FromForm] string title)
        {
            try
            {
                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(courseId.ToString()), "CourseId");
                formData.Add(new StringContent(title), "Title");

                var resp = await _api.PostFormDataAsync("lessons", formData);

                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    // Clone the element so it survives after doc is disposed
                    var data = doc.RootElement.GetProperty("data").Clone();
                    return Json(new { success = true, data = data });
                }
                var error = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // ─── ADD MATERIAL (AJAX) ──────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> AddMaterial([FromForm] int lessonId, [FromForm] string title, [FromForm] string description, [FromForm] string materialUrl, [FromForm] string type)
        {
            try
            {
                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(title ?? "Untitled Material"), "Title");
                if (!string.IsNullOrEmpty(description))
                    formData.Add(new StringContent(description), "Description");
                if (!string.IsNullOrEmpty(materialUrl))
                    formData.Add(new StringContent(materialUrl), "MaterialUrl");
                
                formData.Add(new StringContent(type ?? "video"), "MaterialMetadata.FileType");

                var resp = await _api.PostFormDataAsync($"lessons/{lessonId}/materials", formData);

                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var data = doc.RootElement.GetProperty("data").Clone();
                    return Json(new { success = true, data = data });
                }
                var error = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // ─── UPDATE COURSE STATUS (AJAX) ──────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> UpdateCourseStatus([FromForm] int courseId, [FromForm] string status)
        {
            try
            {
                var payload = new { status = status };
                var resp = await _api.PatchJsonAsync($"courses/{courseId}/status", payload);

                if (resp.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                var error = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("TestCourse/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> TestCourse(int id)
        {
            var materials = await _api.GetAsync($"courses/{id}");
            var json = await materials.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
    }
}
