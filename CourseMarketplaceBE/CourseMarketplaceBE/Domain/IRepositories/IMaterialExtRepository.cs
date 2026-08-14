using CourseMarketplaceBE.Domain.Entities;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    public interface IMaterialExtRepository
    {
        Task<bool> IsHashExistsAsync(string hash);
        Task<MaterialExt?> GetByMaterialIdAsync(int materialId);
        Task<int> AddMaterialExtAsync(MaterialExt ext);
        Task<int> DeleteByMaterialIdAsync(int materialId);
    }
}
