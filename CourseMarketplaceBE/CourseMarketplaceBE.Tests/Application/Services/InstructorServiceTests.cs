using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.IRepositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class InstructorServiceTests
    {
        private readonly IInstructorRepository _repoMock;
        private readonly IFileUploadService _uploadServiceMock;
        private readonly IAdminFinanceRepository _financeRepoMock;
        private readonly IUserRepository _userRepoMock;
        private readonly IStripeConnectService _stripeConnectMock;
        private readonly ICourseRepository _courseRepoMock;
        private readonly InstructorService _sut;

        public InstructorServiceTests()
        {
            _repoMock = Substitute.For<IInstructorRepository>();
            _uploadServiceMock = Substitute.For<IFileUploadService>();
            _financeRepoMock = Substitute.For<IAdminFinanceRepository>();
            _userRepoMock = Substitute.For<IUserRepository>();
            _stripeConnectMock = Substitute.For<IStripeConnectService>();
            _courseRepoMock = Substitute.For<ICourseRepository>();

            _sut = new InstructorService(
                _repoMock,
                _uploadServiceMock,
                _financeRepoMock,
                _userRepoMock,
                _stripeConnectMock,
                _courseRepoMock
            );
        }

        [Fact]
        public async Task GetPublicProfileAsync_ReturnsProfile_WhenInstructorExists()
        {
            //Arrange 1
            int instructorId = 2;
            var user = new User { UserId = instructorId, FullName = "John Doe", Bio = "Expert", UserNavigation = new Account { AvatarUrl = "img.png" } };
            
            var courses = new List<Course>
            {
                new Course 
                { 
                    CourseId = 1, 
                    CourseStatus = CourseStatus.Published.ToValue(), 
                    Title = "Course 1", 
                    Enrollments = new List<Enrollment> { new Enrollment(), new Enrollment() } 
                },
                new Course 
                { 
                    CourseId = 2, 
                    CourseStatus = CourseStatus.Draft.ToValue(), 
                    Title = "Course 2" 
                }
            };

            var instructor = new Instructor 
            { 
                InstructorId = instructorId, 
                ApprovalStatus = InstructorApprovalStatus.Approved.ToValue(),
                InstructorNavigation = user,
                Courses = courses
            };

            var stats = new InstructorStats { TotalStudentsCount = 100, InstructorRating = 4.8 };

            //Arrange 2
            _repoMock.GetByIdWithNavigationAsync(instructorId).Returns(instructor);
            _repoMock.GetStatsAsync(instructorId).Returns(stats);
            int activeCoursesCount = 1;
            _repoMock.CountActiveCoursesAsync(instructorId).Returns(activeCoursesCount);

            //Act
            var result = await _sut.GetPublicProfileAsync(instructorId);

            //Assert
            result.Should().NotBeNull();
            result.InstructorId.Should().Be(instructorId);
            result.FullName.Should().Be("John Doe");
            result.Bio.Should().Be("Expert");
            result.AvatarUrl.Should().Be("img.png");
            result.TotalStudents.Should().Be(100);
            result.AverageRating.Should().Be(4.8m);
            result.TotalCourses.Should().Be(1);
            
            result.Courses.Should().HaveCount(1);
            result.Courses.First().Title.Should().Be("Course 1");
            result.Courses.First().TotalStudents.Should().Be(2);

            await _repoMock.Received(1).GetByIdWithNavigationAsync(instructorId);
            await _repoMock.Received(1).GetStatsAsync(instructorId);
            await _repoMock.Received(1).CountActiveCoursesAsync(instructorId);
        }

        [Fact]
        public async Task GetPublicProfileAsync_ReturnsNull_WhenInstructorDoesNotExistOrNotApproved()
        {
            //Arrange 1
            int instructorId = 99;

            //Arrange 2
            _repoMock.GetByIdWithNavigationAsync(instructorId).Returns((Instructor?)null);

            //Act
            var result = await _sut.GetPublicProfileAsync(instructorId);

            //Assert
            result.Should().BeNull();

            await _repoMock.Received(1).GetByIdWithNavigationAsync(instructorId);
            await _repoMock.DidNotReceive().GetStatsAsync(Arg.Any<int>());
        }
    }
}
