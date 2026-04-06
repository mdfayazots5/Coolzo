using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.GapPhaseD;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.GapPhaseA.CancellationRefund;

public sealed record MarkCustomerAbsentCommand(
    long ServiceRequestId,
    string AbsentReasonCode,
    string AbsentReasonText,
    int AttemptCount,
    string ContactAttemptLog) : IRequest<CustomerAbsentDetailResponse>;

public sealed class MarkCustomerAbsentCommandValidator : AbstractValidator<MarkCustomerAbsentCommand>
{
    public MarkCustomerAbsentCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.AbsentReasonCode).NotEmpty().MaximumLength(64);
        RuleFor(request => request.AbsentReasonText).NotEmpty().MaximumLength(512);
        RuleFor(request => request.AttemptCount).GreaterThanOrEqualTo(3);
        RuleFor(request => request.ContactAttemptLog).NotEmpty().MaximumLength(1024);
    }
}

public sealed class MarkCustomerAbsentCommandHandler : IRequestHandler<MarkCustomerAbsentCommand, CustomerAbsentDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly CustomerAbsentOrchestrationService _customerAbsentOrchestrationService;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseANotificationService _notificationService;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public MarkCustomerAbsentCommandHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        CustomerAbsentOrchestrationService customerAbsentOrchestrationService,
        GapPhaseANotificationService notificationService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _customerAbsentOrchestrationService = customerAbsentOrchestrationService;
        _notificationService = notificationService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerAbsentDetailResponse> Handle(MarkCustomerAbsentCommand request, CancellationToken cancellationToken)
    {
        var technician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);
        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestForUpdateAsync(request.ServiceRequestId, cancellationToken);
        var record = await _customerAbsentOrchestrationService.MarkAsync(
            serviceRequest,
            technician,
            request.AbsentReasonCode,
            request.AbsentReasonText,
            request.AttemptCount,
            request.ContactAttemptLog,
            cancellationToken);
        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "MarkCustomerAbsent",
                EntityName = nameof(CustomerAbsentRecord),
                EntityId = serviceRequest.ServiceRequestNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = request.AbsentReasonCode,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _notificationService.RaiseAlertAsync(
            $"customer-absent-{serviceRequest.ServiceRequestId}-{now:yyyyMMddHHmmss}",
            "customer.absent.logged",
            "CustomerAbsentLogged",
            nameof(ServiceRequest),
            serviceRequest.ServiceRequestNumber,
            Coolzo.Domain.Enums.SystemAlertSeverity.Warning,
            $"Customer absent recorded for service request {serviceRequest.ServiceRequestNumber}.",
            now.AddHours(1),
            1,
            "CustomerSupportExecutive>OperationsManager",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PhaseDCancellationRefundSupport.MapCustomerAbsentDetail(record);
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "CustomerAbsent" : _currentUserContext.UserName;
    }
}

public sealed record RescheduleCustomerAbsentServiceRequestCommand(long ServiceRequestId, string Remarks) : IRequest<CustomerAbsentDetailResponse>;

public sealed class RescheduleCustomerAbsentServiceRequestCommandValidator : AbstractValidator<RescheduleCustomerAbsentServiceRequestCommand>
{
    public RescheduleCustomerAbsentServiceRequestCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.Remarks).NotEmpty().MaximumLength(512);
    }
}

public sealed class RescheduleCustomerAbsentServiceRequestCommandHandler :
    IRequestHandler<RescheduleCustomerAbsentServiceRequestCommand, CustomerAbsentDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly CustomerAbsentOrchestrationService _customerAbsentOrchestrationService;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseARepository _gapPhaseARepository;
    private readonly GapPhaseANotificationService _notificationService;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RescheduleCustomerAbsentServiceRequestCommandHandler(
        IServiceRequestRepository serviceRequestRepository,
        IGapPhaseARepository gapPhaseARepository,
        CustomerAbsentOrchestrationService customerAbsentOrchestrationService,
        GapPhaseANotificationService notificationService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _gapPhaseARepository = gapPhaseARepository;
        _customerAbsentOrchestrationService = customerAbsentOrchestrationService;
        _notificationService = notificationService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerAbsentDetailResponse> Handle(RescheduleCustomerAbsentServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _serviceRequestRepository.GetByIdForUpdateAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);
        var record = await _gapPhaseARepository.GetCustomerAbsentByServiceRequestIdForUpdateAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "No customer-absent record exists for this service request.", 404);
        var updatedRecord = await _customerAbsentOrchestrationService.ResolveAsRescheduledAsync(
            serviceRequest,
            record,
            request.Remarks,
            cancellationToken);
        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "ResolveCustomerAbsentReschedule",
                EntityName = nameof(CustomerAbsentRecord),
                EntityId = serviceRequest.ServiceRequestNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = request.Remarks.Trim(),
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _notificationService.RaiseAlertAsync(
            $"customer-absent-rescheduled-{serviceRequest.ServiceRequestId}-{now:yyyyMMddHHmmss}",
            "customer.absent.rescheduled",
            "CustomerAbsentRescheduled",
            nameof(ServiceRequest),
            serviceRequest.ServiceRequestNumber,
            Coolzo.Domain.Enums.SystemAlertSeverity.Warning,
            $"Customer absent service request {serviceRequest.ServiceRequestNumber} was moved to rescheduled.",
            now.AddHours(2),
            1,
            "CustomerSupportExecutive>OperationsManager",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PhaseDCancellationRefundSupport.MapCustomerAbsentDetail(updatedRecord);
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "CustomerAbsentReschedule" : _currentUserContext.UserName;
    }
}

