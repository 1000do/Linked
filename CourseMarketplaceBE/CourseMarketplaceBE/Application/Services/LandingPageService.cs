using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Application.Services;

public class LandingPageService : ILandingPageService
{
    private readonly AppDbContext _context;

    public LandingPageService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PlatformStatsDto> GetPlatformStatsAsync()
    {
        var totalStudents = await _context.Users.CountAsync();
        var totalCourses = await _context.Courses.CountAsync(c => c.CourseStatus == "published");
        var totalInstructors = await _context.Instructors.CountAsync(i => i.ApprovalStatus == "Approved");
        
        var avgRating = await _context.CourseStats
            .Where(s => s.RatingAverage > 0)
            .Select(s => (decimal)s.RatingAverage)
            .DefaultIfEmpty(0)
            .AverageAsync();

        return new PlatformStatsDto
        {
            TotalStudents = totalStudents,
            TotalCourses = totalCourses,
            TotalInstructors = totalInstructors,
            SatisfactionRate = avgRating > 0 ? Math.Round(avgRating, 1) : 0m
        };
    }
}
