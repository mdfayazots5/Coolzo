using FluentValidation;

namespace Coolzo.Application.Features.Inventory.Queries.GetItems;

public sealed class GetItemsQueryValidator : AbstractValidator<GetItemsQuery>
{
    public GetItemsQueryValidator()
    {
        RuleFor(request => request.SearchTerm).MaximumLength(128);
        RuleFor(request => request.PageNumber).GreaterThan(0);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 200);
    }
}
