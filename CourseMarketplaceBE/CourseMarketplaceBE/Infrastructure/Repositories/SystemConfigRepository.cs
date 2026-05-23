using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for SystemConfig entity.
    /// </summary>
    public class SystemConfigRepository : ISystemConfigRepository
    {
        private readonly AppDbContext _context;

        public SystemConfigRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetValueAsync(string key)
        {
            var config = await _context.SystemConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(sc => sc.ConfigKey == key);
            return config?.ConfigValue;
        }

        

        public async Task<List<Dictionary<string, object>>> GetAllConfigAsync()
        {
            var configs = await _context.SystemConfigs
                .AsNoTracking()
                .ToListAsync();

            var result = new List<Dictionary<string, object>>();
            foreach (var config in configs)
            {
                try
                {
                    if (string.IsNullOrEmpty(config.ConfigValue))
                    {
                        result.Add(new Dictionary<string, object> { { config.ConfigKey, new Dictionary<string, object>() } });
                    }
                    else
                    {
                        var parsed = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(config.ConfigValue);
                        result.Add(new Dictionary<string, object> { { config.ConfigKey, parsed ?? new Dictionary<string, object>() } });
                    }
                }
                catch
                {
                    result.Add(new Dictionary<string, object> { { config.ConfigKey, config.ConfigValue } });
                }
            }
            return result;
        }

        public async Task SetConfigByKeyAsync(string key, Dictionary<string, object> value)
        {
            var config = await _context.SystemConfigs
                .FirstOrDefaultAsync(sc => sc.ConfigKey == key);

            var jsonString = System.Text.Json.JsonSerializer.Serialize(value);

            if (config == null)
            {
                config = new Domain.Entities.SystemConfig
                {
                    ConfigKey = key,
                    ConfigValue = jsonString,
                    Description = $"System configuration for {key}"
                };
                await _context.SystemConfigs.AddAsync(config);
            }
            else
            {
                config.ConfigValue = jsonString;
                _context.SystemConfigs.Update(config);
            }

            await _context.SaveChangesAsync();
        }
    }
}
