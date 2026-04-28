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
        public async Task<IActionResult> Index()
        {
            var courses = new List<PublicCourseViewModel>();
            var response = await _apiClient.GetAsync("public/courses");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var data = json.RootElement.GetProperty("data").ToString();
                courses = JsonSerializer.Deserialize<List<PublicCourseViewModel>>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<PublicCourseViewModel>();

                // Check wishlist status
                var wishResponse = await _apiClient.GetAsync("wishlist");
                if (wishResponse.IsSuccessStatusCode)
                {
                    var wishContent = await wishResponse.Content.ReadAsStringAsync();
                    var wishJson = JsonDocument.Parse(wishContent);
                    var wishData = wishJson.RootElement.GetProperty("data");
                    var wishlist = JsonSerializer.Deserialize<List<WishlistIdItem>>(wishData.ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var wishlistIds = wishlist?.Select(w => w.CourseId).ToList() ?? new List<int>();
                    
                    foreach (var c in courses)
                    {
                        c.IsInWishlist = wishlistIds.Contains(c.CourseId);
                    }
                }
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