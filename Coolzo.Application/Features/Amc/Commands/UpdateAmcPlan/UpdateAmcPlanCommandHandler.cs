using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Amc;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Amc.Commands.UpdateAmcPlan;

public sealed class UpdateAmcPlanCommandHandler : IRequestHandler<UpdateAmcPlanCommand, AmcPlanResponse>
{
    private readonly IAmcRepository _amcRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<UpdateAmcPlanCommandHandler> _logger;
    private readonly ServiceLifecycleAccessService _serviceLifecycleAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAmcPlanCommandHandler(
        IAmcRepository amcRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        ServiceLifecycleAccessService serviceLifecycleAccessService,
        IAppLogger<UpdateAmcPlanCommandHandler> logger)
    {
        _amcRepository = amcRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _serviceLifecycleAccessService = serviceLifecycleAccessService;
        _logger = logger;
    }

    public async Task<AmcPlanResponse> Handle(UpdateAmcPlanCommand request, CancellationToken cancellationToken)
    {
        _serviceLifecycleAccessService.EnsureAmcPlanCreateAccess();

        var amcPlan = await _amcRepository.GetAmcPlanByIdForUpdateAsync(request.AmcPlanId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested AMC plan could not be found.", 404);

        var planName = request.PlanName.Trim();

        if (!string.Equals(amcPlan.PlanName, planName, StringComparison.OrdinalIgnoreCase) &&
            await _amcRepository.AmcPlanNameExistsAsync(planName, cancellationToken))
        {
            throw new AppException(ErrorCodes.AmcPlanAlreadyExists, "An AMC plan already exists with the same name.", 409);
        }

        amcPlan.PlanName = planName;
        amcPlan.PlanDescription = request.PlanDescription?.Trim() ?? string.Empty;
        amcPlan.DurationInMonths = request.DurationInMonths;
        amcPlan.VisitCount = request.VisitCount;
        amcPlan.PriceAmount = request.PriceAmount;
        amcPlan.IsActive = request.IsActive;
        amcPlan.TermsAndConditions = request.TermsAndConditions?.Trim() ?? string.Empty;
        amcPlan.LastUpdated = _currentDateTime.UtcNow;
        amcPlan.UpdatedBy = _currentUserContext.UserName;

        await AddAuditLogAsync(amcPlan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("AMC plan {AmcPlanId} updated by user {UserName}.", amcPlan.AmcPlanId, _currentUserContext.UserName);

        return AmcResponseMapper.ToAmcPlan(amcPlan);
    }

    private Task AddAuditLogAsync(AmcPlan amcPlan, CancellationToken cancellationToken)
    {
        return _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "UpdateAmcPlan",
                EntityName = "AmcPlan",
                EntityId = amcPlan.AmcPlanId.ToString(),
                TraceId = _currentUserContext.TraceId,
                StatusName = amcPlan.IsActive ? "Active" : "Inactive",
                NewValues = amcPlan.PlanName,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
    }
}
