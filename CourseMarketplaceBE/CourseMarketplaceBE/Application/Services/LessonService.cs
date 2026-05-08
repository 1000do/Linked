using System;
using System.Security.Cryptography;
using System.Text;
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
    private readonly IRedisService _redisService;

    public LessonService(
        ILessonRepository lessonRepository, 
        ICourseRepository courseRepository,
        IMaterialRepository materialRepository,
        IFileUploadService uploadService,
        IRedisService redisService)
    {
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _materialRepository = materialRepository;
        _uploadService = uploadService;
        _redisService = redisService;
    }

    public async Task<LessonResponse> CreateLessonAsync(LessonCreateRequest request, int instructorId)
    {
        var course = await _courseRepository.GetByIdAsync(request.CourseId);
        if (course == null)
            throw new Exception("Course not found.");

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to add a lesson to this course.");

        if (course.CourseStatus.Equals("pending", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot add lessons while the course is pending review.");

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

        // Invalidate Course Cache
        await _redisService.RemoveCacheAsync($"course:detail:{request.CourseId}");

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

        if (lesson.Course.CourseStatus.Equals("pending", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot add materials while the course is pending review.");

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
        // For videos: enforce one-per-lesson (replace). For documents: allow multiple.
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
                // Xóa file cũ trên Cloudinary trước khi cập nhật link mới
                if (!string.IsNullOrEmpty(material.MaterialUrl))
                {
                    await _uploadService.DeleteFileAsync(material.MaterialUrl);
                }
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

        // Invalidate Course Cache
        if (lesson.CourseId.HasValue)
        {
            await _redisService.RemoveCacheAsync($"course:detail:{lesson.CourseId.Value}");
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

    public async Task RemoveMaterialAsync(int materialId, int instructorId)
    {
        var material = await _materialRepository.GetByIdAsync(materialId);
        if (material == null)
            throw new Exception("Material not found.");

        var lesson = await _lessonRepository.GetByIdAsync(material.LessonId ?? 0);
        if (lesson == null || lesson.Course == null || lesson.Course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to remove this material.");

        if (lesson.Course.CourseStatus.Equals("pending", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot remove materials while the course is pending review.");

        // Move file to trash on Cloudinary instead of deleting
        if (!string.IsNullOrEmpty(material.MaterialUrl))
        {
            var cloudPublicId = _uploadService.GetPublicIdFromUrl(material.MaterialUrl);
            await _uploadService.MoveToTrashAsync(material.MaterialUrl);
            material.CloudPublicId = cloudPublicId;
            material.MaterialUrl = null; // Update to null as requested
        }

        material.LearningStatus = "removed";
        material.UpdatedAt = DateTime.UtcNow;
        _materialRepository.Update(material);
        await _materialRepository.SaveChangesAsync();

        // Invalidate Course Cache
        if (lesson.CourseId.HasValue)
        {
            await _redisService.RemoveCacheAsync($"course:detail:{lesson.CourseId.Value}");
        }
    }

    public async Task DeleteLessonAsync(int lessonId, int instructorId)
    {
        var lesson = await _lessonRepository.GetByIdAsync(lessonId);
        if (lesson == null)
            throw new Exception("Lesson not found.");

        if (lesson.Course == null || lesson.Course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to delete this lesson.");

        if (lesson.Course.CourseStatus.Equals("pending", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot delete lessons while the course is pending review.");

        // Soft delete all materials of this lesson
        var materials = await _materialRepository.GetMaterialsByLessonIdAsync(lessonId);
        foreach (var m in materials)
        {
            if (!string.IsNullOrEmpty(m.MaterialUrl))
            {
                var cloudPublicId = _uploadService.GetPublicIdFromUrl(m.MaterialUrl);
                await _uploadService.MoveToTrashAsync(m.MaterialUrl);
                m.CloudPublicId = cloudPublicId;
                m.MaterialUrl = null;
            }
            m.LearningStatus = "removed";
            m.UpdatedAt = DateTime.UtcNow;
            _materialRepository.Update(m);
        }

        lesson.IsRemoved = true;
        lesson.UpdatedAt = DateTime.UtcNow;
        _lessonRepository.Update(lesson);
        await _lessonRepository.SaveChangesAsync();

        // Invalidate Course Cache
        if (lesson.CourseId.HasValue)
        {
            await _redisService.RemoveCacheAsync($"course:detail:{lesson.CourseId.Value}");
        }
    }

    public async Task<IEnumerable<MaterialTrashResponse>> GetTrashMaterialsAsync(int instructorId)
    {
        var materials = await _materialRepository.GetTrashMaterialsAsync(instructorId);
        
        return materials.Select(m => new MaterialTrashResponse
        {
            MaterialId = m.MaterialId,
            Title = m.Title,
            LessonTitle = m.Lesson?.Title,
            CourseTitle = m.Lesson?.Course?.Title,
            DeletedAt = m.UpdatedAt,
            FileType = m.MaterialMetadata?.FileType,
            CloudPublicId = m.CloudPublicId
        });
    }

    public async Task PermanentDeleteMaterialAsync(int materialId, int instructorId)
    {
        var material = await _materialRepository.GetByIdAsync(materialId);
        if (material == null) throw new Exception("Material not found.");

        // Check ownership (even if soft-deleted)
        var materials = await _materialRepository.GetTrashMaterialsAsync(instructorId);
        if (!materials.Any(m => m.MaterialId == materialId))
        {
             throw new UnauthorizedAccessException("You do not have permission to permanently delete this material.");
        }

        // 1. Delete from Cloudinary if public ID exists
        if (!string.IsNullOrEmpty(material.CloudPublicId))
        {
            // Note: Since it was moved to trash/ prefix, we need to delete it using the trash prefix
            var resourceType = material.MaterialMetadata?.FileType ?? "image";
            await _uploadService.DeleteFileByPublicIdAsync($"trash/{material.CloudPublicId}", resourceType); 
        }

        // 2. Delete from DB
        int? courseId = material.Lesson?.CourseId;
        _materialRepository.Delete(material);
        await _materialRepository.SaveChangesAsync();

        // Invalidate Course Cache
        if (courseId.HasValue)
        {
            await _redisService.RemoveCacheAsync($"course:detail:{courseId.Value}");
        }
    }
}
