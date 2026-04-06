using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetSupportTicketEscalations;

public sealed record GetSupportTicketEscalationsQuery(long SupportTicketId) : IRequest<IReadOnlyCollection<SupportTicketEscalationResponse>>;
