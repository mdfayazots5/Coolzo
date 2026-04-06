using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.GapPhaseD;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;
using DomainBooking = Coolzo.Domain.Entities.Booking;
using DomainServiceRequest = Coolzo.Domain.Entities.ServiceRequest;

namespace Coolzo.Application.Features.GapPhaseA.CancellationRefund;

public sealed record CreateCustomerCancellationCommand(
    long? BookingId,
    long? ServiceRequestId,
    string CancellationReasonCode,
    string CancellationReasonText) : IRequest<CancellationDetailResponse>;

public sealed class CreateCustomerCancellationCommandValidator : AbstractValidator<CreateCustomerCancellationCommand>
{
    public CreateCustomerCancellationCommandValidator()
    {
        RuleFor(request => request)
            .Must(request => request.BookingId.HasValue || request.ServiceRequestId.HasValue)
            .WithMessage("A booking or service request reference is required.");
        RuleFor(request => request.CancellationReasonCode).NotEmpty().MaximumLength(64);
        RuleFor(request => request.CancellationReasonText).NotEmpty().MaximumLength(512);
    }
}

public sealed class CreateCustomerCancellationCommandHandler :
    PhaseDCancellationCommandHandlerBase,
    IRequestHandler<CreateCustomerCancellationCommand, CancellationDetailResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IServiceRequestRepository _serviceRequestRepository;

    public CreateCustomerCancellationCommandHandler(
        IBookingRepository bookingRepository,
        IServiceRequestRepository serviceRequestRepository,
        IGapPhaseARepository gapPhaseARepository,
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
        _bookingRepository = bookingRepository;
        _serviceRequestRepository = serviceRequestRepository;
    }

    public async Task<CancellationDetailResponse> Handle(CreateCustomerCancellationCommand request, CancellationToken cancellationToken)
    {
        if (!CurrentUserContext.IsAuthenticated || !CurrentUserContext.UserId.HasValue)
        {
            throw new AppException(ErrorCodes.Unauthorized, "Customer authentication is required.", 401);
        }

        var customer = await _bookingRepository.GetCustomerByUserIdAsync(CurrentUserContext.UserId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.BookingAccessDenied, "The current customer could not be resolved.", 403);
        var (booking, serviceRequest) = await LoadCustomerContextAsync(request.BookingId, request.ServiceRequestId, customer.CustomerId, cancellationToken);

        return await ExecuteCancellationAsync(
            booking,
            serviceRequest,
            "CustomerSelfService",
            request.CancellationReasonCode,
            request.CancellationReasonText,
            true,
            false,
            cancellationToken);
    }

    private async Task<(DomainBooking Booking, DomainServiceRequest? ServiceRequest)> LoadCustomerContextAsync(
        long? bookingId,
        long? serviceRequestId,
        long customerId,
        CancellationToken cancellationToken)
    {
        if (bookingId.HasValue)
        {
            var booking = await _bookingRepository.GetByIdForCustomerForUpdateAsync(bookingId.Value, customerId, cancellationToken)
                ?? throw new AppException(ErrorCodes.BookingAccessDenied, "This booking does not belong to the current customer.", 403);
            return (booking, booking.ServiceRequest);
        }

        var serviceRequest = await _serviceRequestRepository.GetByIdForUpdateAsync(serviceRequestId!.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);

        if (serviceRequest.Booking is null || serviceRequest.Booking.CustomerId != customerId)
        {
            throw new AppException(ErrorCodes.BookingAccessDenied, "This service request does not belong to the current customer.", 403);
        }

        return (serviceRequest.Booking, serviceRequest);
    }
}

public sealed record CreateAdminCancellationCommand(
    long? BookingId,
    long? ServiceRequestId,
    string CancellationSource,
    string CancellationReasonCode,
    string CancellationReasonText,
    bool ForceOverride,
    string? OverrideReason) : IRequest<CancellationDetailResponse>;

public sealed class CreateAdminCancellationCommandValidator : AbstractValidator<CreateAdminCancellationCommand>
{
    public CreateAdminCancellationCommandValidator()
    {
        RuleFor(request => request)
            .Must(request => request.BookingId.HasValue || request.ServiceRequestId.HasValue)
            .WithMessage("A booking or service request reference is required.");
        RuleFor(request => request.CancellationSource).NotEmpty().MaximumLength(64);
        RuleFor(request => request.CancellationReasonCode).NotEmpty().MaximumLength(64);
        RuleFor(request => request.CancellationReasonText).NotEmpty().MaximumLength(512);
        When(request => request.ForceOverride, () =>
        {
            RuleFor(request => request.OverrideReason).NotEmpty().MaximumLength(512);
        });
    }
}

