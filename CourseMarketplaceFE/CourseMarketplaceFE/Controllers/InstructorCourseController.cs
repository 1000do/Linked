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

        // ─── LIST COURSES ─────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var courses = new List<CourseListViewModel>();

            try
            {
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
                            courses.Add(new CourseListViewModel
                            {
                                Id = item.GetProperty("courseId").GetInt32(),
                                Title = item.GetProperty("title").GetString() ?? "Untitled",
                                Students = 0, // Backend doesn't return student count yet
                                Rating = 0,
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
                ViewBag.Error = "Failed to load courses: " + ex.Message;
            }

            return View(courses);
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
                            ViewBag.LessonsJson = lessonsEl.GetRawText();
                        }
                    }
                }
            }
            catch
            {
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
    }
}
