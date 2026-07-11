using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services;

public class ModerationPenaltyServiceTests
{
    private readonly ICourseRepository _courseRepoMock;
    private readonly IInstructorRepository _instructorRepoMock;
    private readonly ILockoutRepository _lockoutRepoMock;
    private readonly IUserRepository _userRepoMock;
    private readonly INotificationService _notificationServiceMock;
    private readonly IEnrollmentRepository _enrollmentRepoMock;
    private readonly IHubContext<NotificationHub> _hubContextMock;
    private readonly IHubClients _hubClientsMock;
    private readonly IClientProxy _clientProxyMock;
    private readonly ModerationPenaltyService _sut;

    public ModerationPenaltyServiceTests()
    {
        _courseRepoMock = Substitute.For<ICourseRepository>();
        _instructorRepoMock = Substitute.For<IInstructorRepository>();
        _lockoutRepoMock = Substitute.For<ILockoutRepository>();
        _userRepoMock = Substitute.For<IUserRepository>();
        _notificationServiceMock = Substitute.For<INotificationService>();
        _enrollmentRepoMock = Substitute.For<IEnrollmentRepository>();
        _hubContextMock = Substitute.For<IHubContext<NotificationHub>>();
        _hubClientsMock = Substitute.For<IHubClients>();
        _clientProxyMock = Substitute.For<IClientProxy>();
        
        _hubContextMock.Clients.Returns(_hubClientsMock);
        _hubClientsMock.User(Arg.Any<string>()).Returns(_clientProxyMock);

        _sut = new ModerationPenaltyService(
            _courseRepoMock,
            _instructorRepoMock,
            _lockoutRepoMock,
            _userRepoMock,
            _notificationServiceMock,
            _enrollmentRepoMock,
            _hubContextMock
        );
    }

    // ── ProcessCourseStrikeAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessCourseStrikeAsync_CourseIsNull_ReturnsFalse()
    {
        // Arrange 1
        int courseId = 1;
        string resolutionNote = "note";

        // Arrange 2
        _courseRepoMock.GetByIdAsync(courseId).Returns((Course?)null);

        // Act
        var result = await _sut.ProcessCourseStrikeAsync(courseId, resolutionNote);

        // Assert
        result.Should().BeFalse();
        await _courseRepoMock.Received(1).GetByIdAsync(courseId);
    }

    [Fact]
    public async Task ProcessCourseStrikeAsync_UnderThreeStrikes_SendsWarningNotification_ReturnsTrue()
    {
        // Arrange 1
        int courseId = 1;
        var course = new Course { CourseId = courseId, Title = "Test", InstructorId = 2, CourseFlagCount = 1 };
        string resolutionNote = "bad content";

        // Arrange 2
        _courseRepoMock.GetByIdAsync(courseId).Returns(course);

        // Act
        var result = await _sut.ProcessCourseStrikeAsync(courseId, resolutionNote);

        // Assert
        result.Should().BeTrue();
        course.CourseFlagCount.Should().Be(2);

        await _courseRepoMock.Received(1).GetByIdAsync(courseId);
        await _notificationServiceMock.Received(1).SendNotificationAsync(
            2,
            "Course Violation Warning",
            Arg.Is<string>(s => s.Contains("Strike 2") && s.Contains(resolutionNote)),
            "/InstructorCourse/Editor/1"
        );
    }