public sealed record CancelCustomerAbsentServiceRequestCommand(
    long ServiceRequestId,
    string CancellationReasonCode,
    string CancellationReasonText,
    string Remarks) : IRequest<CancellationDetailResponse>;

public sealed class CancelCustomerAbsentServiceRequestCommandValidator : AbstractValidator<CancelCustomerAbsentServiceRequestCommand>
{
    public CancelCustomerAbsentServiceRequestCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.CancellationReasonCode).NotEmpty().MaximumLength(64);
        RuleFor(request => request.CancellationReasonText).NotEmpty().MaximumLength(512);
        RuleFor(request => request.Remarks).NotEmpty().MaximumLength(512);
    }
}

public sealed class CancelCustomerAbsentServiceRequestCommandHandler :
    PhaseDCancellationCommandHandlerBase,
    IRequestHandler<CancelCustomerAbsentServiceRequestCommand, CancellationDetailResponse>
{
    private readonly CustomerAbsentOrchestrationService _customerAbsentOrchestrationService;
    private readonly IGapPhaseARepository _gapPhaseARepository;
    private readonly IServiceRequestRepository _serviceRequestRepository;

    public CancelCustomerAbsentServiceRequestCommandHandler(
        IServiceRequestRepository serviceRequestRepository,
        IGapPhaseARepository gapPhaseARepository,
        CustomerAbsentOrchestrationService customerAbsentOrchestrationService,
        CancellationPolicyEvaluationService cancellationPolicyEvaluationService,
        RefundApprovalRulesService refundApprovalRulesService,
        GapPhaseANotificationService notificationService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
        : base(
            gapPhaseARepository,
            cancellationPolicyEvaluationService,
            refundApprovalRulesService,
            notificationService,
            auditLogRepository,
            unitOfWork,
            currentDateTime,
            currentUserContext)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _gapPhaseARepository = gapPhaseARepository;
        _customerAbsentOrchestrationService = customerAbsentOrchestrationService;
    }

    public async Task<CancellationDetailResponse> Handle(CancelCustomerAbsentServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _serviceRequestRepository.GetByIdForUpdateAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);
        var absentRecord = await _gapPhaseARepository.GetCustomerAbsentByServiceRequestIdForUpdateAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "No customer-absent record exists for this service request.", 404);
        var booking = serviceRequest.Booking
            ?? throw new AppException(ErrorCodes.NotFound, "The linked booking could not be found.", 404);

        var cancellation = await ExecuteCancellationAsync(
            booking,
            serviceRequest,
            "CustomerAbsentResolution",
            request.CancellationReasonCode,
            request.CancellationReasonText,
            false,
            true,
            cancellationToken);

        await _customerAbsentOrchestrationService.ResolveAsCancelledAsync(
            serviceRequest,
            absentRecord,
            request.Remarks,
            cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return cancellation;
    }
}

public sealed record GetCustomerAbsentDetailQuery(long ServiceRequestId) : IRequest<CustomerAbsentDetailResponse>;

public sealed class GetCustomerAbsentDetailQueryHandler : IRequestHandler<GetCustomerAbsentDetailQuery, CustomerAbsentDetailResponse>
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseARepository _gapPhaseARepository;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;

    public GetCustomerAbsentDetailQueryHandler(
        IGapPhaseARepository gapPhaseARepository,
        ITechnicianJobAccessService technicianJobAccessService,
        ICurrentUserContext currentUserContext)
    {
        _gapPhaseARepository = gapPhaseARepository;
        _technicianJobAccessService = technicianJobAccessService;
        _currentUserContext = currentUserContext;
    }

    public async Task<CustomerAbsentDetailResponse> Handle(GetCustomerAbsentDetailQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserContext.Roles.Contains(RoleNames.Technician))
        {
            _ = await _technicianJobAccessService.GetOwnedServiceRequestAsync(request.ServiceRequestId, cancellationToken);
        }

        var customerAbsentRecord = await _gapPhaseARepository.GetCustomerAbsentByServiceRequestIdAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "No customer-absent record exists for this service request.", 404);

        return PhaseDCancellationRefundSupport.MapCustomerAbsentDetail(customerAbsentRecord);
    }
}
