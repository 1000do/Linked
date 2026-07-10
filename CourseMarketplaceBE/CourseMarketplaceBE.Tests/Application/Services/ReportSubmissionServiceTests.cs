using System;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using FluentAssertions;
using NSubstitute;
using Xunit;
using AutoMapper;

namespace CourseMarketplaceBE.Tests.Application.Services;

public class ReportSubmissionServiceTests
{
    private readonly IReportRepository _reportRepoMock;
    private readonly IEnrollmentRepository _enrollmentRepoMock;
    private readonly ICourseRepository _courseRepoMock;
    private readonly IReviewRepository _reviewRepoMock;
    private readonly IMapper _mapperMock;
    private readonly INotificationService _notificationServiceMock;
    private readonly IUserRepository _userRepoMock;
    private readonly ReportSubmissionService _sut;

    public ReportSubmissionServiceTests()
    {
        _reportRepoMock = Substitute.For<IReportRepository>();
        _enrollmentRepoMock = Substitute.For<IEnrollmentRepository>();
        _courseRepoMock = Substitute.For<ICourseRepository>();
        _reviewRepoMock = Substitute.For<IReviewRepository>();
        _mapperMock = Substitute.For<IMapper>();
        _notificationServiceMock = Substitute.For<INotificationService>();
        _userRepoMock = Substitute.For<IUserRepository>();

        _sut = new ReportSubmissionService(
            _reportRepoMock,
            _enrollmentRepoMock,
            _courseRepoMock,
            _reviewRepoMock,
            _mapperMock,
            _notificationServiceMock,
            _userRepoMock
        );
    }

    // ── CreateCourseReportAsync ──────────────────────────────────────────────

    [Fact]
    public async Task CreateCourseReportAsync_ValidRequest_ShouldReturnTrue()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReportRequest { CourseId = 10, Reason = "Spam", Description = "Desc" };
        var course = new Course { CourseId = 10, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };
        var enrollment = new Enrollment { UserId = reporterId, CourseId = 10 };

