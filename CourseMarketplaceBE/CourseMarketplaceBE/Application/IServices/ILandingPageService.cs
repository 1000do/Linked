using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface ILandingPageService
{
    Task<PlatformStatsDto> GetPlatformStatsAsync();
}
