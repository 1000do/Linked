using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Domain.Enums;
using CourseMarketplaceBE.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using System.Linq;
using CourseMarketplaceBE.Application.Exceptions;
using NSubstitute.ExceptionExtensions;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class CourseCommandServiceTests
    {
        private readonly ICourseRepository _courseRepoMock;
        private readonly IInstructorRepository _instructorRepoMock;
        private readonly IFileUploadService _uploadServiceMock;
        private readonly IMaterialRepository _materialRepoMock;
        private readonly ILessonRepository _lessonRepoMock;
        private readonly IRedisService _redisServiceMock;
        private readonly IContentHashService _hashServiceMock;
        private readonly ICourseAiIntegrationRepository _aiRepoMock;
        private readonly ILogger<CourseCommandService> _loggerMock;
        private readonly IHttpClientFactory _httpClientFactoryMock;
        private readonly IMapper _mapperMock;
        private readonly ILockoutRepository _lockoutRepoMock;
        private readonly ICourseExtRepository _courseExtRepoMock;
        private readonly IHtmlTextManipulationService _htmlServiceMock;
        private readonly INotificationService _notificationServiceMock;
        private readonly IUserRepository _userRepoMock;
        
        private readonly CourseCommandService _sut;

        public CourseCommandServiceTests()
        {
            _courseRepoMock = Substitute.For<ICourseRepository>();
            _instructorRepoMock = Substitute.For<IInstructorRepository>();
            _uploadServiceMock = Substitute.For<IFileUploadService>();
            _materialRepoMock = Substitute.For<IMaterialRepository>();
            _lessonRepoMock = Substitute.For<ILessonRepository>();
            _redisServiceMock = Substitute.For<IRedisService>();
            _hashServiceMock = Substitute.For<IContentHashService>();
            _aiRepoMock = Substitute.For<ICourseAiIntegrationRepository>();
            _loggerMock = Substitute.For<ILogger<CourseCommandService>>();
            _httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            _mapperMock = Substitute.For<IMapper>();
            _lockoutRepoMock = Substitute.For<ILockoutRepository>();
            _courseExtRepoMock = Substitute.For<ICourseExtRepository>();
            _htmlServiceMock = Substitute.For<IHtmlTextManipulationService>();
            _notificationServiceMock = Substitute.For<INotificationService>();
            _userRepoMock = Substitute.For<IUserRepository>();

            _httpClientFactoryMock.CreateClient(Arg.Any<string>()).Returns(new HttpClient());

            _sut = new CourseCommandService(
                _courseRepoMock,
                _instructorRepoMock,
                _uploadServiceMock,
                _materialRepoMock,
                _lessonRepoMock,
                _redisServiceMock,
                _hashServiceMock,
                _aiRepoMock,
                _loggerMock,
                _httpClientFactoryMock,
                _mapperMock,
                _lockoutRepoMock,
                _courseExtRepoMock,
                _htmlServiceMock,
                _notificationServiceMock,
                _userRepoMock
            );
        }

        [Fact]
        public async Task UpdateCourseAsync_WhenCourseIsPublished_ShouldUpdateStatusToDraft_AndNotNotifyAdmin()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var request = new CourseUpdateRequest 
            {
                Title = "Updated Title",
                Description = "Updated Desc",
                CategoryId = 1,
                Price = 100,
                CourseThumbnailUrl = "url",
                WhatYouWillLearn = "Lots of stuff",
                Requirements = "None"
            };

            var course = new Course 
            { 
                CourseId = courseId, 
                InstructorId = instructorId, 
                CourseStatus = CourseStatus.Published.ToValue(),
                Title = "Old Title"
            };
            
            var instructor = new Instructor { InstructorId = instructorId, StripeAccountId = "acct_123", StripeOnboardingStatus = "active" };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _instructorRepoMock.GetByIdAsync(instructorId).Returns(instructor);
            _htmlServiceMock.SanitizeHtml(Arg.Any<string>()).Returns(x => x.ArgAt<string>(0));
            // _courseRepoMock.Update is void, no need to set up Returns
            _courseRepoMock.SaveChangesAsync().Returns(1);
            _courseExtRepoMock.SaveChangesAsync().Returns(1);

            //Act
            await _sut.UpdateCourseAsync(courseId, request, instructorId);

            //Assert
            course.CourseStatus.Should().Be(CourseStatus.Draft.ToValue());
            
            _courseRepoMock.Received(1).Update(Arg.Is<Course>(c => c.CourseId == courseId && c.CourseStatus == CourseStatus.Draft.ToValue()));
            await _courseExtRepoMock.Received(1).SaveChangesAsync();
            await _notificationServiceMock.DidNotReceive().SendNotificationAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task UpdateCourseStatusAsync_WhenStatusIsPending_ShouldUpdateStatus_AndNotifyAdmin()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            int adminId = 99;
            string status = CourseStatus.Pending.ToValue();

            var course = new Course 
            { 
                CourseId = courseId, 
                InstructorId = instructorId, 
                CourseStatus = CourseStatus.Draft.ToValue(),
                Title = "Test Course",
                WhatYouWillLearn = new string('a', 25),
                Requirements = new string('a', 25),
                CourseThumbnailUrl = "thumb.jpg",
                Price = 100
            };
            
            var instructor = new Instructor { InstructorId = instructorId, StripeAccountId = "acct_123", StripeOnboardingStatus = "active" };

            var materials = new List<LearningMaterial>
            {
                new LearningMaterial { MaterialId = 1, LearningStatus = LearningStatus.Draft.ToValue() },
                new LearningMaterial { MaterialId = 2, LearningStatus = LearningStatus.Active.ToValue() }
            };

            var lessons = new List<Lesson>
            {
                new Lesson 
                { 
                    LessonId = 1, 
                    LessonStatus = LessonStatus.Draft.ToValue(), 
                    LearningMaterials = new List<LearningMaterial> 
                    { 
                        new LearningMaterial { LearningStatus = LearningStatus.Active.ToValue(), MaterialMetadata = new MaterialMetadata { FileType = "video", Duration = 300 } } 
                    } 
                }
            };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _instructorRepoMock.GetByIdAsync(instructorId).Returns(instructor);
            _materialRepoMock.GetByCourseIdAsync(courseId).Returns(materials);
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(lessons);
            _userRepoMock.GetAdminIdAsync().Returns(adminId);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            await _sut.UpdateCourseStatusAsync(courseId, status, instructorId);

            //Assert
            course.CourseStatus.Should().Be(CourseStatus.Pending.ToValue());
            
            _courseRepoMock.Received(1).Update(Arg.Is<Course>(c => c.CourseId == courseId && c.CourseStatus == CourseStatus.Pending.ToValue()));
            await _courseRepoMock.Received(1).SaveChangesAsync();
            
            await _notificationServiceMock.Received(1).SendNotificationAsync(
                adminId, 
                "Course Submitted", 
                $"Course '{course.Title}' was submitted for review.", 
                "/AdminModeration/Courses"
            );
        }

        [Fact]
        public async Task CreateCourseAsync_ReturnsCourseResponse_WhenValidRequestAndInstructorExists()
        {
            //Arrange 1
            int instructorId = 2;
            var request = new CourseCreateRequest 
            { 
                Title = "New Course", 
                CategoryId = 1, 
                Price = 50,
                CourseThumbnailUrl = "url"
            };
            var instructor = new Instructor { InstructorId = instructorId, ApprovalStatus = InstructorApprovalStatus.Approved.ToValue(), StripeAccountId = "acct", StripeOnboardingStatus = "active" };
            var courseEntity = new Course { CourseId = 1, Title = request.Title, Price = request.Price };
            var courseResponse = new CourseResponse { CourseId = 1, Title = request.Title, Price = request.Price };

            //Arrange 2
            _instructorRepoMock.GetByIdAsync(instructorId).Returns(instructor);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _courseRepoMock.Add(Arg.Any<Course>());
            _courseRepoMock.SaveChangesAsync().Returns(1);
            _courseExtRepoMock.SaveChangesAsync().Returns(1);
            _mapperMock.Map<CourseResponse>(Arg.Any<Course>()).Returns(courseResponse);

            //Act
            var result = await _sut.CreateCourseAsync(request, instructorId);

            //Assert
            result.Should().NotBeNull();
            result.CourseId.Should().Be(1);
            result.Title.Should().Be(request.Title);

            await _instructorRepoMock.Received(1).GetByIdAsync(instructorId);
            _courseRepoMock.Received(1).Add(Arg.Is<Course>(c => c.Title == request.Title));
            await _courseExtRepoMock.Received(1).SaveChangesAsync();
            _mapperMock.Received(1).Map<CourseResponse>(Arg.Any<Course>());
        }

        [Fact]
        public async Task UpdateCourseAsync_ThrowsUnauthorized_WhenInstructorDoesNotOwnCourse()
        {
            //Arrange 1
            int courseId = 1;
            int wrongInstructorId = 99;
            var request = new CourseUpdateRequest { Title = "Update" };
            var course = new Course { CourseId = courseId, InstructorId = 2 }; // Owned by 2

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseAsync(courseId, request, wrongInstructorId);

            //Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You do not have permission to modify this course.");

            await _courseRepoMock.Received(1).GetByIdAsync(courseId);
            _courseRepoMock.DidNotReceive().Update(Arg.Any<Course>());
        }

        [Fact]
        public async Task DeleteCourseAsync_DeletesCourse_WhenInstructorOwnsCourse()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var course = new Course { CourseId = courseId, InstructorId = instructorId, CourseStatus = CourseStatus.Draft.ToValue() };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(new List<Lesson>());
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            await _sut.DeleteCourseAsync(courseId, instructorId);

            //Assert
            course.IsRemoved.Should().BeTrue();
            _courseRepoMock.Received(1).Update(Arg.Is<Course>(c => c.CourseId == courseId && c.IsRemoved));
            await _courseRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CreateCourseAsync_RequestNull_ThrowsBadRequestException()
        {
            //Arrange 1
            CourseCreateRequest request = null!;
            int instructorId = 2;

            //Arrange 2

            //Act
            Func<Task> act = async () => await _sut.CreateCourseAsync(request, instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage("Missing required data for course creation.");
        }

        [Fact]
        public async Task CreateCourseAsync_InstructorNotApproved_ThrowsBadRequestException()
        {
            //Arrange 1
            var request = new CourseCreateRequest { Title = "Test" };
            int instructorId = 2;
            var instructor = new Instructor { InstructorId = instructorId, ApprovalStatus = InstructorApprovalStatus.Pending.ToValue() };

            //Arrange 2
            _instructorRepoMock.GetByIdAsync(instructorId).Returns(instructor);

            //Act
            Func<Task> act = async () => await _sut.CreateCourseAsync(request, instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage("You must be an approved instructor to create a course.");
        }

        [Fact]
        public async Task CreateCourseAsync_ActiveLockoutExists_ThrowsBadRequestException()
        {
            //Arrange 1
            var request = new CourseCreateRequest { Title = "Test" };
            int instructorId = 2;
            var instructor = new Instructor { InstructorId = instructorId, ApprovalStatus = InstructorApprovalStatus.Approved.ToValue() };
            var lockout = new Lockout { LockoutEnd = DateTime.UtcNow.AddDays(1) };

            //Arrange 2
            _instructorRepoMock.GetByIdAsync(instructorId).Returns(instructor);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns(lockout);

            //Act
            Func<Task> act = async () => await _sut.CreateCourseAsync(request, instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage($"Your instructor rights are locked until {lockout.LockoutEnd.Value:yyyy-MM-dd HH:mm:ss} due to policy violations. You cannot create new courses.");
        }

        [Fact]
        public async Task CreateCourseAsync_NoStripeLimitExceeded_ThrowsBadRequestException()
        {
            //Arrange 1
            var request = new CourseCreateRequest { Title = "Test" };
            int instructorId = 2;
            var instructor = new Instructor { InstructorId = instructorId, ApprovalStatus = InstructorApprovalStatus.Approved.ToValue(), StripeOnboardingStatus = "pending" };
            var existingCourses = new List<Course> { new Course { IsRemoved = false }, new Course { IsRemoved = false } };

            //Arrange 2
            _instructorRepoMock.GetByIdAsync(instructorId).Returns(instructor);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _courseRepoMock.GetInstructorCoursesAsync(instructorId).Returns(existingCourses);

            //Act
            Func<Task> act = async () => await _sut.CreateCourseAsync(request, instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage("Instructors who have not linked a Stripe account are only allowed to create up to 2 courses.");
        }

        [Fact]
        public async Task CreateCourseAsync_DuplicateContent_ThrowsBadRequestException()
        {
            //Arrange 1
            var request = new CourseCreateRequest { Title = "Test" };
            int instructorId = 2;
            var instructor = new Instructor { InstructorId = instructorId, ApprovalStatus = InstructorApprovalStatus.Approved.ToValue(), StripeAccountId = "acc", StripeOnboardingStatus = "active" };

            //Arrange 2
            _instructorRepoMock.GetByIdAsync(instructorId).Returns(instructor);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _htmlServiceMock.SanitizeHtml(Arg.Any<string>()).Returns(x => x.ArgAt<string>(0));
            _courseExtRepoMock.SaveChangesAsync().Throws(new CourseExtException("{\"constraint\":\"title_hash_idx\"}"));

            //Act
            Func<Task> act = async () => await _sut.CreateCourseAsync(request, instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage("Content duplication detected on 'Title'");
        }

        [Fact]
        public async Task UpdateCourseAsync_CourseNotFound_ThrowsException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var request = new CourseUpdateRequest();

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns((Course)null);

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseAsync(courseId, request, instructorId);

            //Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Course not found.");
        }

        [Fact]
        public async Task UpdateCourseAsync_ActiveLockoutExists_ThrowsBadRequestException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var request = new CourseUpdateRequest();
            var course = new Course { CourseId = courseId, InstructorId = instructorId };
            var lockout = new Lockout { LockoutEnd = DateTime.UtcNow.AddDays(1) };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns(lockout);

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseAsync(courseId, request, instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage($"Your instructor rights are locked until {lockout.LockoutEnd.Value:yyyy-MM-dd HH:mm:ss} due to policy violations. You cannot update courses.");
        }

        [Fact]
        public async Task UpdateCourseAsync_ArchivedWith3Flags_ThrowsBadRequestException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var request = new CourseUpdateRequest();
            var course = new Course { CourseId = courseId, InstructorId = instructorId, CourseStatus = CourseStatus.Archived.ToValue(), CourseFlagCount = 3 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseAsync(courseId, request, instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage("This course has been permanently discontinued due to policy violations and cannot be edited.");
        }

        [Fact]
        public async Task UpdateCourseAsync_CourseIsPending_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var request = new CourseUpdateRequest();
            var course = new Course { CourseId = courseId, InstructorId = instructorId, CourseStatus = CourseStatus.Pending.ToValue() };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseAsync(courseId, request, instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<InvalidOperationException>();
            ex.WithMessage("Cannot modify course while it is pending review.");
        }

        [Fact]
        public async Task UpdateCourseAsync_NoChanges_ReturnsCourse()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var request = new CourseUpdateRequest 
            { 
                CategoryId = 1, Title = "Title", Description = "Desc", Price = 10, CourseThumbnailUrl = "url", WhatYouWillLearn = "What", Requirements = "Req" 
            };
            var course = new Course 
            { 
                CourseId = courseId, InstructorId = instructorId, CategoryId = 1, Title = "Title", Description = "Desc", Price = 10, CourseThumbnailUrl = "url", WhatYouWillLearn = "What", Requirements = "Req", CourseStatus = CourseStatus.Published.ToValue() 
            };
            var instructor = new Instructor { InstructorId = instructorId, StripeAccountId = "acc", StripeOnboardingStatus = "active" };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _htmlServiceMock.SanitizeHtml(Arg.Any<string>()).Returns(x => x.ArgAt<string>(0));
            _instructorRepoMock.GetByIdAsync(instructorId).Returns(instructor);
            _mapperMock.Map<CourseResponse>(course).Returns(new CourseResponse());

            //Act
            var result = await _sut.UpdateCourseAsync(courseId, request, instructorId);

            //Assert
            result.Should().NotBeNull();
            course.CourseStatus.Should().Be(CourseStatus.Published.ToValue()); // Status not changed to draft
        }

        [Fact]
        public async Task UpdateCourseStatusAsync_CourseNotFound_ThrowsKeyNotFoundException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns((Course)null);

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseStatusAsync(courseId, CourseStatus.Pending.ToValue(), instructorId);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Course not found.");
        }

        [Fact]
        public async Task UpdateCourseStatusAsync_UnauthorizedInstructor_ThrowsUnauthorizedAccessException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var course = new Course { CourseId = courseId, InstructorId = 99 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseStatusAsync(courseId, CourseStatus.Pending.ToValue(), instructorId);

            //Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You do not have permission to modify this course.");
        }

        [Fact]
        public async Task UpdateCourseStatusAsync_ActiveLockoutExists_ThrowsBadRequestException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var course = new Course { CourseId = courseId, InstructorId = instructorId };
            var lockout = new Lockout { LockoutEnd = DateTime.UtcNow.AddDays(1) };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns(lockout);

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseStatusAsync(courseId, CourseStatus.Pending.ToValue(), instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage($"Your instructor rights are locked until {lockout.LockoutEnd.Value:yyyy-MM-dd HH:mm:ss} due to policy violations. You cannot change course status.");
        }

        [Fact]
        public async Task UpdateCourseStatusAsync_ArchivedWith3Flags_ThrowsBadRequestException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var course = new Course { CourseId = courseId, InstructorId = instructorId, CourseStatus = CourseStatus.Archived.ToValue(), CourseFlagCount = 3 };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseStatusAsync(courseId, CourseStatus.Pending.ToValue(), instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage("This course has been permanently discontinued due to policy violations and its status cannot be changed.");
        }

        [Fact]
        public async Task UpdateCourseStatusAsync_ToPublished_WhenNotArchived_ThrowsBadRequestException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var course = new Course { CourseId = courseId, InstructorId = instructorId, CourseStatus = CourseStatus.Draft.ToValue() };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseStatusAsync(courseId, CourseStatus.Published.ToValue(), instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage("Only archived courses can be set back to published by instructor.");
        }

        [Fact]
        public async Task UpdateCourseStatusAsync_InvalidStatus_ThrowsBadRequestException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var course = new Course { CourseId = courseId, InstructorId = instructorId, CourseStatus = CourseStatus.Draft.ToValue() };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseStatusAsync(courseId, "invalid_status", instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage("Invalid status. Allowed values are 'pending', 'archived', or 'published' (for unarchiving).");
        }

        [Fact]
        public async Task UpdateCourseStatusAsync_Pending_MissingWhatYouWillLearn_ThrowsBadRequestException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var course = new Course { CourseId = courseId, InstructorId = instructorId, CourseStatus = CourseStatus.Draft.ToValue(), WhatYouWillLearn = "too short" };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseStatusAsync(courseId, CourseStatus.Pending.ToValue(), instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage("What you will learn is required and must be at least 20 characters long.");
        }

        [Fact]
        public async Task UpdateCourseStatusAsync_Pending_MissingRequirements_ThrowsBadRequestException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var course = new Course { CourseId = courseId, InstructorId = instructorId, CourseStatus = CourseStatus.Draft.ToValue(), WhatYouWillLearn = new string('a', 25), Requirements = "short" };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseStatusAsync(courseId, CourseStatus.Pending.ToValue(), instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage("Requirements are required and must be at least 20 characters long.");
        }

        [Fact]
        public async Task UpdateCourseStatusAsync_Pending_MissingThumbnail_ThrowsBadRequestException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var course = new Course { CourseId = courseId, InstructorId = instructorId, CourseStatus = CourseStatus.Draft.ToValue(), WhatYouWillLearn = new string('a', 25), Requirements = new string('a', 25), CourseThumbnailUrl = null };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseStatusAsync(courseId, CourseStatus.Pending.ToValue(), instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage("Course thumbnail is required.");
        }

        [Fact]
        public async Task UpdateCourseStatusAsync_Pending_NoLessons_ThrowsBadRequestException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var course = new Course { CourseId = courseId, InstructorId = instructorId, CourseStatus = CourseStatus.Draft.ToValue(), WhatYouWillLearn = new string('a', 25), Requirements = new string('a', 25), CourseThumbnailUrl = "url" };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(new List<Lesson>());
            _instructorRepoMock.GetByIdAsync(instructorId).Returns(new Instructor { StripeAccountId = "a", StripeOnboardingStatus = "active" });

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseStatusAsync(courseId, CourseStatus.Pending.ToValue(), instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage("Cannot submit course for review. The course must have at least one lesson.");
        }

        [Fact]
        public async Task UpdateCourseStatusAsync_Pending_MissingVideoInLesson_ThrowsBadRequestException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var course = new Course { CourseId = courseId, InstructorId = instructorId, CourseStatus = CourseStatus.Draft.ToValue(), WhatYouWillLearn = new string('a', 25), Requirements = new string('a', 25), CourseThumbnailUrl = "url" };
            var lessons = new List<Lesson> { new Lesson { Title = "L1", IsRemoved = false, LearningMaterials = new List<LearningMaterial>() } };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(lessons);
            _instructorRepoMock.GetByIdAsync(instructorId).Returns(new Instructor { StripeAccountId = "a", StripeOnboardingStatus = "active" });

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseStatusAsync(courseId, CourseStatus.Pending.ToValue(), instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage("Cannot submit course for review. Every lesson must contain at least one video. Lesson 'L1' is missing a video.");
        }

        [Fact]
        public async Task UpdateCourseStatusAsync_Pending_NoStripeExceeds30Mins_ThrowsBadRequestException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var course = new Course { CourseId = courseId, InstructorId = instructorId, CourseStatus = CourseStatus.Draft.ToValue(), WhatYouWillLearn = new string('a', 25), Requirements = new string('a', 25), CourseThumbnailUrl = "url", Price = 10 };
            var materials = new List<LearningMaterial> { new LearningMaterial { MaterialMetadata = new MaterialMetadata { FileType = "video", Duration = 1801 } } };
            var lessons = new List<Lesson> { new Lesson { IsRemoved = false, LearningMaterials = materials } };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(lessons);
            _instructorRepoMock.GetByIdAsync(instructorId).Returns(new Instructor { StripeAccountId = null }); // No stripe

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseStatusAsync(courseId, CourseStatus.Pending.ToValue(), instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage("The total video duration of the course is currently 30 minutes. Instructors who have not linked a Stripe account are only allowed a maximum of 30 minutes.");
        }

        [Fact]
        public async Task UpdateCourseStatusAsync_Pending_FreeCourseExceeds60Mins_ThrowsBadRequestException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var course = new Course { CourseId = courseId, InstructorId = instructorId, CourseStatus = CourseStatus.Draft.ToValue(), WhatYouWillLearn = new string('a', 25), Requirements = new string('a', 25), CourseThumbnailUrl = "url", Price = 0 }; // Free course
            var materials = new List<LearningMaterial> { new LearningMaterial { MaterialMetadata = new MaterialMetadata { FileType = "video", Duration = 3601 } } };
            var lessons = new List<Lesson> { new Lesson { IsRemoved = false, LearningMaterials = materials } };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(lessons);
            _instructorRepoMock.GetByIdAsync(instructorId).Returns(new Instructor { StripeAccountId = "a", StripeOnboardingStatus = "active" }); // Has stripe

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseStatusAsync(courseId, CourseStatus.Pending.ToValue(), instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage("The total video duration of the free course is currently 60 minutes. Free courses are only allowed a maximum of 60 minutes.");
        }

        [Fact]
        public async Task DeleteCourseAsync_CourseNotFound_ThrowsException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns((Course)null);

            //Act
            Func<Task> act = async () => await _sut.DeleteCourseAsync(courseId, instructorId);

            //Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Course not found.");
        }

        [Fact]
        public async Task DeleteCourseAsync_ActiveLockoutExists_ThrowsBadRequestException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var course = new Course { CourseId = courseId, InstructorId = instructorId };
            var lockout = new Lockout { LockoutEnd = DateTime.UtcNow.AddDays(1) };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns(lockout);

            //Act
            Func<Task> act = async () => await _sut.DeleteCourseAsync(courseId, instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage($"Your instructor rights are locked until {lockout.LockoutEnd.Value:yyyy-MM-dd HH:mm:ss} due to policy violations. You cannot delete courses.");
        }

        [Fact]
        public async Task DeleteCourseAsync_HasEnrollments_ThrowsBadRequestException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var course = new Course { CourseId = courseId, InstructorId = instructorId, CourseStatus = CourseStatus.Draft.ToValue() };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _courseRepoMock.HasEnrollmentsAsync(courseId).Returns(true);

            //Act
            Func<Task> act = async () => await _sut.DeleteCourseAsync(courseId, instructorId);

            //Assert
            var ex = await act.Should().ThrowAsync<BadRequestException>();
            ex.WithMessage("Cannot delete a course with active students. Please archive it instead.");
        }

        [Fact]
        public async Task IntegrateAItoCourseAsync_ReturnsResultWithDefaultThresholds_WhenConfigJsonIsNull()
        {
            //Arrange 1
            var command = new CourseAIIntegrationCommand { CourseId = 1, ModelId = 2, Role = "Tutor", IsEnabled = true, ConfigJson = null };

            //Arrange 2
            _aiRepoMock.AddAsync(Arg.Any<CourseAiIntegration>()).Returns(Task.CompletedTask);
            _aiRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.IntegrateAItoCourseAsync(command);

            //Assert
            result.Should().NotBeNull();
            result.ConfigJson.Should().NotBeNull();
            result.ConfigJson.Should().ContainKey(AiModelConst.Similarity);
            await _aiRepoMock.Received(1).AddAsync(Arg.Any<CourseAiIntegration>());
            await _aiRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateCourseStatusAndFeedbackAsync_CourseNotFound_ThrowsKeyNotFoundException()
        {
            //Arrange 1
            int courseId = 1;

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns((Course)null);

            //Act
            Func<Task> act = async () => await _sut.UpdateCourseStatusAndFeedbackAsync(courseId, null, null);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Course {courseId} not found");
        }

        [Fact]
        public async Task UpdateCourseStatusAndFeedbackAsync_UpdatesStatusAndThreatLevel()
        {
            //Arrange 1
            int courseId = 1;
            var course = new Course { CourseId = courseId };
            string status = "approved";
            string feedback = "Looks good";
            var threatLevel = AiThreatLevel.Approved;

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            await _sut.UpdateCourseStatusAndFeedbackAsync(courseId, status, feedback, threatLevel);

            //Assert
            course.CourseStatus.Should().Be("approved");
            course.ModerationFeedback.Should().Be(feedback);
            course.ThreatLevel.Should().Be(threatLevel);
            
            _courseRepoMock.Received(1).Update(course);
            await _courseRepoMock.Received(1).SaveChangesAsync();
            await _redisServiceMock.Received(1).RemoveCacheAsync(Arg.Any<string>());
        }
        [Fact]
        public async Task CreateCourseAsync_DuplicateDescription_ThrowsBadRequestException()
        {
            var request = new CourseCreateRequest { Title = "Test" };
            var instructor = new Instructor { InstructorId = 2, ApprovalStatus = InstructorApprovalStatus.Approved.ToValue() };
            _instructorRepoMock.GetByIdAsync(2).Returns(instructor);
            _courseExtRepoMock.SaveChangesAsync().Throws(new CourseExtException("{\"constraint\":\"description_hash\"}"));

            Func<Task> act = async () => await _sut.CreateCourseAsync(request, 2);
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Content duplication detected on 'Description'");
        }

        [Fact]
        public async Task CreateCourseAsync_DuplicateWhatYouWillLearn_ThrowsBadRequestException()
        {
            var request = new CourseCreateRequest { Title = "Test" };
            var instructor = new Instructor { InstructorId = 2, ApprovalStatus = InstructorApprovalStatus.Approved.ToValue() };
            _instructorRepoMock.GetByIdAsync(2).Returns(instructor);
            _courseExtRepoMock.SaveChangesAsync().Throws(new CourseExtException("{\"constraint\":\"what_you_will_learn_hash\"}"));

            Func<Task> act = async () => await _sut.CreateCourseAsync(request, 2);
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Content duplication detected on 'What You Will Learn'");
        }

        [Fact]
        public async Task CreateCourseAsync_DuplicateRequirements_ThrowsBadRequestException()
        {
            var request = new CourseCreateRequest { Title = "Test" };
            var instructor = new Instructor { InstructorId = 2, ApprovalStatus = InstructorApprovalStatus.Approved.ToValue() };
            _instructorRepoMock.GetByIdAsync(2).Returns(instructor);
            _courseExtRepoMock.SaveChangesAsync().Throws(new CourseExtException("{\"constraint\":\"requirements_hash\"}"));

            Func<Task> act = async () => await _sut.CreateCourseAsync(request, 2);
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Content duplication detected on 'Requirements'");
        }

        [Fact]
        public async Task CreateCourseAsync_DuplicateThumbnail_ThrowsBadRequestException()
        {
            var request = new CourseCreateRequest { Title = "Test" };
            var instructor = new Instructor { InstructorId = 2, ApprovalStatus = InstructorApprovalStatus.Approved.ToValue() };
            _instructorRepoMock.GetByIdAsync(2).Returns(instructor);
            _courseExtRepoMock.SaveChangesAsync().Throws(new CourseExtException("{\"constraint\":\"thumbnail_hash\"}"));

            Func<Task> act = async () => await _sut.CreateCourseAsync(request, 2);
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Content duplication detected on 'Thumbnail'");
        }

        [Fact]
        public async Task CreateCourseAsync_DuplicateFallback_ThrowsBadRequestException()
        {
            var request = new CourseCreateRequest { Title = "Test" };
            var instructor = new Instructor { InstructorId = 2, ApprovalStatus = InstructorApprovalStatus.Approved.ToValue() };
            _instructorRepoMock.GetByIdAsync(2).Returns(instructor);
            _courseExtRepoMock.SaveChangesAsync().Throws(new CourseExtException("{\"constraint\":\"idx_some_random_field\"}"));

            Func<Task> act = async () => await _sut.CreateCourseAsync(request, 2);
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Content duplication detected on 'Some Random'");
        }

        [Fact]
        public async Task CreateCourseAsync_WithThumbnailFile_UploadsThumbnail()
        {
            var fileMock = Substitute.For<Microsoft.AspNetCore.Http.IFormFile>();
            var request = new CourseCreateRequest { Title = "Test", ThumbnailFile = fileMock };
            var instructor = new Instructor { InstructorId = 2, ApprovalStatus = InstructorApprovalStatus.Approved.ToValue() };
            _instructorRepoMock.GetByIdAsync(2).Returns(instructor);
            _uploadServiceMock.UploadImageAsync(fileMock).Returns("uploaded_url");
            _courseExtRepoMock.SaveChangesAsync().Returns(1);
            _mapperMock.Map<CourseResponse>(Arg.Any<Course>()).Returns(new CourseResponse());

            await _sut.CreateCourseAsync(request, 2);

            _courseRepoMock.Received(1).Add(Arg.Is<Course>(c => c.CourseThumbnailUrl == "uploaded_url"));
        }

        [Fact]
        public async Task UpdateCourseAsync_ExtNotFound_CreatesNewExt()
        {
            int courseId = 1;
            var request = new CourseUpdateRequest { Title = "New Title", CategoryId = 2, Price = 50, Description = "D", WhatYouWillLearn = "W", Requirements = "R", CourseThumbnailUrl = "url" };
            var course = new Course { CourseId = courseId, InstructorId = 2 };
            var instructor = new Instructor { InstructorId = 2, ApprovalStatus = InstructorApprovalStatus.Approved.ToValue() };

            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _instructorRepoMock.GetByIdAsync(2).Returns(instructor);
            _courseExtRepoMock.GetByIdAsync(courseId).Returns((CourseExt)null);
            _courseExtRepoMock.SaveChangesAsync().Returns(1);
            _mapperMock.Map<CourseResponse>(course).Returns(new CourseResponse());

            await _sut.UpdateCourseAsync(courseId, request, 2);

            await _courseExtRepoMock.Received(1).AddAsync(Arg.Is<CourseExt>(e => e.CourseId == courseId));
        }

        [Fact]
        public async Task UpdateCourseStatusAsync_Pending_ValidDuration_SendsNotification()
        {
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = 2, CourseStatus = CourseStatus.Draft.ToValue(), WhatYouWillLearn = new string('a', 25), Requirements = new string('a', 25), CourseThumbnailUrl = "url", Price = 10, Title = "Title" };
            var instructor = new Instructor { InstructorId = 2, StripeAccountId = "acct", StripeOnboardingStatus = "active" };
            var materials = new List<LearningMaterial> { new LearningMaterial { LearningStatus = LearningStatus.Active.ToValue(), MaterialMetadata = new MaterialMetadata { FileType = "video", Duration = 300 } } };
            var lessons = new List<Lesson> { new Lesson { IsRemoved = false, LearningMaterials = materials } };

            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _instructorRepoMock.GetByIdAsync(2).Returns(instructor);
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(lessons);
            _userRepoMock.GetAdminIdAsync().Returns(99);

            await _sut.UpdateCourseStatusAsync(courseId, CourseStatus.Pending.ToValue(), 2);

            await _notificationServiceMock.Received(1).SendNotificationAsync(99, "Course Submitted", "Course 'Title' was submitted for review.", "/AdminModeration/Courses");
        }

        [Fact]
        public async Task DeleteCourseAsync_MultipleLessons_SetsIsRemovedOnAll()
        {
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = 2, CourseStatus = CourseStatus.Draft.ToValue() };
            var lesson1 = new Lesson { LessonId = 1, IsRemoved = false };
            var lesson2 = new Lesson { LessonId = 2, IsRemoved = false };

            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(new List<Lesson> { lesson1, lesson2 });

            await _sut.DeleteCourseAsync(courseId, 2);

            _lessonRepoMock.Received(1).Update(lesson1);
            _lessonRepoMock.Received(1).Update(lesson2);
            lesson1.IsRemoved.Should().BeTrue();
            lesson2.IsRemoved.Should().BeTrue();
        }

        [Fact]
        public async Task CreateCourseAsync_EmptyDescription_ComputesHashForEmptyString()
        {
            var request = new CourseCreateRequest { Title = "Test", Description = null, WhatYouWillLearn = null, Requirements = null };
            var instructor = new Instructor { InstructorId = 2, ApprovalStatus = InstructorApprovalStatus.Approved.ToValue() };
            
            _instructorRepoMock.GetByIdAsync(2).Returns(instructor);
            _courseExtRepoMock.SaveChangesAsync().Returns(1);
            _mapperMock.Map<CourseResponse>(Arg.Any<Course>()).Returns(new CourseResponse());

            await _sut.CreateCourseAsync(request, 2);

            await _hashServiceMock.Received(3).ComputeCourseHashAsync(""); // Desc, What, Req
        }
        [Fact]
        public async Task UpdateCourseAsync_EmptyThumbnailUrl_KeepsOldThumbnail()
        {
            var course = new Course { CourseId = 1, InstructorId = 2, CourseThumbnailUrl = "old_url" };
            var request = new CourseUpdateRequest { CourseThumbnailUrl = null };
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _instructorRepoMock.GetByIdAsync(2).Returns(new Instructor { StripeAccountId = "acc", StripeOnboardingStatus = "active" });
            
            await _sut.UpdateCourseAsync(1, request, 2);
            
            course.CourseThumbnailUrl.Should().Be("old_url");
        }

        [Fact]
        public async Task UpdateCourseAsync_WithThumbnailFile_UploadsNewThumbnail()
        {
            var course = new Course { CourseId = 1, InstructorId = 2, CourseThumbnailUrl = "old_url" };
            var fileMock = Substitute.For<Microsoft.AspNetCore.Http.IFormFile>();
            var request = new CourseUpdateRequest { ThumbnailFile = fileMock };
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _instructorRepoMock.GetByIdAsync(2).Returns(new Instructor { StripeAccountId = "acc", StripeOnboardingStatus = "active" });
            _uploadServiceMock.UploadImageAsync(fileMock).Returns("new_url");
            
            await _sut.UpdateCourseAsync(1, request, 2);
            
            course.CourseThumbnailUrl.Should().Be("new_url");
        }

        [Fact]
        public async Task UpdateCourseAsync_WithPartialUpdates_UpdatesOnlyProvidedFields()
        {
            var course = new Course { CourseId = 1, InstructorId = 2, CategoryId = 1, Price = 10, Title = "old", Description = "old", WhatYouWillLearn = "old", Requirements = "old" };
            var request = new CourseUpdateRequest { CategoryId = 2, Price = 20, Title = "new", Description = "new", WhatYouWillLearn = "new", Requirements = "new" };
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _instructorRepoMock.GetByIdAsync(2).Returns(new Instructor { StripeAccountId = "acc", StripeOnboardingStatus = "active" });
            _htmlServiceMock.SanitizeHtml(Arg.Any<string>()).Returns(x => x.ArgAt<string>(0));
            
            await _sut.UpdateCourseAsync(1, request, 2);
            
            course.CategoryId.Should().Be(2);
            course.Price.Should().Be(20);
            course.Title.Should().Be("new");
            course.Description.Should().Be("new");
            course.WhatYouWillLearn.Should().Be("new");
            course.Requirements.Should().Be("new");
        }

        [Fact]
        public async Task ValidateCourseDurationLimitsAsync_NoStripe_Under30Mins_Returns()
        {
            var course = new Course { CourseId = 1, InstructorId = 2, Price = 10, CourseStatus = CourseStatus.Draft.ToValue(), WhatYouWillLearn = new string('a', 20), Requirements = new string('a', 20), CourseThumbnailUrl = "url" };
            var lessons = new List<Lesson> { new Lesson { LearningMaterials = new List<LearningMaterial> { new LearningMaterial { MaterialMetadata = new MaterialMetadata { FileType = "video", Duration = 1000 } } } } };
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _instructorRepoMock.GetByIdAsync(2).Returns(new Instructor { StripeAccountId = null });
            _lessonRepoMock.GetByCourseIdAsync(1).Returns(lessons);

            await _sut.UpdateCourseStatusAsync(1, CourseStatus.Pending.ToValue(), 2);
            
            course.CourseStatus.Should().Be(CourseStatus.Pending.ToValue());
        }

        [Fact]
        public async Task ValidateCourseDurationLimitsAsync_FreeCourse_Under60Mins_Returns()
        {
            var course = new Course { CourseId = 1, InstructorId = 2, Price = 0, CourseStatus = CourseStatus.Draft.ToValue(), WhatYouWillLearn = new string('a', 20), Requirements = new string('a', 20), CourseThumbnailUrl = "url" };
            var lessons = new List<Lesson> { new Lesson { LearningMaterials = new List<LearningMaterial> { new LearningMaterial { MaterialMetadata = new MaterialMetadata { FileType = "video", Duration = 2000 } } } } };
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _instructorRepoMock.GetByIdAsync(2).Returns(new Instructor { StripeAccountId = "acct", StripeOnboardingStatus = "active" });
            _lessonRepoMock.GetByCourseIdAsync(1).Returns(lessons);

            await _sut.UpdateCourseStatusAsync(1, CourseStatus.Pending.ToValue(), 2);
            
            course.CourseStatus.Should().Be(CourseStatus.Pending.ToValue());
        }

        [Fact]
        public async Task UpdateCourseStatusAndFeedbackAsync_NullStatusAndThreatLevel_KeepsOldValues()
        {
            var course = new Course { CourseId = 1, CourseStatus = "old_status", ThreatLevel = AiThreatLevel.Approved };
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            
            await _sut.UpdateCourseStatusAndFeedbackAsync(1, null, "feedback", null);
            
            course.CourseStatus.Should().Be("old_status");
            course.ThreatLevel.Should().Be(AiThreatLevel.Approved);
        }
        [Fact]
        public async Task UpdateCourseStatusAsync_ToPublished_NotArchived_ThrowsBadRequestException()
        {
            var course = new Course { CourseId = 1, InstructorId = 2, CourseStatus = CourseStatus.Draft.ToValue() };
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _instructorRepoMock.GetByIdAsync(2).Returns(new Instructor { StripeAccountId = "acc", StripeOnboardingStatus = "active" });
            
            Func<Task> act = async () => await _sut.UpdateCourseStatusAsync(1, CourseStatus.Published.ToValue(), 2);
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Only archived courses can be set back to published by instructor.");
        }

        [Fact]
        public async Task UpdateCourseAsync_ExistingExt_UpdatesFingerprint()
        {
            var courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = 2, Title = "old", Description = "old", WhatYouWillLearn = "old", Requirements = "old", CourseThumbnailUrl = "https://example.com/" };
            var request = new CourseUpdateRequest { Title = "new", CategoryId = 2, Price = 50, Description = "D", WhatYouWillLearn = "W", Requirements = "R", CourseThumbnailUrl = "https://example.com/" };
            
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _instructorRepoMock.GetByIdAsync(2).Returns(new Instructor { StripeAccountId = "acc", StripeOnboardingStatus = "active" });
            
            var existingExt = new CourseExt { CourseId = courseId };
            _courseExtRepoMock.GetByIdAsync(courseId).Returns(existingExt);
            _htmlServiceMock.SanitizeHtml(Arg.Any<string>()).Returns(x => x.ArgAt<string>(0));
            
            await _sut.UpdateCourseAsync(courseId, request, 2);
            
            _courseExtRepoMock.Received(1).Update(existingExt);
            await _hashServiceMock.Received(1).ComputeCourseHashAsync("new");
            await _hashServiceMock.Received(1).ComputeCourseHashAsync("D");
            await _hashServiceMock.Received(1).ComputeCourseHashAsync("W");
            await _hashServiceMock.Received(1).ComputeCourseHashAsync("R");
        }

        [Fact]
        public async Task DeleteCourseAsync_InstructorLockedOut_ThrowsBadRequestException()
        {
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = 2 };
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            
            _lockoutRepoMock.GetActiveLockoutAsync(2, "instructor").Returns(new Lockout { LockoutEnd = DateTime.UtcNow.AddDays(1) });
            
            Func<Task> act = async () => await _sut.DeleteCourseAsync(courseId, 2);
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Your instructor rights are locked until*");
        }

        [Fact]
        public async Task DeleteCourseAsync_CourseArchivedWith3Flags_ThrowsBadRequestException()
        {
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = 2, CourseStatus = CourseStatus.Archived.ToValue(), CourseFlagCount = 3 };
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(2, "instructor").Returns((Lockout)null);
            
            Func<Task> act = async () => await _sut.DeleteCourseAsync(courseId, 2);
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("This course has been permanently discontinued due to policy violations and cannot be deleted by the instructor.");
        }

        [Fact]
        public async Task DeleteCourseAsync_CoursePending_ThrowsInvalidOperationException()
        {
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = 2, CourseStatus = CourseStatus.Pending.ToValue() };
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(2, "instructor").Returns((Lockout)null);
            
            Func<Task> act = async () => await _sut.DeleteCourseAsync(courseId, 2);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot delete course while it is pending review.");
        }

        [Fact]
        public async Task DeleteCourseAsync_Unauthorized_ThrowsUnauthorizedAccessException()
        {
            int courseId = 1;
            var course = new Course { CourseId = courseId, InstructorId = 99 };
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            
            Func<Task> act = async () => await _sut.DeleteCourseAsync(courseId, 2);
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You do not have permission to delete this course.");
        }

        [Fact]
        public async Task UpdateCourseStatusAsync_ToPublished_FromArchived_UpdatesStatusSuccessfully()
        {
            var courseId = 1;
            var instructorId = 2;
            var course = new Course 
            { 
                CourseId = courseId, 
                InstructorId = instructorId, 
                CourseStatus = CourseStatus.Archived.ToValue(),
                CourseFlagCount = 0 
            };
            
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);

            await _sut.UpdateCourseStatusAsync(courseId, CourseStatus.Published.ToValue(), instructorId);

            course.CourseStatus.Should().Be(CourseStatus.Published.ToValue());
        }

        [Fact]
        public async Task CreateCourseAsync_WithThumbnailUploadFailure_ReturnsNullThumbnailUrl()
        {
            var request = new CourseCreateRequest
            {
                Title = "Title", CategoryId = 1, Price = 10, Description = "Desc",
                WhatYouWillLearn = "What", Requirements = "Req",
                ThumbnailFile = Substitute.For<IFormFile>()
            };
            
            _instructorRepoMock.GetByIdAsync(2).Returns(new Instructor { ApprovalStatus = InstructorApprovalStatus.Approved.ToValue(), StripeAccountId = "acc", StripeOnboardingStatus = "active" });
            _uploadServiceMock.UploadImageAsync(request.ThumbnailFile).Returns((string)null);
            
            Course savedCourse = null;
            _courseRepoMock.When(x => x.Add(Arg.Any<Course>())).Do(callInfo => savedCourse = callInfo.ArgAt<Course>(0));
            _htmlServiceMock.SanitizeHtml(Arg.Any<string>()).Returns(x => x.ArgAt<string>(0));
            _mapperMock.Map<CourseResponse>(Arg.Any<Course>()).Returns(new CourseResponse());
            
            await _sut.CreateCourseAsync(request, 2);
            
            savedCourse.CourseThumbnailUrl.Should().BeNull();
        }
    }
}
