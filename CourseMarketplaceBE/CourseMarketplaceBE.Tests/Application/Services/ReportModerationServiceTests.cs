using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Application.IServices;
using FluentAssertions;
using NSubstitute;
using Xunit;
using AutoMapper;

namespace CourseMarketplaceBE.Tests.Application.Services;

public class ReportModerationServiceTests
{
    private readonly IReportRepository _reportRepoMock;
    private readonly ICourseRepository _courseRepoMock;
    private readonly IReviewRepository _reviewRepoMock;
    private readonly INotificationService _notificationServiceMock;
    private readonly IUserRepository _userRepoMock;
    private readonly IRedisService _redisServiceMock;
    private readonly IMapper _mapperMock;
    private readonly IModerationPenaltyService _penaltyServiceMock;
    private readonly ReportModerationService _sut;

    public ReportModerationServiceTests()
    {
        _reportRepoMock = Substitute.For<IReportRepository>();
        _courseRepoMock = Substitute.For<ICourseRepository>();
        _reviewRepoMock = Substitute.For<IReviewRepository>();
        _notificationServiceMock = Substitute.For<INotificationService>();
        _userRepoMock = Substitute.For<IUserRepository>();
        _redisServiceMock = Substitute.For<IRedisService>();
        _mapperMock = Substitute.For<IMapper>();
        _penaltyServiceMock = Substitute.For<IModerationPenaltyService>();

        _sut = new ReportModerationService(
            _reportRepoMock,
            _courseRepoMock,
            _reviewRepoMock,
            _notificationServiceMock,
            _userRepoMock,
            _redisServiceMock,
            _mapperMock,
            _penaltyServiceMock
        );
    }

    // ── GetAllCourseReportsAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetAllCourseReportsAsync_ShouldReturnPagedReports()
    {
        //Arrange 1
        var reports = new List<CourseReport> { new CourseReport { CourseReportId = 1 } };
        int totalCount = 10;
        var dtos = new List<CourseReportDetailResponse> { new CourseReportDetailResponse { ReportId = 1 } };

        //Arrange 2
        _reportRepoMock.GetAllCourseReportsAsync("pending", 1, 10).Returns((reports, totalCount));
        _mapperMock.Map<IEnumerable<CourseReportDetailResponse>>(reports).Returns(dtos);

        //Act
        var req = new PagedReportRequestDto { Status = "pending", Page = 1, PageSize = 10 };
        var result = await _sut.GetAllCourseReportsAsync(req);

        //Assert
        result.Items.Should().BeEquivalentTo(dtos);
        result.TotalCount.Should().Be(totalCount);

        await _reportRepoMock.Received(1).GetAllCourseReportsAsync("pending", 1, 10);
        _mapperMock.Received(1).Map<IEnumerable<CourseReportDetailResponse>>(reports);
    }

    [Fact]
    public async Task GetAllCourseReportsAsync_WhenEmpty_ShouldThrowKeyNotFoundException()
    {
        //Arrange 1
        var empty = new List<CourseReport>();

        //Arrange 2
        _reportRepoMock.GetAllCourseReportsAsync("pending", 1, 10).Returns((empty, 0));

        //Act
        var req = new PagedReportRequestDto { Status = "pending", Page = 1, PageSize = 10 };
        Func<Task> act = async () => await _sut.GetAllCourseReportsAsync(req);

        //Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("No reports found.");

        await _reportRepoMock.Received(1).GetAllCourseReportsAsync("pending", 1, 10);
    }

    // ── GetAllCourseReviewReportsAsync ───────────────────────────────────────

    [Fact]
    public async Task GetAllCourseReviewReportsAsync_ShouldReturnPagedReports()
    {
        //Arrange 1
        var reports = new List<CourseReviewReport> { new CourseReviewReport { CourseReviewReportId = 1 } };
        int totalCount = 10;
        var dtos = new List<ReviewReportDetailResponse> { new ReviewReportDetailResponse { ReportId = 1 } };

        //Arrange 2
        _reportRepoMock.GetAllCourseReviewReportsAsync(null, 1, 10).Returns((reports, totalCount));
        _mapperMock.Map<IEnumerable<ReviewReportDetailResponse>>(reports).Returns(dtos);

        //Act
        var req = new PagedReportRequestDto { Status = null, Page = 1, PageSize = 10 };
        var result = await _sut.GetAllCourseReviewReportsAsync(req);

        //Assert
        result.Items.Should().BeEquivalentTo(dtos);
        result.TotalCount.Should().Be(totalCount);

        await _reportRepoMock.Received(1).GetAllCourseReviewReportsAsync(null, 1, 10);
    }

