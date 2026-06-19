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

        private readonly HttpClient _client;

        public NotificationController(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("BackendApi");
        }

        public async Task<IActionResult> Index()
        {
            var response = await _client.GetAsync("notifications");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                // Map vào NotificationViewModel của FE
                var apiResp = JsonConvert.DeserializeObject<ApiResp<List<NotificationViewModel>>>(json);
                return View(apiResp?.Data ?? new List<NotificationViewModel>());
            }
            return View(new List<NotificationViewModel>());
        }

        [HttpGet]
        public async Task<IActionResult> GetListTiny()
        {
            var response = await _client.GetAsync("notifications");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResp = JsonConvert.DeserializeObject<ApiResp<List<NotificationViewModel>>>(json);
                return Json(apiResp?.Data?.Take(5).ToList() ?? new List<NotificationViewModel>());
            }
            return Json(new object[] { });
        }
        public IActionResult Admin()
        {
            return View();
        }
    }
}
