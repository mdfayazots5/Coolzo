using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;

namespace Coolzo.Application.Common.Services;

public sealed class CustomerAbsentOrchestrationService
{
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseARepository _gapPhaseARepository;

    public CustomerAbsentOrchestrationService(
        IGapPhaseARepository gapPhaseARepository,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _gapPhaseARepository = gapPhaseARepository;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerAbsentRecord> MarkAsync(
        ServiceRequest serviceRequest,
        Technician technician,
        string absentReasonCode,
        string absentReasonText,
        int attemptCount,
        string contactAttemptLog,
        CancellationToken cancellationToken)
    {
        if (serviceRequest.CurrentStatus != ServiceRequestStatus.Reached)
        {
            throw new AppException(
                ErrorCodes.InvalidStatusTransition,
                "Customer absent can only be marked after the technician reaches the location.",
                409);
        }

        var existingRecord = await _gapPhaseARepository.GetCustomerAbsentByServiceRequestIdForUpdateAsync(serviceRequest.ServiceRequestId, cancellationToken);
        if (existingRecord is not null && existingRecord.CustomerAbsentStatus == CustomerAbsentStatus.Marked)
        {
            throw new AppException(ErrorCodes.Conflict, "Customer absent is already marked for this service request.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();
        var record = existingRecord ?? new CustomerAbsentRecord
        {
            ServiceRequestId = serviceRequest.ServiceRequestId,
            TechnicianId = technician.TechnicianId,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        record.TechnicianId = technician.TechnicianId;
        record.Technician = technician;
        record.ServiceRequest = serviceRequest;
        record.MarkedOn = now;
        record.AttemptCount = attemptCount;
        record.ContactAttemptLog = contactAttemptLog.Trim();
        record.AbsentReasonCode = absentReasonCode.Trim();
        record.AbsentReasonText = absentReasonText.Trim();
        record.CustomerAbsentStatus = CustomerAbsentStatus.Marked;
        record.ResolutionRemarks = string.Empty;
        record.ResolvedOn = null;
        record.UpdatedBy = actor;
        record.LastUpdated = now;
        record.IPAddress = _currentUserContext.IPAddress;

        if (existingRecord is null)
        {
            await _gapPhaseARepository.AddCustomerAbsentRecordAsync(record, cancellationToken);
        }

        ApplyServiceRequestTransition(serviceRequest, ServiceRequestStatus.CustomerAbsent, "Customer absent recorded by technician.", now, actor);
        await AddWorkflowHistoryAsync(serviceRequest.ServiceRequestNumber, ServiceRequestStatus.Reached, ServiceRequestStatus.CustomerAbsent, record.AbsentReasonText, cancellationToken);

        return record;
    }

    public async Task<CustomerAbsentRecord> ResolveAsRescheduledAsync(
        ServiceRequest serviceRequest,
        CustomerAbsentRecord record,
        string remarks,
        CancellationToken cancellationToken)
    {
        if (record.CustomerAbsentStatus != CustomerAbsentStatus.Marked || serviceRequest.CurrentStatus != ServiceRequestStatus.CustomerAbsent)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "Only open customer-absent records can be rescheduled.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();
        record.CustomerAbsentStatus = CustomerAbsentStatus.Rescheduled;
        record.ResolutionRemarks = remarks.Trim();
        record.ResolvedOn = now;
        record.UpdatedBy = actor;
        record.LastUpdated = now;
        record.IPAddress = _currentUserContext.IPAddress;

        ApplyServiceRequestTransition(serviceRequest, ServiceRequestStatus.Rescheduled, remarks.Trim(), now, actor);
        await AddWorkflowHistoryAsync(serviceRequest.ServiceRequestNumber, ServiceRequestStatus.CustomerAbsent, ServiceRequestStatus.Rescheduled, remarks.Trim(), cancellationToken);

        return record;
    }

    public async Task<CustomerAbsentRecord> ResolveAsCancelledAsync(
        ServiceRequest serviceRequest,
        CustomerAbsentRecord record,
        string remarks,
        CancellationToken cancellationToken)
    {
        if (record.CustomerAbsentStatus != CustomerAbsentStatus.Marked || serviceRequest.CurrentStatus != ServiceRequestStatus.CustomerAbsent)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "Only open customer-absent records can be cancelled.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();
        record.CustomerAbsentStatus = CustomerAbsentStatus.Cancelled;
        record.ResolutionRemarks = remarks.Trim();
        record.ResolvedOn = now;
        record.UpdatedBy = actor;
        record.LastUpdated = now;
        record.IPAddress = _currentUserContext.IPAddress;

        ApplyServiceRequestTransition(serviceRequest, ServiceRequestStatus.Cancelled, remarks.Trim(), now, actor);
        await AddWorkflowHistoryAsync(serviceRequest.ServiceRequestNumber, ServiceRequestStatus.CustomerAbsent, ServiceRequestStatus.Cancelled, remarks.Trim(), cancellationToken);

        return record;
    }

    private async Task AddWorkflowHistoryAsync(
        string entityReference,
        ServiceRequestStatus previousStatus,
        ServiceRequestStatus currentStatus,
        string remarks,
        CancellationToken cancellationToken)
    {
        var actorRole = _currentUserContext.Roles.FirstOrDefault() ?? RoleNames.Technician;

        await _gapPhaseARepository.AddWorkflowStatusHistoryAsync(
            new WorkflowStatusHistory
            {
                EntityType = WorkflowEntityType.ServiceRequest,
                EntityReference = entityReference,
                PreviousStatus = previousStatus.ToString(),
                CurrentStatus = currentStatus.ToString(),
                Remarks = remarks,
                ChangedByRole = actorRole,
                ChangedDateUtc = _currentDateTime.UtcNow,
                CreatedBy = ResolveActor(),
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
    }

    private void ApplyServiceRequestTransition(
        ServiceRequest serviceRequest,
        ServiceRequestStatus targetStatus,
        string remarks,
        DateTime now,
        string actor)
    {
        var previousStatus = serviceRequest.CurrentStatus;
        serviceRequest.CurrentStatus = targetStatus;
        serviceRequest.UpdatedBy = actor;
        serviceRequest.LastUpdated = now;
        serviceRequest.IPAddress = _currentUserContext.IPAddress;

        serviceRequest.StatusHistories.Add(new ServiceRequestStatusHistory
        {
            Status = targetStatus,
            Remarks = remarks,
            StatusDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });

        if (serviceRequest.JobCard is not null)
        {
            serviceRequest.JobCard.ExecutionTimelines.Add(new JobExecutionTimeline
            {
                Status = targetStatus,
                EventType = "StatusChanged",
                EventTitle = targetStatus.ToString(),
                Remarks = string.IsNullOrWhiteSpace(remarks)
                    ? $"Service request moved from {previousStatus} to {targetStatus}."
                    : remarks,
                EventDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });
        }
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "CustomerAbsent" : _currentUserContext.UserName;
    }
}
