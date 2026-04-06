using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.CloseSupportTicket;

public sealed record CloseSupportTicketCommand(
    long SupportTicketId,
    string Remarks) : IRequest<SupportTicketDetailResponse>;
