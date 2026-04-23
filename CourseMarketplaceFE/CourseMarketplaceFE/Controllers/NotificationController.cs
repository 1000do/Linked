using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CourseMarketplaceFE.Controllers
{
    public class NotificationController : Controller
    {
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
                var notifications = JsonConvert.DeserializeObject<List<NotificationViewModel>>(json);
                return View(notifications);
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
                var list = JsonConvert.DeserializeObject<List<NotificationViewModel>>(json);
                return Json(list?.Take(5).ToList());
            }
            return Json(new object[] { });
        }
        public IActionResult Admin()
        {
            return View();
        }
    }
}
