using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.Amc;
using Coolzo.Contracts.Responses.Revisit;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Revisit.Commands.CreateRevisitRequest;

public sealed class CreateRevisitRequestCommandHandler : IRequestHandler<CreateRevisitRequestCommand, RevisitRequestResponse>
{
    private readonly IAmcRepository _amcRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CreateRevisitRequestCommandHandler> _logger;
    private readonly ServiceLifecycleAccessService _serviceLifecycleAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRevisitRequestCommandHandler(
        IAmcRepository amcRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        ServiceLifecycleAccessService serviceLifecycleAccessService,
        IAppLogger<CreateRevisitRequestCommandHandler> logger)
    {
        _amcRepository = amcRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _serviceLifecycleAccessService = serviceLifecycleAccessService;
        _logger = logger;
    }

    public async Task<RevisitRequestResponse> Handle(CreateRevisitRequestCommand request, CancellationToken cancellationToken)
    {
        var originalJobCard = await _amcRepository.GetJobCardByIdAsync(request.OriginalJobCardId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The original job card could not be found.", 404);
        var booking = originalJobCard.ServiceRequest?.Booking
            ?? throw new AppException(ErrorCodes.NotFound, "The original booking linked to the job card could not be found.", 404);

        await _serviceLifecycleAccessService.EnsureRevisitCreateAccessAsync(booking, cancellationToken);

        if (!Enum.TryParse<RevisitType>(request.RevisitType, true, out var revisitType))
        {
            throw new AppException(ErrorCodes.ValidationFailure, "The requested revisit type is invalid.", 400);
        }

        CustomerAmc? customerAmc = null;
        WarrantyClaim? warrantyClaim = null;
        var chargeAmount = request.ChargeAmount ?? 0.00m;

        switch (revisitType)
        {
            case RevisitType.Amc:
                if (!request.CustomerAmcId.HasValue)
                {
                    throw new AppException(ErrorCodes.ValidationFailure, "CustomerAmcId is required for AMC revisit requests.", 400);
                }

                customerAmc = await _amcRepository.GetCustomerAmcByIdAsync(request.CustomerAmcId.Value, cancellationToken)
                    ?? throw new AppException(ErrorCodes.NotFound, "The requested customer AMC subscription could not be found.", 404);

                if (customerAmc.CustomerId != booking.CustomerId)
                {
                    throw new AppException(ErrorCodes.RevisitReferenceInvalid, "The AMC subscription does not belong to the booking customer.", 409);
                }

                if (customerAmc.CurrentStatus != AmcSubscriptionStatus.Active || customerAmc.EndDateUtc < _currentDateTime.UtcNow)
                {
                    throw new AppException(ErrorCodes.RevisitReferenceInvalid, "The AMC subscription is not active for revisit creation.", 409);
                }

                if (customerAmc.ConsumedVisitCount >= customerAmc.TotalVisitCount)
                {
                    throw new AppException(ErrorCodes.RevisitReferenceInvalid, "The AMC visit allocation has already been fully consumed.", 409);
                }

                chargeAmount = 0.00m;
                break;

            case RevisitType.Warranty:
                if (!request.WarrantyClaimId.HasValue)
                {
                    throw new AppException(ErrorCodes.ValidationFailure, "WarrantyClaimId is required for warranty revisit requests.", 400);
                }

                warrantyClaim = await _amcRepository.GetWarrantyClaimByIdAsync(request.WarrantyClaimId.Value, cancellationToken)
                    ?? throw new AppException(ErrorCodes.NotFound, "The requested warranty claim could not be found.", 404);

                if (warrantyClaim.CustomerId != booking.CustomerId || warrantyClaim.JobCardId != originalJobCard.JobCardId)
                {
                    throw new AppException(ErrorCodes.RevisitReferenceInvalid, "The warranty claim does not match the selected original job.", 409);
                }

                if (!warrantyClaim.IsEligible || warrantyClaim.CoverageEndDateUtc < _currentDateTime.UtcNow)
                {
                    throw new AppException(ErrorCodes.WarrantyNotEligible, "The linked warranty claim is no longer eligible for revisit creation.", 409);
                }

                chargeAmount = 0.00m;
                break;

            case RevisitType.Paid:
                if (chargeAmount <= 0.00m)
                {
                    throw new AppException(ErrorCodes.ValidationFailure, "ChargeAmount is required for paid revisit requests.", 400);
                }

                break;
        }

        var revisitRequest = new RevisitRequest
        {
            BookingId = booking.BookingId,
            CustomerId = booking.CustomerId,
            OriginalServiceRequestId = originalJobCard.ServiceRequestId,
            OriginalJobCardId = originalJobCard.JobCardId,
            CustomerAmcId = customerAmc?.CustomerAmcId,
            WarrantyClaimId = warrantyClaim?.WarrantyClaimId,
            RevisitType = revisitType,
            CurrentStatus = RevisitStatus.Requested,
            RequestedDateUtc = _currentDateTime.UtcNow,
            PreferredVisitDateUtc = request.PreferredVisitDateUtc,
            IssueSummary = request.IssueSummary.Trim(),
            RequestRemarks = request.RequestRemarks?.Trim() ?? string.Empty,
            ChargeAmount = chargeAmount,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _amcRepository.AddRevisitRequestAsync(revisitRequest, cancellationToken);
        await AddAuditLogAsync(booking.BookingReference, revisitRequest.CurrentStatus.ToString(), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        revisitRequest.Booking = booking;
        revisitRequest.OriginalJobCard = originalJobCard;
        revisitRequest.OriginalServiceRequest = originalJobCard.ServiceRequest;
        revisitRequest.CustomerAmc = customerAmc;
        revisitRequest.WarrantyClaim = warrantyClaim;

        _logger.LogInformation(
            "Revisit request {RevisitRequestId} created against booking {BookingReference} with type {RevisitType}.",
            revisitRequest.RevisitRequestId,
            booking.BookingReference,
            revisitRequest.RevisitType);

        return RevisitResponseMapper.ToResponse(revisitRequest);
    }

    private Task AddAuditLogAsync(string bookingReference, string statusName, CancellationToken cancellationToken)
    {
        return _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreateRevisitRequest",
                EntityName = "RevisitRequest",
                EntityId = bookingReference,
                TraceId = _currentUserContext.TraceId,
                StatusName = statusName,
                NewValues = bookingReference,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
    }
}
