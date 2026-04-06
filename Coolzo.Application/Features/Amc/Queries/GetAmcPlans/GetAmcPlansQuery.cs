using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Amc;
using MediatR;

namespace Coolzo.Application.Features.Amc.Queries.GetAmcPlans;

public sealed record GetAmcPlansQuery(
    bool? IsActive,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<AmcPlanResponse>>;
