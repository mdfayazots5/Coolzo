using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface ISystemSettingRepository
{
    Task<SystemSetting?> GetByKeyAsync(string settingKey, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<string, SystemSetting>> GetByKeysAsync(IReadOnlyCollection<string> settingKeys, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SystemSetting>> ListAsync(CancellationToken cancellationToken);

    Task<SystemSetting?> GetTrackedByKeyAsync(string settingKey, CancellationToken cancellationToken);

    Task AddAsync(SystemSetting systemSetting, CancellationToken cancellationToken);
}
