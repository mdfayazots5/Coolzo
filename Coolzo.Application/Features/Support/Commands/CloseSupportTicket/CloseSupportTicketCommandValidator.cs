using FluentValidation;

namespace Coolzo.Application.Features.Support.Commands.CloseSupportTicket;

public sealed class CloseSupportTicketCommandValidator : AbstractValidator<CloseSupportTicketCommand>
{
    public CloseSupportTicketCommandValidator()
    {
        RuleFor(request => request.SupportTicketId).GreaterThan(0);
        RuleFor(request => request.Remarks).MaximumLength(256);
    }
}
