using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.CMS;
using Coolzo.Shared.Constants;
using MediatR;

namespace Coolzo.Application.Features.CMS.Theme.Queries.GetTheme;

public sealed record GetThemeQuery : IRequest<ThemeResponse>;

public sealed class GetThemeQueryHandler : IRequestHandler<GetThemeQuery, ThemeResponse>
{
    private readonly ISystemSettingRepository _systemSettingRepository;

    public GetThemeQueryHandler(ISystemSettingRepository systemSettingRepository)
    {
        _systemSettingRepository = systemSettingRepository;
    }

    public async Task<ThemeResponse> Handle(GetThemeQuery request, CancellationToken cancellationToken)
    {
        var stored = await _systemSettingRepository.GetByKeysAsync(SnapshotKeys.Theme.AllKeys, cancellationToken);

        var tokens = SnapshotKeys.Theme.AllKeys.ToDictionary(
            key => key,
            key => stored.TryGetValue(key, out var setting) ? setting.SettingValue : string.Empty);

        return new ThemeResponse(tokens);
    }
}
