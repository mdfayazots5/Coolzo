using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.ServiceRequest.Commands.CreateServiceRequestFromBooking;

public sealed class CreateServiceRequestFromBookingCommandHandler : IRequestHandler<CreateServiceRequestFromBookingCommand, ServiceRequestDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CreateServiceRequestFromBookingCommandHandler> _logger;
    private readonly IServiceRequestNumberGenerator _serviceRequestNumberGenerator;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceRequestFromBookingCommandHandler(
        IBookingRepository bookingRepository,
        IServiceRequestRepository serviceRequestRepository,
        IServiceRequestNumberGenerator serviceRequestNumberGenerator,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<CreateServiceRequestFromBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _serviceRequestRepository = serviceRequestRepository;
        _serviceRequestNumberGenerator = serviceRequestNumberGenerator;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<ServiceRequestDetailResponse> Handle(CreateServiceRequestFromBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested booking could not be found.", 404);

        if (booking.ServiceRequest is not null && !booking.ServiceRequest.IsDeleted)
        {
            throw new AppException(
                ErrorCodes.ServiceRequestAlreadyExists,
                "A service request has already been created for this booking.",
                409);
        }

        var serviceRequestNumber = await GenerateUniqueServiceRequestNumberAsync(cancellationToken);
        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
        var ipAddress = _currentUserContext.IPAddress;

        var serviceRequest = new Domain.Entities.ServiceRequest
        {
            BookingId = booking.BookingId,
            ServiceRequestNumber = serviceRequestNumber,
            CurrentStatus = ServiceRequestStatus.New,
            ServiceRequestDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        };

        serviceRequest.StatusHistories.Add(new ServiceRequestStatusHistory
        {
            Status = ServiceRequestStatus.New,
            Remarks = $"Service request created from booking {booking.BookingReference}.",
            StatusDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        });

        await _serviceRequestRepository.AddAsync(serviceRequest, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreateServiceRequestFromBooking",
                EntityName = "ServiceRequest",
                EntityId = serviceRequestNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = booking.BookingReference,
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = ipAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Service request {ServiceRequestNumber} created from booking {BookingId}.",
            serviceRequestNumber,
            booking.BookingId);

        var createdServiceRequest = await _serviceRequestRepository.GetByIdAsync(serviceRequest.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The created service request could not be loaded.", 404);

        return ServiceRequestResponseMapper.ToDetail(createdServiceRequest, Array.Empty<ServiceChecklistMaster>());
    }

    private async Task<string> GenerateUniqueServiceRequestNumberAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var serviceRequestNumber = _serviceRequestNumberGenerator.GenerateNumber();

            if (!await _serviceRequestRepository.ServiceRequestNumberExistsAsync(serviceRequestNumber, cancellationToken))
            {
                return serviceRequestNumber;
            }
        }
    }
}
