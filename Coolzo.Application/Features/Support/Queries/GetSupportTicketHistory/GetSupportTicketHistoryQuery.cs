using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetSupportTicketHistory;

public sealed record GetSupportTicketHistoryQuery(long SupportTicketId) : IRequest<IReadOnlyCollection<SupportTicketStatusHistoryResponse>>;
