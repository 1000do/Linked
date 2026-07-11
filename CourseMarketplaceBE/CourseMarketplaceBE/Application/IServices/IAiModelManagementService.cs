using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;

namespace CourseMarketplaceBE.Application.IServices;

public interface IAiModelManagementService
{
    Task<List<AiModelAdminDto>> GetAllModelsAsync();
    Task<PagedResult<AiModelAdminDto>> GetPagedModelsAsync(PagedRequestDto req);
    Task<AiModelAdminDto> GetModelByIdAsync(int id);
    Task<AiModelAdminDto> AddModelAsync(CreateAiModelRequest req);
    Task<AiModelAdminDto> UpdateModelAsync(int id, UpdateAiModelRequest req);
    Task<bool> ToggleModelStatusAsync(int id);
    Task<List<int>> GetModelIdsByType(string type);
    Task<List<AiModelDto>> GetModelsByTypeAsync(string modelType);
}
