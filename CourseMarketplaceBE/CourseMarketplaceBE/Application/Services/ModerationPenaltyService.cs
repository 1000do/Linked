using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CourseMarketplaceBE.Application.Services;

public class ModerationPenaltyService : IModerationPenaltyService
{
    private readonly ICourseRepository _courseRepo;
    private readonly IInstructorRepository _instructorRepo;
    private readonly ILockoutRepository _lockoutRepo;
    private readonly IUserRepository _userRepo;
    private readonly INotificationService _notificationService;
    private readonly IEnrollmentRepository _enrollmentRepo;
    private readonly IHubContext<NotificationHub> _hubContext;

    public ModerationPenaltyService(
        ICourseRepository courseRepo,
        IInstructorRepository instructorRepo,
        ILockoutRepository lockoutRepo,
        IUserRepository userRepo,
        INotificationService notificationService,
        IEnrollmentRepository enrollmentRepo,
        IHubContext<NotificationHub> hubContext)
    {
        _courseRepo = courseRepo;
        _instructorRepo = instructorRepo;
        _lockoutRepo = lockoutRepo;
        _userRepo = userRepo;
        _notificationService = notificationService;
        _enrollmentRepo = enrollmentRepo;
        _hubContext = hubContext;
    }

    public async Task<bool> ProcessCourseStrikeAsync(int courseId, string resolutionNote)
    {
        var course = await _courseRepo.GetByIdAsync(courseId);
        if (course == null) return false;

        var currentFlags = (course.CourseFlagCount ?? 0) + 1;
        course.CourseFlagCount = currentFlags;

        if (currentFlags < 3)
        {
            if (course.InstructorId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    "Course Violation Warning",
                    $"Your course '{course.Title}' has received a policy violation warning. This is Strike {currentFlags}. Reason: {resolutionNote}. Please review and correct the content.",
                    $"/InstructorCourse/Editor/{course.CourseId}"
                );
            }
        }
        else if (currentFlags >= 3)
        {
            course.CourseStatus = CourseStatus.Archived.ToValue();
            if (course.InstructorId.HasValue)
            {
                var instructor = await _instructorRepo.GetByIdAsync(course.InstructorId.Value);
                if (instructor != null)
                {
                    await _lockoutRepo.AddAsync(new Lockout
                    {
                        AccountId = instructor.InstructorId,
                        LockoutType = "instructor",
                        LockoutLevel = "severe",
                        LockoutEnd = DateTime.Now.AddDays(30)
                    });
                    await _lockoutRepo.SaveChangesAsync();
                    await NotifyStudentsAboutInstructorSuspensionAsync(course.InstructorId.Value);
                }
                
                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    "Permanent Course Discontinuation Notice",
                    $"Your course '{course.Title}' has violated our policies. It has been permanently discontinued and archived (Strike {currentFlags}). New enrollments are disabled. Furthermore, your instructor rights are locked for 30 days (you cannot create, update, or delete courses, lessons, and materials).",
                    $"/Course/Details/{course.CourseId}"
                );
            }
        }
        return true;
    }

    public async Task<bool> ProcessReviewStrikeAsync(int userId, string resolutionNote, string? linkAction)
    {
        var account = await _userRepo.GetAccountByIdAsync(userId);
        if (account == null) return false;

        account.AccountFlagCount = (account.AccountFlagCount ?? 0) + 1;

        if (account.AccountFlagCount == 1)
        {
            await _notificationService.SendNotificationAsync(userId, "Community Standards Violation (1st Warning)", "Your comment has been removed for violating community standards. This is your first warning.", linkAction!);
        }
        else if (account.AccountFlagCount == 2)
        {
            await _lockoutRepo.AddAsync(new Lockout
            {
                AccountId = userId,
                LockoutType = "review",
                LockoutLevel = "moderate",
                LockoutEnd = DateTime.Now.AddDays(7)
            });
            await _lockoutRepo.SaveChangesAsync();
            await _notificationService.SendNotificationAsync(userId, "Commenting Restricted (2nd Violation)", "Due to repeated violations, you are restricted from posting comments or reviews for 7 days.", linkAction!);
        }
        else if (account.AccountFlagCount >= 3)
        {
            await _lockoutRepo.AddAsync(new Lockout
            {
                AccountId = userId,
                LockoutType = "account",
                LockoutLevel = "severe",
                LockoutEnd = DateTime.Now.AddDays(30)
            });
            await _lockoutRepo.SaveChangesAsync();
            account.AccountStatus = AccountStatus.Banned.ToValue();
            await _notificationService.SendNotificationAsync(userId, "Account Suspended (3rd Violation)", "Your account has been suspended for 30 days due to repeated and severe community standards violations.", linkAction!);
            await _hubContext.Clients.User(userId.ToString()).SendAsync("AccountLockedOut");
            var inst = await _instructorRepo.GetByIdAsync(userId);
            if (inst != null && string.Equals(inst.ApprovalStatus, InstructorApprovalStatus.Approved.ToValue(), StringComparison.OrdinalIgnoreCase))
            {
                await NotifyStudentsAboutInstructorSuspensionAsync(userId);
            }
        }
        return await _userRepo.UpdateAccountAsync(account);
    }

    public async Task<bool> NotifyStudentsAboutInstructorSuspensionAsync(int instructorId)
    {
        var instructor = await _instructorRepo.GetByIdAsync(instructorId);
        if (instructor == null) return false;

        var courses = await _courseRepo.GetInstructorCoursesAsync(instructorId);
        if (courses == null || !courses.Any()) return false;

        var notifiedUserIds = new HashSet<int>();
        bool sentAny = false;
        foreach (var c in courses)
        {
            var studentIds = await _enrollmentRepo.GetEnrolledUserIdsAsync(c.CourseId);
            foreach (var sId in studentIds)
            {
                if (notifiedUserIds.Add(sId))
                {
                    await _notificationService.SendNotificationAsync(
                        sId,
                        "Instructor Temporarily Suspended",
                        "This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.",
                        null!
                    );
                    sentAny = true;
                }
            }
        }
        return sentAny;
    }
}
