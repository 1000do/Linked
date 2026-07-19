using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.IRepositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class CourseQueryServiceTests
    {
        private readonly ICourseRepository _courseRepoMock;
        private readonly IInstructorRepository _instructorRepoMock;
        private readonly IRedisService _redisServiceMock;
        private readonly ICourseAiIntegrationRepository _aiRepoMock;
        private readonly IMapper _mapperMock;
        private readonly ICartRepository _cartRepoMock;
        private readonly ILogger<CourseQueryService> _loggerMock;
        private readonly CourseQueryService _sut;

        public CourseQueryServiceTests()
        {
            _courseRepoMock = Substitute.For<ICourseRepository>();
            _instructorRepoMock = Substitute.For<IInstructorRepository>();
            _redisServiceMock = Substitute.For<IRedisService>();
            _aiRepoMock = Substitute.For<ICourseAiIntegrationRepository>();
            _mapperMock = Substitute.For<IMapper>();
            _cartRepoMock = Substitute.For<ICartRepository>();
            _loggerMock = Substitute.For<ILogger<CourseQueryService>>();

            _sut = new CourseQueryService(
                _courseRepoMock,
                _instructorRepoMock,
                _redisServiceMock,
                _aiRepoMock,
                _mapperMock,
                _cartRepoMock,
                _loggerMock
            );
        }

        [Fact]
        public async Task GetPublishedCoursesPagedAsync_ReturnsPagedCourses_WhenValidFiltersProvided()
        {
            //Arrange 1
            string query = "test";
            string category = "IT";
            string sort = "price_asc";
            string price = "free";
            string rating = "4";
            int page = 1;
            int pageSize = 10;
            int userId = 1;

            var courseEntity = new Course { CourseId = 100, InstructorId = 2 };
            var courses = new List<Course> { courseEntity };
            int totalCount = 1;
            
            var stats = new List<CourseStats> { new CourseStats { CourseId = 100, TotalStudents = 50, RatingAverage = 4.5, TotalReviews = 10 } };
            var mappedResponse = new List<CourseResponse> { new CourseResponse { CourseId = 100, InstructorId = 2 } };

            //Arrange 2
            _courseRepoMock.GetAllPublishedCoursesPagedAsync(query, category, sort, price, rating, page, pageSize)
                .Returns((courses, totalCount));
            _courseRepoMock.GetCourseStatsAsync(Arg.Any<List<int>>()).Returns(stats);
            _courseRepoMock.IsEnrolledAsync(userId, 100).Returns(true);
            _mapperMock.Map<List<CourseResponse>>(courses).Returns(mappedResponse);

            //Act
            var result = await _sut.GetPublishedCoursesPagedAsync(query, category, sort, price, rating, page, pageSize, userId);

            //Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Page.Should().Be(page);
            result.PageSize.Should().Be(pageSize);
            
            var item = result.Items.First();
            item.TotalStudents.Should().Be(50);
            item.RatingAverage.Should().Be(4.5m);
            item.TotalReviews.Should().Be(10);
            item.IsEnrolled.Should().BeTrue();
            item.IsOwner.Should().BeFalse();

            await _courseRepoMock.Received(1).GetAllPublishedCoursesPagedAsync(query, category, sort, price, rating, page, pageSize);
            await _courseRepoMock.Received(1).GetCourseStatsAsync(Arg.Is<List<int>>(x => x.Contains(100)));
            await _courseRepoMock.Received(1).IsEnrolledAsync(userId, 100);
            _mapperMock.Received(1).Map<List<CourseResponse>>(courses);
        }

        [Fact]
        public async Task GetCourseWithDetailsAsync_ReturnsCourseDetail_WhenCourseExists()
        {
            //Arrange 1
            int courseId = 100;
            int userId = 1;
            string userRole = "student";

            var courseEntity = new Course { CourseId = courseId, InstructorId = 2, CourseStatus = CourseStatus.Published.ToValue() };
            var stats = new CourseStats { CourseId = courseId, TotalStudents = 50, RatingAverage = 4.5, TotalReviews = 10 };
            var instructorStats = new InstructorStats { TotalStudentsCount = 200 };
            var mappedResponse = new CourseDetailResponse { CourseId = courseId, InstructorId = 2, CourseStatus = CourseStatus.Published.ToValue() };

            //Arrange 2
            _redisServiceMock.GetCacheAsync<CourseDetailResponse>(Arg.Any<string>()).Returns((CourseDetailResponse?)null);
            _courseRepoMock.GetCourseWithDetailsAsync(courseId).Returns(courseEntity);
            _courseRepoMock.GetCourseStatsAsync(courseId).Returns(stats);
            _instructorRepoMock.GetStatsAsync(2).Returns(instructorStats);
            _mapperMock.Map<CourseDetailResponse>(courseEntity).Returns(mappedResponse);
            _cartRepoMock.IsCourseInAnyCartAsync(courseId).Returns(true);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(false);

            //Act
            var result = await _sut.GetCourseWithDetailsAsync(courseId, userId, userRole);

            //Assert
            result.Should().NotBeNull();
            result.CourseId.Should().Be(courseId);
            result.InstructorStudentsCount.Should().Be(200);
            result.TotalStudents.Should().Be(50);
            result.TotalReviews.Should().Be(10);
            result.RatingAverage.Should().Be(4.5m);
            result.IsInAnyCart.Should().BeTrue();
            result.IsOwner.Should().BeFalse();
            result.IsEnrolled.Should().BeFalse();

            await _redisServiceMock.Received(2).GetCacheAsync<CourseDetailResponse>(Arg.Any<string>());
            await _courseRepoMock.Received(1).GetCourseWithDetailsAsync(courseId);
            await _courseRepoMock.Received(1).GetCourseStatsAsync(courseId);
            await _instructorRepoMock.Received(1).GetStatsAsync(2);
            _mapperMock.Received(1).Map<CourseDetailResponse>(courseEntity);
            await _redisServiceMock.Received(1).SetCacheAsync(Arg.Any<string>(), Arg.Any<CourseDetailResponse>(), Arg.Any<TimeSpan>());
            await _cartRepoMock.Received(1).IsCourseInAnyCartAsync(courseId);
            await _courseRepoMock.Received(1).IsEnrolledAsync(userId, courseId);
        }

        [Fact]
        public async Task GetCourseWithDetailsAsync_ThrowsNotFoundException_WhenCourseDoesNotExist()
        {
            //Arrange 1
            int courseId = 999;

            //Arrange 2
            _redisServiceMock.GetCacheAsync<CourseDetailResponse>(Arg.Any<string>()).Returns((CourseDetailResponse?)null);
            _courseRepoMock.GetCourseWithDetailsAsync(courseId).Returns((Course?)null);

            //Act
            Func<Task> act = async () => await _sut.GetCourseWithDetailsAsync(courseId);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Course not found.");

            await _redisServiceMock.Received(1).GetCacheAsync<CourseDetailResponse>(Arg.Any<string>());
            await _courseRepoMock.Received(1).GetCourseWithDetailsAsync(courseId);
            await _courseRepoMock.DidNotReceive().GetCourseStatsAsync(Arg.Any<int>());
            _mapperMock.DidNotReceive().Map<CourseDetailResponse>(Arg.Any<Course>());
        }

        [Fact]
        public async Task GetInstructorCoursesPagedAsync_ReturnsInstructorCourses_WhenInstructorExists()
        {
            //Arrange 1
            int instructorId = 2;
            string search = "ASP.NET";
            string status = "Published";
            int page = 1;
            int pageSize = 10;
            
            var courseEntity = new Course { CourseId = 100, InstructorId = instructorId };
            var courses = new List<Course> { courseEntity };
            int totalCount = 1;

            var stats = new List<CourseStats> { new CourseStats { CourseId = 100, TotalStudents = 50, RatingAverage = 4.5, TotalReviews = 10 } };
            var mappedResponse = new List<CourseResponse> { new CourseResponse { CourseId = 100, InstructorId = instructorId } };

            //Arrange 2
            _courseRepoMock.GetInstructorCoursesPagedAsync(instructorId, search, status, page, pageSize)
                .Returns((courses, totalCount));
            _courseRepoMock.GetCourseStatsAsync(Arg.Any<List<int>>()).Returns(stats);
            _mapperMock.Map<List<CourseResponse>>(courses).Returns(mappedResponse);

            //Act
            var result = await _sut.GetInstructorCoursesPagedAsync(instructorId, search, status, page, pageSize);

            //Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Items.First().TotalStudents.Should().Be(50);
            result.Items.First().RatingAverage.Should().Be(4.5m);

            await _courseRepoMock.Received(1).GetInstructorCoursesPagedAsync(instructorId, search, status, page, pageSize);
            await _courseRepoMock.Received(1).GetCourseStatsAsync(Arg.Is<List<int>>(x => x.Contains(100)));
            _mapperMock.Received(1).Map<List<CourseResponse>>(courses);
        }

        [Fact]
        public async Task GetAllPublishedCoursesAsync_WithUserId_ReturnsMappedCoursesAndChecksEnrollment()
        {
            //Arrange 1
            int userId = 1;
            var course = new Course { CourseId = 100, InstructorId = 2 };
            var courses = new List<Course> { course };
            var stats = new List<CourseStats> { new CourseStats { CourseId = 100, TotalStudents = 50, RatingAverage = 4.5, TotalReviews = 10 } };
            var mappedResponse = new List<CourseResponse> { new CourseResponse { CourseId = 100, InstructorId = 2 } };

            //Arrange 2
            _courseRepoMock.GetAllPublishedCoursesAsync().Returns(courses);
            _courseRepoMock.GetCourseStatsAsync(Arg.Any<List<int>>()).Returns(stats);
            _courseRepoMock.IsEnrolledAsync(userId, 100).Returns(true);
            _mapperMock.Map<List<CourseResponse>>(courses).Returns(mappedResponse);

            //Act
            var result = await _sut.GetAllPublishedCoursesAsync(userId);

            //Assert
            result.Should().NotBeNull();
            var item = result.First();
            item.TotalStudents.Should().Be(50);
            item.RatingAverage.Should().Be(4.5m);
            item.TotalReviews.Should().Be(10);
            item.IsEnrolled.Should().BeTrue();
            item.IsOwner.Should().BeFalse();

            await _courseRepoMock.Received(1).GetAllPublishedCoursesAsync();
            await _courseRepoMock.Received(1).GetCourseStatsAsync(Arg.Is<List<int>>(x => x.Contains(100)));
            await _courseRepoMock.Received(1).IsEnrolledAsync(userId, 100);
            _mapperMock.Received(1).Map<List<CourseResponse>>(courses);
        }

        [Fact]
        public async Task GetAllPublishedCoursesAsync_WithoutUserId_ReturnsMappedCoursesWithoutEnrollmentChecks()
        {
            //Arrange 1
            var course = new Course { CourseId = 100, InstructorId = 2 };
            var courses = new List<Course> { course };
            var stats = new List<CourseStats> { new CourseStats { CourseId = 100, TotalStudents = 50, RatingAverage = 4.5, TotalReviews = 10 } };
            var mappedResponse = new List<CourseResponse> { new CourseResponse { CourseId = 100, InstructorId = 2 } };

            //Arrange 2
            _courseRepoMock.GetAllPublishedCoursesAsync().Returns(courses);
            _courseRepoMock.GetCourseStatsAsync(Arg.Any<List<int>>()).Returns(stats);
            _mapperMock.Map<List<CourseResponse>>(courses).Returns(mappedResponse);

            //Act
            var result = await _sut.GetAllPublishedCoursesAsync(null);

            //Assert
            result.Should().NotBeNull();
            var item = result.First();
            item.IsEnrolled.Should().BeFalse();
            item.IsOwner.Should().BeFalse();

            await _courseRepoMock.Received(1).GetAllPublishedCoursesAsync();
            await _courseRepoMock.Received(1).GetCourseStatsAsync(Arg.Is<List<int>>(x => x.Contains(100)));
            await _courseRepoMock.DidNotReceive().IsEnrolledAsync(Arg.Any<int>(), Arg.Any<int>());
            _mapperMock.Received(1).Map<List<CourseResponse>>(courses);
        }

        [Fact]
        public async Task GetAllPublishedCoursesAsync_StatsNull_DefaultsToZero()
        {
            //Arrange 1
            var course = new Course { CourseId = 100, InstructorId = 2 };
            var courses = new List<Course> { course };
            var stats = new List<CourseStats>(); // No stats found
            var mappedResponse = new List<CourseResponse> { new CourseResponse { CourseId = 100, InstructorId = 2 } };

            //Arrange 2
            _courseRepoMock.GetAllPublishedCoursesAsync().Returns(courses);
            _courseRepoMock.GetCourseStatsAsync(Arg.Any<List<int>>()).Returns(stats);
            _mapperMock.Map<List<CourseResponse>>(courses).Returns(mappedResponse);

            //Act
            var result = await _sut.GetAllPublishedCoursesAsync(null);

            //Assert
            result.Should().NotBeNull();
            var item = result.First();
            item.TotalStudents.Should().Be(0);
            item.RatingAverage.Should().Be(0);
            item.TotalReviews.Should().Be(0);

            await _courseRepoMock.Received(1).GetAllPublishedCoursesAsync();
            await _courseRepoMock.Received(1).GetCourseStatsAsync(Arg.Is<List<int>>(x => x.Contains(100)));
            _mapperMock.Received(1).Map<List<CourseResponse>>(courses);
        }

        [Fact]
        public async Task GetPublishedCoursesPagedAsync_WithoutUserId_DoesNotCheckEnrollment()
        {
            //Arrange 1
            var courses = new List<Course> { new Course { CourseId = 100, InstructorId = 2 } };
            var stats = new List<CourseStats> { new CourseStats { CourseId = 100, TotalStudents = 50, RatingAverage = 4.5, TotalReviews = 10 } };
            var mappedResponse = new List<CourseResponse> { new CourseResponse { CourseId = 100, InstructorId = 2 } };

            //Arrange 2
            _courseRepoMock.GetAllPublishedCoursesPagedAsync(null, null, null, null, null, null, null)
                .Returns((courses, 1));
            _courseRepoMock.GetCourseStatsAsync(Arg.Any<List<int>>()).Returns(stats);
            _mapperMock.Map<List<CourseResponse>>(courses).Returns(mappedResponse);

            //Act
            var result = await _sut.GetPublishedCoursesPagedAsync();

            //Assert
            result.Should().NotBeNull();
            var item = result.Items.First();
            item.IsEnrolled.Should().BeFalse();
            item.IsOwner.Should().BeFalse();

            await _courseRepoMock.Received(1).GetAllPublishedCoursesPagedAsync(null, null, null, null, null, null, null);
            await _courseRepoMock.Received(1).GetCourseStatsAsync(Arg.Any<List<int>>());
            await _courseRepoMock.DidNotReceive().IsEnrolledAsync(Arg.Any<int>(), Arg.Any<int>());
        }

        [Fact]
        public async Task GetPublishedCoursesPagedAsync_NullPageAndPageSize_UsesDefaultPagination()
        {
            //Arrange 1
            var courses = new List<Course>();
            var mappedResponse = new List<CourseResponse>();

            //Arrange 2
            _courseRepoMock.GetAllPublishedCoursesPagedAsync(null, null, null, null, null, null, null)
                .Returns((courses, 0));
            _courseRepoMock.GetCourseStatsAsync(Arg.Any<List<int>>()).Returns(new List<CourseStats>());
            _mapperMock.Map<List<CourseResponse>>(courses).Returns(mappedResponse);

            //Act
            var result = await _sut.GetPublishedCoursesPagedAsync();

            //Assert
            result.Should().NotBeNull();
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(12);
            result.TotalCount.Should().Be(0);

            await _courseRepoMock.Received(1).GetAllPublishedCoursesPagedAsync(null, null, null, null, null, null, null);
            await _courseRepoMock.Received(1).GetCourseStatsAsync(Arg.Any<List<int>>());
        }

        [Fact]
        public async Task IsEnrolledAsync_ReturnsExpectedBooleanFromRepository()
        {
            //Arrange 1
            int userId = 1;
            int courseId = 100;

            //Arrange 2
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(true);

            //Act
            var result = await _sut.IsEnrolledAsync(userId, courseId);

            //Assert
            result.Should().BeTrue();
            await _courseRepoMock.Received(1).IsEnrolledAsync(userId, courseId);
        }

        [Fact]
        public async Task GetInstructorCoursesAsync_ReturnsMappedCoursesWithStats()
        {
            //Arrange 1
            int instructorId = 2;
            var course = new Course { CourseId = 100, InstructorId = instructorId };
            var courses = new List<Course> { course };
            var stats = new List<CourseStats> { new CourseStats { CourseId = 100, TotalStudents = 50, RatingAverage = 4.5 } };
            var mappedResponse = new List<CourseResponse> { new CourseResponse { CourseId = 100, InstructorId = instructorId } };

            //Arrange 2
            _courseRepoMock.GetInstructorCoursesAsync(instructorId).Returns(courses);
            _courseRepoMock.GetCourseStatsAsync(Arg.Any<List<int>>()).Returns(stats);
            _mapperMock.Map<List<CourseResponse>>(courses).Returns(mappedResponse);

            //Act
            var result = await _sut.GetInstructorCoursesAsync(instructorId);

            //Assert
            result.Should().NotBeNull();
            var item = result.First();
            item.TotalStudents.Should().Be(50);
            item.RatingAverage.Should().Be(4.5m);

            await _courseRepoMock.Received(1).GetInstructorCoursesAsync(instructorId);
            await _courseRepoMock.Received(1).GetCourseStatsAsync(Arg.Is<List<int>>(x => x.Contains(100)));
            _mapperMock.Received(1).Map<List<CourseResponse>>(courses);
        }

        [Fact]
        public async Task GetInstructorCoursesPagedAsync_NullPageAndPageSize_UsesDefaultPagination()
        {
            //Arrange 1
            int instructorId = 2;
            var courses = new List<Course>();
            var mappedResponse = new List<CourseResponse>();

            //Arrange 2
            _courseRepoMock.GetInstructorCoursesPagedAsync(instructorId, null, null, null, null)
                .Returns((courses, 0));
            _courseRepoMock.GetCourseStatsAsync(Arg.Any<List<int>>()).Returns(new List<CourseStats>());
            _mapperMock.Map<List<CourseResponse>>(courses).Returns(mappedResponse);

            //Act
            var result = await _sut.GetInstructorCoursesPagedAsync(instructorId);

            //Assert
            result.Should().NotBeNull();
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(6);

            await _courseRepoMock.Received(1).GetInstructorCoursesPagedAsync(instructorId, null, null, null, null);
        }

        [Fact]
        public async Task GetCourseWithDetailsAsync_CachedValueExists_ReturnsCachedValue()
        {
            //Arrange 1
            int courseId = 100;
            var cachedResponse = new CourseDetailResponse { CourseId = courseId, CourseStatus = CourseStatus.Published.ToValue() };

            //Arrange 2
            _redisServiceMock.GetCacheAsync<CourseDetailResponse>(Arg.Any<string>()).Returns(cachedResponse);
            _cartRepoMock.IsCourseInAnyCartAsync(courseId).Returns(false);

            //Act
            var result = await _sut.GetCourseWithDetailsAsync(courseId);

            //Assert
            result.Should().BeEquivalentTo(cachedResponse);
            
            await _redisServiceMock.Received(1).GetCacheAsync<CourseDetailResponse>(Arg.Any<string>());
            await _courseRepoMock.DidNotReceive().GetCourseWithDetailsAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task GetCourseWithDetailsAsync_CourseNotPublished_UserIsAdmin_ReturnsCourseDetail()
        {
            //Arrange 1
            int courseId = 100;
            int userId = 1;
            string userRole = "admin";

            var courseEntity = new Course { CourseId = courseId, CourseStatus = CourseStatus.Draft.ToValue() };
            var mappedResponse = new CourseDetailResponse { CourseId = courseId, CourseStatus = CourseStatus.Draft.ToValue(), InstructorId = 2 };

            //Arrange 2
            _redisServiceMock.GetCacheAsync<CourseDetailResponse>(Arg.Any<string>()).Returns((CourseDetailResponse?)null);
            _courseRepoMock.GetCourseWithDetailsAsync(courseId).Returns(courseEntity);
            _courseRepoMock.GetCourseStatsAsync(courseId).Returns(new CourseStats());
            _mapperMock.Map<CourseDetailResponse>(courseEntity).Returns(mappedResponse);
            _cartRepoMock.IsCourseInAnyCartAsync(courseId).Returns(false);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(false);

            //Act
            var result = await _sut.GetCourseWithDetailsAsync(courseId, userId, userRole);

            //Assert
            result.Should().NotBeNull();
            result.CourseId.Should().Be(courseId);
        }

        [Fact]
        public async Task GetCourseWithDetailsAsync_CourseNotPublished_UserIsStaff_ReturnsCourseDetail()
        {
            //Arrange 1
            int courseId = 100;
            int userId = 1;
            string userRole = "staff";

            var courseEntity = new Course { CourseId = courseId, CourseStatus = CourseStatus.Draft.ToValue() };
            var mappedResponse = new CourseDetailResponse { CourseId = courseId, CourseStatus = CourseStatus.Draft.ToValue(), InstructorId = 2 };

            //Arrange 2
            _redisServiceMock.GetCacheAsync<CourseDetailResponse>(Arg.Any<string>()).Returns((CourseDetailResponse?)null);
            _courseRepoMock.GetCourseWithDetailsAsync(courseId).Returns(courseEntity);
            _courseRepoMock.GetCourseStatsAsync(courseId).Returns(new CourseStats());
            _mapperMock.Map<CourseDetailResponse>(courseEntity).Returns(mappedResponse);
            _cartRepoMock.IsCourseInAnyCartAsync(courseId).Returns(false);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(false);

            //Act
            var result = await _sut.GetCourseWithDetailsAsync(courseId, userId, userRole);

            //Assert
            result.Should().NotBeNull();
            result.CourseId.Should().Be(courseId);
        }

        [Fact]
        public async Task GetCourseWithDetailsAsync_CourseNotPublished_UserIsOwner_ReturnsCourseDetail()
        {
            //Arrange 1
            int courseId = 100;
            int userId = 2; // user is owner
            string userRole = "instructor";

            var courseEntity = new Course { CourseId = courseId, CourseStatus = CourseStatus.Draft.ToValue(), InstructorId = userId };
            var mappedResponse = new CourseDetailResponse { CourseId = courseId, CourseStatus = CourseStatus.Draft.ToValue(), InstructorId = userId };

            //Arrange 2
            _redisServiceMock.GetCacheAsync<CourseDetailResponse>(Arg.Any<string>()).Returns((CourseDetailResponse?)null);
            _courseRepoMock.GetCourseWithDetailsAsync(courseId).Returns(courseEntity);
            _courseRepoMock.GetCourseStatsAsync(courseId).Returns(new CourseStats());
            _mapperMock.Map<CourseDetailResponse>(courseEntity).Returns(mappedResponse);
            _cartRepoMock.IsCourseInAnyCartAsync(courseId).Returns(false);

            //Act
            var result = await _sut.GetCourseWithDetailsAsync(courseId, userId, userRole);

            //Assert
            result.Should().NotBeNull();
            result.IsOwner.Should().BeTrue();
            result.IsEnrolled.Should().BeTrue();
        }

        [Fact]
        public async Task GetCourseWithDetailsAsync_CourseNotPublished_UserIsEnrolled_ReturnsCourseDetail()
        {
            //Arrange 1
            int courseId = 100;
            int userId = 1;
            string userRole = "student";

            var courseEntity = new Course { CourseId = courseId, CourseStatus = CourseStatus.Draft.ToValue() };
            var mappedResponse = new CourseDetailResponse { CourseId = courseId, CourseStatus = CourseStatus.Draft.ToValue(), InstructorId = 2 };

            //Arrange 2
            _redisServiceMock.GetCacheAsync<CourseDetailResponse>(Arg.Any<string>()).Returns((CourseDetailResponse?)null);
            _courseRepoMock.GetCourseWithDetailsAsync(courseId).Returns(courseEntity);
            _courseRepoMock.GetCourseStatsAsync(courseId).Returns(new CourseStats());
            _mapperMock.Map<CourseDetailResponse>(courseEntity).Returns(mappedResponse);
            _cartRepoMock.IsCourseInAnyCartAsync(courseId).Returns(false);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(true);

            //Act
            var result = await _sut.GetCourseWithDetailsAsync(courseId, userId, userRole);

            //Assert
            result.Should().NotBeNull();
            result.IsEnrolled.Should().BeTrue();
        }

        [Fact]
        public async Task GetCourseWithDetailsAsync_CourseNotPublished_UserNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            //Arrange 1
            int courseId = 100;
            int userId = 1;
            string userRole = "student";

            var courseEntity = new Course { CourseId = courseId, CourseStatus = CourseStatus.Draft.ToValue() };
            var mappedResponse = new CourseDetailResponse { CourseId = courseId, CourseStatus = CourseStatus.Draft.ToValue(), InstructorId = 2 };

            //Arrange 2
            _redisServiceMock.GetCacheAsync<CourseDetailResponse>(Arg.Any<string>()).Returns((CourseDetailResponse?)null);
            _courseRepoMock.GetCourseWithDetailsAsync(courseId).Returns(courseEntity);
            _courseRepoMock.GetCourseStatsAsync(courseId).Returns(new CourseStats());
            _mapperMock.Map<CourseDetailResponse>(courseEntity).Returns(mappedResponse);
            _cartRepoMock.IsCourseInAnyCartAsync(courseId).Returns(false);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(false);

            //Act
            Func<Task> act = async () => await _sut.GetCourseWithDetailsAsync(courseId, userId, userRole);

            //Assert
            var ex = await act.Should().ThrowAsync<UnauthorizedAccessException>();
            ex.WithMessage("You do not have permission to view this course.");
        }

        [Fact]
        public async Task GetCategoriesAsync_ReturnsMappedCategories()
        {
            //Arrange 1
            var categories = new List<Category> { new Category { CategoryId = 1, CategoriesName = "Tech" } };
            var mappedResponse = new List<CategoryResponse> { new CategoryResponse { CategoryId = 1, CategoriesName = "Tech" } };

            //Arrange 2
            _courseRepoMock.GetCategoriesAsync().Returns(categories);
            _mapperMock.Map<IEnumerable<CategoryResponse>>(categories).Returns(mappedResponse);

            //Act
            var result = await _sut.GetCategoriesAsync();

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().CategoriesName.Should().Be("Tech");

            await _courseRepoMock.Received(1).GetCategoriesAsync();
            _mapperMock.Received(1).Map<IEnumerable<CategoryResponse>>(categories);
        }

        [Fact]
        public async Task GetByModelAndCourseAsync_IntegrationNotFound_ReturnsNull()
        {
            //Arrange 1
            int modelId = 1;
            int courseId = 100;

            //Arrange 2
            _aiRepoMock.GetByModelAndCourseAsync(modelId, courseId).Returns((CourseAiIntegration?)null);

            //Act
            var result = await _sut.GetByModelAndCourseAsync(modelId, courseId);

            //Assert
            result.Should().BeNull();
            await _aiRepoMock.Received(1).GetByModelAndCourseAsync(modelId, courseId);
        }

        [Fact]
        public async Task GetByModelAndCourseAsync_IntegrationFound_WithEmptyConfigJson_ReturnsEmptyConfig()
        {
            //Arrange 1
            int modelId = 1;
            int courseId = 100;
            var integration = new CourseAiIntegration
            {
                CourseId = courseId,
                ModelId = modelId,
                IsEnabled = true,
                ConfigJson = null,
                Role = "Tutor"
            };

            //Arrange 2
            _aiRepoMock.GetByModelAndCourseAsync(modelId, courseId).Returns(integration);

            //Act
            var result = await _sut.GetByModelAndCourseAsync(modelId, courseId);

            //Assert
            result.Should().NotBeNull();
            result.CourseId.Should().Be(courseId);
            result.ConfigJson.Should().NotBeNull();
            result.ConfigJson.Should().BeEmpty();

            await _aiRepoMock.Received(1).GetByModelAndCourseAsync(modelId, courseId);
        }

        [Fact]
        public async Task GetByModelAndCourseAsync_IntegrationFound_WithValidConfigJson_ReturnsParsedConfig()
        {
            //Arrange 1
            int modelId = 1;
            int courseId = 100;
            var integration = new CourseAiIntegration
            {
                CourseId = courseId,
                ModelId = modelId,
                IsEnabled = true,
                ConfigJson = "{\"temperature\": 0.7}",
                Role = "Tutor"
            };

            //Arrange 2
            _aiRepoMock.GetByModelAndCourseAsync(modelId, courseId).Returns(integration);

            //Act
            var result = await _sut.GetByModelAndCourseAsync(modelId, courseId);

            //Assert
            result.Should().NotBeNull();
            result.ConfigJson.Should().ContainKey("temperature");
            result.ConfigJson["temperature"].Should().Be(0.7f);

            await _aiRepoMock.Received(1).GetByModelAndCourseAsync(modelId, courseId);
        }
    }
}
