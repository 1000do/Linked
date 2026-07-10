using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Application.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class LessonServiceTests
    {
        private readonly ILessonRepository _lessonRepoMock;
        private readonly ICourseRepository _courseRepoMock;
        private readonly IMaterialRepository _materialRepoMock;
        private readonly IFileUploadService _uploadServiceMock;
        private readonly IRedisService _redisServiceMock;
        private readonly IInstructorRepository _instructorRepoMock;
        private readonly ILogger<LessonService> _loggerMock;
        private readonly ILockoutRepository _lockoutRepoMock;
        private readonly IHtmlTextManipulationService _htmlServiceMock;

        private readonly LessonService _sut;

        public LessonServiceTests()
        {
            _lessonRepoMock = Substitute.For<ILessonRepository>();
            _courseRepoMock = Substitute.For<ICourseRepository>();
            _materialRepoMock = Substitute.For<IMaterialRepository>();
            _uploadServiceMock = Substitute.For<IFileUploadService>();
            _redisServiceMock = Substitute.For<IRedisService>();
            _instructorRepoMock = Substitute.For<IInstructorRepository>();
            _loggerMock = Substitute.For<ILogger<LessonService>>();
            _lockoutRepoMock = Substitute.For<ILockoutRepository>();
            _htmlServiceMock = Substitute.For<IHtmlTextManipulationService>();

            _sut = new LessonService(
                _lessonRepoMock,
                _courseRepoMock,
                _materialRepoMock,
                _uploadServiceMock,
                _redisServiceMock,
                _instructorRepoMock,
                _loggerMock,
                _lockoutRepoMock,
                _htmlServiceMock
            );
        }

        [Fact]
        public async Task AddLessonAsync_WhenCourseIsPublished_ShouldUpdateLessonAndCourseToDraft()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var request = new LessonCreateRequest
            {
                CourseId = courseId,
                Title = "New Lesson",
                Description = "Description",
                ThumbnailUrl = "url"
            };

            var course = new Course 
            { 
                CourseId = courseId, 
                InstructorId = instructorId, 
                CourseStatus = CourseStatus.Published.ToValue() 
            };
            
            var instructor = new Instructor { InstructorId = instructorId, StripeAccountId = "acct_123", StripeOnboardingStatus = "active" };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _instructorRepoMock.GetByIdAsync(instructorId).Returns(instructor);
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(new List<Lesson>());
            _lessonRepoMock.AddAsync(Arg.Any<Lesson>()).Returns(Task.CompletedTask);
            _lessonRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.CreateLessonAsync(request, instructorId);

            //Assert
            result.LessonStatus.Should().Be(LessonStatus.Draft.ToValue());
            result.CourseStatus.Should().Be(CourseStatus.Draft.ToValue());
            course.CourseStatus.Should().Be(CourseStatus.Draft.ToValue());

            await _lessonRepoMock.Received(1).AddAsync(Arg.Is<Lesson>(l => l.LessonStatus == LessonStatus.Draft.ToValue()));
            _courseRepoMock.Received(1).Update(Arg.Is<Course>(c => c.CourseId == courseId && c.CourseStatus == CourseStatus.Draft.ToValue()));
            await _lessonRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateLessonTitleAsync_WhenCourseIsPublished_ShouldUpdateLessonAndCourseToDraft()
        {
            //Arrange 1
            int lessonId = 1;
            int courseId = 1;
            int instructorId = 2;
            var request = new LessonUpdateTitleRequest
            {
                Title = "Updated Lesson Title"
            };

            var course = new Course 
            { 
                CourseId = courseId, 
                InstructorId = instructorId, 
                CourseStatus = CourseStatus.Published.ToValue() 
            };
            
            var lesson = new Lesson 
            {
                LessonId = lessonId,
                CourseId = courseId,
                Course = course,
                LessonStatus = LessonStatus.Active.ToValue(),
                Title = "Old Title"
            };

            //Arrange 2
            _lessonRepoMock.GetByIdAsync(lessonId).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(new List<Lesson> { lesson });
            _lessonRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.UpdateLessonTitleAsync(lessonId, request, instructorId);

            //Assert
            result.LessonStatus.Should().Be(LessonStatus.Draft.ToValue());
            result.CourseStatus.Should().Be(CourseStatus.Draft.ToValue());
            lesson.LessonStatus.Should().Be(LessonStatus.Draft.ToValue());
            course.CourseStatus.Should().Be(CourseStatus.Draft.ToValue());

            _lessonRepoMock.Received(1).Update(Arg.Is<Lesson>(l => l.LessonId == lessonId && l.LessonStatus == LessonStatus.Draft.ToValue()));
            _courseRepoMock.Received(1).Update(Arg.Is<Course>(c => c.CourseId == courseId && c.CourseStatus == CourseStatus.Draft.ToValue()));
            await _lessonRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AddMaterialToLessonAsync_WhenCourseIsPublished_ShouldUpdateMaterialAndCourseToDraft()
        {
            //Arrange 1
            int lessonId = 1;
            int courseId = 1;
            int instructorId = 2;
            var request = new MaterialCreateRequest
            {
                Title = "New Material",
                Description = "Desc",
                MaterialUrl = "url",
                MaterialMetadata = new MaterialMetadata { FileType = "video" }
            };

            var course = new Course 
            { 
                CourseId = courseId, 
                InstructorId = instructorId, 
                CourseStatus = CourseStatus.Published.ToValue() 
            };
            
            var lesson = new Lesson 
            {
                LessonId = lessonId,
                CourseId = courseId,
                Course = course,
                LessonStatus = LessonStatus.Active.ToValue()
            };

            var instructor = new Instructor { InstructorId = instructorId, StripeAccountId = "acct_123", StripeOnboardingStatus = "active" };

            //Arrange 2
            _lessonRepoMock.GetByIdAsync(lessonId).Returns(lesson);
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _instructorRepoMock.GetByIdAsync(instructorId).Returns(instructor);
            
            // Assuming no existing active videos
            _materialRepoMock.GetMaterialsByLessonIdAsync(lessonId).Returns(new List<LearningMaterial>());
            _materialRepoMock.SaveChangesAsync().Returns(1);
            _courseRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.AddMaterialToLessonAsync(lessonId, request, instructorId);

            //Assert
            result.LearningStatus.Should().Be(LearningStatus.Draft.ToValue());
            course.CourseStatus.Should().Be(CourseStatus.Draft.ToValue());

            await _materialRepoMock.Received(1).AddAsync(Arg.Is<LearningMaterial>(m => m.LearningStatus == LearningStatus.Draft.ToValue()));
            _courseRepoMock.Received(1).Update(Arg.Is<Course>(c => c.CourseId == courseId && c.CourseStatus == CourseStatus.Draft.ToValue()));
            await _materialRepoMock.Received(1).SaveChangesAsync();
            await _courseRepoMock.Received(1).SaveChangesAsync();
        }
        [Fact]
        public async Task DeleteLessonAsync_DeletesLesson_WhenInstructorOwnsLesson()
        {
            //Arrange 1
            int lessonId = 1;
            int courseId = 1;
            int instructorId = 2;

            var course = new Course { CourseId = courseId, InstructorId = instructorId, CourseStatus = CourseStatus.Draft.ToValue() };
            var lesson = new Lesson { LessonId = lessonId, CourseId = courseId, Course = course };

            //Arrange 2
            _lessonRepoMock.GetByIdAsync(lessonId).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _materialRepoMock.GetMaterialsByLessonIdAsync(lessonId).Returns(new List<LearningMaterial>());
            _lessonRepoMock.Delete(lesson);
            _lessonRepoMock.SaveChangesAsync().Returns(1);

            //Act
            await _sut.DeleteLessonAsync(lessonId, instructorId);

            //Assert
            _lessonRepoMock.Received(1).Delete(lesson);
            await _lessonRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RemoveMaterialAsync_SoftDeletesMaterial_WhenInstructorOwnsMaterial()
        {
            //Arrange 1
            int materialId = 1;
            int lessonId = 1;
            int courseId = 1;
            int instructorId = 2;

            var course = new Course { CourseId = courseId, InstructorId = instructorId, CourseStatus = CourseStatus.Draft.ToValue() };
            var lesson = new Lesson { LessonId = lessonId, CourseId = courseId, Course = course };
            var material = new LearningMaterial { MaterialId = materialId, LessonId = lessonId, Lesson = lesson };

            //Arrange 2
            _materialRepoMock.GetByIdAsync(materialId).Returns(material);
            _lessonRepoMock.GetByIdAsync(lessonId).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _materialRepoMock.SaveChangesAsync().Returns(1);

            //Act
            await _sut.RemoveMaterialAsync(materialId, instructorId);

            //Assert
            material.LearningStatus.Should().Be(LearningStatus.Removed.ToValue());
            _materialRepoMock.Received(1).Update(material);
            await _materialRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task GetTrashMaterialsAsync_ReturnsTrashedMaterials_WhenExist()
        {
            //Arrange 1
            int instructorId = 2;
            var materials = new List<LearningMaterial>
            {
                new LearningMaterial { MaterialId = 1, Title = "Trashed" }
            };

            //Arrange 2
            _materialRepoMock.GetTrashMaterialsAsync(instructorId).Returns(materials);

            //Act
            var result = await _sut.GetTrashMaterialsAsync(instructorId);

            //Assert
            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Trashed");

            await _materialRepoMock.Received(1).GetTrashMaterialsAsync(instructorId);
        }

        [Fact]
        public async Task RestoreMaterialAsync_RestoresMaterial_WhenMaterialInTrash()
        {
            //Arrange 1
            int materialId = 1;
            int instructorId = 2;
            var course = new Course { InstructorId = instructorId };
            var lesson = new Lesson { Course = course };
            var material = new LearningMaterial { MaterialId = materialId, LearningStatus = LearningStatus.Removed.ToValue(), Lesson = lesson };

            //Arrange 2
            _materialRepoMock.GetByIdAsync(materialId).Returns(material);
            _lessonRepoMock.GetByIdAsync(material.LessonId ?? 0).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _materialRepoMock.GetMaterialsByLessonIdAsync(lesson.LessonId).Returns(new List<LearningMaterial>());
            _materialRepoMock.SaveChangesAsync().Returns(1);

            //Act
            await _sut.RestoreMaterialAsync(materialId, instructorId);

            //Assert
            material.LearningStatus.Should().NotBe(LearningStatus.Removed.ToValue());
            _materialRepoMock.Received(1).Update(material);
            await _materialRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task PermanentDeleteMaterialAsync_HardDeletesMaterial_WhenMaterialInTrash()
        {
            //Arrange 1
            int materialId = 1;
            int instructorId = 2;
            var course = new Course { InstructorId = instructorId };
            var lesson = new Lesson { Course = course };
            var material = new LearningMaterial { MaterialId = materialId, LearningStatus = LearningStatus.Removed.ToValue(), Lesson = lesson };

            //Arrange 2
            _materialRepoMock.GetByIdAsync(materialId).Returns(material);
            _materialRepoMock.GetTrashMaterialsAsync(instructorId).Returns(new List<LearningMaterial> { material });
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _materialRepoMock.SaveChangesAsync().Returns(1);

            //Act
            await _sut.PermanentDeleteMaterialAsync(materialId, instructorId);

            //Assert
            _materialRepoMock.Received(1).Delete(material);
            await _materialRepoMock.Received(1).SaveChangesAsync();
        }
        [Fact]
        public async Task UpdateMaterialDetailsAsync_MaterialNotFound_ThrowsException()
        {
            //Arrange 1
            int materialId = 1;
            int instructorId = 2;
            var request = new MaterialUpdateRequest { Title = "New Title", Description = "New Desc" };

            //Arrange 2
            _htmlServiceMock.SanitizeHtml(request.Description).Returns(request.Description);
            _materialRepoMock.GetByIdAsync(materialId).Returns((LearningMaterial)null);

            //Act
            Func<Task> act = async () => await _sut.UpdateMaterialDetailsAsync(materialId, request, instructorId);

            //Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Material not found.");
            _materialRepoMock.DidNotReceive().Update(Arg.Any<LearningMaterial>());
        }

        [Fact]
        public async Task UpdateMaterialDetailsAsync_InstructorDoesNotOwnCourse_ThrowsUnauthorized()
        {
            //Arrange 1
            int materialId = 1;
            int instructorId = 2;
            var request = new MaterialUpdateRequest { Title = "New Title", Description = "New Desc" };
            var material = new LearningMaterial { MaterialId = materialId, LessonId = 10 };
            
            //Arrange 2
            _htmlServiceMock.SanitizeHtml(request.Description).Returns(request.Description);
            _materialRepoMock.GetByIdAsync(materialId).Returns(material);
            var lesson = new Lesson { Course = new Course { InstructorId = 999 } }; // Different instructor
            _lessonRepoMock.GetByIdAsync(material.LessonId.Value).Returns(lesson);

            //Act
            Func<Task> act = async () => await _sut.UpdateMaterialDetailsAsync(materialId, request, instructorId);

            //Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You do not have permission to update materials.");
        }

        [Fact]
        public async Task UpdateMaterialDetailsAsync_InstructorLockedOut_ThrowsBadRequest()
        {
            //Arrange 1
            int materialId = 1;
            int instructorId = 2;
            var request = new MaterialUpdateRequest { Title = "New Title", Description = "New Desc" };
            var material = new LearningMaterial { MaterialId = materialId, LessonId = 10 };
            var course = new Course { CourseId = 20, InstructorId = instructorId };
            var lesson = new Lesson { LessonId = 10, CourseId = 20, Course = course };
            var lockout = new Lockout { LockoutEnd = DateTime.UtcNow.AddDays(1) };

            //Arrange 2
            _htmlServiceMock.SanitizeHtml(request.Description).Returns(request.Description);
            _materialRepoMock.GetByIdAsync(materialId).Returns(material);
            _lessonRepoMock.GetByIdAsync(material.LessonId.Value).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns(lockout);

            //Act
            Func<Task> act = async () => await _sut.UpdateMaterialDetailsAsync(materialId, request, instructorId);

            //Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage($"Your instructor rights are locked until *");
        }

        [Fact]
        public async Task UpdateMaterialDetailsAsync_CourseIsPending_ThrowsInvalidOperation()
        {
            //Arrange 1
            int materialId = 1;
            int instructorId = 2;
            var request = new MaterialUpdateRequest { Title = "New Title", Description = "New Desc" };
            var material = new LearningMaterial { MaterialId = materialId, LessonId = 10 };
            var course = new Course { CourseId = 20, InstructorId = instructorId, CourseStatus = CourseStatus.Pending.ToValue() };
            var lesson = new Lesson { LessonId = 10, CourseId = 20, Course = course };

            //Arrange 2
            _htmlServiceMock.SanitizeHtml(request.Description).Returns(request.Description);
            _materialRepoMock.GetByIdAsync(materialId).Returns(material);
            _lessonRepoMock.GetByIdAsync(material.LessonId.Value).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);

            //Act
            Func<Task> act = async () => await _sut.UpdateMaterialDetailsAsync(materialId, request, instructorId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot update materials while the course is pending review.");
        }

        [Fact]
        public async Task UpdateMaterialDetailsAsync_CourseIsArchivedAndFlagged_ThrowsInvalidOperation()
        {
            //Arrange 1
            int materialId = 1;
            int instructorId = 2;
            var request = new MaterialUpdateRequest { Title = "New Title", Description = "New Desc" };
            var material = new LearningMaterial { MaterialId = materialId, LessonId = 10 };
            var course = new Course { CourseId = 20, InstructorId = instructorId, CourseStatus = CourseStatus.Archived.ToValue(), CourseFlagCount = 3 };
            var lesson = new Lesson { LessonId = 10, CourseId = 20, Course = course };

            //Arrange 2
            _htmlServiceMock.SanitizeHtml(request.Description).Returns(request.Description);
            _materialRepoMock.GetByIdAsync(materialId).Returns(material);
            _lessonRepoMock.GetByIdAsync(material.LessonId.Value).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);

            //Act
            Func<Task> act = async () => await _sut.UpdateMaterialDetailsAsync(materialId, request, instructorId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This course is permanently archived due to policy violations and cannot be modified.");
        }

        [Fact]
        public async Task UpdateMaterialDetailsAsync_CourseIsPublished_UpdatesStatusesToDraftAndClearsCache()
        {
            //Arrange 1
            int materialId = 1;
            int instructorId = 2;
            var request = new MaterialUpdateRequest { Title = "New Title", Description = "New Desc" };
            var material = new LearningMaterial { MaterialId = materialId, LessonId = 10 };
            var course = new Course { CourseId = 20, InstructorId = instructorId, CourseStatus = CourseStatus.Published.ToValue(), CourseFlagCount = 0 };
            var lesson = new Lesson { LessonId = 10, CourseId = 20, Course = course };

            //Arrange 2
            _htmlServiceMock.SanitizeHtml(request.Description).Returns(request.Description);
            _materialRepoMock.GetByIdAsync(materialId).Returns(material);
            _lessonRepoMock.GetByIdAsync(material.LessonId.Value).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _materialRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.UpdateMaterialDetailsAsync(materialId, request, instructorId);

            //Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("New Title");
            result.LearningStatus.Should().Be(LearningStatus.Draft.ToValue());
            course.CourseStatus.Should().Be(CourseStatus.Draft.ToValue());

            _materialRepoMock.Received(1).Update(material);
            _courseRepoMock.Received(1).Update(course);
            await _materialRepoMock.Received(1).SaveChangesAsync();
            await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(course.CourseId));
        }

        [Fact]
        public async Task UpdateMaterialDetailsAsync_CourseIsDraft_UpdatesMaterialAndClearsCacheWithoutChangingCourseStatus()
        {
            //Arrange 1
            int materialId = 1;
            int instructorId = 2;
            var request = new MaterialUpdateRequest { Title = "New Title", Description = "New Desc" };
            var material = new LearningMaterial { MaterialId = materialId, LessonId = 10, LearningStatus = LearningStatus.Active.ToValue() };
            var course = new Course { CourseId = 20, InstructorId = instructorId, CourseStatus = CourseStatus.Draft.ToValue(), CourseFlagCount = 0 };
            var lesson = new Lesson { LessonId = 10, CourseId = 20, Course = course };

            //Arrange 2
            _htmlServiceMock.SanitizeHtml(request.Description).Returns(request.Description);
            _materialRepoMock.GetByIdAsync(materialId).Returns(material);
            _lessonRepoMock.GetByIdAsync(material.LessonId.Value).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _materialRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.UpdateMaterialDetailsAsync(materialId, request, instructorId);

            //Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("New Title");
            result.LearningStatus.Should().Be(LearningStatus.Active.ToValue());
            course.CourseStatus.Should().Be(CourseStatus.Draft.ToValue());

            _materialRepoMock.Received(1).Update(material);
            _courseRepoMock.DidNotReceive().Update(course);
            await _materialRepoMock.Received(1).SaveChangesAsync();
            await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(course.CourseId));
        }
    }
}

