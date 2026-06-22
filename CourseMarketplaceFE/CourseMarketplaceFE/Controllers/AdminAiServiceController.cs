using Microsoft.AspNetCore.Mvc;
using CourseMarketplaceFE.Models;
using System;
using System.Collections.Generic;

using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using CourseMarketplaceFE.Helpers;
using Microsoft.AspNetCore.Authorization;

using CourseMarketplaceFE.Models.Common;

namespace CourseMarketplaceFE.Controllers;

[Authorize(Roles = "admin")]
public class AdminAiServiceController : Controller
{
    private readonly ApiClient _api;
    private readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

    public AdminAiServiceController(ApiClient api)
    {
        _api = api;
    }
    public async Task<IActionResult> Index(string tab = "models", string subtab = "course", int modelPage = 1, int coursePage = 1, int cReviewPage = 1, int lReviewPage = 1)
    {
        ViewBag.ActiveTab = tab;
        ViewBag.ActiveSubtab = subtab;
        ViewBag.ModelPage = modelPage;
        ViewBag.CoursePage = coursePage;
        ViewBag.CReviewPage = cReviewPage;
        ViewBag.LReviewPage = lReviewPage;
        
        var model = new AdminAiServicePageViewModel();
        
        try
        {
            var modelsTask = _api.GetAsync($"admin/ai-service/models?page={modelPage}&pageSize=10");
            var allModelsTask = _api.GetAsync("admin/ai-service/models/all");
            var configTask = _api.GetAsync("admin/ai-service/configs");
            var courseLogTask = _api.GetAsync($"admin/ai-service/logs/course?page={coursePage}&pageSize=10");
            var cReviewLogTask = _api.GetAsync($"admin/ai-service/logs/course-review?page={cReviewPage}&pageSize=10");
            var lReviewLogTask = _api.GetAsync($"admin/ai-service/logs/lesson-review?page={lReviewPage}&pageSize=10");

            await Task.WhenAll(modelsTask, allModelsTask, configTask, courseLogTask, cReviewLogTask, lReviewLogTask);

            var modelsResp = await modelsTask;
            if (modelsResp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");

            if (modelsResp.IsSuccessStatusCode) {
                 var json = await modelsResp.Content.ReadAsStringAsync();
                 var parsed = JsonSerializer.Deserialize<BaseApiResponse<PagedResult<AiModelViewModel>>>(json, _jsonOpts);
                 if (parsed?.Data != null) {
                     model.AiModels = parsed.Data.Items;
                     ViewBag.ModelTotalPages = parsed.Data.TotalPages;
                 }
            }

            var allModelsResp = await allModelsTask;
            if (allModelsResp.IsSuccessStatusCode) {
                 var json = await allModelsResp.Content.ReadAsStringAsync();
                 var parsed = JsonSerializer.Deserialize<BaseApiResponse<List<AiModelViewModel>>>(json, _jsonOpts);
                 if (parsed?.Data != null) model.AllActiveModels = parsed.Data.Where(m => m.ModelStatus == "active").ToList();
            }

            var configResp = await configTask;
            if (configResp.IsSuccessStatusCode) {
                 var json = await configResp.Content.ReadAsStringAsync();
                 var parsed = JsonSerializer.Deserialize<BaseApiResponse<AiConfigurationViewModel>>(json, _jsonOpts);
                 if (parsed?.Data != null) model.Config = parsed.Data;
            }

            var courseLogResp = await courseLogTask;
            if (courseLogResp.IsSuccessStatusCode) {
                 var json = await courseLogResp.Content.ReadAsStringAsync();
                 var parsed = JsonSerializer.Deserialize<BaseApiResponse<PagedResult<CourseModerationLogViewModel>>>(json, _jsonOpts);
                 if (parsed?.Data != null) {
                     model.CourseLogs = parsed.Data.Items;
                     ViewBag.CourseTotalPages = parsed.Data.TotalPages;
                 }
            }

            var cReviewLogResp = await cReviewLogTask;
            if (cReviewLogResp.IsSuccessStatusCode) {
                 var json = await cReviewLogResp.Content.ReadAsStringAsync();
                 var parsed = JsonSerializer.Deserialize<BaseApiResponse<PagedResult<ReviewModerationLogViewModel>>>(json, _jsonOpts);
                 if (parsed?.Data != null) {
                     model.CourseReviewLogs = parsed.Data.Items;
                     ViewBag.CReviewTotalPages = parsed.Data.TotalPages;
                 }
            }

            var lReviewLogResp = await lReviewLogTask;
            if (lReviewLogResp.IsSuccessStatusCode) {
                 var json = await lReviewLogResp.Content.ReadAsStringAsync();
                 var parsed = JsonSerializer.Deserialize<BaseApiResponse<PagedResult<ReviewModerationLogViewModel>>>(json, _jsonOpts);
                 if (parsed?.Data != null) {
                     model.LessonReviewLogs = parsed.Data.Items;
                     ViewBag.LReviewTotalPages = parsed.Data.TotalPages;
                 }
            }
        }
        catch { }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetModelsPartial(int page = 1)
    {
        var model = new AdminAiServicePageViewModel();
        ViewBag.ModelPage = page;
        
        try {
            var resp = await _api.GetAsync($"admin/ai-service/models?page={page}&pageSize=10");
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");

            if (resp.IsSuccessStatusCode) {
                var json = await resp.Content.ReadAsStringAsync();
                var parsed = JsonSerializer.Deserialize<BaseApiResponse<PagedResult<AiModelViewModel>>>(json, _jsonOpts);
                if (parsed?.Data != null) {
                    model.AiModels = parsed.Data.Items;
                    ViewBag.ModelTotalPages = parsed.Data.TotalPages;
                }
            }
        } catch { }

        return PartialView("_ModelsTablePartial", model);
    }

    [HttpGet]
    public async Task<IActionResult> GetCourseLogsPartial(int page = 1)
    {
        var model = new AdminAiServicePageViewModel();
        ViewBag.CoursePage = page;
        
        try {
            var resp = await _api.GetAsync($"admin/ai-service/logs/course?page={page}&pageSize=10");
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");

            if (resp.IsSuccessStatusCode) {
                var json = await resp.Content.ReadAsStringAsync();
                var parsed = JsonSerializer.Deserialize<BaseApiResponse<PagedResult<CourseModerationLogViewModel>>>(json, _jsonOpts);
                if (parsed?.Data != null) {
                    model.CourseLogs = parsed.Data.Items;
                    ViewBag.CourseTotalPages = parsed.Data.TotalPages;
                }
            }
        } catch { }

        return PartialView("_CourseLogsTablePartial", model);
    }

    [HttpGet]
    public async Task<IActionResult> GetCourseReviewLogsPartial(int page = 1)
    {
        var model = new AdminAiServicePageViewModel();
        ViewBag.CReviewPage = page;
        
        try {
            var resp = await _api.GetAsync($"admin/ai-service/logs/course-review?page={page}&pageSize=10");
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");

            if (resp.IsSuccessStatusCode) {
                var json = await resp.Content.ReadAsStringAsync();
                var parsed = JsonSerializer.Deserialize<BaseApiResponse<PagedResult<ReviewModerationLogViewModel>>>(json, _jsonOpts);
                if (parsed?.Data != null) {
                    model.CourseReviewLogs = parsed.Data.Items;
                    ViewBag.CReviewTotalPages = parsed.Data.TotalPages;
                }
            }
        } catch { }

        return PartialView("_CourseReviewLogsTablePartial", model);
    }

    [HttpGet]
    public async Task<IActionResult> GetLessonReviewLogsPartial(int page = 1)
    {
        var model = new AdminAiServicePageViewModel();
        ViewBag.LReviewPage = page;
        
        try {
            var resp = await _api.GetAsync($"admin/ai-service/logs/lesson-review?page={page}&pageSize=10");
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");

            if (resp.IsSuccessStatusCode) {
                var json = await resp.Content.ReadAsStringAsync();
                var parsed = JsonSerializer.Deserialize<BaseApiResponse<PagedResult<ReviewModerationLogViewModel>>>(json, _jsonOpts);
                if (parsed?.Data != null) {
                    model.LessonReviewLogs = parsed.Data.Items;
                    ViewBag.LReviewTotalPages = parsed.Data.TotalPages;
                }
            }
        } catch { }

        return PartialView("_LessonReviewLogsTablePartial", model);
    }

    public async Task<IActionResult> CourseModerationLogDetail(int id)
    {
        try {
            var resp = await _api.GetAsync($"admin/ai-service/logs/course/{id}");
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");

            if (!resp.IsSuccessStatusCode) {
                TempData["Error"] = "Log not found or backend is unreachable.";
                return RedirectToAction("Index", new { tab = "logs", subtab = "course" });
            }
            
            var json = await resp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<BaseApiResponse<CourseModerationLogViewModel>>(json, _jsonOpts);
            if (parsed?.Data == null) {
                TempData["Error"] = "Log data is corrupted or not found.";
                return RedirectToAction("Index", new { tab = "logs", subtab = "course" });
            }
            
            return View(parsed.Data);
        } catch {
            TempData["Error"] = "Failed to fetch log details due to a network error.";
            return RedirectToAction("Index", new { tab = "logs", subtab = "course" });
        }
    }

    public async Task<IActionResult> ReviewModerationLogDetail(int id, string type = "course")
    {
        ViewBag.ReviewType = type;
        var url = type == "course" ? $"admin/ai-service/logs/course-review/{id}" : $"admin/ai-service/logs/lesson-review/{id}";
        
        try {
            var resp = await _api.GetAsync(url);
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");

            if (!resp.IsSuccessStatusCode) {
                TempData["Error"] = "Log not found or backend is unreachable.";
                return RedirectToAction("Index", new { tab = "logs", subtab = type == "course" ? "course_review" : "lesson_review" });
            }
            
            var json = await resp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<BaseApiResponse<ReviewModerationLogViewModel>>(json, _jsonOpts);
            if (parsed?.Data == null) {
                TempData["Error"] = "Log data is corrupted or not found.";
                return RedirectToAction("Index", new { tab = "logs", subtab = type == "course" ? "course_review" : "lesson_review" });
            }
            
            return View(parsed.Data);
        } catch {
            TempData["Error"] = "Failed to fetch log details due to a network error.";
            return RedirectToAction("Index", new { tab = "logs", subtab = type == "course" ? "course_review" : "lesson_review" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddModel([FromBody] CreateAiModelRequest req)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { Message = "Validation failed", Errors = errors });
        }

        var res = await _api.PostJsonAsync("admin/ai-service/models", req);
        if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return Unauthorized(new { Message = "Unauthorized access." });

        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadAsStringAsync();
            return BadRequest(new { Message = "Failed to add AI Model", Details = err });
        }
        return Ok(new { Message = "AI Model added successfully" });
    }

    [HttpPost]
    public async Task<IActionResult> EditModel([FromQuery] int id, [FromBody] UpdateAiModelRequest req)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { Message = "Validation failed", Errors = errors });
        }

