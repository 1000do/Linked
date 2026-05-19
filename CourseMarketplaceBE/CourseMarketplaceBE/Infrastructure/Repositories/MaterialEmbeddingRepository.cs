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
    /// Repository for MaterialEmbedding entity.
    /// </summary>
    public class MaterialEmbeddingRepository : IMaterialEmbeddingRepository
    {
        private readonly AppDbContext _context;

        public MaterialEmbeddingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(MaterialEmbedding embedding)
        {
            await _context.MaterialEmbeddings.AddAsync(embedding);
        }

        public void Update(MaterialEmbedding embedding)
        {
            _context.MaterialEmbeddings.Update(embedding);
        }

        public void Delete(MaterialEmbedding embedding)
        {
            _context.MaterialEmbeddings.Remove(embedding);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<MaterialEmbedding?> GetByIdAsync(int embeddingId)
        {
            return await _context.MaterialEmbeddings
                .AsNoTracking()
                .FirstOrDefaultAsync(me => me.EmbeddingId == embeddingId);
        }

        public async Task<List<MaterialEmbedding>> GetAllAsync()
        {
            return await _context.MaterialEmbeddings
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<MaterialEmbedding>> GetByMaterialIdAsync(int materialId)
        {
            return await _context.MaterialEmbeddings
                .AsNoTracking()
                .Where(me => me.MaterialId == materialId)
                .ToListAsync();
        }
    }
}
