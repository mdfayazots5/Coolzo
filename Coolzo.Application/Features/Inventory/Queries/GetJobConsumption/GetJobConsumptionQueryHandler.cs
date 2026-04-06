using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Queries.GetJobConsumption;

public sealed class GetJobConsumptionQueryHandler : IRequestHandler<GetJobConsumptionQuery, JobPartConsumptionSummaryResponse>
{
    private readonly InventoryAccessService _inventoryAccessService;
    private readonly IInventoryRepository _inventoryRepository;

    public GetJobConsumptionQueryHandler(
        IInventoryRepository inventoryRepository,
        InventoryAccessService inventoryAccessService)
    {
        _inventoryRepository = inventoryRepository;
        _inventoryAccessService = inventoryAccessService;
    }

    public async Task<JobPartConsumptionSummaryResponse> Handle(GetJobConsumptionQuery request, CancellationToken cancellationToken)
    {
        var jobCard = await _inventoryRepository.GetJobCardByIdAsync(request.JobCardId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested job card could not be found.", 404);

        await _inventoryAccessService.EnsureJobConsumptionReadAccessAsync(jobCard, cancellationToken);

        var items = await _inventoryRepository.GetJobPartConsumptionsByJobCardIdAsync(request.JobCardId, cancellationToken);

        return InventoryResponseMapper.ToJobPartConsumptionSummary(jobCard, items);
    }
}
