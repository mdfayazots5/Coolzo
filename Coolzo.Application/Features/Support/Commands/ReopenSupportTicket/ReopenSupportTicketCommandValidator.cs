using FluentValidation;

namespace Coolzo.Application.Features.Support.Commands.ReopenSupportTicket;

public sealed class ReopenSupportTicketCommandValidator : AbstractValidator<ReopenSupportTicketCommand>
{
    public ReopenSupportTicketCommandValidator()
    {
        RuleFor(request => request.SupportTicketId).GreaterThan(0);
        RuleFor(request => request.Remarks).MaximumLength(256);
    }
}
