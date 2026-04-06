using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.AddSupportTicketReply;

public sealed record AddSupportTicketReplyCommand(
    long SupportTicketId,
    string ReplyText,
    bool IsInternalOnly) : IRequest<SupportTicketReplyResponse>;
