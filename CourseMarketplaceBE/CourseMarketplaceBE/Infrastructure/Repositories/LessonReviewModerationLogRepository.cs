using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using CourseMarketplaceBE.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories
{
    public class LessonReviewModerationLogRepository : ILessonReviewModerationLogRepository
    {
        private readonly AppDbContext _context;

        public LessonReviewModerationLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(LessonReviewModerationLog log)
        {
            await _context.LessonReviewModerationLogs.AddAsync(log);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<(List<LessonReviewModerationLog> Items, int TotalCount)> GetPagedAdminAsync(int page, int pageSize)
        {
            var query = _context.LessonReviewModerationLogs
                .Include(l => l.Model)
                .Include(l => l.LessonReview)
                .AsNoTracking();

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(l => l.LogCreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<LessonReviewModerationLog?> GetAdminDetailByIdAsync(int logId)
        {
            var l = await _context.LessonReviewModerationLogs
                .Include(l => l.Model)
                .Include(l => l.LessonReview)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.LogId == logId);

            return l;
        }
    }
}
