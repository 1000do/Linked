using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
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
            .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title))
            .ForMember(dest => dest.CourseFlagCount, opt => opt.MapFrom(src => src.Course.CourseFlagCount ?? 0))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.CourseReportsStatus))
            .ForMember(dest => dest.ResolverEmail, opt => opt.MapFrom(src => src.Resolver.Email));

        CreateMap<CourseReviewReport, ReviewReportDetailResponse>()
            .ForMember(dest => dest.ReportId, opt => opt.MapFrom(src => src.CourseReviewReportId))
            .ForMember(dest => dest.ReviewType, opt => opt.MapFrom(src => "course_review"))
            .ForMember(dest => dest.ReporterEmail, opt => opt.MapFrom(src => src.Reporter.Email))
            .ForMember(dest => dest.ReporterName, opt => opt.Ignore())
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
            .ForMember(dest => dest.ReporterName, opt => opt.Ignore())
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
            .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons));

        CreateMap<Lesson, LessonResponse>()
            .ForMember(dest => dest.LearningMaterials, opt => opt.MapFrom(src => src.LearningMaterials));

        CreateMap<LearningMaterial, MaterialResponse>();

        CreateMap<Category, CategoryResponse>();
    }
}
