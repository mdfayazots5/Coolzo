using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;

namespace Coolzo.Application.Common.Services;

public sealed class TechnicianJobAccessService : ITechnicianJobAccessService
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly ITechnicianRepository _technicianRepository;

    public TechnicianJobAccessService(
        ITechnicianRepository technicianRepository,
        IServiceRequestRepository serviceRequestRepository,
        ICurrentUserContext currentUserContext)
    {
        _technicianRepository = technicianRepository;
        _serviceRequestRepository = serviceRequestRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<Technician> GetCurrentTechnicianAsync(CancellationToken cancellationToken)
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
        {
            throw new AppException(ErrorCodes.Unauthorized, "The technician session is invalid.", 401);
        }

        var technician = await _technicianRepository.GetByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken)
            ?? throw new AppException(
                ErrorCodes.TechnicianProfileNotFound,
                "A technician profile could not be resolved for the current user.",
                404);

        if (!technician.IsActive)
        {
            throw new AppException(ErrorCodes.TechnicianInactive, "The technician profile is inactive.", 409);
        }

        return technician;
    }

    public async Task<ServiceRequest> GetOwnedServiceRequestAsync(long serviceRequestId, CancellationToken cancellationToken)
    {
        var technician = await GetCurrentTechnicianAsync(cancellationToken);
        var serviceRequest = await _serviceRequestRepository.GetByIdAsync(serviceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested technician job could not be found.", 404);

        EnsureOwnership(serviceRequest, technician);
        return serviceRequest;
    }

    public async Task<ServiceRequest> GetOwnedServiceRequestForUpdateAsync(long serviceRequestId, CancellationToken cancellationToken)
    {
        var technician = await GetCurrentTechnicianAsync(cancellationToken);
        var serviceRequest = await _serviceRequestRepository.GetByIdForUpdateAsync(serviceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested technician job could not be found.", 404);

        EnsureOwnership(serviceRequest, technician);
        return serviceRequest;
    }

    private static void EnsureOwnership(ServiceRequest serviceRequest, Technician technician)
    {
        var activeAssignment = serviceRequest.Assignments
            .FirstOrDefault(assignment => assignment.IsActiveAssignment && !assignment.IsDeleted);

        if (activeAssignment is null || activeAssignment.TechnicianId != technician.TechnicianId)
        {
            throw new AppException(
                ErrorCodes.TechnicianJobAccessDenied,
                "The current technician is not assigned to this job.",
                403);
        }
    }
}