    [Fact]
    public async Task ProcessCourseStrikeAsync_ThreeStrikes_ArchivesCourse_AddsLockout_SendsSuspensionNotice_ReturnsTrue()
    {
        // Arrange 1
        int courseId = 1;
        var course = new Course { CourseId = courseId, Title = "Test", InstructorId = 2, CourseFlagCount = 2, CourseStatus = "active" };
        var instructor = new Instructor { InstructorId = 2 };
        string resolutionNote = "bad content";

        // Arrange 2
        _courseRepoMock.GetByIdAsync(courseId).Returns(course);
        _instructorRepoMock.GetByIdAsync(2).Returns(instructor);

        // Act
        var result = await _sut.ProcessCourseStrikeAsync(courseId, resolutionNote);

        // Assert
        result.Should().BeTrue();
        course.CourseFlagCount.Should().Be(3);
        course.CourseStatus.Should().Be(CourseStatus.Archived.ToValue());

        await _courseRepoMock.Received(1).GetByIdAsync(courseId);
        await _instructorRepoMock.Received(2).GetByIdAsync(2);
        
        await _lockoutRepoMock.Received(1).AddAsync(Arg.Is<Lockout>(l => 
            l.AccountId == 2 && 
            l.LockoutType == "instructor" && 
            l.LockoutLevel == "severe" &&
            l.LockoutEnd.HasValue && l.LockoutEnd.Value.Date == DateTime.Now.AddDays(30).Date
        ));

        // Note: It calls NotifyStudentsAboutInstructorSuspensionAsync inside which calls _instructorRepoMock.GetByIdAsync(2) again.
        // It's verified below in that method's tests, but we also ensure notification is sent to instructor.
        await _notificationServiceMock.Received(1).SendNotificationAsync(
            2,
            "Permanent Course Discontinuation Notice",
            Arg.Is<string>(s => s.Contains("Strike 3") && s.Contains("archived")),
            "/Course/Details/1"
        );
    }

    // ── ProcessReviewStrikeAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessReviewStrikeAsync_AccountNotFound_ReturnsFalse()
    {
        // Arrange 1
        int userId = 1;
        
        // Arrange 2
        _userRepoMock.GetAccountByIdAsync(userId).Returns((Account?)null);

        // Act
        var result = await _sut.ProcessReviewStrikeAsync(userId, "note", "/link");

        // Assert
        result.Should().BeFalse();
        await _userRepoMock.Received(1).GetAccountByIdAsync(userId);
    }

    [Fact]
    public async Task ProcessReviewStrikeAsync_FirstStrike_SendsWarningNotification_ReturnsTrue()
    {
        // Arrange 1
        int userId = 1;
        var account = new Account { AccountId = userId, AccountFlagCount = 0 };
        
        // Arrange 2
        _userRepoMock.GetAccountByIdAsync(userId).Returns(account);
        _userRepoMock.UpdateAccountAsync(account).Returns(true);

        // Act
        var result = await _sut.ProcessReviewStrikeAsync(userId, "note", "/link");

        // Assert
        result.Should().BeTrue();
        account.AccountFlagCount.Should().Be(1);

        await _notificationServiceMock.Received(1).SendNotificationAsync(
            userId,
            "Community Standards Violation (1st Warning)",
            Arg.Any<string>(),
            "/link"
        );
        await _userRepoMock.Received(1).UpdateAccountAsync(account);
    }

    [Fact]
    public async Task ProcessReviewStrikeAsync_SecondStrike_AddsModerateLockout_SendsRestrictedNotification_ReturnsTrue()
    {
        // Arrange 1
        int userId = 1;
        var account = new Account { AccountId = userId, AccountFlagCount = 1 };
        
        // Arrange 2
        _userRepoMock.GetAccountByIdAsync(userId).Returns(account);
        _userRepoMock.UpdateAccountAsync(account).Returns(true);

        // Act
        var result = await _sut.ProcessReviewStrikeAsync(userId, "note", "/link");

        // Assert
        result.Should().BeTrue();
        account.AccountFlagCount.Should().Be(2);

        await _lockoutRepoMock.Received(1).AddAsync(Arg.Is<Lockout>(l => 
            l.AccountId == userId && 
            l.LockoutType == "review" && 
            l.LockoutLevel == "moderate" &&
            l.LockoutEnd.HasValue && l.LockoutEnd.Value.Date == DateTime.Now.AddDays(7).Date
        ));

        await _notificationServiceMock.Received(1).SendNotificationAsync(
            userId,
            "Commenting Restricted (2nd Violation)",
            Arg.Any<string>(),
            "/link"
        );
        await _userRepoMock.Received(1).UpdateAccountAsync(account);
    }