    [Fact]
    public async Task GetAllCourseReviewReportsAsync_WhenEmpty_ShouldThrowKeyNotFoundException()
    {
        //Arrange 1
        var empty = new List<CourseReviewReport>();

        //Arrange 2
        _reportRepoMock.GetAllCourseReviewReportsAsync(null, 1, 10).Returns((empty, 0));

        //Act
        var req = new PagedReportRequestDto { Status = null, Page = 1, PageSize = 10 };
        Func<Task> act = async () => await _sut.GetAllCourseReviewReportsAsync(req);

        //Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("No course review reports found.");

        await _reportRepoMock.Received(1).GetAllCourseReviewReportsAsync(null, 1, 10);
    }

    // ── GetAllLessonReviewReportsAsync ───────────────────────────────────────

    [Fact]
    public async Task GetAllLessonReviewReportsAsync_ShouldReturnPagedReports()
    {
        //Arrange 1
        var reports = new List<LessonReviewReport> { new LessonReviewReport { LessonReviewReportId = 1 } };
        int totalCount = 10;
        var dtos = new List<ReviewReportDetailResponse> { new ReviewReportDetailResponse { ReportId = 1 } };

        //Arrange 2
        _reportRepoMock.GetAllLessonReviewReportsAsync(null, 1, 10).Returns((reports, totalCount));
        _mapperMock.Map<IEnumerable<ReviewReportDetailResponse>>(reports).Returns(dtos);

        //Act
        var req = new PagedReportRequestDto { Status = null, Page = 1, PageSize = 10 };
        var result = await _sut.GetAllLessonReviewReportsAsync(req);

        //Assert
        result.Items.Should().BeEquivalentTo(dtos);
        result.TotalCount.Should().Be(totalCount);

        await _reportRepoMock.Received(1).GetAllLessonReviewReportsAsync(null, 1, 10);
    }

    [Fact]
    public async Task GetAllLessonReviewReportsAsync_WhenEmpty_ShouldThrowKeyNotFoundException()
    {
        //Arrange 1
        var empty = new List<LessonReviewReport>();

        //Arrange 2
        _reportRepoMock.GetAllLessonReviewReportsAsync(null, 1, 10).Returns((empty, 0));

        //Act
        var req = new PagedReportRequestDto { Status = null, Page = 1, PageSize = 10 };
        Func<Task> act = async () => await _sut.GetAllLessonReviewReportsAsync(req);

        //Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("No lesson review reports found.");

        await _reportRepoMock.Received(1).GetAllLessonReviewReportsAsync(null, 1, 10);
    }

    // ── GetReportStatsAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetReportStatsAsync_ShouldReturnAccurateStats()
    {
        //Arrange 1
        var today = DateTime.Today;

        //Arrange 2
        _reportRepoMock.CountCourseReportsByStatusAsync(ReportStatus.Pending.ToValue(), null).Returns(2);
        _reportRepoMock.CountCourseReviewReportsByStatusAsync(ReportStatus.Pending.ToValue(), null).Returns(1);
        _reportRepoMock.CountLessonReviewReportsByStatusAsync(ReportStatus.Pending.ToValue(), null).Returns(3);

        _reportRepoMock.CountCourseReportsByStatusAsync(ReportStatus.Resolved.ToValue(), today).Returns(1);
        _reportRepoMock.CountCourseReviewReportsByStatusAsync(ReportStatus.Resolved.ToValue(), today).Returns(1);
        _reportRepoMock.CountLessonReviewReportsByStatusAsync(ReportStatus.Resolved.ToValue(), today).Returns(0);

        _reportRepoMock.CountCourseReportsByStatusAsync(ReportStatus.Rejected.ToValue(), today).Returns(1);
        _reportRepoMock.CountCourseReviewReportsByStatusAsync(ReportStatus.Rejected.ToValue(), today).Returns(0);
        _reportRepoMock.CountLessonReviewReportsByStatusAsync(ReportStatus.Rejected.ToValue(), today).Returns(1);

        //Act
        var result = await _sut.GetReportStatsAsync();

        //Assert
        result.TotalPendingCourseReports.Should().Be(2);
        result.TotalPendingCourseReviewReports.Should().Be(1);
        result.TotalPendingLessonReviewReports.Should().Be(3);
        result.TotalResolvedToday.Should().Be(2); // 1 course + 1 course review
        result.TotalRejectedToday.Should().Be(2); // 1 course + 1 lesson review

        await _reportRepoMock.Received(1).CountCourseReportsByStatusAsync(ReportStatus.Pending.ToValue(), null);
        await _reportRepoMock.Received(1).CountCourseReviewReportsByStatusAsync(ReportStatus.Pending.ToValue(), null);
        await _reportRepoMock.Received(1).CountLessonReviewReportsByStatusAsync(ReportStatus.Pending.ToValue(), null);

        await _reportRepoMock.Received(1).CountCourseReportsByStatusAsync(ReportStatus.Resolved.ToValue(), today);
        await _reportRepoMock.Received(1).CountCourseReviewReportsByStatusAsync(ReportStatus.Resolved.ToValue(), today);
        await _reportRepoMock.Received(1).CountLessonReviewReportsByStatusAsync(ReportStatus.Resolved.ToValue(), today);

        await _reportRepoMock.Received(1).CountCourseReportsByStatusAsync(ReportStatus.Rejected.ToValue(), today);
        await _reportRepoMock.Received(1).CountCourseReviewReportsByStatusAsync(ReportStatus.Rejected.ToValue(), today);
        await _reportRepoMock.Received(1).CountLessonReviewReportsByStatusAsync(ReportStatus.Rejected.ToValue(), today);
    }

