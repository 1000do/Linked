using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace CourseMarketplaceFE.Controllers;

public class ChatController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ApiClient _api;

    public ChatController(IConfiguration configuration, ApiClient api)
    {
        _configuration = configuration;
        _api = api;
    }

    private string GetActorFromCookie()
    {
        var role = HttpContext.Request.Cookies["UserRole"] ?? "";
        return role.ToLower() switch
        {
            "admin" => "Admin",
            "staff" => "Admin", // Dùng chung layout _AdminLayout nhưng sẽ tự ẩn tính năng bên trong dựa trên role thực tế
            "instructor" => "Instructor",
            _ => "Learner"
        };
    }

    [Authorize(Roles = "user,instructor,admin,staff")]
    public IActionResult Index()
    {
        ViewBag.Actor = GetActorFromCookie();
        ViewBag.EnableAttachments = _configuration.GetValue<bool>("ChatSettings:EnableAttachments");
        return View("Index");
    }

    [Authorize(Roles = "user,instructor")]
    public IActionResult Learner()
    {
        ViewBag.Actor = "Learner";
        ViewBag.EnableAttachments = _configuration.GetValue<bool>("ChatSettings:EnableAttachments");
        return View("Index");
    }

    [Authorize(Roles = "instructor")]
    public IActionResult Instructor()
    {
        // Guard: Chỉ giảng viên đã duyệt mới được vào chat của Instructor
        var approvalStatus = Request.Cookies["InstructorApprovalStatus"];
        if (approvalStatus != "Approved") 
            return RedirectToAction("ApplicationStatus", "Instructor");

        ViewBag.Actor = "Instructor";
        ViewBag.EnableAttachments = _configuration.GetValue<bool>("ChatSettings:EnableAttachments");
        return View("Index");
    }

    [Authorize(Roles = "admin,staff")]
    public IActionResult Admin()
    {
        ViewBag.Actor = "Admin";
        ViewBag.EnableAttachments = _configuration.GetValue<bool>("ChatSettings:EnableAttachments");
        return View("Index");
    }

    // --- BFF Proxy Methods --- //

    [HttpGet]
    [Authorize(Roles = "user,instructor,admin,staff")]
    public async Task<IActionResult> GetList()
    {
        var response = await _api.GetAsync("chat/list");
        if (response.IsSuccessStatusCode)
            return Content(await response.Content.ReadAsStringAsync(), "application/json");
        return BadRequest();
    }

    [HttpGet]
    [Authorize(Roles = "user,instructor,admin,staff")]
    public async Task<IActionResult> Search(string q)
    {
        var response = await _api.GetAsync($"chat/search?q={Uri.EscapeDataString(q ?? "")}");
        if (response.IsSuccessStatusCode)
            return Content(await response.Content.ReadAsStringAsync(), "application/json");
        return BadRequest();
    }

    [HttpGet]
    [Authorize(Roles = "user,instructor,admin,staff")]
    public async Task<IActionResult> GetHistory(int roomId)
    {
        var response = await _api.GetAsync($"chat/history?roomId={roomId}");
        if (response.IsSuccessStatusCode)
            return Content(await response.Content.ReadAsStringAsync(), "application/json");
            
        var errorContent = await response.Content.ReadAsStringAsync();
        return StatusCode((int)response.StatusCode, errorContent);
    }

    [HttpGet]
    [Authorize(Roles = "user,instructor,admin,staff")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var response = await _api.GetAsync("chat/unread-count");
        if (response.IsSuccessStatusCode)
            return Content(await response.Content.ReadAsStringAsync(), "application/json");
        return BadRequest();
    }

    [HttpGet]
    [Authorize(Roles = "user,instructor,admin,staff")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var response = await _api.GetAsync("chat/pending-requests");
        if (response.IsSuccessStatusCode)
            return Content(await response.Content.ReadAsStringAsync(), "application/json");
        return BadRequest();
    }

    [HttpGet]
    [Authorize(Roles = "admin,staff")]
    public async Task<IActionResult> GetAdminAccount()
    {
        var response = await _api.GetAsync("chat/admin-account");
        if (response.IsSuccessStatusCode)
            return Content(await response.Content.ReadAsStringAsync(), "application/json");
        return NotFound();
    }

    [HttpPost]
    [Authorize(Roles = "user,instructor")]
    public async Task<IActionResult> RequestSupport([FromBody] dynamic dto)
    {
        var response = await _api.PostJsonAsync("chat/request-support", dto);
        var json = await response.Content.ReadAsStringAsync();
        return StatusCode((int)response.StatusCode, json);
    }

    [HttpPost]
    [Authorize(Roles = "admin,staff")]
    public async Task<IActionResult> AcceptSupport(string ticketId)
    {
        var response = await _api.PostAsync($"chat/accept-support/{ticketId}");
        var json = await response.Content.ReadAsStringAsync();
        return StatusCode((int)response.StatusCode, json);
    }

    [HttpPost]
    [Authorize(Roles = "user,instructor,admin,staff")]
    public async Task<IActionResult> CreateDirectChat([FromBody] dynamic dto)
    {
        var response = await _api.PostJsonAsync("chat/create", dto);
        var json = await response.Content.ReadAsStringAsync();
        return StatusCode((int)response.StatusCode, json);
    }

    [HttpDelete]
    [Authorize(Roles = "user,instructor,admin,staff")]
    public async Task<IActionResult> ClearHistory(int chatId)
    {
        var response = await _api.DeleteAsync($"chat/{chatId}/clear");
        if (response.IsSuccessStatusCode) return Ok();
        return BadRequest();
    }

    [HttpPost]
    [Authorize(Roles = "user,instructor,admin,staff")]
    public async Task<IActionResult> UploadAttachment(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File is empty");

        using var content = new MultipartFormDataContent();
        using var fileStream = file.OpenReadStream();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.FileName);

        var token = HttpContext.Request.Cookies["AccessToken"];
        var httpClient = new System.Net.Http.HttpClient();
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var baseUrl = _configuration.GetValue<string>("BackendApiUrl") 
            ?? _configuration.GetValue<string>("ApiSettings:BaseUrl") 
            ?? "http://localhost:5207/api/";
            
        // Ensure trailing slash
        if (!baseUrl.EndsWith("/")) baseUrl += "/";
            
        var response = await httpClient.PostAsync(baseUrl + "chatattachment/upload", content);

        if (response.IsSuccessStatusCode)
            return Content(await response.Content.ReadAsStringAsync(), "application/json");
        return BadRequest("Upload failed");
    }
}
