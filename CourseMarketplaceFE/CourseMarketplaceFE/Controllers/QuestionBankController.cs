using Microsoft.AspNetCore.Authorization;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using CourseMarketplaceFE.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceFE.Controllers
{
    public class QuestionBankController : Controller
    {
        private readonly ApiClient _api;

        public QuestionBankController(ApiClient api)
        {
            _api = api;
        }

        public override async Task OnActionExecutionAsync(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context, Microsoft.AspNetCore.Mvc.Filters.ActionExecutionDelegate next)
        {
            if (!Request.Cookies.ContainsKey("AccessToken"))
            {
                context.Result = RedirectToAction("Login", "Account");
                return;
            }

            var statusCookie = Request.Cookies["InstructorApprovalStatus"];
            if (statusCookie != "Approved")
            {
                context.Result = RedirectToAction("Index", "Home");
                return;
            }

            await next();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> GetCourses(string? search = null, int page = 1, int pageSize = 6)
        {
            var url = $"courses/my-courses?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(search)) url += $"&search={Uri.EscapeDataString(search)}";

            var resp = await _api.GetAsync(url);
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<CourseMarketplaceFE.Models.Common.BaseApiResponse<CourseMarketplaceFE.Models.PagedResult<CourseMarketplaceFE.Models.CourseListViewModel>>>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                
                if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
                {
                    return PartialView("_CourseGridPartial", apiResponse.Data);
                }
            }
            return PartialView("_CourseGridPartial", new CourseMarketplaceFE.Models.PagedResult<CourseMarketplaceFE.Models.CourseListViewModel>());
        }

        [HttpGet]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> GetLessons(int courseId)
        {
            var resp = await _api.GetAsync($"courses/{courseId}");
            var json = await resp.Content.ReadAsStringAsync();
            HttpContext.Response.StatusCode = (int)resp.StatusCode;
            return Content(json, "application/json");
        }

        [HttpGet]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> CourseLessonsSummary(int courseId)
        {
            var resp = await _api.GetAsync($"questionbank/courses/{courseId}/lessons-summary");
            var json = await resp.Content.ReadAsStringAsync();
            HttpContext.Response.StatusCode = (int)resp.StatusCode;
            return Content(json, "application/json");
        }

        [HttpGet]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> GetQuestions(int lessonId)
        {
            var resp = await _api.GetAsync($"QuestionBank/lessons/{lessonId}/questions");
            var json = await resp.Content.ReadAsStringAsync();
            HttpContext.Response.StatusCode = (int)resp.StatusCode;
            // Return raw json from backend since JS expects it
            return Content(json, "application/json");
        }

        [HttpPost]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> CreateQuestion(int courseId, [FromBody] object request)
        {
            var resp = await _api.PostJsonAsync($"QuestionBank/courses/{courseId}/questions", request);
            var json = await resp.Content.ReadAsStringAsync();
            HttpContext.Response.StatusCode = (int)resp.StatusCode;
            return Content(json, "application/json");
        }

        [HttpPut]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> UpdateQuestion(int qId, [FromBody] object request)
        {
            var resp = await _api.PutJsonAsync($"QuestionBank/questions/{qId}", request);
            var json = await resp.Content.ReadAsStringAsync();
            HttpContext.Response.StatusCode = (int)resp.StatusCode;
            return Content(json, "application/json");
        }

        [HttpDelete]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> DeleteQuestion(int qId)
        {
            var resp = await _api.DeleteAsync($"QuestionBank/questions/{qId}");
            var json = await resp.Content.ReadAsStringAsync();
            HttpContext.Response.StatusCode = (int)resp.StatusCode;
            return Content(json, "application/json");
        }
    }
}
