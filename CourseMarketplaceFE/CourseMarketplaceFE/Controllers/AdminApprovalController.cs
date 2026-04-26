using System.Text.Json;
using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceFE.Controllers
{
    public class AdminApprovalController : Controller
    {
        private readonly ApiClient _apiClient;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public AdminApprovalController(ApiClient apiClient) => _apiClient = apiClient;

        // Trang danh sách chờ duyệt
        public async Task<IActionResult> Index()
        {
            // Gọi GetAsync (không có <T>)
            var response = await _apiClient.GetAsync("/api/AdminApproval/pending-instructors");

            if (response.IsSuccessStatusCode)
            {
                // Đọc string và tự giải mã JSON
                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<List<InstructorApprovalViewModel>>(content, _jsonOptions);
                return View(data ?? new List<InstructorApprovalViewModel>());
            }

            return View(new List<InstructorApprovalViewModel>());
        }

        // Action xử lý duyệt/từ chối
        [HttpPost]
        public async Task<IActionResult> Process(int id, string status)
        {
            var payload = new { instructorId = id, status = status };

            // Sử dụng PostJsonAsync vì ApiClient của bạn có phương thức này để gửi Body JSON
            var response = await _apiClient.PostJsonAsync("/api/AdminApproval/process", payload);

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Lỗi khi xử lý từ Server" });
        }
    }
}
