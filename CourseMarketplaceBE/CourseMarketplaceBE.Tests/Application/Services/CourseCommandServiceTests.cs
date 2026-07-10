using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using System.Linq;

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
        public async Task UpdateCourseStatusAsync_WhenStatusIsPending_ShouldChangeDraftMaterialsAndLessonsToPending_AndNotifyAdmin()
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
            materials[0].LearningStatus.Should().Be(LearningStatus.Pending.ToValue());
            materials[1].LearningStatus.Should().Be(LearningStatus.Active.ToValue()); // Should not be modified
            lessons[0].LessonStatus.Should().Be(LessonStatus.Pending.ToValue());
            
            _materialRepoMock.Received(1).Update(Arg.Is<LearningMaterial>(m => m.MaterialId == 1 && m.LearningStatus == LearningStatus.Pending.ToValue()));
            _materialRepoMock.Received(1).Update(Arg.Is<LearningMaterial>(m => m.MaterialId == 2 && m.LearningStatus == LearningStatus.Active.ToValue()));
            
            _lessonRepoMock.Received(1).Update(Arg.Is<Lesson>(l => l.LessonId == 1 && l.LessonStatus == LessonStatus.Pending.ToValue()));
            
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
            _courseRepoMock.Delete(course); // void
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            await _sut.DeleteCourseAsync(courseId, instructorId);

            //Assert
            _courseRepoMock.Received(1).Delete(course);
            await _courseRepoMock.Received(1).SaveChangesAsync();
        }
    }
}
