using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    /// <summary>
    /// Repository interface for SystemConfig entity.
    /// </summary>
    public interface ISystemConfigRepository
    {
        Task<string?> GetValueAsync(string key);
        Task<List<Dictionary<string, object>>> GetAllConfigAsync();
        Task SetConfigByKeyAsync(string key, Dictionary<string, object> value);
        Task UpsertConfigAsync(string configKey, string configValue, string? description = null);
    }
}