public sealed class CreateAdminCancellationCommandHandler :
    PhaseDCancellationCommandHandlerBase,
    IRequestHandler<CreateAdminCancellationCommand, CancellationDetailResponse>
{
    private static readonly IReadOnlyCollection<string> ElevatedCancellationRoles =
    [
        RoleNames.SuperAdmin,
        RoleNames.Admin,
        RoleNames.OperationsManager
    ];

    private readonly IBookingRepository _bookingRepository;
    private readonly IServiceRequestRepository _serviceRequestRepository;

    public CreateAdminCancellationCommandHandler(
        IBookingRepository bookingRepository,
        IServiceRequestRepository serviceRequestRepository,
        IGapPhaseARepository gapPhaseARepository,
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
        _bookingRepository = bookingRepository;
        _serviceRequestRepository = serviceRequestRepository;
    }

    public async Task<CancellationDetailResponse> Handle(CreateAdminCancellationCommand request, CancellationToken cancellationToken)
    {
        var (booking, serviceRequest) = await LoadAdminContextAsync(request.BookingId, request.ServiceRequestId, cancellationToken);
        if (serviceRequest is not null &&
            serviceRequest.CurrentStatus is Coolzo.Domain.Enums.ServiceRequestStatus.Assigned or
                Coolzo.Domain.Enums.ServiceRequestStatus.EnRoute or
                Coolzo.Domain.Enums.ServiceRequestStatus.Reached or
                Coolzo.Domain.Enums.ServiceRequestStatus.WorkStarted or
                Coolzo.Domain.Enums.ServiceRequestStatus.WorkInProgress or
                Coolzo.Domain.Enums.ServiceRequestStatus.CustomerAbsent &&
            !CurrentUserContext.Roles.Any(ElevatedCancellationRoles.Contains))
        {
            throw new AppException(ErrorCodes.Forbidden, "Only an elevated operations role can cancel after technician dispatch.", 403);
        }

        return await ExecuteCancellationAsync(
            booking,
            serviceRequest,
            string.IsNullOrWhiteSpace(request.CancellationSource) ? "Admin" : request.CancellationSource.Trim(),
            request.CancellationReasonCode,
            request.CancellationReasonText,
            false,
            false,
            cancellationToken);
    }

    private async Task<(DomainBooking Booking, DomainServiceRequest? ServiceRequest)> LoadAdminContextAsync(
        long? bookingId,
        long? serviceRequestId,
        CancellationToken cancellationToken)
    {
        if (bookingId.HasValue)
        {
            var trackedBooking = await _bookingRepository.GetByIdForUpdateAsync(bookingId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.NotFound, "The requested booking could not be found.", 404);
            return (trackedBooking, trackedBooking.ServiceRequest);
        }

        var serviceRequest = await _serviceRequestRepository.GetByIdForUpdateAsync(serviceRequestId!.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);
        var booking = serviceRequest.Booking
            ?? throw new AppException(ErrorCodes.NotFound, "The linked booking could not be found.", 404);

        return (booking, serviceRequest);
    }
}

public sealed record GetCancellationOptionsQuery(long ServiceRequestId, long? BookingId) : IRequest<CancellationOptionsResponse>;

