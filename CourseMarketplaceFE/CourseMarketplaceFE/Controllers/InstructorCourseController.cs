using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseMarketplaceFE.Models;
using CourseMarketplaceFE.Helpers;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CourseMarketplaceFE.Controllers
{
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
        public async Task<IActionResult> Index(string? searchTerm, string? status, int page = 1)
        {
            var viewModel = new InstructorStudioViewModel
            {
                CurrentPage = page,
                SearchTerm = searchTerm,
                FilterStatus = status
            };

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
                        var allCourses = new List<CourseListViewModel>();
                        foreach (var item in dataEl.EnumerateArray())
                        {
                            allCourses.Add(new CourseListViewModel
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

                        // Apply Search & Filter
                        var filtered = allCourses.AsQueryable();
                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            filtered = filtered.Where(c => c.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                        }
                        if (!string.IsNullOrEmpty(status) && status != "All")
                        {
                            filtered = filtered.Where(c => c.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
                        }

                        // Pagination
                        int pageSize = 6;
                        var final = filtered.ToList();
                        viewModel.TotalItems = final.Count;
                        viewModel.TotalPages = (int)Math.Ceiling(viewModel.TotalItems / (double)pageSize);
                        
                        viewModel.Courses = final
                            .OrderByDescending(c => c.UpdatedAt)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Failed to load dashboard: " + ex.Message;
            }

            return View(viewModel);
        }

        private async Task<List<CategoryViewModel>> GetCategoriesAsync()
        {
            var resp = await _api.GetAsync("public/courses/categories");
            if (resp.IsSuccessStatusCode)
            {
                var content = await resp.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                if (json.RootElement.TryGetProperty("data", out var data))
                {
                    return JsonSerializer.Deserialize<List<CategoryViewModel>>(data.ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<CategoryViewModel>();
                }
            }
            return new List<CategoryViewModel>();
        }

        // ─── CREATE (GET) ─────────────────────────────────────────────────
        public async Task<IActionResult> Create()
        {
            var model = new CreateCourseViewModel
            {
                AvailableCategories = await GetCategoriesAsync()
            };
            return View(model);
        }

        // ─── CREATE (POST) ────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create(CreateCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableCategories = await GetCategoriesAsync();
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

            model.AvailableCategories = await GetCategoriesAsync();
            return View(model);
        }

        // ─── EDITOR ──────────────────────────────────────────────────────
        public async Task<IActionResult> Editor(int id)
        {
            ViewBag.CourseId = id;
            ViewBag.AvailableCategories = await GetCategoriesAsync();

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
                        ViewBag.Description = data.TryGetProperty("description", out var d) ? d.GetString() : "";
                        ViewBag.WhatYouWillLearn = data.TryGetProperty("whatYouWillLearn", out var w) ? w.GetString() : "";
                        ViewBag.Requirements = data.TryGetProperty("requirements", out var r) ? r.GetString() : "";
                        ViewBag.CategoryId = data.TryGetProperty("categoryId", out var c) ? c.GetInt32() : 0;
                        ViewBag.Price = data.TryGetProperty("price", out var p) ? p.GetDecimal() : 0;
                        ViewBag.ThumbnailUrl = data.TryGetProperty("courseThumbnailUrl", out var t) ? t.GetString() : "";

                        // Parse lessons
                        if (data.TryGetProperty("lessons", out var lessonsEl) && lessonsEl.ValueKind == JsonValueKind.Array)
                        {
                            var rawJson = lessonsEl.GetRawText();
                            ViewBag.LessonsJson = rawJson;
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
        public async Task<IActionResult> AddMaterial([FromForm] int lessonId, [FromForm] string title, [FromForm] string description, [FromForm] string materialUrl, [FromForm] string type, [FromForm] int? duration, [FromForm] long? fileSize, [FromForm] string? fileExtension)
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
                if (duration.HasValue) formData.Add(new StringContent(duration.Value.ToString()), "MaterialMetadata.Duration");
                if (fileSize.HasValue) formData.Add(new StringContent(fileSize.Value.ToString()), "MaterialMetadata.FileSize");
                if (!string.IsNullOrEmpty(fileExtension)) formData.Add(new StringContent(fileExtension), "MaterialMetadata.FileExtension");

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
        
        // ─── UPDATE COURSE DETAILS (AJAX) ─────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> UpdateDetails([FromForm] int courseId, [FromForm] string title, [FromForm] string description, [FromForm] decimal price, [FromForm] int categoryId, [FromForm] string whatYouWillLearn, [FromForm] string requirements, [FromForm] string thumbnailUrl)
        {
            try
            {
                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(courseId.ToString()), "CourseId");
                formData.Add(new StringContent(title), "Title");
                formData.Add(new StringContent(description ?? ""), "Description");
                formData.Add(new StringContent(price.ToString()), "Price");
                formData.Add(new StringContent(categoryId.ToString()), "CategoryId");
                formData.Add(new StringContent(whatYouWillLearn ?? ""), "WhatYouWillLearn");
                formData.Add(new StringContent(requirements ?? ""), "Requirements");
                formData.Add(new StringContent(thumbnailUrl ?? ""), "CourseThumbnailUrl");

                var resp = await _api.PutFormDataAsync($"courses/{courseId}", formData);

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

    }
}