    [Fact]
    public async Task ProcessReviewStrikeAsync_ThirdStrike_AddsSevereLockout_BansAccount_SuspendsInstructorIfApproved_ReturnsTrue()
    {
        // Arrange 1
        int userId = 1;
        var account = new Account { AccountId = userId, AccountFlagCount = 2, AccountStatus = "active" };
        var instructor = new Instructor { InstructorId = userId, ApprovalStatus = "approved" };
        
        // Arrange 2
        _userRepoMock.GetAccountByIdAsync(userId).Returns(account);
        _userRepoMock.UpdateAccountAsync(account).Returns(true);
        _instructorRepoMock.GetByIdAsync(userId).Returns(instructor);

        // Act
        var result = await _sut.ProcessReviewStrikeAsync(userId, "note", "/link");

        // Assert
        result.Should().BeTrue();
        account.AccountFlagCount.Should().Be(3);
        account.AccountStatus.Should().Be(AccountStatus.Banned.ToValue());

        await _lockoutRepoMock.Received(1).AddAsync(Arg.Is<Lockout>(l => 
            l.AccountId == userId && 
            l.LockoutType == "account" && 
            l.LockoutLevel == "severe" &&
            l.LockoutEnd.HasValue && l.LockoutEnd.Value.Date == DateTime.Now.AddDays(30).Date
        ));

        await _notificationServiceMock.Received(1).SendNotificationAsync(
            userId,
            "Account Suspended (3rd Violation)",
            Arg.Any<string>(),
            "/link"
        );
        await _clientProxyMock.Received(1).SendCoreAsync("AccountLockedOut", Arg.Any<object[]>());
        
        await _userRepoMock.Received(1).UpdateAccountAsync(account);
        await _instructorRepoMock.Received(2).GetByIdAsync(userId);
    }

    // ── NotifyStudentsAboutInstructorSuspensionAsync ──────────────────────────────────────────

