using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Domain.Rules;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.ServiceRequest.Commands.UpdateServiceRequestStatus;

public sealed class UpdateServiceRequestStatusCommandHandler : IRequestHandler<UpdateServiceRequestStatusCommand, ServiceRequestDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<UpdateServiceRequestStatusCommandHandler> _logger;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceRequestStatusCommandHandler(
        IServiceRequestRepository serviceRequestRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<UpdateServiceRequestStatusCommandHandler> logger)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<ServiceRequestDetailResponse> Handle(UpdateServiceRequestStatusCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _serviceRequestRepository.GetByIdForUpdateAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);

        if (!Enum.TryParse<ServiceRequestStatus>(request.Status, true, out var targetStatus))
        {
            throw new AppException(
                ErrorCodes.InvalidStatusTransition,
                "The requested status is invalid.",
                400);
        }

        if (targetStatus == serviceRequest.CurrentStatus || !ServiceRequestStatusRule.CanTransition(serviceRequest.CurrentStatus, targetStatus))
        {
            throw new AppException(
                ErrorCodes.InvalidStatusTransition,
                "The requested service request status transition is not allowed.",
                409);
        }

        var activeAssignment = serviceRequest.Assignments.FirstOrDefault(
            assignment => assignment.IsActiveAssignment && !assignment.IsDeleted);

        if (activeAssignment is null)
        {
            throw new AppException(
                ErrorCodes.Conflict,
                "A technician must be assigned before the service request status can progress.",
                409);
        }

        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
        var ipAddress = _currentUserContext.IPAddress;
        var remarks = !string.IsNullOrWhiteSpace(request.Remarks)
            ? request.Remarks.Trim()
            : $"Service request moved to {targetStatus}.";

        serviceRequest.CurrentStatus = targetStatus;
        serviceRequest.UpdatedBy = userName;
        serviceRequest.LastUpdated = now;

        serviceRequest.StatusHistories.Add(new ServiceRequestStatusHistory
        {
            Status = targetStatus,
            Remarks = remarks,
            StatusDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        });

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "UpdateServiceRequestStatus",
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

        _logger.LogInformation(
            "Service request {ServiceRequestId} moved to status {Status}.",
            serviceRequest.ServiceRequestId,
            targetStatus);

        var refreshedServiceRequest = await _serviceRequestRepository.GetByIdAsync(serviceRequest.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The updated service request could not be loaded.", 404);

        return ServiceRequestResponseMapper.ToDetail(refreshedServiceRequest, Array.Empty<ServiceChecklistMaster>());
    }
}
