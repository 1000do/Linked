using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
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

        [Fact]
        public async Task SubmitReviewAsync_SavesReview_WhenUserCompletedCourseOrNoRestriction()
        {
            //Arrange 1
            int userId = 1;
            int courseId = 2;
            var request = new ReviewRequest { CourseId = courseId, Rating = 5, Comment = "Great" };
            var enrollment = new Enrollment { EnrollmentId = 10, UserId = userId, CourseId = courseId, IsCompleted = true };
            var course = new Course { CourseId = courseId, InstructorId = 99 };

            //Arrange 2
            _lockoutRepoMock.GetActiveLockoutAsync(userId, "review").Returns((Lockout)null);
            _courseRepoMock.IsOwnerAsync(userId, courseId).Returns(false);
            _enrollmentRepoMock.GetEnrollmentWithProgressAsync(userId, courseId).Returns(enrollment);
            _reviewRepoMock.AddCourseReviewAsync(Arg.Any<CourseReview>()).Returns(Task.CompletedTask);
            _reviewRepoMock.SaveChangesAsync().Returns(1);
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);

            //Act
            await _sut.SubmitReviewAsync(userId, request, requireCompletion: true);

            //Assert
            await _reviewRepoMock.Received(1).AddCourseReviewAsync(Arg.Is<CourseReview>(r => r.Rating == 5 && r.Comment == "Great"));
            await _reviewRepoMock.Received(1).SaveChangesAsync();
            await _notifMock.Received(1).SendNotificationAsync(99, "New Review", Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetCourseReviewsAsync_ReturnsPagedReviews_WhenReviewsExist()
        {
            //Arrange 1
            int courseId = 2;
            int page = 1;
            int pageSize = 10;
            var course = new Course { CourseId = courseId, InstructorId = 99 };
            var courseReviews = new List<CourseReview>
            {
                new CourseReview { CourseReviewId = 1, Rating = 5, Comment = "Good", Enrollment = new Enrollment { UserId = 1, User = new User { FullName = "User 1" } } }
            };
            int totalCount = 1;

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _reviewRepoMock.GetCourseReviewsWithDetailsAsync(courseId, page, pageSize, null).Returns((courseReviews, totalCount));

            //Act
            var result = await _sut.GetCourseReviewsAsync(courseId, page, pageSize, null);

            //Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Items.First().Rating.Should().Be(5);
            result.Items.First().Comment.Should().Be("Good");

            await _reviewRepoMock.Received(1).GetCourseReviewsWithDetailsAsync(courseId, page, pageSize, null);
        }

        [Fact]
        public async Task DeleteReviewAsync_DeletesReview_WhenReviewExistsAndBelongsToUser()
        {
            //Arrange 1
            int userId = 1;
            int reviewId = 100;
            var request = new DeleteReviewRequest { ReviewId = reviewId, Type = "course" };
            var review = new CourseReview { CourseReviewId = reviewId, Enrollment = new Enrollment { UserId = userId } };

            //Arrange 2
            _reviewRepoMock.GetCourseReviewByIdAsync(reviewId).Returns(review);
            _reviewRepoMock.HasPendingCourseReviewReportsAsync(reviewId).Returns(false);
            _reviewRepoMock.SaveChangesAsync().Returns(1);

            //Act
            await _sut.DeleteReviewAsync(userId, request);

            //Assert
            review.IsRemoved.Should().BeTrue();
            review.CourseReviewStatus.Should().Be("removed");
            
            _reviewRepoMock.Received(1).UpdateCourseReview(review);
            await _reviewRepoMock.Received(1).SaveChangesAsync();
        }
    }
}
