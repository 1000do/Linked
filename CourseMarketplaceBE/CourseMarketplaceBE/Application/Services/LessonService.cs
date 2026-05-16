using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Application.Exceptions;

namespace CourseMarketplaceBE.Application.Services;

public class LessonService : ILessonService
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IMaterialRepository _materialRepository;
    private readonly IFileUploadService _uploadService;
    private readonly IRedisService _redisService;
    private readonly IInstructorRepository _instructorRepository;
    private readonly IMaterialEmbeddingRepository _materialEmbeddingRepository;

    public LessonService(
        ILessonRepository lessonRepository, 
        ICourseRepository courseRepository,
        IMaterialRepository materialRepository,
        IFileUploadService uploadService,
        IRedisService redisService,
        IInstructorRepository instructorRepository,
        IMaterialEmbeddingRepository materialEmbeddingRepository)
    {
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _materialRepository = materialRepository;
        _uploadService = uploadService;
        _redisService = redisService;
        _instructorRepository = instructorRepository;
        _materialEmbeddingRepository = materialEmbeddingRepository;
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

        // ★ Limit: Max 5 lessons for unlinked Stripe
        var instructor = await _instructorRepository.GetByIdAsync(instructorId);
        var isStripeActive = instructor != null
            && !string.IsNullOrEmpty(instructor.StripeAccountId)
            && string.Equals(instructor.StripeOnboardingStatus, "Active", StringComparison.OrdinalIgnoreCase);

        if (!isStripeActive)
        {
            var currentLessons = await _lessonRepository.GetByCourseIdAsync(request.CourseId);
            if (currentLessons.Count(l => !l.IsRemoved) >= 5)
            {
                throw new BadRequestException("Instructor chưa liên kết Stripe chỉ được phép tạo tối đa 5 bài học mỗi khóa học.");
            }
        }


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
            course.CourseStatus = "draft";
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

        // ★ Limit: Max 1 resource for unlinked Stripe
        var instructor = await _instructorRepository.GetByIdAsync(instructorId);
        var isStripeActive = instructor != null
            && !string.IsNullOrEmpty(instructor.StripeAccountId)
            && string.Equals(instructor.StripeOnboardingStatus, "Active", StringComparison.OrdinalIgnoreCase);

        var fileType = request.MaterialMetadata?.FileType ?? "video";
        if (!isStripeActive && (fileType == "document" || fileType == "file" || fileType == "raw"))
        {
            var existingMaterials = await _materialRepository.GetMaterialsByLessonIdAsync(lessonId);
            if (existingMaterials.Count(m => m.LearningStatus != "removed" && (m.MaterialMetadata?.FileType == "document" || m.MaterialMetadata?.FileType == "file" || m.MaterialMetadata?.FileType == "raw")) >= 1)
            {
                throw new BadRequestException("Instructor chưa liên kết Stripe chỉ được phép đính kèm tối đa 1 tài liệu mỗi bài học.");
            }
        }


        string? materialUrl = request.MaterialUrl;

        if (request.MaterialFile != null)
        {
            var uploadedUrl = await _uploadService.UploadVideoAsync(request.MaterialFile);
                if (uploadedUrl != null)
            {
                materialUrl = uploadedUrl;
            }
        }

        var allMaterials = await _materialRepository.GetMaterialsByLessonIdAsync(lessonId);
        fileType = request.MaterialMetadata?.FileType ?? "video";
        
        LearningMaterial? existingMaterial = null;
        // For videos: enforce one-per-lesson (replace). For documents: allow multiple.
        if (fileType == "video")
        {
            existingMaterial = allMaterials.FirstOrDefault(m => 
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
            material.LearningStatus = "active";
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
        
        if (lesson.Course.CourseStatus.Equals("published", StringComparison.OrdinalIgnoreCase))
        {
            lesson.Course.CourseStatus = "draft";
            lesson.Course.ModerationFeedback = null;
            _courseRepository.Update(lesson.Course);
            await _courseRepository.SaveChangesAsync();
        }

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
            var trashUrl = await _uploadService.MoveToTrashAsync(material.MaterialUrl);
            
            material.CloudPublicId = cloudPublicId;
            // Giữ lại URL (đã được đổi sang link trong thư mục trash/) để vẫn có thể xem được detail
            if (!string.IsNullOrEmpty(trashUrl))
            {
                material.MaterialUrl = trashUrl;
            }
        }

        material.LearningStatus = "removed";
        material.UpdatedAt = DateTime.UtcNow;
        _materialRepository.Update(material);
        await _materialRepository.SaveChangesAsync();

        if (lesson.Course.CourseStatus.Equals("published", StringComparison.OrdinalIgnoreCase))
        {
            lesson.Course.CourseStatus = "draft";
            lesson.Course.ModerationFeedback = null;
            _courseRepository.Update(lesson.Course);
            await _courseRepository.SaveChangesAsync();
        }

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
                var trashUrl = await _uploadService.MoveToTrashAsync(m.MaterialUrl);
                
                m.CloudPublicId = cloudPublicId;
                if (!string.IsNullOrEmpty(trashUrl))
                {
                    m.MaterialUrl = trashUrl;
                }
            }
            m.LearningStatus = "removed";
            m.UpdatedAt = DateTime.UtcNow;
            _materialRepository.Update(m);
        }

        lesson.IsRemoved = true;
        lesson.UpdatedAt = DateTime.UtcNow;
        _lessonRepository.Update(lesson);
        await _lessonRepository.SaveChangesAsync();

        if (lesson.Course.CourseStatus.Equals("published", StringComparison.OrdinalIgnoreCase))
        {
            lesson.Course.CourseStatus = "draft";
            lesson.Course.ModerationFeedback = null;
            _courseRepository.Update(lesson.Course);
            await _courseRepository.SaveChangesAsync();
        }

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

        // Get course status to prevent deletion if pending
        var courseStatus = material.Lesson?.Course?.CourseStatus;
        if (courseStatus != null && courseStatus.Equals("pending", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot permanently delete materials while the course is pending review.");

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

    public async Task RestoreMaterialAsync(int materialId, int instructorId)
    {
        var material = await _materialRepository.GetByIdAsync(materialId);
        if (material == null) throw new Exception("Material not found.");

        var lesson = await _lessonRepository.GetByIdAsync(material.LessonId ?? 0);
        if (lesson == null || lesson.Course == null || lesson.Course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to restore this material.");

        if (lesson.Course.CourseStatus.Equals("pending", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot restore materials while the course is pending review.");

        var course = lesson.Course;

        // If course is published, move it back to draft as this is an update
        if (course.CourseStatus.Equals("published", StringComparison.OrdinalIgnoreCase))
        {
            course.CourseStatus = "draft";
            course.ModerationFeedback = null;
            _courseRepository.Update(course);
        }

        // Check if restoring a video and an active video already exists
        var fileType = material.MaterialMetadata?.FileType ?? "video";
        if (fileType == "video")
        {
            var existingMaterials = await _materialRepository.GetMaterialsByLessonIdAsync(lesson.LessonId);
            var activeVideo = existingMaterials.FirstOrDefault(m => 
                m.LearningStatus == "active" && 
                ((m.MaterialMetadata != null && m.MaterialMetadata.FileType == "video") || m.MaterialMetadata == null));
            
            if (activeVideo != null)
            {
                throw new InvalidOperationException("Bài học này đã có một video đang hoạt động. Vui lòng xóa video hiện tại trước khi khôi phục video này.");
            }
        }

        // 1. Restore from Cloudinary trash
        if (!string.IsNullOrEmpty(material.CloudPublicId))
        {
            var publicId = material.CloudPublicId;
            var fileTypeLower = fileType.ToLower();
            bool isRaw = fileTypeLower == "raw" || fileTypeLower == "document" || fileTypeLower == "file";

            // Fix for legacy materials that had their extension stripped from CloudPublicId
            if (isRaw && !publicId.Contains('.') && material.MaterialMetadata != null && !string.IsNullOrEmpty(material.MaterialMetadata.FileExtension))
            {
                var ext = material.MaterialMetadata.FileExtension;
                if (!ext.StartsWith('.')) ext = "." + ext;
                publicId += ext;
            }

            var restoredUrl = await _uploadService.RestoreFromTrashAsync(publicId, fileType);
            if (restoredUrl != null)
            {
                material.MaterialUrl = restoredUrl;
                material.CloudPublicId = null;
            }
        }

        material.LearningStatus = "active";
        material.UpdatedAt = DateTime.UtcNow;
        _materialRepository.Update(material);
        await _materialRepository.SaveChangesAsync();

        // Invalidate Course Cache
        if (lesson.CourseId.HasValue)
        {
            await _redisService.RemoveCacheAsync($"course:detail:{lesson.CourseId.Value}");
        }
    }

    public async Task<List<MaterialEmbedding>> GetAllMaterialEmbeddingsAsync()
    {
        return await _materialEmbeddingRepository.GetAllAsync();
    }
}
