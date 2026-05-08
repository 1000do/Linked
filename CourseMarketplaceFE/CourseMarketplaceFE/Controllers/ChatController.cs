using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceFE.Controllers;

public class ChatController : Controller
{
    private readonly IConfiguration _configuration;

    public ChatController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string GetActorFromCookie()
    {
        var role = HttpContext.Request.Cookies["UserRole"] ?? "";
        return role.ToLower() switch
        {
            "manager" => "Admin",
            "instructor" => "Instructor",
            _ => "Learner"
        };
    }

    public IActionResult Index()
    {
        ViewBag.Actor = GetActorFromCookie();
        ViewBag.EnableAttachments = _configuration.GetValue<bool>("ChatSettings:EnableAttachments");
        return View("Index");
    }

    public IActionResult Learner()
    {
        ViewBag.Actor = "Learner";
        return View("Index");
    }

    public IActionResult Instructor()
    {
        ViewBag.Actor = "Instructor";
        return View("Index");
    }

    public IActionResult Admin()
    {
        ViewBag.Actor = "Admin";
        return View("Index");
    }
}
