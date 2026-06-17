using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services;

public class AiModerationLogService : IAiModerationLogService
{
    private readonly ICourseAiUsageLogRepository _courseLogRepo;
    private readonly ICourseReviewModerationLogRepository _courseReviewLogRepo;
    private readonly ILessonReviewModerationLogRepository _lessonReviewLogRepo;
    private readonly IMapper _mapper;

    public AiModerationLogService(
        ICourseAiUsageLogRepository courseLogRepo,
        ICourseReviewModerationLogRepository courseReviewLogRepo,
        ILessonReviewModerationLogRepository lessonReviewLogRepo,
        IMapper mapper)
    {
        _courseLogRepo = courseLogRepo;
        _courseReviewLogRepo = courseReviewLogRepo;
        _lessonReviewLogRepo = lessonReviewLogRepo;
        _mapper = mapper;
    }

    public async Task<PagedResult<CourseModerationLogAdminDto>> GetCourseModerationLogsAsync(PagedRequestDto req)
    {
        if (req.Page <= 0) req.Page = 1;
        if (req.PageSize <= 0) req.PageSize = 10;

        var (items, totalCount) = await _courseLogRepo.GetPagedAdminAsync(req.Page, req.PageSize);
        if (items == null || items.Count == 0) throw new KeyNotFoundException("No course moderation logs found.");
        var dtos =  _mapper.Map<List<CourseModerationLogAdminDto>>(items);
        return new PagedResult<CourseModerationLogAdminDto>(dtos, totalCount, req.Page, req.PageSize);
    }

    public async Task<CourseModerationLogAdminDto?> GetCourseModerationLogDetailAsync(int logId)
    {
        var entity = await _courseLogRepo.GetAdminDetailByIdAsync(logId);
        if (entity == null) throw new KeyNotFoundException("Log not found.");
        return _mapper.Map<CourseModerationLogAdminDto>(entity);
    }

    public async Task<PagedResult<ReviewModerationLogAdminDto>> GetCourseReviewModerationLogsAsync(PagedRequestDto req)
    {
        if (req.Page <= 0) req.Page = 1;
        if (req.PageSize <= 0) req.PageSize = 10;

        var (items, totalCount) = await _courseReviewLogRepo.GetPagedAdminAsync(req.Page, req.PageSize);
        if (items == null || items.Count == 0) throw new KeyNotFoundException("No course review moderation logs found.");
        var dtos = _mapper.Map<List<ReviewModerationLogAdminDto>>(items);
        return new PagedResult<ReviewModerationLogAdminDto>(dtos, totalCount, req.Page, req.PageSize);
    }

    public async Task<ReviewModerationLogAdminDto?> GetCourseReviewModerationLogDetailAsync(int logId)
    {
        var entity = await _courseReviewLogRepo.GetAdminDetailByIdAsync(logId);
        if (entity == null) throw new KeyNotFoundException("Log not found.");
        return _mapper.Map<ReviewModerationLogAdminDto>(entity);
    }

    public async Task<PagedResult<ReviewModerationLogAdminDto>> GetLessonReviewModerationLogsAsync(PagedRequestDto req)
    {
        if (req.Page <= 0) req.Page = 1;
        if (req.PageSize <= 0) req.PageSize = 10;

        var (items, totalCount) = await _lessonReviewLogRepo.GetPagedAdminAsync(req.Page, req.PageSize);
        if (items == null || items.Count == 0) throw new KeyNotFoundException("No lesson review moderation logs found.");
        var dtos = _mapper.Map<List<ReviewModerationLogAdminDto>>(items);
        return new PagedResult<ReviewModerationLogAdminDto>(dtos, totalCount, req.Page, req.PageSize);
    }

    public async Task<ReviewModerationLogAdminDto?> GetLessonReviewModerationLogDetailAsync(int logId)
    {
        var entity = await _lessonReviewLogRepo.GetAdminDetailByIdAsync(logId);
        if (entity == null) throw new KeyNotFoundException("Log not found.");
        return _mapper.Map<ReviewModerationLogAdminDto>(entity);
    }
}
