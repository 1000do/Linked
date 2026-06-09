using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Constants;
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
                .Where(am => am.ModelType == modelType && am.ModelStatus == AiModelConst.Active)
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
                .Where(am => am.ModelType == modelType && am.ModelStatus == AiModelConst.Active)
                .ToListAsync();
        }

        public async Task<List<AiModel>> GetModelsByTypeAsync(string modelType)
        {
            return await _context.AiModels
                .AsNoTracking()
                .Where(am => am.ModelType == modelType)
                .ToListAsync();
        }

        public async Task<CourseMarketplaceBE.Application.DTOs.AiModelDto?> GetByModelPathAsync(string modelPath)
        {
            var m = await _context.AiModels
                .AsNoTracking()
                .FirstOrDefaultAsync(am => am.ModelPath == modelPath);

            if (m == null) return null;

            return new CourseMarketplaceBE.Application.DTOs.AiModelDto
            {
                ModelId = m.ModelId,
                ModelName = m.ModelName,
                ModelType = m.ModelType,
                ModelProvider = m.ModelProvider,
                ModelVersion = m.ModelVersion,
                ModelStatus = m.ModelStatus,
                Description = m.Description,
                ModelPath = m.ModelPath,
                ProcessType = m.ProcessType
            };
        }
    }
}
