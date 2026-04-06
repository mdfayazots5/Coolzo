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

namespace Coolzo.Application.Features.Assignment.Commands.AssignTechnician;

public sealed class AssignTechnicianCommandHandler : IRequestHandler<AssignTechnicianCommand, ServiceRequestDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<AssignTechnicianCommandHandler> _logger;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignTechnicianCommandHandler(
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<AssignTechnicianCommandHandler> logger)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _technicianRepository = technicianRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<ServiceRequestDetailResponse> Handle(AssignTechnicianCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _serviceRequestRepository.GetByIdForUpdateAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);

        var activeAssignment = serviceRequest.Assignments.FirstOrDefault(
            assignment => assignment.IsActiveAssignment && !assignment.IsDeleted);

        if (activeAssignment is not null)
        {
            throw new AppException(
                ErrorCodes.DuplicateAssignment,
                "A technician is already assigned to this service request.",
                409);
        }

        if (serviceRequest.CurrentStatus != ServiceRequestStatus.New)
        {
            throw new AppException(
                ErrorCodes.InvalidStatusTransition,
                "Only new service requests can receive an initial assignment.",
                409);
        }

        var availabilitySnapshots = await _technicianRepository.GetAvailabilityByServiceRequestIdAsync(serviceRequest.ServiceRequestId, cancellationToken);
        var selectedSnapshot = SelectTechnician(availabilitySnapshots, request.TechnicianId);
        ValidateAvailability(selectedSnapshot);

        var technician = await _technicianRepository.GetByIdForUpdateAsync(selectedSnapshot.TechnicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The selected technician could not be found.", 404);

        if (!technician.IsActive)
        {
            throw new AppException(ErrorCodes.TechnicianInactive, "The selected technician is inactive.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
        var ipAddress = _currentUserContext.IPAddress;
        var remarks = BuildAssignmentRemarks(request.Remarks, request.TechnicianId.HasValue, technician.TechnicianName);

        serviceRequest.CurrentStatus = ServiceRequestStatus.Assigned;
        serviceRequest.UpdatedBy = userName;
        serviceRequest.LastUpdated = now;

        serviceRequest.Assignments.Add(new ServiceRequestAssignment
        {
            TechnicianId = technician.TechnicianId,
            AssignedDateUtc = now,
            AssignmentRemarks = remarks,
            IsAutoAssigned = !request.TechnicianId.HasValue,
            IsActiveAssignment = true,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        });

        serviceRequest.StatusHistories.Add(new ServiceRequestStatusHistory
        {
            Status = ServiceRequestStatus.Assigned,
            Remarks = remarks,
            StatusDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        });

        serviceRequest.AssignmentLogs.Add(new AssignmentLog
        {
            CurrentTechnicianId = technician.TechnicianId,
            ActionName = request.TechnicianId.HasValue ? "Assigned" : "AutoAssigned",
            Remarks = remarks,
            ActionDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        });

        await UpdateAvailabilityAsync(technician, selectedSnapshot.AvailableDate, 1, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "AssignTechnician",
                EntityName = "ServiceRequest",
                EntityId = serviceRequest.ServiceRequestNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = technician.TechnicianCode,
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = ipAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Technician {TechnicianId} assigned to service request {ServiceRequestId}.",
            technician.TechnicianId,
            serviceRequest.ServiceRequestId);

        var refreshedServiceRequest = await _serviceRequestRepository.GetByIdAsync(serviceRequest.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The updated service request could not be loaded.", 404);

        return ServiceRequestResponseMapper.ToDetail(refreshedServiceRequest, Array.Empty<ServiceChecklistMaster>());
    }

    private TechnicianAvailabilitySnapshot SelectTechnician(
        IReadOnlyCollection<TechnicianAvailabilitySnapshot> availabilitySnapshots,
        long? technicianId)
    {
        TechnicianAvailabilitySnapshot? selectedSnapshot;

        if (technicianId.HasValue)
        {
            selectedSnapshot = availabilitySnapshots.FirstOrDefault(snapshot => snapshot.TechnicianId == technicianId.Value);

            if (selectedSnapshot is null)
            {
                throw new AppException(ErrorCodes.NotFound, "The selected technician could not be found.", 404);
            }

            return selectedSnapshot;
        }

        selectedSnapshot = availabilitySnapshots
            .Where(snapshot => snapshot.IsAvailable && snapshot.IsSkillMatched)
            .OrderBy(snapshot => snapshot.BookedAssignmentCount)
            .ThenBy(snapshot => snapshot.TechnicianName)
            .FirstOrDefault();

        if (selectedSnapshot is null)
        {
            throw new AppException(
                ErrorCodes.TechnicianUnavailable,
                "No available technician could be found for this service request.",
                409);
        }

        return selectedSnapshot;
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

    private string BuildAssignmentRemarks(string? remarks, bool isManualAssignment, string technicianName)
    {
        if (!string.IsNullOrWhiteSpace(remarks))
        {
            return remarks.Trim();
        }

        return isManualAssignment
            ? $"Technician {technicianName} assigned by operations."
            : $"Technician {technicianName} auto-assigned by workload rules.";
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
                AvailabilityRemarks = "Auto-created from assignment flow.",
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
