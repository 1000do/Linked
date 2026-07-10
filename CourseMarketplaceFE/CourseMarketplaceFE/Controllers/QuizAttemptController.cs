using Microsoft.AspNetCore.Authorization;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceFE.Controllers;

public class QuizAttemptController : Controller
{
    private readonly ApiClient _api;
    private readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

    public QuizAttemptController(ApiClient api)
    {
        _api = api;
    }

    [HttpGet("QuizAttempt/Take/{quizId:int}")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> Take(int quizId)
    {
        if (!Request.Cookies.ContainsKey("AccessToken"))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var resp = await _api.GetAsync($"quiz-attempts/quiz/{quizId}");
            if (resp.IsSuccessStatusCode)
            {
                var content = await resp.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                if (root.TryGetProperty("data", out var dataEl))
                {
                    // Map to a view model or just pass JSON to ViewBag for Vue/React/VanillaJS rendering
                    ViewBag.QuizDataJson = dataEl.GetRawText();
                    return View(quizId); // we'll use a Razor view to render the UI
                }
            }
            else if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                ViewBag.Error = "You do not have access to this quiz. Make sure you are enrolled in a course that includes it.";
            }
            else
            {
                var errContent = await resp.Content.ReadAsStringAsync();
                ViewBag.Error = $"Failed to load quiz. Status: {resp.StatusCode}. Details: {errContent}";
            }
        }
        catch (Exception ex)
        {
            ViewBag.Error = "System error: " + ex.Message;
        }

        return View(quizId);
    }

    [HttpGet("QuizAttempt/Preview/{quizId:int}")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> Preview(int quizId)
    {
        if (!Request.Cookies.ContainsKey("AccessToken"))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            // Instructor fetches quiz details using instructor endpoint
            var resp = await _api.GetAsync($"quizzes/{quizId}");
            var poolResp = await _api.GetAsync($"quizzes/{quizId}/question-pool");
            if (resp.IsSuccessStatusCode && poolResp.IsSuccessStatusCode)
            {
                var content = await resp.Content.ReadAsStringAsync();
                var poolContent = await poolResp.Content.ReadAsStringAsync();
                
                var doc = JsonDocument.Parse(content);
                var poolDoc = JsonDocument.Parse(poolContent);

                if (doc.RootElement.TryGetProperty("data", out var dataEl) && 
                    poolDoc.RootElement.TryGetProperty("data", out var poolDataEl))
                {
                    var quizDict = JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(dataEl.GetRawText(), _jsonOpts);
                    var questionsList = JsonSerializer.Deserialize<System.Collections.Generic.List<object>>(poolDataEl.GetRawText(), _jsonOpts);
                    
                    if (quizDict != null)
                    {
                        quizDict["questions"] = questionsList ?? new System.Collections.Generic.List<object>();
                        ViewBag.QuizDataJson = JsonSerializer.Serialize(quizDict, _jsonOpts);
                        ViewBag.IsPreview = true;
                        return View("Take", quizId);
                    }
                }
            }
            else if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                ViewBag.Error = "You do not have access to preview this quiz.";
            }
            else
            {
                ViewBag.Error = "Failed to load quiz preview. Please try again later.";
            }
        }
        catch (Exception ex)
        {
            ViewBag.Error = "System error: " + ex.Message;
        }

        ViewBag.IsPreview = true;
        return View("Take", quizId);
    }

    [HttpPost("QuizAttempt/Submit")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> Submit([FromBody] JsonElement payload)
    {
        if (!Request.Cookies.ContainsKey("AccessToken"))
        {
            return Json(new { success = false, message = "Please log in first." });
        }

        try
        {
            var resp = await _api.PostJsonAsync("quiz-attempts", payload);
            var content = await resp.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (resp.IsSuccessStatusCode && root.TryGetProperty("data", out var dataEl))
            {
                return Json(new { success = true, data = dataEl });
            }
            else
            {
                var msg = root.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : "Submission failed.";
                return Json(new { success = false, message = msg });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "System error: " + ex.Message });
        }
    }

    [HttpGet("QuizAttempt/History/{quizId:int}")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> History(int quizId, int page = 1, int pageSize = 10)
    {
        if (!Request.Cookies.ContainsKey("AccessToken"))
        {
            return Json(new { success = false, message = "Please log in first." });
        }

        try
        {
            var resp = await _api.GetAsync($"quiz-attempts/quiz/{quizId}/history?page={page}&pageSize={pageSize}");
            var content = await resp.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (resp.IsSuccessStatusCode && root.TryGetProperty("data", out var dataEl))
            {
                return Json(new { success = true, data = dataEl });
            }
            return Json(new { success = false, message = "Failed to load history." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("QuizAttempt/Details/{attemptId:int}")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> Details(int attemptId)
    {
        if (!Request.Cookies.ContainsKey("AccessToken"))
        {
            return Json(new { success = false, message = "Please log in first." });
        }

        try
        {
            var resp = await _api.GetAsync($"quiz-attempts/{attemptId}/details");
            var content = await resp.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (resp.IsSuccessStatusCode && root.TryGetProperty("data", out var dataEl))
            {
                return Json(new { success = true, data = dataEl });
            }
            else
            {
                var msg = root.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : "Failed to load details.";
                return Json(new { success = false, message = msg });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "System error: " + ex.Message });
        }
    }

    [HttpGet("QuizAttempt/Review/{attemptId:int}")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> Review(int attemptId)
    {
        if (!Request.Cookies.ContainsKey("AccessToken"))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var resp = await _api.GetAsync($"quiz-attempts/{attemptId}/details");
            if (resp.IsSuccessStatusCode)
            {
                var content = await resp.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("data", out var dataEl))
                {
                    ViewBag.AttemptDataJson = dataEl.GetRawText();
                    return View();
                }
            }
            ViewBag.Error = "Failed to load attempt details.";
        }
        catch (Exception ex)
        {
            ViewBag.Error = "System error: " + ex.Message;
        }

        return View();
    }
}
