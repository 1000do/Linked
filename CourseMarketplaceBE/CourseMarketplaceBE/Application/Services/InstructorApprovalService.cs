using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services
{
    public class InstructorApprovalService : IInstructorApprovalService
    {
        private readonly IInstructorRepository _repo;
        private readonly INotificationService _notiService; // Tận dụng service thông báo bạn đã có

        public InstructorApprovalService(IInstructorRepository repo, INotificationService notiService)
        {
            _repo = repo;
            _notiService = notiService;
        }

        public async Task<IEnumerable<InstructorApprovalDto>> GetPendingListAsync()
        {
            var list = await _repo.GetPendingInstructorsAsync();
            return list.Select(i => new InstructorApprovalDto
            {
                InstructorId = i.InstructorId,
                FullName = i.InstructorNavigation.FullName,
                // ĐỔI TÊN Ở ĐÂY:
                Email = i.InstructorNavigation.UserNavigation.Email,
                ProfessionalTitle = i.ProfessionalTitle,
                DocumentUrl = i.DocumentUrl,
                ApprovalStatus = i.ApprovalStatus,
                ExpertiseCategories = i.ExpertiseCategories
            });
        }

        public async Task<bool> ApproveOrRejectAsync(UpdateApprovalStatusDto dto)
        {
            var instructor = await _repo.GetByIdAsync(dto.InstructorId);
            if (instructor == null) return false;

            instructor.ApprovalStatus = dto.Status;
            _repo.Update(instructor);
            await _repo.SaveChangesAsync();

            // Gửi thông báo cho người dùng (Sử dụng NotificationService bạn đã làm)
            string message = dto.Status == "Approved"
                ? "Chúc mừng! Đơn đăng ký giảng viên của bạn đã được duyệt."
                : "Rất tiếc, đơn đăng ký giảng viên của bạn không được chấp nhận.";

            await _notiService.SendNotificationAsync(instructor.InstructorId, "Kết quả xét duyệt", message, "/Instructor/Dashboard");

            return true;
        }
    }
}
