using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface IAiConfigurationService
{
    Task<AiConfigurationDto> GetConfigurationsAsync();
    Task<bool> UpdateThresholdsAsync(UpdateThresholdsRequest req);
    Task<bool> UpdateIntegrationAsync(UpdateIntegrationRequest req);
}