public sealed class GetCancellationOptionsQueryHandler : IRequestHandler<GetCancellationOptionsQuery, CancellationOptionsResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly CancellationPolicyEvaluationService _cancellationPolicyEvaluationService;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseARepository _gapPhaseARepository;
    private readonly IServiceRequestRepository _serviceRequestRepository;

    public GetCancellationOptionsQueryHandler(
        IBookingRepository bookingRepository,
        IServiceRequestRepository serviceRequestRepository,
        IGapPhaseARepository gapPhaseARepository,
        CancellationPolicyEvaluationService cancellationPolicyEvaluationService,
        ICurrentUserContext currentUserContext)
    {
        _bookingRepository = bookingRepository;
        _serviceRequestRepository = serviceRequestRepository;
        _gapPhaseARepository = gapPhaseARepository;
        _cancellationPolicyEvaluationService = cancellationPolicyEvaluationService;
        _currentUserContext = currentUserContext;
    }

    public async Task<CancellationOptionsResponse> Handle(GetCancellationOptionsQuery request, CancellationToken cancellationToken)
    {
        var (booking, serviceRequest) = await LoadContextAsync(request.ServiceRequestId, request.BookingId, cancellationToken);
        var evaluation = await _cancellationPolicyEvaluationService.EvaluateAsync(booking, serviceRequest, cancellationToken);
        var duplicateExists = (serviceRequest is not null &&
            await _gapPhaseARepository.GetCancellationByServiceRequestIdAsync(serviceRequest.ServiceRequestId, cancellationToken) is not null)
            || await _gapPhaseARepository.GetCancellationByBookingIdAsync(booking.BookingId, cancellationToken) is not null;

        return new CancellationOptionsResponse(
            booking.BookingId,
            serviceRequest?.ServiceRequestId,
            evaluation.PolicyCode,
            evaluation.PolicyName,
            evaluation.PolicyDescription,
            evaluation.TimeToSlotMinutes,
            evaluation.PaidAmount,
            evaluation.CancellationFee,
            evaluation.RefundEligibleAmount,
            evaluation.ApprovalRequired,
            !duplicateExists && (!_currentUserContext.Roles.Contains(RoleNames.Customer) || evaluation.CanCustomerCancel),
            duplicateExists ? "Cancellation already exists for this booking." : evaluation.CustomerDenialReason,
            evaluation.ScheduledStartUtc,
            evaluation.IsTechnicianDispatched);
    }

    private async Task<(DomainBooking Booking, DomainServiceRequest? ServiceRequest)> LoadContextAsync(
        long serviceRequestId,
        long? bookingId,
        CancellationToken cancellationToken)
    {
        if (_currentUserContext.Roles.Contains(RoleNames.Customer))
        {
            if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
            {
                throw new AppException(ErrorCodes.Unauthorized, "Customer authentication is required.", 401);
            }

            var customer = await _bookingRepository.GetCustomerByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.BookingAccessDenied, "The current customer could not be resolved.", 403);

            if (bookingId.HasValue)
            {
                var booking = await _bookingRepository.GetByIdForCustomerAsync(bookingId.Value, customer.CustomerId, cancellationToken)
                    ?? throw new AppException(ErrorCodes.BookingAccessDenied, "This booking does not belong to the current customer.", 403);
                return (booking, booking.ServiceRequest);
            }

            var serviceRequest = await _serviceRequestRepository.GetByIdAsync(serviceRequestId, cancellationToken)
                ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);

            if (serviceRequest.Booking is null || serviceRequest.Booking.CustomerId != customer.CustomerId)
            {
                throw new AppException(ErrorCodes.BookingAccessDenied, "This service request does not belong to the current customer.", 403);
            }

            return (serviceRequest.Booking, serviceRequest);
        }

        if (bookingId.HasValue)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.NotFound, "The requested booking could not be found.", 404);
            return (booking, booking.ServiceRequest);
        }

        var adminServiceRequest = await _serviceRequestRepository.GetByIdAsync(serviceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);
        var adminBooking = adminServiceRequest.Booking
            ?? throw new AppException(ErrorCodes.NotFound, "The linked booking could not be found.", 404);

        return (adminBooking, adminServiceRequest);
    }
}

public sealed record GetCancellationListQuery(
    long? BookingId,
    long? ServiceRequestId,
    string? CancellationStatus,
    string? CancellationSource,
    string? CancellationReasonCode,
    int? BranchId,
    DateTime? FromDateUtc,
    DateTime? ToDateUtc) : IRequest<IReadOnlyCollection<CancellationListItemResponse>>;