    // ── ResolveCourseReportAsync ─────────────────────────────────────────────

    [Fact]
    public async Task ResolveCourseReportAsync_ValidRequest_ShouldReturnTrueAndNotify()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Noted", RemoveContent = true };
        var report = new CourseReport { CourseReportId = 1, CourseId = 10, CourseReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var course = new Course { CourseId = 10, Title = "Test" };

        //Arrange 2
        _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
        _courseRepoMock.GetByIdAsync(10).Returns(course);
        _userRepoMock.GetRoleByAccountIdAsync(resolverId).Returns("staff");
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        report.CourseReportsStatus.Should().Be(req.Status);

        await _penaltyServiceMock.Received(1).ProcessCourseStrikeAsync(10, "Noted");
        _reportRepoMock.Received(1).UpdateCourseReport(report);
        await _reportRepoMock.Received(1).SaveChangesAsync();
        await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(course.CourseId));
        await _notificationServiceMock.Received(1).SendNotificationAsync(report.ReporterId.Value, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveCourseReportAsync_UnauthorizedRole_ShouldThrowUnauthorizedAccessException()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Noted", RemoveContent = true };
        var report = new CourseReport { CourseReportId = 1, CourseId = 10, CourseReportsStatus = ReportStatus.Pending.ToValue() };
        var course = new Course { CourseId = 10 };

        //Arrange 2
        _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
        _courseRepoMock.GetByIdAsync(10).Returns(course);
        _userRepoMock.GetRoleByAccountIdAsync(resolverId).Returns("user");

        //Act
        Func<Task> act = async () => await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

        //Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Only staff or admin can resolve reports.");
    }

    [Fact]
    public async Task ResolveCourseReportAsync_ReportMissingCourseId_ShouldThrowBadRequestException()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Noted", RemoveContent = true };
        var report = new CourseReport { CourseReportId = 1, CourseId = null };

