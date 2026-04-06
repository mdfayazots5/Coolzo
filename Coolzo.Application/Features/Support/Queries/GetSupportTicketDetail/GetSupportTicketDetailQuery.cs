using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetSupportTicketDetail;

public sealed record GetSupportTicketDetailQuery(long SupportTicketId) : IRequest<SupportTicketDetailResponse>;
