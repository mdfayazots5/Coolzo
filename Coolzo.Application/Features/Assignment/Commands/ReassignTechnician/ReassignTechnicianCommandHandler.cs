using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Coolzo.Application.Features.ServiceRequest;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;
using DomainTechnician = Coolzo.Domain.Entities.Technician;

namespace Coolzo.Application.Features.Assignment.Commands.ReassignTechnician;

public sealed class ReassignTechnicianCommandHandler : IRequestHandler<ReassignTechnicianCommand, ServiceRequestDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<ReassignTechnicianCommandHandler> _logger;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReassignTechnicianCommandHandler(
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<ReassignTechnicianCommandHandler> logger)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _technicianRepository = technicianRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<ServiceRequestDetailResponse> Handle(ReassignTechnicianCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _serviceRequestRepository.GetByIdForUpdateAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);

        if (serviceRequest.CurrentStatus is ServiceRequestStatus.EnRoute or ServiceRequestStatus.Reached)
        {
            throw new AppException(
                ErrorCodes.InvalidStatusTransition,
                "Technician reassignment is not allowed once the service request has moved beyond the assigned state.",
                409);
        }

        var activeAssignment = serviceRequest.Assignments.FirstOrDefault(
            assignment => assignment.IsActiveAssignment && !assignment.IsDeleted)
            ?? throw new AppException(ErrorCodes.Conflict, "No technician is currently assigned to this service request.", 409);

        if (activeAssignment.TechnicianId == request.TechnicianId)
        {
            throw new AppException(
                ErrorCodes.DuplicateAssignment,
                "The selected technician is already assigned to this service request.",
                409);
        }

        var availabilitySnapshots = await _technicianRepository.GetAvailabilityByServiceRequestIdAsync(serviceRequest.ServiceRequestId, cancellationToken);
        var selectedSnapshot = availabilitySnapshots.FirstOrDefault(snapshot => snapshot.TechnicianId == request.TechnicianId)
            ?? throw new AppException(ErrorCodes.NotFound, "The selected technician could not be found.", 404);

        ValidateAvailability(selectedSnapshot);

        var previousTechnician = await _technicianRepository.GetByIdForUpdateAsync(activeAssignment.TechnicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The currently assigned technician could not be found.", 404);

        var currentTechnician = await _technicianRepository.GetByIdForUpdateAsync(request.TechnicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The selected technician could not be found.", 404);

        if (!currentTechnician.IsActive)
        {
            throw new AppException(ErrorCodes.TechnicianInactive, "The selected technician is inactive.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
        var ipAddress = _currentUserContext.IPAddress;
        var remarks = !string.IsNullOrWhiteSpace(request.Remarks)
            ? request.Remarks.Trim()
            : $"Reassigned from {previousTechnician.TechnicianName} to {currentTechnician.TechnicianName}.";

        activeAssignment.IsActiveAssignment = false;
        activeAssignment.UnassignedDateUtc = now;
        activeAssignment.UnassignmentRemarks = remarks;
        activeAssignment.UpdatedBy = userName;
        activeAssignment.LastUpdated = now;

        serviceRequest.CurrentStatus = ServiceRequestStatus.Assigned;
        serviceRequest.UpdatedBy = userName;
        serviceRequest.LastUpdated = now;

        serviceRequest.Assignments.Add(new ServiceRequestAssignment
        {
            TechnicianId = currentTechnician.TechnicianId,
            AssignedDateUtc = now,
            AssignmentRemarks = remarks,
            IsAutoAssigned = false,
            IsActiveAssignment = true,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        });

        serviceRequest.AssignmentLogs.Add(new AssignmentLog
        {
            PreviousTechnicianId = previousTechnician.TechnicianId,
            CurrentTechnicianId = currentTechnician.TechnicianId,
            ActionName = "Reassigned",
            Remarks = remarks,
            ActionDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        });

        await UpdateAvailabilityAsync(previousTechnician, selectedSnapshot.AvailableDate, -1, cancellationToken);
        await UpdateAvailabilityAsync(currentTechnician, selectedSnapshot.AvailableDate, 1, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "ReassignTechnician",
                EntityName = "ServiceRequest",
                EntityId = serviceRequest.ServiceRequestNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = $"{previousTechnician.TechnicianCode}->{currentTechnician.TechnicianCode}",
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = ipAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Technician {PreviousTechnicianId} reassigned to {CurrentTechnicianId} for service request {ServiceRequestId}.",
            previousTechnician.TechnicianId,
            currentTechnician.TechnicianId,
            serviceRequest.ServiceRequestId);

        var refreshedServiceRequest = await _serviceRequestRepository.GetByIdAsync(serviceRequest.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The updated service request could not be loaded.", 404);

        return ServiceRequestResponseMapper.ToDetail(refreshedServiceRequest, Array.Empty<ServiceChecklistMaster>());
    }

    private static void ValidateAvailability(TechnicianAvailabilitySnapshot availabilitySnapshot)
    {
        if (!availabilitySnapshot.IsAvailable)
        {
            throw new AppException(
                ErrorCodes.TechnicianUnavailable,
                "The selected technician is unavailable or overloaded for the service date.",
                409);
        }
    }

    private async Task UpdateAvailabilityAsync(DomainTechnician technician, DateOnly availableDate, int delta, CancellationToken cancellationToken)
    {
        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
        var ipAddress = _currentUserContext.IPAddress;
        var availabilityEntry = await _technicianRepository.GetAvailabilityEntryForUpdateAsync(
            technician.TechnicianId,
            availableDate,
            cancellationToken);

        if (availabilityEntry is null)
        {
            technician.TechnicianAvailabilities.Add(new TechnicianAvailability
            {
                AvailableDate = availableDate,
                AvailableSlotCount = technician.MaxDailyAssignments,
                BookedAssignmentCount = Math.Max(delta, 0),
                IsAvailable = true,
                AvailabilityRemarks = "Auto-created from reassignment flow.",
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = ipAddress
            });

            return;
        }

        availabilityEntry.BookedAssignmentCount = Math.Max(availabilityEntry.BookedAssignmentCount + delta, 0);
        availabilityEntry.UpdatedBy = userName;
        availabilityEntry.LastUpdated = now;
        availabilityEntry.IPAddress = ipAddress;
    }
}