        //Arrange 2
        _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);

        //Act
        Func<Task> act = async () => await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Course ID is missing from the report.");
    }

    [Fact]
    public async Task ResolveCourseReportAsync_SaveThrowsReportException_ShouldThrowBadRequestException()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Rejected.ToValue(), ResolutionNote = "Noted", RemoveContent = false };
        var report = new CourseReport { CourseReportId = 1, CourseId = 10, CourseReportsStatus = ReportStatus.Pending.ToValue() };
        var course = new Course { CourseId = 10 };

        //Arrange 2
        _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
        _courseRepoMock.GetByIdAsync(10).Returns(course);
        _userRepoMock.GetRoleByAccountIdAsync(resolverId).Returns("staff");
        _reportRepoMock.When(x => x.SaveChangesAsync()).Throw(new ReportException("DB Error"));

        //Act
        Func<Task> act = async () => await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("DB Error");
    }

    // ── ResolveCourseReviewReportAsync ───────────────────────────────────────

    [Fact]
    public async Task ResolveCourseReviewReportAsync_ValidRequest_ShouldReturnTrueAndNotify()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Noted", RemoveContent = true };
        var report = new CourseReviewReport { CourseReviewReportId = 1, CourseReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new CourseReview { CourseReviewId = 10, Enrollment = new Enrollment { UserId = 4, CourseId = 5 } };

        //Arrange 2
        _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetCourseReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetRoleByAccountIdAsync(resolverId).Returns("staff");
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        report.UserReportsStatus.Should().Be(req.Status);
        review.IsRemoved.Should().BeTrue();

        await _penaltyServiceMock.Received(1).ProcessReviewStrikeAsync(4, "Noted", Arg.Any<string>());
        _reportRepoMock.Received(1).UpdateCourseReviewReport(report);
        await _reportRepoMock.Received(1).SaveChangesAsync();
        await _notificationServiceMock.Received(1).SendNotificationAsync(report.ReporterId.Value, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveCourseReviewReportAsync_WarnOnly_ShouldNotifyOwner()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Noted", RemoveContent = false };
        var report = new CourseReviewReport { CourseReviewReportId = 1, CourseReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new CourseReview { CourseReviewId = 10, Enrollment = new Enrollment { UserId = 4, CourseId = 5 } };

        //Arrange 2
        _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetCourseReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetRoleByAccountIdAsync(resolverId).Returns("staff");
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        
        await _penaltyServiceMock.DidNotReceive().ProcessReviewStrikeAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>());
        await _notificationServiceMock.Received(1).SendNotificationAsync(4, "Review Policy Warning", Arg.Any<string>(), Arg.Any<string>());
        await _reportRepoMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task ResolveCourseReviewReportAsync_ReportMissingReviewId_ShouldThrowBadRequestException()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue() };
        var report = new CourseReviewReport { CourseReviewReportId = 1, CourseReviewId = null };

        //Arrange 2
        _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns(report);

        //Act
        Func<Task> act = async () => await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Course Review ID is missing from the report.");
    }

    // ── ResolveLessonReviewReportAsync ───────────────────────────────────────

    [Fact]
    public async Task ResolveLessonReviewReportAsync_ValidRequest_ShouldReturnTrueAndNotify()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Noted", RemoveContent = true };
        var report = new LessonReviewReport { LessonReviewReportId = 1, LessonReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new LessonReview { LessonReviewId = 10, LessonId = 6, Enrollment = new Enrollment { UserId = 4, CourseId = 5 } };

        //Arrange 2
        _reportRepoMock.GetLessonReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetLessonReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetRoleByAccountIdAsync(resolverId).Returns("staff");
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveLessonReviewReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        report.UserReportsStatus.Should().Be(req.Status);
        review.IsRemoved.Should().BeTrue();

        await _penaltyServiceMock.Received(1).ProcessReviewStrikeAsync(4, "Noted", Arg.Any<string>());
        _reportRepoMock.Received(1).UpdateLessonReviewReport(report);
        await _reportRepoMock.Received(1).SaveChangesAsync();
        await _notificationServiceMock.Received(1).SendNotificationAsync(report.ReporterId.Value, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveLessonReviewReportAsync_WarnOnly_ShouldNotifyOwner()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Noted", RemoveContent = false };
        var report = new LessonReviewReport { LessonReviewReportId = 1, LessonReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new LessonReview { LessonReviewId = 10, LessonId = 6, Enrollment = new Enrollment { UserId = 4, CourseId = 5 } };

        //Arrange 2
        _reportRepoMock.GetLessonReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetLessonReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetRoleByAccountIdAsync(resolverId).Returns("staff");
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveLessonReviewReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();

        await _penaltyServiceMock.DidNotReceive().ProcessReviewStrikeAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>());
        await _notificationServiceMock.Received(1).SendNotificationAsync(4, "Review Policy Warning", Arg.Any<string>(), Arg.Any<string>());
        await _reportRepoMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task ResolveLessonReviewReportAsync_ReportMissingReviewId_ShouldThrowBadRequestException()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue() };
        var report = new LessonReviewReport { LessonReviewReportId = 1, LessonReviewId = null };

        //Arrange 2
        _reportRepoMock.GetLessonReviewReportByIdAsync(reportId).Returns(report);

        //Act
        Func<Task> act = async () => await _sut.ResolveLessonReviewReportAsync(reportId, resolverId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Lesson Review ID is missing from the report.");
    }
}
