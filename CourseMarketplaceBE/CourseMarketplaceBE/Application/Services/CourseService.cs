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
    private readonly IFileUploadService _uploadService;

    public CourseService(ICourseRepository courseRepository, IFileUploadService uploadService)
    {
        _courseRepository = courseRepository;
        _uploadService = uploadService;
    }

    public async Task<IEnumerable<CourseResponse>> GetInstructorCoursesAsync(int instructorId)
    {
        var courses = await _courseRepository.GetInstructorCoursesAsync(instructorId);
        return courses.Select(c => new CourseResponse
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
            UpdatedAt = c.UpdatedAt
        });
    }

    public async Task<CourseDetailResponse?> GetCourseWithDetailsAsync(int courseId, int instructorId)
    {
        var course = await _courseRepository.GetCourseWithDetailsAsync(courseId);
        if (course == null) return null;

        // Optionally, if only the instructor can view it:
        // if (course.InstructorId != instructorId) throw new UnauthorizedAccessException();

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
            Lessons = course.Lessons.Select(l => new LessonResponse
            {
                LessonId = l.LessonId,
                CourseId = l.CourseId,
                Title = l.Title,
                Description = l.Description,
                ThumbnailUrl = l.ThumbnailUrl,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt,
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
            UpdatedAt = course.UpdatedAt
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
            UpdatedAt = course.UpdatedAt
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
}
