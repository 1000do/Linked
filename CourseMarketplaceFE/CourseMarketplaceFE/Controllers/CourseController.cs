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

        public async Task<IActionResult> Index(string query, string category, string sort)
        {
            var response = await _apiClient.GetAsync("public/courses");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var data = json.RootElement.GetProperty("data").ToString();
                var courses = JsonSerializer.Deserialize<List<PublicCourseViewModel>>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (courses != null)
                {
                    // Check wishlist status
                    var wishlistIds = await GetWishlistIdsAsync();
                    foreach (var c in courses)
                    {
                        c.IsInWishlist = wishlistIds.Contains(c.CourseId);
                    }

                    if (!string.IsNullOrEmpty(query))
                    {
                        courses = courses.Where(c => c.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                                                     (c.Description != null && c.Description.Contains(query, StringComparison.OrdinalIgnoreCase))).ToList();
                    }
                    if (!string.IsNullOrEmpty(category))
                    {
                        courses = courses.Where(c => string.Equals(c.CategoryName, category, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    
                    if (sort == "price_asc")
                        courses = courses.OrderBy(c => c.Price).ToList();
                    else if (sort == "price_desc")
                        courses = courses.OrderByDescending(c => c.Price).ToList();
                    else if (sort == "rating")
                        courses = courses.OrderByDescending(c => c.RatingAverage).ToList();
                    else // default: newest
                        courses = courses.OrderByDescending(c => c.CreatedAt).ToList();
                }
                
                ViewBag.Query = query;
                ViewBag.Category = category;
                ViewBag.Sort = sort;

                var catResponse = await _apiClient.GetAsync("public/courses/categories");
                if (catResponse.IsSuccessStatusCode)
                {
                    var catContent = await catResponse.Content.ReadAsStringAsync();
                    var catJson = JsonDocument.Parse(catContent);
                    var catData = catJson.RootElement.GetProperty("data").ToString();
                    ViewBag.Categories = JsonSerializer.Deserialize<List<CategoryViewModel>>(catData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                return View(courses ?? new List<PublicCourseViewModel>());
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
