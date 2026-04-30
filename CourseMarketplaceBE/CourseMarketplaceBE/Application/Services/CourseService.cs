using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IInstructorRepository _instructorRepository;
    private readonly IFileUploadService _uploadService;

    public CourseService(ICourseRepository courseRepository, IInstructorRepository instructorRepository, IFileUploadService uploadService)
    {
        _courseRepository = courseRepository;
        _instructorRepository = instructorRepository;
        _uploadService = uploadService;
    }

    public async Task<IEnumerable<CourseResponse>> GetAllPublishedCoursesAsync(int? userId = null)
    {
        var courses = await _courseRepository.GetAllPublishedCoursesAsync();
        var courseIds = courses.Select(c => c.CourseId).ToList();

        // Fetch stats for these courses
        var stats = await _courseRepository.GetCourseStatsAsync(courseIds);

        // Check enrollment if user is logged in
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

        return courses.Select(c =>
        {
            var s = stats.FirstOrDefault(st => st.CourseId == c.CourseId);
            return new CourseResponse
            {
                CourseId = c.CourseId,
                InstructorId = c.InstructorId,
                CategoryId = c.CategoryId,
                Title = c.Title,
                Description = c.Description,
                Price = c.Price,
                CourseThumbnailUrl = c.CourseThumbnailUrl,
                CourseStatus = c.CourseStatus,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                WhatYouWillLearn = c.WhatYouWillLearn,
                Requirements = c.Requirements,
                CategoryName = c.Category?.CategoriesName,
                InstructorName = c.Instructor?.InstructorNavigation?.FullName ?? "Unknown Instructor",
                InstructorAvatarUrl = c.Instructor?.InstructorNavigation?.UserNavigation?.AvatarUrl,
                TotalStudents = s?.TotalStudents ?? 0,
                RatingAverage = (decimal)(s?.RatingAverage ?? 0),
                IsEnrolled = enrolledCourseIds.Contains(c.CourseId),
                IsOwner = userId.HasValue && c.InstructorId == userId.Value,
                TotalReviews = s?.TotalReviews ?? 0
            };
        });
    }

    public async Task<bool> IsEnrolledAsync(int userId, int courseId)
    {
        return await _courseRepository.IsEnrolledAsync(userId, courseId);
    }

    public async Task<IEnumerable<CourseResponse>> GetInstructorCoursesAsync(int instructorId)
    {
        var courses = await _courseRepository.GetInstructorCoursesAsync(instructorId);
        var courseIds = courses.Select(c => c.CourseId).ToList();

        // Fetch stats for these courses
        var stats = await _courseRepository.GetCourseStatsAsync(courseIds);

        return courses.Select(c =>
        {
            var s = stats.FirstOrDefault(st => st.CourseId == c.CourseId);
            return new CourseResponse
            {
                CourseId = c.CourseId,
                InstructorId = c.InstructorId,
                CategoryId = c.CategoryId,
                Title = c.Title,
                Description = c.Description,
                Price = c.Price,
                CourseThumbnailUrl = c.CourseThumbnailUrl,
                CourseStatus = c.CourseStatus,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                WhatYouWillLearn = c.WhatYouWillLearn,
                Requirements = c.Requirements,
                TotalStudents = s?.TotalStudents ?? 0,
                RatingAverage = (decimal)(s?.RatingAverage ?? 0)
            };
        });
    }

    public async Task<CourseDetailResponse?> GetCourseWithDetailsAsync(int courseId, int instructorId, int? userId = null)
    {
        var course = await _courseRepository.GetCourseWithDetailsAsync(courseId);
        if (course == null) return null;

        var courseStats = await _courseRepository.GetCourseStatsAsync(courseId);
        var instructorStats = course.InstructorId.HasValue 
            ? await _instructorRepository.GetStatsAsync(course.InstructorId.Value) 
            : null;

        var response = new CourseDetailResponse
        {
            CourseId = course.CourseId,
            InstructorId = course.InstructorId,
            CategoryId = course.CategoryId,
            Title = course.Title,
            Description = course.Description,
            Price = course.Price,
            CourseThumbnailUrl = course.CourseThumbnailUrl,
            CourseStatus = course.CourseStatus,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            WhatYouWillLearn = course.WhatYouWillLearn,
            Requirements = course.Requirements,
            CategoryName = course.Category?.CategoriesName,
            InstructorName = course.Instructor?.InstructorNavigation?.FullName ?? "Unknown Instructor",
            InstructorAvatarUrl = course.Instructor?.InstructorNavigation?.UserNavigation?.AvatarUrl,
            InstructorBio = course.Instructor?.InstructorNavigation?.Bio,
            InstructorProfessionalTitle = course.Instructor?.ProfessionalTitle,
            InstructorCoursesCount = course.Instructor?.Courses?.Count ?? 0,
            InstructorReviewCount = 0, 
            InstructorStudentsCount = instructorStats?.TotalStudentsCount ?? 0,
            TotalStudents = courseStats?.TotalStudents ?? 0,
            TotalReviews = courseStats?.TotalReviews ?? 0,
            RatingAverage = (decimal)(courseStats?.RatingAverage ?? 0),
            IsEnrolled = userId.HasValue && await _courseRepository.IsEnrolledAsync(userId.Value, courseId),
            IsOwner = userId.HasValue && course.InstructorId == userId.Value,
            Lessons = course.Lessons.Select(l => new LessonResponse
            {
                LessonId = l.LessonId,
                CourseId = l.CourseId,
                Title = l.Title,
                Description = l.Description,
                ThumbnailUrl = l.ThumbnailUrl,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt,
                LessonStatus = l.LessonStatus,
                LearningMaterials = l.LearningMaterials.Select(m => new MaterialResponse
                {
                    MaterialId = m.MaterialId,
                    LessonId = m.LessonId,
                    Title = m.Title,
                    Description = m.Description,
                    MaterialUrl = m.MaterialUrl,
                    MaterialMetadata = m.MaterialMetadata,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                }).ToList()
            }).ToList()
        };

        return response;
    }


    public async Task<CourseResponse> CreateCourseAsync(CourseCreateRequest request, int instructorId)
    {
        var instructor = await _instructorRepository.GetByIdAsync(instructorId);
        if (instructor == null || instructor.ApprovalStatus != "Approved")
        {
            throw new BadRequestException("You must be an approved instructor to create a course.");
        }

        string? thumbnailUrl = request.CourseThumbnailUrl;

        if (request.ThumbnailFile != null)
        {
            var uploadedUrl = await _uploadService.UploadImageAsync(request.ThumbnailFile);
            if (uploadedUrl != null)
            {
                thumbnailUrl = uploadedUrl;
            }
        }

        var course = new Course
        {
            InstructorId = instructorId,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            CourseThumbnailUrl = thumbnailUrl,
            WhatYouWillLearn = request.WhatYouWillLearn,
            Requirements = request.Requirements,
            CourseStatus = "draft", // Default status
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _courseRepository.AddAsync(course);
        await _courseRepository.SaveChangesAsync();

        return new CourseResponse
        {
            CourseId = course.CourseId,
            InstructorId = course.InstructorId,
            CategoryId = course.CategoryId,
            Title = course.Title,
            Description = course.Description,
            Price = course.Price,
            CourseThumbnailUrl = course.CourseThumbnailUrl,
            CourseStatus = course.CourseStatus,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            WhatYouWillLearn = course.WhatYouWillLearn,
            Requirements = course.Requirements
        };
    }

    public async Task<CourseResponse> UpdateCourseAsync(int courseId, CourseUpdateRequest request, int instructorId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            throw new Exception("Course not found.");

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to modify this course.");

        string? thumbnailUrl = request.CourseThumbnailUrl ?? course.CourseThumbnailUrl;

        if (request.ThumbnailFile != null)
        {
            var uploadedUrl = await _uploadService.UploadImageAsync(request.ThumbnailFile);
            if (uploadedUrl != null)
            {
                thumbnailUrl = uploadedUrl;
            }
        }

        course.CategoryId = request.CategoryId;
        course.Title = request.Title;
        course.Description = request.Description;
        course.Price = request.Price;
        course.CourseThumbnailUrl = thumbnailUrl;
        course.WhatYouWillLearn = request.WhatYouWillLearn;
        course.Requirements = request.Requirements;
        course.UpdatedAt = DateTime.UtcNow;

        _courseRepository.Update(course);
        await _courseRepository.SaveChangesAsync();

        return new CourseResponse
        {
            CourseId = course.CourseId,
            InstructorId = course.InstructorId,
            CategoryId = course.CategoryId,
            Title = course.Title,
            Description = course.Description,
            Price = course.Price,
            CourseThumbnailUrl = course.CourseThumbnailUrl,
            CourseStatus = course.CourseStatus,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            WhatYouWillLearn = course.WhatYouWillLearn,
            Requirements = course.Requirements
        };
    }

    public async Task UpdateCourseStatusAsync(int courseId, string status, int instructorId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            throw new Exception("Course not found.");

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to modify this course.");

        if (status != "published" && status != "archived")
            throw new BadRequestException("Invalid status. Allowed values are 'published' or 'archived'.");

        course.CourseStatus = status;
        course.UpdatedAt = DateTime.UtcNow;

        _courseRepository.Update(course);
        await _courseRepository.SaveChangesAsync();
    }

    public async Task DeleteCourseAsync(int courseId, int instructorId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            throw new Exception("Course not found.");

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to delete this course.");

        var hasEnrollments = await _courseRepository.HasEnrollmentsAsync(courseId);
        if (hasEnrollments)
            throw new BadRequestException("Cannot delete a course with active students. Please archive it instead.");

        _courseRepository.Delete(course);
        await _courseRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<CategoryResponse>> GetCategoriesAsync()
    {
        var categories = await _courseRepository.GetCategoriesAsync();
        return categories.Select(c => new CategoryResponse
        {
            CategoryId = c.CategoryId,
            CategoriesName = c.CategoriesName
        });
    }
}
