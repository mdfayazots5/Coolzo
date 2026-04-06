using Coolzo.Application.Common.Interfaces;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;

namespace Coolzo.Application.Common.Services;

public sealed class GapPhaseAFeatureFlagService
{
    private readonly IGapPhaseARepository _gapPhaseARepository;

    public GapPhaseAFeatureFlagService(IGapPhaseARepository gapPhaseARepository)
    {
        _gapPhaseARepository = gapPhaseARepository;
    }

    public async Task EnsureEnabledAsync(string flagCode, CancellationToken cancellationToken)
    {
        var flag = await _gapPhaseARepository.GetFeatureFlagByCodeAsync(flagCode, cancellationToken);

        if (flag is null || flag.IsEnabled)
        {
            return;
        }

        throw new AppException(ErrorCodes.FeatureDisabled, $"The requested flow is disabled by feature flag {flagCode}.", 409);
    }
}
