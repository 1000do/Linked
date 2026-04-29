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
    }
}
