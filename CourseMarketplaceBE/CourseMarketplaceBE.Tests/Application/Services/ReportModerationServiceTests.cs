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
    public async Task ResolveCourseReportAsync_StatusResolvedAndRemoveContentTrue_AppliesStrikeRemovesCacheAndNotifies()
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
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
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
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.When(x => x.SaveChangesAsync()).Throw(new ReportException("DB Error"));

        //Act
        Func<Task> act = async () => await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("DB Error");
    }


    [Fact]
    public async Task ResolveCourseReportAsync_ReportNotFound_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue() };

        //Arrange 2
        _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns((CourseReport)null);

        //Act
        Func<Task> act = async () => await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

        //Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Report not found.");
    }

    [Fact]
    public async Task ResolveCourseReportAsync_CourseNotFoundInRepo_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue() };
        var report = new CourseReport { CourseReportId = 1, CourseId = 10 };

        //Arrange 2
        _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
        _courseRepoMock.GetByIdAsync(10).Returns((Course)null);

        //Act
        Func<Task> act = async () => await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

        //Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Course not found.");
    }

    [Fact]
    public async Task ResolveCourseReportAsync_StatusResolvedAndRemoveContentFalse_NotifiesInstructorOfPolicyWarning()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Warn", RemoveContent = false };
        var report = new CourseReport { CourseReportId = 1, CourseId = 10, CourseReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var course = new Course { CourseId = 10, Title = "Test", InstructorId = 4 };

        //Arrange 2
        _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
        _courseRepoMock.GetByIdAsync(10).Returns(course);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        await _penaltyServiceMock.DidNotReceive().ProcessCourseStrikeAsync(Arg.Any<int>(), Arg.Any<string>());
        await _notificationServiceMock.Received(1).SendNotificationAsync(4, "Course Policy Warning", Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveCourseReportAsync_StatusRejected_SendsNotificationToReporterWithDismissedMessage()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Rejected.ToValue(), ResolutionNote = "No issue" };
        var report = new CourseReport { CourseReportId = 1, CourseId = 10, CourseReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var course = new Course { CourseId = 10, Title = "Test" };

        //Arrange 2
        _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
        _courseRepoMock.GetByIdAsync(10).Returns(course);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        await _notificationServiceMock.Received(1).SendNotificationAsync(3, "Report Resolution Update", Arg.Is<string>(s => s.Contains("dismissed")), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveCourseReportAsync_StatusUnderReview_SendsNotificationToReporterWithUnderReviewMessage()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.UnderReview.ToValue(), ResolutionNote = "Checking" };
        var report = new CourseReport { CourseReportId = 1, CourseId = 10, CourseReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var course = new Course { CourseId = 10, Title = "Test" };

        //Arrange 2
        _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
        _courseRepoMock.GetByIdAsync(10).Returns(course);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        await _notificationServiceMock.Received(1).SendNotificationAsync(3, "Report Resolution Update", Arg.Is<string>(s => s.Contains("under review")), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveCourseReportAsync_WhenCurrentStatusIsEscalatedAndResolverIsStaff_ThrowsUnauthorizedAccessException()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Fixing" };
        var report = new CourseReport { CourseReportId = 1, CourseId = 10, CourseReportsStatus = ReportStatus.Escalated.ToValue(), ReporterId = 3 };
        var course = new Course { CourseId = 10, Title = "Test" };

        //Arrange 2
        _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
        _courseRepoMock.GetByIdAsync(10).Returns(course);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });

        //Act
        Func<Task> act = async () => await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

        //Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Only admins can resolve escalated reports.");
    }

    [Fact]
    public async Task ResolveCourseReportAsync_WhenCurrentStatusIsEscalatedAndResolverIsAdmin_ResolvesSuccessfully()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        int adminId = 99;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Fixed" };
        var report = new CourseReport { CourseReportId = 1, CourseId = 10, CourseReportsStatus = ReportStatus.Escalated.ToValue(), ReporterId = 3 };
        var course = new Course { CourseId = 10, Title = "Test" };

        //Arrange 2
        _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
        _courseRepoMock.GetByIdAsync(10).Returns(course);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "admin" } });
        _userRepoMock.GetAdminIdAsync().Returns(adminId);
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        await _notificationServiceMock.Received(1).SendNotificationAsync(3, "Report Resolution Update", Arg.Is<string>(s => s.Contains("accepted")), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveCourseReportAsync_StatusEscalated_SendsNotificationToReporterWithEscalatedMessage_AndToAdmin()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        int adminId = 99;
        var req = new ResolveReportRequest { Status = ReportStatus.Escalated.ToValue(), ResolutionNote = "Need admin" };
        var report = new CourseReport { CourseReportId = 1, CourseId = 10, CourseReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var course = new Course { CourseId = 10, Title = "Test" };

        //Arrange 2
        _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
        _courseRepoMock.GetByIdAsync(10).Returns(course);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _userRepoMock.GetAdminIdAsync().Returns(adminId);
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        await _notificationServiceMock.Received(1).SendNotificationAsync(3, "Report Resolution Update", Arg.Is<string>(s => s.Contains("escalated")), Arg.Any<string>());
        await _notificationServiceMock.Received(1).SendNotificationAsync(adminId, "Report Escalated", Arg.Any<string>(), Arg.Any<string>());
    }

    // ── ResolveCourseReviewReportAsync ───────────────────────────────────────

    [Fact]
    public async Task ResolveCourseReviewReportAsync_StatusResolvedAndRemoveContentTrueWithEnrollment_AppliesStrikeAndRemovesReview()
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
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
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
    public async Task ResolveCourseReviewReportAsync_StatusResolvedAndRemoveContentFalse_NotifiesReviewAuthorOfPolicyWarning()
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
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
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


    [Fact]
    public async Task ResolveCourseReviewReportAsync_ReportNotFound_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue() };

        //Arrange 2
        _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns((CourseReviewReport)null);

        //Act
        Func<Task> act = async () => await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

        //Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Report not found.");
    }

    [Fact]
    public async Task ResolveCourseReviewReportAsync_ReviewNotFoundInRepo_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue() };
        var report = new CourseReviewReport { CourseReviewReportId = 1, CourseReviewId = 10 };

        //Arrange 2
        _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetCourseReviewByIdAsync(10).Returns((CourseReview)null);

        //Act
        Func<Task> act = async () => await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

        //Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Course review not found.");
    }

    [Fact]
    public async Task ResolveCourseReviewReportAsync_StatusRejected_SendsNotificationToReporterWithDismissedMessage()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Rejected.ToValue(), ResolutionNote = "No issue" };
        var report = new CourseReviewReport { CourseReviewReportId = 1, CourseReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new CourseReview { CourseReviewId = 10 };

        //Arrange 2
        _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetCourseReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        await _notificationServiceMock.Received(1).SendNotificationAsync(3, "Report Resolution Update", Arg.Is<string>(s => s.Contains("dismissed")), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveCourseReviewReportAsync_StatusUnderReview_SendsNotificationToReporterWithUnderReviewMessage()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.UnderReview.ToValue(), ResolutionNote = "Checking" };
        var report = new CourseReviewReport { CourseReviewReportId = 1, CourseReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new CourseReview { CourseReviewId = 10 };

        //Arrange 2
        _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetCourseReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        await _notificationServiceMock.Received(1).SendNotificationAsync(3, "Report Resolution Update", Arg.Is<string>(s => s.Contains("under review")), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveCourseReviewReportAsync_StatusEscalated_SendsNotificationToReporterWithEscalatedMessage_AndToAdmin()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        int adminId = 99;
        var req = new ResolveReportRequest { Status = ReportStatus.Escalated.ToValue(), ResolutionNote = "Need admin" };
        var report = new CourseReviewReport { CourseReviewReportId = 1, CourseReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new CourseReview { CourseReviewId = 10 };

        //Arrange 2
        _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetCourseReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _userRepoMock.GetAdminIdAsync().Returns(adminId);
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        await _notificationServiceMock.Received(1).SendNotificationAsync(3, "Report Resolution Update", Arg.Is<string>(s => s.Contains("escalated")), Arg.Any<string>());
        await _notificationServiceMock.Received(1).SendNotificationAsync(adminId, "Report Escalated", Arg.Any<string>(), Arg.Any<string>());
    }

    // ── ResolveLessonReviewReportAsync ───────────────────────────────────────

    [Fact]
    public async Task ResolveLessonReviewReportAsync_StatusResolvedAndRemoveContentTrueWithEnrollment_AppliesStrikeAndRemovesReview()
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
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
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
    public async Task ResolveLessonReviewReportAsync_StatusResolvedAndRemoveContentFalse_NotifiesReviewAuthorOfPolicyWarning()
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
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
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

    [Fact]
    public async Task ResolveLessonReviewReportAsync_ReportNotFound_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue() };

        //Arrange 2
        _reportRepoMock.GetLessonReviewReportByIdAsync(reportId).Returns((LessonReviewReport)null);

        //Act
        Func<Task> act = async () => await _sut.ResolveLessonReviewReportAsync(reportId, resolverId, req);

        //Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Report not found.");
    }

    [Fact]
    public async Task ResolveLessonReviewReportAsync_ReviewNotFoundInRepo_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue() };
        var report = new LessonReviewReport { LessonReviewReportId = 1, LessonReviewId = 10 };

        //Arrange 2
        _reportRepoMock.GetLessonReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetLessonReviewByIdAsync(10).Returns((LessonReview)null);

        //Act
        Func<Task> act = async () => await _sut.ResolveLessonReviewReportAsync(reportId, resolverId, req);

        //Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Lesson review not found.");
    }

    [Fact]
    public async Task ResolveLessonReviewReportAsync_StatusRejected_SendsNotificationToReporterWithDismissedMessage()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Rejected.ToValue(), ResolutionNote = "No issue" };
        var report = new LessonReviewReport { LessonReviewReportId = 1, LessonReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new LessonReview { LessonReviewId = 10 };

        //Arrange 2
        _reportRepoMock.GetLessonReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetLessonReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveLessonReviewReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        await _notificationServiceMock.Received(1).SendNotificationAsync(3, "Report Resolution Update", Arg.Is<string>(s => s.Contains("dismissed")), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveLessonReviewReportAsync_StatusUnderReview_SendsNotificationToReporterWithUnderReviewMessage()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.UnderReview.ToValue(), ResolutionNote = "Checking" };
        var report = new LessonReviewReport { LessonReviewReportId = 1, LessonReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new LessonReview { LessonReviewId = 10 };

        //Arrange 2
        _reportRepoMock.GetLessonReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetLessonReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveLessonReviewReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        await _notificationServiceMock.Received(1).SendNotificationAsync(3, "Report Resolution Update", Arg.Is<string>(s => s.Contains("under review")), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveLessonReviewReportAsync_StatusEscalated_SendsNotificationToReporterWithEscalatedMessage_AndToAdmin()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        int adminId = 99;
        var req = new ResolveReportRequest { Status = ReportStatus.Escalated.ToValue(), ResolutionNote = "Need admin" };
        var report = new LessonReviewReport { LessonReviewReportId = 1, LessonReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new LessonReview { LessonReviewId = 10 };

        //Arrange 2
        _reportRepoMock.GetLessonReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetLessonReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _userRepoMock.GetAdminIdAsync().Returns(adminId);
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveLessonReviewReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        await _notificationServiceMock.Received(1).SendNotificationAsync(3, "Report Resolution Update", Arg.Is<string>(s => s.Contains("escalated")), Arg.Any<string>());
        await _notificationServiceMock.Received(1).SendNotificationAsync(adminId, "Report Escalated", Arg.Any<string>(), Arg.Any<string>());
    }

    // --- Additional Coverage Tests ---



    [Fact]
    public async Task ResolveCourseReportAsync_ReporterIdIsNull_DoesNotNotifyReporter()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Fixing" };
        var report = new CourseReport { CourseReportId = 1, CourseId = 10, CourseReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = null };
        var course = new Course { CourseId = 10, Title = "Test" };

        //Arrange 2
        _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
        _courseRepoMock.GetByIdAsync(10).Returns(course);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        await _notificationServiceMock.DidNotReceiveWithAnyArgs().SendNotificationAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveCourseReviewReportAsync_EnrollmentIsNull_RemovesReviewWithoutStrike()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Violation", RemoveContent = true };
        var report = new CourseReviewReport { CourseReviewReportId = 1, CourseReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new CourseReview { CourseReviewId = 10, Enrollment = null };

        //Arrange 2
        _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetCourseReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        _reviewRepoMock.Received(1).UpdateCourseReview(Arg.Is<CourseReview>(r => r.IsRemoved == true));
        await _penaltyServiceMock.DidNotReceiveWithAnyArgs().ProcessReviewStrikeAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveCourseReviewReportAsync_UserIdIsZero_RemovesReviewWithoutStrike()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Violation", RemoveContent = true };
        var report = new CourseReviewReport { CourseReviewReportId = 1, CourseReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new CourseReview { CourseReviewId = 10, Enrollment = new Enrollment { UserId = 0 } };

        //Arrange 2
        _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetCourseReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        _reviewRepoMock.Received(1).UpdateCourseReview(Arg.Is<CourseReview>(r => r.IsRemoved == true));
        await _penaltyServiceMock.DidNotReceiveWithAnyArgs().ProcessReviewStrikeAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveLessonReviewReportAsync_EnrollmentIsNull_RemovesReviewWithoutStrike()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Violation", RemoveContent = true };
        var report = new LessonReviewReport { LessonReviewReportId = 1, LessonReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new LessonReview { LessonReviewId = 10, Enrollment = null };

        //Arrange 2
        _reportRepoMock.GetLessonReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetLessonReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveLessonReviewReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        _reviewRepoMock.Received(1).UpdateLessonReview(Arg.Is<LessonReview>(r => r.IsRemoved == true));
        await _penaltyServiceMock.DidNotReceiveWithAnyArgs().ProcessReviewStrikeAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveLessonReviewReportAsync_UserIdIsZero_RemovesReviewWithoutStrike()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Violation", RemoveContent = true };
        var report = new LessonReviewReport { LessonReviewReportId = 1, LessonReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new LessonReview { LessonReviewId = 10, Enrollment = new Enrollment { UserId = 0 } };

        //Arrange 2
        _reportRepoMock.GetLessonReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetLessonReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveLessonReviewReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        _reviewRepoMock.Received(1).UpdateLessonReview(Arg.Is<LessonReview>(r => r.IsRemoved == true));
        await _penaltyServiceMock.DidNotReceiveWithAnyArgs().ProcessReviewStrikeAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>());
    }

    // --- Last 4% Coverage Tests ---

    [Fact]
    public async Task ResolveCourseReportAsync_RemoveContentIsFalse_AppliesWarningToInstructor()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Fixing", RemoveContent = false };
        var report = new CourseReport { CourseReportId = 1, CourseId = 10, CourseReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var course = new Course { CourseId = 10, Title = "Test", InstructorId = 5 };

        //Arrange 2
        _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
        _courseRepoMock.GetByIdAsync(10).Returns(course);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        await _notificationServiceMock.Received(1).SendNotificationAsync(5, "Course Policy Warning", Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveCourseReviewReportAsync_RemoveContentIsFalse_SendsWarningNotification()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Violation", RemoveContent = false };
        var report = new CourseReviewReport { CourseReviewReportId = 1, CourseReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new CourseReview { CourseReviewId = 10, Enrollment = new Enrollment { UserId = 5 } };

        //Arrange 2
        _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetCourseReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        await _notificationServiceMock.Received(1).SendNotificationAsync(5, "Review Policy Warning", Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveLessonReviewReportAsync_RemoveContentIsFalse_SendsWarningNotification()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Violation", RemoveContent = false };
        var report = new LessonReviewReport { LessonReviewReportId = 1, LessonReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new LessonReview { LessonReviewId = 10, Enrollment = new Enrollment { UserId = 5 } };

        //Arrange 2
        _reportRepoMock.GetLessonReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetLessonReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act
        var result = await _sut.ResolveLessonReviewReportAsync(reportId, resolverId, req);

        //Assert
        result.Should().BeTrue();
        await _notificationServiceMock.Received(1).SendNotificationAsync(5, "Review Policy Warning", Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveCourseReviewReportAsync_RemoveContentIsFalse_EnrollmentIsNull_DoesNotSendWarning()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Violation", RemoveContent = false };
        var report = new CourseReviewReport { CourseReviewReportId = 1, CourseReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new CourseReview { CourseReviewId = 10, Enrollment = null };
        //Arrange 2
        _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetCourseReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act

        var result = await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

        //Assert

        result.Should().BeTrue();
        await _notificationServiceMock.DidNotReceive().SendNotificationAsync(Arg.Any<int>(), "Review Policy Warning", Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveCourseReviewReportAsync_RemoveContentIsFalse_UserIdIsZero_DoesNotSendWarning()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Violation", RemoveContent = false };
        var report = new CourseReviewReport { CourseReviewReportId = 1, CourseReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new CourseReview { CourseReviewId = 10, Enrollment = new Enrollment { UserId = 0 } };
        //Arrange 2
        _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetCourseReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act

        var result = await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

        //Assert

        result.Should().BeTrue();
        await _notificationServiceMock.DidNotReceive().SendNotificationAsync(Arg.Any<int>(), "Review Policy Warning", Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveLessonReviewReportAsync_RemoveContentIsFalse_EnrollmentIsNull_DoesNotSendWarning()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Violation", RemoveContent = false };
        var report = new LessonReviewReport { LessonReviewReportId = 1, LessonReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new LessonReview { LessonReviewId = 10, Enrollment = null };
        //Arrange 2
        _reportRepoMock.GetLessonReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetLessonReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act

        var result = await _sut.ResolveLessonReviewReportAsync(reportId, resolverId, req);

        //Assert

        result.Should().BeTrue();
        await _notificationServiceMock.DidNotReceive().SendNotificationAsync(Arg.Any<int>(), "Review Policy Warning", Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveLessonReviewReportAsync_RemoveContentIsFalse_UserIdIsZero_DoesNotSendWarning()
    {
        //Arrange 1
        int reportId = 1;
        int resolverId = 2;
        var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), ResolutionNote = "Violation", RemoveContent = false };
        var report = new LessonReviewReport { LessonReviewReportId = 1, LessonReviewId = 10, UserReportsStatus = ReportStatus.Pending.ToValue(), ReporterId = 3 };
        var review = new LessonReview { LessonReviewId = 10, Enrollment = new Enrollment { UserId = 0 } };
        //Arrange 2
        _reportRepoMock.GetLessonReviewReportByIdAsync(reportId).Returns(report);
        _reviewRepoMock.GetLessonReviewByIdAsync(10).Returns(review);
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = new CourseMarketplaceBE.Domain.Entities.Manager { Role = "staff" } });
        _reportRepoMock.SaveChangesAsync().Returns(1);

        //Act

        var result = await _sut.ResolveLessonReviewReportAsync(reportId, resolverId, req);

        //Assert

        result.Should().BeTrue();
        await _notificationServiceMock.DidNotReceive().SendNotificationAsync(Arg.Any<int>(), "Review Policy Warning", Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveLinkActionAsync_UnknownType_ReturnsRoot()
    {
        //Arrange 1
        //Arrange 2
        var method = typeof(ReportModerationService).GetMethod("ResolveLinkActionAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        //Act
        var result = await (Task<string>)method!.Invoke(_sut, new object[] { "unknown", 1 })!;
        //Assert
        result.Should().Be("/");
    }


    [Fact]
    public async Task ValidateReportResolutionAccessAsync_AccountIsNull_ThrowsUnauthorized()
    {
        //Arrange 1
        int resolverId = 2;
        //Arrange 2
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns((CourseMarketplaceBE.Domain.Entities.Account)null!);
        var method = typeof(ReportModerationService).GetMethod("ValidateReportResolutionAccessAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        //Act
        
        Func<Task> act = async () => await (Task)method!.Invoke(_sut, new object[] { "Pending", resolverId })!;
        
        //Assert
        
        var ex = await act.Should().ThrowAsync<UnauthorizedAccessException>();
        ex.WithMessage("Only staff or admin can resolve reports.");
    }

    [Fact]
    public async Task ValidateReportResolutionAccessAsync_ManagerIsNull_ThrowsUnauthorized()
    {
        //Arrange 1
        int resolverId = 2;
        //Arrange 2
        _userRepoMock.GetAccountByIdAsync(resolverId).Returns(new CourseMarketplaceBE.Domain.Entities.Account { Manager = null! });
        var method = typeof(ReportModerationService).GetMethod("ValidateReportResolutionAccessAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        //Act
        
        Func<Task> act = async () => await (Task)method!.Invoke(_sut, new object[] { "Pending", resolverId })!;
        
        //Assert
        
        var ex = await act.Should().ThrowAsync<UnauthorizedAccessException>();
        ex.WithMessage("Only staff or admin can resolve reports.");
    }
}
