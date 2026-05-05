using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceFE.Controllers;

public class ChatController : Controller
{
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
        return View("MockChat");
    }

    public IActionResult Learner()
    {
        ViewBag.Actor = "Learner";
        return View("MockChat");
    }

    public IActionResult Instructor()
    {
        ViewBag.Actor = "Instructor";
        return View("MockChat");
    }

    public IActionResult Admin()
    {
        ViewBag.Actor = "Admin";
        return View("MockChat");
    }
}
