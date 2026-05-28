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
    /// Repository for CourseExt entity (course hashes).
    /// </summary>
    public class CourseExtRepository : ICourseExtRepository
    {
        private readonly AppDbContext _context;

        public CourseExtRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CourseExt courseExt)
        {
            await _context.CourseExts.AddAsync(courseExt);
        }

        public void Update(CourseExt courseExt)
        {
            _context.CourseExts.Update(courseExt);
        }

        public void Delete(CourseExt courseExt)
        {
            _context.CourseExts.Remove(courseExt);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<CourseExt?> GetByIdAsync(int courseId)
        {
            return await _context.CourseExts
                .AsNoTracking()
                .FirstOrDefaultAsync(ce => ce.CourseId == courseId);
        }

        public async Task<List<CourseExt>> GetAllAsync()
        {
            return await _context.CourseExts
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
