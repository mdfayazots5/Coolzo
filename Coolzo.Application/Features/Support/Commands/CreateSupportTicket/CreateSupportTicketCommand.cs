using Coolzo.Contracts.Requests.Support;
using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.CreateSupportTicket;

public sealed record CreateSupportTicketCommand(
    long? CustomerId,
    string Subject,
    long SupportTicketCategoryId,
    long SupportTicketPriorityId,
    string Description,
    IReadOnlyCollection<CreateSupportTicketLinkRequest> Links) : IRequest<SupportTicketDetailResponse>;
