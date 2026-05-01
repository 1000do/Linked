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

        public async Task<IActionResult> Index(string query, string category, string sort, int page = 1)
        {
            var response = await _apiClient.GetAsync("public/courses");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var data = json.RootElement.GetProperty("data").ToString();
                var allCourses = JsonSerializer.Deserialize<List<PublicCourseViewModel>>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<PublicCourseViewModel>();
                
                // Check wishlist status
                var wishlistIds = await GetWishlistIdsAsync();
                foreach (var c in allCourses)
                {
                    c.IsInWishlist = wishlistIds.Contains(c.CourseId);
                }

                // Filtering
                var filtered = allCourses.AsQueryable();
                if (!string.IsNullOrEmpty(query))
                {
                    filtered = filtered.Where(c => c.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                                                 (c.Description != null && c.Description.Contains(query, StringComparison.OrdinalIgnoreCase)));
                }
                if (!string.IsNullOrEmpty(category))
                {
                    filtered = filtered.Where(c => string.Equals(c.CategoryName, category, StringComparison.OrdinalIgnoreCase));
                }
                
                // Sorting
                if (sort == "price_asc")
                    filtered = filtered.OrderBy(c => c.Price);
                else if (sort == "price_desc")
                    filtered = filtered.OrderByDescending(c => c.Price);
                else if (sort == "rating")
                    filtered = filtered.OrderByDescending(c => c.RatingAverage);
                else // default: newest
                    filtered = filtered.OrderByDescending(c => c.CreatedAt);

                var final = filtered.ToList();

                // Pagination
                int pageSize = 12;
                int totalItems = final.Count;
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

                var paginatedCourses = final
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                ViewBag.Query = query;
                ViewBag.Category = category;
                ViewBag.Sort = sort;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;

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
                return Json(new { success = true, message = "Ghi danh thành công!" });
            }
            
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var message = json.RootElement.TryGetProperty("message", out var msg) ? msg.GetString() : "Lỗi ghi danh.";
            
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
        public async Task<IActionResult> GetReviews(int id)
        {
            var response = await _apiClient.GetAsync($"review/course/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                return Json(new { success = true, data = json.RootElement.GetProperty("data") });
            }
            return Json(new { success = false, data = new List<object>() });
        }

        [HttpGet]
        public async Task<IActionResult> GetLessonReviews(int id)
        {
            var response = await _apiClient.GetAsync($"review/lesson/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                return Json(new { success = true, data = json.RootElement.GetProperty("data") });
            }
            return Json(new { success = false, data = new List<object>() });
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
                var message = json.RootElement.TryGetProperty("message", out var msg) ? msg.GetString() : "Lỗi gửi đánh giá.";
                return Json(new { success = false, message });
            }
            catch
            {
                return Json(new { success = false, message = "Lỗi gửi đánh giá." });
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
    }
}
