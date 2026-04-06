using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetSupportTicketReplies;

public sealed record GetSupportTicketRepliesQuery(long SupportTicketId) : IRequest<IReadOnlyCollection<SupportTicketReplyResponse>>;
