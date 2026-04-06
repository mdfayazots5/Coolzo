using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Domain.Rules;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;

namespace Coolzo.Application.Common.Services;

public sealed class TechnicianFieldExecutionService : ITechnicianFieldExecutionService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFieldLookupRepository _fieldLookupRepository;
    private readonly IJobCardFactory _jobCardFactory;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public TechnicianFieldExecutionService(
        ITechnicianJobAccessService technicianJobAccessService,
        IJobCardFactory jobCardFactory,
        IFieldLookupRepository fieldLookupRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _jobCardFactory = jobCardFactory;
        _fieldLookupRepository = fieldLookupRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<ServiceRequest> AdvanceStatusAsync(
        long serviceRequestId,
        ServiceRequestStatus targetStatus,
        string? remarks,
        string? workSummary,
        string auditActionName,
        CancellationToken cancellationToken)
    {
        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestForUpdateAsync(serviceRequestId, cancellationToken);

        if (!ServiceRequestStatusRule.CanTransition(serviceRequest.CurrentStatus, targetStatus))
        {
            throw new AppException(
                ErrorCodes.InvalidStatusTransition,
                "The requested technician job transition is not allowed.",
                409);
        }

        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
        var ipAddress = _currentUserContext.IPAddress;
        var jobCard = _jobCardFactory.EnsureCreated(serviceRequest);
        var normalizedRemarks = !string.IsNullOrWhiteSpace(remarks)
            ? remarks.Trim()
            : $"Technician moved job to {targetStatus}.";

        if (!string.IsNullOrWhiteSpace(workSummary))
        {
            jobCard.CompletionSummary = workSummary.Trim();
        }

        if (targetStatus == ServiceRequestStatus.SubmittedForClosure)
        {
            await ValidateSubmissionRequirementsAsync(serviceRequest, cancellationToken);
        }

        ApplyJobCardDates(jobCard, targetStatus, now);

        serviceRequest.CurrentStatus = targetStatus;
        serviceRequest.UpdatedBy = userName;
        serviceRequest.LastUpdated = now;
        serviceRequest.IPAddress = ipAddress;

        serviceRequest.StatusHistories.Add(new ServiceRequestStatusHistory
        {
            Status = targetStatus,
            Remarks = normalizedRemarks,
            StatusDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        });

        jobCard.ExecutionTimelines.Add(new JobExecutionTimeline
        {
            Status = targetStatus,
            EventType = "StatusChanged",
            EventTitle = targetStatus.ToString(),
            Remarks = normalizedRemarks,
            EventDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        });

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = auditActionName,
                EntityName = "ServiceRequest",
                EntityId = serviceRequest.ServiceRequestNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = targetStatus.ToString(),
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = ipAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return serviceRequest;
    }

    private async Task ValidateSubmissionRequirementsAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken)
    {
        var jobCard = serviceRequest.JobCard ?? _jobCardFactory.EnsureCreated(serviceRequest);
        var hasNotes = jobCard.ExecutionNotes.Any(note => !note.IsDeleted);
        var hasSummary = !string.IsNullOrWhiteSpace(jobCard.CompletionSummary);

        if (!hasNotes && !hasSummary)
        {
            throw new AppException(
                ErrorCodes.SubmissionRequirementMissing,
                "Submission requires at least one execution note or a completion summary.",
                409);
        }

        if (!jobCard.ExecutionTimelines.Any(item => !item.IsDeleted))
        {
            throw new AppException(
                ErrorCodes.SubmissionRequirementMissing,
                "Submission requires at least one execution timeline entry.",
                409);
        }

        var serviceId = serviceRequest.Booking?.BookingLines
            .OrderBy(line => line.BookingLineId)
            .Select(line => line.ServiceId)
            .FirstOrDefault() ?? 0;

        if (serviceId <= 0)
        {
            return;
        }

        var checklistMasters = await _fieldLookupRepository.GetChecklistByServiceIdAsync(serviceId, cancellationToken);
        var mandatoryMasters = checklistMasters.Where(master => master.IsMandatory).ToArray();

        if (mandatoryMasters.Length == 0)
        {
            return;
        }

        var responseLookup = jobCard.ChecklistResponses
            .Where(response => !response.IsDeleted)
            .ToDictionary(response => response.ServiceChecklistMasterId);
        var incompleteMandatoryItem = mandatoryMasters.FirstOrDefault(master =>
        {
            if (!responseLookup.TryGetValue(master.ServiceChecklistMasterId, out var response))
            {
                return true;
            }

            return !response.IsChecked.HasValue && string.IsNullOrWhiteSpace(response.ResponseRemarks);
        });

        if (incompleteMandatoryItem is not null)
        {
            throw new AppException(
                ErrorCodes.SubmissionRequirementMissing,
                $"Checklist item '{incompleteMandatoryItem.ChecklistTitle}' must be completed before submission.",
                409);
        }
    }

    private static void ApplyJobCardDates(JobCard jobCard, ServiceRequestStatus targetStatus, DateTime now)
    {
        if (targetStatus == ServiceRequestStatus.WorkStarted)
        {
            jobCard.WorkStartedDateUtc ??= now;
        }

        if (targetStatus == ServiceRequestStatus.WorkInProgress)
        {
            jobCard.WorkStartedDateUtc ??= now;
            jobCard.WorkInProgressDateUtc ??= now;
        }

        if (targetStatus == ServiceRequestStatus.WorkCompletedPendingSubmission)
        {
            jobCard.WorkStartedDateUtc ??= now;
            jobCard.WorkInProgressDateUtc ??= now;
            jobCard.WorkCompletedDateUtc = now;
        }

        if (targetStatus == ServiceRequestStatus.SubmittedForClosure)
        {
            jobCard.SubmittedForClosureDateUtc = now;
        }
    }
}
