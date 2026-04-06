using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Amc;
using MediatR;

namespace Coolzo.Application.Features.Amc.Queries.GetAmcPlans;

public sealed class GetAmcPlansQueryHandler : IRequestHandler<GetAmcPlansQuery, PagedResult<AmcPlanResponse>>
{
    private readonly IAmcRepository _amcRepository;

    public GetAmcPlansQueryHandler(IAmcRepository amcRepository)
    {
        _amcRepository = amcRepository;
    }

    public async Task<PagedResult<AmcPlanResponse>> Handle(GetAmcPlansQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var items = await _amcRepository.SearchAmcPlansAsync(request.IsActive, pageNumber, pageSize, cancellationToken);
        var totalCount = await _amcRepository.CountAmcPlansAsync(request.IsActive, cancellationToken);

        return new PagedResult<AmcPlanResponse>(
            items.Select(AmcResponseMapper.ToAmcPlan).ToArray(),
            totalCount,
            pageNumber,
            pageSize);
    }
}
