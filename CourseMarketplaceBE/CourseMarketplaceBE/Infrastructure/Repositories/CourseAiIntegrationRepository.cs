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
    /// Repository for CourseAiIntegration entity (formerly AiModelsCourse).
    /// </summary>
    public class CourseAiIntegrationRepository : ICourseAiIntegrationRepository
    {
        private readonly AppDbContext _context;

        public CourseAiIntegrationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CourseAiIntegration integration)
        {
            await _context.CourseAiIntegrations.AddAsync(integration);
        }

        public async Task AddRangeAsync(List<CourseAiIntegration> integrations)
        {
            await _context.CourseAiIntegrations.AddRangeAsync(integrations);
        }

        public void Update(CourseAiIntegration integration)
        {
            _context.CourseAiIntegrations.Update(integration);
        }

        public void Delete(CourseAiIntegration integration)
        {
            _context.CourseAiIntegrations.Remove(integration);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<CourseAiIntegration?> GetByIdAsync(int integrationId)
        {
            return await _context.CourseAiIntegrations
                .AsNoTracking()
                .FirstOrDefaultAsync(cai => cai.Id == integrationId);
        }

        public async Task<List<CourseAiIntegration>> GetByCourseIdAsync(int courseId)
        {
            return await _context.CourseAiIntegrations
                .AsNoTracking()
                .Where(cai => cai.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<List<CourseAiIntegration>> GetByModelIdAsync(int modelId)
        {
            return await _context.CourseAiIntegrations
                .AsNoTracking()
                .Where(cai => cai.ModelId == modelId)
                .ToListAsync();
        }

        public async Task<List<CourseAiIntegration>> GetAllAsync()
        {
            return await _context.CourseAiIntegrations
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CourseAiIntegration?> GetByModelAndCourseAsync(int modelId, int courseId)
        {
            return await _context.CourseAiIntegrations
                .AsNoTracking()
                .FirstOrDefaultAsync(cai => cai.ModelId == modelId && cai.CourseId == courseId);
        }
    }
}