        var res = await _api.PutJsonAsync($"admin/ai-service/models/{id}", req);
        if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return Unauthorized(new { Message = "Unauthorized access." });

        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadAsStringAsync();
            return BadRequest(new { Message = "Failed to edit AI Model", Details = err });
        }
        return Ok(new { Message = "AI Model updated successfully" });
    }

    [HttpPatch]
    public async Task<IActionResult> ToggleModelStatus([FromQuery] int id)
    {
        var res = await _api.PatchAsync($"admin/ai-service/models/{id}/status");
        if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return Unauthorized(new { Message = "Unauthorized access." });

        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadAsStringAsync();
            return BadRequest(new { Message = "Failed to toggle status", Details = err });
        }
        return Ok(new { Message = "Model status toggled successfully" });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateThresholds([FromBody] UpdateThresholdsRequest req)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { Message = "Validation failed", Errors = errors });
        }

        var res = await _api.PutJsonAsync("admin/ai-service/configs/thresholds", req);
        if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return Unauthorized(new { Message = "Unauthorized access." });

        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadAsStringAsync();
            return BadRequest(new { Message = "Failed to update thresholds", Details = err });
        }
        return Ok(new { Message = "Thresholds updated successfully" });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateIntegration([FromBody] UpdateIntegrationRequest req)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { Message = "Validation failed", Errors = errors });
        }

        var res = await _api.PutJsonAsync("admin/ai-service/configs/integrations", req);
        if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return Unauthorized(new { Message = "Unauthorized access." });

        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadAsStringAsync();
            return BadRequest(new { Message = "Failed to update integration mapping", Details = err });
        }
        return Ok(new { Message = "Integration mapping updated successfully" });
    }
}
