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
    
        [Fact]
        public async Task CreateLessonAsync_DbUpdateException_ThrowsException()
        {
            //Arrange 1
            int courseId = 1;
            int instructorId = 2;
            var request = new LessonCreateRequest { CourseId = courseId, Title = "Title", Description = "Desc" };
            var course = new Course { CourseId = courseId, InstructorId = instructorId, CourseStatus = CourseStatus.Draft.ToValue() };
            var instructor = new Instructor { InstructorId = instructorId, StripeAccountId = "acct_1", StripeOnboardingStatus = "active" };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(instructorId, "instructor").Returns((Lockout)null);
            _instructorRepoMock.GetByIdAsync(instructorId).Returns(instructor);
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(new List<Lesson>());
            _lessonRepoMock.AddAsync(Arg.Any<Lesson>()).Returns(Task.CompletedTask);
            _lessonRepoMock.When(x => x.SaveChangesAsync()).Do(x => throw new Microsoft.EntityFrameworkCore.DbUpdateException("db error", new Exception("inner")));

            //Act
            Func<Task> act = async () => await _sut.CreateLessonAsync(request, instructorId);

            //Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("DB Error: inner");
            await _lessonRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateLessonTitleAsync_DuplicateTitle_ThrowsBadRequestException()
        {
            //Arrange 1
            int lessonId = 1;
            var lesson = new Lesson { LessonId = lessonId, CourseId = 1, Course = new Course { InstructorId = 2 } };
            var request = new LessonUpdateTitleRequest { Title = "Existing Title" };
            var existingLesson = new Lesson { LessonId = 2, Title = "Existing Title", IsRemoved = false };

            //Arrange 2
            _lessonRepoMock.GetByIdAsync(lessonId).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(2, "instructor").Returns((Lockout)null);
            _lessonRepoMock.GetByCourseIdAsync(1).Returns(new List<Lesson> { existingLesson });

            //Act
            Func<Task> act = async () => await _sut.UpdateLessonTitleAsync(lessonId, request, 2);

            //Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Lesson title already exists in this course. Please choose a different title.");
            _lessonRepoMock.DidNotReceive().Update(Arg.Any<Lesson>());
        }

        [Fact]
        public async Task AddMaterialToLessonAsync_InstructorWithoutStripe_LimitExceeded_ThrowsBadRequestException()
        {
            //Arrange 1
            var lesson = new Lesson { LessonId = 1, CourseId = 1, Course = new Course { InstructorId = 2 } };
            var request = new MaterialCreateRequest { MaterialMetadata = new MaterialMetadata { FileType = "document" } };
            var existingMaterials = new List<LearningMaterial> 
            { 
                new LearningMaterial { LearningStatus = LearningStatus.Active.ToValue(), MaterialMetadata = new MaterialMetadata { FileType = "document" } },
                new LearningMaterial { LearningStatus = LearningStatus.Active.ToValue(), MaterialMetadata = new MaterialMetadata { FileType = "file" } }
            };

            //Arrange 2
            _htmlServiceMock.SanitizeHtml(Arg.Any<string>()).Returns(x => x.ArgAt<string>(0));
            _lessonRepoMock.GetByIdAsync(1).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(2, "instructor").Returns((Lockout)null);
            _instructorRepoMock.GetByIdAsync(2).Returns(new Instructor { StripeAccountId = null });
            _materialRepoMock.GetMaterialsByLessonIdAsync(1).Returns(existingMaterials);

            //Act
            Func<Task> act = async () => await _sut.AddMaterialToLessonAsync(1, request, 2);

            //Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Instructors who have not linked a Stripe account are only allowed to attach up to 2 document per lesson.");
            await _materialRepoMock.DidNotReceive().AddAsync(Arg.Any<LearningMaterial>());
        }

        [Fact]
        public async Task AddMaterialToLessonAsync_VideoReplacesExistingVideo_MovesOldToTrash()
        {
            //Arrange 1
            var lesson = new Lesson { LessonId = 1, CourseId = 1, Course = new Course { InstructorId = 2 } };
            var request = new MaterialCreateRequest { Title = "New Video", MaterialUrl = "new_url", MaterialMetadata = new MaterialMetadata { FileType = "video" } };
            var activeVideo = new LearningMaterial { MaterialId = 10, MaterialUrl = "old_url", LearningStatus = LearningStatus.Active.ToValue(), MaterialMetadata = new MaterialMetadata { FileType = "video" } };
            var existingMaterials = new List<LearningMaterial> { activeVideo };

            //Arrange 2
            _htmlServiceMock.SanitizeHtml(Arg.Any<string>()).Returns(x => x.ArgAt<string>(0));
            _lessonRepoMock.GetByIdAsync(1).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(2, "instructor").Returns((Lockout)null);
            _instructorRepoMock.GetByIdAsync(2).Returns(new Instructor { StripeAccountId = "acct_1", StripeOnboardingStatus = "active" });
            _materialRepoMock.GetMaterialsByLessonIdAsync(1).Returns(existingMaterials);
            _uploadServiceMock.MoveToTrashAsync("old_url").Returns("trash_old_url");
            _uploadServiceMock.GetPublicIdFromUrl("old_url").Returns("public_id");

            //Act
            var result = await _sut.AddMaterialToLessonAsync(1, request, 2);

            //Assert
            result.MaterialUrl.Should().Be("new_url");
            activeVideo.MaterialUrl.Should().Be("trash_old_url");
            activeVideo.LearningStatus.Should().Be(LearningStatus.Removed.ToValue());
            _materialRepoMock.Received(1).Update(activeVideo);
            await _materialRepoMock.Received(1).AddAsync(Arg.Is<LearningMaterial>(m => m.MaterialUrl == "new_url"));
            await _materialRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AddMaterialToLessonAsync_OnlyUpdateMetadataForExistingVideo_NoUpload()
        {
            //Arrange 1
            var lesson = new Lesson { LessonId = 1, CourseId = 1, Course = new Course { InstructorId = 2 } };
            var request = new MaterialCreateRequest { Title = "Updated Title", MaterialMetadata = new MaterialMetadata { FileType = "video", Duration = 10 } };
            var activeVideo = new LearningMaterial { MaterialId = 10, Title = "Old Title", LearningStatus = LearningStatus.Active.ToValue(), MaterialMetadata = new MaterialMetadata { FileType = "video" } };
            var extraVideo = new LearningMaterial { MaterialId = 11, LearningStatus = LearningStatus.Active.ToValue(), MaterialMetadata = new MaterialMetadata { FileType = "video" } };
            var existingMaterials = new List<LearningMaterial> { activeVideo, extraVideo };

            //Arrange 2
            _htmlServiceMock.SanitizeHtml(Arg.Any<string>()).Returns(x => x.ArgAt<string>(0));
            _lessonRepoMock.GetByIdAsync(1).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(2, "instructor").Returns((Lockout)null);
            _instructorRepoMock.GetByIdAsync(2).Returns(new Instructor { StripeAccountId = "acct_1", StripeOnboardingStatus = "active" });
            _materialRepoMock.GetMaterialsByLessonIdAsync(1).Returns(existingMaterials);

            //Act
            var result = await _sut.AddMaterialToLessonAsync(1, request, 2);

            //Assert
            result.Title.Should().Be("Updated Title");
            activeVideo.Title.Should().Be("Updated Title");
            extraVideo.LearningStatus.Should().Be(LearningStatus.Removed.ToValue());
            _materialRepoMock.Received(1).Update(activeVideo);
            _materialRepoMock.Received(1).Update(extraVideo);
            await _materialRepoMock.DidNotReceive().AddAsync(Arg.Any<LearningMaterial>());
            await _materialRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateMaterialDetailsAsync_CourseNotPublished_UpdatesNormally()
        {
            //Arrange 1
            var material = new LearningMaterial { MaterialId = 1, LessonId = 1 };
            var lesson = new Lesson { LessonId = 1, Course = new Course { InstructorId = 2, CourseStatus = CourseStatus.Draft.ToValue() } };
            var request = new MaterialUpdateRequest { Title = "New Title" };

            //Arrange 2
            _htmlServiceMock.SanitizeHtml(Arg.Any<string>()).Returns(x => x.ArgAt<string>(0));
            _materialRepoMock.GetByIdAsync(1).Returns(material);
            _lessonRepoMock.GetByIdAsync(1).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(Arg.Any<int>(), Arg.Any<string>()).Returns((Lockout)null);

            //Act
            var result = await _sut.UpdateMaterialDetailsAsync(1, request, 2);

            //Assert
            result.Title.Should().Be("New Title");
            _materialRepoMock.Received(1).Update(material);
            _courseRepoMock.DidNotReceive().Update(Arg.Any<Course>());
            await _materialRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RemoveMaterialAsync_ValidMaterial_MovesToTrashAndSaves()
        {
            //Arrange 1
            var material = new LearningMaterial { MaterialId = 1, LessonId = 1, MaterialUrl = "url" };
            var lesson = new Lesson { LessonId = 1, Course = new Course { InstructorId = 2, CourseStatus = CourseStatus.Published.ToValue() } };

            //Arrange 2
            _materialRepoMock.GetByIdAsync(1).Returns(material);
            _lessonRepoMock.GetByIdAsync(1).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(Arg.Any<int>(), Arg.Any<string>()).Returns((Lockout)null);
            _uploadServiceMock.GetPublicIdFromUrl("url").Returns("public_id");
            _uploadServiceMock.MoveToTrashAsync("url").Returns("trash_url");

            //Act
            await _sut.RemoveMaterialAsync(1, 2);

            //Assert
            material.LearningStatus.Should().Be(LearningStatus.Removed.ToValue());
            material.MaterialUrl.Should().Be("trash_url");
            _materialRepoMock.Received(1).Update(material);
            await _materialRepoMock.Received(1).SaveChangesAsync();
            _courseRepoMock.Received(1).Update(lesson.Course);
        }

        [Fact]
        public async Task DeleteLessonAsync_LessonHasMaterials_MovesMaterialsToTrashAndSoftDeletes()
        {
            //Arrange 1
            var material = new LearningMaterial { MaterialId = 1, MaterialUrl = "url" };
            var materials = new List<LearningMaterial> { material };
            var lesson = new Lesson { LessonId = 1, Course = new Course { InstructorId = 2, CourseStatus = CourseStatus.Published.ToValue() } };

            //Arrange 2
            _lessonRepoMock.GetByIdAsync(1).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(Arg.Any<int>(), Arg.Any<string>()).Returns((Lockout)null);
            _materialRepoMock.GetMaterialsByLessonIdAsync(1).Returns(materials);
            _uploadServiceMock.GetPublicIdFromUrl("url").Returns("public_id");
            _uploadServiceMock.MoveToTrashAsync("url").Returns("trash_url");

            //Act
            await _sut.DeleteLessonAsync(1, 2);

            //Assert
            lesson.IsRemoved.Should().BeTrue();
            material.LearningStatus.Should().Be(LearningStatus.Removed.ToValue());
            _materialRepoMock.Received(1).Update(material);
            _lessonRepoMock.Received(1).Update(lesson);
            await _lessonRepoMock.Received(1).SaveChangesAsync();
            _courseRepoMock.Received(1).Update(lesson.Course);
        }

        [Fact]
        public async Task GetTrashMaterialsAsync_ReturnsMappedMaterials()
        {
            //Arrange 1
            var materials = new List<LearningMaterial> 
            {
                new LearningMaterial { MaterialId = 1, Title = "M1", Lesson = new Lesson { Title = "L1", Course = new Course { Title = "C1" } }, MaterialMetadata = new MaterialMetadata { FileType = "video" }, CloudPublicId = "pub1" }
            };

            //Arrange 2
            _materialRepoMock.GetTrashMaterialsAsync(2).Returns(materials);

            //Act
            var result = await _sut.GetTrashMaterialsAsync(2);

            //Assert
            var response = result.First();
            response.MaterialId.Should().Be(1);
            response.Title.Should().Be("M1");
            response.LessonTitle.Should().Be("L1");
            response.CourseTitle.Should().Be("C1");
            response.FileType.Should().Be("video");
            response.CloudPublicId.Should().Be("pub1");
        }

        [Fact]
        public async Task PermanentDeleteMaterialAsync_NotOwner_ThrowsUnauthorizedAccessException()
        {
            //Arrange 1
            var material = new LearningMaterial { MaterialId = 1 };
            
            //Arrange 2
            _materialRepoMock.GetByIdAsync(1).Returns(material);
            _materialRepoMock.GetTrashMaterialsAsync(2).Returns(new List<LearningMaterial>());

            //Act
            Func<Task> act = async () => await _sut.PermanentDeleteMaterialAsync(1, 2);

            //Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You do not have permission to permanently delete this material.");
        }

        [Fact]
        public async Task PermanentDeleteMaterialAsync_ValidOwner_DeletesFromCloudinaryAndDb()
        {
            //Arrange 1
            var material = new LearningMaterial { MaterialId = 1, CloudPublicId = "pub1", MaterialMetadata = new MaterialMetadata { FileType = "video" }, Lesson = new Lesson { Course = new Course { InstructorId = 2 } } };
            
            //Arrange 2
            _materialRepoMock.GetByIdAsync(1).Returns(material);
            _materialRepoMock.GetTrashMaterialsAsync(2).Returns(new List<LearningMaterial> { material });
            _lockoutRepoMock.GetActiveLockoutAsync(Arg.Any<int>(), Arg.Any<string>()).Returns((Lockout)null);

            //Act
            await _sut.PermanentDeleteMaterialAsync(1, 2);

            //Assert
            await _uploadServiceMock.Received(1).DeleteFileByPublicIdAsync("trash/pub1", "video");
            _materialRepoMock.Received(1).Delete(material);
            await _materialRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RestoreMaterialAsync_ValidMaterial_RestoresFromCloudinaryAndUpdatesStatus()
        {
            //Arrange 1
            var material = new LearningMaterial { MaterialId = 1, LessonId = 1, CloudPublicId = "pub1", MaterialMetadata = new MaterialMetadata { FileType = "raw", FileExtension = "pdf" } };
            var lesson = new Lesson { LessonId = 1, Course = new Course { InstructorId = 2, CourseStatus = CourseStatus.Published.ToValue() } };

            //Arrange 2
            _materialRepoMock.GetByIdAsync(1).Returns(material);
            _lessonRepoMock.GetByIdAsync(1).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(Arg.Any<int>(), Arg.Any<string>()).Returns((Lockout)null);
            _materialRepoMock.GetMaterialsByLessonIdAsync(1).Returns(new List<LearningMaterial>());
            _uploadServiceMock.RestoreFromTrashAsync("pub1.pdf", "raw").Returns("restored_url");

            //Act
            await _sut.RestoreMaterialAsync(1, 2);

            //Assert
            material.MaterialUrl.Should().Be("restored_url");
            material.CloudPublicId.Should().BeNull();
            material.LearningStatus.Should().Be(LearningStatus.Draft.ToValue());
            _materialRepoMock.Received(1).Update(material);
            await _materialRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RestoreMaterialAsync_IsVideoAndActiveVideoExists_MovesExistingToTrash()
        {
            //Arrange 1
            var material = new LearningMaterial { MaterialId = 1, LessonId = 1, MaterialMetadata = new MaterialMetadata { FileType = "video" } };
            var lesson = new Lesson { LessonId = 1, Course = new Course { InstructorId = 2, CourseStatus = CourseStatus.Draft.ToValue() } };
            var activeVideo = new LearningMaterial { MaterialId = 2, MaterialUrl = "url", LearningStatus = LearningStatus.Active.ToValue(), MaterialMetadata = new MaterialMetadata { FileType = "video" } };

            //Arrange 2
            _materialRepoMock.GetByIdAsync(1).Returns(material);
            _lessonRepoMock.GetByIdAsync(1).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(Arg.Any<int>(), Arg.Any<string>()).Returns((Lockout)null);
            _materialRepoMock.GetMaterialsByLessonIdAsync(1).Returns(new List<LearningMaterial> { activeVideo });
            _uploadServiceMock.GetPublicIdFromUrl("url").Returns("public_id");
            _uploadServiceMock.MoveToTrashAsync("url").Returns("trash_url");

            //Act
            await _sut.RestoreMaterialAsync(1, 2);

            //Assert
            activeVideo.LearningStatus.Should().Be(LearningStatus.Removed.ToValue());
            material.LearningStatus.Should().Be(LearningStatus.Draft.ToValue());
            _materialRepoMock.Received(1).Update(activeVideo);
            _materialRepoMock.Received(1).Update(material);
        }

        [Fact]
        public async Task SaveLessonChangesAsync_DbUpdateException_ThrowsBadRequestException()
        {
            //Arrange 1
            var method = typeof(LessonService).GetMethod("SaveLessonChangesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            //Arrange 2
            _lessonRepoMock.When(x => x.SaveChangesAsync()).Do(x => throw new Microsoft.EntityFrameworkCore.DbUpdateException("db error", new Exception("inner")));

            //Act
            Func<Task> act = async () => await (Task<int>)method.Invoke(_sut, null)!;

            //Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("*Database operation failed due to a constraint violation or data issue while saving lessons.*");
        }

        [Fact]
        public async Task SaveMaterialChangesAsync_DbUpdateException_ThrowsBadRequestException()
        {
            //Arrange 1
            var method = typeof(LessonService).GetMethod("SaveMaterialChangesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            //Arrange 2
            _materialRepoMock.When(x => x.SaveChangesAsync()).Do(x => throw new Microsoft.EntityFrameworkCore.DbUpdateException("db error", new Exception("inner")));

            //Act
            Func<Task> act = async () => await (Task<int>)method.Invoke(_sut, null)!;

            //Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("*Database operation failed due to a constraint violation or data issue while saving materials.*");
        }

        [Fact]
        public async Task UpdateLessonStatusByCourseIdAsync_NoLessons_ReturnsZero()
        {
            //Arrange 1
            
            //Arrange 2
            _lessonRepoMock.GetByCourseIdAsync(1).Returns(new List<Lesson>());

            //Act
            var result = await _sut.UpdateLessonStatusByCourseIdAsync(1, "Draft");

            //Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateLessonStatusByCourseIdAsync_WithLessons_UpdatesAndSaves()
        {
            //Arrange 1
            var lesson = new Lesson { LessonId = 1 };
            
            //Arrange 2
            _lessonRepoMock.GetByCourseIdAsync(1).Returns(new List<Lesson> { lesson });
            _lessonRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.UpdateLessonStatusByCourseIdAsync(1, "Draft");

            //Assert
            result.Should().Be(1);
            lesson.LessonStatus.Should().Be("Draft");
            _lessonRepoMock.Received(1).Update(lesson);
        }

        [Fact]
        public async Task UpdateLearningMaterialStatusByCourseIdAsync_NoLessons_ReturnsZero()
        {
            //Arrange 1
            
            //Arrange 2
            _lessonRepoMock.GetByCourseIdAsync(1).Returns(new List<Lesson>());

            //Act
            var result = await _sut.UpdateLearningMaterialStatusByCourseIdAsync(1, "Draft");

            //Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateLearningMaterialStatusByCourseIdAsync_WithLessonsAndMaterials_UpdatesAndSaves()
        {
            //Arrange 1
            var lesson = new Lesson { LessonId = 1 };
            var material = new LearningMaterial { MaterialId = 1 };
            
            //Arrange 2
            _lessonRepoMock.GetByCourseIdAsync(1).Returns(new List<Lesson> { lesson });
            _materialRepoMock.GetMaterialsByLessonIdAsync(1).Returns(new List<LearningMaterial> { material });
            _materialRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.UpdateLearningMaterialStatusByCourseIdAsync(1, "Draft");

            //Assert
            result.Should().Be(1);
            material.LearningStatus.Should().Be("Draft");
            _materialRepoMock.Received(1).Update(material);
        }

        [Fact]
        public async Task CreateLessonAsync_CourseNotFound_ThrowsException()
        {
            _courseRepoMock.GetByIdAsync(1).Returns((Course)null);
            Func<Task> act = async () => await _sut.CreateLessonAsync(new LessonCreateRequest { CourseId = 1 }, 2);
            await act.Should().ThrowAsync<Exception>().WithMessage("Course not found.");
        }

        [Fact]
        public async Task CreateLessonAsync_InstructorWithoutStripe_LimitExceeded_ThrowsBadRequestException()
        {
            var request = new LessonCreateRequest { CourseId = 1, Title = "Title" };
            var course = new Course { CourseId = 1, InstructorId = 2 };
            var instructor = new Instructor { StripeAccountId = null };
            var existingLessons = new List<Lesson> 
            { 
                new Lesson { IsRemoved = false }, new Lesson { IsRemoved = false }, new Lesson { IsRemoved = false }, new Lesson { IsRemoved = false }, new Lesson { IsRemoved = false }
            };

            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(2, "instructor").Returns((Lockout)null);
            _instructorRepoMock.GetByIdAsync(2).Returns(instructor);
            _lessonRepoMock.GetByCourseIdAsync(1).Returns(existingLessons);

            Func<Task> act = async () => await _sut.CreateLessonAsync(request, 2);
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Instructors who have not linked a Stripe account are only allowed to create up to 5 lessons per course.");
        }

        [Fact]
        public async Task CreateLessonAsync_DuplicateTitle_ThrowsBadRequestException()
        {
            var request = new LessonCreateRequest { CourseId = 1, Title = "Existing Title" };
            var course = new Course { CourseId = 1, InstructorId = 2 };
            var instructor = new Instructor { StripeAccountId = "acct_1", StripeOnboardingStatus = "active" };
            var existingLessons = new List<Lesson> { new Lesson { Title = "Existing Title", IsRemoved = false } };

            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(2, "instructor").Returns((Lockout)null);
            _instructorRepoMock.GetByIdAsync(2).Returns(instructor);
            _lessonRepoMock.GetByCourseIdAsync(1).Returns(existingLessons);

            Func<Task> act = async () => await _sut.CreateLessonAsync(request, 2);
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Lesson title already exists in this course. Please choose a different title.");
        }

        [Fact]
        public async Task CreateLessonAsync_WithThumbnailFile_UploadsImage()
        {
            var request = new LessonCreateRequest { CourseId = 1, Title = "Title", ThumbnailFile = Substitute.For<Microsoft.AspNetCore.Http.IFormFile>() };
            var course = new Course { CourseId = 1, InstructorId = 2 };
            var instructor = new Instructor { StripeAccountId = "acct_1", StripeOnboardingStatus = "active" };
            
            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(2, "instructor").Returns((Lockout)null);
            _instructorRepoMock.GetByIdAsync(2).Returns(instructor);
            _lessonRepoMock.GetByCourseIdAsync(1).Returns(new List<Lesson>());
            _uploadServiceMock.UploadImageAsync(request.ThumbnailFile).Returns("uploaded_url");

            var result = await _sut.CreateLessonAsync(request, 2);
            result.ThumbnailUrl.Should().Be("uploaded_url");
        }

        [Fact]
        public async Task UpdateLessonTitleAsync_LessonNotFound_ThrowsException()
        {
            _lessonRepoMock.GetByIdAsync(1).Returns((Lesson)null);
            Func<Task> act = async () => await _sut.UpdateLessonTitleAsync(1, new LessonUpdateTitleRequest(), 2);
            await act.Should().ThrowAsync<Exception>().WithMessage("Lesson not found.");
        }

        [Fact]
        public async Task AddMaterialToLessonAsync_LessonNotFound_ThrowsException()
        {
            _lessonRepoMock.GetByIdAsync(1).Returns((Lesson)null);
            Func<Task> act = async () => await _sut.AddMaterialToLessonAsync(1, new MaterialCreateRequest(), 2);
            await act.Should().ThrowAsync<Exception>().WithMessage("Lesson not found.");
        }

        [Fact]
        public async Task AddMaterialToLessonAsync_WithMaterialFile_UploadsVideo()
        {
            var request = new MaterialCreateRequest { Title = "Title", MaterialFile = Substitute.For<Microsoft.AspNetCore.Http.IFormFile>() };
            var lesson = new Lesson { LessonId = 1, Course = new Course { InstructorId = 2 } };
            
            _lessonRepoMock.GetByIdAsync(1).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(2, "instructor").Returns((Lockout)null);
            _instructorRepoMock.GetByIdAsync(2).Returns(new Instructor { StripeAccountId = "acct_1", StripeOnboardingStatus = "active" });
            _materialRepoMock.GetMaterialsByLessonIdAsync(1).Returns(new List<LearningMaterial>());
            _uploadServiceMock.UploadVideoAsync(request.MaterialFile).Returns("uploaded_video_url");

            var result = await _sut.AddMaterialToLessonAsync(1, request, 2);
            result.MaterialUrl.Should().Be("uploaded_video_url");
        }

        [Fact]
        public async Task RemoveMaterialAsync_MaterialNotFound_ThrowsException()
        {
            _materialRepoMock.GetByIdAsync(1).Returns((LearningMaterial)null);
            Func<Task> act = async () => await _sut.RemoveMaterialAsync(1, 2);
            await act.Should().ThrowAsync<Exception>().WithMessage("Material not found.");
        }

        [Fact]
        public async Task DeleteLessonAsync_LessonNotFound_ThrowsException()
        {
            _lessonRepoMock.GetByIdAsync(1).Returns((Lesson)null);
            Func<Task> act = async () => await _sut.DeleteLessonAsync(1, 2);
            await act.Should().ThrowAsync<Exception>().WithMessage("Lesson not found.");
        }

        [Fact]
        public async Task CreateLessonAsync_InstructorWithoutStripe_LimitNotExceeded_Success()
        {
            var request = new LessonCreateRequest { CourseId = 1, Title = "Title" };
            var course = new Course { CourseId = 1, InstructorId = 2 };
            var instructor = new Instructor { StripeAccountId = null };
            var existingLessons = new List<Lesson>(); // < 5 lessons

            _courseRepoMock.GetByIdAsync(1).Returns(course);
            _lockoutRepoMock.GetActiveLockoutAsync(2, "instructor").Returns((Lockout)null);
            _instructorRepoMock.GetByIdAsync(2).Returns(instructor);
            _lessonRepoMock.GetByCourseIdAsync(1).Returns(existingLessons);

            var result = await _sut.CreateLessonAsync(request, 2);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task AddMaterialToLessonAsync_InstructorWithoutStripe_LimitNotExceeded_Success()
        {
            var request = new MaterialCreateRequest { Title = "Title", MaterialMetadata = new MaterialMetadata { FileType = "document" } };
            var lesson = new Lesson { LessonId = 1, Course = new Course { InstructorId = 2 } };
            
            _htmlServiceMock.SanitizeHtml(Arg.Any<string>()).Returns(x => x.ArgAt<string>(0));
            _lessonRepoMock.GetByIdAsync(1).Returns(lesson);
            _lockoutRepoMock.GetActiveLockoutAsync(2, "instructor").Returns((Lockout)null);
            _instructorRepoMock.GetByIdAsync(2).Returns(new Instructor { StripeAccountId = null });
            _materialRepoMock.GetMaterialsByLessonIdAsync(1).Returns(new List<LearningMaterial>()); // < 2 documents

            var result = await _sut.AddMaterialToLessonAsync(1, request, 2);
            result.Should().NotBeNull();
        }
}
}

