using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Operations;
using MediatR;

namespace Coolzo.Application.Features.Technician.Queries.GetTechnicianList;

public sealed class GetTechnicianListQueryHandler : IRequestHandler<GetTechnicianListQuery, IReadOnlyCollection<TechnicianListItemResponse>>
{
    private readonly ITechnicianRepository _technicianRepository;

    public GetTechnicianListQueryHandler(ITechnicianRepository technicianRepository)
    {
        _technicianRepository = technicianRepository;
    }

    public async Task<IReadOnlyCollection<TechnicianListItemResponse>> Handle(GetTechnicianListQuery request, CancellationToken cancellationToken)
    {
        var technicians = await _technicianRepository.SearchAsync(request.SearchTerm, request.ActiveOnly, cancellationToken);

        return technicians
            .Select(technician => new TechnicianListItemResponse(
                technician.TechnicianId,
                technician.TechnicianCode,
                technician.TechnicianName,
                technician.MobileNumber,
                technician.EmailAddress,
                technician.BaseZone?.ZoneName,
                technician.IsActive,
                technician.MaxDailyAssignments,
                technician.ServiceRequestAssignments.Count(assignment => assignment.IsActiveAssignment && !assignment.IsDeleted)))
            .ToArray();
    }
}
