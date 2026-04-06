using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Configuration;
using MediatR;

namespace Coolzo.Application.Features.Configuration.Queries.GetSystemSettings;

public sealed class GetSystemSettingsQueryHandler : IRequestHandler<GetSystemSettingsQuery, IReadOnlyCollection<SystemSettingResponse>>
{
    private readonly ISystemSettingRepository _systemSettingRepository;

    public GetSystemSettingsQueryHandler(ISystemSettingRepository systemSettingRepository)
    {
        _systemSettingRepository = systemSettingRepository;
    }

    public async Task<IReadOnlyCollection<SystemSettingResponse>> Handle(GetSystemSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _systemSettingRepository.ListAsync(cancellationToken);

        return settings
            .Select(
                systemSetting => new SystemSettingResponse(
                    systemSetting.SystemSettingId,
                    systemSetting.SettingKey,
                    systemSetting.IsSensitive ? "******" : systemSetting.SettingValue,
                    systemSetting.DataType,
                    systemSetting.IsSensitive))
            .ToArray();
    }
}
