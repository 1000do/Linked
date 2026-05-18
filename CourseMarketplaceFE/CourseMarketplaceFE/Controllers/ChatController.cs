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
            "admin" => "Admin",
            "staff" => "Admin", // Dùng chung layout _AdminLayout nhưng sẽ tự ẩn tính năng bên trong dựa trên role thực tế
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
        // Guard: Chỉ giảng viên đã duyệt mới được vào chat của Instructor
        var approvalStatus = Request.Cookies["InstructorApprovalStatus"];
        if (approvalStatus != "Approved") 
            return RedirectToAction("ApplicationStatus", "Instructor");

        ViewBag.Actor = "Instructor";
        return View("Index");
    }

    public IActionResult Admin()
    {
        ViewBag.Actor = "Admin";
        return View("Index");
    }
}
