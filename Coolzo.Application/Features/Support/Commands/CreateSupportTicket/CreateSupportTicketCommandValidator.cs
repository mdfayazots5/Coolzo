using FluentValidation;

namespace Coolzo.Application.Features.Support.Commands.CreateSupportTicket;

public sealed class CreateSupportTicketCommandValidator : AbstractValidator<CreateSupportTicketCommand>
{
    public CreateSupportTicketCommandValidator()
    {
        RuleFor(request => request.Subject).NotEmpty().MaximumLength(128);
        RuleFor(request => request.Description).NotEmpty().MaximumLength(1024);
        RuleFor(request => request.SupportTicketCategoryId).GreaterThan(0);
        RuleFor(request => request.SupportTicketPriorityId).GreaterThan(0);
        RuleForEach(request => request.Links).ChildRules(link =>
        {
            link.RuleFor(item => item.LinkedEntityType).NotEmpty().MaximumLength(64);
            link.RuleFor(item => item.LinkedEntityId).GreaterThan(0);
        });
    }
}
