using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CourseMarketplaceFE.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApiClient _apiClient;

        public CourseController(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> MyCourses()
        {
            var response = await _apiClient.GetAsync("enrollment/my-courses");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var data = json.RootElement.GetProperty("data").ToString();
                var courses = JsonSerializer.Deserialize<List<EnrolledCourseViewModel>>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(courses);
            }
            return RedirectToAction("Login", "Account");
        }

        private async Task<List<int>> GetWishlistIdsAsync()
        {
            var response = await _apiClient.GetAsync("wishlist");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var data = json.RootElement.GetProperty("data");
                var wishlist = JsonSerializer.Deserialize<List<WishlistResponseItem>>(data.ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return wishlist?.Select(w => w.CourseId).ToList() ?? new List<int>();
            }
            return new List<int>();
        }

        private class WishlistResponseItem
        {
            public int CourseId { get; set; }
        }

        public async Task<IActionResult> Index(string query, string category, string sort, string price, string rating, int page = 1)
        {
            int pageSize = 12;
            var url = $"public/courses?query={Uri.EscapeDataString(query ?? "")}&category={Uri.EscapeDataString(category ?? "")}&sort={Uri.EscapeDataString(sort ?? "")}&price={Uri.EscapeDataString(price ?? "")}&rating={Uri.EscapeDataString(rating ?? "")}&page={page}&pageSize={pageSize}";
            var response = await _apiClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var data = json.RootElement.GetProperty("data");
                
                // Handle both old format (courses) and new format (items from PagedResult)
                string coursesJson;
                if (data.TryGetProperty("items", out var itemsProp))
                {
                    coursesJson = itemsProp.ToString();
                }
                else if (data.TryGetProperty("courses", out var coursesProp))
                {
                    coursesJson = coursesProp.ToString();
                }
                else 
                {
                    coursesJson = data.ToString();
                }
                
                var paginatedCourses = JsonSerializer.Deserialize<List<PublicCourseViewModel>>(coursesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<PublicCourseViewModel>();
                
                int totalPages = 1;
                int totalItems = paginatedCourses.Count;
                if (data.TryGetProperty("totalPages", out var tp))
                {
                    totalPages = tp.GetInt32();
                }
                if (data.TryGetProperty("totalCount", out var tc))
                {
                    totalItems = tc.GetInt32();
                }
                
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;

                // Check wishlist status
                var wishlistIds = await GetWishlistIdsAsync();
                foreach (var c in paginatedCourses)
                {
                    c.IsInWishlist = wishlistIds.Contains(c.CourseId);
                }

                ViewBag.Query = query;
                ViewBag.Category = category;
                ViewBag.Sort = sort;
                ViewBag.Price = price;
                ViewBag.Rating = rating;

                var catResponse = await _apiClient.GetAsync("public/courses/categories");
                if (catResponse.IsSuccessStatusCode)
                {
                    var catContent = await catResponse.Content.ReadAsStringAsync();
                    var catJson = JsonDocument.Parse(catContent);
                    var catData = catJson.RootElement.GetProperty("data").ToString();
                    ViewBag.Categories = JsonSerializer.Deserialize<List<CategoryViewModel>>(catData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                return View(paginatedCourses);
            }
            return View(new List<PublicCourseViewModel>());
        }

        [HttpGet]
        public async Task<IActionResult> SearchCoursesJson(string query)
        {
            var url = $"public/courses?query={Uri.EscapeDataString(query ?? "")}&page=1&pageSize=8";
            var response = await _apiClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var data = json.RootElement.GetProperty("data");
                
                // Handle both old format (courses) and new format (items from PagedResult)
                string coursesJson;
                if (data.TryGetProperty("items", out var itemsProp))
                {
                    coursesJson = itemsProp.ToString();
                }
                else if (data.TryGetProperty("courses", out var coursesProp))
                {
                    coursesJson = coursesProp.ToString();
                }
                else 
                {
                    coursesJson = data.ToString();
                }
                
                var courses = JsonSerializer.Deserialize<List<PublicCourseViewModel>>(coursesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<PublicCourseViewModel>();

                var results = courses.Select(c => new {
                    c.CourseId,
                    c.Title,
                    c.InstructorName,
                    c.CourseThumbnailUrl,
                    c.Price,
                    c.RatingAverage
                }).ToList();

                return Json(new { success = true, data = results });
            }
            return Json(new { success = false, data = new List<object>() });
        }

        public async Task<IActionResult> Details(int id)
        {
            var response = await _apiClient.GetAsync($"public/courses/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var data = json.RootElement.GetProperty("data").ToString();
                var course = JsonSerializer.Deserialize<CourseDetailViewModel>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (course != null)
                {
                    var checkResponse = await _apiClient.GetAsync($"wishlist/check/{id}");
                    if (checkResponse.IsSuccessStatusCode)
                    {
                        var checkContent = await checkResponse.Content.ReadAsStringAsync();
                        var checkJson = JsonDocument.Parse(checkContent);
                        course.IsInWishlist = checkJson.RootElement.GetProperty("isInWishlist").GetBoolean();
                    }
                }

                return View(course);
            }
            return NotFound();
        }

        public async Task<IActionResult> Learn(int id)
        {
            var response = await _apiClient.GetAsync($"public/courses/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var data = json.RootElement.GetProperty("data").ToString();
                var course = JsonSerializer.Deserialize<CourseDetailViewModel>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(course);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> EnrollFree(int id)
        {
            var response = await _apiClient.PostAsync($"enrollment/free-enroll/{id}");
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Enrollment successful!" });
            }
            
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var message = json.RootElement.TryGetProperty("message", out var msg) ? msg.GetString() : "Enrollment error.";
            
            return Json(new { success = false, message });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadMaterial(string url, string fileName)
        {
            if (string.IsNullOrEmpty(url)) return BadRequest("URL is missing.");
            
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Could not fetch the file from the remote server.");
                }

                var stream = await response.Content.ReadAsStreamAsync();
                var extension = Path.GetExtension(new Uri(url).AbsolutePath);
                if (string.IsNullOrEmpty(extension)) extension = ".pdf";
                
                var finalName = string.IsNullOrEmpty(fileName) ? "document" : fileName;
                if (!finalName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    finalName += extension;
                }

                return File(stream, "application/octet-stream", finalName);
            }
            catch (Exception)
            {
                return BadRequest("An error occurred while downloading the file.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetProgress(int id)
        {
            var response = await _apiClient.GetAsync($"enrollment/progress/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                return Json(new { success = true, data = json.RootElement.GetProperty("data") });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProgress([FromBody] JsonElement body)
        {
            var response = await _apiClient.PostJsonAsync("enrollment/progress", body);
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetReviews(int id, int page = 1, int pageSize = 5, int? starFilter = null)
        {
            var url = $"review/course/{id}?page={page}&pageSize={pageSize}";
            if (starFilter.HasValue)
                url += $"&starFilter={starFilter.Value}";

            var response = await _apiClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                return Json(new { success = true, data = json.RootElement.GetProperty("data") });
            }
            return Json(new { success = false, data = new { items = new List<object>(), totalCount = 0 } });
        }

        [HttpGet]
        public async Task<IActionResult> GetLessonReviews(int id, int page = 1, int pageSize = 5)
        {
            var response = await _apiClient.GetAsync($"review/lesson/{id}?page={page}&pageSize={pageSize}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                return Json(new { success = true, data = json.RootElement.GetProperty("data") });
            }
            return Json(new { success = false, data = new { items = new List<object>(), totalCount = 0 } });
        }

        /// <summary>Thống kê phân bổ sao (dynamic từ DB)</summary>
        [HttpGet]
        public async Task<IActionResult> GetReviewStats(int id)
        {
            var response = await _apiClient.GetAsync($"review/stats/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                return Json(new { success = true, data = json.RootElement.GetProperty("data") });
            }
            return Json(new { success = false });
        }

        /// <summary>Kiểm tra quyền review của user cho khóa học</summary>
        [HttpGet]
        public async Task<IActionResult> GetReviewEligibility(int id)
        {
            var response = await _apiClient.GetAsync($"review/eligibility/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                return Json(new { success = true, data = json.RootElement.GetProperty("data") });
            }
            return Json(new { success = false });
        }

        /// <summary>Gửi review — source = detail | learn</summary>
        [HttpPost]
        public async Task<IActionResult> SubmitReview([FromBody] JsonElement body, [FromQuery] string source = "learn")
        {
            var response = await _apiClient.PostJsonAsync($"review?source={source}", body);
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var json = JsonDocument.Parse(content);
                var message = json.RootElement.TryGetProperty("message", out var msg) ? msg.GetString() : "Review submission error.";
                return Json(new { success = false, message });
            }
            catch
            {
                return Json(new { success = false, message = "Review submission error." });
            }
        }
        [HttpPost]
        public async Task<IActionResult> ReportReview([FromBody] JsonElement body)
        {
            var response = await _apiClient.PostJsonAsync("review/report", body);
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        /// <summary>Avg rating cho tất cả lesson của 1 course (dùng cho sidebar Learn)</summary>
        [HttpGet]
        public async Task<IActionResult> GetLessonRatings(int id)
        {
            var response = await _apiClient.GetAsync($"review/lesson-ratings/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                return Json(new { success = true, data = json.RootElement.GetProperty("data") });
            }
            return Json(new { success = false, data = new List<object>() });
        }

        /// <summary>Chỉnh sửa review (chỉ chủ review)</summary>
        [HttpPut]
        public async Task<IActionResult> UpdateReview(int reviewId, string type, [FromBody] JsonElement body)
        {
            var response = await _apiClient.PutJsonAsync($"review/{reviewId}?type={type}", body);
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var json = JsonDocument.Parse(content);
                var message = json.RootElement.TryGetProperty("message", out var msg) ? msg.GetString() : "Update error.";
                return Json(new { success = false, message });
            }
            catch { return Json(new { success = false, message = "Update error." }); }
        }

        /// <summary>Xóa mềm review (chỉ chủ review)</summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteReview(int reviewId, string type = "course")
        {
            var response = await _apiClient.DeleteAsync($"review/{reviewId}?type={type}");
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var json = JsonDocument.Parse(content);
                var message = json.RootElement.TryGetProperty("message", out var msg) ? msg.GetString() : "Delete error.";
                return Json(new { success = false, message });
            }
            catch { return Json(new { success = false, message = "Delete error." }); }
        }
    }
}
