using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceFE.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminAvatarFrameController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
