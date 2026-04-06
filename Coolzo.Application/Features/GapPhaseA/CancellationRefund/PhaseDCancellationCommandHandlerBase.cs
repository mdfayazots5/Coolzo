using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.GapPhaseD;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;
using DomainBooking = Coolzo.Domain.Entities.Booking;
using DomainServiceRequest = Coolzo.Domain.Entities.ServiceRequest;

namespace Coolzo.Application.Features.GapPhaseA.CancellationRefund;

public abstract class PhaseDCancellationCommandHandlerBase
{
    protected readonly IAuditLogRepository AuditLogRepository;
    protected readonly CancellationPolicyEvaluationService CancellationPolicyEvaluationService;
    protected readonly ICurrentDateTime CurrentDateTime;
    protected readonly ICurrentUserContext CurrentUserContext;
    protected readonly IGapPhaseARepository GapPhaseARepository;
    protected readonly GapPhaseANotificationService NotificationService;
    protected readonly RefundApprovalRulesService RefundApprovalRulesService;
    protected readonly IUnitOfWork UnitOfWork;

    protected PhaseDCancellationCommandHandlerBase(
        IGapPhaseARepository gapPhaseARepository,
        CancellationPolicyEvaluationService cancellationPolicyEvaluationService,
        RefundApprovalRulesService refundApprovalRulesService,
        GapPhaseANotificationService notificationService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        GapPhaseARepository = gapPhaseARepository;
        CancellationPolicyEvaluationService = cancellationPolicyEvaluationService;
        RefundApprovalRulesService = refundApprovalRulesService;
        NotificationService = notificationService;
        AuditLogRepository = auditLogRepository;
        UnitOfWork = unitOfWork;
        CurrentDateTime = currentDateTime;
        CurrentUserContext = currentUserContext;
    }

