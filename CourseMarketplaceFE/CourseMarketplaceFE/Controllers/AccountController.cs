using CourseMarketplaceFE.Models;
using CourseMarketplaceFE.Helpers;
using LinkedLearn.Models.UserVM;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;

namespace CourseMarketplaceFE.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiClient _api;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IConfiguration _config;

        public AccountController(IHttpClientFactory httpClientFactory, ApiClient api, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _api = api;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _config = config;
        }

        // ================= AUTHENTICATION =================

        [HttpGet]
        public IActionResult Register(string? giftToken = null, string? returnUrl = null)
        {
            ViewBag.GiftToken = giftToken;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model, string? giftToken = null, string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(model.Email) && !model.Email.EndsWith("@gmail.com"))
                ModelState.AddModelError("Email", "Only Gmail registration is supported.");

            if (!ModelState.IsValid)
            {
                ViewBag.GiftToken = giftToken;
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            // Endpoint register không cần auth → dùng HttpClient trực tiếp
            var client = _httpClientFactory.CreateClient("BackendApi");
            var response = await client.PostAsJsonAsync("Auth/register", new
            {
                model.Email,
                model.Username,
                model.Password,
                model.ConfirmPassword,
                model.FullName
            });

            if (response.IsSuccessStatusCode)
            {
                TempData["RecommendVerifyEmail"] = model.Email;
                if (!string.IsNullOrEmpty(giftToken)) TempData["GiftToken"] = giftToken;
                if (!string.IsNullOrEmpty(returnUrl)) TempData["ReturnUrl"] = returnUrl;
                return RedirectToAction("VerifyRecommend");
            }

            await HandleApiError(response);
            ViewBag.GiftToken = giftToken;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Đã login (có AccessToken) → về Home/returnUrl/AdminFinance
            if (Request.Cookies.ContainsKey("AccessToken"))
            {
                var role = Request.Cookies["UserRole"];
                var roleLower = role?.Trim().ToLower();
                if (roleLower == "admin")
                {
                    return RedirectToAction("Index", "AdminFinance");
                }
                else if (roleLower == "staff")
                {
                    return RedirectToAction("Admin", "Notification");
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.GoogleClientId = _config["Authentication:Google:ClientId"];
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.GoogleClientId = _config["Authentication:Google:ClientId"];

            if (!ModelState.IsValid) return View(model);

            // Endpoint login không cần auth → dùng HttpClient trực tiếp
            var client = _httpClientFactory.CreateClient("BackendApi");
            var loginRequest = new { UsernameOrEmail = model.UsernameOrEmail.Trim(), Password = model.Password };
            var response = await client.PostAsJsonAsync("Auth/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);

                if (result == null)
                {
                    ViewBag.ErrorMessage = "Invalid server response.";
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
                Response.Cookies.Append("UserName", result.FullName ?? model.UsernameOrEmail, displayOpts);
                Response.Cookies.Append("AvatarUrl", result.AvatarUrl ?? "", displayOpts);
                Response.Cookies.Append("UserRole", result.Role ?? "user", displayOpts);
                Response.Cookies.Append("UserId", result.AccountId.ToString(), displayOpts);
                Response.Cookies.Append("IsVerified", result.IsVerified ? "true" : "false", displayOpts);

                if (string.IsNullOrEmpty(result.Role))
                {
                    return Content("BACKEND COMMUNICATION ERROR: API did not return Role variable. This is 100% because the Backend has not been recompiled and restarted (still running old code). Please FULLY STOP Backend, Rebuild and run again!");
                }

                // ── 4. Redirect theo role ──────────────────────────────────────────
                var roleLower = result.Role.Trim().ToLower();
                if (roleLower == "admin")
                {
                    return RedirectToAction("Index", "AdminFinance");
                }
                else if (roleLower == "staff")
                {
                    return RedirectToAction("Admin", "Notification");
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home", new { debug = "is_user" });

            }
            var errorJson = await response.Content.ReadAsStringAsync();
            ViewBag.ErrorMessage = ParseErrorMessage(errorJson, "Incorrect username/email or password.");
            return View(model);
        }
        
        [HttpPost]
        [Authorize(Roles = "user,instructor,staff,admin")]
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
            Response.Cookies.Delete("UserId", new CookieOptions { Path = "/" });

            return RedirectToAction("Index", "Home");
        }

        // Giữ GET logout cho trường hợp link đơn giản (không form)
        [HttpGet("logout")]
        [Authorize(Roles = "user,instructor,staff,admin")]
        public async Task<IActionResult> LogoutGet()
        {
            await _api.PostAsync("Auth/logout");

            Response.Cookies.Delete("AccessToken", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("RefreshToken", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("UserName", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("AvatarUrl", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("UserRole", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("UserId", new CookieOptions { Path = "/" });

            return RedirectToAction("Login");
        }

        // ================= PROFILE MANAGEMENT =================

        [HttpGet]
        [Authorize(Roles = "user,instructor")]
        public async Task<IActionResult> Profile()
        {
            if (!Request.Cookies.ContainsKey("AccessToken")) return RedirectToAction("Login");

            // ApiClient tự gắn token + tự refresh nếu 401
            var response = await _api.GetAsync("Profile");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UserProfileApiResponse>(_jsonOptions);

                // Đồng bộ cờ IsVerified
                var isVerified = result?.Data?.IsVerified ?? false;
                var displayOpts = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddHours(24), Path = "/" };
                Response.Cookies.Append("IsVerified", isVerified ? "true" : "false", displayOpts);

                return View(result?.Data);
            }

            // Nếu vẫn lỗi sau khi đã thử refresh → hết phiên, bắt đăng nhập lại
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize(Roles = "user,instructor")]
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
                Email = data?.Email,
                Username = data?.Username,
                AuthProvider = data?.AuthProvider,
                Bio = data?.Bio,
                PhoneNumber = data?.PhoneNumber,
                AvatarUrl = data?.AvatarUrl,
                DateOfBirth = data?.DateOfBirth?.ToString("yyyy-MM-dd"),
                IsInstructor = data?.IsInstructor ?? false,
                ProfessionalTitle = data?.ProfessionalTitle,
                ExpertiseCategories = data?.ExpertiseCategories,
                LinkedinUrl = data?.LinkedinUrl,
                YoutubeUrl = data?.YoutubeUrl,
                FacebookUrl = data?.FacebookUrl
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "user,instructor")]
        public async Task<IActionResult> EditProfile(UpdateProfileViewModel model)
        {
            if (!Request.Cookies.ContainsKey("AccessToken")) return RedirectToAction("Login");

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.FirstName ?? ""), "FirstName");
            content.Add(new StringContent(model.LastName ?? ""), "LastName");
            content.Add(new StringContent(model.Email ?? ""), "Email");
            content.Add(new StringContent(model.Bio ?? ""), "Bio");
            content.Add(new StringContent(model.PhoneNumber ?? ""), "PhoneNumber");
            content.Add(new StringContent(model.DateOfBirth ?? ""), "DateOfBirth");

            if (model.IsInstructor)
            {
                content.Add(new StringContent(model.ProfessionalTitle ?? ""), "ProfessionalTitle");
                content.Add(new StringContent(model.ExpertiseCategories ?? ""), "ExpertiseCategories");
                content.Add(new StringContent(model.LinkedinUrl ?? ""), "LinkedinUrl");
                content.Add(new StringContent(model.YoutubeUrl ?? ""), "YoutubeUrl");
                content.Add(new StringContent(model.FacebookUrl ?? ""), "FacebookUrl");
            }

            if (model.AvatarFile != null)
            {
                var fileContent = new StreamContent(model.AvatarFile.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.AvatarFile.ContentType);
                content.Add(fileContent, "AvatarFile", model.AvatarFile.FileName);
            }

            var response = await _api.PutAsync("Profile/update", content);
            if (response.IsSuccessStatusCode)
            {
                // Đồng bộ lại cookie display (FullName, AvatarUrl, IsVerified)
                var profileResponse = await _api.GetAsync("Profile");
                if (profileResponse.IsSuccessStatusCode)
                {
                    var profileResult = await profileResponse.Content.ReadFromJsonAsync<UserProfileApiResponse>(_jsonOptions);
                    var updated = profileResult?.Data;

                    var cookieOptions = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7), Path = "/" };
                    Response.Cookies.Append("UserName", updated?.FullName ?? "User", cookieOptions);
                    Response.Cookies.Append("AvatarUrl", updated?.AvatarUrl ?? "", cookieOptions);
                    Response.Cookies.Append("IsVerified", (updated?.IsVerified ?? false) ? "true" : "false", cookieOptions);
                }

                TempData["SuccessMessage"] = "Updated successfully!";
                return RedirectToAction("Profile");
            }

            var errorJson = await response.Content.ReadAsStringAsync();
            ViewBag.ErrorMessage = ParseErrorMessage(errorJson, "Cannot update profile.");
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "user,instructor")]
        public IActionResult ChangePassword()
        {
            if (!Request.Cookies.ContainsKey("AccessToken")) return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "user,instructor")]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmNewPassword)
        {
            if (!Request.Cookies.ContainsKey("AccessToken")) return RedirectToAction("Login");

            if (newPassword != confirmNewPassword)
            {
                ViewBag.ErrorMessage = "Password confirmation does not match.";
                return View();
            }

            var response = await _api.PostJsonAsync("Profile/change-password", new
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            });

            if (response.IsSuccessStatusCode)
            {
                // Gọi BE logout để dọn token nếu cần
                await _api.PostAsync("Auth/logout");

                // Xoá cookie FE để force đăng nhập lại
                Response.Cookies.Delete("AccessToken", new CookieOptions { Path = "/" });
                Response.Cookies.Delete("RefreshToken", new CookieOptions { Path = "/" });
                Response.Cookies.Delete("UserName", new CookieOptions { Path = "/" });
                Response.Cookies.Delete("AvatarUrl", new CookieOptions { Path = "/" });
                Response.Cookies.Delete("UserRole", new CookieOptions { Path = "/" });

                TempData["SuccessMessage"] = "Password changed successfully! Please login again.";
                return RedirectToAction("Login");
            }

            var errorJson = await response.Content.ReadAsStringAsync();
            ViewBag.ErrorMessage = ParseErrorMessage(errorJson, "Error changing password.");
            return View();
        }

        // ─── Helpers ──────────────────────────────────────────────────────

        private async Task HandleApiError(HttpResponseMessage response)
        {
            var errorJson = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, ParseErrorMessage(errorJson, "Server error."));
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
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                var msg = ParseErrorMessage(errorJson, "Google login failed");
                return BadRequest(new { message = msg });
            }

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
            Response.Cookies.Append("UserId", result?.AccountId.ToString() ?? "0", cookieOptions);
            Response.Cookies.Append("IsVerified", (result?.IsVerified ?? true) ? "true" : "false", cookieOptions);

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
            TempData.Keep("GiftToken");
            TempData.Keep("ReturnUrl");
            TempData.Keep("IsVerifyFlow");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string otp)
        {
            var email = TempData["VerifyEmail"]?.ToString() ?? TempData["ResetEmail"]?.ToString();
            var isEmailVerify = TempData["IsVerifyFlow"] != null;
            var giftToken = TempData["GiftToken"]?.ToString();
            var returnUrl = TempData["ReturnUrl"]?.ToString();

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
                ViewBag.ErrorMessage = "Incorrect or expired OTP";
                ViewBag.Email = email;

                if (isEmailVerify)
                {
                    TempData.Keep("VerifyEmail");
                    TempData.Keep("IsVerifyFlow");
                    TempData.Keep("GiftToken");
                    TempData.Keep("ReturnUrl");
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
                TempData.Remove("GiftToken");
                TempData.Remove("ReturnUrl");

                var cookieOptions = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddHours(24), Path = "/" };
                Response.Cookies.Append("IsVerified", "true", cookieOptions);

                TempData["SuccessMessage"] = "Email verification successful!";
                
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction("Login", new { returnUrl = returnUrl });
                }
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
                TempData["SuccessMessage"] = "Password changed successfully";
                return RedirectToAction("Login");
            }

            // ❗ FIX QUAN TRỌNG
            ViewBag.ErrorMessage = "Incorrect or expired OTP";

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
                TempData["ErrorMessage"] = "Could not send OTP";
                return RedirectToAction("Profile");
            }

            return RedirectToAction("VerifyOtp");
        }

        [HttpGet]
        public IActionResult VerifyRecommend()
        {
            var email = TempData["RecommendVerifyEmail"]?.ToString();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");
            ViewBag.Email = email;
            TempData.Keep("RecommendVerifyEmail");

            ViewBag.GiftToken = TempData["GiftToken"]?.ToString();
            ViewBag.ReturnUrl = TempData["ReturnUrl"]?.ToString();
            TempData.Keep("GiftToken");
            TempData.Keep("ReturnUrl");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SendRegisterOtp(string email, string? giftToken = null, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            var client = _httpClientFactory.CreateClient("BackendApi");
            var res = await client.PostAsync($"Auth/send-otp?email={email}", null);

            if (!res.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Could not send OTP";
                return RedirectToAction("Login");
            }

            TempData["VerifyEmail"] = email;
            TempData["IsVerifyFlow"] = true;
            if (!string.IsNullOrEmpty(giftToken)) TempData["GiftToken"] = giftToken;
            if (!string.IsNullOrEmpty(returnUrl)) TempData["ReturnUrl"] = returnUrl;

            return RedirectToAction("VerifyOtp");
        }
    }
}