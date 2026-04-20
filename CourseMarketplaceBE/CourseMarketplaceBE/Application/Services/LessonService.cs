using System;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services;

public class LessonService : ILessonService
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IMaterialRepository _materialRepository;

    public LessonService(
        ILessonRepository lessonRepository, 
        ICourseRepository courseRepository,
        IMaterialRepository materialRepository)
    {
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _materialRepository = materialRepository;
    }

    public async Task<LessonResponse> CreateLessonAsync(LessonCreateRequest request, int instructorId)
    {
        var course = await _courseRepository.GetByIdAsync(request.CourseId);
        if (course == null)
            throw new Exception("Course not found.");

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to add a lesson to this course.");

        var lesson = new Lesson
        {
            CourseId = request.CourseId,
            Title = request.Title,
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LessonStatus = "active"
        };

        await _lessonRepository.AddAsync(lesson);
        await _lessonRepository.SaveChangesAsync();

        return new LessonResponse
        {
            LessonId = lesson.LessonId,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Description = lesson.Description,
            ThumbnailUrl = lesson.ThumbnailUrl,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt
        };
    }

    public async Task<MaterialResponse> AddMaterialToLessonAsync(int lessonId, MaterialCreateRequest request, int instructorId)
    {
        var lesson = await _lessonRepository.GetByIdAsync(lessonId);
        if (lesson == null)
            throw new Exception("Lesson not found.");

        if (lesson.Course == null || lesson.Course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to add material to this lesson.");

        var material = new LearningMaterial
        {
            LessonId = lessonId,
            Title = request.Title,
            Description = request.Description,
            MaterialUrl = request.MaterialUrl,
            Duration = request.Duration,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LearningStatus = "active"
        };

        await _materialRepository.AddAsync(material);
        await _materialRepository.SaveChangesAsync();

        return new MaterialResponse
        {
            MaterialId = material.MaterialId,
            LessonId = material.LessonId,
            Title = material.Title,
            Description = material.Description,
            MaterialUrl = material.MaterialUrl,
            Duration = material.Duration,
            CreatedAt = material.CreatedAt,
            UpdatedAt = material.UpdatedAt
        };
    }

    public async Task DeleteLessonAsync(int lessonId, int instructorId)
    {
        var lesson = await _lessonRepository.GetByIdAsync(lessonId);
        if (lesson == null)
            throw new Exception("Lesson not found.");

        if (lesson.Course == null || lesson.Course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to delete this lesson.");

        _lessonRepository.Delete(lesson);
        await _lessonRepository.SaveChangesAsync();
    }
}
