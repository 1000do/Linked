using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

public class MaterialRepository : IMaterialRepository
{
    private readonly AppDbContext _context;

    public MaterialRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LearningMaterial?> GetByIdAsync(int materialId)
    {
        return await _context.LearningMaterials.FindAsync(materialId);
    }

    public async Task<List<LearningMaterial>> GetMaterialsByLessonIdAsync(int lessonId)
    {
        return await _context.LearningMaterials.Where(m => m.LessonId == lessonId).ToListAsync();
    }

    public async Task AddAsync(LearningMaterial material)
    {
        await _context.LearningMaterials.AddAsync(material);
    }

    public void Update(LearningMaterial material)
    {
        _context.LearningMaterials.Update(material);
    }

    public void Delete(LearningMaterial material)
    {
        _context.LearningMaterials.Remove(material);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
