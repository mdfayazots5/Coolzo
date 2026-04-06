using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.ChangeSupportTicketPriority;

public sealed record ChangeSupportTicketPriorityCommand(
    long SupportTicketId,
    long SupportTicketPriorityId,
    string Remarks) : IRequest<SupportTicketDetailResponse>;
