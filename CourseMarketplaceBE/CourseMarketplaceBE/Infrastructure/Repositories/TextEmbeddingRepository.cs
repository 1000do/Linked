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
    public class TextEmbeddingRepository : ITextEmbeddingRepository
    {
        private readonly AppDbContext _context;

        public TextEmbeddingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TextEmbedding embedding)
        {
            await _context.TextEmbeddings.AddAsync(embedding);
        }

        public void Update(TextEmbedding embedding)
        {
            _context.TextEmbeddings.Update(embedding);
        }

        public void Delete(TextEmbedding embedding)
        {
            _context.TextEmbeddings.Remove(embedding);
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new CourseMarketplaceBE.Domain.Exceptions.TextEmbeddingException("Database operation failed due to a constraint violation or data issue while saving Text Embedding.", ex);
            }
        }

        public async Task<TextEmbedding?> GetByIdAsync(int embeddingId)
        {
            return await _context.TextEmbeddings
                .AsNoTracking()
                .FirstOrDefaultAsync(me => me.TextEmbeddingId == embeddingId);
        }

        public async Task<List<TextEmbedding>> GetAllAsync()
        {
            return await _context.TextEmbeddings
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<TextEmbedding>> GetByMaterialIdAsync(int materialId)
        {
            return await _context.TextEmbeddings
                .AsNoTracking()
                .Where(me => me.MaterialId == materialId)
                .ToListAsync();
        }
    }
}
