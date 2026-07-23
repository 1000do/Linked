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
using CourseMarketplaceBE.Application.Exceptions;
using NSubstitute.ExceptionExtensions;

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

        #region GetCourseModerationStatsAsync
        [Fact]
        public async Task GetCourseModerationStatsAsync_ValidCall_ReturnsStats()
        {
            //Arrange 1
            var expected = new CourseModerationStatsDto();
            
            //Arrange 2
            _courseRepoMock.GetCourseModerationStatsAsync().Returns(expected);

            //Act
            var result = await _sut.GetCourseModerationStatsAsync();

            //Assert
            result.Should().Be(expected);
            await _courseRepoMock.Received(1).GetCourseModerationStatsAsync();
        }
        #endregion

        #region ApproveCourseAsync (additional)
        [Fact]
        public async Task ApproveCourseAsync_CourseNull_ReturnsFalse()
        {
            //Arrange 1
            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(Task.FromResult<Course?>(null));

            //Act
            var result = await _sut.ApproveCourseAsync(1, "feedback");

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ApproveCourseAsync_CourseFound_NoMaterials_NoLessons_InstructorNull_UpdatesCourseAndReturnsTrue()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = null };
            
            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _materialRepoMock.GetByCourseIdAsync(courseId).Returns(new List<LearningMaterial>());
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(new List<Lesson>());
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ApproveCourseAsync(courseId, "feedback");

            //Assert
            result.Should().BeTrue();
            course.CourseStatus.Should().Be(CourseStatus.Published.ToValue());
            course.ModerationFeedback.Should().BeNull();
            await _notifMock.DidNotReceiveWithAnyArgs().SendNotificationAsync(default, default!, default!, default);
        }

        [Fact]
        public async Task ApproveCourseAsync_CourseFound_WithMaterialsNotRemoved_UpdatesLearningStatusToActive()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = null };
            var materials = new List<LearningMaterial>
            {
                new LearningMaterial { LearningStatus = LearningStatus.Pending.ToValue() },
                new LearningMaterial { LearningStatus = LearningStatus.Removed.ToValue() }
            };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _materialRepoMock.GetByCourseIdAsync(courseId).Returns(materials);
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(new List<Lesson>());
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ApproveCourseAsync(courseId, "feedback");

            //Assert
            result.Should().BeTrue();
            materials[0].LearningStatus.Should().Be(LearningStatus.Active.ToValue());
            materials[1].LearningStatus.Should().Be(LearningStatus.Removed.ToValue());
            _materialRepoMock.Received(1).Update(materials[0]);
            _materialRepoMock.DidNotReceive().Update(materials[1]);
            await _embeddingServiceMock.Received(1).PersistPendingMaterialEmbeddingsAsync(courseId, Arg.Is<HashSet<int>>(x => x.Count == 0));
        }

        [Fact]
        public async Task ApproveCourseAsync_CourseFound_WithMaterialsAlreadyActive_DoesNotUpdateLearningStatus()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = null };
            var materials = new List<LearningMaterial>
            {
                new LearningMaterial { LearningStatus = LearningStatus.Active.ToValue() }
            };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _materialRepoMock.GetByCourseIdAsync(courseId).Returns(materials);
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(new List<Lesson>());
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ApproveCourseAsync(courseId, "feedback");

            //Assert
            result.Should().BeTrue();
            materials[0].LearningStatus.Should().Be(LearningStatus.Active.ToValue());
            _materialRepoMock.Received(1).Update(materials[0]);
        }

        [Fact]
        public async Task ApproveCourseAsync_CourseFound_WithLessonsNotActive_UpdatesLessonStatusToActive()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = null };
            var lessons = new List<Lesson>
            {
                new Lesson { LessonStatus = LessonStatus.Pending.ToValue() }
            };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _materialRepoMock.GetByCourseIdAsync(courseId).Returns(new List<LearningMaterial>());
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(lessons);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ApproveCourseAsync(courseId, "feedback");

            //Assert
            result.Should().BeTrue();
            lessons[0].LessonStatus.Should().Be(LessonStatus.Active.ToValue());
            _lessonRepoMock.Received(1).Update(lessons[0]);
        }

        [Fact]
        public async Task ApproveCourseAsync_CourseFound_WithLessonsAlreadyActive_DoesNotUpdateLessonStatus()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = null };
            var lessons = new List<Lesson>
            {
                new Lesson { LessonStatus = LessonStatus.Active.ToValue() }
            };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _materialRepoMock.GetByCourseIdAsync(courseId).Returns(new List<LearningMaterial>());
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(lessons);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ApproveCourseAsync(courseId, "feedback");

            //Assert
            result.Should().BeTrue();
            lessons[0].LessonStatus.Should().Be(LessonStatus.Active.ToValue());
            _lessonRepoMock.DidNotReceive().Update(lessons[0]);
        }

        [Fact]
        public async Task ApproveCourseAsync_CourseFound_WithInstructor_NoEnrolledUsers_SendsInstructorNotification()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = 2 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _materialRepoMock.GetByCourseIdAsync(courseId).Returns(new List<LearningMaterial>());
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(new List<Lesson>());
            _courseRepoMock.SaveChangesAsync().Returns(1);
            _enrollmentRepoMock.GetEnrolledUserIdsAsync(courseId).Returns(new List<int>());

            //Act
            var result = await _sut.ApproveCourseAsync(courseId, "feedback");

            //Assert
            result.Should().BeTrue();
            await _notifMock.Received(1).SendNotificationAsync(2, "Course Approved", Arg.Any<string>(), Arg.Any<string>());
            await _notifMock.DidNotReceive().SendBulkNotificationsAsync(Arg.Any<List<NotificationBulkDto>>());
        }

        [Fact]
        public async Task ApproveCourseAsync_CourseFound_WithInstructor_WithEnrolledUsers_SendsInstructorAndBulkNotifications()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = 2 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _materialRepoMock.GetByCourseIdAsync(courseId).Returns(new List<LearningMaterial>());
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(new List<Lesson>());
            _courseRepoMock.SaveChangesAsync().Returns(1);
            _enrollmentRepoMock.GetEnrolledUserIdsAsync(courseId).Returns(new List<int> { 3, 4 });

            //Act
            var result = await _sut.ApproveCourseAsync(courseId, "feedback");

            //Assert
            result.Should().BeTrue();
            await _notifMock.Received(1).SendNotificationAsync(2, "Course Approved", Arg.Any<string>(), Arg.Any<string>());
            await _notifMock.Received(1).SendBulkNotificationsAsync(Arg.Is<List<NotificationBulkDto>>(l => l.Count == 2));
            await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));
        }
        #endregion

        #region RejectCourseAsync
        [Fact]
        public async Task RejectCourseAsync_CourseNull_ReturnsFalse()
        {
            //Arrange 1
            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(Task.FromResult<Course?>(null));

            //Act
            var result = await _sut.RejectCourseAsync(1, "reason");

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RejectCourseAsync_CourseFound_NoInstructor_UpdatesCourseAndReturnsTrue()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = null };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.RejectCourseAsync(courseId, "reason");

            //Assert
            result.Should().BeTrue();
            course.CourseStatus.Should().Be(CourseStatus.Rejected.ToValue());
            course.ModerationFeedback.Should().Be("reason");
            _courseRepoMock.Received(1).Update(course);
            await _courseRepoMock.Received(1).SaveChangesAsync();
            await _notifMock.DidNotReceiveWithAnyArgs().SendNotificationAsync(default, default!, default!, default);
            await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));
        }

        [Fact]
        public async Task RejectCourseAsync_CourseFound_WithInstructor_SendsNotification()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = 2 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.RejectCourseAsync(courseId, "reason");

            //Assert
            result.Should().BeTrue();
            await _notifMock.Received(1).SendNotificationAsync(2, "Course Rejected", Arg.Any<string>(), Arg.Any<string>());
        }
        #endregion

        #region FlagCourseAsync
        [Fact]
        public async Task FlagCourseAsync_CourseNull_ReturnsFalse()
        {
            //Arrange 1
            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(Task.FromResult<Course?>(null));

            //Act
            var result = await _sut.FlagCourseAsync(1, "reason");

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task FlagCourseAsync_FlagCountIsThree_ReturnsFalse()
        {
            //Arrange 1
            var course = new Course { CourseFlagCount = 3 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);

            //Act
            var result = await _sut.FlagCourseAsync(1, "reason");

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task FlagCourseAsync_FirstFlag_UpdatesCourseAndSendsFirstWarning()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = 2, CourseFlagCount = 0 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.FlagCourseAsync(courseId, "reason");

            //Assert
            result.Should().BeTrue();
            course.CourseFlagCount.Should().Be(1);
            await _notifMock.Received(1).SendNotificationAsync(2, "Course Violation Reminder (1st Time)", Arg.Any<string>(), Arg.Any<string>());
            await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));
        }

        [Fact]
        public async Task FlagCourseAsync_SecondFlag_UpdatesCourseAndSendsSecondWarning()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = 2, CourseFlagCount = 1 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.FlagCourseAsync(courseId, "reason");

            //Assert
            result.Should().BeTrue();
            course.CourseFlagCount.Should().Be(2);
            await _notifMock.Received(1).SendNotificationAsync(2, "Severe Violation Warning (2nd Time)", Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task FlagCourseAsync_ThirdFlag_UpdatesCourseToArchivedAndSendsPermanentNotice()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = 2, CourseFlagCount = 2 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.FlagCourseAsync(courseId, "reason");

            //Assert
            result.Should().BeTrue();
            course.CourseFlagCount.Should().Be(3);
            course.CourseStatus.Should().Be(CourseStatus.Archived.ToValue());
            _courseRepoMock.Received(2).Update(course); // Once for flag, once for archive
            await _courseRepoMock.Received(2).SaveChangesAsync(); // Once for flag, once for archive
            await _notifMock.Received(1).SendNotificationAsync(2, "Permanent Course Discontinuation Notice (3rd Time)", Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task FlagCourseAsync_InstructorNull_UpdatesCourseAndReturnsTrue()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = null, CourseFlagCount = 0 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.FlagCourseAsync(courseId, "reason");

            //Assert
            result.Should().BeTrue();
            course.CourseFlagCount.Should().Be(1);
            await _notifMock.DidNotReceiveWithAnyArgs().SendNotificationAsync(default, default!, default!, default);
        }
        #endregion

        #region UnflagCourseAsync
        [Fact]
        public async Task UnflagCourseAsync_CourseNull_ReturnsFalse()
        {
            //Arrange 1
            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(Task.FromResult<Course?>(null));

            //Act
            var result = await _sut.UnflagCourseAsync(1);

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UnflagCourseAsync_FlagCountIsZero_ReturnsFalse()
        {
            //Arrange 1
            var course = new Course { CourseFlagCount = 0 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);

            //Act
            var result = await _sut.UnflagCourseAsync(1);

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UnflagCourseAsync_FlagCountGreaterThanZero_NotThreeToTwoArchived_UpdatesCourse()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId, CourseFlagCount = 1, InstructorId = null };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.UnflagCourseAsync(courseId);

            //Assert
            result.Should().BeTrue();
            course.CourseFlagCount.Should().Be(0);
            _courseRepoMock.Received(1).Update(course);
            await _courseRepoMock.Received(1).SaveChangesAsync();
            await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));
        }

        [Fact]
        public async Task UnflagCourseAsync_FlagCountThree_StatusArchived_UpdatesStatusToRejected()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId, CourseFlagCount = 3, CourseStatus = CourseStatus.Archived.ToValue(), InstructorId = null };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.UnflagCourseAsync(courseId);

            //Assert
            result.Should().BeTrue();
            course.CourseFlagCount.Should().Be(2);
            course.CourseStatus.Should().Be(CourseStatus.Rejected.ToValue());
        }

        [Fact]
        public async Task UnflagCourseAsync_WithInstructor_SendsNotification()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId, CourseFlagCount = 1, InstructorId = 2 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.UnflagCourseAsync(courseId);

            //Assert
            result.Should().BeTrue();
            await _notifMock.Received(1).SendNotificationAsync(2, "Course Unflagged", Arg.Any<string>(), Arg.Any<string>());
        }
        #endregion

        #region RejectCourseDetailedAsync (additional)
        [Fact]
        public async Task RejectCourseDetailedAsync_CourseNull_ReturnsFalse()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest { CourseId = 1 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(Task.FromResult<Course?>(null));

            //Act
            var result = await _sut.RejectCourseDetailedAsync(request);

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RejectCourseDetailedAsync_TargetFile_MaterialFoundWithLesson_UpdatesMaterialLearningStatus()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest
            {
                CourseId = 1,
                Items = new List<RejectCourseItemDto>
                {
                    new RejectCourseItemDto { Target = "file", MaterialId = 2, Reason = "Bad material" }
                }
            };
            var course = new Course { CourseId = 1 };
            var material = new LearningMaterial { MaterialId = 2, LessonId = 3 };
            var lesson = new Lesson { LessonId = 3 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _materialRepoMock.GetByIdAsync(2).Returns(material);
            _lessonRepoMock.GetByIdAsync(3).Returns(lesson);
            _materialRepoMock.GetByCourseIdAsync(1).Returns(new List<LearningMaterial> { material });

            //Act
            var result = await _sut.RejectCourseDetailedAsync(request);

            //Assert
            result.Should().BeTrue();
            material.LearningStatus.Should().Be(LearningStatus.Rejected.ToValue());
            material.ModerationFeedback.Should().Be("Bad material");
            lesson.LessonStatus.Should().Be(LessonStatus.Rejected.ToValue());
            _materialRepoMock.Received(1).Update(material);
            _lessonRepoMock.Received(1).Update(lesson);
            await _embeddingServiceMock.Received(1).PersistPendingMaterialEmbeddingsAsync(1, Arg.Is<HashSet<int>>(x => x.Contains(2)));
        }

        [Fact]
        public async Task RejectCourseDetailedAsync_TargetFile_MaterialNotFound_Ignores()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest
            {
                CourseId = 1,
                Items = new List<RejectCourseItemDto>
                {
                    new RejectCourseItemDto { Target = "file", MaterialId = 2 }
                }
            };
            var course = new Course { CourseId = 1 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _materialRepoMock.GetByIdAsync(2).Returns(Task.FromResult<LearningMaterial?>(null));

            //Act
            var result = await _sut.RejectCourseDetailedAsync(request);

            //Assert
            result.Should().BeTrue();
            _materialRepoMock.DidNotReceive().Update(Arg.Any<LearningMaterial>());
        }

        [Fact]
        public async Task RejectCourseDetailedAsync_TargetFile_MaterialFoundWithoutLesson_UpdatesMaterial()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest
            {
                CourseId = 1,
                Items = new List<RejectCourseItemDto>
                {
                    new RejectCourseItemDto { Target = "file", MaterialId = 2, Reason = "reason" }
                }
            };
            var course = new Course { CourseId = 1 };
            var material = new LearningMaterial { MaterialId = 2, LessonId = null };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _materialRepoMock.GetByIdAsync(2).Returns(material);
            _materialRepoMock.GetByCourseIdAsync(1).Returns(new List<LearningMaterial> { material });

            //Act
            var result = await _sut.RejectCourseDetailedAsync(request);

            //Assert
            result.Should().BeTrue();
            material.LearningStatus.Should().Be(LearningStatus.Rejected.ToValue());
            _lessonRepoMock.DidNotReceive().Update(Arg.Any<Lesson>());
        }

        [Fact]
        public async Task RejectCourseDetailedAsync_TargetLessonTitle_AddsLessonIdToRejectedList()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest
            {
                CourseId = 1,
                Items = new List<RejectCourseItemDto>
                {
                    new RejectCourseItemDto { Target = "lesson.title", LessonId = 3 }
                }
            };
            var course = new Course { CourseId = 1 };
            var lesson = new Lesson { LessonId = 3 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _lessonRepoMock.GetByIdAsync(3).Returns(lesson);

            //Act
            var result = await _sut.RejectCourseDetailedAsync(request);

            //Assert
            result.Should().BeTrue();
            lesson.LessonStatus.Should().Be(LessonStatus.Rejected.ToValue());
            _lessonRepoMock.Received(1).Update(lesson);
        }

        [Fact]
        public async Task RejectCourseDetailedAsync_TargetCourseField_ExistingFeedback_UpdatesFeedback()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest
            {
                CourseId = 1,
                Items = new List<RejectCourseItemDto>
                {
                    new RejectCourseItemDto { Target = "course.description", Reason = "new reason" }
                }
            };
            var course = new Course 
            { 
                CourseId = 1,
                FieldModerationFeedbacks = new List<CourseFieldModerationFeedback>
                {
                    new CourseFieldModerationFeedback { FieldName = "course.description", FeedbackText = "old reason" }
                }
            };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);

            //Act
            var result = await _sut.RejectCourseDetailedAsync(request);

            //Assert
            result.Should().BeTrue();
            course.FieldModerationFeedbacks.First().FeedbackText.Should().Be("new reason");
        }

        [Fact]
        public async Task RejectCourseDetailedAsync_TargetCourseField_VariousFieldNames_UsesCorrectLabels()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest
            {
                CourseId = 1,
                Items = new List<RejectCourseItemDto>
                {
                    new RejectCourseItemDto { Target = "course.title", Reason = "t" },
                    new RejectCourseItemDto { Target = "course.description", Reason = "d" },
                    new RejectCourseItemDto { Target = "course.thumbnail", Reason = "th" },
                    new RejectCourseItemDto { Target = "course.what_you_will_learn", Reason = "w" },
                    new RejectCourseItemDto { Target = "course.requirements", Reason = "r" },
                    new RejectCourseItemDto { Target = "course.other", Reason = "o" }
                }
            };
            var course = new Course { CourseId = 1 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);

            //Act
            var result = await _sut.RejectCourseDetailedAsync(request);

            //Assert
            result.Should().BeTrue();
            course.ModerationFeedback.Should().Contain("Course Title:");
            course.ModerationFeedback.Should().Contain("Description:");
            course.ModerationFeedback.Should().Contain("Thumbnail:");
            course.ModerationFeedback.Should().Contain("What You Will Learn:");
            course.ModerationFeedback.Should().Contain("Requirements:");
            course.ModerationFeedback.Should().Contain("course.other:");
        }

        [Fact]
        public async Task RejectCourseDetailedAsync_DbUpdateExceptionThrown_ThrowsBadRequestException()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest { CourseId = 1, Items = new List<RejectCourseItemDto>() };
            var course = new Course { CourseId = 1 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _courseRepoMock.SaveChangesAsync().ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateException("error"));

            //Act
            Func<Task> act = async () => await _sut.RejectCourseDetailedAsync(request);

            //Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Database operation failed due to a constraint violation or data issue while saving the course rejection.");
        }

        [Fact]
        public async Task RejectCourseDetailedAsync_EmptyItems_UsesDefaultModerationFeedback()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest { CourseId = 1, Items = new List<RejectCourseItemDto>() };
            var course = new Course { CourseId = 1 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);

            //Act
            var result = await _sut.RejectCourseDetailedAsync(request);

            //Assert
            result.Should().BeTrue();
            course.ModerationFeedback.Should().Be("Course rejected. Please check flagged files.");
        }
        #endregion

        #region FlagCourseDetailedAsync (additional)
        [Fact]
        public async Task FlagCourseDetailedAsync_CourseNull_ReturnsFalse()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest { CourseId = 1 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(Task.FromResult<Course?>(null));

            //Act
            var result = await _sut.FlagCourseDetailedAsync(request);

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task FlagCourseDetailedAsync_FlagCountIsThree_ReturnsFalse()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest { CourseId = 1 };
            var course = new Course { CourseId = 1, CourseFlagCount = 3 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);

            //Act
            var result = await _sut.FlagCourseDetailedAsync(request);

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task FlagCourseDetailedAsync_MaterialNotRemovedNotActive_UpdatesToActive()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest { CourseId = 1, Items = new List<RejectCourseItemDto>() };
            var course = new Course { CourseId = 1 };
            var material = new LearningMaterial { LearningStatus = LearningStatus.Pending.ToValue() };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _materialRepoMock.GetByCourseIdAsync(1).Returns(new List<LearningMaterial> { material });
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.FlagCourseDetailedAsync(request);

            //Assert
            result.Should().BeTrue();
            material.LearningStatus.Should().Be(LearningStatus.Active.ToValue());
            _materialRepoMock.Received(1).Update(material);
        }

        [Fact]
        public async Task FlagCourseDetailedAsync_LessonNotActive_UpdatesToActive()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest { CourseId = 1, Items = new List<RejectCourseItemDto>() };
            var course = new Course { CourseId = 1 };
            var lesson = new Lesson { LessonStatus = LessonStatus.Pending.ToValue() };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _lessonRepoMock.GetByCourseIdAsync(1).Returns(new List<Lesson> { lesson });
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.FlagCourseDetailedAsync(request);

            //Assert
            result.Should().BeTrue();
            lesson.LessonStatus.Should().Be(LessonStatus.Active.ToValue());
            _lessonRepoMock.Received(1).Update(lesson);
        }

        [Fact]
        public async Task FlagCourseDetailedAsync_TargetFile_MaterialFoundWithLesson_UpdatesMaterialLearningStatusToFlagged()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest
            {
                CourseId = 1,
                Items = new List<RejectCourseItemDto>
                {
                    new RejectCourseItemDto { Target = "file", MaterialId = 2, Reason = "flag reason" }
                }
            };
            var course = new Course { CourseId = 1 };
            var material = new LearningMaterial { MaterialId = 2, LessonId = 3 };
            var lesson = new Lesson { LessonId = 3 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _materialRepoMock.GetByIdAsync(2).Returns(material);
            _lessonRepoMock.GetByIdAsync(3).Returns(lesson);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.FlagCourseDetailedAsync(request);

            //Assert
            result.Should().BeTrue();
            material.LearningStatus.Should().Be(LearningStatus.Flagged.ToValue());
            material.ModerationFeedback.Should().Be("flag reason");
            _materialRepoMock.Received(1).Update(material);
            lesson.LessonStatus.Should().Be(LessonStatus.Rejected.ToValue());
            _lessonRepoMock.Received(1).Update(lesson);
        }

        [Fact]
        public async Task FlagCourseDetailedAsync_TargetLessonTitle_AddsToFlaggedListAndFeedbackParts()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest
            {
                CourseId = 1,
                Items = new List<RejectCourseItemDto>
                {
                    new RejectCourseItemDto { Target = "lesson.title", LessonId = 3, LessonTitle = "L1", Reason = "r1" },
                    new RejectCourseItemDto { Target = "lesson.title", LessonId = 4, LessonTitle = null, Reason = "r2" }
                }
            };
            var course = new Course { CourseId = 1 };
            var lesson3 = new Lesson { LessonId = 3 };
            var lesson4 = new Lesson { LessonId = 4 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _lessonRepoMock.GetByIdAsync(3).Returns(lesson3);
            _lessonRepoMock.GetByIdAsync(4).Returns(lesson4);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.FlagCourseDetailedAsync(request);

            //Assert
            result.Should().BeTrue();
            course.ModerationFeedback.Should().Contain("[Lesson: L1]");
            course.ModerationFeedback.Should().Contain("[Lesson: #4]");
        }

        [Fact]
        public async Task FlagCourseDetailedAsync_TargetCourseField_VariousFields_AddsFeedback()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest
            {
                CourseId = 1,
                Items = new List<RejectCourseItemDto>
                {
                    new RejectCourseItemDto { Target = "course.title", Reason = "t" },
                    new RejectCourseItemDto { Target = "course.description", Reason = "d" },
                    new RejectCourseItemDto { Target = "course.thumbnail", Reason = "th" },
                    new RejectCourseItemDto { Target = "course.what_you_will_learn", Reason = "w" },
                    new RejectCourseItemDto { Target = "course.requirements", Reason = "r" },
                    new RejectCourseItemDto { Target = "course.other", Reason = "o" }
                }
            };
            var course = new Course { CourseId = 1 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.FlagCourseDetailedAsync(request);

            //Assert
            result.Should().BeTrue();
            course.ModerationFeedback.Should().Contain("[@Title]");
            course.ModerationFeedback.Should().Contain("[@Description]");
            course.ModerationFeedback.Should().Contain("[@Thumbnail]");
            course.ModerationFeedback.Should().Contain("[@What you will learn]");
            course.ModerationFeedback.Should().Contain("[@Requirements]");
            course.ModerationFeedback.Should().Contain("[@course.other]");
            course.FieldModerationFeedbacks.Should().HaveCount(6);
        }

        [Fact]
        public async Task FlagCourseDetailedAsync_SaveChangesReturnsZero_ThrowsInvalidOperationException()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest { CourseId = 1, Items = new List<RejectCourseItemDto>() };
            var course = new Course { CourseId = 1 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _courseRepoMock.SaveChangesAsync().Returns(0);

            //Act
            Func<Task> act = async () => await _sut.FlagCourseDetailedAsync(request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Failed to save changes when flagging course (detailed).");
        }

        [Fact]
        public async Task FlagCourseDetailedAsync_SecondFlag_SendsSecondWarning()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest { CourseId = 1, Items = new List<RejectCourseItemDto>() };
            var course = new Course { CourseId = 1, CourseFlagCount = 1, InstructorId = 2 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.FlagCourseDetailedAsync(request);

            //Assert
            result.Should().BeTrue();
            await _notifMock.Received(1).SendNotificationAsync(2, "Severe Violation Warning (2nd Time)", Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task FlagCourseDetailedAsync_ThirdFlag_UpdatesCourseToArchivedAndSendsPermanentNotice()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest { CourseId = 1, Items = new List<RejectCourseItemDto>() };
            var course = new Course { CourseId = 1, CourseFlagCount = 2, InstructorId = 2 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.FlagCourseDetailedAsync(request);

            //Assert
            result.Should().BeTrue();
            course.CourseStatus.Should().Be(CourseStatus.Archived.ToValue());
            await _notifMock.Received(1).SendNotificationAsync(2, "Permanent Course Discontinuation Notice (3rd Time)", Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task FlagCourseDetailedAsync_NoInstructor_UpdatesCourseAndReturnsTrue()
        {
            //Arrange 1
            var request = new RejectCourseDetailedRequest { CourseId = 1, Items = new List<RejectCourseItemDto>() };
            var course = new Course { CourseId = 1, InstructorId = null };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.FlagCourseDetailedAsync(request);

            //Assert
            result.Should().BeTrue();
            await _notifMock.DidNotReceiveWithAnyArgs().SendNotificationAsync(default, default!, default!, default);
        }
        #endregion

        #region NotifyAdminAsync
        [Fact]
        public async Task NotifyAdminAsync_AdminIdFound_SendsNotification()
        {
            //Arrange 1
            //Arrange 2
            _userRepoMock.GetAdminIdAsync().Returns(99);

            //Act
            await _sut.NotifyAdminAsync("title", "content", "link");

            //Assert
            await _notifMock.Received(1).SendNotificationAsync(99, "title", "content", "link");
        }

        [Fact]
        public async Task NotifyAdminAsync_AdminIdNull_DoesNotSendNotification()
        {
            //Arrange 1
            //Arrange 2
            _userRepoMock.GetAdminIdAsync().Returns(Task.FromResult<int?>(null));

            //Act
            await _sut.NotifyAdminAsync("title", "content", "link");

            //Assert
            await _notifMock.DidNotReceiveWithAnyArgs().SendNotificationAsync(default, default!, default!, default);
        }
        #endregion

    }
}