    protected async Task<CancellationDetailResponse> ExecuteCancellationAsync(
        DomainBooking booking,
        DomainServiceRequest? serviceRequest,
        string cancellationSource,
        string cancellationReasonCode,
        string cancellationReasonText,
        bool enforceCustomerCutoff,
        bool skipServiceRequestTransition,
        CancellationToken cancellationToken)
    {
        if (serviceRequest is not null &&
            await GapPhaseARepository.GetCancellationByServiceRequestIdAsync(serviceRequest.ServiceRequestId, cancellationToken) is not null)
        {
            throw new AppException(ErrorCodes.CancellationAlreadyExists, "A cancellation record already exists for this service request.", 409);
        }

        if (await GapPhaseARepository.GetCancellationByBookingIdAsync(booking.BookingId, cancellationToken) is not null)
        {
            throw new AppException(ErrorCodes.CancellationAlreadyExists, "A cancellation record already exists for this booking.", 409);
        }

        var evaluation = await CancellationPolicyEvaluationService.EvaluateAsync(booking, serviceRequest, cancellationToken);
        if (enforceCustomerCutoff && !evaluation.CanCustomerCancel)
        {
            throw new AppException(ErrorCodes.Conflict, evaluation.CustomerDenialReason, 409);
        }

        var now = CurrentDateTime.UtcNow;
        var actor = ResolveActor();
        var actorRole = CurrentUserContext.Roles.FirstOrDefault() ?? RoleNames.Customer;
        var cancellationRecord = new CancellationRecord
        {
            BookingId = booking.BookingId,
            ServiceRequestId = serviceRequest?.ServiceRequestId,
            CancelledByUserId = CurrentUserContext.UserId,
            CancelledByRole = actorRole,
            CancellationSource = cancellationSource,
            CancellationReasonCode = cancellationReasonCode.Trim(),
            CancellationReasonText = cancellationReasonText.Trim(),
            TimeToSlotMinutes = evaluation.TimeToSlotMinutes,
            CancellationFeeAmount = evaluation.CancellationFee,
            RefundEligibleAmount = evaluation.RefundEligibleAmount,
            CancellationStatus = CancellationStatus.Cancelled,
            PolicyCode = evaluation.PolicyCode,
            ReasonCode = cancellationReasonCode.Trim(),
            ReasonDescription = cancellationReasonText.Trim(),
            RequiresApproval = evaluation.ApprovalRequired,
            RequestedByRole = actorRole,
            RequestedDateUtc = now,
            Comments = evaluation.PolicyDescription,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = CurrentUserContext.IPAddress
        };

        await GapPhaseARepository.AddCancellationRecordAsync(cancellationRecord, cancellationToken);
        PhaseDCancellationRefundSupport.ApplyBookingCancelled(booking, cancellationReasonText.Trim(), now, actor, CurrentUserContext.IPAddress);

        if (serviceRequest is not null && !skipServiceRequestTransition)
        {
            PhaseDCancellationRefundSupport.ApplyServiceRequestStatus(
                serviceRequest,
                ServiceRequestStatus.Cancelled,
                cancellationReasonText.Trim(),
                now,
                actor,
                CurrentUserContext.IPAddress);
        }

        await AddCancellationWorkflowHistoryAsync(cancellationRecord, cancellationReasonText.Trim(), cancellationToken);

        RefundRequest? refundRequest = null;
        var invoiceHeader = ResolveInvoiceHeader(booking);
        if (invoiceHeader is not null && evaluation.RefundEligibleAmount > 0)
        {
            var approvalDecision = await RefundApprovalRulesService.EvaluateAsync(invoiceHeader, evaluation.RefundEligibleAmount, cancellationToken);
            refundRequest = new RefundRequest
            {
                CancellationRecord = cancellationRecord,
                InvoiceHeaderId = invoiceHeader.InvoiceHeaderId,
                PaymentTransactionId = approvalDecision.PaymentTransactionId,
                RefundRequestNo = PhaseDCancellationRefundSupport.BuildRefundRequestNumber(now),
                RefundMethod = approvalDecision.SuggestedRefundMethod,
                RefundAmount = evaluation.RefundEligibleAmount,
                RequestedAmount = evaluation.RefundEligibleAmount,
                ApprovedAmount = approvalDecision.ApprovalRequired ? 0m : evaluation.RefundEligibleAmount,
                MaxAllowedAmount = evaluation.PaidAmount,
                RefundReason = cancellationReasonText.Trim(),
                RefundStatus = approvalDecision.ApprovalRequired ? RefundStatus.PendingApproval : RefundStatus.Approved,
                ApprovalRequiredFlag = approvalDecision.ApprovalRequired,
                RequestedDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = CurrentUserContext.IPAddress
            };

            cancellationRecord.CancellationStatus = CancellationStatus.RefundPending;
            var refundStatusHistory = PhaseDCancellationRefundSupport.BuildRefundStatusHistory(
                refundRequest,
                "None",
                refundRequest.RefundStatus.ToString(),
                CurrentUserContext.UserId ?? 0,
                "Refund request created from cancellation.",
                now,
                actor,
                CurrentUserContext.IPAddress);
            refundRequest.StatusHistory.Add(refundStatusHistory);
            await GapPhaseARepository.AddRefundRequestAsync(refundRequest, cancellationToken);
            await GapPhaseARepository.AddRefundStatusHistoryAsync(refundStatusHistory, cancellationToken);
        }
        else
        {
            cancellationRecord.CancellationStatus = CancellationStatus.Closed;
        }

        await AuditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = CurrentUserContext.UserId,
                ActionName = "CreateCancellation",
                EntityName = nameof(CancellationRecord),
                EntityId = booking.BookingReference,
                TraceId = CurrentUserContext.TraceId,
                StatusName = "Success",
                NewValues = cancellationReasonCode.Trim(),
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = CurrentUserContext.IPAddress
            },
            cancellationToken);

        await NotificationService.RaiseAlertAsync(
            $"cancellation-{booking.BookingId}-{now:yyyyMMddHHmmss}",
            "cancellation.confirmed",
            "CancellationConfirmed",
            nameof(Booking),
            booking.BookingReference,
            SystemAlertSeverity.Warning,
            $"Cancellation confirmed for booking {booking.BookingReference}.",
            now.AddHours(1),
            1,
            "CustomerSupportExecutive>OperationsManager",
            cancellationToken);

        if (refundRequest is not null)
        {
            await NotificationService.RaiseAlertAsync(
                $"refund-{booking.BookingId}-{now:yyyyMMddHHmmss}",
                "refund.request.created",
                "RefundRequested",
                nameof(RefundRequest),
                refundRequest.RefundRequestNo,
                SystemAlertSeverity.Warning,
                $"Refund request {refundRequest.RefundRequestNo} was created from cancellation {cancellationRecord.CancellationRecordId}.",
                now.AddHours(4),
                1,
                "CustomerSupportExecutive>Admin",
                cancellationToken);
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return PhaseDCancellationRefundSupport.MapCancellationDetail(cancellationRecord, refundRequest);
    }

    private async Task AddCancellationWorkflowHistoryAsync(
        CancellationRecord cancellationRecord,
        string remarks,
        CancellationToken cancellationToken)
    {
        await GapPhaseARepository.AddWorkflowStatusHistoryAsync(
            new WorkflowStatusHistory
            {
                EntityType = WorkflowEntityType.Cancellation,
                EntityReference = cancellationRecord.CancellationRecordId.ToString(),
                PreviousStatus = "ActiveBookingOrSR",
                CurrentStatus = cancellationRecord.CancellationStatus.ToString(),
                Remarks = remarks,
                ChangedByRole = CurrentUserContext.Roles.FirstOrDefault() ?? RoleNames.Customer,
                ChangedDateUtc = CurrentDateTime.UtcNow,
                CreatedBy = ResolveActor(),
                DateCreated = CurrentDateTime.UtcNow,
                IPAddress = CurrentUserContext.IPAddress
            },
            cancellationToken);
    }

    private static InvoiceHeader? ResolveInvoiceHeader(DomainBooking booking)
    {
        return booking.ServiceRequest?.JobCard?.Quotations
            .Where(item => !item.IsDeleted && item.InvoiceHeader is { IsDeleted: false })
            .OrderByDescending(item => item.QuotationDateUtc)
            .Select(item => item.InvoiceHeader)
            .FirstOrDefault();
    }

    protected string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(CurrentUserContext.UserName) ? "Cancellation" : CurrentUserContext.UserName;
    }
}
