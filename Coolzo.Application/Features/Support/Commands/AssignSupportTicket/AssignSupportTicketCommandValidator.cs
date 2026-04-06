using FluentValidation;

namespace Coolzo.Application.Features.Support.Commands.AssignSupportTicket;

public sealed class AssignSupportTicketCommandValidator : AbstractValidator<AssignSupportTicketCommand>
{
    public AssignSupportTicketCommandValidator()
    {
        RuleFor(request => request.SupportTicketId).GreaterThan(0);
        RuleFor(request => request.AssignedUserId).GreaterThan(0);
        RuleFor(request => request.Remarks).MaximumLength(256);
    }
}
