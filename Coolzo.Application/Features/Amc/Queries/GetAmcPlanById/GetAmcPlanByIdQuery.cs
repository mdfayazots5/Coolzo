using Coolzo.Contracts.Responses.Amc;
using MediatR;

namespace Coolzo.Application.Features.Amc.Queries.GetAmcPlanById;

public sealed record GetAmcPlanByIdQuery(long AmcPlanId) : IRequest<AmcPlanResponse>;
