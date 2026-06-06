using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CourseMarketplaceBE.Share.Helpers;
using Npgsql;

namespace CourseMarketplaceBE.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for CourseExt entity (course hashes).
    /// </summary>
    public class CourseExtRepository : ICourseExtRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CourseExtRepository> _logger;

        public CourseExtRepository(AppDbContext context, ILogger<CourseExtRepository> logger)
        {
            _context = context;
            _logger = logger;
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
            try
            {

                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {

                var jsonString = JsonSerializer.Serialize(
                   new Dictionary<string, object?>
                    {
                        { "constraint", pgEx.ConstraintName },
                        { "table", pgEx.TableName },
                        { "message", pgEx.MessageText }
                    }
                );
                
                _logger.LogError(ex, jsonString);
                throw new CourseExtException(jsonString);
            }

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
