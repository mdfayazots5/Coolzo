using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Amc;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Amc.Commands.GenerateAmcVisits;

public sealed class GenerateAmcVisitsCommandHandler : IRequestHandler<GenerateAmcVisitsCommand, CustomerAmcResponse>
{
    private readonly IAmcRepository _amcRepository;
    private readonly AmcScheduleService _amcScheduleService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<GenerateAmcVisitsCommandHandler> _logger;
    private readonly ServiceLifecycleAccessService _serviceLifecycleAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateAmcVisitsCommandHandler(
        IAmcRepository amcRepository,
        AmcScheduleService amcScheduleService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        ServiceLifecycleAccessService serviceLifecycleAccessService,
        IAppLogger<GenerateAmcVisitsCommandHandler> logger)
    {
        _amcRepository = amcRepository;
        _amcScheduleService = amcScheduleService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _serviceLifecycleAccessService = serviceLifecycleAccessService;
        _logger = logger;
    }

    public async Task<CustomerAmcResponse> Handle(GenerateAmcVisitsCommand request, CancellationToken cancellationToken)
    {
        _serviceLifecycleAccessService.EnsureAmcAssignmentAccess();

        var customerAmc = await _amcRepository.GetCustomerAmcByIdForUpdateAsync(request.CustomerAmcId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested AMC subscription could not be found.", 404);

        if (!customerAmc.Visits.Any(visit => !visit.IsDeleted))
        {
            foreach (var schedule in _amcScheduleService.BuildInitialSchedule(
                customerAmc,
                _currentUserContext.UserName,
                _currentUserContext.IPAddress,
                _currentDateTime.UtcNow))
            {
                customerAmc.Visits.Add(schedule);
            }

            await _auditLogRepository.AddAsync(
                new AuditLog
                {
                    UserId = _currentUserContext.UserId,
                    ActionName = "GenerateAmcVisits",
                    EntityName = "CustomerAmc",
                    EntityId = customerAmc.CustomerAmcId.ToString(),
                    TraceId = _currentUserContext.TraceId,
                    StatusName = customerAmc.CurrentStatus.ToString(),
                    NewValues = customerAmc.CustomerAmcId.ToString(),
                    CreatedBy = _currentUserContext.UserName,
                    DateCreated = _currentDateTime.UtcNow,
                    IPAddress = _currentUserContext.IPAddress
                },
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var savedSubscription = await _amcRepository.GetCustomerAmcByIdAsync(customerAmc.CustomerAmcId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The AMC subscription could not be reloaded.", 404);

        _logger.LogInformation("AMC visits ensured for subscription {CustomerAmcId}.", customerAmc.CustomerAmcId);

        return AmcResponseMapper.ToCustomerAmc(savedSubscription);
    }
}
