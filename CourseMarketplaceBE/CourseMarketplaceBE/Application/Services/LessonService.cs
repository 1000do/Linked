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
    private readonly IFileUploadService _uploadService;

    public LessonService(
        ILessonRepository lessonRepository, 
        ICourseRepository courseRepository,
        IMaterialRepository materialRepository,
        IFileUploadService uploadService)
    {
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _materialRepository = materialRepository;
        _uploadService = uploadService;
    }

    public async Task<LessonResponse> CreateLessonAsync(LessonCreateRequest request, int instructorId)
    {
        var course = await _courseRepository.GetByIdAsync(request.CourseId);
        if (course == null)
            throw new Exception("Course not found.");

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to add a lesson to this course.");

        string? thumbnailUrl = request.ThumbnailUrl;

        if (request.ThumbnailFile != null)
        {
            var uploadedUrl = await _uploadService.UploadImageAsync(request.ThumbnailFile);
            if (uploadedUrl != null)
            {
                thumbnailUrl = uploadedUrl;
            }
        }

        var lesson = new Lesson
        {
            CourseId = request.CourseId,
            Title = request.Title,
            Description = request.Description,
            ThumbnailUrl = thumbnailUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LessonStatus = "active"
        };

        await _lessonRepository.AddAsync(lesson);
        
        if (course.CourseStatus.Equals("published", StringComparison.OrdinalIgnoreCase))
        {
            course.CourseStatus = "pending";
            course.ModerationFeedback = null;
            _courseRepository.Update(course);
        }

        await _lessonRepository.SaveChangesAsync();

        return new LessonResponse
        {
            LessonId = lesson.LessonId,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Description = lesson.Description,
            ThumbnailUrl = lesson.ThumbnailUrl,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt,
            LessonStatus = lesson.LessonStatus,
            CourseStatus = course.CourseStatus
        };
    }

    public async Task<MaterialResponse> AddMaterialToLessonAsync(int lessonId, MaterialCreateRequest request, int instructorId)
    {
        var lesson = await _lessonRepository.GetByIdAsync(lessonId);
        if (lesson == null)
            throw new Exception("Lesson not found.");

        if (lesson.Course == null || lesson.Course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to add material to this lesson.");

        string? materialUrl = request.MaterialUrl;

        if (request.MaterialFile != null)
        {
            var uploadedUrl = await _uploadService.UploadVideoAsync(request.MaterialFile);
            if (uploadedUrl != null)
            {
                materialUrl = uploadedUrl;
            }
        }

        var existingMaterials = await _materialRepository.GetMaterialsByLessonIdAsync(lessonId);
        var fileType = request.MaterialMetadata?.FileType ?? "video";
        
        LearningMaterial? existingMaterial = null;
        // Only enforce 1-to-1 for videos. For documents, allow multiple by creating new.
        if (fileType == "video")
        {
            existingMaterial = existingMaterials.FirstOrDefault(m => 
                (m.MaterialMetadata != null && m.MaterialMetadata.FileType == "video") || 
                (m.MaterialMetadata == null));
        }

        LearningMaterial material;
        if (existingMaterial != null)
        {
            material = existingMaterial;
            material.Title = request.Title;
            material.Description = request.Description;
            if (!string.IsNullOrEmpty(materialUrl)) {
                material.MaterialUrl = materialUrl;
            }
            if (request.MaterialMetadata != null) {
                if (material.MaterialMetadata == null) material.MaterialMetadata = new MaterialMetadata();
                material.MaterialMetadata.FileType = request.MaterialMetadata.FileType;
                if (request.MaterialMetadata.Duration.HasValue) material.MaterialMetadata.Duration = request.MaterialMetadata.Duration;
            }
            material.UpdatedAt = DateTime.UtcNow;
            
            _materialRepository.Update(material);
        }
        else
        {
            material = new LearningMaterial
            {
                LessonId = lessonId,
                Title = request.Title,
                Description = request.Description,
                MaterialUrl = materialUrl,
                MaterialMetadata = request.MaterialMetadata ?? new MaterialMetadata { FileType = "video" },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LearningStatus = "active"
            };
            await _materialRepository.AddAsync(material);
        }

        await _materialRepository.SaveChangesAsync();

        if (lesson.Course != null && lesson.Course.CourseStatus.Equals("published", StringComparison.OrdinalIgnoreCase))
        {
            lesson.Course.CourseStatus = "pending";
            lesson.Course.ModerationFeedback = null;
            _courseRepository.Update(lesson.Course);
            await _courseRepository.SaveChangesAsync();
        }

        return new MaterialResponse
        {
            MaterialId = material.MaterialId,
            LessonId = material.LessonId,
            Title = material.Title,
            Description = material.Description,
            MaterialUrl = material.MaterialUrl,
            MaterialMetadata = material.MaterialMetadata,
            CreatedAt = material.CreatedAt,
            UpdatedAt = material.UpdatedAt,
            CourseStatus = lesson.Course?.CourseStatus
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
