using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface IMaterialStreamService
{
    Task<MaterialStreamResult> GetMaterialStreamAsync(int materialId, int userId, string? userRole, string? rangeHeader);
}
