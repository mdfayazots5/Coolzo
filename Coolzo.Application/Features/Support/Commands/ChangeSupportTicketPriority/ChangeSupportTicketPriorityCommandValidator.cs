using FluentValidation;

namespace Coolzo.Application.Features.Support.Commands.ChangeSupportTicketPriority;

public sealed class ChangeSupportTicketPriorityCommandValidator : AbstractValidator<ChangeSupportTicketPriorityCommand>
{
    public ChangeSupportTicketPriorityCommandValidator()
    {
        RuleFor(request => request.SupportTicketId).GreaterThan(0);
        RuleFor(request => request.SupportTicketPriorityId).GreaterThan(0);
        RuleFor(request => request.Remarks).MaximumLength(256);
    }
}
