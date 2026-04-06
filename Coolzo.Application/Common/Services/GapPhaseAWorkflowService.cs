using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Domain.Rules;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;

namespace Coolzo.Application.Common.Services;

public sealed class GapPhaseAWorkflowService
{
    private static readonly IReadOnlyCollection<string> ApprovalRoles =
    [
        RoleNames.SuperAdmin,
        RoleNames.Admin,
        RoleNames.OperationsManager
    ];

    private static readonly IReadOnlyCollection<string> OperationsRoles =
    [
        RoleNames.SuperAdmin,
        RoleNames.Admin,
        RoleNames.OperationsManager,
        RoleNames.OperationsExecutive,
        RoleNames.CustomerSupportExecutive
    ];

    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseARepository _gapPhaseARepository;

    public GapPhaseAWorkflowService(
        IGapPhaseARepository gapPhaseARepository,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _gapPhaseARepository = gapPhaseARepository;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task ChangeLeadStatusAsync(Lead lead, LeadStatus targetStatus, string remarks, CancellationToken cancellationToken)
    {
        EnsureTransition(nameof(Lead), CanLeadTransition(lead.LeadStatus, targetStatus));
        EnsureRoles(OperationsRoles);

        lead.StatusHistories.Add(new LeadStatusHistory
        {
            PreviousStatus = lead.LeadStatus,
            CurrentStatus = targetStatus,
            Remarks = remarks,
            ChangedDateUtc = _currentDateTime.UtcNow,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        });

        await AddWorkflowHistoryAsync(WorkflowEntityType.Lead, lead.LeadNumber, lead.LeadStatus.ToString(), targetStatus.ToString(), remarks, cancellationToken);
        lead.LeadStatus = targetStatus;
        lead.UpdatedBy = _currentUserContext.UserName;
        lead.LastUpdated = _currentDateTime.UtcNow;

        if (targetStatus == LeadStatus.Contacted)
        {
            lead.LastContactedDateUtc = _currentDateTime.UtcNow;
        }

        if (targetStatus == LeadStatus.Converted)
        {
            lead.ConvertedDateUtc = _currentDateTime.UtcNow;
        }

        if (targetStatus == LeadStatus.Closed)
        {
            lead.ClosedDateUtc = _currentDateTime.UtcNow;
        }
    }

    public Task EnsureInstallationTransitionAsync(InstallationOrder order, InstallationOrderStatus targetStatus, string remarks, CancellationToken cancellationToken)
    {
        EnsureTransition(nameof(InstallationOrder), CanInstallationTransition(order.CurrentStatus, targetStatus));
        EnsureRoles(OperationsRoles);
        var previousStatus = order.CurrentStatus;
        order.CurrentStatus = targetStatus;
        order.UpdatedBy = _currentUserContext.UserName;
        order.LastUpdated = _currentDateTime.UtcNow;

        return AddWorkflowHistoryAsync(
            WorkflowEntityType.InstallationOrder,
            order.InstallationOrderNumber,
            previousStatus.ToString(),
            targetStatus.ToString(),
            remarks,
            cancellationToken);
    }

    public Task EnsureCancellationTransitionAsync(CancellationRecord cancellationRecord, CancellationStatus targetStatus, string remarks, CancellationToken cancellationToken)
    {
        EnsureTransition(nameof(CancellationRecord), CanCancellationTransition(cancellationRecord.CancellationStatus, targetStatus));
        EnsureRoles(targetStatus is CancellationStatus.Approved or CancellationStatus.Completed ? ApprovalRoles : OperationsRoles);
        var previousStatus = cancellationRecord.CancellationStatus;
        cancellationRecord.CancellationStatus = targetStatus;
        cancellationRecord.UpdatedBy = _currentUserContext.UserName;
        cancellationRecord.LastUpdated = _currentDateTime.UtcNow;

        return AddWorkflowHistoryAsync(
            WorkflowEntityType.Cancellation,
            cancellationRecord.CancellationRecordId.ToString(),
            previousStatus.ToString(),
            targetStatus.ToString(),
            remarks,
            cancellationToken);
    }

    public Task EnsureRefundTransitionAsync(RefundRequest refundRequest, RefundStatus targetStatus, string remarks, CancellationToken cancellationToken)
    {
        EnsureTransition(nameof(RefundRequest), CanRefundTransition(refundRequest.RefundStatus, targetStatus));
        EnsureRoles(targetStatus is RefundStatus.Approved or RefundStatus.Processed ? ApprovalRoles : OperationsRoles);
        var previousStatus = refundRequest.RefundStatus;
        refundRequest.RefundStatus = targetStatus;
        refundRequest.UpdatedBy = _currentUserContext.UserName;
        refundRequest.LastUpdated = _currentDateTime.UtcNow;

        return AddWorkflowHistoryAsync(
            WorkflowEntityType.Refund,
            refundRequest.RefundRequestId.ToString(),
            previousStatus.ToString(),
            targetStatus.ToString(),
            remarks,
            cancellationToken);
    }

    public Task EnsurePartsReturnTransitionAsync(PartsReturn partsReturn, PartsReturnStatus targetStatus, string remarks, CancellationToken cancellationToken)
    {
        EnsureTransition(nameof(PartsReturn), CanPartsReturnTransition(partsReturn.PartsReturnStatus, targetStatus));
        EnsureRoles(targetStatus is PartsReturnStatus.Approved or PartsReturnStatus.SupplierClaimRaised ? ApprovalRoles : OperationsRoles);
        var previousStatus = partsReturn.PartsReturnStatus;
        partsReturn.PartsReturnStatus = targetStatus;
        partsReturn.UpdatedBy = _currentUserContext.UserName;
        partsReturn.LastUpdated = _currentDateTime.UtcNow;

        return AddWorkflowHistoryAsync(
            WorkflowEntityType.PartsReturn,
            partsReturn.PartsReturnNumber,
            previousStatus.ToString(),
            targetStatus.ToString(),
            remarks,
            cancellationToken);
    }

    public Task EnsureServiceRequestTransitionAsync(ServiceRequest serviceRequest, ServiceRequestStatus targetStatus, string remarks, CancellationToken cancellationToken)
    {
        EnsureTransition(nameof(ServiceRequest), targetStatus != serviceRequest.CurrentStatus && ServiceRequestStatusRule.CanTransition(serviceRequest.CurrentStatus, targetStatus));
        EnsureRoles(OperationsRoles);
        var previousStatus = serviceRequest.CurrentStatus;
        serviceRequest.CurrentStatus = targetStatus;
        serviceRequest.UpdatedBy = _currentUserContext.UserName;
        serviceRequest.LastUpdated = _currentDateTime.UtcNow;

        return AddWorkflowHistoryAsync(
            WorkflowEntityType.ServiceRequest,
            serviceRequest.ServiceRequestNumber,
            previousStatus.ToString(),
            targetStatus.ToString(),
            remarks,
            cancellationToken);
    }

    private Task AddWorkflowHistoryAsync(
        WorkflowEntityType entityType,
        string entityReference,
        string previousStatus,
        string currentStatus,
        string remarks,
        CancellationToken cancellationToken)
    {
        var actorRole = _currentUserContext.Roles.FirstOrDefault() ?? "System";

        return _gapPhaseARepository.AddWorkflowStatusHistoryAsync(
            new WorkflowStatusHistory
            {
                EntityType = entityType,
                EntityReference = entityReference,
                PreviousStatus = previousStatus,
                CurrentStatus = currentStatus,
                Remarks = remarks,
                ChangedByRole = actorRole,
                ChangedDateUtc = _currentDateTime.UtcNow,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
    }

    private void EnsureTransition(string entityName, bool isAllowed)
    {
        if (!isAllowed)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, $"The requested {entityName} status transition is not allowed.", 409);
        }
    }

    private void EnsureRoles(IReadOnlyCollection<string> allowedRoles)
    {
        if (_currentUserContext.Roles.Any(allowedRoles.Contains))
        {
            return;
        }

        throw new AppException(ErrorCodes.Forbidden, "The current user cannot perform this workflow transition.", 403);
    }

    private static bool CanLeadTransition(LeadStatus currentStatus, LeadStatus targetStatus)
    {
        return currentStatus switch
        {
            LeadStatus.New => targetStatus == LeadStatus.Contacted,
            LeadStatus.Contacted => targetStatus is LeadStatus.Qualified or LeadStatus.Lost,
            LeadStatus.Qualified => targetStatus is LeadStatus.Converted or LeadStatus.Lost,
            LeadStatus.Converted => targetStatus == LeadStatus.Closed,
            _ => false
        };
    }

    private static bool CanInstallationTransition(InstallationOrderStatus currentStatus, InstallationOrderStatus targetStatus)
    {
        return currentStatus switch
        {
            InstallationOrderStatus.Draft => targetStatus is InstallationOrderStatus.SurveyScheduled or InstallationOrderStatus.Cancelled,
            InstallationOrderStatus.SurveyScheduled => targetStatus is InstallationOrderStatus.SurveyCompleted or InstallationOrderStatus.Cancelled,
            InstallationOrderStatus.SurveyCompleted => targetStatus is InstallationOrderStatus.ApprovedForInstallation or InstallationOrderStatus.Cancelled,
            InstallationOrderStatus.ApprovedForInstallation => targetStatus is InstallationOrderStatus.InstallationScheduled,
            InstallationOrderStatus.InstallationScheduled => targetStatus is InstallationOrderStatus.InstallationInProgress,
            InstallationOrderStatus.InstallationInProgress => targetStatus is InstallationOrderStatus.Commissioned,
            _ => false
        };
    }

    private static bool CanCancellationTransition(CancellationStatus currentStatus, CancellationStatus targetStatus)
    {
        return currentStatus switch
        {
            CancellationStatus.Requested => targetStatus is CancellationStatus.PendingApproval or CancellationStatus.Approved or CancellationStatus.Rejected,
            CancellationStatus.PendingApproval => targetStatus is CancellationStatus.Approved or CancellationStatus.Rejected,
            CancellationStatus.Approved => targetStatus is CancellationStatus.Completed,
            _ => false
        };
    }

    private static bool CanRefundTransition(RefundStatus currentStatus, RefundStatus targetStatus)
    {
        return currentStatus switch
        {
            RefundStatus.Initiated => targetStatus is RefundStatus.PendingApproval or RefundStatus.Approved or RefundStatus.Rejected,
            RefundStatus.PendingApproval => targetStatus is RefundStatus.Approved or RefundStatus.Rejected,
            RefundStatus.Approved => targetStatus is RefundStatus.Processed,
            _ => false
        };
    }

    private static bool CanPartsReturnTransition(PartsReturnStatus currentStatus, PartsReturnStatus targetStatus)
    {
        return currentStatus switch
        {
            PartsReturnStatus.Draft => targetStatus is PartsReturnStatus.Submitted,
            PartsReturnStatus.Submitted => targetStatus is PartsReturnStatus.Approved or PartsReturnStatus.Rejected,
            PartsReturnStatus.Approved => targetStatus is PartsReturnStatus.SupplierClaimRaised or PartsReturnStatus.Closed,
            PartsReturnStatus.SupplierClaimRaised => targetStatus is PartsReturnStatus.Closed,
            _ => false
        };
    }
}
