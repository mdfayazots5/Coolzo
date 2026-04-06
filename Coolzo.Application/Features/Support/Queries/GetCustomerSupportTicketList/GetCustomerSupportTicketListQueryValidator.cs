using FluentValidation;

namespace Coolzo.Application.Features.Support.Queries.GetCustomerSupportTicketList;

public sealed class GetCustomerSupportTicketListQueryValidator : AbstractValidator<GetCustomerSupportTicketListQuery>
{
    public GetCustomerSupportTicketListQueryValidator()
    {
        RuleFor(request => request.PageNumber).GreaterThan(0);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 100);
    }
}