    [Fact]
    public async Task NotifyStudentsAboutInstructorSuspensionAsync_InstructorNotFound_ReturnsFalse()
    {
        // Arrange 1
        int instructorId = 1;

        // Arrange 2
        _instructorRepoMock.GetByIdAsync(instructorId).Returns((Instructor?)null);

        // Act
        var result = await _sut.NotifyStudentsAboutInstructorSuspensionAsync(instructorId);

        // Assert
        result.Should().BeFalse();
        await _instructorRepoMock.Received(1).GetByIdAsync(instructorId);
        await _courseRepoMock.DidNotReceive().GetInstructorCoursesAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task NotifyStudentsAboutInstructorSuspensionAsync_InstructorHasNoCourses_ReturnsFalse()
    {
        // Arrange 1
        int instructorId = 1;
        var instructor = new Instructor { InstructorId = instructorId };

        // Arrange 2
        _instructorRepoMock.GetByIdAsync(instructorId).Returns(instructor);
        _courseRepoMock.GetInstructorCoursesAsync(instructorId).Returns(new List<Course>());

        // Act
        var result = await _sut.NotifyStudentsAboutInstructorSuspensionAsync(instructorId);

        // Assert
        result.Should().BeFalse();
        await _instructorRepoMock.Received(1).GetByIdAsync(instructorId);
        await _courseRepoMock.Received(1).GetInstructorCoursesAsync(instructorId);
    }

    [Fact]
    public async Task NotifyStudentsAboutInstructorSuspensionAsync_InstructorHasCourses_NotifiesUniqueStudents_ReturnsTrue()
    {
        // Arrange 1
        int instructorId = 1;
        var instructor = new Instructor { InstructorId = instructorId };
        var courses = new List<Course> 
        { 
            new Course { CourseId = 10 },
            new Course { CourseId = 20 }
        };
        var studentsCourse10 = new List<int> { 100, 101 };
        var studentsCourse20 = new List<int> { 101, 102 }; // 101 is enrolled in both

        // Arrange 2
        _instructorRepoMock.GetByIdAsync(instructorId).Returns(instructor);
        _courseRepoMock.GetInstructorCoursesAsync(instructorId).Returns(courses);
        _enrollmentRepoMock.GetEnrolledUserIdsAsync(10).Returns(studentsCourse10);
        _enrollmentRepoMock.GetEnrolledUserIdsAsync(20).Returns(studentsCourse20);

        // Act
        var result = await _sut.NotifyStudentsAboutInstructorSuspensionAsync(instructorId);

        // Assert
        result.Should().BeTrue();
        await _instructorRepoMock.Received(1).GetByIdAsync(instructorId);
        await _courseRepoMock.Received(1).GetInstructorCoursesAsync(instructorId);
        await _enrollmentRepoMock.Received(1).GetEnrolledUserIdsAsync(10);
        await _enrollmentRepoMock.Received(1).GetEnrolledUserIdsAsync(20);

        // User 101 should only be notified once despite being in both courses
        await _notificationServiceMock.Received(1).SendNotificationAsync(100, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        await _notificationServiceMock.Received(1).SendNotificationAsync(101, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        await _notificationServiceMock.Received(1).SendNotificationAsync(102, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ProcessCourseStrikeAsync_AlreadyAtThreeStrikes_CapsAtThree_ArchivesCourse_ReturnsTrue()
    {
        // Arrange 1
        int courseId = 1;
        var course = new Course { CourseId = courseId, Title = "Test", InstructorId = 2, CourseFlagCount = 3, CourseStatus = "active" };
        var instructor = new Instructor { InstructorId = 2 };
        string resolutionNote = "bad content";

        // Arrange 2
        _courseRepoMock.GetByIdAsync(courseId).Returns(course);
        _instructorRepoMock.GetByIdAsync(2).Returns(instructor);

        // Act
        var result = await _sut.ProcessCourseStrikeAsync(courseId, resolutionNote);

        // Assert
        result.Should().BeTrue();
        course.CourseFlagCount.Should().Be(3);
        course.CourseStatus.Should().Be(CourseStatus.Archived.ToValue());

        await _courseRepoMock.Received(1).GetByIdAsync(courseId);
        await _instructorRepoMock.Received(2).GetByIdAsync(2);
        
        await _lockoutRepoMock.Received(1).AddAsync(Arg.Any<Lockout>());
        await _notificationServiceMock.Received(1).SendNotificationAsync(
            2,
            "Permanent Course Discontinuation Notice",
            Arg.Any<string>(),
            "/Course/Details/1"
        );
    }

    [Fact]
    public async Task ProcessCourseStrikeAsync_UnderThreeStrikes_NoInstructorId_ReturnsTrue_DoesNotSendWarning()
    {
        // Arrange 1
        int courseId = 1;
        var course = new Course { CourseId = courseId, Title = "Test", InstructorId = null, CourseFlagCount = 1 };
        string resolutionNote = "bad content";

        // Arrange 2
        _courseRepoMock.GetByIdAsync(courseId).Returns(course);

        // Act
        var result = await _sut.ProcessCourseStrikeAsync(courseId, resolutionNote);

        // Assert
        result.Should().BeTrue();
        course.CourseFlagCount.Should().Be(2);

        await _courseRepoMock.Received(1).GetByIdAsync(courseId);
        await _notificationServiceMock.DidNotReceive().SendNotificationAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ProcessCourseStrikeAsync_ThreeStrikes_NoInstructorId_ReturnsTrue_DoesNotAddLockoutOrNotice()
    {
        // Arrange 1
        int courseId = 1;
        var course = new Course { CourseId = courseId, Title = "Test", InstructorId = null, CourseFlagCount = 2, CourseStatus = "active" };
        string resolutionNote = "bad content";

        // Arrange 2
        _courseRepoMock.GetByIdAsync(courseId).Returns(course);

        // Act
        var result = await _sut.ProcessCourseStrikeAsync(courseId, resolutionNote);

        // Assert
        result.Should().BeTrue();
        course.CourseFlagCount.Should().Be(3);
        course.CourseStatus.Should().Be(CourseStatus.Archived.ToValue());

        await _courseRepoMock.Received(1).GetByIdAsync(courseId);
        await _instructorRepoMock.DidNotReceive().GetByIdAsync(Arg.Any<int>());
        await _lockoutRepoMock.DidNotReceive().AddAsync(Arg.Any<Lockout>());
        await _notificationServiceMock.DidNotReceive().SendNotificationAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ProcessCourseStrikeAsync_ThreeStrikes_InstructorNotFound_ReturnsTrue_SendsNoticeButNoLockout()
    {
        // Arrange 1
        int courseId = 1;
        var course = new Course { CourseId = courseId, Title = "Test", InstructorId = 2, CourseFlagCount = 2, CourseStatus = "active" };
        string resolutionNote = "bad content";

        // Arrange 2
        _courseRepoMock.GetByIdAsync(courseId).Returns(course);
        _instructorRepoMock.GetByIdAsync(2).Returns((Instructor?)null);

        // Act
        var result = await _sut.ProcessCourseStrikeAsync(courseId, resolutionNote);

        // Assert
        result.Should().BeTrue();
        course.CourseFlagCount.Should().Be(3);
        course.CourseStatus.Should().Be(CourseStatus.Archived.ToValue());

        await _courseRepoMock.Received(1).GetByIdAsync(courseId);
        await _instructorRepoMock.Received(1).GetByIdAsync(2);
        
        await _lockoutRepoMock.DidNotReceive().AddAsync(Arg.Any<Lockout>());
        await _notificationServiceMock.Received(1).SendNotificationAsync(
            2,
            "Permanent Course Discontinuation Notice",
            Arg.Any<string>(),
            "/Course/Details/1"
        );
    }

    [Fact]
    public async Task ProcessReviewStrikeAsync_AlreadyAtThreeStrikes_CapsAtThree_BansAccount_ReturnsTrue()
    {
        // Arrange 1
        int userId = 1;
        var account = new Account { AccountId = userId, AccountFlagCount = 3, AccountStatus = "active" };
        var instructor = new Instructor { InstructorId = userId, ApprovalStatus = "approved" };
        
        // Arrange 2
        _userRepoMock.GetAccountByIdAsync(userId).Returns(account);
        _userRepoMock.UpdateAccountAsync(account).Returns(true);
        _instructorRepoMock.GetByIdAsync(userId).Returns(instructor);

        // Act
        var result = await _sut.ProcessReviewStrikeAsync(userId, "note", "/link");

        // Assert
        result.Should().BeTrue();
        account.AccountFlagCount.Should().Be(3);
        account.AccountStatus.Should().Be(AccountStatus.Banned.ToValue());

        await _lockoutRepoMock.Received(1).AddAsync(Arg.Any<Lockout>());
        await _notificationServiceMock.Received(1).SendNotificationAsync(userId, "Account Suspended (3rd Violation)", Arg.Any<string>(), "/link");
        await _clientProxyMock.Received(1).SendCoreAsync("AccountLockedOut", Arg.Any<object[]>());
        
        await _userRepoMock.Received(1).UpdateAccountAsync(account);
    }

    [Fact]
    public async Task ProcessReviewStrikeAsync_ThirdStrike_InstructorNull_ReturnsTrue_NoStudentSuspensionNotice()
    {
        // Arrange 1
        int userId = 1;
        var account = new Account { AccountId = userId, AccountFlagCount = 2, AccountStatus = "active" };
        
        // Arrange 2
        _userRepoMock.GetAccountByIdAsync(userId).Returns(account);
        _userRepoMock.UpdateAccountAsync(account).Returns(true);
        _instructorRepoMock.GetByIdAsync(userId).Returns((Instructor?)null);

        // Act
        var result = await _sut.ProcessReviewStrikeAsync(userId, "note", "/link");

        // Assert
        result.Should().BeTrue();
        account.AccountFlagCount.Should().Be(3);
        account.AccountStatus.Should().Be(AccountStatus.Banned.ToValue());

        await _lockoutRepoMock.Received(1).AddAsync(Arg.Any<Lockout>());
        await _notificationServiceMock.Received(1).SendNotificationAsync(userId, "Account Suspended (3rd Violation)", Arg.Any<string>(), "/link");
        await _clientProxyMock.Received(1).SendCoreAsync("AccountLockedOut", Arg.Any<object[]>());
        
        await _userRepoMock.Received(1).UpdateAccountAsync(account);
        await _instructorRepoMock.Received(1).GetByIdAsync(userId);
    }

    [Fact]
    public async Task ProcessReviewStrikeAsync_ThirdStrike_InstructorNotApproved_ReturnsTrue_NoStudentSuspensionNotice()
    {
        // Arrange 1
        int userId = 1;
        var account = new Account { AccountId = userId, AccountFlagCount = 2, AccountStatus = "active" };
        var instructor = new Instructor { InstructorId = userId, ApprovalStatus = "pending" };
        
        // Arrange 2
        _userRepoMock.GetAccountByIdAsync(userId).Returns(account);
        _userRepoMock.UpdateAccountAsync(account).Returns(true);
        _instructorRepoMock.GetByIdAsync(userId).Returns(instructor);

        // Act
        var result = await _sut.ProcessReviewStrikeAsync(userId, "note", "/link");

        // Assert
        result.Should().BeTrue();
        account.AccountFlagCount.Should().Be(3);
        account.AccountStatus.Should().Be(AccountStatus.Banned.ToValue());

        await _lockoutRepoMock.Received(1).AddAsync(Arg.Any<Lockout>());
        await _notificationServiceMock.Received(1).SendNotificationAsync(userId, "Account Suspended (3rd Violation)", Arg.Any<string>(), "/link");
        await _clientProxyMock.Received(1).SendCoreAsync("AccountLockedOut", Arg.Any<object[]>());
        
        await _userRepoMock.Received(1).UpdateAccountAsync(account);
        await _instructorRepoMock.Received(1).GetByIdAsync(userId);
    }

    [Fact]
    public async Task NotifyStudentsAboutInstructorSuspensionAsync_CoursesIsNull_ReturnsFalse()
    {
        // Arrange 1
        int instructorId = 1;
        var instructor = new Instructor { InstructorId = instructorId };

        // Arrange 2
        _instructorRepoMock.GetByIdAsync(instructorId).Returns(instructor);
        _courseRepoMock.GetInstructorCoursesAsync(instructorId).Returns((IEnumerable<Course>?)null);

        // Act
        var result = await _sut.NotifyStudentsAboutInstructorSuspensionAsync(instructorId);

        // Assert
        result.Should().BeFalse();
        await _instructorRepoMock.Received(1).GetByIdAsync(instructorId);
        await _courseRepoMock.Received(1).GetInstructorCoursesAsync(instructorId);
    }

    [Fact]
    public async Task NotifyStudentsAboutInstructorSuspensionAsync_CoursesHaveNoStudents_ReturnsFalse()
    {
        // Arrange 1
        int instructorId = 1;
        var instructor = new Instructor { InstructorId = instructorId };
        var courses = new List<Course> { new Course { CourseId = 10 } };
        var students = new List<int>(); // Empty

        // Arrange 2
        _instructorRepoMock.GetByIdAsync(instructorId).Returns(instructor);
        _courseRepoMock.GetInstructorCoursesAsync(instructorId).Returns(courses);
        _enrollmentRepoMock.GetEnrolledUserIdsAsync(10).Returns(students);

        // Act
        var result = await _sut.NotifyStudentsAboutInstructorSuspensionAsync(instructorId);

        // Assert
        result.Should().BeFalse();
        await _instructorRepoMock.Received(1).GetByIdAsync(instructorId);
        await _courseRepoMock.Received(1).GetInstructorCoursesAsync(instructorId);
        await _enrollmentRepoMock.Received(1).GetEnrolledUserIdsAsync(10);
        await _notificationServiceMock.DidNotReceive().SendNotificationAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }
}
