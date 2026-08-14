using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Infrastructure.Repositories
{
    public class MaterialExtRepository : IMaterialExtRepository
    {
        private readonly AppDbContext _context;

        public MaterialExtRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsHashExistsAsync(string hash)
        {
            return await _context.MaterialExts.AnyAsync(m => m.FileHash == hash);
        }

        public async Task<MaterialExt?> GetByMaterialIdAsync(int materialId)
        {
            return await _context.MaterialExts.FirstOrDefaultAsync(m => m.MaterialId == materialId);
        }

        public async Task<int> AddMaterialExtAsync(MaterialExt ext)
        {
            _context.MaterialExts.Add(ext);
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new CourseException("An unexpected database error occurred while saving the material file. Please try again.");
            }
        }

        public async Task<int> DeleteByMaterialIdAsync(int materialId)
        {
            var ext = await _context.MaterialExts.FirstOrDefaultAsync(m => m.MaterialId == materialId);
            if (ext != null)
            {
                _context.MaterialExts.Remove(ext);
                return await _context.SaveChangesAsync();
            }
            return 0;
        }
    }
}
