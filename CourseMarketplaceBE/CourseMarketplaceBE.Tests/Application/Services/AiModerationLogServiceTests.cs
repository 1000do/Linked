using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class AiModerationLogServiceTests
    {
        private readonly ICourseAiUsageLogRepository _courseLogRepoMock;
        private readonly ICourseReviewModerationLogRepository _courseReviewLogRepoMock;
        private readonly ILessonReviewModerationLogRepository _lessonReviewLogRepoMock;
        private readonly IMapper _mapperMock;
        private readonly AiModerationLogService _sut;

        public AiModerationLogServiceTests()
        {
            _courseLogRepoMock = Substitute.For<ICourseAiUsageLogRepository>();
            _courseReviewLogRepoMock = Substitute.For<ICourseReviewModerationLogRepository>();
            _lessonReviewLogRepoMock = Substitute.For<ILessonReviewModerationLogRepository>();
            _mapperMock = Substitute.For<IMapper>();

            _sut = new AiModerationLogService(
                _courseLogRepoMock,
                _courseReviewLogRepoMock,
                _lessonReviewLogRepoMock,
                _mapperMock
            );
        }

        // ── GetCourseModerationLogsAsync ─────────────────────────────────────────

        [Fact]
        public async Task GetCourseModerationLogsAsync_ShouldReturnMappedLogsAndTotalCount()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var req = new PagedRequestDto { Page = page, PageSize = pageSize };
            var logs = new List<CourseAiUsageLog> { new CourseAiUsageLog { LogId = 1 } };
            int totalCount = 5;
            var dtos = new List<CourseModerationLogAdminDto> { new CourseModerationLogAdminDto { LogId = 1 } };

            //Arrange 2
            _courseLogRepoMock.GetPagedAdminAsync(page, pageSize).Returns((logs, totalCount));
            _mapperMock.Map<List<CourseModerationLogAdminDto>>(logs).Returns(dtos);

            //Act
            var result = await _sut.GetCourseModerationLogsAsync(req);

            //Assert
            result.Items.Should().BeEquivalentTo(dtos);
            result.TotalCount.Should().Be(totalCount);

            await _courseLogRepoMock.Received(1).GetPagedAdminAsync(page, pageSize);
            _mapperMock.Received(1).Map<List<CourseModerationLogAdminDto>>(logs);
        }

        [Fact]
        public async Task GetCourseModerationLogsAsync_WhenEmpty_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var req = new PagedRequestDto { Page = page, PageSize = pageSize };
            var emptyLogs = new List<CourseAiUsageLog>();

            //Arrange 2
            _courseLogRepoMock.GetPagedAdminAsync(page, pageSize).Returns((emptyLogs, 0));
            
            //Act
            Func<Task> act = async () => await _sut.GetCourseModerationLogsAsync(req);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("No course moderation logs found.");

            await _courseLogRepoMock.Received(1).GetPagedAdminAsync(page, pageSize);
            _mapperMock.DidNotReceive().Map<List<CourseModerationLogAdminDto>>(Arg.Any<List<CourseAiUsageLog>>());
        }

        // ── GetCourseModerationLogDetailAsync ────────────────────────────────────

        [Fact]
        public async Task GetCourseModerationLogDetailAsync_WhenExists_ShouldReturnMappedLog()
        {
            //Arrange 1
            int mockId = 1;
            var log = new CourseAiUsageLog { LogId = mockId };
            var dto = new CourseModerationLogAdminDto { LogId = mockId };

            //Arrange 2
            _courseLogRepoMock.GetAdminDetailByIdAsync(mockId).Returns(log);
            _mapperMock.Map<CourseModerationLogAdminDto>(log).Returns(dto);

            //Act
            var result = await _sut.GetCourseModerationLogDetailAsync(mockId);

            //Assert
            result.Should().BeEquivalentTo(dto);

            await _courseLogRepoMock.Received(1).GetAdminDetailByIdAsync(mockId);
            _mapperMock.Received(1).Map<CourseModerationLogAdminDto>(log);
        }

        [Fact]
        public async Task GetCourseModerationLogDetailAsync_WhenNotExists_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            int mockId = 1;

            //Arrange 2
            _courseLogRepoMock.GetAdminDetailByIdAsync(mockId).Returns((CourseAiUsageLog?)null);

            //Act
            Func<Task> act = async () => await _sut.GetCourseModerationLogDetailAsync(mockId);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Log not found.");

            await _courseLogRepoMock.Received(1).GetAdminDetailByIdAsync(mockId);
            _mapperMock.DidNotReceive().Map<CourseModerationLogAdminDto>(Arg.Any<CourseAiUsageLog>());
        }

        // ── GetCourseReviewModerationLogsAsync ───────────────────────────────────

        [Fact]
        public async Task GetCourseReviewModerationLogsAsync_ShouldReturnMappedLogsAndTotalCount()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var req = new PagedRequestDto { Page = page, PageSize = pageSize };
            var logs = new List<CourseReviewModerationLog> { new CourseReviewModerationLog { LogId = 1 } };
            int totalCount = 5;
            var dtos = new List<ReviewModerationLogAdminDto> { new ReviewModerationLogAdminDto { LogId = 1 } };

            //Arrange 2
            _courseReviewLogRepoMock.GetPagedAdminAsync(page, pageSize).Returns((logs, totalCount));
            _mapperMock.Map<List<ReviewModerationLogAdminDto>>(logs).Returns(dtos);

            //Act
            var result = await _sut.GetCourseReviewModerationLogsAsync(req);

            //Assert
            result.Items.Should().BeEquivalentTo(dtos);
            result.TotalCount.Should().Be(totalCount);

            await _courseReviewLogRepoMock.Received(1).GetPagedAdminAsync(page, pageSize);
            _mapperMock.Received(1).Map<List<ReviewModerationLogAdminDto>>(logs);
        }

        [Fact]
        public async Task GetCourseReviewModerationLogsAsync_WhenEmpty_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var req = new PagedRequestDto { Page = page, PageSize = pageSize };
            var emptyLogs = new List<CourseReviewModerationLog>();

            //Arrange 2
            _courseReviewLogRepoMock.GetPagedAdminAsync(page, pageSize).Returns((emptyLogs, 0));
            
            //Act
            Func<Task> act = async () => await _sut.GetCourseReviewModerationLogsAsync(req);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("No course review moderation logs found.");

            await _courseReviewLogRepoMock.Received(1).GetPagedAdminAsync(page, pageSize);
            _mapperMock.DidNotReceive().Map<List<ReviewModerationLogAdminDto>>(Arg.Any<List<CourseReviewModerationLog>>());
        }

        // ── GetCourseReviewModerationLogDetailAsync ──────────────────────────────

        [Fact]
        public async Task GetCourseReviewModerationLogDetailAsync_WhenExists_ShouldReturnMappedLog()
        {
            //Arrange 1
            int mockId = 1;
            var log = new CourseReviewModerationLog { LogId = mockId };
            var dto = new ReviewModerationLogAdminDto { LogId = mockId };

            //Arrange 2
            _courseReviewLogRepoMock.GetAdminDetailByIdAsync(mockId).Returns(log);
            _mapperMock.Map<ReviewModerationLogAdminDto>(log).Returns(dto);

            //Act
            var result = await _sut.GetCourseReviewModerationLogDetailAsync(mockId);

            //Assert
            result.Should().BeEquivalentTo(dto);

            await _courseReviewLogRepoMock.Received(1).GetAdminDetailByIdAsync(mockId);
            _mapperMock.Received(1).Map<ReviewModerationLogAdminDto>(log);
        }

        [Fact]
        public async Task GetCourseReviewModerationLogDetailAsync_WhenNotExists_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            int mockId = 1;

            //Arrange 2
            _courseReviewLogRepoMock.GetAdminDetailByIdAsync(mockId).Returns((CourseReviewModerationLog?)null);

            //Act
            Func<Task> act = async () => await _sut.GetCourseReviewModerationLogDetailAsync(mockId);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Log not found.");

            await _courseReviewLogRepoMock.Received(1).GetAdminDetailByIdAsync(mockId);
            _mapperMock.DidNotReceive().Map<ReviewModerationLogAdminDto>(Arg.Any<CourseReviewModerationLog>());
        }

        // ── GetLessonReviewModerationLogsAsync ───────────────────────────────────

        [Fact]
        public async Task GetLessonReviewModerationLogsAsync_ShouldReturnMappedLogsAndTotalCount()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var req = new PagedRequestDto { Page = page, PageSize = pageSize };
            var logs = new List<LessonReviewModerationLog> { new LessonReviewModerationLog { LogId = 1 } };
            int totalCount = 5;
            var dtos = new List<ReviewModerationLogAdminDto> { new ReviewModerationLogAdminDto { LogId = 1 } };

            //Arrange 2
            _lessonReviewLogRepoMock.GetPagedAdminAsync(page, pageSize).Returns((logs, totalCount));
            _mapperMock.Map<List<ReviewModerationLogAdminDto>>(logs).Returns(dtos);

            //Act
            var result = await _sut.GetLessonReviewModerationLogsAsync(req);

            //Assert
            result.Items.Should().BeEquivalentTo(dtos);
            result.TotalCount.Should().Be(totalCount);

            await _lessonReviewLogRepoMock.Received(1).GetPagedAdminAsync(page, pageSize);
            _mapperMock.Received(1).Map<List<ReviewModerationLogAdminDto>>(logs);
        }

        [Fact]
        public async Task GetLessonReviewModerationLogsAsync_WhenEmpty_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var req = new PagedRequestDto { Page = page, PageSize = pageSize };
            var emptyLogs = new List<LessonReviewModerationLog>();

            //Arrange 2
            _lessonReviewLogRepoMock.GetPagedAdminAsync(page, pageSize).Returns((emptyLogs, 0));
            
            //Act
            Func<Task> act = async () => await _sut.GetLessonReviewModerationLogsAsync(req);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("No lesson review moderation logs found.");

            await _lessonReviewLogRepoMock.Received(1).GetPagedAdminAsync(page, pageSize);
            _mapperMock.DidNotReceive().Map<List<ReviewModerationLogAdminDto>>(Arg.Any<List<LessonReviewModerationLog>>());
        }

        // ── GetLessonReviewModerationLogDetailAsync ──────────────────────────────

        [Fact]
        public async Task GetLessonReviewModerationLogDetailAsync_WhenExists_ShouldReturnMappedLog()
        {
            //Arrange 1
            int mockId = 1;
            var log = new LessonReviewModerationLog { LogId = mockId };
            var dto = new ReviewModerationLogAdminDto { LogId = mockId };

            //Arrange 2
            _lessonReviewLogRepoMock.GetAdminDetailByIdAsync(mockId).Returns(log);
            _mapperMock.Map<ReviewModerationLogAdminDto>(log).Returns(dto);

            //Act
            var result = await _sut.GetLessonReviewModerationLogDetailAsync(mockId);

            //Assert
            result.Should().BeEquivalentTo(dto);

            await _lessonReviewLogRepoMock.Received(1).GetAdminDetailByIdAsync(mockId);
            _mapperMock.Received(1).Map<ReviewModerationLogAdminDto>(log);
        }

        [Fact]
        public async Task GetLessonReviewModerationLogDetailAsync_WhenNotExists_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            int mockId = 1;

            //Arrange 2
            _lessonReviewLogRepoMock.GetAdminDetailByIdAsync(mockId).Returns((LessonReviewModerationLog?)null);

            //Act
            Func<Task> act = async () => await _sut.GetLessonReviewModerationLogDetailAsync(mockId);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Log not found.");

            await _lessonReviewLogRepoMock.Received(1).GetAdminDetailByIdAsync(mockId);
            _mapperMock.DidNotReceive().Map<ReviewModerationLogAdminDto>(Arg.Any<LessonReviewModerationLog>());
        }
    }
}
