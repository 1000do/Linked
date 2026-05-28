using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context)
    {
        _context = context;
    }

    // ── Course Reports ──────────────────────────────────────────────────────

    public async Task<CourseReport?> GetCourseReportByIdAsync(int reportId)
        => await _context.CourseReports
            .Include(r => r.Reporter)
            .Include(r => r.Course)
            .Include(r => r.Resolver)
            .FirstOrDefaultAsync(r => r.CourseReportId == reportId);

    public async Task<(List<CourseReport> Items, int TotalCount)> GetAllCourseReportsAsync(string? status = null, int page = 1, int pageSize = 10)
    {
        var query = _context.CourseReports
            .Include(r => r.Reporter)
            .Include(r => r.Course)
            .Include(r => r.Resolver)
            .Where(r => status == null || r.CourseReportsStatus == status);
            
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (items, totalCount);
    }

    public async Task<List<CourseReport>> GetCourseReportsByReporterAsync(int reporterId)
        => await _context.CourseReports
            .Include(r => r.Course)
            .Where(r => r.ReporterId == reporterId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<List<CourseReport>> GetCourseReportsByCourseAsync(int courseId)
        => await _context.CourseReports
            .Include(r => r.Reporter)
            .Where(r => r.CourseId == courseId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<CourseReport?> GetPendingCourseReportAsync(int reporterId, int courseId)
        => await _context.CourseReports
            .FirstOrDefaultAsync(r => r.ReporterId == reporterId
                                   && r.CourseId == courseId
                                   && r.CourseReportsStatus == "pending");

    public async Task AddCourseReportAsync(CourseReport report)
        => await _context.CourseReports.AddAsync(report);

    public void UpdateCourseReport(CourseReport report)
        => _context.CourseReports.Update(report);

    // ── Course Review Reports ───────────────────────────────────────────────

    public async Task<CourseReviewReport?> GetCourseReviewReportByIdAsync(int reportId)
        => await _context.CourseReviewReports
            .Include(r => r.Reporter)
            .Include(r => r.CourseReview)
            .Include(r => r.Resolver)
            .FirstOrDefaultAsync(r => r.CourseReviewReportId == reportId);

    public async Task<(List<CourseReviewReport> Items, int TotalCount)> GetAllCourseReviewReportsAsync(string? status = null, int page = 1, int pageSize = 10)
    {
        var query = _context.CourseReviewReports
            .Include(r => r.Reporter)
            .Include(r => r.CourseReview!)
                .ThenInclude(cr => cr.Enrollment)
                    .ThenInclude(e => e.User)
            .Include(r => r.CourseReview!)
                .ThenInclude(cr => cr.Enrollment)
                    .ThenInclude(e => e.Course)
            .Include(r => r.Resolver)
            .Where(r => status == null || r.UserReportsStatus == status);
            
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (items, totalCount);
    }

    public async Task<List<CourseReviewReport>> GetCourseReviewReportsByReporterAsync(int reporterId)
        => await _context.CourseReviewReports
            .Include(r => r.CourseReview)
            .Where(r => r.ReporterId == reporterId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<CourseReviewReport?> GetPendingCourseReviewReportAsync(int reporterId, int reviewId)
        => await _context.CourseReviewReports
            .FirstOrDefaultAsync(r => r.ReporterId == reporterId
                                   && r.CourseReviewId == reviewId
                                   && r.UserReportsStatus == "pending");

    public async Task AddCourseReviewReportAsync(CourseReviewReport report)
        => await _context.CourseReviewReports.AddAsync(report);

    public void UpdateCourseReviewReport(CourseReviewReport report)
        => _context.CourseReviewReports.Update(report);

    // ── Lesson Review Reports ───────────────────────────────────────────────

    public async Task<LessonReviewReport?> GetLessonReviewReportByIdAsync(int reportId)
        => await _context.LessonReviewReports
            .Include(r => r.Reporter)
            .Include(r => r.LessonReview)
            .Include(r => r.Resolver)
            .FirstOrDefaultAsync(r => r.LessonReviewReportId == reportId);

    public async Task<(List<LessonReviewReport> Items, int TotalCount)> GetAllLessonReviewReportsAsync(string? status = null, int page = 1, int pageSize = 10)
    {
        var query = _context.LessonReviewReports
            .Include(r => r.Reporter)
            .Include(r => r.LessonReview!)
                .ThenInclude(lr => lr.Enrollment)
                    .ThenInclude(e => e.User)
            .Include(r => r.LessonReview!)
                .ThenInclude(lr => lr.Lesson!)
                    .ThenInclude(l => l.Course)
            .Include(r => r.Resolver)
            .Where(r => status == null || r.UserReportsStatus == status);
            
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (items, totalCount);
    }

    public async Task<List<LessonReviewReport>> GetLessonReviewReportsByReporterAsync(int reporterId)
        => await _context.LessonReviewReports
            .Include(r => r.LessonReview)
            .Where(r => r.ReporterId == reporterId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<LessonReviewReport?> GetPendingLessonReviewReportAsync(int reporterId, int reviewId)
        => await _context.LessonReviewReports
            .FirstOrDefaultAsync(r => r.ReporterId == reporterId
                                   && r.LessonReviewId == reviewId
                                   && r.UserReportsStatus == "pending");

    public async Task AddLessonReviewReportAsync(LessonReviewReport report)
        => await _context.LessonReviewReports.AddAsync(report);

    public void UpdateLessonReviewReport(LessonReviewReport report)
        => _context.LessonReviewReports.Update(report);

    // ── Shared ──────────────────────────────────────────────────────────────

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
