using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Coolzo.Application.Common.Services;
using Coolzo.Application.Features.ServiceRequest;
using Coolzo.Contracts.Responses.GapPhaseA;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;
using DomainTechnician = Coolzo.Domain.Entities.Technician;

namespace Coolzo.Application.Features.GapPhaseA.Escalation;

public sealed record CreateEscalationCommand(
    string AlertType,
    string RelatedEntityName,
    string RelatedEntityId,
    string Severity,
    int EscalationLevel,
    int SlaMinutes,
    string? NotificationChain,
    string Message) : IRequest<EscalationResponse>;

public sealed class CreateEscalationCommandValidator : AbstractValidator<CreateEscalationCommand>
{
    public CreateEscalationCommandValidator()
    {
        RuleFor(request => request.AlertType).NotEmpty().MaximumLength(64);
        RuleFor(request => request.RelatedEntityName).NotEmpty().MaximumLength(64);
        RuleFor(request => request.RelatedEntityId).NotEmpty().MaximumLength(64);
        RuleFor(request => request.Severity).NotEmpty().Must(BeValidSeverity).WithMessage("Escalation severity is invalid.");
        RuleFor(request => request.EscalationLevel).GreaterThan(0).LessThanOrEqualTo(5);
        RuleFor(request => request.SlaMinutes).GreaterThan(0).LessThanOrEqualTo(1440);
        RuleFor(request => request.NotificationChain).MaximumLength(256);
        RuleFor(request => request.Message).NotEmpty().MaximumLength(512);
    }

    private static bool BeValidSeverity(string severity)
    {
        return Enum.TryParse<SystemAlertSeverity>(severity, true, out _);
    }
}

public sealed class CreateEscalationCommandHandler : IRequestHandler<CreateEscalationCommand, EscalationResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly GapPhaseANotificationService _notificationService;
    private readonly IGapPhaseARepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEscalationCommandHandler(
        IGapPhaseARepository repository,
        GapPhaseANotificationService notificationService,
        GapPhaseAFeatureFlagService featureFlagService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _notificationService = notificationService;
        _featureFlagService = featureFlagService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<EscalationResponse> Handle(CreateEscalationCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.escalation.enabled", cancellationToken);

        var now = _currentDateTime.UtcNow;
        var alertCode = $"{request.AlertType.Trim().ToLowerInvariant()}-{now:yyyyMMddHHmmss}";
        var severity = Enum.Parse<SystemAlertSeverity>(request.Severity, true);

        var alert = await _notificationService.RaiseAlertAsync(
            alertCode,
            "escalation.alert",
            request.AlertType.Trim(),
            request.RelatedEntityName.Trim(),
            request.RelatedEntityId.Trim(),
            severity,
            request.Message.Trim(),
            now.AddMinutes(request.SlaMinutes),
            request.EscalationLevel,
            request.NotificationChain?.Trim() ?? "OperationsExecutive>OperationsManager",
            cancellationToken);

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreateEscalation",
                EntityName = nameof(SystemAlert),
                EntityId = alertCode,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = request.AlertType.Trim(),
                CreatedBy = ResolveActor(),
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EscalationResponse(
            alert.SystemAlertId,
            alert.AlertCode,
            alert.AlertType,
            alert.RelatedEntityName,
            alert.RelatedEntityId,
            alert.Severity.ToString(),
            alert.AlertStatus.ToString(),
            alert.EscalationLevel);
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "Escalation" : _currentUserContext.UserName;
    }
}

public sealed record HandleNoShowCommand(
    long ServiceRequestId,
    string Reason,
    long? PreferredTechnicianId) : IRequest<ServiceRequestDetailResponse>;

public sealed class HandleNoShowCommandValidator : AbstractValidator<HandleNoShowCommand>
{
    public HandleNoShowCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.Reason).NotEmpty().MaximumLength(512);
    }
}

