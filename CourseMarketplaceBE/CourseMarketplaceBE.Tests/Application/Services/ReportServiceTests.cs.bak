using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class ReportServiceTests
    {
        private readonly IReportRepository _reportRepoMock;
        private readonly IEnrollmentRepository _enrollmentRepoMock;
        private readonly ICourseRepository _courseRepoMock;
        private readonly IReviewRepository _reviewRepoMock;
        private readonly INotificationService _notificationServiceMock;
        private readonly IUserRepository _userRepoMock;
        private readonly IInstructorRepository _instructorRepoMock;
        private readonly IMapper _mapperMock;
        private readonly ILockoutRepository _lockoutRepoMock;
        private readonly IRedisService _redisServiceMock;
        private readonly IHubContext<NotificationHub> _hubContextMock;
        private readonly IHubClients _hubClientsMock;
        private readonly IClientProxy _clientProxyMock;
        private readonly ReportService _sut;

        public ReportServiceTests()
        {
            _reportRepoMock = Substitute.For<IReportRepository>();
            _enrollmentRepoMock = Substitute.For<IEnrollmentRepository>();
            _courseRepoMock = Substitute.For<ICourseRepository>();
            _reviewRepoMock = Substitute.For<IReviewRepository>();
            _notificationServiceMock = Substitute.For<INotificationService>();
            _userRepoMock = Substitute.For<IUserRepository>();
            _instructorRepoMock = Substitute.For<IInstructorRepository>();
            _mapperMock = Substitute.For<IMapper>();
            _lockoutRepoMock = Substitute.For<ILockoutRepository>();
            _redisServiceMock = Substitute.For<IRedisService>();
            _hubContextMock = Substitute.For<IHubContext<NotificationHub>>();
            _hubClientsMock = Substitute.For<IHubClients>();
            _clientProxyMock = Substitute.For<IClientProxy>();

            _hubContextMock.Clients.Returns(_hubClientsMock);
            _hubClientsMock.User(Arg.Any<string>()).Returns(_clientProxyMock);

            _sut = new ReportService(
                _reportRepoMock,
                _enrollmentRepoMock,
                _courseRepoMock,
                _reviewRepoMock,
                _notificationServiceMock,
                _userRepoMock,
                _instructorRepoMock,
                _mapperMock,
                _lockoutRepoMock,
                _redisServiceMock,
                _hubContextMock
            );
        }

        // ── CreateCourseReportAsync ─────────────────────────────────────────

        [Fact]
        public async Task CreateCourseReportAsync_WhenCourseNotFound_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateCourseReportRequest { CourseId = 1, Reason = "Reason" };
            
            //Arrange 2
            _courseRepoMock.GetByIdAsync(req.CourseId).Returns((Course?)null);

            //Act
            Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req, false);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Course not found.");
            
            await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
            await _reportRepoMock.DidNotReceive().AddCourseReportAsync(Arg.Any<CourseReport>());
        }

        [Fact]
        public async Task CreateCourseReportAsync_WhenCourseIsPending_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateCourseReportRequest { CourseId = 1, Reason = "Reason" };
            var course = new Course { CourseId = 1, CourseStatus = CourseStatus.Pending.ToValue() };
            
            //Arrange 2
            _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);

            //Act
            Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req, false);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This course is currently under review and cannot be reported.");
            
            await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
            await _reportRepoMock.DidNotReceive().AddCourseReportAsync(Arg.Any<CourseReport>());
        }

        [Fact]
        public async Task CreateCourseReportAsync_WhenCourseIsArchivedAndFlagged3Times_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateCourseReportRequest { CourseId = 1, Reason = "Reason" };
            var course = new Course { CourseId = 1, CourseStatus = CourseStatus.Archived.ToValue(), CourseFlagCount = 3 };
            
            //Arrange 2
            _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);

            //Act
            Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req, false);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This course is permanently locked and cannot be reported.");
            
            await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
            await _reportRepoMock.DidNotReceive().AddCourseReportAsync(Arg.Any<CourseReport>());
        }

        [Fact]
        public async Task CreateCourseReportAsync_WhenReporterIsInstructorOfCourse_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateCourseReportRequest { CourseId = 1, Reason = "Reason" };
            var course = new Course { CourseId = 1, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = reporterId };
            
            //Arrange 2
            _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);

            //Act
            Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req, true);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You cannot report your own course.");
            
            await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
            await _reportRepoMock.DidNotReceive().AddCourseReportAsync(Arg.Any<CourseReport>());
        }

        [Fact]
        public async Task CreateCourseReportAsync_WhenReporterIsNotEnrolledAndNotInstructor_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateCourseReportRequest { CourseId = 1, Reason = "Reason" };
            var course = new Course { CourseId = 1, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };
            
            //Arrange 2
            _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);
            _enrollmentRepoMock.GetEnrollmentWithProgressAsync(reporterId, req.CourseId).Returns((Enrollment?)null);

            //Act
            Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req, false);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You must be enrolled in the course before reporting it.");
            
            await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
            await _enrollmentRepoMock.Received(1).GetEnrollmentWithProgressAsync(reporterId, req.CourseId);
            await _reportRepoMock.DidNotReceive().AddCourseReportAsync(Arg.Any<CourseReport>());
        }

        [Fact]
        public async Task CreateCourseReportAsync_WhenReasonIsEmpty_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateCourseReportRequest { CourseId = 1, Reason = "" };
            var course = new Course { CourseId = 1, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };
            
            //Arrange 2
            _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);

            //Act
            Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req, true);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Report reason cannot be empty.");
            
            await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
            await _reportRepoMock.DidNotReceive().AddCourseReportAsync(Arg.Any<CourseReport>());
        }

        [Fact]
        public async Task CreateCourseReportAsync_WhenPendingReportAlreadyExists_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateCourseReportRequest { CourseId = 1, Reason = "Reason" };
            var course = new Course { CourseId = 1, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };
            var existingReport = new CourseReport { ReporterId = reporterId, CourseId = 1 };
            
            //Arrange 2
            _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);
            _reportRepoMock.GetPendingCourseReportAsync(reporterId, req.CourseId, req.Reason).Returns(existingReport);

            //Act
            Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req, true);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You already have a pending report with this reason for this course.");
            
            await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
            await _reportRepoMock.Received(1).GetPendingCourseReportAsync(reporterId, req.CourseId, req.Reason);
            await _reportRepoMock.DidNotReceive().AddCourseReportAsync(Arg.Any<CourseReport>());
        }

        [Fact]
        public async Task CreateCourseReportAsync_WhenSaveFails_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateCourseReportRequest { CourseId = 1, Reason = "Reason", Description = "Desc" };
            var course = new Course { CourseId = 1, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };
            
            //Arrange 2
            _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);
            _reportRepoMock.GetPendingCourseReportAsync(reporterId, req.CourseId, req.Reason).Returns((CourseReport?)null);
            _reportRepoMock.SaveChangesAsync().Returns(0);

            //Act
            Func<Task> act = async () => await _sut.CreateCourseReportAsync(reporterId, req, true);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Failed to save changes");
            
            await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
            await _reportRepoMock.Received(1).GetPendingCourseReportAsync(reporterId, req.CourseId, req.Reason);
            await _reportRepoMock.Received(1).AddCourseReportAsync(Arg.Is<CourseReport>(r => r.Reason == req.Reason && r.ReporterId == reporterId));
            await _reportRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CreateCourseReportAsync_WhenValidRequest_ShouldReturnTrueAndCallRepository()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateCourseReportRequest { CourseId = 1, Reason = "Reason", Description = "Desc" };
            var course = new Course { CourseId = 1, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };
            
            //Arrange 2
            _courseRepoMock.GetByIdAsync(req.CourseId).Returns(course);
            _reportRepoMock.GetPendingCourseReportAsync(reporterId, req.CourseId, req.Reason).Returns((CourseReport?)null);
            _reportRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.CreateCourseReportAsync(reporterId, req, true);

            //Assert
            result.Should().BeTrue();
            
            await _courseRepoMock.Received(1).GetByIdAsync(req.CourseId);
            await _reportRepoMock.Received(1).GetPendingCourseReportAsync(reporterId, req.CourseId, req.Reason);
            await _reportRepoMock.Received(1).AddCourseReportAsync(Arg.Is<CourseReport>(r => r.Reason == req.Reason && r.ReporterId == reporterId));
            await _reportRepoMock.Received(1).SaveChangesAsync();
        }

        // ── CreateCourseReviewReportAsync ───────────────────────────────────

        [Fact]
        public async Task CreateCourseReviewReportAsync_WhenReviewNotFound_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateCourseReviewReportRequest { CourseReviewId = 1, Reason = "Reason" };
            
            //Arrange 2
            _reviewRepoMock.GetCourseReviewByIdAsync(req.CourseReviewId).Returns((CourseReview?)null);

            //Act
            Func<Task> act = async () => await _sut.CreateCourseReviewReportAsync(reporterId, req);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Review not found.");
            
            await _reviewRepoMock.Received(1).GetCourseReviewByIdAsync(req.CourseReviewId);
            await _reportRepoMock.DidNotReceive().AddCourseReviewReportAsync(Arg.Any<CourseReviewReport>());
        }

        [Fact]
        public async Task CreateCourseReviewReportAsync_WhenReasonIsEmpty_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateCourseReviewReportRequest { CourseReviewId = 1, Reason = "" };
            var review = new CourseReview { CourseReviewId = 1 };
            
            //Arrange 2
            _reviewRepoMock.GetCourseReviewByIdAsync(req.CourseReviewId).Returns(review);

            //Act
            Func<Task> act = async () => await _sut.CreateCourseReviewReportAsync(reporterId, req);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Report reason cannot be empty.");
            
            await _reviewRepoMock.Received(1).GetCourseReviewByIdAsync(req.CourseReviewId);
            await _reportRepoMock.DidNotReceive().AddCourseReviewReportAsync(Arg.Any<CourseReviewReport>());
        }

        [Fact]
        public async Task CreateCourseReviewReportAsync_WhenPendingReportAlreadyExists_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateCourseReviewReportRequest { CourseReviewId = 1, Reason = "Reason" };
            var review = new CourseReview { CourseReviewId = 1 };
            var existingReport = new CourseReviewReport { ReporterId = reporterId, CourseReviewId = 1 };
            
            //Arrange 2
            _reviewRepoMock.GetCourseReviewByIdAsync(req.CourseReviewId).Returns(review);
            _reportRepoMock.GetPendingCourseReviewReportAsync(reporterId, req.CourseReviewId, req.Reason).Returns(existingReport);

            //Act
            Func<Task> act = async () => await _sut.CreateCourseReviewReportAsync(reporterId, req);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You already have a pending report with this reason for this review.");
            
            await _reviewRepoMock.Received(1).GetCourseReviewByIdAsync(req.CourseReviewId);
            await _reportRepoMock.Received(1).GetPendingCourseReviewReportAsync(reporterId, req.CourseReviewId, req.Reason);
            await _reportRepoMock.DidNotReceive().AddCourseReviewReportAsync(Arg.Any<CourseReviewReport>());
        }

        [Fact]
        public async Task CreateCourseReviewReportAsync_WhenSaveFails_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateCourseReviewReportRequest { CourseReviewId = 1, Reason = "Reason", Description = "Desc" };
            var review = new CourseReview { CourseReviewId = 1 };
            
            //Arrange 2
            _reviewRepoMock.GetCourseReviewByIdAsync(req.CourseReviewId).Returns(review);
            _reportRepoMock.GetPendingCourseReviewReportAsync(reporterId, req.CourseReviewId, req.Reason).Returns((CourseReviewReport?)null);
            _reportRepoMock.SaveChangesAsync().Returns(0);

            //Act
            Func<Task> act = async () => await _sut.CreateCourseReviewReportAsync(reporterId, req);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Failed to save changes");
            
            await _reviewRepoMock.Received(1).GetCourseReviewByIdAsync(req.CourseReviewId);
            await _reportRepoMock.Received(1).GetPendingCourseReviewReportAsync(reporterId, req.CourseReviewId, req.Reason);
            await _reportRepoMock.Received(1).AddCourseReviewReportAsync(Arg.Is<CourseReviewReport>(r => r.Reason == req.Reason && r.ReporterId == reporterId));
            await _reportRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CreateCourseReviewReportAsync_WhenValidRequest_ShouldReturnTrueAndCallRepository()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateCourseReviewReportRequest { CourseReviewId = 1, Reason = "Reason", Description = "Desc" };
            var review = new CourseReview { CourseReviewId = 1 };
            
            //Arrange 2
            _reviewRepoMock.GetCourseReviewByIdAsync(req.CourseReviewId).Returns(review);
            _reportRepoMock.GetPendingCourseReviewReportAsync(reporterId, req.CourseReviewId, req.Reason).Returns((CourseReviewReport?)null);
            _reportRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.CreateCourseReviewReportAsync(reporterId, req);

            //Assert
            result.Should().BeTrue();
            
            await _reviewRepoMock.Received(1).GetCourseReviewByIdAsync(req.CourseReviewId);
            await _reportRepoMock.Received(1).GetPendingCourseReviewReportAsync(reporterId, req.CourseReviewId, req.Reason);
            await _reportRepoMock.Received(1).AddCourseReviewReportAsync(Arg.Is<CourseReviewReport>(r => r.Reason == req.Reason && r.ReporterId == reporterId));
            await _reportRepoMock.Received(1).SaveChangesAsync();
        }

        // ── CreateLessonReviewReportAsync ───────────────────────────────────

        [Fact]
        public async Task CreateLessonReviewReportAsync_WhenReviewNotFound_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateLessonReviewReportRequest { LessonReviewId = 1, Reason = "Reason" };
            
            //Arrange 2
            _reviewRepoMock.GetLessonReviewByIdAsync(req.LessonReviewId).Returns((LessonReview?)null);

            //Act
            Func<Task> act = async () => await _sut.CreateLessonReviewReportAsync(reporterId, req);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Review not found.");
            
            await _reviewRepoMock.Received(1).GetLessonReviewByIdAsync(req.LessonReviewId);
            await _reportRepoMock.DidNotReceive().AddLessonReviewReportAsync(Arg.Any<LessonReviewReport>());
        }

        [Fact]
        public async Task CreateLessonReviewReportAsync_WhenReasonIsEmpty_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateLessonReviewReportRequest { LessonReviewId = 1, Reason = "" };
            var review = new LessonReview { LessonReviewId = 1 };
            
            //Arrange 2
            _reviewRepoMock.GetLessonReviewByIdAsync(req.LessonReviewId).Returns(review);

            //Act
            Func<Task> act = async () => await _sut.CreateLessonReviewReportAsync(reporterId, req);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Report reason cannot be empty.");
            
            await _reviewRepoMock.Received(1).GetLessonReviewByIdAsync(req.LessonReviewId);
            await _reportRepoMock.DidNotReceive().AddLessonReviewReportAsync(Arg.Any<LessonReviewReport>());
        }

        [Fact]
        public async Task CreateLessonReviewReportAsync_WhenPendingReportAlreadyExists_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateLessonReviewReportRequest { LessonReviewId = 1, Reason = "Reason" };
            var review = new LessonReview { LessonReviewId = 1 };
            var existingReport = new LessonReviewReport { ReporterId = reporterId, LessonReviewId = 1 };
            
            //Arrange 2
            _reviewRepoMock.GetLessonReviewByIdAsync(req.LessonReviewId).Returns(review);
            _reportRepoMock.GetPendingLessonReviewReportAsync(reporterId, req.LessonReviewId, req.Reason).Returns(existingReport);

            //Act
            Func<Task> act = async () => await _sut.CreateLessonReviewReportAsync(reporterId, req);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You already have a pending report with this reason for this review.");
            
            await _reviewRepoMock.Received(1).GetLessonReviewByIdAsync(req.LessonReviewId);
            await _reportRepoMock.Received(1).GetPendingLessonReviewReportAsync(reporterId, req.LessonReviewId, req.Reason);
            await _reportRepoMock.DidNotReceive().AddLessonReviewReportAsync(Arg.Any<LessonReviewReport>());
        }

        [Fact]
        public async Task CreateLessonReviewReportAsync_WhenSaveFails_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateLessonReviewReportRequest { LessonReviewId = 1, Reason = "Reason", Description = "Desc" };
            var review = new LessonReview { LessonReviewId = 1 };
            
            //Arrange 2
            _reviewRepoMock.GetLessonReviewByIdAsync(req.LessonReviewId).Returns(review);
            _reportRepoMock.GetPendingLessonReviewReportAsync(reporterId, req.LessonReviewId, req.Reason).Returns((LessonReviewReport?)null);
            _reportRepoMock.SaveChangesAsync().Returns(0);

            //Act
            Func<Task> act = async () => await _sut.CreateLessonReviewReportAsync(reporterId, req);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Failed to save changes");
            
            await _reviewRepoMock.Received(1).GetLessonReviewByIdAsync(req.LessonReviewId);
            await _reportRepoMock.Received(1).GetPendingLessonReviewReportAsync(reporterId, req.LessonReviewId, req.Reason);
            await _reportRepoMock.Received(1).AddLessonReviewReportAsync(Arg.Is<LessonReviewReport>(r => r.Reason == req.Reason && r.ReporterId == reporterId));
            await _reportRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CreateLessonReviewReportAsync_WhenValidRequest_ShouldReturnTrueAndCallRepository()
        {
            //Arrange 1
            int reporterId = 1;
            var req = new CreateLessonReviewReportRequest { LessonReviewId = 1, Reason = "Reason", Description = "Desc" };
            var review = new LessonReview { LessonReviewId = 1 };
            
            //Arrange 2
            _reviewRepoMock.GetLessonReviewByIdAsync(req.LessonReviewId).Returns(review);
            _reportRepoMock.GetPendingLessonReviewReportAsync(reporterId, req.LessonReviewId, req.Reason).Returns((LessonReviewReport?)null);
            _reportRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.CreateLessonReviewReportAsync(reporterId, req);

            //Assert
            result.Should().BeTrue();
            
            await _reviewRepoMock.Received(1).GetLessonReviewByIdAsync(req.LessonReviewId);
            await _reportRepoMock.Received(1).GetPendingLessonReviewReportAsync(reporterId, req.LessonReviewId, req.Reason);
            await _reportRepoMock.Received(1).AddLessonReviewReportAsync(Arg.Is<LessonReviewReport>(r => r.Reason == req.Reason && r.ReporterId == reporterId));
            await _reportRepoMock.Received(1).SaveChangesAsync();
        }

        // ── GetMyCourseReportsAsync & GetMyReviewReportsAsync ───────────────

        [Fact]
        public async Task GetMyCourseReportsAsync_ShouldReturnMappedReports()
        {
            //Arrange 1
            int reporterId = 1;
            var reports = new List<CourseReport> { new CourseReport { CourseReportId = 1 } };
            var dtos = new List<MyCourseReportResponse> { new MyCourseReportResponse { ReportId = 1 } };

            //Arrange 2
            _reportRepoMock.GetCourseReportsByReporterAsync(reporterId).Returns(reports);
            _mapperMock.Map<IEnumerable<MyCourseReportResponse>>(reports).Returns(dtos);

            //Act
            var result = await _sut.GetMyCourseReportsAsync(reporterId);

            //Assert
            result.Should().BeEquivalentTo(dtos);

            await _reportRepoMock.Received(1).GetCourseReportsByReporterAsync(reporterId);
            _mapperMock.Received(1).Map<IEnumerable<MyCourseReportResponse>>(reports);
        }

        [Fact]
        public async Task GetMyReviewReportsAsync_ShouldReturnMergedMappedAndSortedReports()
        {
            //Arrange 1
            int reporterId = 1;
            var courseReviewReports = new List<CourseReviewReport> { new CourseReviewReport { CourseReviewReportId = 1, CreatedAt = DateTime.Now.AddDays(-2) } };
            var lessonReviewReports = new List<LessonReviewReport> { new LessonReviewReport { LessonReviewReportId = 2, CreatedAt = DateTime.Now.AddDays(-1) } };
            var courseDtos = new List<MyReviewReportResponse> { new MyReviewReportResponse { ReportId = 1, CreatedAt = courseReviewReports[0].CreatedAt } };
            var lessonDtos = new List<MyReviewReportResponse> { new MyReviewReportResponse { ReportId = 2, CreatedAt = lessonReviewReports[0].CreatedAt } };

            //Arrange 2
            _reportRepoMock.GetCourseReviewReportsByReporterAsync(reporterId).Returns(courseReviewReports);
            _reportRepoMock.GetLessonReviewReportsByReporterAsync(reporterId).Returns(lessonReviewReports);
            _mapperMock.Map<IEnumerable<MyReviewReportResponse>>(courseReviewReports).Returns(courseDtos);
            _mapperMock.Map<IEnumerable<MyReviewReportResponse>>(lessonReviewReports).Returns(lessonDtos);

            //Act
            var result = await _sut.GetMyReviewReportsAsync(reporterId);

            //Assert
            var expectedList = courseDtos.Concat(lessonDtos).OrderByDescending(r => r.CreatedAt).ToList();
            result.Should().BeEquivalentTo(expectedList, options => options.WithStrictOrdering());

            await _reportRepoMock.Received(1).GetCourseReviewReportsByReporterAsync(reporterId);
            await _reportRepoMock.Received(1).GetLessonReviewReportsByReporterAsync(reporterId);
            _mapperMock.Received(1).Map<IEnumerable<MyReviewReportResponse>>(courseReviewReports);
            _mapperMock.Received(1).Map<IEnumerable<MyReviewReportResponse>>(lessonReviewReports);
        }

        // ── GetReportsOnMyCourseAsync ───────────────────────────────────────

        [Fact]
        public async Task GetReportsOnMyCourseAsync_WhenInstructorNotOwner_ShouldThrowUnauthorizedAccessException()
        {
            //Arrange 1
            int instructorId = 1;
            int courseId = 1;

            //Arrange 2
            _courseRepoMock.IsOwnerAsync(instructorId, courseId).Returns(false);

            //Act
            Func<Task> act = async () => await _sut.GetReportsOnMyCourseAsync(instructorId, courseId);

            //Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You do not have permission to view reports for this course.");

            await _courseRepoMock.Received(1).IsOwnerAsync(instructorId, courseId);
            await _reportRepoMock.DidNotReceive().GetCourseReportsByCourseAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task GetReportsOnMyCourseAsync_WhenInstructorIsOwner_ShouldReturnMappedReports()
        {
            //Arrange 1
            int instructorId = 1;
            int courseId = 1;
            var reports = new List<CourseReport> { new CourseReport { CourseReportId = 1 } };
            var dtos = new List<CourseReportDetailResponse> { new CourseReportDetailResponse { ReportId = 1 } };

            //Arrange 2
            _courseRepoMock.IsOwnerAsync(instructorId, courseId).Returns(true);
            _reportRepoMock.GetCourseReportsByCourseAsync(courseId).Returns(reports);
            _mapperMock.Map<IEnumerable<CourseReportDetailResponse>>(reports).Returns(dtos);

            //Act
            var result = await _sut.GetReportsOnMyCourseAsync(instructorId, courseId);

            //Assert
            result.Should().BeEquivalentTo(dtos);

            await _courseRepoMock.Received(1).IsOwnerAsync(instructorId, courseId);
            await _reportRepoMock.Received(1).GetCourseReportsByCourseAsync(courseId);
            _mapperMock.Received(1).Map<IEnumerable<CourseReportDetailResponse>>(reports);
        }

        // ── GetAll...ReportsAsync & GetReportStatsAsync ─────────────────────

        [Fact]
        public async Task GetAllCourseReportsAsync_ShouldReturnPagedMappedResults()
        {
            //Arrange 1
            string status = "pending";
            int page = 1;
            int pageSize = 10;
            int totalCount = 5;
            var reports = new List<CourseReport> { new CourseReport { CourseReportId = 1 } };
            var dtos = new List<CourseReportDetailResponse> { new CourseReportDetailResponse { ReportId = 1 } };

            //Arrange 2
            _reportRepoMock.GetAllCourseReportsAsync(status, page, pageSize).Returns((reports, totalCount));
            _mapperMock.Map<IEnumerable<CourseReportDetailResponse>>(reports).Returns(dtos);

            //Act
            var result = await _sut.GetAllCourseReportsAsync(status, page, pageSize);

            //Assert
            result.Items.Should().BeEquivalentTo(dtos);
            result.TotalCount.Should().Be(totalCount);

            await _reportRepoMock.Received(1).GetAllCourseReportsAsync(status, page, pageSize);
            _mapperMock.Received(1).Map<IEnumerable<CourseReportDetailResponse>>(reports);
        }

        [Fact]
        public async Task GetAllCourseReviewReportsAsync_ShouldReturnPagedMappedResults()
        {
            //Arrange 1
            string status = "pending";
            int page = 1;
            int pageSize = 10;
            int totalCount = 5;
            var reports = new List<CourseReviewReport> { new CourseReviewReport { CourseReviewReportId = 1 } };
            var dtos = new List<ReviewReportDetailResponse> { new ReviewReportDetailResponse { ReportId = 1 } };

            //Arrange 2
            _reportRepoMock.GetAllCourseReviewReportsAsync(status, page, pageSize).Returns((reports, totalCount));
            _mapperMock.Map<IEnumerable<ReviewReportDetailResponse>>(reports).Returns(dtos);

            //Act
            var result = await _sut.GetAllCourseReviewReportsAsync(status, page, pageSize);

            //Assert
            result.Items.Should().BeEquivalentTo(dtos);
            result.TotalCount.Should().Be(totalCount);

            await _reportRepoMock.Received(1).GetAllCourseReviewReportsAsync(status, page, pageSize);
            _mapperMock.Received(1).Map<IEnumerable<ReviewReportDetailResponse>>(reports);
        }

        [Fact]
        public async Task GetAllLessonReviewReportsAsync_ShouldReturnPagedMappedResults()
        {
            //Arrange 1
            string status = "pending";
            int page = 1;
            int pageSize = 10;
            int totalCount = 5;
            var reports = new List<LessonReviewReport> { new LessonReviewReport { LessonReviewReportId = 1 } };
            var dtos = new List<ReviewReportDetailResponse> { new ReviewReportDetailResponse { ReportId = 1 } };

            //Arrange 2
            _reportRepoMock.GetAllLessonReviewReportsAsync(status, page, pageSize).Returns((reports, totalCount));
            _mapperMock.Map<IEnumerable<ReviewReportDetailResponse>>(reports).Returns(dtos);

            //Act
            var result = await _sut.GetAllLessonReviewReportsAsync(status, page, pageSize);

            //Assert
            result.Items.Should().BeEquivalentTo(dtos);
            result.TotalCount.Should().Be(totalCount);

            await _reportRepoMock.Received(1).GetAllLessonReviewReportsAsync(status, page, pageSize);
            _mapperMock.Received(1).Map<IEnumerable<ReviewReportDetailResponse>>(reports);
        }

        [Fact]
        public async Task GetReportStatsAsync_ShouldCalculateAndReturnCorrectStats()
        {
            //Arrange 1
            var today = DateTime.Today;
            var pendingCourseReports = new List<CourseReport> { new CourseReport { CourseReportId = 1 } };
            var pendingCourseReviewReports = new List<CourseReviewReport> { new CourseReviewReport { CourseReviewReportId = 1 }, new CourseReviewReport { CourseReviewReportId = 2 } };
            var pendingLessonReviewReports = new List<LessonReviewReport>();

            var allCourseReports = new List<CourseReport> { new CourseReport { CourseReportId = 2, CourseReportsStatus = "resolved", ResolvedAt = today } };
            var allCourseReviewReports = new List<CourseReviewReport> { new CourseReviewReport { CourseReviewReportId = 3, UserReportsStatus = "rejected", ResolvedAt = today } };
            var allLessonReviewReports = new List<LessonReviewReport>();

            //Arrange 2
            _reportRepoMock.GetAllCourseReportsAsync("pending", 1, 100000).Returns((pendingCourseReports, 1));
            _reportRepoMock.GetAllCourseReviewReportsAsync("pending", 1, 100000).Returns((pendingCourseReviewReports, 2));
            _reportRepoMock.GetAllLessonReviewReportsAsync("pending", 1, 100000).Returns((pendingLessonReviewReports, 0));

            _reportRepoMock.GetAllCourseReportsAsync(null, 1, 100000).Returns((allCourseReports, 1));
            _reportRepoMock.GetAllCourseReviewReportsAsync(null, 1, 100000).Returns((allCourseReviewReports, 1));
            _reportRepoMock.GetAllLessonReviewReportsAsync(null, 1, 100000).Returns((allLessonReviewReports, 0));

            //Act
            var result = await _sut.GetReportStatsAsync();

            //Assert
            result.TotalPendingCourseReports.Should().Be(1);
            result.TotalPendingCourseReviewReports.Should().Be(2);
            result.TotalPendingLessonReviewReports.Should().Be(0);
            result.TotalResolvedToday.Should().Be(1);
            result.TotalRejectedToday.Should().Be(1);

            await _reportRepoMock.Received(1).GetAllCourseReportsAsync("pending", 1, 100000);
            await _reportRepoMock.Received(1).GetAllCourseReportsAsync(null, 1, 100000);
        }

        // ── ResolveCourseReportAsync ────────────────────────────────────────

        [Fact]
        public async Task ResolveCourseReportAsync_WhenReportNotFound_ShouldReturnFalse()
        {
            //Arrange 1
            int reportId = 1;
            int resolverId = 2;
            var req = new ResolveReportRequest { Status = "resolved" };

            //Arrange 2
            _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns((CourseReport?)null);

            //Act
            var result = await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

            //Assert
            result.Should().BeFalse();

            await _reportRepoMock.Received(1).GetCourseReportByIdAsync(reportId);
            await _reportRepoMock.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task ResolveCourseReportAsync_WhenReportEscalatedAndResolverNotAdmin_ShouldThrowUnauthorizedAccessException()
        {
            //Arrange 1
            int reportId = 1;
            int resolverId = 2;
            var req = new ResolveReportRequest { Status = "resolved" };
            var report = new CourseReport { CourseReportId = reportId, CourseReportsStatus = "escalated" };

            //Arrange 2
            _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
            _userRepoMock.GetRoleByAccountIdAsync(resolverId).Returns("staff");

            //Act
            Func<Task> act = async () => await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

            //Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Only admins can resolve escalated reports.");

            await _reportRepoMock.Received(1).GetCourseReportByIdAsync(reportId);
            await _userRepoMock.Received(1).GetRoleByAccountIdAsync(resolverId);
        }

        [Fact]
        public async Task ResolveCourseReportAsync_WhenStatusResolvedAndRemoveContentAndFlagsLessThan3_ShouldSendWarningAndReturnTrue()
        {
            //Arrange 1
            int reportId = 1;
            int resolverId = 2;
            int instructorId = 3;
            var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), RemoveContent = true, ResolutionNote = "Violation" };
            var report = new CourseReport { CourseReportId = reportId, CourseId = 1, ReporterId = 4 };
            var course = new Course { CourseId = 1, CourseFlagCount = 1, InstructorId = instructorId, Title = "Test Course" };

            //Arrange 2
            _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
            _courseRepoMock.GetByIdAsync(course.CourseId).Returns(course);
            _reportRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

            //Assert
            result.Should().BeTrue();
            report.CourseReportsStatus.Should().Be(req.Status);
            course.CourseFlagCount.Should().Be(2);

            await _reportRepoMock.Received(1).GetCourseReportByIdAsync(reportId);
            await _courseRepoMock.Received(1).GetByIdAsync(course.CourseId);
            await _notificationServiceMock.Received(1).SendNotificationAsync(instructorId, "Course Violation Warning", Arg.Is<string>(s => s.Contains("Strike 2")), Arg.Any<string>());
            _reportRepoMock.Received(1).UpdateCourseReport(report);
            await _reportRepoMock.Received(1).SaveChangesAsync();
            await _redisServiceMock.Received(1).RemoveCacheAsync(Arg.Any<string>());
            await _notificationServiceMock.Received(1).SendNotificationAsync(4, "Report Resolution Update", "Your report has been accepted and appropriate action has been taken.", Arg.Any<string>());
        }

        [Fact]
        public async Task ResolveCourseReportAsync_WhenStatusResolvedAndRemoveContentAndFlagsAre3_ShouldArchiveCourseSuspendInstructorAndReturnTrue()
        {
            //Arrange 1
            int reportId = 1;
            int resolverId = 2;
            int instructorId = 3;
            var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), RemoveContent = true, ResolutionNote = "Violation" };
            var report = new CourseReport { CourseReportId = reportId, CourseId = 1, ReporterId = 4 };
            var course = new Course { CourseId = 1, CourseFlagCount = 2, InstructorId = instructorId, Title = "Test Course" };
            var instructor = new Instructor { InstructorId = instructorId };
            var studentCourses = new List<Course> { new Course { CourseId = 1 } };
            var studentIds = new List<int> { 5 };

            //Arrange 2
            _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
            _courseRepoMock.GetByIdAsync(course.CourseId).Returns(course);
            _instructorRepoMock.GetByIdAsync(instructorId).Returns(instructor);
            _courseRepoMock.GetInstructorCoursesAsync(instructorId).Returns(studentCourses);
            _enrollmentRepoMock.GetEnrolledUserIdsAsync(studentCourses[0].CourseId).Returns(studentIds);
            _reportRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

            //Assert
            result.Should().BeTrue();
            course.CourseStatus.Should().Be(CourseStatus.Archived.ToValue());

            await _lockoutRepoMock.Received(1).AddAsync(Arg.Is<Lockout>(l => l.AccountId == instructorId && l.LockoutType == "instructor"));
            await _notificationServiceMock.Received(1).SendNotificationAsync(5, "Instructor Temporarily Suspended", Arg.Any<string>(), Arg.Any<string>());
            await _notificationServiceMock.Received(1).SendNotificationAsync(instructorId, "Permanent Course Discontinuation Notice", Arg.Any<string>(), Arg.Any<string>());
            _reportRepoMock.Received(1).UpdateCourseReport(report);
            await _reportRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ResolveCourseReportAsync_WhenStatusResolvedAndNoRemoveContent_ShouldSendWarningToInstructorAndReturnTrue()
        {
            //Arrange 1
            int reportId = 1;
            int resolverId = 2;
            int instructorId = 3;
            var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), RemoveContent = false, ResolutionNote = "Warning" };
            var report = new CourseReport { CourseReportId = reportId, CourseId = 1 };
            var course = new Course { CourseId = 1, InstructorId = instructorId, Title = "Test Course" };

            //Arrange 2
            _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
            _courseRepoMock.GetByIdAsync(course.CourseId).Returns(course);
            _reportRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

            //Assert
            result.Should().BeTrue();
            report.CourseReportsStatus.Should().Be(req.Status);

            await _notificationServiceMock.Received(1).SendNotificationAsync(instructorId, "Course Policy Warning", Arg.Is<string>(s => s.Contains("compl")), Arg.Any<string>());
            _reportRepoMock.Received(1).UpdateCourseReport(report);
            await _reportRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ResolveCourseReportAsync_WhenStatusRejected_ShouldNotifyReporterWithDismissalAndReturnTrue()
        {
            //Arrange 1
            int reportId = 1;
            int resolverId = 2;
            var req = new ResolveReportRequest { Status = ReportStatus.Rejected.ToValue(), RemoveContent = false };
            var report = new CourseReport { CourseReportId = reportId, CourseId = 1, ReporterId = 4 };
            var course = new Course { CourseId = 1 };

            //Arrange 2
            _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
            _courseRepoMock.GetByIdAsync(course.CourseId).Returns(course);
            _reportRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

            //Assert
            result.Should().BeTrue();

            await _notificationServiceMock.Received(1).SendNotificationAsync(4, "Report Resolution Update", "No issues found on course, your report has been dismissed.", Arg.Any<string>());
            _reportRepoMock.Received(1).UpdateCourseReport(report);
            await _reportRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ResolveCourseReportAsync_WhenStatusEscalated_ShouldNotifyReporterAndAdminAndReturnTrue()
        {
            //Arrange 1
            int reportId = 1;
            int resolverId = 2;
            var req = new ResolveReportRequest { Status = ReportStatus.Escalated.ToValue(), RemoveContent = false };
            var report = new CourseReport { CourseReportId = reportId, CourseId = 1, ReporterId = 4 };
            var course = new Course { CourseId = 1 };
            int adminId = 99;

            //Arrange 2
            _reportRepoMock.GetCourseReportByIdAsync(reportId).Returns(report);
            _courseRepoMock.GetByIdAsync(course.CourseId).Returns(course);
            _userRepoMock.GetAdminIdAsync().Returns(adminId);
            _reportRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ResolveCourseReportAsync(reportId, resolverId, req);

            //Assert
            result.Should().BeTrue();

            await _notificationServiceMock.Received(1).SendNotificationAsync(4, "Report Resolution Update", "Your report has been escalated to senior administrators.", Arg.Any<string>());
            await _notificationServiceMock.Received(1).SendNotificationAsync(adminId, "Report Escalated", Arg.Any<string>(), Arg.Any<string>());
            _reportRepoMock.Received(1).UpdateCourseReport(report);
            await _reportRepoMock.Received(1).SaveChangesAsync();
        }

        // ── ResolveCourseReviewReportAsync & LessonReviewReportAsync ───────

        [Fact]
        public async Task ResolveCourseReviewReportAsync_WhenReportNotFound_ShouldReturnFalse()
        {
            //Arrange 1
            int reportId = 1;
            int resolverId = 2;
            var req = new ResolveReportRequest { Status = "resolved" };

            //Arrange 2
            _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns((CourseReviewReport?)null);

            //Act
            var result = await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

            //Assert
            result.Should().BeFalse();

            await _reportRepoMock.Received(1).GetCourseReviewReportByIdAsync(reportId);
            await _reportRepoMock.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task ResolveCourseReviewReportAsync_WhenStatusResolvedAndNoRemoveContent_ShouldSendWarningToReviewerAndReturnTrue()
        {
            //Arrange 1
            int reportId = 1;
            int resolverId = 2;
            var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), RemoveContent = false, ResolutionNote = "Warning" };
            var report = new CourseReviewReport { CourseReviewReportId = reportId, CourseReviewId = 1 };
            var review = new CourseReview { CourseReviewId = 1, Enrollment = new Enrollment { UserId = 5 } };

            //Arrange 2
            _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns(report);
            _reviewRepoMock.GetCourseReviewByIdAsync(review.CourseReviewId).Returns(review);
            _reportRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

            //Assert
            result.Should().BeTrue();

            await _notificationServiceMock.Received(1).SendNotificationAsync(5, "Review Policy Warning", Arg.Is<string>(s => s.Contains("guidelines")), Arg.Any<string>());
            _reportRepoMock.Received(1).UpdateCourseReviewReport(report);
            await _reportRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ResolveCourseReviewReportAsync_WhenStatusResolvedAndRemoveContent_FirstStrike_ShouldRemoveReviewAndSendWarning()
        {
            //Arrange 1
            int reportId = 1;
            int resolverId = 2;
            int userId = 5;
            var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), RemoveContent = true, ResolutionNote = "Violation" };
            var report = new CourseReviewReport { CourseReviewReportId = reportId, CourseReviewId = 1 };
            var review = new CourseReview { CourseReviewId = 1, Enrollment = new Enrollment { UserId = userId } };
            var account = new Account { AccountId = userId, AccountFlagCount = 0 };

            //Arrange 2
            _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns(report);
            _reviewRepoMock.GetCourseReviewByIdAsync(review.CourseReviewId).Returns(review);
            _userRepoMock.GetAccountByIdAsync(userId).Returns(account);
            _reportRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

            //Assert
            result.Should().BeTrue();
            review.IsRemoved.Should().BeTrue();
            review.CourseReviewStatus.Should().Be(ReviewStatus.Violating.ToValue());
            account.AccountFlagCount.Should().Be(1);

            _reviewRepoMock.Received(1).UpdateCourseReview(review);
            await _notificationServiceMock.Received(1).SendNotificationAsync(userId, "Community Standards Violation (1st Warning)", Arg.Any<string>(), Arg.Any<string>());
            await _userRepoMock.Received(1).UpdateAccountAsync(account);
            _reportRepoMock.Received(1).UpdateCourseReviewReport(report);
            await _reportRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ResolveCourseReviewReportAsync_WhenStatusResolvedAndRemoveContent_SecondStrike_ShouldRemoveReviewAddModerateLockoutAndSendRestrictionNotice()
        {
            //Arrange 1
            int reportId = 1;
            int resolverId = 2;
            int userId = 5;
            var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), RemoveContent = true, ResolutionNote = "Violation" };
            var report = new CourseReviewReport { CourseReviewReportId = reportId, CourseReviewId = 1 };
            var review = new CourseReview { CourseReviewId = 1, Enrollment = new Enrollment { UserId = userId } };
            var account = new Account { AccountId = userId, AccountFlagCount = 1 };

            //Arrange 2
            _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns(report);
            _reviewRepoMock.GetCourseReviewByIdAsync(review.CourseReviewId).Returns(review);
            _userRepoMock.GetAccountByIdAsync(userId).Returns(account);
            _reportRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

            //Assert
            result.Should().BeTrue();
            account.AccountFlagCount.Should().Be(2);

            await _lockoutRepoMock.Received(1).AddAsync(Arg.Is<Lockout>(l => l.AccountId == userId && l.LockoutLevel == "moderate"));
            await _notificationServiceMock.Received(1).SendNotificationAsync(userId, "Commenting Restricted (2nd Violation)", Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ResolveLessonReviewReportAsync_WhenStatusResolvedAndRemoveContent_ThirdStrike_ShouldRemoveReviewAddSevereLockoutBanAccountAndSendSuspensionNotice()
        {
            //Arrange 1
            int reportId = 1;
            int resolverId = 2;
            int userId = 5;
            var req = new ResolveReportRequest { Status = ReportStatus.Resolved.ToValue(), RemoveContent = true, ResolutionNote = "Violation" };
            var report = new LessonReviewReport { LessonReviewReportId = reportId, LessonReviewId = 1 };
            var review = new LessonReview { LessonReviewId = 1, Enrollment = new Enrollment { UserId = userId } };
            var account = new Account { AccountId = userId, AccountFlagCount = 2 };

            //Arrange 2
            _reportRepoMock.GetLessonReviewReportByIdAsync(reportId).Returns(report);
            _reviewRepoMock.GetLessonReviewByIdAsync(review.LessonReviewId).Returns(review);
            _userRepoMock.GetAccountByIdAsync(userId).Returns(account);
            _reportRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ResolveLessonReviewReportAsync(reportId, resolverId, req);

            //Assert
            result.Should().BeTrue();
            account.AccountFlagCount.Should().Be(3);
            account.AccountStatus.Should().Be(AccountStatus.Banned.ToValue());
            review.IsRemoved.Should().BeTrue();

            await _lockoutRepoMock.Received(1).AddAsync(Arg.Is<Lockout>(l => l.AccountId == userId && l.LockoutLevel == "severe"));
            await _notificationServiceMock.Received(1).SendNotificationAsync(userId, "Account Suspended (3rd Violation)", Arg.Any<string>(), Arg.Any<string>());
            await _clientProxyMock.Received(1).SendCoreAsync("AccountLockedOut", Arg.Any<object[]>());
        }

        [Fact]
        public async Task ResolveCourseReviewReportAsync_WhenStatusEscalated_ShouldNotifyReporterAndAdminAndReturnTrue()
        {
            //Arrange 1
            int reportId = 1;
            int resolverId = 2;
            var req = new ResolveReportRequest { Status = ReportStatus.Escalated.ToValue(), RemoveContent = false };
            var report = new CourseReviewReport { CourseReviewReportId = reportId, CourseReviewId = 1, ReporterId = 4 };
            var review = new CourseReview { CourseReviewId = 1, Enrollment = new Enrollment { CourseId = 10 } };
            int adminId = 99;

            //Arrange 2
            _reportRepoMock.GetCourseReviewReportByIdAsync(reportId).Returns(report);
            _reviewRepoMock.GetCourseReviewByIdAsync(review.CourseReviewId).Returns(review);
            _userRepoMock.GetAdminIdAsync().Returns(adminId);
            _reportRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ResolveCourseReviewReportAsync(reportId, resolverId, req);

            //Assert
            result.Should().BeTrue();

            await _notificationServiceMock.Received(1).SendNotificationAsync(4, "Report Resolution Update", "Your report about the course review has been escalated to senior administrators.", Arg.Any<string>());
            await _notificationServiceMock.Received(1).SendNotificationAsync(adminId, "Report Escalated", Arg.Any<string>(), Arg.Any<string>());
        }

        // ── RemoveCourseAsync ───────────────────────────────────────────────

        [Fact]
        public async Task RemoveCourseAsync_WhenCourseNotFound_ShouldReturnFalse()
        {
            //Arrange 1
            int courseId = 1;
            int adminId = 2;

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns((Course?)null);

            //Act
            var result = await _sut.RemoveCourseAsync(courseId, adminId);

            //Assert
            result.Should().BeFalse();

            await _courseRepoMock.Received(1).GetByIdAsync(courseId);
            await _reportRepoMock.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task RemoveCourseAsync_WhenCourseExists_ShouldSoftDeleteArchiveClearCacheAndReturnTrue()
        {
            //Arrange 1
            int courseId = 1;
            int adminId = 2;
            var course = new Course { CourseId = courseId };

            //Arrange 2
            _courseRepoMock.GetByIdAsync(courseId).Returns(course);
            _reportRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.RemoveCourseAsync(courseId, adminId);

            //Assert
            result.Should().BeTrue();
            course.IsRemoved.Should().BeTrue();
            course.CourseStatus.Should().Be(CourseStatus.Archived.ToValue());

            await _courseRepoMock.Received(1).GetByIdAsync(courseId);
            await _reportRepoMock.Received(1).SaveChangesAsync();
            await _redisServiceMock.Received(1).RemoveCacheAsync(Arg.Any<string>());
        }

        // END_OF_FILE
    }
}

