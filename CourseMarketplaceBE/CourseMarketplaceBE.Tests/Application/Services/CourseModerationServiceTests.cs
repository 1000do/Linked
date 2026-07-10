using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class CourseModerationServiceTests
    {
        private readonly ICourseRepository _courseRepoMock;
        private readonly IMaterialRepository _materialRepoMock;
        private readonly ILessonRepository _lessonRepoMock;
        private readonly IEnrollmentRepository _enrollmentRepoMock;
        private readonly IRedisService _redisServiceMock;
        private readonly INotificationService _notifMock;
        private readonly IUserRepository _userRepoMock;
        private readonly ILessonService _lessonServiceMock;
        private readonly IEmbeddingService _embeddingServiceMock;
        private readonly CourseModerationService _sut;

        public CourseModerationServiceTests()
        {
            _courseRepoMock = Substitute.For<ICourseRepository>();
            _materialRepoMock = Substitute.For<IMaterialRepository>();
            _lessonRepoMock = Substitute.For<ILessonRepository>();
            _enrollmentRepoMock = Substitute.For<IEnrollmentRepository>();
            _redisServiceMock = Substitute.For<IRedisService>();
            _notifMock = Substitute.For<INotificationService>();
            _userRepoMock = Substitute.For<IUserRepository>();
            _lessonServiceMock = Substitute.For<ILessonService>();
            _embeddingServiceMock = Substitute.For<IEmbeddingService>();

            _sut = new CourseModerationService(
                _courseRepoMock,
                _materialRepoMock,
                _lessonRepoMock,
                _enrollmentRepoMock,
                _redisServiceMock,
                _notifMock,
                _userRepoMock,
                _lessonServiceMock,
                _embeddingServiceMock
            );
        }

        [Fact]
        public async Task GetPendingCoursesAsync_ReturnsPagedPendingCourses_WhenFiltersProvided()
        {
            //Arrange 1
            var filter = new ModerationFilterDto { Page = 1, PageSize = 10 };
            var list = new List<CourseModerationDto> { new CourseModerationDto { CourseId = 1, Title = "Pending" } };
            var pagedResult = new PagedResult<CourseModerationDto> { Items = list, TotalCount = 1 };

            //Arrange 2
            _courseRepoMock.GetPendingCoursesModerationAsync(filter).Returns(pagedResult);

            //Act
            var result = await _sut.GetPendingCoursesAsync(filter);

            //Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Items.First().Title.Should().Be("Pending");

            await _courseRepoMock.Received(1).GetPendingCoursesModerationAsync(filter);
        }

        [Fact]
        public async Task ApproveCourseAsync_ApprovesCourse_WhenCourseIsPending()
        {
            //Arrange 1
            int courseId = 1;
            string feedback = "Looks good";
            var course = new Course { CourseId = courseId, CourseStatus = CourseStatus.Pending.ToValue(), InstructorId = 2 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _materialRepoMock.GetByCourseIdAsync(courseId).Returns(new List<LearningMaterial>());
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(new List<Lesson>());
            _courseRepoMock.SaveChangesAsync().Returns(1);
            _enrollmentRepoMock.GetEnrolledUserIdsAsync(courseId).Returns(new List<int> { 3 });

            //Act
            var result = await _sut.ApproveCourseAsync(courseId, feedback);

            //Assert
            result.Should().BeTrue();
            course.CourseStatus.Should().Be(CourseStatus.Published.ToValue());

            _courseRepoMock.Received(1).Update(course);
            await _courseRepoMock.Received(1).SaveChangesAsync();
            await _notifMock.Received(1).SendNotificationAsync(2, "Course Approved", Arg.Any<string>(), Arg.Any<string>());
            await _notifMock.Received(1).SendBulkNotificationsAsync(Arg.Is<List<NotificationBulkDto>>(l => l.Count == 1));
        }

        [Fact]
        public async Task RejectCourseDetailedAsync_RejectsCourse_WhenCourseIsPending()
        {
            //Arrange 1
            int courseId = 1;
            var request = new RejectCourseDetailedRequest 
            { 
                CourseId = courseId, 
                Items = new List<RejectCourseItemDto> 
                { 
                    new RejectCourseItemDto { Target = "course.title", Reason = "Bad title" } 
                } 
            };
            var course = new Course { CourseId = courseId, CourseStatus = CourseStatus.Pending.ToValue(), InstructorId = 2 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _materialRepoMock.GetByCourseIdAsync(courseId).Returns(new List<LearningMaterial>());
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(new List<Lesson>());
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.RejectCourseDetailedAsync(request);

            //Assert
            result.Should().BeTrue();
            course.CourseStatus.Should().Be(CourseStatus.Rejected.ToValue());
            course.ModerationFeedback.Should().Contain("Bad title");

            _courseRepoMock.Received(1).Update(course);
            await _courseRepoMock.Received(1).SaveChangesAsync();
            await _notifMock.Received(1).SendNotificationAsync(2, "Course Rejected", Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task FlagCourseDetailedAsync_FlagsCourse_WhenCourseIsPublished()
        {
            //Arrange 1
            int courseId = 1;
            var request = new RejectCourseDetailedRequest 
            { 
                CourseId = courseId, 
                Items = new List<RejectCourseItemDto> 
                { 
                    new RejectCourseItemDto { Target = "course.title", Reason = "Inappropriate title" } 
                } 
            };
            var course = new Course { CourseId = courseId, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2, CourseFlagCount = 0 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _materialRepoMock.GetByCourseIdAsync(courseId).Returns(new List<LearningMaterial>());
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(new List<Lesson>());
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.FlagCourseDetailedAsync(request);

            //Assert
            result.Should().BeTrue();
            course.CourseFlagCount.Should().Be(1);
            course.ModerationFeedback.Should().Contain("Inappropriate title");
            course.ModerationFeedback.Should().Contain("[VIOLATION FLAG #1]");

            _courseRepoMock.Received(1).Update(course);
            await _courseRepoMock.Received(1).SaveChangesAsync();
            await _notifMock.Received(1).SendNotificationAsync(2, "Course Violation Reminder (1st Time)", Arg.Any<string>(), Arg.Any<string>());
        }
    }
}
