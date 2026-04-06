using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class SystemSettingRepository : ISystemSettingRepository
{
    private readonly CoolzoDbContext _dbContext;

    public SystemSettingRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SystemSetting?> GetByKeyAsync(string settingKey, CancellationToken cancellationToken)
    {
        return _dbContext.SystemSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(setting => setting.SettingKey == settingKey && !setting.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, SystemSetting>> GetByKeysAsync(IReadOnlyCollection<string> settingKeys, CancellationToken cancellationToken)
    {
        if (settingKeys.Count == 0)
        {
            return new Dictionary<string, SystemSetting>(StringComparer.OrdinalIgnoreCase);
        }

        var keys = settingKeys
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var matchingSettings = await _dbContext.SystemSettings
            .AsNoTracking()
            .Where(setting => !setting.IsDeleted && keys.Contains(setting.SettingKey))
            .ToArrayAsync(cancellationToken);

        return matchingSettings.ToDictionary(setting => setting.SettingKey, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyCollection<SystemSetting>> ListAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.SystemSettings
            .AsNoTracking()
            .Where(setting => !setting.IsDeleted)
            .OrderBy(setting => setting.SettingKey)
            .ToArrayAsync(cancellationToken);
    }
}
