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
        public async Task<IActionResult> GetCourses()
        {
            var resp = await _api.GetAsync("courses/my-courses?pageSize=100");
            var json = await resp.Content.ReadAsStringAsync();
            HttpContext.Response.StatusCode = (int)resp.StatusCode;
            return Content(json, "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> GetLessons(int courseId)
        {
            var resp = await _api.GetAsync($"courses/{courseId}");
            var json = await resp.Content.ReadAsStringAsync();
            HttpContext.Response.StatusCode = (int)resp.StatusCode;
            return Content(json, "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> CourseLessonsSummary(int courseId)
        {
            var resp = await _api.GetAsync($"questionbank/courses/{courseId}/lessons-summary");
            var json = await resp.Content.ReadAsStringAsync();
            HttpContext.Response.StatusCode = (int)resp.StatusCode;
            return Content(json, "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> GetQuestions(int lessonId)
        {
            var resp = await _api.GetAsync($"QuestionBank/lessons/{lessonId}/questions");
            var json = await resp.Content.ReadAsStringAsync();
            HttpContext.Response.StatusCode = (int)resp.StatusCode;
            // Return raw json from backend since JS expects it
            return Content(json, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuestion(int courseId, [FromBody] object request)
        {
            var resp = await _api.PostJsonAsync($"QuestionBank/courses/{courseId}/questions", request);
            var json = await resp.Content.ReadAsStringAsync();
            HttpContext.Response.StatusCode = (int)resp.StatusCode;
            return Content(json, "application/json");
        }

        [HttpPut]
        public async Task<IActionResult> UpdateQuestion(int qId, [FromBody] object request)
        {
            var resp = await _api.PutJsonAsync($"QuestionBank/questions/{qId}", request);
            var json = await resp.Content.ReadAsStringAsync();
            HttpContext.Response.StatusCode = (int)resp.StatusCode;
            return Content(json, "application/json");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteQuestion(int qId)
        {
            var resp = await _api.DeleteAsync($"QuestionBank/questions/{qId}");
            var json = await resp.Content.ReadAsStringAsync();
            HttpContext.Response.StatusCode = (int)resp.StatusCode;
            return Content(json, "application/json");
        }
    }
}
