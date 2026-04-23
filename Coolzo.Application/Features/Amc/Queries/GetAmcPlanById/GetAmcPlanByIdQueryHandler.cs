using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Amc;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Amc.Queries.GetAmcPlanById;

public sealed class GetAmcPlanByIdQueryHandler : IRequestHandler<GetAmcPlanByIdQuery, AmcPlanResponse>
{
    private readonly IAmcRepository _amcRepository;

    public GetAmcPlanByIdQueryHandler(IAmcRepository amcRepository)
    {
        _amcRepository = amcRepository;
    }

    public async Task<AmcPlanResponse> Handle(GetAmcPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var amcPlan = await _amcRepository.GetAmcPlanByIdAsync(request.AmcPlanId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested AMC plan could not be found.", 404);

        return AmcResponseMapper.ToAmcPlan(amcPlan);
    }
}
