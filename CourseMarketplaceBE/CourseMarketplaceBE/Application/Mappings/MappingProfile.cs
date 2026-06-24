using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Constants;
using System;

namespace CourseMarketplaceBE.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Notification, NotificationResponseDto>();

        // Reports Mapping
        CreateMap<CourseReport, MyCourseReportResponse>()
            .ForMember(dest => dest.ReportId, opt => opt.MapFrom(src => src.CourseReportId))
            .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.CourseReportsStatus));

        CreateMap<CourseReviewReport, MyReviewReportResponse>()
            .ForMember(dest => dest.ReportId, opt => opt.MapFrom(src => src.CourseReviewReportId))
            .ForMember(dest => dest.ReviewType, opt => opt.MapFrom(src => "course_review"))
            .ForMember(dest => dest.ReviewComment, opt => opt.MapFrom(src => src.CourseReview.Comment))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.UserReportsStatus));

        CreateMap<LessonReviewReport, MyReviewReportResponse>()
            .ForMember(dest => dest.ReportId, opt => opt.MapFrom(src => src.LessonReviewReportId))
            .ForMember(dest => dest.ReviewType, opt => opt.MapFrom(src => "lesson_review"))
            .ForMember(dest => dest.ReviewComment, opt => opt.MapFrom(src => src.LessonReview.Comment))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.UserReportsStatus));

        CreateMap<CourseReport, CourseReportDetailResponse>()
            .ForMember(dest => dest.ReportId, opt => opt.MapFrom(src => src.CourseReportId))
            .ForMember(dest => dest.ReporterEmail, opt => opt.MapFrom(src => src.Reporter.Email))
            .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter != null ? (src.Reporter.User != null ? src.Reporter.User.FullName : src.Reporter.Username) : null))
            .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title))
            .ForMember(dest => dest.CourseFlagCount, opt => opt.MapFrom(src => src.Course.CourseFlagCount ?? 0))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.CourseReportsStatus))
            .ForMember(dest => dest.ResolverEmail, opt => opt.MapFrom(src => src.Resolver != null ? src.Resolver.Email : null));

        CreateMap<CourseReviewReport, ReviewReportDetailResponse>()
            .ForMember(dest => dest.ReportId, opt => opt.MapFrom(src => src.CourseReviewReportId))
            .ForMember(dest => dest.ReviewType, opt => opt.MapFrom(src => "course_review"))
            .ForMember(dest => dest.ReporterEmail, opt => opt.MapFrom(src => src.Reporter.Email))
            .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter != null ? (src.Reporter.User != null ? src.Reporter.User.FullName : src.Reporter.Username) : null))
            .ForMember(dest => dest.ReviewComment, opt => opt.MapFrom(src => src.CourseReview.Comment))
            .ForMember(dest => dest.ReviewRating, opt => opt.MapFrom(src => src.CourseReview.Rating))
            .ForMember(dest => dest.ReviewAuthorName, opt => opt.MapFrom(src => src.CourseReview != null && src.CourseReview.Enrollment != null && src.CourseReview.Enrollment.User != null ? src.CourseReview.Enrollment.User.FullName : null))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseReview != null && src.CourseReview.Enrollment != null ? src.CourseReview.Enrollment.CourseId : null))
            .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.CourseReview != null && src.CourseReview.Enrollment != null && src.CourseReview.Enrollment.Course != null ? src.CourseReview.Enrollment.Course.Title : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.UserReportsStatus))
            .ForMember(dest => dest.ResolverEmail, opt => opt.MapFrom(src => src.Resolver.Email));

        CreateMap<LessonReviewReport, ReviewReportDetailResponse>()
            .ForMember(dest => dest.ReportId, opt => opt.MapFrom(src => src.LessonReviewReportId))
            .ForMember(dest => dest.ReviewType, opt => opt.MapFrom(src => "lesson_review"))
            .ForMember(dest => dest.ReporterEmail, opt => opt.MapFrom(src => src.Reporter.Email))
            .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter != null ? (src.Reporter.User != null ? src.Reporter.User.FullName : src.Reporter.Username) : null))
            .ForMember(dest => dest.ReviewComment, opt => opt.MapFrom(src => src.LessonReview.Comment))
            .ForMember(dest => dest.ReviewRating, opt => opt.MapFrom(src => src.LessonReview.Rating))
            .ForMember(dest => dest.ReviewAuthorName, opt => opt.MapFrom(src => src.LessonReview != null && src.LessonReview.Enrollment != null && src.LessonReview.Enrollment.User != null ? src.LessonReview.Enrollment.User.FullName : null))
            .ForMember(dest => dest.LessonId, opt => opt.MapFrom(src => src.LessonReview != null ? src.LessonReview.LessonId : null))
            .ForMember(dest => dest.LessonTitle, opt => opt.MapFrom(src => src.LessonReview != null && src.LessonReview.Lesson != null ? src.LessonReview.Lesson.Title : null))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.LessonReview != null && src.LessonReview.Lesson != null ? src.LessonReview.Lesson.CourseId : null))
            .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.LessonReview != null && src.LessonReview.Lesson != null && src.LessonReview.Lesson.Course != null ? src.LessonReview.Lesson.Course.Title : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.UserReportsStatus))
            .ForMember(dest => dest.ResolverEmail, opt => opt.MapFrom(src => src.Resolver.Email));

        // Course Service Mapping
        CreateMap<Course, CourseResponse>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoriesName : null))
            .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor != null && src.Instructor.InstructorNavigation != null ? src.Instructor.InstructorNavigation.FullName : "Unknown Instructor"))
            .ForMember(dest => dest.InstructorAvatarUrl, opt => opt.MapFrom(src => src.Instructor != null && src.Instructor.InstructorNavigation != null && src.Instructor.InstructorNavigation.UserNavigation != null ? src.Instructor.InstructorNavigation.UserNavigation.AvatarUrl : null))
            .ForMember(dest => dest.FlagCount, opt => opt.MapFrom(src => src.CourseFlagCount ?? 0))
            .ForMember(dest => dest.TotalStudents, opt => opt.Ignore())
            .ForMember(dest => dest.RatingAverage, opt => opt.Ignore())
            .ForMember(dest => dest.TotalReviews, opt => opt.Ignore())
            .ForMember(dest => dest.IsEnrolled, opt => opt.Ignore())
            .ForMember(dest => dest.IsOwner, opt => opt.Ignore());

        CreateMap<Course, CourseDetailResponse>()
            .IncludeBase<Course, CourseResponse>()
            .ForMember(dest => dest.AppliedCouponCode, opt => opt.MapFrom(src => src.Coupon != null ? src.Coupon.CouponCode : null))
            .ForMember(dest => dest.AppliedCouponType, opt => opt.MapFrom(src => src.Coupon != null ? src.Coupon.CouponType : null))
            .ForMember(dest => dest.AppliedCouponValue, opt => opt.MapFrom(src => src.Coupon != null ? (decimal?)src.Coupon.DiscountValue : null))
            .ForMember(dest => dest.InstructorBio, opt => opt.MapFrom(src => src.Instructor != null && src.Instructor.InstructorNavigation != null ? src.Instructor.InstructorNavigation.Bio : null))
            .ForMember(dest => dest.InstructorProfessionalTitle, opt => opt.MapFrom(src => src.Instructor != null ? src.Instructor.ProfessionalTitle : null))
            .ForMember(dest => dest.InstructorCoursesCount, opt => opt.MapFrom(src => src.Instructor != null && src.Instructor.Courses != null ? src.Instructor.Courses.Count : 0))
            .ForMember(dest => dest.InstructorReviewCount, opt => opt.Ignore())
            .ForMember(dest => dest.InstructorStudentsCount, opt => opt.Ignore())
            .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons))
            .ForMember(dest => dest.CourseQuizzes, opt => opt.MapFrom(src => src.CourseQuizzes));

        CreateMap<CourseQuiz, CourseQuizItemResponse>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Quiz != null ? src.Quiz.Title : ""))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Quiz != null ? src.Quiz.Description : null))
            .ForMember(dest => dest.QuestionCount, opt => opt.MapFrom(src => src.Quiz != null ? src.Quiz.TotalQuestions : 0))
            .ForMember(dest => dest.TimeLimitMinutes, opt => opt.MapFrom(src => src.Quiz != null ? src.Quiz.TimeLimitMinutes : null));

        CreateMap<Course, CourseModerationDetailResponse>()
            .ForMember(dest => dest.CourseThumbnailUrl, opt => opt.MapFrom(src => src.CourseThumbnailUrl))
            .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons));

        CreateMap<Lesson, LessonResponse>()
            .ForMember(dest => dest.LearningMaterials, opt => opt.MapFrom(src => src.LearningMaterials));

        CreateMap<LearningMaterial, MaterialResponse>();

        CreateMap<Category, CategoryResponse>();

        // Admin AI Service Log Mapping
        CreateMap<AiModel, AiModelAdminDto>();

        CreateMap<CourseAiUsageLog, CourseModerationLogAdminDto>()
            .ForMember(dest => dest.ModelId, opt => opt.MapFrom(src => src.CourseAiIntegration != null ? (src.CourseAiIntegration.ModelId ?? 0) : 0))
            .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.CourseAiIntegration != null && src.CourseAiIntegration.Model != null ? src.CourseAiIntegration.Model.ModelName : "Unknown"))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseAiIntegration != null ? (src.CourseAiIntegration.CourseId ?? 0) : 0))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.CourseAiIntegration != null && src.CourseAiIntegration.Course != null ? src.CourseAiIntegration.Course.Title : "Unknown"))
            .ForMember(dest => dest.TokenUsage, opt => opt.MapFrom(src => (int)(src.TokenUsage ?? 0)))
            .ForMember(dest => dest.LatencyMs, opt => opt.MapFrom(src => (int)(src.LatencyMs ?? 0)))
            .ForMember(dest => dest.LogCreatedAt, opt => opt.MapFrom(src => src.LogCreatedAt ?? DateTime.UtcNow))
            .AfterMap((src, dest) => MapResultStatus(src.ErrorMessage, src.OutputJson, dest));

        CreateMap<CourseReviewModerationLog, ReviewModerationLogAdminDto>()
            .ForMember(dest => dest.ModelId, opt => opt.MapFrom(src => src.ModelId ?? 0))
            .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.Model != null ? src.Model.ModelName : "Unknown"))
            .ForMember(dest => dest.ReviewId, opt => opt.MapFrom(src => src.CourseReviewId ?? 0))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.CourseReview != null ? (src.CourseReview.Comment ?? "") : ""))
            .ForMember(dest => dest.LatencyMs, opt => opt.MapFrom(src => (int)(src.LatencyMs ?? 0)))
            .ForMember(dest => dest.LogCreatedAt, opt => opt.MapFrom(src => src.LogCreatedAt ?? DateTime.UtcNow))
            .AfterMap((src, dest) => MapResultStatus(src.ErrorMessage, src.OutputJson, dest));

        CreateMap<LessonReviewModerationLog, ReviewModerationLogAdminDto>()
            .ForMember(dest => dest.ModelId, opt => opt.MapFrom(src => src.ModelId ?? 0))
            .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.Model != null ? src.Model.ModelName : "Unknown"))
            .ForMember(dest => dest.ReviewId, opt => opt.MapFrom(src => src.LessonReviewId ?? 0))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.LessonReview != null ? (src.LessonReview.Comment ?? "") : ""))
            .ForMember(dest => dest.LatencyMs, opt => opt.MapFrom(src => (int)(src.LatencyMs ?? 0)))
            .ForMember(dest => dest.LogCreatedAt, opt => opt.MapFrom(src => src.LogCreatedAt ?? DateTime.UtcNow))
            .AfterMap((src, dest) => MapResultStatus(src.ErrorMessage, src.OutputJson, dest));
    }

    private static void MapResultStatus(string? errorMessage, string? outputJson, BaseAiLogAdminDto dest)
    {
        dest.ResultStatus = string.IsNullOrEmpty(errorMessage) ? "Processed" : "ERROR";

        if (dest.ResultStatus == "ERROR") return;

        dest.ResultStatus = "Pass";
        if (!string.IsNullOrEmpty(outputJson))
        {
            try {
                using var doc = System.Text.Json.JsonDocument.Parse(outputJson);
                if (doc.RootElement.TryGetProperty("result", out var resProp))
                {
                    var rawResult = resProp.GetString()?.ToUpperInvariant();
                    if (rawResult == StageLogResult.Flagged.ToValue()) dest.ResultStatus = "Flagged";
                    else if (rawResult == StageLogResult.MatchFound.ToValue()) dest.ResultStatus = "Match Found";
                    else if (rawResult == StageLogResult.ManualAudit.ToValue()) dest.ResultStatus = "Manual Audit";
                    else if (rawResult == StageLogResult.NoMatch.ToValue()) dest.ResultStatus = "No Match";
                    else if (rawResult == StageLogResult.Approved.ToValue()) dest.ResultStatus = "Approved";
                    else dest.ResultStatus = "Pass";
                }
            } catch { }
        }
    }
}
