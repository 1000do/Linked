using CourseMarketplaceFE.Models;
using CourseMarketplaceFE.Helpers;
using LinkedLearn.Models.UserVM;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;

namespace CourseMarketplaceFE.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiClient _api;
        private readonly JsonSerializerOptions _jsonOptions;

        public AccountController(IHttpClientFactory httpClientFactory, ApiClient api)
        {
            _httpClientFactory = httpClientFactory;
            _api = api;
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

            // Endpoint register không cần auth → dùng HttpClient trực tiếp
            var client = _httpClientFactory.CreateClient("BackendApi");
            var response = await client.PostAsJsonAsync("Auth/register", new
            {
                model.Email,
                model.Password,
                model.ConfirmPassword,
                model.FullName
            });

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
            // Đã login (có AccessToken) → về Home
            if (Request.Cookies.ContainsKey("AccessToken")) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Endpoint login không cần auth → dùng HttpClient trực tiếp
            var client = _httpClientFactory.CreateClient("BackendApi");
            var loginRequest = new { Email = model.Identifier.Trim().ToLower(), Password = model.Password };
            var response = await client.PostAsJsonAsync("Auth/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);

                if (result == null)
                {
                    ViewBag.ErrorMessage = "Phản hồi từ server không hợp lệ.";
                    return View(model);
                }

                // ── 1. Lưu AccessToken (HttpOnly, 24 giờ — khớp JWT expiration) ──────
                var tokenExpiry = DateTimeOffset.UtcNow.AddHours(24);
                Response.Cookies.Append("AccessToken", result.AccessToken ?? "", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = tokenExpiry,
                    Path = "/"
                });

                // ── 2. Lưu RefreshToken (HttpOnly, 7 ngày) ───────────────────────
                // RefreshToken BE trả trong body (vì cookie BE dùng path /api/auth/refresh)
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    Response.Cookies.Append("RefreshToken", result.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = Request.IsHttps,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddDays(7),
                        Path = "/"
                    });
                }

                // ── 3. Lưu display info (cùng thời hạn với AccessToken) ───────────
                var displayOpts = new CookieOptions
                {
                    Expires = tokenExpiry,
                    Path = "/"
                };
                Response.Cookies.Append("UserName", result.FullName ?? model.Identifier, displayOpts);
                Response.Cookies.Append("AvatarUrl", result.AvatarUrl ?? "", displayOpts);
                Response.Cookies.Append("UserRole", result.Role ?? "user", displayOpts);

                if (string.IsNullOrEmpty(result.Role))
                {
                    return Content("LỖI GIAO TIẾP VỚI BACKEND: API không trả về biến Role. Điều này chắc chắn 100% là do Backend chưa được biên dịch và chạy lại (vẫn đang chạy code cũ). Vui lòng TẮT HẲN Backend, Build lại và chạy lại!");
                }

                // ── 4. Redirect theo role ──────────────────────────────────────────
                if (result.Role.Trim().ToLower() == "manager")
                    return RedirectToAction("Admin", "Notification");

                return RedirectToAction("Index", "Home", new { debug = "is_user" });

            }
                var errorJson = await response.Content.ReadAsStringAsync();
                ViewBag.ErrorMessage = ParseErrorMessage(errorJson, "Email hoặc mật khẩu không chính xác.");
                return View(model);
            
            }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Gọi BE để revoke refresh token khỏi DB
            // ApiClient sẽ tự gắn AccessToken; nếu 401 nó sẽ thử refresh trước
            await _api.PostAsync("Auth/logout");

            // Xoá hết cookie FE
            Response.Cookies.Delete("AccessToken", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("RefreshToken", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("UserName", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("AvatarUrl", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("UserRole", new CookieOptions { Path = "/" });

            return RedirectToAction("Index", "Home");
        }

        // Giữ GET logout cho trường hợp link đơn giản (không form)
        [HttpGet]
        public async Task<IActionResult> LogoutGet()
        {
            await _api.PostAsync("Auth/logout");

            Response.Cookies.Delete("AccessToken", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("RefreshToken", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("UserName", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("AvatarUrl", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("UserRole", new CookieOptions { Path = "/" });

            return RedirectToAction("Login");
        }

        // ================= PROFILE MANAGEMENT =================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!Request.Cookies.ContainsKey("AccessToken")) return RedirectToAction("Login");

            // ApiClient tự gắn token + tự refresh nếu 401
            var response = await _api.GetAsync("Profile");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UserProfileApiResponse>(_jsonOptions);
                return View(result?.Data);
            }

            // Nếu vẫn lỗi sau khi đã thử refresh → hết phiên, bắt đăng nhập lại
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            if (!Request.Cookies.ContainsKey("AccessToken")) return RedirectToAction("Login");

            var response = await _api.GetAsync("Profile");
            if (!response.IsSuccessStatusCode) return RedirectToAction("Profile");

            var result = await response.Content.ReadFromJsonAsync<UserProfileApiResponse>(_jsonOptions);
            var data = result?.Data;

            var nameParts = data?.FullName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            var vm = new UpdateProfileViewModel
            {
                FirstName = nameParts.Length > 0 ? nameParts[0] : "",
                LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "",
                Bio = data?.Bio,
                PhoneNumber = data?.PhoneNumber,
                AvatarUrl = data?.AvatarUrl,
                DateOfBirth = data?.DateOfBirth?.ToString("yyyy-MM-dd")
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(UpdateProfileViewModel model)
        {
            if (!Request.Cookies.ContainsKey("AccessToken")) return RedirectToAction("Login");

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.FirstName ?? ""), "FirstName");
            content.Add(new StringContent(model.LastName ?? ""), "LastName");
            content.Add(new StringContent(model.Bio ?? ""), "Bio");
            content.Add(new StringContent(model.PhoneNumber ?? ""), "PhoneNumber");
            content.Add(new StringContent(model.DateOfBirth ?? ""), "DateOfBirth");

            if (model.AvatarFile != null)
            {
                var fileContent = new StreamContent(model.AvatarFile.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.AvatarFile.ContentType);
                content.Add(fileContent, "AvatarFile", model.AvatarFile.FileName);
            }

            var response = await _api.PutAsync("Profile/update", content);
            if (response.IsSuccessStatusCode)
            {
                // Đồng bộ lại cookie display (FullName, AvatarUrl)
                var profileResponse = await _api.GetAsync("Profile");
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

        // ─── Helpers ──────────────────────────────────────────────────────

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

        [HttpPost]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            var client = _httpClientFactory.CreateClient("BackendApi");

            var response = await client.PostAsJsonAsync("Auth/google-login", request);

            if (!response.IsSuccessStatusCode)
                return BadRequest();

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);

            var tokenExpiry = DateTimeOffset.UtcNow.AddHours(24);

            var cookieOptions = new CookieOptions
            {
                Expires = tokenExpiry,
                Path = "/"
            };

            Response.Cookies.Append("AccessToken", result?.AccessToken ?? "", new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = tokenExpiry,
                Path = "/"
            });

            if (!string.IsNullOrEmpty(result?.RefreshToken))
            {
                Response.Cookies.Append("RefreshToken", result.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    Path = "/"
                });
            }

            Response.Cookies.Append("UserName", result?.FullName ?? "User", cookieOptions);
            Response.Cookies.Append("AvatarUrl", result?.AvatarUrl ?? "", cookieOptions);
            Response.Cookies.Append("UserRole", result?.Role ?? "user", cookieOptions);

            return Ok();
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var client = _httpClientFactory.CreateClient("BackendApi");

            var res = await client.PostAsync($"Auth/forgot-password?email={email}", null);

            if (res.IsSuccessStatusCode)
            {
                TempData.Remove("IsVerifyFlow");
                TempData["ResetEmail"] = email;
                return RedirectToAction("VerifyOtp");
            }

            var error = await res.Content.ReadAsStringAsync();
            ViewBag.ErrorMessage = error;
            return View();
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var email = TempData["VerifyEmail"] ?? TempData["ResetEmail"];

            if (email == null) return RedirectToAction("Login");

            ViewBag.Email = email;
            TempData.Keep("VerifyEmail");
            TempData.Keep("ResetEmail");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string otp)
        {
            var email = TempData["VerifyEmail"]?.ToString() ?? TempData["ResetEmail"]?.ToString();
            var isEmailVerify = TempData["IsVerifyFlow"] != null;

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login");

            var client = _httpClientFactory.CreateClient("BackendApi");

            HttpResponseMessage res;

            if (isEmailVerify)
            {
                // VERIFY EMAIL
                res = await client.PostAsJsonAsync("Auth/verify-email", new
                {
                    Email = email,
                    Otp = otp
                });
            }
            else
            {
                // VERIFY OTP CHO RESET PASSWORD
                res = await client.PostAsJsonAsync("Auth/verify-otp", new
                {
                    Email = email,
                    Otp = otp
                });
            }

            if (!res.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = "OTP không đúng hoặc đã hết hạn";
                ViewBag.Email = email;

                if (isEmailVerify)
                {
                    TempData.Keep("VerifyEmail");
                    TempData.Keep("IsVerifyFlow");
                }
                else
                {
                    TempData.Keep("ResetEmail");
                }

                return View();
            }

            if (isEmailVerify)
            {
                // 🔥 XÓA CỜ SAU KHI DÙNG
                TempData.Remove("IsVerifyFlow");
                TempData.Remove("VerifyEmail");

                TempData["SuccessMessage"] = "Xác thực email thành công!";
                return RedirectToAction("Profile");
            }

            // flow reset password
            TempData["Otp"] = otp;
            TempData.Keep("ResetEmail");
            TempData.Keep("Otp");

            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (TempData["ResetEmail"] == null || TempData["Otp"] == null)
                return RedirectToAction("Login");

            ViewBag.Email = TempData["ResetEmail"];
            ViewBag.Otp = TempData["Otp"];

            TempData.Keep("ResetEmail");
            TempData.Keep("Otp");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string otp, string newPassword)
        {
            var client = _httpClientFactory.CreateClient("BackendApi");

            var res = await client.PostAsJsonAsync("Auth/reset-password", new
            {
                Email = email,
                Otp = otp,
                NewPassword = newPassword
            });

            if (res.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công";
                return RedirectToAction("Login");
            }

            // ❗ FIX QUAN TRỌNG
            ViewBag.ErrorMessage = "OTP sai hoặc hết hạn";

            ViewBag.Email = email;
            ViewBag.Otp = otp;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SendVerifyOtp()
        {
            var token = Request.Cookies["AccessToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            var client = _httpClientFactory.CreateClient("BackendApi");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // 🔥 GỌI PROFILE ĐỂ LẤY EMAIL
            var profileRes = await client.GetAsync("Profile");

            if (!profileRes.IsSuccessStatusCode)
                return RedirectToAction("Profile");

            var profile = await profileRes.Content.ReadFromJsonAsync<UserProfileApiResponse>(_jsonOptions);

            var email = profile?.Data?.Email;

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Profile");

            // 🔥 SET EMAIL
            TempData["VerifyEmail"] = email;
            TempData["IsVerifyFlow"] = true;

            var res = await client.PostAsync($"Auth/send-otp?email={email}", null);

            if (!res.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không gửi được OTP";
                return RedirectToAction("Profile");
            }

            return RedirectToAction("VerifyOtp");
        }
    }
}