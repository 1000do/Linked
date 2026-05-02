using System.Threading.Tasks;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LandingPageController : ControllerBase
{
    private readonly ILandingPageService _landingPageService;

    public LandingPageController(ILandingPageService landingPageService)
    {
        _landingPageService = landingPageService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _landingPageService.GetPlatformStatsAsync();
        return Ok(stats);
    }
}
