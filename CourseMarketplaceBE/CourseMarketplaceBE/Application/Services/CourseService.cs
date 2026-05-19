using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace CourseMarketplaceBE.Application.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IInstructorRepository _instructorRepository;
    private readonly IFileUploadService _uploadService;
    private readonly IMaterialRepository _materialRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IRedisService _redisService;
    private readonly IContentHashService _contentHashService;
    private readonly ICourseAiIntegrationRepository _aiIntegrationRepository;
    private readonly ILogger<CourseService> _logger;
    private readonly HttpClient _httpClient;

    public CourseService(
        ICourseRepository courseRepository, 
        IInstructorRepository instructorRepository, 
        IFileUploadService uploadService, 
        IMaterialRepository materialRepository, 
        ILessonRepository lessonRepository, 
        IRedisService redisService,
        IContentHashService contentHashService,
        ICourseAiIntegrationRepository aiIntegrationRepository,
        ILogger<CourseService> logger,
        HttpClient httpClient)
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
        _httpClient = httpClient;
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

    public async Task<PaginatedCoursesResponse> GetPublishedCoursesPagedAsync(
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

        var courseResponses = courses.Select(c =>
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
        }).ToList();

        int finalPage = page ?? 1;
        int finalPageSize = pageSize ?? 12;
        int totalPages = (int)Math.Ceiling(totalCount / (double)finalPageSize);

        return new PaginatedCoursesResponse
        {
            Courses = courseResponses,
            TotalItems = totalCount,
            TotalPages = totalPages > 0 ? totalPages : 1,
            CurrentPage = finalPage,
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

    public async Task<PaginatedCoursesResponse> GetInstructorCoursesPagedAsync(
        int instructorId,
        string? search = null,
        string? status = null,
        int? page = null,
        int? pageSize = null)
    {
        var (courses, totalCount) = await _courseRepository.GetInstructorCoursesPagedAsync(instructorId, search, status, page, pageSize);
        var courseIds = courses.Select(c => c.CourseId).ToList();

        // Fetch stats for these courses
        var stats = await _courseRepository.GetCourseStatsAsync(courseIds);

        var courseResponses = courses.Select(c =>
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
        }).ToList();

        int finalPage = page ?? 1;
        int finalPageSize = pageSize ?? 6;
        int totalPages = (int)Math.Ceiling(totalCount / (double)finalPageSize);

        return new PaginatedCoursesResponse
        {
            Courses = courseResponses,
            TotalItems = totalCount,
            TotalPages = totalPages > 0 ? totalPages : 1,
            CurrentPage = finalPage,
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

            response = new CourseDetailResponse
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
                AppliedCouponCode = course.Coupon?.CouponCode,
                AppliedCouponType = course.Coupon?.CouponType,
                AppliedCouponValue = course.Coupon?.DiscountValue,
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
                LastApprovedAt = course.LastApprovedAt,
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
                        UpdatedAt = m.UpdatedAt,
                        LearningStatus = m.LearningStatus,
                        ModerationFeedback = m.ModerationFeedback
                    }).ToList()
                }).ToList()
            };

            await _redisService.SetCacheAsync(cacheKey, response, CacheTtl.Short.GetTtl());
        }

        // Cập nhật thông tin định danh riêng cho từng User (Không cache phần này)
        if (userId.HasValue)
        {
            response.IsOwner = response.InstructorId == userId.Value;
            // Bypass: Nếu là chủ sở hữu thì luôn coi như đã ghi danh
            response.IsEnrolled = response.IsOwner || await _courseRepository.IsEnrolledAsync(userId.Value, courseId);
        }
        else
        {
            response.IsEnrolled = false;
            response.IsOwner = false;
        }

        return response;
    }


    public async Task<CourseResponse> CreateCourseAsync(CourseCreateRequest request, int instructorId)
    {
        var instructor = await _instructorRepository.GetByIdAsync(instructorId);
        if (instructor == null || !string.Equals(instructor.ApprovalStatus, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("You must be an approved instructor to create a course.");
        }

        // ★ Limit: Max 2 courses for unlinked Stripe
        var isStripeActive = !string.IsNullOrEmpty(instructor.StripeAccountId)
            && string.Equals(instructor.StripeOnboardingStatus, "Active", StringComparison.OrdinalIgnoreCase);

        if (!isStripeActive)
        {
            var instructorCourses = await _courseRepository.GetInstructorCoursesAsync(instructorId);
            if (instructorCourses.Count(c => !c.IsRemoved) >= 2)
            {
                throw new BadRequestException("Instructors who have not linked a Stripe account are only allowed to create up to 2 courses.");
            }
        }


        // ★ Kiểm tra trùng lặp tiêu đề (Case-insensitive)
        var titleLower = request.Title.Trim().ToLower();
        var existingCourse = await _courseRepository.GetAllPublishedCoursesAsync(); // Hoặc dùng một phương thức chuyên biệt
        // Thực tế nên dùng một phương thức CheckTitleExists trong Repository để tối ưu hơn.
        // Tôi sẽ tạm thời kiểm tra thông qua GetInstructorCoursesAsync để tránh load quá nhiều dữ liệu nếu chưa có hàm chuyên biệt.
        var allCourses = await _courseRepository.GetInstructorCoursesAsync(instructorId);
        if (allCourses.Any(c => c.Title.Trim().ToLower() == titleLower && !c.IsRemoved))
        {
            throw new BadRequestException("You already have a course with this title. Please choose a different title.");
        }

        // ★ Nếu chưa hoàn tất Stripe → ép giá = 0 (chỉ tạo khóa free)
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
            InstructorId = instructorId,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            Price = coursePrice,
            CourseThumbnailUrl = thumbnailUrl,
            WhatYouWillLearn = request.WhatYouWillLearn,
            Requirements = request.Requirements,
            CourseStatus = "draft", // Default status
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _courseRepository.AddAsync(course);
        await _courseRepository.SaveChangesAsync();

        // 2. Compute & store hashes for deduplication
        await UpdateCourseHashesAsync(course);

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

        // ★ Block updates if course is archived by moderation (3+ flags)
        if (string.Equals(course.CourseStatus, "archived", StringComparison.OrdinalIgnoreCase) && (course.CourseFlagCount ?? 0) >= 3)
        {
            throw new BadRequestException("This course has been permanently discontinued due to policy violations and cannot be edited.");
        }

        if ("pending".Equals(course.CourseStatus, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot modify course while it is pending review.");

        // ★ Kiểm tra trùng lặp tiêu đề khi cập nhật (nếu tiêu đề thay đổi)
        if (!string.Equals(course.Title, request.Title, StringComparison.OrdinalIgnoreCase))
        {
            var titleLower = request.Title.Trim().ToLower();
            var instructorCourses = await _courseRepository.GetInstructorCoursesAsync(instructorId);
            if (instructorCourses.Any(c => c.Title.Trim().ToLower() == titleLower && c.CourseId != courseId && !c.IsRemoved))
            {
                throw new BadRequestException("You already have another course with this title.");
            }
        }

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

        // ★ Nếu chưa hoàn tất Stripe → ép giá = 0
        var instructor = await _instructorRepository.GetByIdAsync(instructorId);
        var isStripeActive = instructor != null
            && !string.IsNullOrEmpty(instructor.StripeAccountId)
            && string.Equals(instructor.StripeOnboardingStatus, "Active", StringComparison.OrdinalIgnoreCase);
        course.Price = isStripeActive ? request.Price : 0m;

        course.CourseThumbnailUrl = thumbnailUrl;
        course.WhatYouWillLearn = request.WhatYouWillLearn;
        course.Requirements = request.Requirements;
        course.UpdatedAt = DateTime.UtcNow;

        // Nếu khóa học đang ở trạng thái Published, chuyển về Draft để sửa
        if ("published".Equals(course.CourseStatus, StringComparison.OrdinalIgnoreCase))
        {
            course.CourseStatus = "draft";
            // Tùy chọn: Xóa feedback cũ khi có thay đổi mới
            course.ModerationFeedback = null;
        }

        _courseRepository.Update(course);
        await _courseRepository.SaveChangesAsync();

        // Update Hashes
        await UpdateCourseHashesAsync(course);

        await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(course.CourseId));

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

    private async Task UpdateCourseHashesAsync(Course course)
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

        var command = new SaveCourseHashesCommand
        {
            CourseId = course.CourseId,
            title_hash = await _contentHashService.ComputeCourseHashAsync(course.Title),
            description_hash = await _contentHashService.ComputeCourseHashAsync(course.Description ?? ""),
            what_you_will_learn_hash = await _contentHashService.ComputeCourseHashAsync(course.WhatYouWillLearn ?? ""),
            requirements_hash = await _contentHashService.ComputeCourseHashAsync(course.Requirements ?? ""),
            thumbnail_hash = thumbHash
        };

        await _contentHashService.SaveCourseHashesAsync(command);
    }

    public async Task UpdateCourseStatusAsync(int courseId, string status, int instructorId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            throw new Exception("Course not found.");

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to modify this course.");

        // ★ Block status changes if course is archived by moderation (3+ flags)
        if (string.Equals(course.CourseStatus, "archived", StringComparison.OrdinalIgnoreCase) && (course.CourseFlagCount ?? 0) >= 3)
        {
            throw new BadRequestException("This course has been permanently discontinued due to policy violations and its status cannot be changed.");
        }

        // Instructor chỉ được gửi yêu cầu duyệt (pending) hoặc ẩn khóa học (archived)
        // Việc chuyển sang "published" chỉ được phép nếu khóa học đang ở trạng thái "archived" (Unhide)
        // Các trường hợp khác để lên "published" phải qua Admin
        if (status.Equals("published", StringComparison.OrdinalIgnoreCase))
        {
            if (!"archived".Equals(course.CourseStatus, StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("Only archived courses can be set back to published by instructor.");
        }
        else if (!status.Equals("pending", StringComparison.OrdinalIgnoreCase) && !status.Equals("archived", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Invalid status. Allowed values are 'pending', 'archived', or 'published' (for unarchiving).");
        }

        course.CourseStatus = status.ToLower();
        course.UpdatedAt = DateTime.UtcNow;

        if (status.Equals("pending", StringComparison.OrdinalIgnoreCase))
        {
            // ★ Limit: Total Video Duration
            var instructor = await _instructorRepository.GetByIdAsync(instructorId);
            var isStripeActive = instructor != null
                && !string.IsNullOrEmpty(instructor.StripeAccountId)
                && string.Equals(instructor.StripeOnboardingStatus, "Active", StringComparison.OrdinalIgnoreCase);

            var lessons = await _lessonRepository.GetByCourseIdAsync(courseId);
            int totalDurationSeconds = 0;
            foreach (var lesson in lessons.Where(l => !l.IsRemoved))
            {
                foreach (var material in lesson.LearningMaterials.Where(m => m.LearningStatus != "removed" && (m.MaterialMetadata?.FileType == "video" || m.MaterialMetadata == null)))
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

            // Clear moderation feedback when resubmitting

            course.ModerationFeedback = null;
            
            var materials = await _materialRepository.GetByCourseIdAsync(courseId);
            if (materials != null)
            {
                foreach (var material in materials)
                {
                    material.ModerationFeedback = null;
                    if (material.LearningStatus == "rejected")
                    {
                        material.LearningStatus = "active";
                    }
                    _materialRepository.Update(material);
                }
            }

            // Clear lesson status when resubmitting
            lessons = await _lessonRepository.GetByCourseIdAsync(courseId);
            if (lessons != null)
            {
                foreach (var lesson in lessons)
                {
                    lesson.LessonStatus = "active";
                    _lessonRepository.Update(lesson);
                }
            }
        }
        _courseRepository.Update(course);
        await _courseRepository.SaveChangesAsync();

        // Invalidate Cache
        await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));
    }

    public async Task DeleteCourseAsync(int courseId, int instructorId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            throw new Exception("Course not found.");

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to delete this course.");

        if ("pending".Equals(course.CourseStatus, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot delete course while it is pending review.");

        var hasEnrollments = await _courseRepository.HasEnrollmentsAsync(courseId);
        if (hasEnrollments)
            throw new BadRequestException("Cannot delete a course with active students. Please archive it instead.");

        course.IsRemoved = true;
        course.UpdatedAt = DateTime.UtcNow;

        // Optional: Soft delete all lessons in the course as well
        var lessons = await _lessonRepository.GetByCourseIdAsync(courseId);
        foreach (var lesson in lessons)
        {
            lesson.IsRemoved = true;
            lesson.UpdatedAt = DateTime.UtcNow;
            _lessonRepository.Update(lesson);
        }
        _courseRepository.Update(course);
        await _courseRepository.SaveChangesAsync();

        // Invalidate Cache
        await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));
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

    public async Task<CourseAIIntegrationResult> IntegrateAItoCourseAsync(CourseAIIntegrationCommand command)
    {
        var integration = new CourseAiIntegration
        {
            CourseId = command.CourseId,
            ModelId = command.ModelId,
            Role = command.Role,
            IsEnabled = command.IsEnabled,
            ConfigJson = System.Text.Json.JsonSerializer.Serialize(command.ConfigJson)
        };

        await _aiIntegrationRepository.AddAsync(integration);
        await _aiIntegrationRepository.SaveChangesAsync();
        return new CourseAIIntegrationResult
        {
            CourseId = command.CourseId,
            ModelId = command.ModelId,
            IsEnabled = command.IsEnabled,
            ConfigJson = command.ConfigJson 
            ?? new Dictionary<string, float> { { "similarity", 0.8f }, { "spam", 0.7f }, { "toxic", 0.7f } },
            Role = command.Role
        };
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

    public async Task UpdateCourseStatusAndFeedbackAsync(int courseId, string status, string? feedback)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course != null)
        {
            course.CourseStatus = status.ToLower();
            course.ModerationFeedback = feedback;
            course.UpdatedAt = DateTime.UtcNow;
            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();

            // Invalidate Cache
            await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));
        }
    }
}
