using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IUserReportModerationService
    {
        Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<UserReportModerationDto>> GetAllReportsAsync(int page = 1, int pageSize = 10);
        Task<bool> ResolveReportAsync(ResolveReportDto dto, int resolverId);
    }
}
