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
    public class MediaEmbeddingRepository : IMediaEmbeddingRepository
    {
        private readonly AppDbContext _context;

        public MediaEmbeddingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(MediaEmbedding embedding)
        {
            await _context.MediaEmbeddings.AddAsync(embedding);
        }

        public void Update(MediaEmbedding embedding)
        {
            _context.MediaEmbeddings.Update(embedding);
        }

        public void Delete(MediaEmbedding embedding)
        {
            _context.MediaEmbeddings.Remove(embedding);
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new CourseMarketplaceBE.Domain.Exceptions.MediaEmbeddingException("Database operation failed due to a constraint violation or data issue while saving Media Embedding.", ex);
            }
        }

        public async Task<MediaEmbedding?> GetByIdAsync(int embeddingId)
        {
            return await _context.MediaEmbeddings
                .AsNoTracking()
                .FirstOrDefaultAsync(me => me.MediaEmbeddingId == embeddingId);
        }

        public async Task<List<MediaEmbedding>> GetAllAsync()
        {
            return await _context.MediaEmbeddings
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<MediaEmbedding>> GetByMaterialIdAsync(int materialId)
        {
            return await _context.MediaEmbeddings
                .AsNoTracking()
                .Where(me => me.MaterialId == materialId)
                .ToListAsync();
        }
    }
}