        //Arrange 2
        _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);
        _enrollmentRepoMock.GetEnrollmentWithProgressAsync(reporterId, req.CourseId).Returns(enrollment);
        _reportRepoMock.GetPendingCourseReportAsync(reporterId, req.CourseId, req.Reason).Returns((CourseReport?)null);
        _reportRepoMock.SaveChangesAsync().Returns(1);
        _userRepoMock.GetAllManagerIdsAsync().Returns(new System.Collections.Generic.List<int> { 999 });

        //Act
        var result = await _sut.CreateCourseReportAsync(reporterId, req);

        //Assert
        result.Should().BeTrue();
        
        await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
        await _enrollmentRepoMock.Received(1).GetEnrollmentWithProgressAsync(reporterId, req.CourseId);
        await _reportRepoMock.Received(1).GetPendingCourseReportAsync(reporterId, req.CourseId, req.Reason);
        await _reportRepoMock.Received(1).AddCourseReportAsync(Arg.Is<CourseReport>(r => 
            r.ReporterId == reporterId && r.CourseId == req.CourseId && r.Reason == req.Reason));
        await _reportRepoMock.Received(1).SaveChangesAsync();
        await _notificationServiceMock.Received(1).SendBulkNotificationsAsync(Arg.Any<System.Collections.Generic.IEnumerable<NotificationBulkDto>>());
    }

    [Fact]
    public async Task CreateCourseReportAsync_CourseNotFound_ShouldThrowInvalidOperationException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReportRequest { CourseId = 10, Reason = "Spam" };

        //Arrange 2
        _courseRepoMock.GetByIdAsync(req.CourseId).Returns((Course?)null);

        //Act
        Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req);

        //Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Course not found.");
        
        await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
        await _enrollmentRepoMock.DidNotReceive().GetEnrollmentWithProgressAsync(Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public async Task CreateCourseReportAsync_CoursePending_ShouldThrowInvalidOperationException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReportRequest { CourseId = 10, Reason = "Spam" };
        var course = new Course { CourseId = 10, CourseStatus = CourseStatus.Pending.ToValue(), InstructorId = 2 };

        //Arrange 2
        _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);

        //Act
        Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("This course is currently in draft, pending, under review, or rejected and cannot be reported.");
        
        await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
    }

    [Fact]
    public async Task CreateCourseReportAsync_CourseArchivedAndLocked_ShouldThrowInvalidOperationException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReportRequest { CourseId = 10, Reason = "Spam" };
        var course = new Course { CourseId = 10, CourseStatus = CourseStatus.Archived.ToValue(), CourseFlagCount = 3 };

        //Arrange 2
        _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);

        //Act
        Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("This course is permanently locked and cannot be reported.");
        
        await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
    }

    [Fact]
    public async Task CreateCourseReportAsync_ReportingOwnCourse_ShouldThrowInvalidOperationException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReportRequest { CourseId = 10, Reason = "Spam" };
        var course = new Course { CourseId = 10, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = reporterId };

        //Arrange 2
        _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);

        //Act
        Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("You cannot report your own course.");
        
        await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
    }

    [Fact]
    public async Task CreateCourseReportAsync_NotEnrolled_ShouldThrowInvalidOperationException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReportRequest { CourseId = 10, Reason = "Spam" };
        var course = new Course { CourseId = 10, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };

        //Arrange 2
        _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);
        _enrollmentRepoMock.GetEnrollmentWithProgressAsync(reporterId, req.CourseId).Returns((Enrollment?)null);

        //Act
        Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("You must be enrolled in the course before reporting it.");
        
        await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
        await _enrollmentRepoMock.Received(1).GetEnrollmentWithProgressAsync(reporterId, req.CourseId);
    }

    [Fact]
    public async Task CreateCourseReportAsync_EmptyReason_ShouldThrowInvalidOperationException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReportRequest { CourseId = 10, Reason = "" };
        var course = new Course { CourseId = 10, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };
        var enrollment = new Enrollment { UserId = reporterId, CourseId = 10 };

        //Arrange 2
        _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);
        _enrollmentRepoMock.GetEnrollmentWithProgressAsync(reporterId, req.CourseId).Returns(enrollment);

        //Act
        Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Report reason cannot be empty.");
        
        await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
        await _enrollmentRepoMock.Received(1).GetEnrollmentWithProgressAsync(reporterId, req.CourseId);
    }

    [Fact]
    public async Task CreateCourseReportAsync_DuplicateReport_ShouldThrowInvalidOperationException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReportRequest { CourseId = 10, Reason = "Spam" };
        var course = new Course { CourseId = 10, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };
        var enrollment = new Enrollment { UserId = reporterId, CourseId = 10 };
        var existingReport = new CourseReport { CourseReportId = 5 };

        //Arrange 2
        _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);
        _enrollmentRepoMock.GetEnrollmentWithProgressAsync(reporterId, req.CourseId).Returns(enrollment);
        _reportRepoMock.GetPendingCourseReportAsync(reporterId, req.CourseId, req.Reason).Returns(existingReport);

        //Act
        Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("You already have a pending report with this reason for this course.");
        
        await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
        await _enrollmentRepoMock.Received(1).GetEnrollmentWithProgressAsync(reporterId, req.CourseId);
        await _reportRepoMock.Received(1).GetPendingCourseReportAsync(reporterId, req.CourseId, req.Reason);
    }

    [Fact]
    public async Task CreateCourseReportAsync_SaveThrowsReportException_ShouldThrowBadRequestException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReportRequest { CourseId = 10, Reason = "Spam" };
        var course = new Course { CourseId = 10, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };
        var enrollment = new Enrollment { UserId = reporterId, CourseId = 10 };

        //Arrange 2
        _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);
        _enrollmentRepoMock.GetEnrollmentWithProgressAsync(reporterId, req.CourseId).Returns(enrollment);
        _reportRepoMock.GetPendingCourseReportAsync(reporterId, req.CourseId, req.Reason).Returns((CourseReport?)null);
        _reportRepoMock.When(x => x.SaveChangesAsync()).Throw(new ReportException("DB Error"));

        //Act
        Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("DB Error");
        
        await _reportRepoMock.Received(1).SaveChangesAsync();
    }

    // ── CreateCourseReviewReportAsync ────────────────────────────────────────

    [Fact]
    public async Task CreateCourseReviewReportAsync_ValidRequest_ShouldReturnTrue()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReviewReportRequest { CourseReviewId = 10, Reason = "Offensive" };
        var review = new CourseReview { CourseReviewId = 10 };

        //Arrange 2
        _reviewRepoMock.GetCourseReviewByIdAsync(req.CourseReviewId).Returns(review);
        _reportRepoMock.GetPendingCourseReviewReportAsync(reporterId, req.CourseReviewId, req.Reason).Returns((CourseReviewReport?)null);
        _reportRepoMock.SaveChangesAsync().Returns(1);
        _userRepoMock.GetAllManagerIdsAsync().Returns(new System.Collections.Generic.List<int> { 999 });

        //Act
        var result = await _sut.CreateCourseReviewReportAsync(reporterId, req);

        //Assert
        result.Should().BeTrue();
        
        await _reviewRepoMock.Received(1).GetCourseReviewByIdAsync(req.CourseReviewId);
        await _reportRepoMock.Received(1).GetPendingCourseReviewReportAsync(reporterId, req.CourseReviewId, req.Reason);
        await _reportRepoMock.Received(1).AddCourseReviewReportAsync(Arg.Is<CourseReviewReport>(r => 
            r.ReporterId == reporterId && r.CourseReviewId == req.CourseReviewId && r.Reason == req.Reason));
        await _reportRepoMock.Received(1).SaveChangesAsync();
        await _notificationServiceMock.Received(1).SendBulkNotificationsAsync(Arg.Any<System.Collections.Generic.IEnumerable<NotificationBulkDto>>());
    }

    [Fact]
    public async Task CreateCourseReviewReportAsync_ReviewNotFound_ShouldThrowInvalidOperationException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReviewReportRequest { CourseReviewId = 10, Reason = "Offensive" };

        //Arrange 2
        _reviewRepoMock.GetCourseReviewByIdAsync(req.CourseReviewId).Returns((CourseReview?)null);

        //Act
        Func<Task> act = async () => await _sut.CreateCourseReviewReportAsync(reporterId, req);

        //Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Review not found.");
        
        await _reviewRepoMock.Received(1).GetCourseReviewByIdAsync(req.CourseReviewId);
    }

    [Fact]
    public async Task CreateCourseReviewReportAsync_EmptyReason_ShouldThrowInvalidOperationException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReviewReportRequest { CourseReviewId = 10, Reason = "" };
        var review = new CourseReview { CourseReviewId = 10 };

        //Arrange 2
        _reviewRepoMock.GetCourseReviewByIdAsync(req.CourseReviewId).Returns(review);

        //Act
        Func<Task> act = async () => await _sut.CreateCourseReviewReportAsync(reporterId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Report reason cannot be empty.");
        
        await _reviewRepoMock.Received(1).GetCourseReviewByIdAsync(req.CourseReviewId);
    }

    [Fact]
    public async Task CreateCourseReviewReportAsync_DuplicateReport_ShouldThrowInvalidOperationException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReviewReportRequest { CourseReviewId = 10, Reason = "Offensive" };
        var review = new CourseReview { CourseReviewId = 10 };
        var existing = new CourseReviewReport { CourseReviewReportId = 1 };

        //Arrange 2
        _reviewRepoMock.GetCourseReviewByIdAsync(req.CourseReviewId).Returns(review);
        _reportRepoMock.GetPendingCourseReviewReportAsync(reporterId, req.CourseReviewId, req.Reason).Returns(existing);

        //Act
        Func<Task> act = async () => await _sut.CreateCourseReviewReportAsync(reporterId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("You already have a pending report with this reason for this review.");
        
        await _reviewRepoMock.Received(1).GetCourseReviewByIdAsync(req.CourseReviewId);
        await _reportRepoMock.Received(1).GetPendingCourseReviewReportAsync(reporterId, req.CourseReviewId, req.Reason);
    }

    [Fact]
    public async Task CreateCourseReviewReportAsync_SaveThrowsReportException_ShouldThrowBadRequestException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReviewReportRequest { CourseReviewId = 10, Reason = "Offensive" };
        var review = new CourseReview { CourseReviewId = 10 };

        //Arrange 2
        _reviewRepoMock.GetCourseReviewByIdAsync(req.CourseReviewId).Returns(review);
        _reportRepoMock.GetPendingCourseReviewReportAsync(reporterId, req.CourseReviewId, req.Reason).Returns((CourseReviewReport?)null);
        _reportRepoMock.When(x => x.SaveChangesAsync()).Throw(new ReportException("DB Error"));

        //Act
        Func<Task> act = async () => await _sut.CreateCourseReviewReportAsync(reporterId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("DB Error");
        
        await _reportRepoMock.Received(1).SaveChangesAsync();
    }

    // ── CreateLessonReviewReportAsync ────────────────────────────────────────

    [Fact]
    public async Task CreateLessonReviewReportAsync_ValidRequest_ShouldReturnTrue()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateLessonReviewReportRequest { LessonReviewId = 10, Reason = "Offensive" };
        var review = new LessonReview { LessonReviewId = 10 };

        //Arrange 2
        _reviewRepoMock.GetLessonReviewByIdAsync(req.LessonReviewId).Returns(review);
        _reportRepoMock.GetPendingLessonReviewReportAsync(reporterId, req.LessonReviewId, req.Reason).Returns((LessonReviewReport?)null);
        _reportRepoMock.SaveChangesAsync().Returns(1);
        _userRepoMock.GetAllManagerIdsAsync().Returns(new System.Collections.Generic.List<int> { 999 });

        //Act
        var result = await _sut.CreateLessonReviewReportAsync(reporterId, req);

        //Assert
        result.Should().BeTrue();
        
        await _reviewRepoMock.Received(1).GetLessonReviewByIdAsync(req.LessonReviewId);
        await _reportRepoMock.Received(1).GetPendingLessonReviewReportAsync(reporterId, req.LessonReviewId, req.Reason);
        await _reportRepoMock.Received(1).AddLessonReviewReportAsync(Arg.Is<LessonReviewReport>(r => 
            r.ReporterId == reporterId && r.LessonReviewId == req.LessonReviewId && r.Reason == req.Reason));
        await _reportRepoMock.Received(1).SaveChangesAsync();
        await _notificationServiceMock.Received(1).SendBulkNotificationsAsync(Arg.Any<System.Collections.Generic.IEnumerable<NotificationBulkDto>>());
    }

    [Fact]
    public async Task CreateLessonReviewReportAsync_ReviewNotFound_ShouldThrowInvalidOperationException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateLessonReviewReportRequest { LessonReviewId = 10, Reason = "Offensive" };

        //Arrange 2
        _reviewRepoMock.GetLessonReviewByIdAsync(req.LessonReviewId).Returns((LessonReview?)null);

        //Act
        Func<Task> act = async () => await _sut.CreateLessonReviewReportAsync(reporterId, req);

        //Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Review not found.");
        
        await _reviewRepoMock.Received(1).GetLessonReviewByIdAsync(req.LessonReviewId);
    }

    [Fact]
    public async Task CreateLessonReviewReportAsync_EmptyReason_ShouldThrowInvalidOperationException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateLessonReviewReportRequest { LessonReviewId = 10, Reason = "" };
        var review = new LessonReview { LessonReviewId = 10 };

        //Arrange 2
        _reviewRepoMock.GetLessonReviewByIdAsync(req.LessonReviewId).Returns(review);

        //Act
        Func<Task> act = async () => await _sut.CreateLessonReviewReportAsync(reporterId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Report reason cannot be empty.");
        
        await _reviewRepoMock.Received(1).GetLessonReviewByIdAsync(req.LessonReviewId);
    }

    [Fact]
    public async Task CreateLessonReviewReportAsync_DuplicateReport_ShouldThrowInvalidOperationException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateLessonReviewReportRequest { LessonReviewId = 10, Reason = "Offensive" };
        var review = new LessonReview { LessonReviewId = 10 };
        var existing = new LessonReviewReport { LessonReviewReportId = 1 };

        //Arrange 2
        _reviewRepoMock.GetLessonReviewByIdAsync(req.LessonReviewId).Returns(review);
        _reportRepoMock.GetPendingLessonReviewReportAsync(reporterId, req.LessonReviewId, req.Reason).Returns(existing);

        //Act
        Func<Task> act = async () => await _sut.CreateLessonReviewReportAsync(reporterId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("You already have a pending report with this reason for this review.");
        
        await _reviewRepoMock.Received(1).GetLessonReviewByIdAsync(req.LessonReviewId);
        await _reportRepoMock.Received(1).GetPendingLessonReviewReportAsync(reporterId, req.LessonReviewId, req.Reason);
    }

    [Fact]
    public async Task CreateLessonReviewReportAsync_SaveThrowsReportException_ShouldThrowBadRequestException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateLessonReviewReportRequest { LessonReviewId = 10, Reason = "Offensive" };
        var review = new LessonReview { LessonReviewId = 10 };

        //Arrange 2
        _reviewRepoMock.GetLessonReviewByIdAsync(req.LessonReviewId).Returns(review);
        _reportRepoMock.GetPendingLessonReviewReportAsync(reporterId, req.LessonReviewId, req.Reason).Returns((LessonReviewReport?)null);
        _reportRepoMock.When(x => x.SaveChangesAsync()).Throw(new ReportException("DB Error"));

        //Act
        Func<Task> act = async () => await _sut.CreateLessonReviewReportAsync(reporterId, req);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("DB Error");
        
        await _reportRepoMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CreateCourseReportAsync_ReporterIsManager_ThrowsBadRequestException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReportRequest { CourseId = 10, Reason = "Spam" };
        //Arrange 2
        _userRepoMock.GetRoleByAccountIdAsync(reporterId).Returns("manager");
        //Act
        Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req);
        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Managers cannot report courses.");
        await _userRepoMock.Received(1).GetRoleByAccountIdAsync(reporterId);
        await _courseRepoMock.DidNotReceive().GetByIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task CreateCourseReportAsync_CourseIsDraft_ThrowsBadRequestException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReportRequest { CourseId = 10, Reason = "Spam" };
        var course = new Course { CourseId = 10, CourseStatus = CourseStatus.Draft.ToValue(), InstructorId = 2 };
        //Arrange 2
        _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);
        //Act
        Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req);
        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("This course is currently in draft, pending, under review, or rejected and cannot be reported.");
        await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
    }

    [Fact]
    public async Task CreateCourseReportAsync_CourseIsUnderReview_ThrowsBadRequestException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReportRequest { CourseId = 10, Reason = "Spam" };
        var course = new Course { CourseId = 10, CourseStatus = "under_review", InstructorId = 2 };
        //Arrange 2
        _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);
        //Act
        Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req);
        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("This course is currently in draft, pending, under review, or rejected and cannot be reported.");
        await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
    }

    [Fact]
    public async Task CreateCourseReportAsync_CourseIsRejected_ThrowsBadRequestException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReportRequest { CourseId = 10, Reason = "Spam" };
        var course = new Course { CourseId = 10, CourseStatus = CourseStatus.Rejected.ToValue(), InstructorId = 2 };
        //Arrange 2
        _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);
        //Act
        Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req);
        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("This course is currently in draft, pending, under review, or rejected and cannot be reported.");
        await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
    }

    [Fact]
    public async Task CreateCourseReportAsync_CourseArchivedButNotLocked_ThrowsNotEnrolledException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReportRequest { CourseId = 10, Reason = "Spam" };
        var course = new Course { CourseId = 10, CourseStatus = CourseStatus.Archived.ToValue(), CourseFlagCount = 2, InstructorId = 2 };
        //Arrange 2
        _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);
        _enrollmentRepoMock.GetEnrollmentWithProgressAsync(reporterId, req.CourseId).Returns((Enrollment?)null);
        //Act
        Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req);
        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("You must be enrolled in the course before reporting it.");
        await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
        await _enrollmentRepoMock.Received(1).GetEnrollmentWithProgressAsync(reporterId, req.CourseId);
    }

    [Fact]
    public async Task CreateCourseReportAsync_NoManagers_DoesNotSendBulkNotifications()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReportRequest { CourseId = 10, Reason = "Spam", Description = "Desc" };
        var course = new Course { CourseId = 10, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };
        var enrollment = new Enrollment { UserId = reporterId, CourseId = 10 };
        //Arrange 2
        _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);
        _enrollmentRepoMock.GetEnrollmentWithProgressAsync(reporterId, req.CourseId).Returns(enrollment);
        _reportRepoMock.GetPendingCourseReportAsync(reporterId, req.CourseId, req.Reason).Returns((CourseReport?)null);
        _userRepoMock.GetAllManagerIdsAsync().Returns(new System.Collections.Generic.List<int>());
        //Act
        var result = await _sut.CreateCourseReportAsync(reporterId, req);
        //Assert
        result.Should().BeTrue();
        await _notificationServiceMock.DidNotReceive().SendBulkNotificationsAsync(Arg.Any<System.Collections.Generic.IEnumerable<NotificationBulkDto>>());
    }

    [Fact]
    public async Task CreateCourseReviewReportAsync_ReviewIsRemovedPropertyTrue_ThrowsBadRequestException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReviewReportRequest { CourseReviewId = 10, Reason = "Offensive" };
        var review = new CourseReview { CourseReviewId = 10, IsRemoved = true };
        //Arrange 2
        _reviewRepoMock.GetCourseReviewByIdAsync(req.CourseReviewId).Returns(review);
        //Act
        Func<Task> act = async () => await _sut.CreateCourseReviewReportAsync(reporterId, req);
        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("This review has been removed and cannot be reported.");
        await _reviewRepoMock.Received(1).GetCourseReviewByIdAsync(req.CourseReviewId);
    }

    [Fact]
    public async Task CreateCourseReviewReportAsync_ReviewStatusIsRemoved_ThrowsBadRequestException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReviewReportRequest { CourseReviewId = 10, Reason = "Offensive" };
        var review = new CourseReview { CourseReviewId = 10, IsRemoved = false, CourseReviewStatus = ReviewStatus.Removed.ToValue() };
        //Arrange 2
        _reviewRepoMock.GetCourseReviewByIdAsync(req.CourseReviewId).Returns(review);
        //Act
        Func<Task> act = async () => await _sut.CreateCourseReviewReportAsync(reporterId, req);
        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("This review has been removed and cannot be reported.");
        await _reviewRepoMock.Received(1).GetCourseReviewByIdAsync(req.CourseReviewId);
    }

    [Fact]
    public async Task CreateCourseReviewReportAsync_ReviewStatusIsViolating_ThrowsBadRequestException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateCourseReviewReportRequest { CourseReviewId = 10, Reason = "Offensive" };
        var review = new CourseReview { CourseReviewId = 10, IsRemoved = false, CourseReviewStatus = ReviewStatus.Violating.ToValue() };
        //Arrange 2
        _reviewRepoMock.GetCourseReviewByIdAsync(req.CourseReviewId).Returns(review);
        //Act
        Func<Task> act = async () => await _sut.CreateCourseReviewReportAsync(reporterId, req);
        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("This review has been removed and cannot be reported.");
        await _reviewRepoMock.Received(1).GetCourseReviewByIdAsync(req.CourseReviewId);
    }

    [Fact]
    public async Task CreateLessonReviewReportAsync_ReviewIsRemovedPropertyTrue_ThrowsBadRequestException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateLessonReviewReportRequest { LessonReviewId = 10, Reason = "Offensive" };
        var review = new LessonReview { LessonReviewId = 10, IsRemoved = true };
        //Arrange 2
        _reviewRepoMock.GetLessonReviewByIdAsync(req.LessonReviewId).Returns(review);
        //Act
        Func<Task> act = async () => await _sut.CreateLessonReviewReportAsync(reporterId, req);
        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("This review has been removed and cannot be reported.");
        await _reviewRepoMock.Received(1).GetLessonReviewByIdAsync(req.LessonReviewId);
    }

    [Fact]
    public async Task CreateLessonReviewReportAsync_ReviewStatusIsRemoved_ThrowsBadRequestException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateLessonReviewReportRequest { LessonReviewId = 10, Reason = "Offensive" };
        var review = new LessonReview { LessonReviewId = 10, IsRemoved = false, LessonReviewStatus = ReviewStatus.Removed.ToValue() };
        //Arrange 2
        _reviewRepoMock.GetLessonReviewByIdAsync(req.LessonReviewId).Returns(review);
        //Act
        Func<Task> act = async () => await _sut.CreateLessonReviewReportAsync(reporterId, req);
        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("This review has been removed and cannot be reported.");
        await _reviewRepoMock.Received(1).GetLessonReviewByIdAsync(req.LessonReviewId);
    }

    [Fact]
    public async Task CreateLessonReviewReportAsync_ReviewStatusIsViolating_ThrowsBadRequestException()
    {
        //Arrange 1
        int reporterId = 1;
        var req = new CreateLessonReviewReportRequest { LessonReviewId = 10, Reason = "Offensive" };
        var review = new LessonReview { LessonReviewId = 10, IsRemoved = false, LessonReviewStatus = ReviewStatus.Violating.ToValue() };
        //Arrange 2
        _reviewRepoMock.GetLessonReviewByIdAsync(req.LessonReviewId).Returns(review);
        //Act
        Func<Task> act = async () => await _sut.CreateLessonReviewReportAsync(reporterId, req);
        //Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("This review has been removed and cannot be reported.");
        await _reviewRepoMock.Received(1).GetLessonReviewByIdAsync(req.LessonReviewId);
    }

    // ── GetMyCourseReportsAsync ──────────────────────────────────────────────
    [Fact]
    public async Task GetMyCourseReportsAsync_HasReports_ReturnsMappedResponses()
    {
        //Arrange 1
        int reporterId = 1;
        var reports = new System.Collections.Generic.List<CourseReport> { new CourseReport { CourseReportId = 1 } };
        var expected = new System.Collections.Generic.List<MyCourseReportResponse> { new MyCourseReportResponse { ReportId = 1 } };
        //Arrange 2
        _reportRepoMock.GetCourseReportsByReporterAsync(reporterId).Returns(reports);
        _mapperMock.Map<System.Collections.Generic.IEnumerable<MyCourseReportResponse>>(reports).Returns(expected);
        //Act
        var result = await _sut.GetMyCourseReportsAsync(reporterId);
        //Assert
        result.Should().BeEquivalentTo(expected);
        await _reportRepoMock.Received(1).GetCourseReportsByReporterAsync(reporterId);
    }

    // ── GetMyReviewReportsAsync ──────────────────────────────────────────────
    [Fact]
    public async Task GetMyReviewReportsAsync_HasCourseAndLessonReports_ReturnsOrderedCombinedResponses()
    {
        //Arrange 1
        int reporterId = 1;
        var courseReports = new System.Collections.Generic.List<CourseReviewReport> { new CourseReviewReport { CourseReviewReportId = 1 } };
        var lessonReports = new System.Collections.Generic.List<LessonReviewReport> { new LessonReviewReport { LessonReviewReportId = 1 } };
        
        var courseResults = new System.Collections.Generic.List<MyReviewReportResponse> 
        { 
            new MyReviewReportResponse { CreatedAt = new DateTime(2023, 1, 1) } 
        };
        var lessonResults = new System.Collections.Generic.List<MyReviewReportResponse> 
        { 
            new MyReviewReportResponse { CreatedAt = new DateTime(2023, 1, 2) } 
        };

        //Arrange 2
        _reportRepoMock.GetCourseReviewReportsByReporterAsync(reporterId).Returns(courseReports);
        _reportRepoMock.GetLessonReviewReportsByReporterAsync(reporterId).Returns(lessonReports);
        _mapperMock.Map<System.Collections.Generic.IEnumerable<MyReviewReportResponse>>(courseReports).Returns(courseResults);
        _mapperMock.Map<System.Collections.Generic.IEnumerable<MyReviewReportResponse>>(lessonReports).Returns(lessonResults);

        //Act
        var result = await _sut.GetMyReviewReportsAsync(reporterId);

        //Assert
        var arrayResult = System.Linq.Enumerable.ToArray(result);
        arrayResult.Should().HaveCount(2);
        arrayResult[0].CreatedAt.Should().Be(new DateTime(2023, 1, 2)); // Descending order
        arrayResult[1].CreatedAt.Should().Be(new DateTime(2023, 1, 1));
        
        await _reportRepoMock.Received(1).GetCourseReviewReportsByReporterAsync(reporterId);
        await _reportRepoMock.Received(1).GetLessonReviewReportsByReporterAsync(reporterId);
    }

    // ── GetReportsOnMyCourseAsync ──────────────────────────────────────────────
    [Fact]
    public async Task GetReportsOnMyCourseAsync_NotOwner_ThrowsUnauthorizedAccessException()
    {
        //Arrange 1
        int instructorId = 1;
        int courseId = 10;
        //Arrange 2
        _courseRepoMock.IsOwnerAsync(instructorId, courseId).Returns(false);
        //Act
        Func<Task> act = async () => await _sut.GetReportsOnMyCourseAsync(instructorId, courseId);
        //Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You do not have permission to view reports for this course.");
        await _courseRepoMock.Received(1).IsOwnerAsync(instructorId, courseId);
    }

    [Fact]
    public async Task GetReportsOnMyCourseAsync_IsOwner_ReturnsMappedResponses()
    {
        //Arrange 1
        int instructorId = 1;
        int courseId = 10;
        var reports = new System.Collections.Generic.List<CourseReport> { new CourseReport { CourseReportId = 1 } };
        var expected = new System.Collections.Generic.List<CourseReportDetailResponse> { new CourseReportDetailResponse { ReportId = 1 } };
        //Arrange 2
        _courseRepoMock.IsOwnerAsync(instructorId, courseId).Returns(true);
        _reportRepoMock.GetCourseReportsByCourseAsync(courseId).Returns(reports);
        _mapperMock.Map<System.Collections.Generic.IEnumerable<CourseReportDetailResponse>>(reports).Returns(expected);
        //Act
        var result = await _sut.GetReportsOnMyCourseAsync(instructorId, courseId);
        //Assert
        result.Should().BeEquivalentTo(expected);
        await _courseRepoMock.Received(1).IsOwnerAsync(instructorId, courseId);
        await _reportRepoMock.Received(1).GetCourseReportsByCourseAsync(courseId);
    }
}