public sealed class GetCancellationListQueryHandler : IRequestHandler<GetCancellationListQuery, IReadOnlyCollection<CancellationListItemResponse>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseARepository _gapPhaseARepository;
    private readonly IServiceRequestRepository _serviceRequestRepository;

    public GetCancellationListQueryHandler(
        IGapPhaseARepository gapPhaseARepository,
        IBookingRepository bookingRepository,
        IServiceRequestRepository serviceRequestRepository,
        ICurrentUserContext currentUserContext)
    {
        _gapPhaseARepository = gapPhaseARepository;
        _bookingRepository = bookingRepository;
        _serviceRequestRepository = serviceRequestRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<CancellationListItemResponse>> Handle(GetCancellationListQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserContext.Roles.Contains(RoleNames.Customer))
        {
            if (!request.BookingId.HasValue && !request.ServiceRequestId.HasValue)
            {
                throw new AppException(ErrorCodes.Forbidden, "Customers can only query their own cancellation by booking or service request reference.", 403);
            }

            await EnsureCustomerOwnershipAsync(request.BookingId, request.ServiceRequestId, cancellationToken);
        }

        var cancellations = await _gapPhaseARepository.SearchCancellationsAsync(
            request.BookingId,
            request.ServiceRequestId,
            request.CancellationStatus,
            request.CancellationSource,
            request.CancellationReasonCode,
            request.BranchId,
            request.FromDateUtc,
            request.ToDateUtc,
            cancellationToken);

        return cancellations
            .Select(cancellation => PhaseDCancellationRefundSupport.MapCancellationListItem(
                cancellation,
                cancellation.RefundRequests
                    .Where(item => !item.IsDeleted)
                    .OrderByDescending(item => item.DateCreated)
                    .FirstOrDefault()))
            .ToArray();
    }

    private async Task EnsureCustomerOwnershipAsync(long? bookingId, long? serviceRequestId, CancellationToken cancellationToken)
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
        {
            throw new AppException(ErrorCodes.Unauthorized, "Customer authentication is required.", 401);
        }

        var customer = await _bookingRepository.GetCustomerByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.BookingAccessDenied, "The current customer could not be resolved.", 403);

        if (bookingId.HasValue)
        {
            _ = await _bookingRepository.GetByIdForCustomerAsync(bookingId.Value, customer.CustomerId, cancellationToken)
                ?? throw new AppException(ErrorCodes.BookingAccessDenied, "This booking does not belong to the current customer.", 403);
            return;
        }

        var serviceRequest = await _serviceRequestRepository.GetByIdAsync(serviceRequestId!.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);

        if (serviceRequest.Booking is null || serviceRequest.Booking.CustomerId != customer.CustomerId)
        {
            throw new AppException(ErrorCodes.BookingAccessDenied, "This service request does not belong to the current customer.", 403);
        }
    }
}

public sealed record GetCancellationDetailQuery(long CancellationRecordId) : IRequest<CancellationDetailResponse>;

public sealed class GetCancellationDetailQueryHandler : IRequestHandler<GetCancellationDetailQuery, CancellationDetailResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseARepository _gapPhaseARepository;

    public GetCancellationDetailQueryHandler(
        IGapPhaseARepository gapPhaseARepository,
        IBookingRepository bookingRepository,
        ICurrentUserContext currentUserContext)
    {
        _gapPhaseARepository = gapPhaseARepository;
        _bookingRepository = bookingRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<CancellationDetailResponse> Handle(GetCancellationDetailQuery request, CancellationToken cancellationToken)
    {
        var cancellationRecord = await _gapPhaseARepository.GetCancellationRecordByIdAsync(request.CancellationRecordId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The cancellation record could not be found.", 404);

        if (_currentUserContext.Roles.Contains(RoleNames.Customer))
        {
            if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
            {
                throw new AppException(ErrorCodes.Unauthorized, "Customer authentication is required.", 401);
            }

            var customer = await _bookingRepository.GetCustomerByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.BookingAccessDenied, "The current customer could not be resolved.", 403);
            var bookingId = cancellationRecord.BookingId
                ?? throw new AppException(ErrorCodes.BookingAccessDenied, "This cancellation is not linked to a customer booking.", 403);
            var customerBooking = await _bookingRepository.GetByIdForCustomerAsync(bookingId, customer.CustomerId, cancellationToken);
            if (customerBooking is null)
            {
                throw new AppException(ErrorCodes.BookingAccessDenied, "This cancellation does not belong to the current customer.", 403);
            }
        }

        var refundRequest = cancellationRecord.RefundRequests
            .Where(item => !item.IsDeleted)
            .OrderByDescending(item => item.DateCreated)
            .FirstOrDefault();

        return PhaseDCancellationRefundSupport.MapCancellationDetail(cancellationRecord, refundRequest);
    }
}
