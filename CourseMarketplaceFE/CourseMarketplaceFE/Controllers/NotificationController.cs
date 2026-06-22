using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CourseMarketplaceFE.Controllers
{
    public class NotificationController : Controller
    {
        private class ApiResp<T>
        {
            public bool Success { get; set; }
            public T? Data { get; set; }
            public string? Message { get; set; }
        }
        
        private class PagedResult<T>
        {
            public List<T>? Items { get; set; }
            public int TotalCount { get; set; }
        }

        private readonly ApiClient _api;

        public NotificationController(ApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _api.GetAsync("notification");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResp = JsonConvert.DeserializeObject<ApiResp<PagedResult<NotificationViewModel>>>(json);
                return View(apiResp?.Data?.Items ?? new List<NotificationViewModel>());
            }
            return View(new List<NotificationViewModel>());
        }

        [HttpGet]
        public async Task<IActionResult> GetListTiny()
        {
            var response = await _api.GetAsync("notification");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResp = JsonConvert.DeserializeObject<ApiResp<PagedResult<NotificationViewModel>>>(json);
                return Json(apiResp?.Data?.Items?.Take(5).ToList() ?? new List<NotificationViewModel>());
            }
            return Json(new object[] { });
        }

        public IActionResult Admin()
        {
            return View();
        }

        // --- BFF Proxies for JS Clients ---

        [HttpGet]
        public async Task<IActionResult> GetAllAdmin()
        {
            var response = await _api.GetAsync("notification/all");
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var response = await _api.GetAsync("notification");
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpPut]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var response = await _api.PutAsync("notification/mark-all-as-read", null);
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpPut]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var response = await _api.PutAsync($"notification/mark-as-read/{id}", null);
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _api.DeleteAsync($"notification/{id}");
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> SendAdvanced([FromBody] object payload)
        {
            var response = await _api.PostJsonAsync("notification/send-advanced", payload);
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpGet]
        
        [HttpGet]
        public async Task<IActionResult> GetUnreadSummary()
        {
            var response = await _api.GetAsync("notification/unread-summary");
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> SearchEmails([FromQuery] string query)
        {
            var response = await _api.GetAsync($"notification/search-emails?query={Uri.EscapeDataString(query ?? "")}");
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
    }
}