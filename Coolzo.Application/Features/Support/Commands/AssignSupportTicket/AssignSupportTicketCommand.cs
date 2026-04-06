using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.AssignSupportTicket;

public sealed record AssignSupportTicketCommand(
    long SupportTicketId,
    long AssignedUserId,
    string Remarks) : IRequest<SupportTicketDetailResponse>;
