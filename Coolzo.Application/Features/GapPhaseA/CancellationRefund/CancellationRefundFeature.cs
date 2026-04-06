using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.GapPhaseA;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.GapPhaseA.CancellationRefund;

public sealed record CancelServiceRequestCommand(
    long ServiceRequestId,
    string ReasonCode,
    string ReasonDescription,
    bool RequiresApproval) : IRequest<CancellationRecordResponse>;

public sealed class CancelServiceRequestCommandValidator : AbstractValidator<CancelServiceRequestCommand>
{
    public CancelServiceRequestCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.ReasonCode).NotEmpty().MaximumLength(64);
        RuleFor(request => request.ReasonDescription).NotEmpty().MaximumLength(512);
    }
}

public sealed class CancelServiceRequestCommandHandler : IRequestHandler<CancelServiceRequestCommand, CancellationRecordResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly GapPhaseANotificationService _notificationService;
    private readonly IGapPhaseARepository _repository;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly GapPhaseAWorkflowService _workflowService;
    private readonly IUnitOfWork _unitOfWork;

    public CancelServiceRequestCommandHandler(
        IGapPhaseARepository repository,
        IServiceRequestRepository serviceRequestRepository,
        GapPhaseAWorkflowService workflowService,
        GapPhaseANotificationService notificationService,
        GapPhaseAFeatureFlagService featureFlagService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _serviceRequestRepository = serviceRequestRepository;
        _workflowService = workflowService;
        _notificationService = notificationService;
        _featureFlagService = featureFlagService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CancellationRecordResponse> Handle(CancelServiceRequestCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.cancellation.enabled", cancellationToken);

        var serviceRequest = await _serviceRequestRepository.GetByIdForUpdateAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);

        if (await _repository.GetCancellationByServiceRequestIdAsync(request.ServiceRequestId, cancellationToken) is not null)
        {
            throw new AppException(ErrorCodes.CancellationAlreadyExists, "A cancellation record already exists for this service request.", 409);
        }

        var estimatedValue = serviceRequest.Booking?.EstimatedPrice ?? 0.00m;
        var cancellationFee = serviceRequest.CurrentStatus is ServiceRequestStatus.EnRoute or ServiceRequestStatus.Reached or ServiceRequestStatus.WorkStarted or ServiceRequestStatus.WorkInProgress
            ? Math.Round(estimatedValue * 0.10m, 2)
            : 0.00m;
        var refundEligible = Math.Max(estimatedValue - cancellationFee, 0.00m);
        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();
        var cancellationRecord = new CancellationRecord
        {
            ServiceRequestId = serviceRequest.ServiceRequestId,
            BookingId = serviceRequest.BookingId,
            CancellationStatus = request.RequiresApproval ? CancellationStatus.PendingApproval : CancellationStatus.Approved,
            PolicyCode = cancellationFee > 0 ? "FIELD_VISIT_STARTED" : "FREE_CANCELLATION",
            ReasonCode = request.ReasonCode.Trim(),
            ReasonDescription = request.ReasonDescription.Trim(),
            CancellationFeeAmount = cancellationFee,
            RefundEligibleAmount = refundEligible,
            RequiresApproval = request.RequiresApproval,
            RequestedByRole = _currentUserContext.Roles.FirstOrDefault() ?? RoleNames.Customer,
            RequestedDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        await _repository.AddCancellationRecordAsync(cancellationRecord, cancellationToken);
        await _workflowService.EnsureServiceRequestTransitionAsync(serviceRequest, ServiceRequestStatus.Cancelled, "Service request cancelled.", cancellationToken);
        serviceRequest.StatusHistories.Add(new ServiceRequestStatusHistory
        {
            Status = ServiceRequestStatus.Cancelled,
            Remarks = request.ReasonDescription.Trim(),
            StatusDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });

        await _notificationService.RaiseAlertAsync(
            "payment.reminder",
            "payment.reminder",
            "Cancellation",
            nameof(ServiceRequest),
            serviceRequest.ServiceRequestNumber,
            SystemAlertSeverity.Warning,
            $"Cancellation recorded for service request {serviceRequest.ServiceRequestNumber}.",
            now.AddHours(1),
            request.RequiresApproval ? 2 : 1,
            "OperationsExecutive>OperationsManager",
            cancellationToken);
        await WriteAuditAsync("CancelServiceRequest", serviceRequest.ServiceRequestNumber, request.ReasonCode.Trim(), actor, now, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CancellationRefundMapper.MapCancellation(cancellationRecord);
    }

    private Task WriteAuditAsync(string actionName, string entityId, string newValues, string actor, DateTime now, CancellationToken cancellationToken)
    {
        return _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = actionName,
                EntityName = nameof(CancellationRecord),
                EntityId = entityId,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = newValues,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "Cancellation" : _currentUserContext.UserName;
    }
}

public sealed record InitiateRefundCommand(
    long CancellationRecordId,
    long InvoiceId,
    decimal RequestedAmount,
    string Reason) : IRequest<RefundRequestResponse>;

public sealed class InitiateRefundCommandValidator : AbstractValidator<InitiateRefundCommand>
{
    public InitiateRefundCommandValidator()
    {
        RuleFor(request => request.CancellationRecordId).GreaterThan(0);
        RuleFor(request => request.InvoiceId).GreaterThan(0);
        RuleFor(request => request.RequestedAmount).GreaterThan(0.00m);
        RuleFor(request => request.Reason).NotEmpty().MaximumLength(512);
    }
}

public sealed class InitiateRefundCommandHandler : IRequestHandler<InitiateRefundCommand, RefundRequestResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBillingRepository _billingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly IGapPhaseARepository _repository;
    private readonly GapPhaseAValidationService _validationService;
    private readonly IUnitOfWork _unitOfWork;

    public InitiateRefundCommandHandler(
        IGapPhaseARepository repository,
        IBillingRepository billingRepository,
        GapPhaseAValidationService validationService,
        GapPhaseAFeatureFlagService featureFlagService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _billingRepository = billingRepository;
        _validationService = validationService;
        _featureFlagService = featureFlagService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<RefundRequestResponse> Handle(InitiateRefundCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.refund.enabled", cancellationToken);

        var cancellationRecord = await _repository.GetCancellationRecordByIdAsync(request.CancellationRecordId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The cancellation record could not be found.", 404);
        var invoiceHeader = await _billingRepository.GetInvoiceByIdForUpdateAsync(request.InvoiceId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The invoice could not be found.", 404);

        _validationService.ValidateInvoiceApproval(invoiceHeader);

        var totalApprovedRefund = await _repository.GetTotalApprovedRefundAmountAsync(invoiceHeader.InvoiceHeaderId, cancellationToken);
        var maxAllowed = Math.Max(Math.Min(cancellationRecord.RefundEligibleAmount, invoiceHeader.PaidAmount - totalApprovedRefund), 0.00m);
        _validationService.ValidateRefundLimit(request.RequestedAmount, maxAllowed);

        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();
        var refundRequest = new RefundRequest
        {
            CancellationRecordId = cancellationRecord.CancellationRecordId,
            InvoiceHeaderId = invoiceHeader.InvoiceHeaderId,
            RefundStatus = cancellationRecord.RequiresApproval ? RefundStatus.PendingApproval : RefundStatus.Approved,
            RequestedAmount = request.RequestedAmount,
            ApprovedAmount = cancellationRecord.RequiresApproval ? 0.00m : request.RequestedAmount,
            MaxAllowedAmount = maxAllowed,
            RefundReason = request.Reason.Trim(),
            RequestedDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        await _repository.AddRefundRequestAsync(refundRequest, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "InitiateRefund",
                EntityName = nameof(RefundRequest),
                EntityId = invoiceHeader.InvoiceNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = $"{request.RequestedAmount:0.00}",
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CancellationRefundMapper.MapRefund(refundRequest);
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "Refund" : _currentUserContext.UserName;
    }
}

public sealed record ApproveRefundCommand(
    long RefundRequestId,
    decimal ApprovedAmount,
    string? Remarks) : IRequest<RefundRequestResponse>;

public sealed class ApproveRefundCommandValidator : AbstractValidator<ApproveRefundCommand>
{
    public ApproveRefundCommandValidator()
    {
        RuleFor(request => request.RefundRequestId).GreaterThan(0);
        RuleFor(request => request.ApprovedAmount).GreaterThan(0.00m);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class ApproveRefundCommandHandler : IRequestHandler<ApproveRefundCommand, RefundRequestResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly IGapPhaseARepository _repository;
    private readonly GapPhaseAValidationService _validationService;
    private readonly GapPhaseAWorkflowService _workflowService;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveRefundCommandHandler(
        IGapPhaseARepository repository,
        GapPhaseAValidationService validationService,
        GapPhaseAWorkflowService workflowService,
        GapPhaseAFeatureFlagService featureFlagService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _validationService = validationService;
        _workflowService = workflowService;
        _featureFlagService = featureFlagService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<RefundRequestResponse> Handle(ApproveRefundCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.refund.enabled", cancellationToken);

        var refundRequest = await _repository.GetRefundRequestByIdForUpdateAsync(request.RefundRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The refund request could not be found.", 404);
        var cancellationRecordId = refundRequest.CancellationRecordId
            ?? throw new AppException(ErrorCodes.NotFound, "The linked cancellation record could not be found.", 404);
        var cancellationRecord = await _repository.GetCancellationRecordByIdForUpdateAsync(cancellationRecordId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The linked cancellation record could not be found.", 404);

        _validationService.ValidateRefundLimit(request.ApprovedAmount, refundRequest.MaxAllowedAmount);

        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();
        refundRequest.ApprovedAmount = request.ApprovedAmount;
        refundRequest.ApprovalRemarks = request.Remarks?.Trim() ?? string.Empty;
        refundRequest.ApprovedByUserId = _currentUserContext.UserId;
        refundRequest.ApprovedDateUtc = now;
        await _workflowService.EnsureRefundTransitionAsync(refundRequest, RefundStatus.Approved, refundRequest.ApprovalRemarks, cancellationToken);
        await _workflowService.EnsureCancellationTransitionAsync(cancellationRecord, CancellationStatus.Completed, "Refund approved and cancellation completed.", cancellationToken);

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "ApproveRefund",
                EntityName = nameof(RefundRequest),
                EntityId = refundRequest.RefundRequestId.ToString(),
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = $"{request.ApprovedAmount:0.00}",
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CancellationRefundMapper.MapRefund(refundRequest);
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "RefundApproval" : _currentUserContext.UserName;
    }
}

internal static class CancellationRefundMapper
{
    public static CancellationRecordResponse MapCancellation(CancellationRecord cancellationRecord)
    {
        return new CancellationRecordResponse(
            cancellationRecord.CancellationRecordId,
            cancellationRecord.ServiceRequestId ?? 0,
            cancellationRecord.CancellationStatus.ToString(),
            cancellationRecord.CancellationFeeAmount,
            cancellationRecord.RefundEligibleAmount,
            cancellationRecord.RequiresApproval);
    }

    public static RefundRequestResponse MapRefund(RefundRequest refundRequest)
    {
        return new RefundRequestResponse(
            refundRequest.RefundRequestId,
            refundRequest.CancellationRecordId ?? 0,
            refundRequest.InvoiceHeaderId ?? 0,
            refundRequest.RefundStatus.ToString(),
            refundRequest.RequestedAmount,
            refundRequest.ApprovedAmount);
    }
}
