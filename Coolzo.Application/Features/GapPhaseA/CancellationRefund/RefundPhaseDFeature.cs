using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.GapPhaseD;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;
using DomainBooking = Coolzo.Domain.Entities.Booking;

namespace Coolzo.Application.Features.GapPhaseA.CancellationRefund;

public sealed record CreateRefundRequestCommand(
    long CancellationRecordId,
    long? InvoiceId,
    decimal RefundAmount,
    string RefundMethod,
    string RefundReason) : IRequest<RefundDetailResponse>;

public sealed class CreateRefundRequestCommandValidator : AbstractValidator<CreateRefundRequestCommand>
{
    public CreateRefundRequestCommandValidator()
    {
        RuleFor(request => request.CancellationRecordId).GreaterThan(0);
        RuleFor(request => request.RefundAmount).GreaterThan(0m);
        RuleFor(request => request.RefundMethod).NotEmpty().MaximumLength(64);
        RuleFor(request => request.RefundReason).NotEmpty().MaximumLength(512);
    }
}

public sealed class CreateRefundRequestCommandHandler : IRequestHandler<CreateRefundRequestCommand, RefundDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseARepository _gapPhaseARepository;
    private readonly GapPhaseANotificationService _notificationService;
    private readonly RefundApprovalRulesService _refundApprovalRulesService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRefundRequestCommandHandler(
        IGapPhaseARepository gapPhaseARepository,
        IBookingRepository bookingRepository,
        RefundApprovalRulesService refundApprovalRulesService,
        GapPhaseANotificationService notificationService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _gapPhaseARepository = gapPhaseARepository;
        _bookingRepository = bookingRepository;
        _refundApprovalRulesService = refundApprovalRulesService;
        _notificationService = notificationService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<RefundDetailResponse> Handle(CreateRefundRequestCommand request, CancellationToken cancellationToken)
    {
        var cancellationRecord = await _gapPhaseARepository.GetCancellationRecordByIdForUpdateAsync(request.CancellationRecordId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The cancellation record could not be found.", 404);

        if (await _gapPhaseARepository.GetRefundRequestByCancellationRecordIdAsync(request.CancellationRecordId, cancellationToken) is not null)
        {
            throw new AppException(ErrorCodes.Conflict, "A refund request already exists for this cancellation.", 409);
        }

        var bookingId = cancellationRecord.BookingId
            ?? throw new AppException(ErrorCodes.Conflict, "The cancellation record is not linked to a booking.", 409);
        var booking = await _bookingRepository.GetByIdForUpdateAsync(bookingId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The linked booking could not be found.", 404);
        var invoiceHeader = ResolveInvoiceHeader(booking, request.InvoiceId);
        if (invoiceHeader is null)
        {
            throw new AppException(ErrorCodes.NotFound, "The linked invoice could not be found for refund creation.", 404);
        }

        var approvalDecision = await _refundApprovalRulesService.EvaluateAsync(invoiceHeader, request.RefundAmount, cancellationToken);
        var totalApprovedRefund = await _gapPhaseARepository.GetTotalApprovedRefundAmountAsync(invoiceHeader.InvoiceHeaderId, cancellationToken);
        var maxAllowedAmount = Math.Max(Math.Min(cancellationRecord.RefundEligibleAmount, invoiceHeader.PaidAmount - totalApprovedRefund), 0m);
        if (request.RefundAmount > maxAllowedAmount)
        {
            throw new AppException(ErrorCodes.RefundLimitExceeded, "The refund request exceeds the allowed paid amount.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();
        var refundRequest = new RefundRequest
        {
            CancellationRecordId = cancellationRecord.CancellationRecordId,
            InvoiceHeaderId = invoiceHeader.InvoiceHeaderId,
            PaymentTransactionId = approvalDecision.PaymentTransactionId,
            RefundRequestNo = PhaseDCancellationRefundSupport.BuildRefundRequestNumber(now),
            RefundMethod = PhaseDCancellationRefundSupport.ParseRefundMethod(request.RefundMethod),
            RefundAmount = request.RefundAmount,
            RequestedAmount = request.RefundAmount,
            ApprovedAmount = approvalDecision.ApprovalRequired ? 0m : request.RefundAmount,
            MaxAllowedAmount = maxAllowedAmount,
            RefundReason = request.RefundReason.Trim(),
            RefundStatus = approvalDecision.ApprovalRequired ? RefundStatus.PendingApproval : RefundStatus.Approved,
            ApprovalRequiredFlag = approvalDecision.ApprovalRequired,
            RequestedDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        await _gapPhaseARepository.AddRefundRequestAsync(refundRequest, cancellationToken);
        var createStatusHistory = PhaseDCancellationRefundSupport.BuildRefundStatusHistory(
            refundRequest,
            "None",
            refundRequest.RefundStatus.ToString(),
            _currentUserContext.UserId ?? 0,
            "Refund request created.",
            now,
            actor,
            _currentUserContext.IPAddress);
        refundRequest.StatusHistory.Add(createStatusHistory);
        await _gapPhaseARepository.AddRefundStatusHistoryAsync(createStatusHistory, cancellationToken);

        cancellationRecord.CancellationStatus = CancellationStatus.RefundPending;
        cancellationRecord.UpdatedBy = actor;
        cancellationRecord.LastUpdated = now;

        await AddWorkflowHistoryAsync(refundRequest.RefundRequestNo, "None", refundRequest.RefundStatus.ToString(), "Refund request created.", cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreateRefundRequest",
                EntityName = nameof(RefundRequest),
                EntityId = refundRequest.RefundRequestNo,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = $"{request.RefundAmount:0.00}",
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _notificationService.RaiseAlertAsync(
            $"refund-request-{refundRequest.RefundRequestNo}",
            "refund.request.created",
            "RefundRequested",
            nameof(RefundRequest),
            refundRequest.RefundRequestNo,
            SystemAlertSeverity.Warning,
            $"Refund request {refundRequest.RefundRequestNo} was created.",
            now.AddHours(4),
            1,
            "CustomerSupportExecutive>Admin",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PhaseDCancellationRefundSupport.MapRefundDetail(refundRequest);
    }

    private async Task AddWorkflowHistoryAsync(
        string entityReference,
        string fromStatus,
        string toStatus,
        string remarks,
        CancellationToken cancellationToken)
    {
        await _gapPhaseARepository.AddWorkflowStatusHistoryAsync(
            new WorkflowStatusHistory
            {
                EntityType = WorkflowEntityType.Refund,
                EntityReference = entityReference,
                PreviousStatus = fromStatus,
                CurrentStatus = toStatus,
                Remarks = remarks,
                ChangedByRole = _currentUserContext.Roles.FirstOrDefault() ?? RoleNames.Admin,
                ChangedDateUtc = _currentDateTime.UtcNow,
                CreatedBy = ResolveActor(),
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
    }

    private static InvoiceHeader? ResolveInvoiceHeader(DomainBooking booking, long? requestedInvoiceId)
    {
        var invoices = booking.ServiceRequest?.JobCard?.Quotations
            .Where(item => !item.IsDeleted && item.InvoiceHeader is { IsDeleted: false })
            .OrderByDescending(item => item.QuotationDateUtc)
            .Select(item => item.InvoiceHeader!)
            .ToArray() ?? Array.Empty<InvoiceHeader>();

        if (requestedInvoiceId.HasValue)
        {
            return invoices.FirstOrDefault(invoice => invoice.InvoiceHeaderId == requestedInvoiceId.Value);
        }

        return invoices.FirstOrDefault();
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "Refund" : _currentUserContext.UserName;
    }
}

public sealed record ApproveRefundRequestCommand(long RefundRequestId, decimal? ApprovedAmount, string Remarks) : IRequest<RefundDetailResponse>;

public sealed class ApproveRefundRequestCommandValidator : AbstractValidator<ApproveRefundRequestCommand>
{
    public ApproveRefundRequestCommandValidator()
    {
        RuleFor(request => request.RefundRequestId).GreaterThan(0);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class ApproveRefundRequestCommandHandler : IRequestHandler<ApproveRefundRequestCommand, RefundDetailResponse>
{
    private static readonly IReadOnlyCollection<string> FinanceApprovalRoles =
    [
        RoleNames.SuperAdmin,
        RoleNames.Admin
    ];

    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseARepository _gapPhaseARepository;
    private readonly GapPhaseANotificationService _notificationService;
    private readonly RefundApprovalRulesService _refundApprovalRulesService;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveRefundRequestCommandHandler(
        IGapPhaseARepository gapPhaseARepository,
        RefundApprovalRulesService refundApprovalRulesService,
        GapPhaseANotificationService notificationService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _gapPhaseARepository = gapPhaseARepository;
        _refundApprovalRulesService = refundApprovalRulesService;
        _notificationService = notificationService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<RefundDetailResponse> Handle(ApproveRefundRequestCommand request, CancellationToken cancellationToken)
    {
        var refundRequest = await _gapPhaseARepository.GetRefundRequestByIdForUpdateAsync(request.RefundRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The refund request could not be found.", 404);
        var cancellationRecord = refundRequest.CancellationRecordId.HasValue
            ? await _gapPhaseARepository.GetCancellationRecordByIdForUpdateAsync(refundRequest.CancellationRecordId.Value, cancellationToken)
            : null;
        if (refundRequest.InvoiceHeader is null)
        {
            throw new AppException(ErrorCodes.NotFound, "The linked invoice could not be found.", 404);
        }

        var approvedAmount = request.ApprovedAmount ?? refundRequest.RefundAmount;
        if (approvedAmount > refundRequest.MaxAllowedAmount)
        {
            throw new AppException(ErrorCodes.RefundLimitExceeded, "The approved amount exceeds the allowed limit.", 409);
        }

        var approvalDecision = await _refundApprovalRulesService.EvaluateAsync(refundRequest.InvoiceHeader, approvedAmount, cancellationToken);
        if (approvalDecision.FinanceApprovalRequired && !_currentUserContext.Roles.Any(FinanceApprovalRoles.Contains))
        {
            throw new AppException(ErrorCodes.Forbidden, "Cash-origin refunds require an admin finance approval actor.", 403);
        }

        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();
        var previousStatus = refundRequest.RefundStatus;
        refundRequest.ApprovedAmount = approvedAmount;
        refundRequest.ApprovedByUserId = _currentUserContext.UserId;
        refundRequest.ApprovedDateUtc = now;
        refundRequest.ApprovalRemarks = request.Remarks?.Trim() ?? string.Empty;
        refundRequest.RefundStatus = RefundStatus.Approved;
        refundRequest.UpdatedBy = actor;
        refundRequest.LastUpdated = now;

        var approvalEntry = new RefundApproval
        {
            RefundRequestId = refundRequest.RefundRequestId,
            ApprovalLevel = 1,
            ApproverUserId = _currentUserContext.UserId ?? 0,
            ApprovalStatus = "Approved",
            ApprovalRemarks = refundRequest.ApprovalRemarks,
            ApprovedOn = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };
        refundRequest.Approvals.Add(approvalEntry);
        await _gapPhaseARepository.AddRefundApprovalAsync(approvalEntry, cancellationToken);
        var approvalStatusHistory = PhaseDCancellationRefundSupport.BuildRefundStatusHistory(
            refundRequest,
            previousStatus.ToString(),
            RefundStatus.Approved.ToString(),
            _currentUserContext.UserId ?? 0,
            string.IsNullOrWhiteSpace(refundRequest.ApprovalRemarks) ? "Refund approved." : refundRequest.ApprovalRemarks,
            now,
            actor,
            _currentUserContext.IPAddress);
        refundRequest.StatusHistory.Add(approvalStatusHistory);
        await _gapPhaseARepository.AddRefundStatusHistoryAsync(approvalStatusHistory, cancellationToken);

        if (cancellationRecord is not null)
        {
            cancellationRecord.CancellationStatus = CancellationStatus.RefundApproved;
            cancellationRecord.ApprovedByUserId = _currentUserContext.UserId;
            cancellationRecord.ApprovedDateUtc = now;
            cancellationRecord.ApprovalRemarks = refundRequest.ApprovalRemarks;
            cancellationRecord.UpdatedBy = actor;
            cancellationRecord.LastUpdated = now;
        }

        await AddWorkflowHistoryAsync(refundRequest.RefundRequestNo, previousStatus.ToString(), RefundStatus.Approved.ToString(), refundRequest.ApprovalRemarks, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "ApproveRefundRequest",
                EntityName = nameof(RefundRequest),
                EntityId = refundRequest.RefundRequestNo,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = $"{approvedAmount:0.00}",
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _notificationService.RaiseAlertAsync(
            $"refund-approved-{refundRequest.RefundRequestNo}",
            "refund.approved",
            "RefundApproved",
            nameof(RefundRequest),
            refundRequest.RefundRequestNo,
            SystemAlertSeverity.Warning,
            $"Refund request {refundRequest.RefundRequestNo} was approved.",
            now.AddHours(8),
            1,
            "CustomerSupportExecutive>Admin",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PhaseDCancellationRefundSupport.MapRefundDetail(refundRequest);
    }

    private async Task AddWorkflowHistoryAsync(
        string entityReference,
        string fromStatus,
        string toStatus,
        string remarks,
        CancellationToken cancellationToken)
    {
        await _gapPhaseARepository.AddWorkflowStatusHistoryAsync(
            new WorkflowStatusHistory
            {
                EntityType = WorkflowEntityType.Refund,
                EntityReference = entityReference,
                PreviousStatus = fromStatus,
                CurrentStatus = toStatus,
                Remarks = remarks,
                ChangedByRole = _currentUserContext.Roles.FirstOrDefault() ?? RoleNames.Admin,
                ChangedDateUtc = _currentDateTime.UtcNow,
                CreatedBy = ResolveActor(),
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "RefundApproval" : _currentUserContext.UserName;
    }
}

public sealed record RejectRefundRequestCommand(long RefundRequestId, string Remarks) : IRequest<RefundDetailResponse>;

public sealed class RejectRefundRequestCommandValidator : AbstractValidator<RejectRefundRequestCommand>
{
    public RejectRefundRequestCommandValidator()
    {
        RuleFor(request => request.RefundRequestId).GreaterThan(0);
        RuleFor(request => request.Remarks).NotEmpty().MaximumLength(512);
    }
}

public sealed class RejectRefundRequestCommandHandler : IRequestHandler<RejectRefundRequestCommand, RefundDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseARepository _gapPhaseARepository;
    private readonly GapPhaseANotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public RejectRefundRequestCommandHandler(
        IGapPhaseARepository gapPhaseARepository,
        GapPhaseANotificationService notificationService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _gapPhaseARepository = gapPhaseARepository;
        _notificationService = notificationService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<RefundDetailResponse> Handle(RejectRefundRequestCommand request, CancellationToken cancellationToken)
    {
        var refundRequest = await _gapPhaseARepository.GetRefundRequestByIdForUpdateAsync(request.RefundRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The refund request could not be found.", 404);
        var cancellationRecord = refundRequest.CancellationRecordId.HasValue
            ? await _gapPhaseARepository.GetCancellationRecordByIdForUpdateAsync(refundRequest.CancellationRecordId.Value, cancellationToken)
            : null;
        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();
        var previousStatus = refundRequest.RefundStatus;

        refundRequest.RefundStatus = RefundStatus.Rejected;
        refundRequest.ApprovalRemarks = request.Remarks.Trim();
        refundRequest.ApprovedByUserId = _currentUserContext.UserId;
        refundRequest.ApprovedDateUtc = now;
        refundRequest.UpdatedBy = actor;
        refundRequest.LastUpdated = now;

        var rejectionEntry = new RefundApproval
        {
            RefundRequestId = refundRequest.RefundRequestId,
            ApprovalLevel = 1,
            ApproverUserId = _currentUserContext.UserId ?? 0,
            ApprovalStatus = "Rejected",
            ApprovalRemarks = refundRequest.ApprovalRemarks,
            ApprovedOn = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };
        refundRequest.Approvals.Add(rejectionEntry);
        await _gapPhaseARepository.AddRefundApprovalAsync(rejectionEntry, cancellationToken);
        var rejectionStatusHistory = PhaseDCancellationRefundSupport.BuildRefundStatusHistory(
            refundRequest,
            previousStatus.ToString(),
            RefundStatus.Rejected.ToString(),
            _currentUserContext.UserId ?? 0,
            refundRequest.ApprovalRemarks,
            now,
            actor,
            _currentUserContext.IPAddress);
        refundRequest.StatusHistory.Add(rejectionStatusHistory);
        await _gapPhaseARepository.AddRefundStatusHistoryAsync(rejectionStatusHistory, cancellationToken);

        if (cancellationRecord is not null)
        {
            cancellationRecord.CancellationStatus = CancellationStatus.RefundRejected;
            cancellationRecord.ApprovalRemarks = refundRequest.ApprovalRemarks;
            cancellationRecord.UpdatedBy = actor;
            cancellationRecord.LastUpdated = now;
        }

        await AddWorkflowHistoryAsync(refundRequest.RefundRequestNo, previousStatus.ToString(), RefundStatus.Rejected.ToString(), refundRequest.ApprovalRemarks, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "RejectRefundRequest",
                EntityName = nameof(RefundRequest),
                EntityId = refundRequest.RefundRequestNo,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = refundRequest.ApprovalRemarks,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _notificationService.RaiseAlertAsync(
            $"refund-rejected-{refundRequest.RefundRequestNo}",
            "refund.rejected",
            "RefundRejected",
            nameof(RefundRequest),
            refundRequest.RefundRequestNo,
            SystemAlertSeverity.Warning,
            $"Refund request {refundRequest.RefundRequestNo} was rejected.",
            now.AddHours(8),
            1,
            "CustomerSupportExecutive>Admin",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PhaseDCancellationRefundSupport.MapRefundDetail(refundRequest);
    }

    private async Task AddWorkflowHistoryAsync(
        string entityReference,
        string fromStatus,
        string toStatus,
        string remarks,
        CancellationToken cancellationToken)
    {
        await _gapPhaseARepository.AddWorkflowStatusHistoryAsync(
            new WorkflowStatusHistory
            {
                EntityType = WorkflowEntityType.Refund,
                EntityReference = entityReference,
                PreviousStatus = fromStatus,
                CurrentStatus = toStatus,
                Remarks = remarks,
                ChangedByRole = _currentUserContext.Roles.FirstOrDefault() ?? RoleNames.Admin,
                ChangedDateUtc = _currentDateTime.UtcNow,
                CreatedBy = ResolveActor(),
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "RefundRejection" : _currentUserContext.UserName;
    }
}

public sealed record UpdateRefundStatusCommand(long RefundRequestId, string RefundStatus, string Remarks) : IRequest<RefundDetailResponse>;

public sealed class UpdateRefundStatusCommandValidator : AbstractValidator<UpdateRefundStatusCommand>
{
    public UpdateRefundStatusCommandValidator()
    {
        RuleFor(request => request.RefundRequestId).GreaterThan(0);
        RuleFor(request => request.RefundStatus).NotEmpty().MaximumLength(64);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class UpdateRefundStatusCommandHandler : IRequestHandler<UpdateRefundStatusCommand, RefundDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseARepository _gapPhaseARepository;
    private readonly GapPhaseANotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRefundStatusCommandHandler(
        IGapPhaseARepository gapPhaseARepository,
        GapPhaseANotificationService notificationService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _gapPhaseARepository = gapPhaseARepository;
        _notificationService = notificationService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<RefundDetailResponse> Handle(UpdateRefundStatusCommand request, CancellationToken cancellationToken)
    {
        var refundRequest = await _gapPhaseARepository.GetRefundRequestByIdForUpdateAsync(request.RefundRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The refund request could not be found.", 404);
        if (!Enum.TryParse<RefundStatus>(request.RefundStatus, true, out var targetStatus))
        {
            throw new AppException(ErrorCodes.ValidationFailure, "The requested refund status is invalid.", 400);
        }

        if (targetStatus == RefundStatus.Processed && refundRequest.ApprovalRequiredFlag && refundRequest.RefundStatus != RefundStatus.Approved)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "Refund cannot be processed before approval.", 409);
        }

        var cancellationRecord = refundRequest.CancellationRecordId.HasValue
            ? await _gapPhaseARepository.GetCancellationRecordByIdForUpdateAsync(refundRequest.CancellationRecordId.Value, cancellationToken)
            : null;
        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();
        var previousStatus = refundRequest.RefundStatus;

        refundRequest.RefundStatus = targetStatus;
        refundRequest.ProcessedOn = targetStatus is RefundStatus.Processed or RefundStatus.Closed ? now : refundRequest.ProcessedOn;
        refundRequest.ApprovalRemarks = string.IsNullOrWhiteSpace(request.Remarks) ? refundRequest.ApprovalRemarks : request.Remarks.Trim();
        refundRequest.UpdatedBy = actor;
        refundRequest.LastUpdated = now;

        var updateStatusHistory = PhaseDCancellationRefundSupport.BuildRefundStatusHistory(
            refundRequest,
            previousStatus.ToString(),
            targetStatus.ToString(),
            _currentUserContext.UserId ?? 0,
            string.IsNullOrWhiteSpace(request.Remarks) ? $"Refund moved to {targetStatus}." : request.Remarks.Trim(),
            now,
            actor,
            _currentUserContext.IPAddress);
        refundRequest.StatusHistory.Add(updateStatusHistory);
        await _gapPhaseARepository.AddRefundStatusHistoryAsync(updateStatusHistory, cancellationToken);

        if (cancellationRecord is not null)
        {
            cancellationRecord.CancellationStatus = targetStatus switch
            {
                RefundStatus.Processed => CancellationStatus.RefundProcessed,
                RefundStatus.Closed => CancellationStatus.Closed,
                RefundStatus.Approved => CancellationStatus.RefundApproved,
                RefundStatus.Rejected => CancellationStatus.RefundRejected,
                _ => CancellationStatus.RefundPending
            };
            cancellationRecord.UpdatedBy = actor;
            cancellationRecord.LastUpdated = now;
        }

        await AddWorkflowHistoryAsync(refundRequest.RefundRequestNo, previousStatus.ToString(), targetStatus.ToString(), request.Remarks ?? string.Empty, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "UpdateRefundStatus",
                EntityName = nameof(RefundRequest),
                EntityId = refundRequest.RefundRequestNo,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = targetStatus.ToString(),
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        if (targetStatus == RefundStatus.Processed)
        {
            await _notificationService.RaiseAlertAsync(
                $"refund-processed-{refundRequest.RefundRequestNo}",
                "refund.processed",
                "RefundProcessed",
                nameof(RefundRequest),
                refundRequest.RefundRequestNo,
                SystemAlertSeverity.Warning,
                $"Refund request {refundRequest.RefundRequestNo} was processed.",
                now.AddHours(8),
                1,
                "CustomerSupportExecutive>Admin",
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PhaseDCancellationRefundSupport.MapRefundDetail(refundRequest);
    }

    private async Task AddWorkflowHistoryAsync(
        string entityReference,
        string fromStatus,
        string toStatus,
        string remarks,
        CancellationToken cancellationToken)
    {
        await _gapPhaseARepository.AddWorkflowStatusHistoryAsync(
            new WorkflowStatusHistory
            {
                EntityType = WorkflowEntityType.Refund,
                EntityReference = entityReference,
                PreviousStatus = fromStatus,
                CurrentStatus = toStatus,
                Remarks = remarks,
                ChangedByRole = _currentUserContext.Roles.FirstOrDefault() ?? RoleNames.Admin,
                ChangedDateUtc = _currentDateTime.UtcNow,
                CreatedBy = ResolveActor(),
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "RefundStatus" : _currentUserContext.UserName;
    }
}

public sealed record GetRefundRequestListQuery(
    string? RefundStatus,
    long? CustomerId,
    int? BranchId,
    DateTime? FromDateUtc,
    DateTime? ToDateUtc) : IRequest<IReadOnlyCollection<RefundListItemResponse>>;

public sealed class GetRefundRequestListQueryHandler : IRequestHandler<GetRefundRequestListQuery, IReadOnlyCollection<RefundListItemResponse>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseARepository _gapPhaseARepository;

    public GetRefundRequestListQueryHandler(
        IGapPhaseARepository gapPhaseARepository,
        IBookingRepository bookingRepository,
        ICurrentUserContext currentUserContext)
    {
        _gapPhaseARepository = gapPhaseARepository;
        _bookingRepository = bookingRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<RefundListItemResponse>> Handle(GetRefundRequestListQuery request, CancellationToken cancellationToken)
    {
        long? effectiveCustomerId = request.CustomerId;
        if (_currentUserContext.Roles.Contains(RoleNames.Customer))
        {
            if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
            {
                throw new AppException(ErrorCodes.Unauthorized, "Customer authentication is required.", 401);
            }

            var customer = await _bookingRepository.GetCustomerByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.BookingAccessDenied, "The current customer could not be resolved.", 403);
            effectiveCustomerId = customer.CustomerId;
        }

        var refundRequests = await _gapPhaseARepository.SearchRefundRequestsAsync(
            request.RefundStatus,
            effectiveCustomerId,
            request.BranchId,
            request.FromDateUtc,
            request.ToDateUtc,
            cancellationToken);

        return refundRequests
            .Select(PhaseDCancellationRefundSupport.MapRefundListItem)
            .ToArray();
    }
}

public sealed record GetRefundRequestDetailQuery(long RefundRequestId) : IRequest<RefundDetailResponse>;

public sealed class GetRefundRequestDetailQueryHandler : IRequestHandler<GetRefundRequestDetailQuery, RefundDetailResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseARepository _gapPhaseARepository;

    public GetRefundRequestDetailQueryHandler(
        IGapPhaseARepository gapPhaseARepository,
        IBookingRepository bookingRepository,
        ICurrentUserContext currentUserContext)
    {
        _gapPhaseARepository = gapPhaseARepository;
        _bookingRepository = bookingRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<RefundDetailResponse> Handle(GetRefundRequestDetailQuery request, CancellationToken cancellationToken)
    {
        var refundRequest = await _gapPhaseARepository.GetRefundRequestByIdAsync(request.RefundRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The refund request could not be found.", 404);

        if (_currentUserContext.Roles.Contains(RoleNames.Customer))
        {
            if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
            {
                throw new AppException(ErrorCodes.Unauthorized, "Customer authentication is required.", 401);
            }

            var customer = await _bookingRepository.GetCustomerByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.BookingAccessDenied, "The current customer could not be resolved.", 403);
            if (refundRequest.InvoiceHeader?.CustomerId != customer.CustomerId)
            {
                throw new AppException(ErrorCodes.BillingAccessDenied, "This refund request does not belong to the current customer.", 403);
            }
        }

        return PhaseDCancellationRefundSupport.MapRefundDetail(refundRequest);
    }
}

public sealed record GetCustomerRefundStatusQuery(long CustomerId) : IRequest<IReadOnlyCollection<CustomerRefundStatusResponse>>;

public sealed class GetCustomerRefundStatusQueryHandler : IRequestHandler<GetCustomerRefundStatusQuery, IReadOnlyCollection<CustomerRefundStatusResponse>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseARepository _gapPhaseARepository;

    public GetCustomerRefundStatusQueryHandler(
        IGapPhaseARepository gapPhaseARepository,
        IBookingRepository bookingRepository,
        ICurrentUserContext currentUserContext)
    {
        _gapPhaseARepository = gapPhaseARepository;
        _bookingRepository = bookingRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<CustomerRefundStatusResponse>> Handle(GetCustomerRefundStatusQuery request, CancellationToken cancellationToken)
    {
        var effectiveCustomerId = request.CustomerId;
        if (_currentUserContext.Roles.Contains(RoleNames.Customer))
        {
            if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
            {
                throw new AppException(ErrorCodes.Unauthorized, "Customer authentication is required.", 401);
            }

            var customer = await _bookingRepository.GetCustomerByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.BookingAccessDenied, "The current customer could not be resolved.", 403);
            effectiveCustomerId = customer.CustomerId;
        }

        var refundRequests = await _gapPhaseARepository.SearchRefundRequestsAsync(
            null,
            effectiveCustomerId,
            null,
            null,
            null,
            cancellationToken);

        return refundRequests
            .Select(PhaseDCancellationRefundSupport.MapCustomerRefundStatus)
            .ToArray();
    }
}
