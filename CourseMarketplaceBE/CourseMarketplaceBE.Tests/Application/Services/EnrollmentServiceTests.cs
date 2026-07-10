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

        [Fact]
        public async Task EnrollFreeAsync_EnrollsUser_WhenCourseIsFreeAndNotEnrolled()
        {
            //Arrange 1
            int userId = 1;
            int courseId = 2;
            int instructorId = 3;
            var course = new Course 
            { 
                CourseId = courseId, 
                Price = 0, 
                CourseStatus = CourseStatus.Published.ToValue(), 
                InstructorId = instructorId,
                Title = "Test Free" 
            };
            var student = new User { UserId = userId, FullName = "Test User" };
            var transactionMock = Substitute.For<IDbContextTransaction>();

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(false);
            _repoMock.BeginTransactionAsync().Returns(transactionMock);
            _repoMock.SaveChangesAsync().Returns(1);
            _userRepoMock.GetUserByIdAsync(userId).Returns(student);

            //Act
            await _sut.EnrollFreeAsync(userId, courseId);

            //Assert
            await _courseRepoMock.Received(1).GetByIdAsync(courseId);
            await _courseRepoMock.Received(1).IsEnrolledAsync(userId, courseId);
            
            await _repoMock.Received(1).AddEnrollmentAsync(Arg.Is<Enrollment>(e => 
                e.UserId == userId && 
                e.CourseId == courseId && 
                e.Title == course.Title
            ));
            await _repoMock.Received(1).SaveChangesAsync();
            await transactionMock.Received(1).CommitAsync();
            
            await _notifMock.Received(1).SendNotificationAsync(
                instructorId, 
                "New student enrolled", 
                Arg.Any<string>(), 
                Arg.Any<string>()
            );
        }

        [Fact]
        public async Task EnrollFreeAsync_ThrowsException_WhenCourseIsPaidOrUserAlreadyEnrolled()
        {
            //Arrange 1
            int userId = 1;
            int courseId = 2;
            var course = new Course 
            { 
                CourseId = courseId, 
                Price = 10, // Paid
                CourseStatus = CourseStatus.Published.ToValue()
            };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);

            //Act
            Func<Task> act = async () => await _sut.EnrollFreeAsync(userId, courseId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This course is not free. Please complete the purchase via your cart.");
            
            await _courseRepoMock.Received(1).GetByIdAsync(courseId);
            await _repoMock.DidNotReceive().AddEnrollmentAsync(Arg.Any<Enrollment>());
        }

        [Fact]
        public async Task GetProgressAsync_ReturnsProgress_WhenEnrolled()
        {
            //Arrange 1
            int userId = 1;
            int courseId = 2;
            int enrollmentId = 10;
            var enrollment = new Enrollment { EnrollmentId = enrollmentId, UserId = userId, CourseId = courseId };
            var stats = new CourseStats { TotalMaterials = 5 };
            var completedIds = new List<int> { 1, 2 };

            //Arrange 2
            _repoMock.GetEnrollmentAsync(userId, courseId).Returns(enrollment);
            _courseRepoMock.GetCourseStatsAsync(courseId).Returns(stats);
            _repoMock.GetCompletedMaterialCountAsync(enrollmentId).Returns(2);
            _repoMock.GetCompletedMaterialIdsAsync(enrollmentId).Returns(completedIds);

            //Act
            var result = await _sut.GetProgressAsync(userId, courseId);

            //Assert
            result.Should().NotBeNull();
            result.EnrollmentId.Should().Be(enrollmentId);
            result.TotalMaterialCount.Should().Be(5);
            result.LearnedMaterialCount.Should().Be(2);
            result.CompletedMaterialIds.Should().BeEquivalentTo(completedIds);

            await _repoMock.Received(1).GetEnrollmentAsync(userId, courseId);
            await _courseRepoMock.Received(1).GetCourseStatsAsync(courseId);
            await _repoMock.Received(1).GetCompletedMaterialCountAsync(enrollmentId);
            await _repoMock.Received(1).GetCompletedMaterialIdsAsync(enrollmentId);
        }

        [Fact]
        public async Task GetMyEnrolledCoursesAsync_ReturnsEnrolledCourses_WhenUserEnrolled()
        {
            //Arrange 1
            int userId = 1;
            int courseId = 2;
            var course = new Course { CourseId = courseId, Title = "C# Course" };
            var enrollments = new List<Enrollment> 
            { 
                new Enrollment { EnrollmentId = 10, CourseId = courseId, Course = course, IsCompleted = false } 
            };
            var stats = new List<CourseStats> { new CourseStats { CourseId = courseId, TotalMaterials = 10 } };

            //Arrange 2
            _repoMock.GetMyEnrolledCoursesAsync(userId).Returns(enrollments);
            _courseRepoMock.GetCourseStatsAsync(Arg.Any<List<int>>()).Returns(stats);
            _repoMock.GetCompletedMaterialCountAsync(10).Returns(5); // 5/10 = 50%

            //Act
            var result = await _sut.GetMyEnrolledCoursesAsync(userId);

            //Assert
            result.Should().NotBeNull();
            var resultList = result.ToList();
            resultList.Should().HaveCount(1);
            resultList[0].CourseId.Should().Be(courseId);
            resultList[0].Title.Should().Be("C# Course");
            resultList[0].ProgressPercentage.Should().Be(50);
            resultList[0].IsCompleted.Should().BeFalse();

            await _repoMock.Received(1).GetMyEnrolledCoursesAsync(userId);
            await _courseRepoMock.Received(1).GetCourseStatsAsync(Arg.Is<List<int>>(x => x.Contains(courseId)));
            await _repoMock.Received(1).GetCompletedMaterialCountAsync(10);
        }
    }
}
