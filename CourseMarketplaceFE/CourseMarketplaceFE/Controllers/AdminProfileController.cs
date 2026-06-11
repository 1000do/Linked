using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
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

        [HttpGet]
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

        [HttpGet]
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

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateAdminProfileViewModel model)
        {
            if (!Request.Cookies.ContainsKey("AccessToken")) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(model.DisplayName))
            {
                ModelState.AddModelError("DisplayName", "Display Name is required.");
                return View(model);
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
                // Sync display cookies
                var profileResponse = await _api.GetAsync("admin/profile");
                if (profileResponse.IsSuccessStatusCode)
                {
                    var profileContent = await profileResponse.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(profileContent);
                    var dataEl = doc.RootElement.GetProperty("data");
                    var updated = JsonSerializer.Deserialize<AdminProfileViewModel>(dataEl.GetRawText(), _jsonOptions);

                    var cookieOptions = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7), Path = "/" };
                    Response.Cookies.Append("UserName", updated?.FullName ?? updated?.DisplayName ?? "Admin", cookieOptions);
                    Response.Cookies.Append("AvatarUrl", updated?.AvatarUrl ?? "", cookieOptions);
                }

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Failed to update profile.";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmNewPassword)
        {
            if (!Request.Cookies.ContainsKey("AccessToken")) return RedirectToAction("Login", "Account");

            if (newPassword != confirmNewPassword)
            {
                TempData["ErrorMessage"] = "Password confirmation does not match.";
                return RedirectToAction("Index");
            }

            var response = await _api.PostJsonAsync("admin/profile/change-password", new
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            });

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Failed to change password. Make sure current password is correct.";
            return RedirectToAction("Index");
        }
    }
}
