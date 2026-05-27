using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services
{
    public class UserReportModerationService : IUserReportModerationService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;

        public UserReportModerationService(IChatRepository chatRepository, IUserRepository userRepository)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
        }

        public async Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<UserReportModerationDto>> GetAllReportsAsync(int page = 1, int pageSize = 10)
        {
            var (reports, totalCount) = await _chatRepository.GetAllReportsAsync(page, pageSize);
            var items = reports.Select(r => new UserReportModerationDto
            {
                ReportId = r.ReportId,
                ReporterName = r.Reporter?.Email ?? "Unknown",
                Reason = r.Reason,
                Description = r.Description,
                ChatId = r.ChatId,
                Status = r.UserReportsStatus,
                CreatedAt = r.CreatedAt
            }).ToList();
            
            return new CourseMarketplaceBE.Application.DTOs.Common.PagedResult<UserReportModerationDto>(items, totalCount, page, pageSize);
        }

        public async Task<bool> ResolveReportAsync(ResolveReportDto dto, int resolverId)
        {
            var report = await _chatRepository.GetReportByIdAsync(dto.ReportId);
            if (report == null) return false;

            if (report.UserReportsStatus == "escalated")
            {
                var resolverRole = await _userRepository.GetRoleByAccountIdAsync(resolverId);
                if (resolverRole != "admin")
                {
                    throw new UnauthorizedAccessException("Only Admins have permission to resolve escalated reports.");
                }
            }

            report.UserReportsStatus = dto.Status;
            report.ResolutionNote = dto.ResolutionNote;
            report.ResolverId = resolverId;
            report.ResolvedAt = DateTime.Now;

            await _chatRepository.SaveChangesAsync();
            return true;
        }
    }
}
