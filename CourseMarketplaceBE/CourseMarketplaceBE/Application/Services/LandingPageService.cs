using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services;

public class LandingPageService : ILandingPageService
{
    private readonly IUserRepository _userRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IInstructorRepository _instructorRepository;

    public LandingPageService(IUserRepository userRepository, ICourseRepository courseRepository, IInstructorRepository instructorRepository)
    {
        _userRepository = userRepository;
        _courseRepository = courseRepository;
        _instructorRepository = instructorRepository;
    }

    public async Task<PlatformStatsDto> GetPlatformStatsAsync()
    {
        var totalStudents = await _userRepository.GetTotalStudentsCountAsync();
        var totalCourses = await _courseRepository.GetTotalPublishedCoursesCountAsync();
        var totalInstructors = await _instructorRepository.GetTotalApprovedInstructorsCountAsync();
        var avgRating = await _courseRepository.GetAveragePlatformRatingAsync();

        return new PlatformStatsDto
        {
            TotalStudents = totalStudents,
            TotalCourses = totalCourses,
            TotalInstructors = totalInstructors,
            SatisfactionRate = avgRating > 0 ? Math.Round(avgRating, 1) : 0m
        };
    }
}
