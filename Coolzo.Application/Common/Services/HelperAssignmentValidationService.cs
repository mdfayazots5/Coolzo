using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;

namespace Coolzo.Application.Common.Services;

public sealed class HelperAssignmentValidationService
{
    private readonly IGapPhaseERepository _repository;

    public HelperAssignmentValidationService(IGapPhaseERepository repository)
    {
        _repository = repository;
    }

    public async Task EnsureAssignmentAllowedAsync(HelperProfile helperProfile, long? excludedAssignmentId, CancellationToken cancellationToken)
    {
        if (!helperProfile.ActiveFlag)
        {
            throw new AppException(ErrorCodes.InactiveUser, "The helper profile is inactive.", 409);
        }

        var currentAssignment = await _repository.GetActiveHelperAssignmentAsync(helperProfile.HelperProfileId, asNoTracking: true, cancellationToken);

        if (currentAssignment is not null && (!excludedAssignmentId.HasValue || currentAssignment.HelperAssignmentId != excludedAssignmentId.Value))
        {
            throw new AppException(ErrorCodes.DuplicateAssignment, "The helper already has an active assignment.", 409);
        }
    }
}
