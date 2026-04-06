using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Technician.Queries.GetTechnicianAvailability;

public sealed class GetTechnicianAvailabilityQueryHandler : IRequestHandler<GetTechnicianAvailabilityQuery, IReadOnlyCollection<TechnicianAvailabilityResponse>>
{
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly ITechnicianRepository _technicianRepository;

    public GetTechnicianAvailabilityQueryHandler(
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _technicianRepository = technicianRepository;
    }

    public async Task<IReadOnlyCollection<TechnicianAvailabilityResponse>> Handle(GetTechnicianAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _serviceRequestRepository.GetByIdAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);

        var availabilitySnapshots = await _technicianRepository.GetAvailabilityByServiceRequestIdAsync(serviceRequest.ServiceRequestId, cancellationToken);

        return availabilitySnapshots
            .Select(snapshot => new TechnicianAvailabilityResponse(
                snapshot.TechnicianId,
                snapshot.TechnicianCode,
                snapshot.TechnicianName,
                snapshot.MobileNumber,
                snapshot.EmailAddress,
                snapshot.BaseZoneName,
                snapshot.AvailableDate,
                snapshot.AvailableSlotCount,
                snapshot.BookedAssignmentCount,
                snapshot.RemainingCapacity,
                snapshot.IsAvailable,
                snapshot.IsSkillMatched,
                BuildAvailabilityMessage(snapshot)))
            .ToArray();
    }

    private static string BuildAvailabilityMessage(TechnicianAvailabilitySnapshot snapshot)
    {
        if (!snapshot.IsSkillMatched)
        {
            return "Skill mapping missing for this service.";
        }

        if (!snapshot.IsAvailable)
        {
            return snapshot.RemainingCapacity == 0
                ? "Capacity full for the requested date."
                : "Technician is marked unavailable.";
        }

        return "Available for assignment.";
    }
}
