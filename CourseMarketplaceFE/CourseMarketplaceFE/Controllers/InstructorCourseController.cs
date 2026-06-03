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

        // ─── Auth guard: kiểm tra Token và Trạng thái duyệt (Approved) ────
        public override async Task OnActionExecutionAsync(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context, Microsoft.AspNetCore.Mvc.Filters.ActionExecutionDelegate next)
        {
            if (!Request.Cookies.ContainsKey("AccessToken"))
            {
                context.Result = RedirectToAction("Login", "Account");
                return;
            }

            // 1. Kiểm tra nhanh qua Cookie
            var statusCookie = Request.Cookies["InstructorApprovalStatus"];
            if (statusCookie == "Approved")
            {
                await next();
                return;
            }

            // 2. Nếu cookie chưa có hoặc chưa Approved -> Gọi API kiểm tra lại cho chắc
            try
            {
                var response = await _api.GetAsync("instructor/dashboard");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("data", out var dataEl) &&
                        dataEl.TryGetProperty("approvalStatus", out var statusEl))
                    {
                        var status = statusEl.GetString();
                        
                        // Cập nhật lại Cookie cho lần sau
                        var statusCookieOpts = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7), Path = "/" };
                        Response.Cookies.Append("InstructorApprovalStatus", status ?? "None", statusCookieOpts);

                        if (status != "Approved")
                        {
                            context.Result = RedirectToAction("ApplicationStatus", "Instructor");
                            return;
                        }
                    }
                    else 
                    {
                        context.Result = RedirectToAction("Apply", "Instructor");
                        return;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    context.Result = RedirectToAction("Apply", "Instructor");
                    return;
                }
                else 
                {
                    context.Result = RedirectToAction("Index", "Home");
                    return;
                }
            }
            catch
            {
                context.Result = RedirectToAction("Index", "Home");
                return;
            }

            await next();
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

                // 2. Fetch Courses from the new paged, database-driven endpoint!
                int pageSize = 6;
                var url = $"courses/my-courses?search={Uri.EscapeDataString(searchTerm ?? "")}&status={Uri.EscapeDataString(status ?? "")}&page={page}&pageSize={pageSize}";
                var resp = await _api.GetAsync(url);
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("data", out var dataEl))
                    {
                        // API returns PagedResult: items, totalCount, totalPages, page, pageSize
                        JsonElement coursesEl;
                        if (dataEl.TryGetProperty("items", out var itemsProp))
                            coursesEl = itemsProp;
                        else if (dataEl.TryGetProperty("courses", out var coursesProp))
                            coursesEl = coursesProp;
                        else
                            coursesEl = default;

                        var allCourses = new List<CourseListViewModel>();
                        if (coursesEl.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in coursesEl.EnumerateArray())
                            {
                                allCourses.Add(new CourseListViewModel
                                {
                                    Id = item.GetProperty("courseId").GetInt32(),
                                    Title = item.GetProperty("title").GetString() ?? "Untitled",
                                    Students = item.TryGetProperty("totalStudents", out var s) ? s.GetInt32() : 0, 
                                    Rating = item.TryGetProperty("ratingAverage", out var r) ? (double)r.GetDecimal() : 0,
                                    Status = item.GetProperty("courseStatus").GetString() ?? "Draft",
                                    ThumbnailUrl = item.TryGetProperty("courseThumbnailUrl", out var t) ? t.GetString() ?? "" : "",
                                    IsRemoved = item.TryGetProperty("isRemoved", out var ir) && ir.ValueKind != JsonValueKind.Null && ir.GetBoolean(),
                                    UpdatedAt = item.TryGetProperty("updatedAt", out var u) && u.ValueKind != JsonValueKind.Null
                                        ? u.GetDateTime() : DateTime.Now
                                });
                            }
                        }

                        // totalCount from PagedResult, fallback to totalItems for legacy format
                        int totalItems = 0;
                        if (dataEl.TryGetProperty("totalCount", out var tcProp)) totalItems = tcProp.GetInt32();
                        else if (dataEl.TryGetProperty("totalItems", out var tiProp)) totalItems = tiProp.GetInt32();
                        
                        int totalPages = 1;
                        if (dataEl.TryGetProperty("totalPages", out var tpProp)) totalPages = tpProp.GetInt32();

                        viewModel.TotalItems = totalItems;
                        viewModel.TotalPages = totalPages;
                        viewModel.Courses = allCourses;
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
            // Kiểm tra Stripe status & Transfer Rate
            await LoadStripeStatusAsync();
            await LoadTransferRateAsync();

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
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var errors = new List<string>();
                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            errors.Add(error.ErrorMessage);
                        }
                    }
                    var errorMsg = string.Join("<br/>", errors);
                    return Json(new { success = false, message = errorMsg });
                }
                    
                await LoadStripeStatusAsync();
                await LoadTransferRateAsync();
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
                formData.Add(new StringContent(model.WhatYouWillLearn ?? ""), "WhatYouWillLearn");
                formData.Add(new StringContent(model.Requirements ?? ""), "Requirements");
                
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

                    if (root.TryGetProperty("data", out var dataEl) &&
                        dataEl.TryGetProperty("courseId", out var idEl))
                    {
                        int newCourseId = idEl.GetInt32();
                        if (model.CouponId.HasValue && model.CouponId > 0)
                        {
                            var couponPayload = new { courseId = newCourseId, couponId = model.CouponId.Value };
                            await _api.PostJsonAsync("coupon/platform/apply", couponPayload);
                        }
                        
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = true, courseId = newCourseId, title = model.Title });
                        }
                        return RedirectToAction("Editor", new { id = newCourseId });
                    }
                }
                else
                {
                    var errorBody = await resp.Content.ReadAsStringAsync();
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Json(new { success = false, message = errorBody });
                        
                    ViewBag.ApiError = errorBody;
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "System error: " + ex.Message });
                    
                ViewBag.ApiError = "Failed to create course: " + ex.Message;
            }

            await LoadStripeStatusAsync();
            await LoadTransferRateAsync();
            model.AvailableCategories = await GetCategoriesAsync();
            return View(model);
        }

        // ─── EDITOR ──────────────────────────────────────────────────────
        public async Task<IActionResult> Editor(int id)
        {
            ViewBag.CourseId = id;
            ViewBag.InstructorId = int.Parse(User.FindFirst("InstructorId")?.Value ?? "0");
            ViewBag.AvailableCategories = await GetCategoriesAsync();

            // Kiểm tra Stripe status & Transfer Rate
            await LoadStripeStatusAsync();
            await LoadTransferRateAsync();

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
                        ViewBag.ModerationFeedback = data.TryGetProperty("moderationFeedback", out var mf) ? mf.GetString() : "";

                        // Applied Coupon
                        ViewBag.CouponCode = data.TryGetProperty("appliedCouponCode", out var ccode) && ccode.ValueKind != JsonValueKind.Null ? ccode.GetString() : null;
                        ViewBag.CouponType = data.TryGetProperty("appliedCouponType", out var ctype) && ctype.ValueKind != JsonValueKind.Null ? ctype.GetString() : null;
                        ViewBag.CouponValue = data.TryGetProperty("appliedCouponValue", out var cval) && cval.ValueKind != JsonValueKind.Null ? cval.GetDecimal() : 0;
                        ViewBag.IsInAnyCart = data.TryGetProperty("isInAnyCart", out var inCart) && inCart.ValueKind == JsonValueKind.True;

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
                    var data = doc.RootElement.GetProperty("data");
                    var newStatus = data.TryGetProperty("courseStatus", out var s) ? s.GetString() : null;
                    return Json(new { success = true, data = data.Clone(), status = newStatus });
                }
                var error = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // ─── REMOVE LESSON (AJAX) ────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> RemoveLesson([FromForm] int lessonId)
        {
            try
            {
                var resp = await _api.DeleteAsync($"lessons/{lessonId}");
                if (resp.IsSuccessStatusCode)
                    return Json(new { success = true });
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
                    var data = doc.RootElement.GetProperty("data");
                    var newStatus = data.TryGetProperty("courseStatus", out var s) ? s.GetString() : null;
                    return Json(new { success = true, data = data.Clone(), status = newStatus });
                }
                var error = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // ─── REMOVE MATERIAL (soft-delete, AJAX) ─────────────────────────
        [HttpPost]
        public async Task<IActionResult> RemoveMaterial([FromForm] int materialId)
        {
            try
            {
                var resp = await _api.PatchAsync($"lessons/materials/{materialId}/remove");
                if (resp.IsSuccessStatusCode)
                    return Json(new { success = true });
                var error = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // ─── UPDATE COURSE STATUS (AJAX) ──────────────────────────────────
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
        // [HttpPost]
        // public async Task<IActionResult> UpdateCourseStatus([FromForm] int courseId, [FromForm] string status)
        // {
        //     try
        //     {
        //         if (status.Equals("pending", StringComparison.OrdinalIgnoreCase))
        //         {
        //             // Trigger AI moderation pipeline
        //             var payload = new { courseId = courseId };
        //             var resp = await _api.PostJsonAsync("courses/moderate", payload);
                    
        //             if (resp.IsSuccessStatusCode)
        //             {
        //                 var json = await resp.Content.ReadAsStringAsync();
        //                 using var doc = JsonDocument.Parse(json);
        //                 var data = doc.RootElement.GetProperty("data");
        //                 var modStatus = data.GetProperty("moderationStatus").GetString();
                        
        //                 if (modStatus == "REJECTED")
        //                 {
        //                     return Json(new { success = false, message = "Khóa học bị từ chối tự động do trùng lặp hoặc vi phạm chính sách AI." });
        //                 }
        //                 return Json(new { success = true });
        //             }
        //             var error = await resp.Content.ReadAsStringAsync();
        //             return Json(new { success = false, message = error });
        //         }
        //         else
        //         {
        //             // Regular status update for 'archived' or 'published'
        //             var payload = new { status = status };
        //             var resp = await _api.PatchJsonAsync($"courses/{courseId}/status", payload);

        //             if (resp.IsSuccessStatusCode)
        //             {
        //                 return Json(new { success = true });
        //             }
        //             var error = await resp.Content.ReadAsStringAsync();
        //             return Json(new { success = false, message = error });
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         return Json(new { success = false, message = ex.Message });
        //     }
        // }
        
        [HttpPost]
        public async Task<IActionResult> ModerateCourse([FromForm] int courseId)
        {
            try
            {
                var payload = new { CourseId = courseId };
                var resp = await _api.PostJsonAsync("courses/moderate", payload);
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
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var data = doc.RootElement.GetProperty("data");
                    var newStatus = data.GetProperty("courseStatus").GetString();
                    return Json(new { success = true, status = newStatus });
                }
                var error = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // ─── CHECK STRIPE STATUS (AJAX) ───────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CheckStripeStatus()
        {
            try
            {
                var resp = await _api.GetAsync("instructor/stripe-status");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    return Json(new
                    {
                        success = true,
                        isStripeActive = root.TryGetProperty("isStripeActive", out var sa) && sa.GetBoolean()
                    });
                }
                return Json(new { success = false, isStripeActive = false });
            }
            catch
            {
                return Json(new { success = false, isStripeActive = false });
            }
        }

        /// <summary>
        /// Load Stripe status vào ViewBag cho Create/Editor views.
        /// </summary>
        private async Task LoadStripeStatusAsync()
        {
            try
            {
                var resp = await _api.GetAsync("instructor/stripe-status");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    ViewBag.IsStripeActive = root.TryGetProperty("isStripeActive", out var sa) && sa.GetBoolean();
                    return;
                }
            }
            catch { }
            ViewBag.IsStripeActive = false;
        }

        /// <summary>
        /// Load Transfer Rate từ Backend
        /// </summary>
        private async Task LoadTransferRateAsync()
        {
            try
            {
                var resp = await _api.GetAsync("admin/finance/transfer-rate");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("data", out var rateEl))
                    {
                        ViewBag.TransferRate = rateEl.GetDecimal();
                        return;
                    }
                }
            }
            catch { }
            ViewBag.TransferRate = 70.00m;
        }

        // ─── TRASH VIEW ──────────────────────────────────────────────────
        public async Task<IActionResult> Trash()
        {
            try
            {
                var resp = await _api.GetAsync("lessons/materials/trash");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var data = doc.RootElement.GetProperty("data");
                    
                    var materials = JsonSerializer.Deserialize<List<MaterialTrashViewModel>>(data.GetRawText(), _jsonOpts);
                    return View(materials);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Failed to load trash: " + ex.Message;
            }
            return View(new List<MaterialTrashViewModel>());
        }

        // ─── PERMANENT DELETE MATERIAL (AJAX) ────────────────────────────
        [HttpPost]
        public async Task<IActionResult> PermanentDeleteMaterial(int materialId)
        {
            try
            {
                var resp = await _api.DeleteAsync($"lessons/materials/{materialId}/permanent");
                if (resp.IsSuccessStatusCode)
                    return Json(new { success = true });
                var error = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // ─── RESTORE MATERIAL (AJAX) ─────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> RestoreMaterial(int materialId)
        {
            try
            {
                var resp = await _api.PostAsync($"lessons/materials/{materialId}/restore");
                if (resp.IsSuccessStatusCode)
                    return Json(new { success = true });
                var error = await resp.Content.ReadAsStringAsync();
                return Json(new { success = false, message = error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLessonTitle(int lessonId, [FromBody] object requestBody)
        {
            try
            {
                var resp = await _api.PatchJsonAsync($"lessons/{lessonId}/title", requestBody);
                var respContent = await resp.Content.ReadAsStringAsync();
                
                if (resp.IsSuccessStatusCode)
                    return Json(new { success = true, data = respContent });
                
                return Json(new { success = false, message = respContent });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLessonDescription(int lessonId, [FromBody] object requestBody)
        {
            try
            {
                var resp = await _api.PatchJsonAsync($"lessons/{lessonId}/description", requestBody);
                var respContent = await resp.Content.ReadAsStringAsync();
                
                if (resp.IsSuccessStatusCode)
                    return Json(new { success = true, data = respContent });
                
                return Json(new { success = false, message = respContent });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMaterialDetails(int materialId, [FromBody] object requestBody)
        {
            try
            {
                var resp = await _api.PatchJsonAsync($"lessons/materials/{materialId}", requestBody);
                var respContent = await resp.Content.ReadAsStringAsync();
                
                if (resp.IsSuccessStatusCode)
                    return Json(new { success = true, data = respContent });
                
                return Json(new { success = false, message = respContent });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
