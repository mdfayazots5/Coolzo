using FluentValidation;

namespace Coolzo.Application.Features.Support.Commands.ChangeSupportTicketStatus;

public sealed class ChangeSupportTicketStatusCommandValidator : AbstractValidator<ChangeSupportTicketStatusCommand>
{
    public ChangeSupportTicketStatusCommandValidator()
    {
        RuleFor(request => request.SupportTicketId).GreaterThan(0);
        RuleFor(request => request.Status).NotEmpty().MaximumLength(64);
        RuleFor(request => request.Remarks).MaximumLength(256);
    }
}
