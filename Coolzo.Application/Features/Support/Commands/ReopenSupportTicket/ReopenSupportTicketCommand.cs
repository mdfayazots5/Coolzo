using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.ReopenSupportTicket;

public sealed record ReopenSupportTicketCommand(
    long SupportTicketId,
    string Remarks) : IRequest<SupportTicketDetailResponse>;
