using FluentValidation;

namespace Coolzo.Application.Features.Support.Queries.GetSupportTicketList;

public sealed class GetSupportTicketListQueryValidator : AbstractValidator<GetSupportTicketListQuery>
{
    public GetSupportTicketListQueryValidator()
    {
        RuleFor(request => request.PageNumber).GreaterThan(0);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 100);
        RuleFor(request => request.TicketNumber).MaximumLength(64);
        RuleFor(request => request.CustomerMobile).Matches("^[0-9]{0,16}$")
            .When(request => !string.IsNullOrWhiteSpace(request.CustomerMobile));
        RuleFor(request => request.Status).MaximumLength(64);
        RuleFor(request => request.LinkedEntityType).MaximumLength(64);
    }
}
