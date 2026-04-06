using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Enums;

namespace Coolzo.Application.Features.TechnicianJob;

public sealed class TechnicianJobLifecycleResolver
{
    private readonly IAmcRepository _amcRepository;

    public TechnicianJobLifecycleResolver(IAmcRepository amcRepository)
    {
        _amcRepository = amcRepository;
    }

    public async Task<(string LifecycleType, string LifecycleLabel)> ResolveAsync(
        long serviceRequestId,
        CancellationToken cancellationToken)
    {
        var amcVisit = await _amcRepository.GetLinkedAmcVisitByServiceRequestIdAsync(serviceRequestId, cancellationToken);

        if (amcVisit is not null)
        {
            return ("Amc", "AMC Visit");
        }

        var revisitRequest = await _amcRepository.GetLinkedRevisitByServiceRequestIdAsync(serviceRequestId, cancellationToken);

        if (revisitRequest is null)
        {
            return ("Standard", "Standard Visit");
        }

        return revisitRequest.RevisitType switch
        {
            RevisitType.Warranty => ("Warranty", "Warranty Revisit"),
            RevisitType.Amc => ("Amc", "AMC Revisit"),
            RevisitType.Paid => ("PaidRevisit", "Paid Revisit"),
            _ => ("Standard", "Standard Visit")
        };
    }
}
