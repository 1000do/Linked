using LinkedLearn.Models.UserVM;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
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

        // ================= AUTHENTICATION =================

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Email) && !model.Email.EndsWith("@gmail.com"))
                ModelState.AddModelError("Email", "Chỉ hỗ trợ đăng ký bằng Gmail.");

            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("BackendApi");
            var response = await client.PostAsJsonAsync("Auth/register", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đăng ký thành công!";
                return RedirectToAction("Login");
            }

            await HandleApiError(response);
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (Request.Cookies.ContainsKey("UserName")) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("BackendApi");
            var loginRequest = new { Email = model.Identifier.Trim().ToLower(), Password = model.Password };
            var response = await client.PostAsJsonAsync("Auth/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);

                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    Path = "/"
                };

                // 1. Lưu AuthToken (HttpOnly)
                Response.Cookies.Append("AuthToken", result?.Token ?? "", new CookieOptions
                {
                    Expires = cookieOptions.Expires,
                    HttpOnly = true,
                    Path = "/"
                });

                // 2. Lưu UserName & AvatarUrl từ BE trả về để Header hiển thị ngay
                Response.Cookies.Append("UserName", result?.FullName ?? model.Identifier, cookieOptions);
                Response.Cookies.Append("AvatarUrl", result?.AvatarUrl ?? "", cookieOptions);

                return RedirectToAction("Index", "Home");
            }

            var errorJson = await response.Content.ReadAsStringAsync();
            ViewBag.ErrorMessage = ParseErrorMessage(errorJson, "Email hoặc mật khẩu không chính xác.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("UserName", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("AvatarUrl", new CookieOptions { Path = "/" });
            return RedirectToAction("Login");
        }

        // ================= PROFILE MANAGEMENT =================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            var client = _httpClientFactory.CreateClient("BackendApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("Profile");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UserProfileApiResponse>(_jsonOptions);
                return View(result?.Data);
            }
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            var client = _httpClientFactory.CreateClient("BackendApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("Profile");
            if (!response.IsSuccessStatusCode) return RedirectToAction("Profile");

            var result = await response.Content.ReadFromJsonAsync<UserProfileApiResponse>(_jsonOptions);
            var data = result?.Data;

            var nameParts = data?.FullName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new string[] { "" };
            var vm = new UpdateProfileViewModel
            {
                FirstName = nameParts.Length > 0 ? nameParts[0] : "",
                LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "",
                Bio = data?.Bio,
                PhoneNumber = data?.PhoneNumber,
                AvatarUrl = data?.AvatarUrl,
                DateOfBirth = data?.DateOfBirth?.ToString("yyyy-MM-dd") // Format cho input date
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(UpdateProfileViewModel model)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            var client = _httpClientFactory.CreateClient("BackendApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.FirstName ?? ""), "FirstName");
            content.Add(new StringContent(model.LastName ?? ""), "LastName");
            content.Add(new StringContent(model.Bio ?? ""), "Bio");
            content.Add(new StringContent(model.PhoneNumber ?? ""), "PhoneNumber");
            content.Add(new StringContent(model.DateOfBirth ?? ""), "DateOfBirth");

            if (model.AvatarFile != null)
            {
                var fileContent = new StreamContent(model.AvatarFile.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.AvatarFile.ContentType);
                content.Add(fileContent, "AvatarFile", model.AvatarFile.FileName);
            }

            var response = await client.PutAsync("Profile/update", content);
            if (response.IsSuccessStatusCode)
            {
                // ĐỒNG BỘ LẠI COOKIE: Gọi lại Profile để lấy thông tin mới nhất (bao gồm URL ảnh mới)
                var profileResponse = await client.GetAsync("Profile");
                if (profileResponse.IsSuccessStatusCode)
                {
                    var profileResult = await profileResponse.Content.ReadFromJsonAsync<UserProfileApiResponse>(_jsonOptions);
                    var updated = profileResult?.Data;

                    var cookieOptions = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7), Path = "/" };
                    Response.Cookies.Append("UserName", updated?.FullName ?? "User", cookieOptions);
                    Response.Cookies.Append("AvatarUrl", updated?.AvatarUrl ?? "", cookieOptions);
                }

                TempData["SuccessMessage"] = "Cập nhật thành công!";
                return RedirectToAction("Profile");
            }

            ViewBag.ErrorMessage = "Không thể cập nhật hồ sơ.";
            return View(model);
        }

        private async Task HandleApiError(HttpResponseMessage response)
        {
            var errorJson = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, ParseErrorMessage(errorJson, "Lỗi Server."));
        }

        private string ParseErrorMessage(string json, string defaultMsg)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var msgElement))
                    return msgElement.GetString() ?? defaultMsg;
            }
            catch { }
            return defaultMsg;
        }
    }
}