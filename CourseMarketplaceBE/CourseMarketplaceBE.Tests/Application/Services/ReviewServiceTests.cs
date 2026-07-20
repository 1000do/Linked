using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Application.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class ReviewServiceTests
    {
        private readonly IReviewRepository _reviewRepoMock;
        private readonly IEnrollmentRepository _enrollmentRepoMock;
        private readonly ICourseRepository _courseRepoMock;
        private readonly INotificationService _notifMock;
        private readonly IReportSubmissionService _reportServiceMock;
        private readonly IUserRepository _userRepoMock;
        private readonly ILockoutRepository _lockoutRepoMock;
        private readonly ReviewService _sut;

        public ReviewServiceTests()
        {
            _reviewRepoMock = Substitute.For<IReviewRepository>();
            _enrollmentRepoMock = Substitute.For<IEnrollmentRepository>();
            _courseRepoMock = Substitute.For<ICourseRepository>();
            _notifMock = Substitute.For<INotificationService>();
            _reportServiceMock = Substitute.For<IReportSubmissionService>();
            _userRepoMock = Substitute.For<IUserRepository>();
            _lockoutRepoMock = Substitute.For<ILockoutRepository>();

            _sut = new ReviewService(
                _reviewRepoMock,
                _enrollmentRepoMock,
                _courseRepoMock,
                _notifMock,
                _reportServiceMock,
                _userRepoMock,
                _lockoutRepoMock
            );
        }

        // ----------------------------------------------------
        // GetEnrollmentStatusAsync Tests
        // ----------------------------------------------------

        [Fact]
        public async Task GetEnrollmentStatusAsync_IsOwner_ReturnsCorrectStatus()
        {
            int userId = 1;
            int courseId = 2;
            _courseRepoMock.IsOwnerAsync(userId, courseId).Returns(true);
            _courseRepoMock.GetCourseStatsAsync(courseId).Returns(new CourseStats { TotalMaterials = 5 });
            var enrollment = new Enrollment { EnrollmentId = 10, UserId = userId, CourseId = courseId };
            _enrollmentRepoMock.GetEnrollmentWithProgressAsync(userId, courseId).Returns(enrollment);
            _reviewRepoMock.GetCourseReviewByEnrollmentAsync(10).Returns(new CourseReview());

            var result = await _sut.GetEnrollmentStatusAsync(userId, courseId);

            result.IsOwner.Should().BeTrue();
            result.IsEnrolled.Should().BeTrue();
            result.IsCompleted.Should().BeTrue();
            result.ProgressPercentage.Should().Be(100);
            result.LearnedMaterialCount.Should().Be(5);
            result.TotalMaterialCount.Should().Be(5);
            result.CanReview.Should().BeTrue();
            result.ReviewBlockedReason.Should().BeNull();
            result.HasReviewed.Should().BeTrue();
        }

        [Fact]
        public async Task GetEnrollmentStatusAsync_IsOwner_WithoutEnrollmentAndMaterials_ReturnsCorrectStatus()
        {
            int userId = 1;
            int courseId = 2;
            _courseRepoMock.IsOwnerAsync(userId, courseId).Returns(true);
            _courseRepoMock.GetCourseStatsAsync(courseId).Returns((CourseStats)null);
            _enrollmentRepoMock.GetEnrollmentWithProgressAsync(userId, courseId).Returns((Enrollment)null);

            var result = await _sut.GetEnrollmentStatusAsync(userId, courseId);

            result.IsOwner.Should().BeTrue();
            result.IsEnrolled.Should().BeTrue();
            result.IsCompleted.Should().BeTrue();
            result.ProgressPercentage.Should().Be(100);
            result.LearnedMaterialCount.Should().Be(0);
            result.TotalMaterialCount.Should().Be(0);
            result.CanReview.Should().BeTrue();
            result.ReviewBlockedReason.Should().BeNull();
            result.HasReviewed.Should().BeFalse();
        }

        [Fact]
        public async Task GetEnrollmentStatusAsync_NotOwnerAndNotEnrolled_ReturnsFalse()
        {
            int userId = 1;
            int courseId = 2;
            _courseRepoMock.IsOwnerAsync(userId, courseId).Returns(false);
            _enrollmentRepoMock.GetEnrollmentWithProgressAsync(userId, courseId).Returns((Enrollment)null);

            var result = await _sut.GetEnrollmentStatusAsync(userId, courseId);

            result.IsOwner.Should().BeFalse();
            result.IsEnrolled.Should().BeFalse();
            result.CanReview.Should().BeFalse();
            result.ReviewBlockedReason.Should().Be("You need to enroll in the course before writing a review.");
        }

        [Fact]
        public async Task GetEnrollmentStatusAsync_NotOwnerAndEnrolled_ZeroProgress_ReturnsCorrectStatus()
        {
            int userId = 1;
            int courseId = 2;
            var enrollment = new Enrollment { EnrollmentId = 10, IsCompleted = false };
            _courseRepoMock.IsOwnerAsync(userId, courseId).Returns(false);
            _enrollmentRepoMock.GetEnrollmentWithProgressAsync(userId, courseId).Returns(enrollment);
            _courseRepoMock.GetCourseStatsAsync(courseId).Returns(new CourseStats { TotalMaterials = 10 });
            _enrollmentRepoMock.GetCompletedMaterialCountAsync(10).Returns(0);
            _reviewRepoMock.GetCourseReviewByEnrollmentAsync(10).Returns((CourseReview)null);

            var result = await _sut.GetEnrollmentStatusAsync(userId, courseId);

            result.IsOwner.Should().BeFalse();
            result.IsEnrolled.Should().BeTrue();
            result.IsCompleted.Should().BeFalse();
            result.ProgressPercentage.Should().Be(0);
            result.LearnedMaterialCount.Should().Be(0);
            result.TotalMaterialCount.Should().Be(10);
            result.CanReview.Should().BeFalse();
            result.ReviewBlockedReason.Should().Be("You need to complete at least 1 lesson before writing a review.");
            result.HasReviewed.Should().BeFalse();
        }

        [Fact]
        public async Task GetEnrollmentStatusAsync_NotOwnerAndEnrolled_WithProgress_ReturnsCorrectStatus()
        {
            int userId = 1;
            int courseId = 2;
            var enrollment = new Enrollment { EnrollmentId = 10, IsCompleted = true };
            _courseRepoMock.IsOwnerAsync(userId, courseId).Returns(false);
            _enrollmentRepoMock.GetEnrollmentWithProgressAsync(userId, courseId).Returns(enrollment);
            _courseRepoMock.GetCourseStatsAsync(courseId).Returns(new CourseStats { TotalMaterials = 10 });
            _enrollmentRepoMock.GetCompletedMaterialCountAsync(10).Returns(5);
            _reviewRepoMock.GetCourseReviewByEnrollmentAsync(10).Returns(new CourseReview());

            var result = await _sut.GetEnrollmentStatusAsync(userId, courseId);

            result.IsOwner.Should().BeFalse();
            result.IsEnrolled.Should().BeTrue();
            result.IsCompleted.Should().BeTrue();
            result.ProgressPercentage.Should().Be(50);
            result.LearnedMaterialCount.Should().Be(5);
            result.TotalMaterialCount.Should().Be(10);
            result.CanReview.Should().BeTrue();
            result.ReviewBlockedReason.Should().BeNull();
            result.HasReviewed.Should().BeTrue();
        }
        
        [Fact]
        public async Task GetEnrollmentStatusAsync_NotOwnerAndEnrolled_WithZeroTotalMaterials_ReturnsZeroProgress()
        {
            int userId = 1;
            int courseId = 2;
            var enrollment = new Enrollment { EnrollmentId = 10, IsCompleted = false };
            _courseRepoMock.IsOwnerAsync(userId, courseId).Returns(false);
            _enrollmentRepoMock.GetEnrollmentWithProgressAsync(userId, courseId).Returns(enrollment);
            _courseRepoMock.GetCourseStatsAsync(courseId).Returns((CourseStats)null);
            _enrollmentRepoMock.GetCompletedMaterialCountAsync(10).Returns(1);

            var result = await _sut.GetEnrollmentStatusAsync(userId, courseId);
            result.ProgressPercentage.Should().Be(0);
            result.CanReview.Should().BeTrue();
        }

        // ----------------------------------------------------
        // SubmitReviewAsync Tests
        // ----------------------------------------------------

        [Fact]
        public async Task SubmitReviewAsync_UserIsLockedOut_ThrowsBadRequestException()
        {
            var lockout = new Lockout { LockoutEnd = DateTime.UtcNow.AddDays(1) };
            _lockoutRepoMock.GetActiveLockoutAsync(1, "review").Returns(lockout);

            var request = new ReviewRequest();
            Func<Task> act = async () => await _sut.SubmitReviewAsync(1, request, false);

            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("*Your account has been restricted from posting comments and reviews until*");
        }

        [Fact]
        public async Task SubmitReviewAsync_IsOwnerAndNoEnrollment_CreatesEnrollmentAndSubmitsCourseReview()
        {
            int userId = 1;
            int courseId = 2;
            var request = new ReviewRequest { CourseId = courseId, Rating = 5, Comment = "Great" };
            
            _lockoutRepoMock.GetActiveLockoutAsync(userId, "review").Returns((Lockout)null);
            _courseRepoMock.IsOwnerAsync(userId, courseId).Returns(true);
            _enrollmentRepoMock.GetEnrollmentWithProgressAsync(userId, courseId).Returns((Enrollment)null);
            _courseRepoMock.GetByIdAsync(courseId).Returns(new Course { Title = "Course Title", InstructorId = 99 });

            await _sut.SubmitReviewAsync(userId, request, false);

            await _enrollmentRepoMock.Received(1).AddEnrollmentAsync(Arg.Is<Enrollment>(e => e.IsCompleted == true && e.Title == "Course Title"));
            await _enrollmentRepoMock.Received(1).SaveChangesAsync();
            await _reviewRepoMock.Received(1).AddCourseReviewAsync(Arg.Is<CourseReview>(r => r.Rating == 5));
            await _reviewRepoMock.Received(1).SaveChangesAsync();
            await _notifMock.DidNotReceive().SendNotificationAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task SubmitReviewAsync_IsOwnerAndNoCourseFound_CreatesEnrollmentWithDefaultTitle()
        {
            int userId = 1;
            int courseId = 2;
            var request = new ReviewRequest { CourseId = courseId, Rating = 5, Comment = "Great" };
            
            _lockoutRepoMock.GetActiveLockoutAsync(userId, "review").Returns((Lockout)null);
            _courseRepoMock.IsOwnerAsync(userId, courseId).Returns(true);
            _enrollmentRepoMock.GetEnrollmentWithProgressAsync(userId, courseId).Returns((Enrollment)null);
            _courseRepoMock.GetByIdAsync(courseId).Returns((Course)null);

            await _sut.SubmitReviewAsync(userId, request, false);

            await _enrollmentRepoMock.Received(1).AddEnrollmentAsync(Arg.Is<Enrollment>(e => e.Title == "Owner Access"));
        }

        [Fact]
        public async Task SubmitReviewAsync_NotOwnerAndNotEnrolled_ThrowsInvalidOperationException()
        {
            _lockoutRepoMock.GetActiveLockoutAsync(1, "review").Returns((Lockout)null);
            _courseRepoMock.IsOwnerAsync(1, 2).Returns(false);
            _enrollmentRepoMock.GetEnrollmentWithProgressAsync(1, 2).Returns((Enrollment)null);

            Func<Task> act = async () => await _sut.SubmitReviewAsync(1, new ReviewRequest { CourseId = 2 }, false);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You need to enroll in the course before writing a review.");
        }

        [Fact]
        public async Task SubmitReviewAsync_NotOwnerAndRequireCompletionButNotCompleted_ThrowsInvalidOperationException()
        {
            _lockoutRepoMock.GetActiveLockoutAsync(1, "review").Returns((Lockout)null);
            _courseRepoMock.IsOwnerAsync(1, 2).Returns(false);
            _enrollmentRepoMock.GetEnrollmentWithProgressAsync(1, 2).Returns(new Enrollment { IsCompleted = false });

            Func<Task> act = async () => await _sut.SubmitReviewAsync(1, new ReviewRequest { CourseId = 2 }, true);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You need to complete the course before writing a review on the detail page.");
        }

        [Fact]
        public async Task SubmitReviewAsync_InvalidRating_ThrowsInvalidOperationException()
        {
            _lockoutRepoMock.GetActiveLockoutAsync(1, "review").Returns((Lockout)null);
            _courseRepoMock.IsOwnerAsync(1, 2).Returns(true);
            _enrollmentRepoMock.GetEnrollmentWithProgressAsync(1, 2).Returns(new Enrollment());

            var requestLow = new ReviewRequest { CourseId = 2, Rating = 0, Comment = "Great" };
            var requestHigh = new ReviewRequest { CourseId = 2, Rating = 6, Comment = "Great" };

            Func<Task> actLow = async () => await _sut.SubmitReviewAsync(1, requestLow, false);
            Func<Task> actHigh = async () => await _sut.SubmitReviewAsync(1, requestHigh, false);

            await actLow.Should().ThrowAsync<InvalidOperationException>().WithMessage("Rating must be between 1 and 5 stars.");
            await actHigh.Should().ThrowAsync<InvalidOperationException>().WithMessage("Rating must be between 1 and 5 stars.");
        }

        [Fact]
        public async Task SubmitReviewAsync_EmptyComment_ThrowsInvalidOperationException()
        {
            _lockoutRepoMock.GetActiveLockoutAsync(1, "review").Returns((Lockout)null);
            _courseRepoMock.IsOwnerAsync(1, 2).Returns(true);
            _enrollmentRepoMock.GetEnrollmentWithProgressAsync(1, 2).Returns(new Enrollment());

            Func<Task> act = async () => await _sut.SubmitReviewAsync(1, new ReviewRequest { CourseId = 2, Rating = 5, Comment = "" }, false);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Review content cannot be empty.");
        }

        [Fact]
        public async Task SubmitReviewAsync_NotOwner_SubmitsLessonReviewAndSendsNotification()
        {
            int userId = 1;
            int courseId = 2;
            int lessonId = 3;
            var request = new ReviewRequest { CourseId = courseId, LessonId = lessonId, Rating = 4, Comment = "Good lesson" };
            
            _lockoutRepoMock.GetActiveLockoutAsync(userId, "review").Returns((Lockout)null);
            _courseRepoMock.IsOwnerAsync(userId, courseId).Returns(false);
            _enrollmentRepoMock.GetEnrollmentWithProgressAsync(userId, courseId).Returns(new Enrollment { EnrollmentId = 10, IsCompleted = true });
            _courseRepoMock.GetByIdAsync(courseId).Returns(new Course { CourseId = courseId, Title = "Course", InstructorId = 99 });

            await _sut.SubmitReviewAsync(userId, request, false);

            await _enrollmentRepoMock.Received(1).GetCompletedMaterialCountAsync(10);
            await _reviewRepoMock.Received(1).AddLessonReviewAsync(Arg.Is<LessonReview>(r => r.Rating == 4 && r.LessonId == lessonId));
            await _notifMock.Received(1).SendNotificationAsync(99, "New Review", Arg.Any<string>(), $"/Course/Learn/{courseId}#review-card-0");
        }

        [Fact]
        public async Task SubmitReviewAsync_NotOwner_SubmitsCourseReviewAndSendsNotification()
        {
            int userId = 1;
            int courseId = 2;
            var request = new ReviewRequest { CourseId = courseId, Rating = 4, Comment = "Good course" };
            
            _lockoutRepoMock.GetActiveLockoutAsync(userId, "review").Returns((Lockout)null);
            _courseRepoMock.IsOwnerAsync(userId, courseId).Returns(false);
            _enrollmentRepoMock.GetEnrollmentWithProgressAsync(userId, courseId).Returns(new Enrollment { EnrollmentId = 10 });
            _courseRepoMock.GetByIdAsync(courseId).Returns(new Course { CourseId = courseId, Title = "Course", InstructorId = 99 });

            await _sut.SubmitReviewAsync(userId, request, false);

            await _reviewRepoMock.Received(1).AddCourseReviewAsync(Arg.Is<CourseReview>(r => r.Rating == 4));
            await _notifMock.Received(1).SendNotificationAsync(99, "New Review", Arg.Any<string>(), $"/Course/Details/{courseId}#review-card-0");
        }
        
        [Fact]
        public async Task SubmitReviewAsync_NotOwner_InstructorIsNull_SubmitsButNoNotification()
        {
            int userId = 1;
            int courseId = 2;
            var request = new ReviewRequest { CourseId = courseId, Rating = 4, Comment = "Good course" };
            
            _lockoutRepoMock.GetActiveLockoutAsync(userId, "review").Returns((Lockout)null);
            _courseRepoMock.IsOwnerAsync(userId, courseId).Returns(false);
            _enrollmentRepoMock.GetEnrollmentWithProgressAsync(userId, courseId).Returns(new Enrollment { EnrollmentId = 10 });
            _courseRepoMock.GetByIdAsync(courseId).Returns(new Course { CourseId = courseId, Title = "Course", InstructorId = null });

            await _sut.SubmitReviewAsync(userId, request, false);

            await _reviewRepoMock.Received(1).AddCourseReviewAsync(Arg.Is<CourseReview>(r => r.Rating == 4));
            await _notifMock.DidNotReceive().SendNotificationAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        // ----------------------------------------------------
        // GetCourseReviewsAsync & GetLessonReviewsAsync
        // ----------------------------------------------------

        [Fact]
        public async Task GetCourseReviewsAsync_WithRemovedAndNormalReviews_MapsCorrectly()
        {
            int courseId = 2;
            var course = new Course { CourseId = courseId, InstructorId = 99 };
            var reviews = new List<CourseReview>
            {
                new CourseReview { CourseReviewId = 1, Rating = 5, Comment = "Awesome", IsRemoved = false, CourseReviewStatus = "ok", Enrollment = new Enrollment { UserId = 1, User = new User { FullName = "A" } } },
                new CourseReview { CourseReviewId = 2, Rating = 4, Comment = "Bad", IsRemoved = true, CourseReviewStatus = "violating", Enrollment = new Enrollment { UserId = 2 } },
                new CourseReview { CourseReviewId = 3, Rating = 3, Comment = "Whatever", IsRemoved = true, CourseReviewStatus = "removed", Enrollment = new Enrollment { UserId = 99 } } // Instructor review
            };

            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _reviewRepoMock.GetCourseReviewsWithDetailsAsync(courseId, 1, 10, null).Returns((reviews, 3));

            var result = await _sut.GetCourseReviewsAsync(courseId, 1, 10, null);

            var items = result.Items.ToList();
            items.Should().HaveCount(3);
            
            // Normal review
            items[0].Rating.Should().Be(5);
            items[0].Comment.Should().Be("Awesome");
            items[0].IsRemoved.Should().BeFalse();
            items[0].IsInstructor.Should().BeFalse();

            // Violating review
            items[1].Rating.Should().Be(0);
            items[1].Comment.Should().Be("This review was removed by a moderator for violating community standards.");
            items[1].IsRemoved.Should().BeTrue();

            // Self-removed review by instructor
            items[2].Rating.Should().Be(0);
            items[2].Comment.Should().Be("[deleted]");
            items[2].IsRemoved.Should().BeTrue();
            items[2].IsInstructor.Should().BeTrue();
        }

        [Fact]
        public async Task GetCourseReviewsAsync_CourseIsNull_ReturnsMappedReviews()
        {
            int courseId = 2;
            var reviews = new List<CourseReview>
            {
                new CourseReview { CourseReviewId = 1, Rating = 5, Comment = "Awesome" }
            };

            _courseRepoMock.GetByIdAsync(courseId).Returns((Course)null);
            _reviewRepoMock.GetCourseReviewsWithDetailsAsync(courseId, 1, 10, null).Returns((reviews, 1));

            var result = await _sut.GetCourseReviewsAsync(courseId, 1, 10, null);
            var items = result.Items.ToList();
            items.First().IsInstructor.Should().BeFalse();
        }

        [Fact]
        public async Task GetLessonReviewsAsync_WithRemovedAndNormalReviews_MapsCorrectly()
        {
            int lessonId = 5;
            var lesson = new Lesson { LessonId = lessonId, Course = new Course { InstructorId = 99 }, Title = "Lesson Title" };
            var reviews = new List<LessonReview>
            {
                new LessonReview { LessonReviewId = 1, LessonId = lessonId, Lesson = lesson, Rating = 5, Comment = "Awesome", IsRemoved = false, LessonReviewStatus = "ok", Enrollment = new Enrollment { UserId = 1, User = new User { FullName = "A", UserNavigation = new Account { AvatarUrl = "url" } } } },
                new LessonReview { LessonReviewId = 2, LessonId = lessonId, Lesson = lesson, Rating = 4, Comment = "Bad", IsRemoved = true, LessonReviewStatus = "violating", Enrollment = new Enrollment { UserId = 2 } },
                new LessonReview { LessonReviewId = 3, LessonId = lessonId, Lesson = lesson, Rating = 3, Comment = "Whatever", IsRemoved = true, LessonReviewStatus = "removed", Enrollment = new Enrollment { UserId = 99 } }
            };

            _reviewRepoMock.GetLessonReviewsWithDetailsAsync(lessonId, 1, 10).Returns((reviews, 3));

            var result = await _sut.GetLessonReviewsAsync(lessonId, 1, 10);

            var items = result.Items.ToList();
            items.Should().HaveCount(3);
            
            // Normal
            items[0].Rating.Should().Be(5);
            items[0].Comment.Should().Be("Awesome");
            items[0].UserAvatarUrl.Should().Be("url");
            items[0].LessonTitle.Should().Be("Lesson Title");

            // Violating
            items[1].Rating.Should().Be(0);
            items[1].Comment.Should().Be("This review was removed by a moderator for violating community standards.");

            // Removed
            items[2].Rating.Should().Be(0);
            items[2].Comment.Should().Be("[deleted]");
            items[2].IsInstructor.Should().BeTrue();
        }

        [Fact]
        public async Task GetLessonReviewsAsync_LessonIsNull_ReturnsMappedReviews()
        {
            int lessonId = 5;
            var reviews = new List<LessonReview>
            {
                new LessonReview { LessonReviewId = 1, Lesson = null }
            };

            _reviewRepoMock.GetLessonReviewsWithDetailsAsync(lessonId, 1, 10).Returns((reviews, 1));

            var result = await _sut.GetLessonReviewsAsync(lessonId, 1, 10);
            var items = result.Items.ToList();
            items.First().LessonTitle.Should().BeNull();
        }

        // ----------------------------------------------------
        // Stats Calculation Tests
        // ----------------------------------------------------

        [Fact]
        public async Task GetReviewStatsAsync_CalculatesCorrectly()
        {
            var ratings = new List<float> { 5f, 4.5f, 4f, 3.5f, 3f, 2.5f, 2f, 1.5f, 1f };
            _reviewRepoMock.GetCourseReviewRatingsAsync(1).Returns(ratings);

            var result = await _sut.GetReviewStatsAsync(1);

            result.TotalReviews.Should().Be(9);
            result.AverageRating.Should().Be(3.0); // (27) / 9 = 3.0
            result.Star5Count.Should().Be(2); // 5, 4.5
            result.Star4Count.Should().Be(2); // 4, 3.5
            result.Star3Count.Should().Be(2); // 3, 2.5
            result.Star2Count.Should().Be(2); // 2, 1.5
            result.Star1Count.Should().Be(1); // 1
        }

        [Fact]
        public async Task GetReviewStatsAsync_EmptyRatings_ReturnsZeroes()
        {
            _reviewRepoMock.GetCourseReviewRatingsAsync(1).Returns(new List<float>());
            var result = await _sut.GetReviewStatsAsync(1);
            result.TotalReviews.Should().Be(0);
            result.AverageRating.Should().Be(0);
        }

        [Fact]
        public async Task GetLessonReviewStatsAsync_CalculatesCorrectly()
        {
            var ratings = new List<float> { 5f, 1f };
            _reviewRepoMock.GetLessonReviewRatingsAsync(1).Returns(ratings);
            var result = await _sut.GetLessonReviewStatsAsync(1);
            result.TotalReviews.Should().Be(2);
            result.AverageRating.Should().Be(3.0);
        }

        [Fact]
        public async Task GetLessonRatingsForCourseAsync_MapsCorrectly()
        {
            var data = new List<(int LessonId, double AvgRating, int Count)>
            {
                (1, 4.56, 10),
                (2, 3.12, 5)
            };
            _reviewRepoMock.GetLessonRatingsForCourseAsync(1).Returns(data);

            var result = await _sut.GetLessonRatingsForCourseAsync(1);

            result.Should().HaveCount(2);
            result[0].LessonId.Should().Be(1);
            result[0].AverageRating.Should().Be(4.6);
            result[0].TotalReviews.Should().Be(10);
            result[1].LessonId.Should().Be(2);
            result[1].AverageRating.Should().Be(3.1);
        }

        // ----------------------------------------------------
        // UpdateReviewAsync Tests
        // ----------------------------------------------------

        [Fact]
        public async Task UpdateReviewAsync_InvalidRating_ThrowsInvalidOperationException()
        {
            var request = new UpdateReviewRequest { Rating = 6, Comment = "Good" };
            Func<Task> act = async () => await _sut.UpdateReviewAsync(1, request);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Rating must be between 1 and 5 stars.");
        }

        [Fact]
        public async Task UpdateReviewAsync_EmptyComment_ThrowsInvalidOperationException()
        {
            var request = new UpdateReviewRequest { Rating = 5, Comment = "" };
            Func<Task> act = async () => await _sut.UpdateReviewAsync(1, request);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Review content cannot be empty.");
        }

        [Fact]
        public async Task UpdateReviewAsync_LessonReviewNotFound_ThrowsInvalidOperationException()
        {
            var request = new UpdateReviewRequest { ReviewId = 1, Rating = 5, Comment = "Good", Type = "lesson" };
            _reviewRepoMock.GetLessonReviewByIdAsync(1).Returns((LessonReview)null);
            Func<Task> act = async () => await _sut.UpdateReviewAsync(1, request);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Review not found.");
        }

        [Fact]
        public async Task UpdateReviewAsync_LessonReviewUnauthorized_ThrowsUnauthorizedAccessException()
        {
            var request = new UpdateReviewRequest { ReviewId = 1, Rating = 5, Comment = "Good", Type = "lesson" };
            _reviewRepoMock.GetLessonReviewByIdAsync(1).Returns(new LessonReview { Enrollment = new Enrollment { UserId = 2 } });
            Func<Task> act = async () => await _sut.UpdateReviewAsync(1, request);
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You can only edit your own reviews.");
        }

        [Fact]
        public async Task UpdateReviewAsync_LessonReviewRemoved_ThrowsInvalidOperationException()
        {
            var request = new UpdateReviewRequest { ReviewId = 1, Rating = 5, Comment = "Good", Type = "lesson" };
            _reviewRepoMock.GetLessonReviewByIdAsync(1).Returns(new LessonReview { Enrollment = new Enrollment { UserId = 1 }, IsRemoved = true });
            Func<Task> act = async () => await _sut.UpdateReviewAsync(1, request);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This review has been removed and cannot be edited.");
        }

        [Fact]
        public async Task UpdateReviewAsync_LessonReviewValid_UpdatesReview()
        {
            var request = new UpdateReviewRequest { ReviewId = 1, Rating = 5, Comment = "Updated", Type = "lesson" };
            var review = new LessonReview { Enrollment = new Enrollment { UserId = 1 }, IsRemoved = false };
            _reviewRepoMock.GetLessonReviewByIdAsync(1).Returns(review);

            await _sut.UpdateReviewAsync(1, request);

            review.Rating.Should().Be(5);
            review.Comment.Should().Be("Updated");
            _reviewRepoMock.Received(1).UpdateLessonReview(review);
            await _reviewRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateReviewAsync_CourseReviewNotFound_ThrowsInvalidOperationException()
        {
            var request = new UpdateReviewRequest { ReviewId = 1, Rating = 5, Comment = "Good", Type = "course" };
            _reviewRepoMock.GetCourseReviewByIdAsync(1).Returns((CourseReview)null);
            Func<Task> act = async () => await _sut.UpdateReviewAsync(1, request);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Review not found.");
        }
        
        [Fact]
        public async Task UpdateReviewAsync_CourseReviewNullType_DefaultsToCourse_UpdatesReview()
        {
            var request = new UpdateReviewRequest { ReviewId = 1, Rating = 5, Comment = "Good", Type = null };
            var review = new CourseReview { Enrollment = new Enrollment { UserId = 1 }, IsRemoved = false };
            _reviewRepoMock.GetCourseReviewByIdAsync(1).Returns(review);

            await _sut.UpdateReviewAsync(1, request);

            review.Rating.Should().Be(5);
            review.Comment.Should().Be("Good");
            _reviewRepoMock.Received(1).UpdateCourseReview(review);
            await _reviewRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateReviewAsync_CourseReviewUnauthorized_ThrowsUnauthorizedAccessException()
        {
            var request = new UpdateReviewRequest { ReviewId = 1, Rating = 5, Comment = "Good", Type = "course" };
            _reviewRepoMock.GetCourseReviewByIdAsync(1).Returns(new CourseReview { Enrollment = new Enrollment { UserId = 2 } });
            Func<Task> act = async () => await _sut.UpdateReviewAsync(1, request);
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You can only edit your own reviews.");
        }

        [Fact]
        public async Task UpdateReviewAsync_CourseReviewRemoved_ThrowsInvalidOperationException()
        {
            var request = new UpdateReviewRequest { ReviewId = 1, Rating = 5, Comment = "Good", Type = "course" };
            _reviewRepoMock.GetCourseReviewByIdAsync(1).Returns(new CourseReview { Enrollment = new Enrollment { UserId = 1 }, IsRemoved = true });
            Func<Task> act = async () => await _sut.UpdateReviewAsync(1, request);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This review has been removed and cannot be edited.");
        }

        // ----------------------------------------------------
        // DeleteReviewAsync Tests
        // ----------------------------------------------------

        [Fact]
        public async Task DeleteReviewAsync_LessonReviewNotFound_ThrowsInvalidOperationException()
        {
            var request = new DeleteReviewRequest { ReviewId = 1, Type = "lesson" };
            _reviewRepoMock.GetLessonReviewByIdAsync(1).Returns((LessonReview)null);
            Func<Task> act = async () => await _sut.DeleteReviewAsync(1, request);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Review not found.");
        }

        [Fact]
        public async Task DeleteReviewAsync_LessonReviewUnauthorized_ThrowsUnauthorizedAccessException()
        {
            var request = new DeleteReviewRequest { ReviewId = 1, Type = "lesson" };
            _reviewRepoMock.GetLessonReviewByIdAsync(1).Returns(new LessonReview { Enrollment = new Enrollment { UserId = 2 } });
            Func<Task> act = async () => await _sut.DeleteReviewAsync(1, request);
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You can only delete your own reviews.");
        }

        [Fact]
        public async Task DeleteReviewAsync_LessonReviewHasPendingReports_ThrowsInvalidOperationException()
        {
            var request = new DeleteReviewRequest { ReviewId = 1, Type = "lesson" };
            _reviewRepoMock.GetLessonReviewByIdAsync(1).Returns(new LessonReview { Enrollment = new Enrollment { UserId = 1 } });
            _reviewRepoMock.HasPendingLessonReviewReportsAsync(1).Returns(true);
            
            Func<Task> act = async () => await _sut.DeleteReviewAsync(1, request);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This review is currently under moderation review and cannot be deleted.");
        }

        [Fact]
        public async Task DeleteReviewAsync_LessonReviewValid_SoftDeletesReview()
        {
            var request = new DeleteReviewRequest { ReviewId = 1, Type = "lesson" };
            var review = new LessonReview { Enrollment = new Enrollment { UserId = 1 } };
            _reviewRepoMock.GetLessonReviewByIdAsync(1).Returns(review);
            _reviewRepoMock.HasPendingLessonReviewReportsAsync(1).Returns(false);
            
            await _sut.DeleteReviewAsync(1, request);

            review.IsRemoved.Should().BeTrue();
            review.LessonReviewStatus.Should().Be("removed");
            _reviewRepoMock.Received(1).UpdateLessonReview(review);
            await _reviewRepoMock.Received(1).SaveChangesAsync();
        }
        
        [Fact]
        public async Task DeleteReviewAsync_CourseReviewNotFound_ThrowsInvalidOperationException()
        {
            var request = new DeleteReviewRequest { ReviewId = 1, Type = "course" };
            _reviewRepoMock.GetCourseReviewByIdAsync(1).Returns((CourseReview)null);
            Func<Task> act = async () => await _sut.DeleteReviewAsync(1, request);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Review not found.");
        }

        [Fact]
        public async Task DeleteReviewAsync_CourseReviewNullType_DefaultsToCourse_ThrowsUnauthorizedAccessException()
        {
            var request = new DeleteReviewRequest { ReviewId = 1, Type = null };
            _reviewRepoMock.GetCourseReviewByIdAsync(1).Returns(new CourseReview { Enrollment = new Enrollment { UserId = 2 } });
            Func<Task> act = async () => await _sut.DeleteReviewAsync(1, request);
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You can only delete your own reviews.");
        }

        [Fact]
        public async Task DeleteReviewAsync_CourseReviewHasPendingReports_ThrowsInvalidOperationException()
        {
            var request = new DeleteReviewRequest { ReviewId = 1, Type = "course" };
            _reviewRepoMock.GetCourseReviewByIdAsync(1).Returns(new CourseReview { Enrollment = new Enrollment { UserId = 1 } });
            _reviewRepoMock.HasPendingCourseReviewReportsAsync(1).Returns(true);
            
            Func<Task> act = async () => await _sut.DeleteReviewAsync(1, request);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This review is currently under moderation review and cannot be deleted.");
        }
        
        [Fact]
        public async Task DeleteReviewAsync_CourseReviewValid_SoftDeletesReview()
        {
            var request = new DeleteReviewRequest { ReviewId = 1, Type = "course" };
            var review = new CourseReview { Enrollment = new Enrollment { UserId = 1 } };
            _reviewRepoMock.GetCourseReviewByIdAsync(1).Returns(review);
            _reviewRepoMock.HasPendingCourseReviewReportsAsync(1).Returns(false);
            
            await _sut.DeleteReviewAsync(1, request);

            review.IsRemoved.Should().BeTrue();
            review.CourseReviewStatus.Should().Be("removed");
            _reviewRepoMock.Received(1).UpdateCourseReview(review);
            await _reviewRepoMock.Received(1).SaveChangesAsync();
        }

        // ----------------------------------------------------
        // ReportReviewAsync Tests
        // ----------------------------------------------------

        [Fact]
        public async Task ReportReviewAsync_CourseReview_WithReason_CallsReportService()
        {
            await _sut.ReportReviewAsync(1, 10, "course", "Spam");
            await _reportServiceMock.Received(1).CreateCourseReviewReportAsync(1, Arg.Is<CreateCourseReviewReportRequest>(r => r.CourseReviewId == 10 && r.Reason == "Spam"));
        }

        [Fact]
        public async Task ReportReviewAsync_CourseReview_NoReason_UsesDefaultReason()
        {
            await _sut.ReportReviewAsync(1, 10, "course", "");
            await _reportServiceMock.Received(1).CreateCourseReviewReportAsync(1, Arg.Is<CreateCourseReviewReportRequest>(r => r.CourseReviewId == 10 && r.Reason == "Violates community standards"));
        }

        [Fact]
        public async Task ReportReviewAsync_LessonReview_WithReason_CallsReportService()
        {
            await _sut.ReportReviewAsync(1, 10, "lesson", "Inappropriate");
            await _reportServiceMock.Received(1).CreateLessonReviewReportAsync(1, Arg.Is<CreateLessonReviewReportRequest>(r => r.LessonReviewId == 10 && r.Reason == "Inappropriate"));
        }

        [Fact]
        public async Task ReportReviewAsync_LessonReview_NoReason_UsesDefaultReason()
        {
            await _sut.ReportReviewAsync(1, 10, "lesson", null);
            await _reportServiceMock.Received(1).CreateLessonReviewReportAsync(1, Arg.Is<CreateLessonReviewReportRequest>(r => r.LessonReviewId == 10 && r.Reason == "Violates community standards"));
        }
    }
}
