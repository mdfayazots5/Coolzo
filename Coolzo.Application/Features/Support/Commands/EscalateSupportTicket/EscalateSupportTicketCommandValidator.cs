using FluentValidation;

namespace Coolzo.Application.Features.Support.Commands.EscalateSupportTicket;

public sealed class EscalateSupportTicketCommandValidator : AbstractValidator<EscalateSupportTicketCommand>
{
    public EscalateSupportTicketCommandValidator()
    {
        RuleFor(request => request.SupportTicketId).GreaterThan(0);
        RuleFor(request => request.EscalationTarget).NotEmpty().MaximumLength(128);
        RuleFor(request => request.EscalationRemarks).NotEmpty().MaximumLength(512);
    }
}
