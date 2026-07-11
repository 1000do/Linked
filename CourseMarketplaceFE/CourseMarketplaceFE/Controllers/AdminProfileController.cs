using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CourseMarketplaceFE.Controllers
{
    public class AdminProfileController : Controller
    {
        private readonly ApiClient _api;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public AdminProfileController(ApiClient api)
        {
            _api = api;
        }

        // UC-75: View Profile (For Manager) — Staff và Admin
        [HttpGet]
        [Authorize(Roles = "staff,admin")]
        public async Task<IActionResult> Index()
        {
            if (!Request.Cookies.ContainsKey("AccessToken")) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync("admin/profile");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var dataEl = doc.RootElement.GetProperty("data");
                var profile = JsonSerializer.Deserialize<AdminProfileViewModel>(dataEl.GetRawText(), _jsonOptions);
                return View(profile);
            }
            return RedirectToAction("Login", "Account");
        }

        // UC-76: Edit Profile (For Manager GET) — Staff và Admin
        [HttpGet]
        [Authorize(Roles = "staff,admin")]
        public async Task<IActionResult> Edit()
        {
            if (!Request.Cookies.ContainsKey("AccessToken")) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync("admin/profile");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var dataEl = doc.RootElement.GetProperty("data");
                var profile = JsonSerializer.Deserialize<AdminProfileViewModel>(dataEl.GetRawText(), _jsonOptions);
                
                var model = new UpdateAdminProfileViewModel
                {
                    DisplayName = profile?.DisplayName ?? "",
                    FullName = profile?.FullName,
                    PhoneNumber = profile?.PhoneNumber,
                    Bio = profile?.Bio
                };
                
                ViewBag.AvatarUrl = profile?.AvatarUrl;
                return View(model);
            }
            return RedirectToAction("Index");
        }

        // UC-76: Edit Profile (For Manager POST) — Staff và Admin
        [HttpPost]
        [Authorize(Roles = "staff,admin")]
        public async Task<IActionResult> Edit(UpdateAdminProfileViewModel model)
        {
            if (!Request.Cookies.ContainsKey("AccessToken")) 
                return Json(new { success = false, message = "Session expired. Please login again." });

            if (string.IsNullOrWhiteSpace(model.DisplayName))
            {
                return Json(new { success = false, message = "Display Name is required." });
            }

            if (!string.IsNullOrWhiteSpace(model.PhoneNumber) && model.PhoneNumber.Length > 50)
            {
                return Json(new { success = false, message = "Contact number cannot exceed 50 characters." });
            }

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.DisplayName ?? ""), "DisplayName");
            content.Add(new StringContent(model.FullName ?? ""), "FullName");
            content.Add(new StringContent(model.PhoneNumber ?? ""), "PhoneNumber");
            content.Add(new StringContent(model.Bio ?? ""), "Bio");

            if (model.AvatarFile != null)
            {
                var fileContent = new StreamContent(model.AvatarFile.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.AvatarFile.ContentType);
                content.Add(fileContent, "AvatarFile", model.AvatarFile.FileName);
            }

            var response = await _api.PutAsync("admin/profile/update", content);
            if (response.IsSuccessStatusCode)
            {
                string? avatarUrl = null;
                string? userName = null;
                // Đồng bộ cookie hiển thị
                var profileResponse = await _api.GetAsync("admin/profile");
                if (profileResponse.IsSuccessStatusCode)
                {
                    var profileContent = await profileResponse.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(profileContent);
                    var dataEl = doc.RootElement.GetProperty("data");
                    var updated = JsonSerializer.Deserialize<AdminProfileViewModel>(dataEl.GetRawText(), _jsonOptions);

                    var cookieOptions = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7), Path = "/" };
                    userName = updated?.FullName ?? updated?.DisplayName ?? "Admin";
                    Response.Cookies.Append("UserName", userName, cookieOptions);
                    Response.Cookies.Append("AvatarUrl", updated?.AvatarUrl ?? "", cookieOptions);
                    avatarUrl = updated?.AvatarUrl;
                }

                return Json(new { success = true, message = "Profile updated successfully!", avatarUrl, userName });
            }

            var errContent = await response.Content.ReadAsStringAsync();
            string errMsg = "Failed to update profile.";
            try
            {
                using var doc = JsonDocument.Parse(errContent);
                if (doc.RootElement.TryGetProperty("message", out var msgEl))
                {
                    errMsg = msgEl.GetString() ?? errMsg;
                }
            }
            catch { }

            return Json(new { success = false, message = errMsg });
        }

        // UC-75/76: Change Password (For Manager) — Staff và Admin
        [HttpPost]
        [Authorize(Roles = "staff,admin")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordFERequest model)
        {
            if (!Request.Cookies.ContainsKey("AccessToken")) 
                return Json(new { success = false, message = "Session expired. Please login again." });

            if (model == null || string.IsNullOrWhiteSpace(model.CurrentPassword) || string.IsNullOrWhiteSpace(model.NewPassword))
            {
                return Json(new { success = false, message = "All password fields are required." });
            }

            if (model.NewPassword.Length < 6)
            {
                return Json(new { success = false, message = "New password must be at least 6 characters long." });
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                return Json(new { success = false, message = "Password confirmation does not match." });
            }

            var response = await _api.PostJsonAsync("admin/profile/change-password", new
            {
                CurrentPassword = model.CurrentPassword,
                NewPassword = model.NewPassword
            });

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Password changed successfully!" });
            }

            var errContent = await response.Content.ReadAsStringAsync();
            string errMsg = "Failed to change password. Make sure current password is correct.";
            try
            {
                using var doc = JsonDocument.Parse(errContent);
                if (doc.RootElement.TryGetProperty("message", out var msgEl))
                {
                    errMsg = msgEl.GetString() ?? errMsg;
                }
            }
            catch { }

            return Json(new { success = false, message = errMsg });
        }
    }
}
