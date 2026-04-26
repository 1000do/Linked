using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IInstructorApprovalService
    {
        // Lấy danh sách giảng viên đang chờ duyệt
        Task<IEnumerable<InstructorApprovalDto>> GetPendingListAsync();

        // Xử lý Duyệt hoặc Từ chối
        Task<bool> ApproveOrRejectAsync(UpdateApprovalStatusDto dto);
    }
}
