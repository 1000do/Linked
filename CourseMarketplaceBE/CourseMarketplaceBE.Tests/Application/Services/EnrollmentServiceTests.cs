using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using FluentAssertions;
using NSubstitute;
using Xunit;
using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute.ExceptionExtensions;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class EnrollmentServiceTests
    {
        private readonly IEnrollmentRepository _repoMock;
        private readonly ICourseRepository _courseRepoMock;
        private readonly INotificationService _notifMock;
        private readonly IUserRepository _userRepoMock;
        private readonly EnrollmentService _sut;

        public EnrollmentServiceTests()
        {
            _repoMock = Substitute.For<IEnrollmentRepository>();
            _courseRepoMock = Substitute.For<ICourseRepository>();
            _notifMock = Substitute.For<INotificationService>();
            _userRepoMock = Substitute.For<IUserRepository>();

            _sut = new EnrollmentService(
                _repoMock,
                _courseRepoMock,
                _notifMock,
                _userRepoMock
            );
        }

        // ----------------------------------------------------
        // EnrollFreeAsync Tests
        // ----------------------------------------------------

        [Fact]
        public async Task EnrollFreeAsync_CourseNotFound_ThrowsInvalidOperationException()
        {
            int userId = 1;
            int courseId = 2;
            _courseRepoMock.GetByIdAsync(courseId).Returns((Course)null);

            Func<Task> act = async () => await _sut.EnrollFreeAsync(userId, courseId);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Course not found.");
            await _courseRepoMock.Received(1).GetByIdAsync(courseId);
        }

        [Fact]
        public async Task EnrollFreeAsync_CourseNotPublished_ThrowsInvalidOperationException()
        {
            int userId = 1;
            int courseId = 2;
            var course = new Course { CourseId = courseId, CourseStatus = CourseStatus.Draft.ToValue() };
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);

            Func<Task> act = async () => await _sut.EnrollFreeAsync(userId, courseId);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Course is not published.");
        }

        [Fact]
        public async Task EnrollFreeAsync_CourseIsPaid_ThrowsInvalidOperationException()
        {
            int userId = 1;
            int courseId = 2;
            var course = new Course { CourseId = courseId, CourseStatus = CourseStatus.Published.ToValue(), Price = 10 };
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);

            Func<Task> act = async () => await _sut.EnrollFreeAsync(userId, courseId);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This course is not free. Please complete the purchase via your cart.");
        }

        [Fact]
        public async Task EnrollFreeAsync_InstructorEnrollsOwnCourse_ThrowsInvalidOperationException()
        {
            int userId = 1;
            int courseId = 2;
            var course = new Course { CourseId = courseId, CourseStatus = CourseStatus.Published.ToValue(), Price = 0, InstructorId = userId };
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);

            Func<Task> act = async () => await _sut.EnrollFreeAsync(userId, courseId);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You cannot enroll in your own course.");
        }

        [Fact]
        public async Task EnrollFreeAsync_AlreadyEnrolled_ThrowsInvalidOperationException()
        {
            int userId = 1;
            int courseId = 2;
            var course = new Course { CourseId = courseId, CourseStatus = CourseStatus.Published.ToValue(), Price = 0, InstructorId = 99 };
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(true);

            Func<Task> act = async () => await _sut.EnrollFreeAsync(userId, courseId);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You have already enrolled in this course.");
        }

        [Fact]
        public async Task EnrollFreeAsync_Success_CreatesEnrollmentAndNotifiesInstructor()
        {
            int userId = 1;
            int courseId = 2;
            var course = new Course { CourseId = courseId, CourseStatus = CourseStatus.Published.ToValue(), Price = 0, InstructorId = 99, Title = "Test Title" };
            var transactionMock = Substitute.For<IDbContextTransaction>();

            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(false);
            _repoMock.BeginTransactionAsync().Returns(transactionMock);
            _userRepoMock.GetUserByIdAsync(userId).Returns(new User { FullName = "John Doe" });

            await _sut.EnrollFreeAsync(userId, courseId);

            await _repoMock.Received(1).AddEnrollmentAsync(Arg.Is<Enrollment>(e => e.UserId == userId && e.CourseId == courseId && e.Title == "Test Title"));
            await _repoMock.Received(1).SaveChangesAsync();
            await transactionMock.Received(1).CommitAsync();
            await _notifMock.Received(1).SendNotificationAsync(99, "New student enrolled", "John Doe has enrolled in your free course 'Test Title'.", $"/Course/Details/{courseId}");
        }

        [Fact]
        public async Task EnrollFreeAsync_Success_InstructorIdNull_NoNotificationSent()
        {
            int userId = 1;
            int courseId = 2;
            var course = new Course { CourseId = courseId, CourseStatus = CourseStatus.Published.ToValue(), Price = 0, InstructorId = null };
            var transactionMock = Substitute.For<IDbContextTransaction>();

            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(false);
            _repoMock.BeginTransactionAsync().Returns(transactionMock);

            await _sut.EnrollFreeAsync(userId, courseId);

            await transactionMock.Received(1).CommitAsync();
            await _notifMock.DidNotReceive().SendNotificationAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task EnrollFreeAsync_Success_UserFullNameNull_NotifiesWithFallback()
        {
            int userId = 1;
            int courseId = 2;
            var course = new Course { CourseId = courseId, CourseStatus = CourseStatus.Published.ToValue(), Price = 0, InstructorId = 99, Title = "Title" };
            var transactionMock = Substitute.For<IDbContextTransaction>();

            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(false);
            _repoMock.BeginTransactionAsync().Returns(transactionMock);
            _userRepoMock.GetUserByIdAsync(userId).Returns((User)null);

            await _sut.EnrollFreeAsync(userId, courseId);

            await _notifMock.Received(1).SendNotificationAsync(99, "New student enrolled", "A student has enrolled in your free course 'Title'.", Arg.Any<string>());
        }

        [Fact]
        public async Task EnrollFreeAsync_ExceptionThrown_RollbacksTransaction()
        {
            int userId = 1;
            int courseId = 2;
            var course = new Course { CourseId = courseId, CourseStatus = CourseStatus.Published.ToValue(), Price = 0, InstructorId = 99 };
            var transactionMock = Substitute.For<IDbContextTransaction>();

            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(false);
            _repoMock.BeginTransactionAsync().Returns(transactionMock);
            _repoMock.SaveChangesAsync().ThrowsAsync(new Exception("DB Error"));

            Func<Task> act = async () => await _sut.EnrollFreeAsync(userId, courseId);

            await act.Should().ThrowAsync<Exception>().WithMessage("DB Error");
            await transactionMock.Received(1).RollbackAsync();
        }

        // ----------------------------------------------------
        // GetProgressAsync Tests
        // ----------------------------------------------------

        [Fact]
        public async Task GetProgressAsync_EnrollmentNotFound_NotOwner_ReturnsNull()
        {
            _repoMock.GetEnrollmentAsync(1, 2).Returns((Enrollment)null);
            _courseRepoMock.GetByIdAsync(2).Returns(new Course { InstructorId = 99 });

            var result = await _sut.GetProgressAsync(1, 2);

            result.Should().BeNull();
        }
        
        [Fact]
        public async Task GetProgressAsync_EnrollmentNotFound_CourseIsNull_ReturnsNull()
        {
            _repoMock.GetEnrollmentAsync(1, 2).Returns((Enrollment)null);
            _courseRepoMock.GetByIdAsync(2).Returns((Course)null);

            var result = await _sut.GetProgressAsync(1, 2);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProgressAsync_EnrollmentNotFound_IsOwner_ReturnsVirtualProgress()
        {
            _repoMock.GetEnrollmentAsync(1, 2).Returns((Enrollment)null);
            _courseRepoMock.GetByIdAsync(2).Returns(new Course { InstructorId = 1 });
            _courseRepoMock.GetCourseStatsAsync(2).Returns(new CourseStats { TotalMaterials = 10 });

            var result = await _sut.GetProgressAsync(1, 2);

            result.Should().NotBeNull();
            result.EnrollmentId.Should().Be(0);
            result.LearnedMaterialCount.Should().Be(10);
            result.TotalMaterialCount.Should().Be(10);
            result.CompletedMaterialIds.Should().BeEmpty();
        }
        
        [Fact]
        public async Task GetProgressAsync_EnrollmentNotFound_IsOwner_CourseStatsNull_ReturnsVirtualProgress()
        {
            _repoMock.GetEnrollmentAsync(1, 2).Returns((Enrollment)null);
            _courseRepoMock.GetByIdAsync(2).Returns(new Course { InstructorId = 1 });
            _courseRepoMock.GetCourseStatsAsync(2).Returns((CourseStats)null);

            var result = await _sut.GetProgressAsync(1, 2);

            result.Should().NotBeNull();
            result.EnrollmentId.Should().Be(0);
            result.LearnedMaterialCount.Should().Be(0);
            result.TotalMaterialCount.Should().Be(0);
            result.CompletedMaterialIds.Should().BeEmpty();
        }

        [Fact]
        public async Task GetProgressAsync_Enrolled_ReturnsRealProgress()
        {
            int enrollmentId = 10;
            _repoMock.GetEnrollmentAsync(1, 2).Returns(new Enrollment { EnrollmentId = enrollmentId });
            _courseRepoMock.GetCourseStatsAsync(2).Returns(new CourseStats { TotalMaterials = 5 });
            _repoMock.GetCompletedMaterialCountAsync(enrollmentId).Returns(2);
            _repoMock.GetCompletedMaterialIdsAsync(enrollmentId).Returns(new List<int> { 1, 2 });

            var result = await _sut.GetProgressAsync(1, 2);

            result.Should().NotBeNull();
            result.EnrollmentId.Should().Be(enrollmentId);
            result.TotalMaterialCount.Should().Be(5);
            result.LearnedMaterialCount.Should().Be(2);
            result.CompletedMaterialIds.Should().BeEquivalentTo(new[] { 1, 2 });
        }
        
        [Fact]
        public async Task GetProgressAsync_Enrolled_CourseStatsNull_ReturnsRealProgress()
        {
            int enrollmentId = 10;
            _repoMock.GetEnrollmentAsync(1, 2).Returns(new Enrollment { EnrollmentId = enrollmentId });
            _courseRepoMock.GetCourseStatsAsync(2).Returns((CourseStats)null);
            _repoMock.GetCompletedMaterialCountAsync(enrollmentId).Returns(2);

            var result = await _sut.GetProgressAsync(1, 2);

            result.Should().NotBeNull();
            result.TotalMaterialCount.Should().Be(0);
        }

        // ----------------------------------------------------
        // UpdateProgressAsync Tests
        // ----------------------------------------------------

        [Fact]
        public async Task UpdateProgressAsync_EnrollmentNotFound_IsOwner_ReturnsEarly()
        {
            _repoMock.GetEnrollmentWithProgressAsync(1, 2).Returns((Enrollment)null);
            _courseRepoMock.IsOwnerAsync(1, 2).Returns(true);

            await _sut.UpdateProgressAsync(1, 2, 3);

            await _repoMock.DidNotReceive().AddMaterialCompletionAsync(Arg.Any<MaterialCompletion>());
            await _repoMock.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateProgressAsync_EnrollmentNotFound_NotOwner_ThrowsInvalidOperationException()
        {
            _repoMock.GetEnrollmentWithProgressAsync(1, 2).Returns((Enrollment)null);
            _courseRepoMock.IsOwnerAsync(1, 2).Returns(false);

            Func<Task> act = async () => await _sut.UpdateProgressAsync(1, 2, 3);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You are not enrolled in this course.");
        }

        [Fact]
        public async Task UpdateProgressAsync_AlreadyCompletedMaterial_DoesNotAddCompletion()
        {
            int enrollmentId = 10;
            _repoMock.GetEnrollmentWithProgressAsync(1, 2).Returns(new Enrollment { EnrollmentId = enrollmentId });
            _repoMock.IsMaterialCompletedAsync(enrollmentId, 3).Returns(true);

            await _sut.UpdateProgressAsync(1, 2, 3);

            await _repoMock.DidNotReceive().AddMaterialCompletionAsync(Arg.Any<MaterialCompletion>());
            await _repoMock.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateProgressAsync_NotCompleted_AddsCompletion_DoesNotCompleteCourse()
        {
            int enrollmentId = 10;
            var enrollment = new Enrollment { EnrollmentId = enrollmentId, IsCompleted = false };
            _repoMock.GetEnrollmentWithProgressAsync(1, 2).Returns(enrollment);
            _repoMock.IsMaterialCompletedAsync(enrollmentId, 3).Returns(false);
            _courseRepoMock.GetCourseStatsAsync(2).Returns(new CourseStats { TotalMaterials = 5 });
            _repoMock.GetCompletedMaterialCountAsync(enrollmentId).Returns(2);

            await _sut.UpdateProgressAsync(1, 2, 3);

            await _repoMock.Received(1).AddMaterialCompletionAsync(Arg.Is<MaterialCompletion>(m => m.EnrollmentId == enrollmentId && m.MaterialId == 3));
            await _repoMock.Received(2).SaveChangesAsync(); // One for completion, one for update
            enrollment.IsCompleted.Should().BeFalse();
        }
        
        [Fact]
        public async Task UpdateProgressAsync_NotCompleted_CourseStatsNull_DoesNotCompleteCourse()
        {
            int enrollmentId = 10;
            var enrollment = new Enrollment { EnrollmentId = enrollmentId, IsCompleted = false };
            _repoMock.GetEnrollmentWithProgressAsync(1, 2).Returns(enrollment);
            _repoMock.IsMaterialCompletedAsync(enrollmentId, 3).Returns(false);
            _courseRepoMock.GetCourseStatsAsync(2).Returns((CourseStats)null);
            _repoMock.GetCompletedMaterialCountAsync(enrollmentId).Returns(2);

            await _sut.UpdateProgressAsync(1, 2, 3);

            enrollment.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateProgressAsync_NotCompleted_AddsCompletion_CompletesCourse()
        {
            int enrollmentId = 10;
            var enrollment = new Enrollment { EnrollmentId = enrollmentId, IsCompleted = false };
            _repoMock.GetEnrollmentWithProgressAsync(1, 2).Returns(enrollment);
            _repoMock.IsMaterialCompletedAsync(enrollmentId, 3).Returns(false);
            _courseRepoMock.GetCourseStatsAsync(2).Returns(new CourseStats { TotalMaterials = 5 });
            _repoMock.GetCompletedMaterialCountAsync(enrollmentId).Returns(5);

            await _sut.UpdateProgressAsync(1, 2, 3);

            await _repoMock.Received(2).SaveChangesAsync();
            enrollment.IsCompleted.Should().BeTrue();
            enrollment.CompletedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateProgressAsync_NotCompleted_TotalMaterialsZero_DoesNotCompleteCourse()
        {
            int enrollmentId = 10;
            var enrollment = new Enrollment { EnrollmentId = enrollmentId, IsCompleted = false };
            _repoMock.GetEnrollmentWithProgressAsync(1, 2).Returns(enrollment);
            _repoMock.IsMaterialCompletedAsync(enrollmentId, 3).Returns(false);
            _courseRepoMock.GetCourseStatsAsync(2).Returns(new CourseStats { TotalMaterials = 0 });
            _repoMock.GetCompletedMaterialCountAsync(enrollmentId).Returns(1);

            await _sut.UpdateProgressAsync(1, 2, 3);

            enrollment.IsCompleted.Should().BeFalse();
        }

        // ----------------------------------------------------
        // GetMyEnrolledCoursesAsync Tests
        // ----------------------------------------------------

        [Fact]
        public async Task GetMyEnrolledCoursesAsync_ReturnsEnrolledCourses()
        {
            var enrollments = new List<Enrollment> 
            { 
                new Enrollment 
                { 
                    EnrollmentId = 10, 
                    CourseId = 2, 
                    Course = new Course 
                    { 
                        Title = "Title", 
                        CourseThumbnailUrl = "url",
                        Instructor = new Instructor { InstructorNavigation = new User { FullName = "Inst" } }
                    }, 
                    IsCompleted = true 
                },
                new Enrollment 
                { 
                    EnrollmentId = 11, 
                    CourseId = 3, 
                    // No Course object
                    IsCompleted = false 
                },
                new Enrollment 
                { 
                    EnrollmentId = 12, 
                    CourseId = null
                }
            };

            _repoMock.GetMyEnrolledCoursesAsync(1).Returns(enrollments);
            _courseRepoMock.GetCourseStatsAsync(Arg.Any<List<int>>()).Returns(new List<CourseStats> { new CourseStats { CourseId = 2, TotalMaterials = 4 } });
            _repoMock.GetCompletedMaterialCountAsync(10).Returns(3);
            _repoMock.GetCompletedMaterialCountAsync(11).Returns(0);
            _repoMock.GetCompletedMaterialCountAsync(12).Returns(0);

            var result = (await _sut.GetMyEnrolledCoursesAsync(1)).ToList();

            result.Should().HaveCount(3);
            
            result[0].CourseId.Should().Be(2);
            result[0].Title.Should().Be("Title");
            result[0].CourseThumbnailUrl.Should().Be("url");
            result[0].InstructorName.Should().Be("Inst");
            result[0].ProgressPercentage.Should().Be(75.0); // 3 / 4 * 100
            result[0].IsCompleted.Should().BeTrue();

            result[1].CourseId.Should().Be(3);
            result[1].Title.Should().Be("Unknown Course");
            result[1].InstructorName.Should().Be("Unknown Instructor");
            result[1].ProgressPercentage.Should().Be(0);

            result[2].CourseId.Should().Be(0);
            result[2].Title.Should().Be("Unknown Course");
            result[2].ProgressPercentage.Should().Be(0);
        }

        [Fact]
        public async Task GetMyEnrolledCoursesAsync_EmptyEnrollments_ReturnsEmptyList()
        {
            _repoMock.GetMyEnrolledCoursesAsync(1).Returns(new List<Enrollment>());

            var result = await _sut.GetMyEnrolledCoursesAsync(1);

            result.Should().BeEmpty();
        }
    }
}
