using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Share.Helpers;
using Microsoft.Extensions.Logging;
using CourseMarketplaceBE.Domain.Enums;

namespace CourseMarketplaceBE.Application.Services;

public class CourseCommandService : ICourseCommandService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IInstructorRepository _instructorRepository;
    private readonly IFileUploadService _uploadService;
    private readonly IMaterialRepository _materialRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IRedisService _redisService;
    private readonly IContentHashService _contentHashService;
    private readonly ICourseAiIntegrationRepository _aiIntegrationRepository;
    private readonly ILogger<CourseCommandService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly ILockoutRepository _lockoutRepo;
    private readonly ICourseExtRepository _courseExtRepo;
    private readonly IHtmlTextManipulationService _htmlTextManipulationService;

    public CourseCommandService(
        ICourseRepository courseRepository,
        IInstructorRepository instructorRepository,
        IFileUploadService uploadService,
        IMaterialRepository materialRepository,
        ILessonRepository lessonRepository,
        IRedisService redisService,
        IContentHashService contentHashService,
        ICourseAiIntegrationRepository aiIntegrationRepository,
        ILogger<CourseCommandService> logger,
        IHttpClientFactory httpClientFactory,
        // IMapper mapper)
        IMapper mapper,
        ILockoutRepository lockoutRepo,
        ICourseExtRepository courseExtRepo,
        IHtmlTextManipulationService htmlTextManipulationService)
    {
        _courseRepository = courseRepository;
        _instructorRepository = instructorRepository;
        _uploadService = uploadService;
        _materialRepository = materialRepository;
        _lessonRepository = lessonRepository;
        _redisService = redisService;
        _contentHashService = contentHashService;
        _aiIntegrationRepository = aiIntegrationRepository;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("AiContentModeration");
        _mapper = mapper;
        _lockoutRepo = lockoutRepo;
        _courseExtRepo = courseExtRepo;
        _htmlTextManipulationService = htmlTextManipulationService;
    }

    private async Task<(int, bool)> CheckInstructorRights(int instructorId)
    {
        var instructor = await _instructorRepository.GetByIdAsync(instructorId);
        if (instructor == null || !string.Equals(instructor.ApprovalStatus, InstructorApprovalStatus.Approved.ToValue(), StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("You must be an approved instructor to create a course.");
        }

        var activeLockout = await _lockoutRepo.GetActiveLockoutAsync(instructorId, "instructor");
        if (activeLockout != null)
        {
            throw new BadRequestException($"Your instructor rights are locked until {activeLockout.LockoutEnd.Value:yyyy-MM-dd HH:mm:ss} due to policy violations. You cannot create new courses.");
        }

        // ★ Limit: Max 2 courses for unlinked Stripe
        var isStripeActive = !string.IsNullOrEmpty(instructor.StripeAccountId)
            && string.Equals(instructor.StripeOnboardingStatus, StripeOnboardingStatus.Active.ToValue(), StringComparison.OrdinalIgnoreCase);

        if (!isStripeActive)
        {
            var instructorCourses = await _courseRepository.GetInstructorCoursesAsync(instructorId);
            if (instructorCourses.Count(c => !c.IsRemoved) >= 2)
            {
                throw new BadRequestException("Instructors who have not linked a Stripe account are only allowed to create up to 2 courses.");
            }
        }
        return (instructor.InstructorId, isStripeActive);


    }

    public async Task<CourseResponse> CreateCourseAsync(CourseCreateRequest request, int instructorId)
    {
        if (request == null) throw new BadRequestException("Missing required data for course creation.");

        (int validInstructorId, bool isStripeActive) = await CheckInstructorRights(instructorId);

        var coursePrice = isStripeActive ? request.Price : 0m;

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
            InstructorId = validInstructorId,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = _htmlTextManipulationService.SanitizeHtml(request.Description),
            Price = coursePrice,
            CourseThumbnailUrl = thumbnailUrl,
            WhatYouWillLearn = _htmlTextManipulationService.SanitizeHtml(request.WhatYouWillLearn),
            Requirements = _htmlTextManipulationService.SanitizeHtml(request.Requirements),
            CourseStatus = CourseStatus.Draft.ToValue(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _courseRepository.Add(await AddCourseFingerprintAsync(course));

        int numRows = await SaveChangesWithFingerprintAsync();
        if (numRows == 0) throw new InvalidOperationException("Failed to create course and fingerprint");


        return _mapper.Map<CourseResponse>(course);
    }

    private async Task<int> SaveChangesWithFingerprintAsync()
    {
        try
        {
            return await _courseExtRepo.SaveChangesAsync();
        }
        catch (CourseExtException ex)
        {

            string field = GetDuplicateFieldFromErrorMessage(ex.Message);
            throw new BadRequestException($"Content duplication detected on '{field}'");



        }
    }
    private string GetDuplicateFieldFromErrorMessage(string message)
    {
        //Extract column name from constraint name
        var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(message);
        var constraint = dict?["constraint"]?.ToString() ?? "";
        if (constraint.Contains("title_hash")) return "Title";
        if (constraint.Contains("description_hash")) return "Description";
        if (constraint.Contains("what_you_will_learn_hash")) return "What You Will Learn";
        if (constraint.Contains("requirements_hash")) return "Requirements";
        if (constraint.Contains("thumbnail_hash")) return "Thumbnail";

        var parts = constraint.Split("_") ?? [];
        _logger.LogInformation("Raw parts: {parts}", JsonSerializer.Serialize(parts));
        List<string> nameparts = [];
        int len = parts.Length;
        _logger.LogInformation("Length of parts: {len}", len);
        for (int i = 0; i < len; i++)
        {
            if (i == 0 || i == len - 1)
            {
                continue;
            }

            _logger.LogInformation("Part {i} : {part}", i, parts[i]);
            nameparts.Add(parts[i].ToPascalCase());
        }
        _logger.LogInformation("Column name parts: {nameparts}", JsonSerializer.Serialize(nameparts));
        string field = String.Join(" ", nameparts);
        _logger.LogInformation("Column name: {columnName}", field);
        return field;
    }

    public async Task<CourseResponse> UpdateCourseAsync(int courseId, CourseUpdateRequest request, int instructorId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            throw new Exception("Course not found.");

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to modify this course.");

        var activeLockout = await _lockoutRepo.GetActiveLockoutAsync(instructorId, "instructor");
        if (activeLockout != null)
        {
            throw new BadRequestException($"Your instructor rights are locked until {activeLockout.LockoutEnd.Value:yyyy-MM-dd HH:mm:ss} due to policy violations. You cannot update courses.");
        }

        if (string.Equals(course.CourseStatus, CourseStatus.Archived.ToValue(), StringComparison.OrdinalIgnoreCase) && (course.CourseFlagCount ?? 0) >= 3)
        {
            throw new BadRequestException("This course has been permanently discontinued due to policy violations and cannot be edited.");
        }

        if (CourseStatus.Pending.ToValue().Equals(course.CourseStatus, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot modify course while it is pending review.");

        string? thumbnailUrl = request.CourseThumbnailUrl ?? course.CourseThumbnailUrl;

        if (request.ThumbnailFile != null)
        {
            var uploadedUrl = await _uploadService.UploadImageAsync(request.ThumbnailFile);
            if (uploadedUrl != null)
            {
                thumbnailUrl = uploadedUrl;
            }
        }

        request.Description = _htmlTextManipulationService.SanitizeHtml(request.Description);
        request.WhatYouWillLearn = _htmlTextManipulationService.SanitizeHtml(request.WhatYouWillLearn);
        request.Requirements = _htmlTextManipulationService.SanitizeHtml(request.Requirements);

        bool isChanged = false;

        if (course.CategoryId != request.CategoryId) isChanged = true;
        if (!string.Equals(course.Title, request.Title)) isChanged = true;
        if (!string.Equals(course.Description, request.Description)) isChanged = true;

        var instructor = await _instructorRepository.GetByIdAsync(instructorId);
        var isStripeActive = instructor != null
            && !string.IsNullOrEmpty(instructor.StripeAccountId)
            && string.Equals(instructor.StripeOnboardingStatus, StripeOnboardingStatus.Active.ToValue(), StringComparison.OrdinalIgnoreCase);
        
        var newPrice = isStripeActive ? request.Price : 0m;
        if (course.Price != newPrice) isChanged = true;

        if (!string.Equals(course.CourseThumbnailUrl, thumbnailUrl)) isChanged = true;
        if (!string.Equals(course.WhatYouWillLearn, request.WhatYouWillLearn)) isChanged = true;
        if (!string.Equals(course.Requirements, request.Requirements)) isChanged = true;

        course.CategoryId = request.CategoryId;
        course.Title = request.Title;
        course.Description = request.Description;
        course.Price = newPrice;
        course.CourseThumbnailUrl = thumbnailUrl;
        course.WhatYouWillLearn = request.WhatYouWillLearn;
        course.Requirements = request.Requirements;

        if (isChanged)
        {
            course.UpdatedAt = DateTime.UtcNow;

            if (CourseStatus.Published.ToValue().Equals(course.CourseStatus, StringComparison.OrdinalIgnoreCase))
            {
                course.CourseStatus = CourseStatus.Draft.ToValue();
                course.ModerationFeedback = null;
            }
        }

        _courseRepository.Update(course);

        await UpdateCourseFingerprintAsync(course);

        // Save course updates and hashes together to verify unique constraints before saving
        int rowsUpdate = await SaveChangesWithFingerprintAsync();
        if (rowsUpdate <= 0)
            throw new InvalidOperationException("Failed to save changes when updating course.");

        await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(course.CourseId));

        return _mapper.Map<CourseResponse>(course);
    }

    private async Task<Course> AddCourseFingerprintAsync(Course course)
    {
        string? thumbHash = null;
        if (!string.IsNullOrEmpty(course.CourseThumbnailUrl))
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(course.CourseThumbnailUrl);
                thumbHash = await _contentHashService.ComputeFileHashAsync(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to download thumbnail for hashing: {ex.Message}");
            }
        }

        var courseExt = new CourseExt
        {
            CourseId = course.CourseId,
            TitleHash = await _contentHashService.ComputeCourseHashAsync(course.Title),
            DescriptionHash = await _contentHashService.ComputeCourseHashAsync(course.Description ?? ""),
            WhatYouWillLearnHash = await _contentHashService.ComputeCourseHashAsync(course.WhatYouWillLearn ?? ""),
            RequirementsHash = await _contentHashService.ComputeCourseHashAsync(course.Requirements ?? ""),
            ThumbnailHash = thumbHash
        };

        course.CourseExt = courseExt;
        return course;
    }

    private async Task UpdateCourseFingerprintAsync(Course course)
    {
        var existingExt = await _courseExtRepo.GetByIdAsync(course.CourseId);

        string? thumbHash = null;
        if (!string.IsNullOrEmpty(course.CourseThumbnailUrl))
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(course.CourseThumbnailUrl);
                thumbHash = await _contentHashService.ComputeFileHashAsync(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to download thumbnail for hashing: {ex.Message}");
            }
        }

        if (existingExt != null)
        {
            existingExt.TitleHash = await _contentHashService.ComputeCourseHashAsync(course.Title);
            existingExt.DescriptionHash = await _contentHashService.ComputeCourseHashAsync(course.Description ?? "");
            existingExt.WhatYouWillLearnHash = await _contentHashService.ComputeCourseHashAsync(course.WhatYouWillLearn ?? "");
            existingExt.RequirementsHash = await _contentHashService.ComputeCourseHashAsync(course.Requirements ?? "");
            existingExt.ThumbnailHash = thumbHash;
            _courseExtRepo.Update(existingExt);
        }
        else
        {
            var newExt = new CourseExt
            {
                CourseId = course.CourseId,
                TitleHash = await _contentHashService.ComputeCourseHashAsync(course.Title),
                DescriptionHash = await _contentHashService.ComputeCourseHashAsync(course.Description ?? ""),
                WhatYouWillLearnHash = await _contentHashService.ComputeCourseHashAsync(course.WhatYouWillLearn ?? ""),
                RequirementsHash = await _contentHashService.ComputeCourseHashAsync(course.Requirements ?? ""),
                ThumbnailHash = thumbHash
            };
            await _courseExtRepo.AddAsync(newExt);
        }
    }


    public async Task UpdateCourseStatusAsync(int courseId, string status, int instructorId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            throw new Exception("Course not found.");

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to modify this course.");

        var activeLockout = await _lockoutRepo.GetActiveLockoutAsync(instructorId, "instructor");
        if (activeLockout != null)
        {
            throw new BadRequestException($"Your instructor rights are locked until {activeLockout.LockoutEnd.Value:yyyy-MM-dd HH:mm:ss} due to policy violations. You cannot change course status.");
        }

        if (string.Equals(course.CourseStatus, CourseStatus.Archived.ToValue(), StringComparison.OrdinalIgnoreCase) && (course.CourseFlagCount ?? 0) >= 3)
        {
            throw new BadRequestException("This course has been permanently discontinued due to policy violations and its status cannot be changed.");
        }

        if (status.Equals(CourseStatus.Published.ToValue(), StringComparison.OrdinalIgnoreCase))
        {
            if (!CourseStatus.Archived.ToValue().Equals(course.CourseStatus, StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("Only archived courses can be set back to published by instructor.");
        }
        else if (!status.Equals(CourseStatus.Pending.ToValue(), StringComparison.OrdinalIgnoreCase) && !status.Equals(CourseStatus.Archived.ToValue(), StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Invalid status. Allowed values are 'pending', 'archived', or 'published' (for unarchiving).");
        }

        course.CourseStatus = status.ToLower();
        course.UpdatedAt = DateTime.UtcNow;

        if (status.Equals(CourseStatus.Pending.ToValue(), StringComparison.OrdinalIgnoreCase))
        {
            // Validate required landing page fields
            if (string.IsNullOrWhiteSpace(course.WhatYouWillLearn) || StripHtml(course.WhatYouWillLearn).Length < 20)
            {
                throw new BadRequestException("What you will learn is required and must be at least 20 characters long.");
            }

            if (string.IsNullOrWhiteSpace(course.Requirements) || StripHtml(course.Requirements).Length < 20)
            {
                throw new BadRequestException("Requirements are required and must be at least 20 characters long.");
            }

            if (string.IsNullOrWhiteSpace(course.CourseThumbnailUrl))
            {
                throw new BadRequestException("Course thumbnail is required.");
            }

            var instructor = await _instructorRepository.GetByIdAsync(instructorId);
            var isStripeActive = instructor != null
                && !string.IsNullOrEmpty(instructor.StripeAccountId)
                && string.Equals(instructor.StripeOnboardingStatus, StripeOnboardingStatus.Active.ToValue(), StringComparison.OrdinalIgnoreCase);

            var lessons = await _lessonRepository.GetByCourseIdAsync(courseId);
            var activeLessons = lessons.Where(l => !l.IsRemoved).ToList();

            if (!activeLessons.Any())
            {
                throw new BadRequestException("Cannot submit course for review. The course must have at least one lesson.");
            }

            int totalDurationSeconds = 0;
            foreach (var lesson in activeLessons)
            {
                bool hasVideo = lesson.LearningMaterials.Any(m => m.LearningStatus != LearningStatus.Removed.ToValue() && (m.MaterialMetadata?.FileType == "video" || m.MaterialMetadata == null));
                if (!hasVideo)
                {
                    throw new BadRequestException($"Cannot submit course for review. Every lesson must contain at least one video. Lesson '{lesson.Title}' is missing a video.");
                }

                foreach (var material in lesson.LearningMaterials.Where(m => m.LearningStatus != LearningStatus.Removed.ToValue() && (m.MaterialMetadata?.FileType == "video" || m.MaterialMetadata == null)))
                {
                    totalDurationSeconds += material.MaterialMetadata?.Duration ?? 0;
                }
            }

            double totalMinutes = totalDurationSeconds / 60.0;

            if (!isStripeActive && totalMinutes > 30)
            {
                throw new BadRequestException($"The total video duration of the course is currently {Math.Round(totalMinutes, 1)} minutes. Instructors who have not linked a Stripe account are only allowed a maximum of 30 minutes.");
            }

            bool isFreeCourse = course.Price == 0;
            if (isFreeCourse && totalMinutes > 60)
            {
                throw new BadRequestException($"The total video duration of the free course is currently {Math.Round(totalMinutes, 1)} minutes. Free courses are only allowed a maximum of 60 minutes.");
            }

            course.ModerationFeedback = null;

            var materials = await _materialRepository.GetByCourseIdAsync(courseId);
            if (materials != null)
            {
                foreach (var material in materials)
                {
                    if (material.LearningStatus != LearningStatus.Removed.ToValue())
                    {
                        material.ModerationFeedback = null;
                        if (material.LearningStatus != LearningStatus.Active.ToValue())
                        {
                            material.LearningStatus = LearningStatus.Active.ToValue();
                        }
                        _materialRepository.Update(material);
                    }
                }
            }

            lessons = await _lessonRepository.GetByCourseIdAsync(courseId);
            if (lessons != null)
            {
                foreach (var lesson in lessons)
                {
                    if (lesson.LessonStatus != LessonStatus.Active.ToValue())
                    {
                        lesson.LessonStatus = LessonStatus.Active.ToValue();
                        _lessonRepository.Update(lesson);
                    }
                }
            }
        }
        _courseRepository.Update(course);
        int rowsStatus = await _courseRepository.SaveChangesAsync();
        if (rowsStatus <= 0)
            throw new InvalidOperationException("Failed to save changes when updating course status.");

        await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));
    }

    public async Task DeleteCourseAsync(int courseId, int instructorId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            throw new Exception("Course not found.");

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to delete this course.");

        var activeLockout = await _lockoutRepo.GetActiveLockoutAsync(instructorId, "instructor");
        if (activeLockout != null)
        {
            throw new BadRequestException($"Your instructor rights are locked until {activeLockout.LockoutEnd.Value:yyyy-MM-dd HH:mm:ss} due to policy violations. You cannot delete courses.");
        }

        if (string.Equals(course.CourseStatus, CourseStatus.Archived.ToValue(), StringComparison.OrdinalIgnoreCase) && (course.CourseFlagCount ?? 0) >= 3)
        {
            throw new BadRequestException("This course has been permanently discontinued due to policy violations and cannot be deleted by the instructor.");
        }

        if (CourseStatus.Pending.ToValue().Equals(course.CourseStatus, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot delete course while it is pending review.");

        var hasEnrollments = await _courseRepository.HasEnrollmentsAsync(courseId);
        if (hasEnrollments)
            throw new BadRequestException("Cannot delete a course with active students. Please archive it instead.");

        course.IsRemoved = true;
        course.UpdatedAt = DateTime.UtcNow;

        var lessons = await _lessonRepository.GetByCourseIdAsync(courseId);
        foreach (var lesson in lessons)
        {
            lesson.IsRemoved = true;
            lesson.UpdatedAt = DateTime.UtcNow;
            _lessonRepository.Update(lesson);
        }
        _courseRepository.Update(course);
        int rowsDelete = await _courseRepository.SaveChangesAsync();
        if (rowsDelete <= 0)
            throw new InvalidOperationException("Failed to save changes when deleting course.");

        await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));
    }

    public async Task<CourseAIIntegrationResult> IntegrateAItoCourseAsync(CourseAIIntegrationCommand command)
    {
        var integration = new CourseAiIntegration
        {
            CourseId = command.CourseId,
            ModelId = command.ModelId,
            Role = command.Role,
            IsEnabled = command.IsEnabled,
            ConfigJson = JsonSerializer.Serialize(command.ConfigJson)
        };

        await _aiIntegrationRepository.AddAsync(integration);
        int rowsAi = await _aiIntegrationRepository.SaveChangesAsync();
        if (rowsAi <= 0)
            throw new InvalidOperationException("Failed to save changes when integrating AI to course.");
        return new CourseAIIntegrationResult
        {
            CourseId = command.CourseId,
            ModelId = command.ModelId,
            IsEnabled = command.IsEnabled,
            ConfigJson = command.ConfigJson
            ?? new Dictionary<string, float> {
                { AiModelConst.Similarity, AiModelConst.DefaultSimilarityScoreThreshold },
                { AiModelConst.Spam, AiModelConst.DefaultSpamScoreThreshold },
                { AiModelConst.Toxic, AiModelConst.DefaultToxicScoreThreshold }
             },
            Role = command.Role
        };
    }

    public async Task UpdateCourseStatusAndFeedbackAsync(int courseId, string status, string? feedback, AiThreatLevel? threatLevel = null)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course != null)
        {
            course.CourseStatus = status.ToLower();
            course.ModerationFeedback = feedback;
            if (threatLevel.HasValue)
            {
                course.ThreatLevel = threatLevel.Value;
            }
            course.UpdatedAt = DateTime.UtcNow;
            _courseRepository.Update(course);
            int rowsFeedback = await _courseRepository.SaveChangesAsync();
            if (rowsFeedback <= 0)
                throw new InvalidOperationException("Failed to save changes when updating course status and feedback.");

            await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));
        }
    }

    private string StripHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty).Trim();
    }
}
