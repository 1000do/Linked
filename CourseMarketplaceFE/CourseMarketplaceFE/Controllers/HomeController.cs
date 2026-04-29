using Microsoft.AspNetCore.Mvc;
using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using System.Text.Json;

namespace CourseMarketplaceFE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiClient _apiClient;

        public HomeController(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }
        public async Task<IActionResult> Index(int page = 1)
        {
            var courses = new List<PublicCourseViewModel>();
            var response = await _apiClient.GetAsync("public/courses");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var data = json.RootElement.GetProperty("data").ToString();
                var allCourses = JsonSerializer.Deserialize<List<PublicCourseViewModel>>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<PublicCourseViewModel>();

                // Check wishlist status
                var wishResponse = await _apiClient.GetAsync("wishlist");
                var wishlistIds = new List<int>();
                if (wishResponse.IsSuccessStatusCode)
                {
                    var wishContent = await wishResponse.Content.ReadAsStringAsync();
                    var wishJson = JsonDocument.Parse(wishContent);
                    var wishData = wishJson.RootElement.GetProperty("data");
                    var wishlist = JsonSerializer.Deserialize<List<WishlistIdItem>>(wishData.ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    wishlistIds = wishlist?.Select(w => w.CourseId).ToList() ?? new List<int>();
                }

                foreach (var c in allCourses)
                {
                    c.IsInWishlist = wishlistIds.Contains(c.CourseId);
                }

                // Sorting for Home: newest first
                var final = allCourses.OrderByDescending(c => c.CreatedAt).ToList();

                // Pagination
                int pageSize = 8;
                int totalItems = final.Count;
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

                courses = final
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;

                // Data for specific sections
                ViewBag.FeaturedCourses = allCourses.OrderByDescending(c => c.RatingAverage).ThenByDescending(c => c.TotalStudents).Take(10).ToList();
                ViewBag.RecentCourses = allCourses.OrderByDescending(c => c.CreatedAt).Take(8).ToList();
            }

            var catResponse = await _apiClient.GetAsync("public/courses/categories");
            if (catResponse.IsSuccessStatusCode)
            {
                var catContent = await catResponse.Content.ReadAsStringAsync();
                var catJson = JsonDocument.Parse(catContent);
                var catData = catJson.RootElement.GetProperty("data").ToString();
                ViewBag.Categories = JsonSerializer.Deserialize<List<CategoryViewModel>>(catData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return View(courses);
        }

        private class WishlistIdItem
        {
            public int CourseId { get; set; }
        }
    }
}