using Coolzo.Contracts.Responses.Operations;
using MediatR;

namespace Coolzo.Application.Features.OperationsDashboard.Queries.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery() : IRequest<OperationsDashboardSummaryResponse>;
