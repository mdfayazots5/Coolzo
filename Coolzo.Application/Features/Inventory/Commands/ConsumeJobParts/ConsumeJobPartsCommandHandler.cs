using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Commands.ConsumeJobParts;

public sealed class ConsumeJobPartsCommandHandler : IRequestHandler<ConsumeJobPartsCommand, JobPartConsumptionSummaryResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly InventoryAccessService _inventoryAccessService;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IAppLogger<ConsumeJobPartsCommandHandler> _logger;
    private readonly InventoryStockService _inventoryStockService;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public ConsumeJobPartsCommandHandler(
        IInventoryRepository inventoryRepository,
        InventoryStockService inventoryStockService,
        InventoryAccessService inventoryAccessService,
        ITechnicianJobAccessService technicianJobAccessService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<ConsumeJobPartsCommandHandler> logger)
    {
        _inventoryRepository = inventoryRepository;
        _inventoryStockService = inventoryStockService;
        _inventoryAccessService = inventoryAccessService;
        _technicianJobAccessService = technicianJobAccessService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<JobPartConsumptionSummaryResponse> Handle(ConsumeJobPartsCommand request, CancellationToken cancellationToken)
    {
        var jobCard = await _inventoryRepository.GetJobCardByIdForUpdateAsync(request.JobCardId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested job card could not be found.", 404);

        await _inventoryAccessService.EnsureJobConsumptionCreateAccessAsync(jobCard, cancellationToken);

        var activeAssignment = jobCard.ServiceRequest?.Assignments
            .FirstOrDefault(assignment => assignment.IsActiveAssignment && !assignment.IsDeleted);
        var technicianId = activeAssignment?.TechnicianId
            ?? throw new AppException(ErrorCodes.InvalidStockLocation, "The job must have an active technician assignment for part consumption.", 409);
        var technician = await _inventoryRepository.GetTechnicianByIdForUpdateAsync(technicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The assigned technician could not be found.", 404);

        if (_currentUserContext.Roles.Contains(RoleNames.Technician))
        {
            var currentTechnician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);

            if (currentTechnician.TechnicianId != technician.TechnicianId)
            {
                throw new AppException(
                    ErrorCodes.InventoryAccessDenied,
                    "The current technician cannot consume parts for another technician's job.",
                    403);
            }
        }

        var lines = new List<(Coolzo.Domain.Entities.Item Item, decimal QuantityUsed, decimal UnitPrice, string? Remarks)>();

        foreach (var requestItem in request.Items)
        {
            var item = await _inventoryRepository.GetItemByIdForUpdateAsync(requestItem.ItemId, cancellationToken)
                ?? throw new AppException(ErrorCodes.NotFound, $"Item {requestItem.ItemId} could not be found.", 404);
            var currentRate = item.Rates
                .Where(rate => rate.IsActive && !rate.IsDeleted)
                .OrderByDescending(rate => rate.EffectiveFromUtc)
                .ThenByDescending(rate => rate.ItemRateId)
                .FirstOrDefault();

            lines.Add((item, requestItem.QuantityUsed, currentRate?.SellingPrice ?? 0.00m, requestItem.ConsumptionRemarks));
        }

        var result = await _inventoryStockService.ConsumeJobPartsAsync(jobCard, technician, lines, cancellationToken);

        foreach (var consumption in result.Consumptions)
        {
            jobCard.PartConsumptions.Add(consumption);
        }

        await AddAuditLogAsync("ConsumeJobParts", jobCard.JobCardNumber, technician.TechnicianCode, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Job part consumption recorded for job card {JobCardId} by {UserName}.",
            jobCard.JobCardId,
            _currentUserContext.UserName);

        return InventoryResponseMapper.ToJobPartConsumptionSummary(jobCard, jobCard.PartConsumptions.Where(entity => !entity.IsDeleted).ToArray());
    }

    private Task AddAuditLogAsync(string actionName, string entityId, string newValues, CancellationToken cancellationToken)
    {
        return _auditLogRepository.AddAsync(
            new Coolzo.Domain.Entities.AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = actionName,
                EntityName = "JobPartConsumption",
                EntityId = entityId,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = newValues,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
    }
}