public sealed class HandleNoShowCommandHandler : IRequestHandler<HandleNoShowCommand, ServiceRequestDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly GapPhaseANotificationService _notificationService;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly GapPhaseAWorkflowService _workflowService;
    private readonly IUnitOfWork _unitOfWork;

    public HandleNoShowCommandHandler(
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository,
        GapPhaseAWorkflowService workflowService,
        GapPhaseANotificationService notificationService,
        GapPhaseAFeatureFlagService featureFlagService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _technicianRepository = technicianRepository;
        _workflowService = workflowService;
        _notificationService = notificationService;
        _featureFlagService = featureFlagService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<ServiceRequestDetailResponse> Handle(HandleNoShowCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.escalation.enabled", cancellationToken);

        var serviceRequest = await _serviceRequestRepository.GetByIdForUpdateAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);
        var activeAssignment = serviceRequest.Assignments.FirstOrDefault(assignment => assignment.IsActiveAssignment && !assignment.IsDeleted)
            ?? throw new AppException(ErrorCodes.Conflict, "No active technician assignment was found for this service request.", 409);
        var availabilitySnapshots = await _technicianRepository.GetAvailabilityByServiceRequestIdAsync(serviceRequest.ServiceRequestId, cancellationToken);
        var targetSnapshot = SelectReplacement(availabilitySnapshots, activeAssignment.TechnicianId, request.PreferredTechnicianId);
        var previousTechnician = await _technicianRepository.GetByIdForUpdateAsync(activeAssignment.TechnicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The previously assigned technician could not be found.", 404);
        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();

        await _workflowService.EnsureServiceRequestTransitionAsync(serviceRequest, ServiceRequestStatus.NoShow, request.Reason.Trim(), cancellationToken);
        serviceRequest.StatusHistories.Add(new ServiceRequestStatusHistory
        {
            Status = ServiceRequestStatus.NoShow,
            Remarks = request.Reason.Trim(),
            StatusDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });

        if (targetSnapshot is null)
        {
            await _workflowService.EnsureServiceRequestTransitionAsync(serviceRequest, ServiceRequestStatus.Rescheduled, "No-show triggered automatic reschedule.", cancellationToken);
            serviceRequest.StatusHistories.Add(new ServiceRequestStatusHistory
            {
                Status = ServiceRequestStatus.Rescheduled,
                Remarks = "Technician no-show could not be auto-reassigned. Service request rescheduled.",
                StatusDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });

            await _notificationService.RaiseAlertAsync(
                "customer.absent",
                "customer.absent",
                "ServiceRequest",
                nameof(ServiceRequest),
                serviceRequest.ServiceRequestNumber,
                SystemAlertSeverity.Warning,
                $"No replacement technician was available for service request {serviceRequest.ServiceRequestNumber}.",
                now.AddMinutes(30),
                2,
                "CustomerSupportExecutive>OperationsManager",
                cancellationToken);
        }
        else
        {
            var currentTechnician = await _technicianRepository.GetByIdForUpdateAsync(targetSnapshot.TechnicianId, cancellationToken)
                ?? throw new AppException(ErrorCodes.NotFound, "The replacement technician could not be found.", 404);

            activeAssignment.IsActiveAssignment = false;
            activeAssignment.UnassignedDateUtc = now;
            activeAssignment.UnassignmentRemarks = request.Reason.Trim();
            activeAssignment.UpdatedBy = actor;
            activeAssignment.LastUpdated = now;

            await _workflowService.EnsureServiceRequestTransitionAsync(serviceRequest, ServiceRequestStatus.Assigned, "Auto reassigned after no-show.", cancellationToken);
            serviceRequest.Assignments.Add(new ServiceRequestAssignment
            {
                TechnicianId = currentTechnician.TechnicianId,
                AssignedDateUtc = now,
                AssignmentRemarks = "Auto reassigned after no-show.",
                IsAutoAssigned = !request.PreferredTechnicianId.HasValue,
                IsActiveAssignment = true,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });
            serviceRequest.AssignmentLogs.Add(new AssignmentLog
            {
                PreviousTechnicianId = previousTechnician.TechnicianId,
                CurrentTechnicianId = currentTechnician.TechnicianId,
                ActionName = "NoShowReassigned",
                Remarks = request.Reason.Trim(),
                ActionDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });
            serviceRequest.StatusHistories.Add(new ServiceRequestStatusHistory
            {
                Status = ServiceRequestStatus.Assigned,
                Remarks = $"Technician {currentTechnician.TechnicianName} auto-assigned after no-show.",
                StatusDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });

            await UpdateAvailabilityAsync(previousTechnician, targetSnapshot.AvailableDate, -1, cancellationToken);
            await UpdateAvailabilityAsync(currentTechnician, targetSnapshot.AvailableDate, 1, cancellationToken);
            await _notificationService.RaiseAlertAsync(
                "escalation.alert",
                "escalation.alert",
                "ServiceRequest",
                nameof(ServiceRequest),
                serviceRequest.ServiceRequestNumber,
                SystemAlertSeverity.Warning,
                $"Technician auto-reassigned after no-show on service request {serviceRequest.ServiceRequestNumber}.",
                now.AddMinutes(15),
                1,
                "OperationsExecutive>CustomerSupportExecutive",
                cancellationToken);
        }

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "HandleNoShow",
                EntityName = nameof(ServiceRequest),
                EntityId = serviceRequest.ServiceRequestNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = request.Reason.Trim(),
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var refreshedServiceRequest = await _serviceRequestRepository.GetByIdAsync(serviceRequest.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The updated service request could not be loaded.", 404);

        return ServiceRequestResponseMapper.ToDetail(refreshedServiceRequest, Array.Empty<ServiceChecklistMaster>());
    }

    private TechnicianAvailabilitySnapshot? SelectReplacement(
        IReadOnlyCollection<TechnicianAvailabilitySnapshot> availabilitySnapshots,
        long previousTechnicianId,
        long? preferredTechnicianId)
    {
        if (preferredTechnicianId.HasValue)
        {
            var preferred = availabilitySnapshots.FirstOrDefault(snapshot => snapshot.TechnicianId == preferredTechnicianId.Value);

            if (preferred is null || !preferred.IsAvailable || !preferred.IsSkillMatched)
            {
                throw new AppException(ErrorCodes.TechnicianUnavailable, "The preferred replacement technician is unavailable.", 409);
            }

            return preferred;
        }

        return availabilitySnapshots
            .Where(snapshot => snapshot.TechnicianId != previousTechnicianId && snapshot.IsAvailable && snapshot.IsSkillMatched)
            .OrderBy(snapshot => snapshot.BookedAssignmentCount)
            .ThenBy(snapshot => snapshot.TechnicianName)
            .FirstOrDefault();
    }

    private async Task UpdateAvailabilityAsync(DomainTechnician technician, DateOnly availableDate, int delta, CancellationToken cancellationToken)
    {
        var availabilityEntry = await _technicianRepository.GetAvailabilityEntryForUpdateAsync(technician.TechnicianId, availableDate, cancellationToken);
        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();

        if (availabilityEntry is null)
        {
            technician.TechnicianAvailabilities.Add(new TechnicianAvailability
            {
                AvailableDate = availableDate,
                AvailableSlotCount = technician.MaxDailyAssignments,
                BookedAssignmentCount = Math.Max(delta, 0),
                IsAvailable = true,
                AvailabilityRemarks = "Auto-created from no-show flow.",
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });

            return;
        }

        availabilityEntry.BookedAssignmentCount = Math.Max(availabilityEntry.BookedAssignmentCount + delta, 0);
        availabilityEntry.UpdatedBy = actor;
        availabilityEntry.LastUpdated = now;
        availabilityEntry.IPAddress = _currentUserContext.IPAddress;
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "NoShow" : _currentUserContext.UserName;
    }
}
