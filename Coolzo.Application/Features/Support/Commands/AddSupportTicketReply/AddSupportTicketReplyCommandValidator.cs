using FluentValidation;

namespace Coolzo.Application.Features.Support.Commands.AddSupportTicketReply;

public sealed class AddSupportTicketReplyCommandValidator : AbstractValidator<AddSupportTicketReplyCommand>
{
    public AddSupportTicketReplyCommandValidator()
    {
        RuleFor(request => request.SupportTicketId).GreaterThan(0);
        RuleFor(request => request.ReplyText).NotEmpty().MaximumLength(1024);
    }
}
