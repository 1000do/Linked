using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceFE.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}