using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CourseMarketplaceFE.Controllers
{
    public class WishlistController : Controller
    {
        private readonly ApiClient _apiClient;

        public WishlistController(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _apiClient.GetAsync("wishlist");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var data = json.RootElement.GetProperty("data").ToString();
                var wishlist = JsonSerializer.Deserialize<List<WishlistResponse>>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(wishlist ?? new List<WishlistResponse>());
            }
            
            // If unauthorized, redirect to login or show empty
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(new List<WishlistResponse>());
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int courseId, string returnUrl)
        {
            await ToggleLogic(courseId);
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAjax(int courseId)
        {
            bool isInWishlist = await ToggleLogic(courseId);
            return Json(new { success = true, isInWishlist = isInWishlist });
        }

        private async Task<bool> ToggleLogic(int courseId)
        {
            var checkResponse = await _apiClient.GetAsync($"wishlist/check/{courseId}");
            bool isInWishlist = false;
            if (checkResponse.IsSuccessStatusCode)
            {
                var checkContent = await checkResponse.Content.ReadAsStringAsync();
                var checkJson = JsonDocument.Parse(checkContent);
                isInWishlist = checkJson.RootElement.GetProperty("isInWishlist").GetBoolean();
            }

            if (isInWishlist)
            {
                await _apiClient.DeleteAsync($"wishlist/{courseId}");
                return false;
            }
            else
            {
                await _apiClient.PostAsync($"wishlist/{courseId}");
                return true;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int courseId)
        {
            await _apiClient.DeleteAsync($"wishlist/{courseId}");
            return RedirectToAction("Index");
        }
    }

    public class WishlistResponse
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string? CourseThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        public string? InstructorName { get; set; }
        public DateTime AddedDate { get; set; }
    }
}
