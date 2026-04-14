using LinkedLearn.Models.UserVM;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

namespace CourseMarketplaceFE.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Email) && !model.Email.EndsWith("@gmail.com"))
            {
                ModelState.AddModelError("Email", "Chỉ hỗ trợ đăng ký bằng Gmail.");
            }

            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("BackendApi");
            var registerRequest = new
            {
                Email = model.Email,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword,
                FullName = model.FullName
            };

            var response = await client.PostAsJsonAsync("Auth/register", registerRequest);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đăng ký thành công!";
                return RedirectToAction("Login");
            }

            var error = await response.Content.ReadFromJsonAsync<ApiResponse>();
            ModelState.AddModelError(string.Empty, error?.Message ?? "Đăng ký thất bại.");
            return View(model);
        }

        // 1. Trang GET Login: Nếu login rồi thì thôi
        [HttpGet]
        public IActionResult Login()
        {
            if (Request.Cookies.ContainsKey("UserName"))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // 2. Hàm xử lý Login POST
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("BackendApi");

            // Gửi yêu cầu (Chỉ để "Auth/login" vì BaseAddress đã có "api/")
            var loginRequest = new { Email = model.Identifier.Trim().ToLower(), Password = model.Password };

            // Đã xóa chữ "api/" ở đây
            var response = await client.PostAsJsonAsync("Auth/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);

                Response.Cookies.Append("UserName", result?.FullName ?? model.Identifier, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    HttpOnly = false,
                    Path = "/",
                    SameSite = SameSiteMode.Lax
                });

                return RedirectToAction("Index", "Home");
            }

            // Đọc chính xác thông báo lỗi từ Backend để biết tại sao fail
            var errorJson = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(errorJson);
                ViewBag.ErrorMessage = doc.RootElement.GetProperty("message").GetString();
            }
            catch
            {
                ViewBag.ErrorMessage = "Sai mật khẩu hoặc tài khoản không tồn tại.";
            }

            return View(model);
        }
        [HttpGet]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("UserName", new CookieOptions { Path = "/" });
            return RedirectToAction("Login");
        }
    }
}