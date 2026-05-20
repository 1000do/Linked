using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of ICourseAiUsageLogRepository interface.
    /// </summary>
    public class CourseAiUsageLogRepository : ICourseAiUsageLogRepository
    {
        private readonly AppDbContext _context;

        public CourseAiUsageLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CourseAiUsageLog log)
        {
            await _context.CourseAiUsageLogs.AddAsync(log);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<CourseAiUsageLog?> GetByIdAsync(int logId)
        {
            return await _context.CourseAiUsageLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LogId == logId);
        }

        public async Task<List<CourseAiUsageLog>> GetByIntegrationIdAsync(int integrationId)
        {
            return await _context.CourseAiUsageLogs
                .AsNoTracking()
                .Where(l => l.IntegrationId == integrationId)
                .ToListAsync();
        }

        public async Task<List<CourseAiUsageLog>> GetAllAsync()
        {
            return await _context.CourseAiUsageLogs
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
