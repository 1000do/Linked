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
    /// Repository for AiModel entity.
    /// </summary>
    public class AiModelRepository : IAiModelRepository
    {
        private readonly AppDbContext _context;

        public AiModelRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<int>> GetModelIdsByTypeAsync(string modelType)
        {
            return await _context.AiModels
                .AsNoTracking()
                .Where(am => am.ModelType == modelType && am.ModelStatus == "active")
                .Select(am => am.ModelId)
                .ToListAsync();
        }

        public async Task<AiModel?> GetByIdAsync(int modelId)
        {
            return await _context.AiModels
                .AsNoTracking()
                .FirstOrDefaultAsync(am => am.ModelId == modelId);
        }

        public async Task<List<AiModel>> GetByTypeAsync(string modelType)
        {
            return await _context.AiModels
                .AsNoTracking()
                .Where(am => am.ModelType == modelType && am.ModelStatus == "active")
                .ToListAsync();
        }

        public async Task<List<AiModel>> GetModelsByTypeAsync(string modelType)
        {
            return await _context.AiModels
                .AsNoTracking()
                .Where(am => am.ModelType == modelType)
                .ToListAsync();
        }
    }
}
