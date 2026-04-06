using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.ChangeSupportTicketStatus;

public sealed record ChangeSupportTicketStatusCommand(
    long SupportTicketId,
    string Status,
    string Remarks) : IRequest<SupportTicketDetailResponse>;
