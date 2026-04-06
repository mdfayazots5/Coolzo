using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.EscalateSupportTicket;

public sealed record EscalateSupportTicketCommand(
    long SupportTicketId,
    string EscalationTarget,
    string EscalationRemarks) : IRequest<SupportTicketDetailResponse>;
