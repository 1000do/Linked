using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services;

public class CourseQueryService : ICourseQueryService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IInstructorRepository _instructorRepository;
    private readonly IRedisService _redisService;
    private readonly ICourseAiIntegrationRepository _aiIntegrationRepository;
    private readonly IMapper _mapper;
    private readonly ICartRepository _cartRepository;

    public CourseQueryService(
        ICourseRepository courseRepository,
        IInstructorRepository instructorRepository,
        IRedisService redisService,
        ICourseAiIntegrationRepository aiIntegrationRepository,
        IMapper mapper,
        ICartRepository cartRepository)
    {
        _courseRepository = courseRepository;
        _instructorRepository = instructorRepository;
        _redisService = redisService;
        _aiIntegrationRepository = aiIntegrationRepository;
        _mapper = mapper;
        _cartRepository = cartRepository;
    }

    public async Task<IEnumerable<CourseResponse>> GetAllPublishedCoursesAsync(int? userId = null)
    {
        var courses = await _courseRepository.GetAllPublishedCoursesAsync();
        var courseIds = courses.Select(c => c.CourseId).ToList();

        var stats = await _courseRepository.GetCourseStatsAsync(courseIds);

        var enrolledCourseIds = new List<int>();
        if (userId.HasValue)
        {
            foreach (var cid in courseIds)
            {
                if (await _courseRepository.IsEnrolledAsync(userId.Value, cid))
                {
                    enrolledCourseIds.Add(cid);
                }
            }
        }

        var result = _mapper.Map<List<CourseResponse>>(courses);
        foreach (var r in result)
        {
            var s = stats.FirstOrDefault(st => st.CourseId == r.CourseId);
            r.TotalStudents = s?.TotalStudents ?? 0;
            r.RatingAverage = (decimal)(s?.RatingAverage ?? 0);
            r.TotalReviews = s?.TotalReviews ?? 0;
            r.IsEnrolled = enrolledCourseIds.Contains(r.CourseId);
            r.IsOwner = userId.HasValue && r.InstructorId == userId.Value;
        }

        return result;
    }

    public async Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseMarketplaceBE.Application.DTOs.CourseResponse>> GetPublishedCoursesPagedAsync(
        string? query = null,
        string? category = null,
        string? sort = null,
        string? price = null,
        string? rating = null,
        int? page = null,
        int? pageSize = null,
        int? userId = null)
    {
        var (courses, totalCount) = await _courseRepository.GetAllPublishedCoursesPagedAsync(query, category, sort, price, rating, page, pageSize);
        var courseIds = courses.Select(c => c.CourseId).ToList();

        var stats = await _courseRepository.GetCourseStatsAsync(courseIds);

        var enrolledCourseIds = new List<int>();
        if (userId.HasValue)
        {
            foreach (var cid in courseIds)
            {
                if (await _courseRepository.IsEnrolledAsync(userId.Value, cid))
                {
                    enrolledCourseIds.Add(cid);
                }
            }
        }

        var courseResponses = _mapper.Map<List<CourseResponse>>(courses);
        foreach (var r in courseResponses)
        {
            var s = stats.FirstOrDefault(st => st.CourseId == r.CourseId);
            r.TotalStudents = s?.TotalStudents ?? 0;
            r.RatingAverage = (decimal)(s?.RatingAverage ?? 0);
            r.TotalReviews = s?.TotalReviews ?? 0;
            r.IsEnrolled = enrolledCourseIds.Contains(r.CourseId);
            r.IsOwner = userId.HasValue && r.InstructorId == userId.Value;
        }

        int finalPage = page ?? 1;
        int finalPageSize = pageSize ?? 12;
        int totalPages = (int)Math.Ceiling(totalCount / (double)finalPageSize);

        return new CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseMarketplaceBE.Application.DTOs.CourseResponse>
        {
            Items = courseResponses,
            TotalCount = totalCount,
            
            Page = finalPage,
            PageSize = finalPageSize
        };
    }

    public async Task<bool> IsEnrolledAsync(int userId, int courseId)
    {
        return await _courseRepository.IsEnrolledAsync(userId, courseId);
    }

    public async Task<IEnumerable<CourseResponse>> GetInstructorCoursesAsync(int instructorId)
    {
        var courses = await _courseRepository.GetInstructorCoursesAsync(instructorId);
        var courseIds = courses.Select(c => c.CourseId).ToList();

        var stats = await _courseRepository.GetCourseStatsAsync(courseIds);

        var result = _mapper.Map<List<CourseResponse>>(courses);
        foreach (var r in result)
        {
            var s = stats.FirstOrDefault(st => st.CourseId == r.CourseId);
            r.TotalStudents = s?.TotalStudents ?? 0;
            r.RatingAverage = (decimal)(s?.RatingAverage ?? 0);
        }

        return result;
    }

    public async Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseMarketplaceBE.Application.DTOs.CourseResponse>> GetInstructorCoursesPagedAsync(
        int instructorId,
        string? search = null,
        string? status = null,
        int? page = null,
        int? pageSize = null)
    {
        var (courses, totalCount) = await _courseRepository.GetInstructorCoursesPagedAsync(instructorId, search, status, page, pageSize);
        var courseIds = courses.Select(c => c.CourseId).ToList();

        var stats = await _courseRepository.GetCourseStatsAsync(courseIds);

        var courseResponses = _mapper.Map<List<CourseResponse>>(courses);
        foreach (var r in courseResponses)
        {
            var s = stats.FirstOrDefault(st => st.CourseId == r.CourseId);
            r.TotalStudents = s?.TotalStudents ?? 0;
            r.RatingAverage = (decimal)(s?.RatingAverage ?? 0);
        }

        int finalPage = page ?? 1;
        int finalPageSize = pageSize ?? 6;
        int totalPages = (int)Math.Ceiling(totalCount / (double)finalPageSize);

        return new CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseMarketplaceBE.Application.DTOs.CourseResponse>
        {
            Items = courseResponses,
            TotalCount = totalCount,
            
            Page = finalPage,
            PageSize = finalPageSize
        };
    }

    public async Task<CourseDetailResponse?> GetCourseWithDetailsAsync(int courseId, int instructorId, int? userId = null)
    {
        string cacheKey = CacheKeys.CourseDetail.GetKey(courseId);
        var response = await _redisService.GetCacheAsync<CourseDetailResponse>(cacheKey);

        if (response == null)
        {
            var course = await _courseRepository.GetCourseWithDetailsAsync(courseId);
            if (course == null) return null;

            var courseStats = await _courseRepository.GetCourseStatsAsync(courseId);
            var instructorStats = course.InstructorId.HasValue
                ? await _instructorRepository.GetStatsAsync(course.InstructorId.Value)
                : null;

            response = _mapper.Map<CourseDetailResponse>(course);
            
            response.InstructorStudentsCount = instructorStats?.TotalStudentsCount ?? 0;
            response.TotalStudents = courseStats?.TotalStudents ?? 0;
            response.TotalReviews = courseStats?.TotalReviews ?? 0;
            response.RatingAverage = (decimal)(courseStats?.RatingAverage ?? 0);

            await _redisService.SetCacheAsync(cacheKey, response, CacheTtl.Short.GetTtl());
        }

        response.IsInAnyCart = await _cartRepository.IsCourseInAnyCartAsync(courseId);

        if (userId.HasValue)
        {
            response.IsOwner = response.InstructorId == userId.Value;
            response.IsEnrolled = response.IsOwner || await _courseRepository.IsEnrolledAsync(userId.Value, courseId);
        }
        else
        {
            response.IsEnrolled = false;
            response.IsOwner = false;
        }

        return response;
    }

    public async Task<IEnumerable<CategoryResponse>> GetCategoriesAsync()
    {
        var categories = await _courseRepository.GetCategoriesAsync();
        return _mapper.Map<IEnumerable<CategoryResponse>>(categories);
    }

    public async Task<CourseAiIntegrationResponse> GetByModelAndCourseAsync(int modelId, int courseId)
    {
        var integration = await _aiIntegrationRepository.GetByModelAndCourseAsync(modelId, courseId);
        if (integration == null) return null!;
        return new CourseAiIntegrationResponse
        {
            CourseId = integration.CourseId ?? 0,
            ModelId = integration.ModelId ?? 0,
            IsEnabled = integration.IsEnabled,
            ConfigJson = !string.IsNullOrEmpty(integration.ConfigJson)
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, float>>(integration.ConfigJson) ?? new Dictionary<string, float>()
                : new Dictionary<string, float>(),
            Role = integration.Role
        };
    }
}
