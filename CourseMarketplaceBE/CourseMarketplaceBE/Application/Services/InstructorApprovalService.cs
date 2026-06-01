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

        public async Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<InstructorApprovalDto>> GetPendingListAsync(int page = 1, int pageSize = 10)
        {
            var (instructors, totalCount) = await _repo.GetPendingInstructorsAsync(page, pageSize);
            var items = instructors.Select(i => new InstructorApprovalDto
            {
                InstructorId = i.InstructorId,
                FullName = i.InstructorNavigation?.FullName ?? "N/A",
                Email = i.InstructorNavigation?.UserNavigation?.Email ?? "N/A",
                AvatarUrl = i.InstructorNavigation?.UserNavigation?.AvatarUrl,
                ProfessionalTitle = i.ProfessionalTitle,
                DocumentUrl = i.DocumentUrl,
                LinkedInUrl = i.LinkedinUrl,
                ApprovalStatus = i.ApprovalStatus,
                ExpertiseCategories = i.ExpertiseCategories
            }).ToList();
            
            return new CourseMarketplaceBE.Application.DTOs.Common.PagedResult<InstructorApprovalDto>(items, totalCount, page, pageSize);
        }

        public async Task<bool> ApproveOrRejectAsync(UpdateApprovalStatusDto dto)
        {
            var instructor = await _repo.GetByIdAsync(dto.InstructorId);
            if (instructor == null) return false;

            instructor.ApprovalStatus = dto.Status;
            _repo.Update(instructor);
            int numberOfRowsAffected = await _repo.SaveChangesAsync();
            if (numberOfRowsAffected <= 0)
                throw new InvalidOperationException("Failed to save changes");

            // Gửi thông báo cho người dùng (Sử dụng NotificationService bạn đã làm)
            string message = dto.Status == "Approved"
                ? "Congratulations! Your instructor application has been approved."
                : "Unfortunately, your instructor application was not accepted.";

            string link = dto.Status == "Approved" ? "/InstructorCourse/Create" : "/Instructor/Dashboard";
            await _notiService.SendNotificationAsync(instructor.InstructorId, "Application Result", message, link);

            return true;
        }
    }
}
