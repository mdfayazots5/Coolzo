using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Amc.Commands.CreateAmcPlan;

public sealed class CreateAmcPlanCommandHandler : IRequestHandler<CreateAmcPlanCommand, Coolzo.Contracts.Responses.Amc.AmcPlanResponse>
{
    private readonly IAmcRepository _amcRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CreateAmcPlanCommandHandler> _logger;
    private readonly ServiceLifecycleAccessService _serviceLifecycleAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAmcPlanCommandHandler(
        IAmcRepository amcRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        ServiceLifecycleAccessService serviceLifecycleAccessService,
        IAppLogger<CreateAmcPlanCommandHandler> logger)
    {
        _amcRepository = amcRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _serviceLifecycleAccessService = serviceLifecycleAccessService;
        _logger = logger;
    }

    public async Task<Coolzo.Contracts.Responses.Amc.AmcPlanResponse> Handle(CreateAmcPlanCommand request, CancellationToken cancellationToken)
    {
        _serviceLifecycleAccessService.EnsureAmcPlanCreateAccess();

        var planName = request.PlanName.Trim();

        if (await _amcRepository.AmcPlanNameExistsAsync(planName, cancellationToken))
        {
            throw new AppException(ErrorCodes.AmcPlanAlreadyExists, "An AMC plan already exists with the same name.", 409);
        }

        var amcPlan = new AmcPlan
        {
            PlanName = planName,
            PlanDescription = request.PlanDescription?.Trim() ?? string.Empty,
            DurationInMonths = request.DurationInMonths,
            VisitCount = request.VisitCount,
            PriceAmount = request.PriceAmount,
            IsActive = request.IsActive,
            TermsAndConditions = request.TermsAndConditions?.Trim() ?? string.Empty,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _amcRepository.AddAmcPlanAsync(amcPlan, cancellationToken);
        await AddAuditLogAsync("CreateAmcPlan", planName, amcPlan.IsActive ? "Active" : "Inactive", cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("AMC plan {PlanName} created by user {UserName}.", planName, _currentUserContext.UserName);

        return AmcResponseMapper.ToAmcPlan(amcPlan);
    }

    private Task AddAuditLogAsync(string actionName, string entityId, string statusName, CancellationToken cancellationToken)
    {
        return _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = actionName,
                EntityName = "AmcPlan",
                EntityId = entityId,
                TraceId = _currentUserContext.TraceId,
                StatusName = statusName,
                NewValues = entityId,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
    }
}
