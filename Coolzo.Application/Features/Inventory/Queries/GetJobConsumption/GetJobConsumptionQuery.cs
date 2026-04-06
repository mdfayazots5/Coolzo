using Coolzo.Contracts.Responses.Inventory;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Queries.GetJobConsumption;

public sealed record GetJobConsumptionQuery(long JobCardId) : IRequest<JobPartConsumptionSummaryResponse>;
