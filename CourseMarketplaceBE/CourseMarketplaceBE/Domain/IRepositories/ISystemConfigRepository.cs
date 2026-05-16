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
        Task<Dictionary<string, object>?> GetConfigByKeyAsync(string key);
        Task<List<Dictionary<string, object>>> GetAllConfigAsync();
        Task SetConfigByKeyAsync(string key, Dictionary<string, object> value);
    }
}
