using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.ServiceRequest.Commands.UpdateServiceRequestCoordinates;

public sealed class UpdateServiceRequestCoordinatesCommandHandler : IRequestHandler<UpdateServiceRequestCoordinatesCommand, ServiceRequestDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<UpdateServiceRequestCoordinatesCommandHandler> _logger;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceRequestCoordinatesCommandHandler(
        IServiceRequestRepository serviceRequestRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<UpdateServiceRequestCoordinatesCommandHandler> logger)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<ServiceRequestDetailResponse> Handle(UpdateServiceRequestCoordinatesCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _serviceRequestRepository.GetByIdForUpdateAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);

        var customerAddress = serviceRequest.Booking?.CustomerAddress
            ?? throw new AppException(ErrorCodes.NotFound, "No customer address is linked to this service request.", 404);

        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;

        customerAddress.Latitude = request.Latitude;
        customerAddress.Longitude = request.Longitude;
        customerAddress.LastUpdated = now;
        customerAddress.UpdatedBy = userName;

        if (serviceRequest.Booking is not null)
        {
            serviceRequest.Booking.LatitudeSnapshot = request.Latitude;
            serviceRequest.Booking.LongitudeSnapshot = request.Longitude;
            serviceRequest.Booking.LastUpdated = now;
            serviceRequest.Booking.UpdatedBy = userName;
        }

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "UpdateServiceRequestCoordinates",
                EntityName = "ServiceRequest",
                EntityId = serviceRequest.ServiceRequestNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = $"{request.Latitude},{request.Longitude}",
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Coordinates updated for service request {ServiceRequestId}: {Latitude}, {Longitude}.",
            serviceRequest.ServiceRequestId, request.Latitude, request.Longitude);

        var refreshed = await _serviceRequestRepository.GetByIdAsync(serviceRequest.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The updated service request could not be loaded.", 404);

        return ServiceRequestResponseMapper.ToDetail(refreshed, Array.Empty<ServiceChecklistMaster>());
    }
}
