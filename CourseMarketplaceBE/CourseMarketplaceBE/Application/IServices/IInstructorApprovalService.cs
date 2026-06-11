using CourseMarketplaceBE.Application.DTOs;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IInstructorApprovalService
    {
        Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<InstructorApprovalDto>> GetPendingListAsync(int page = 1, int pageSize = 10);
        Task<bool> ApproveOrRejectAsync(UpdateApprovalStatusDto dto);
        Task<InstructorApprovalDto?> GetDetailAsync(int id);
    }
}
