using FluentValidation;

namespace Coolzo.Application.Features.Inventory.Queries.GetItemById;

public sealed class GetItemByIdQueryValidator : AbstractValidator<GetItemByIdQuery>
{
    public GetItemByIdQueryValidator()
    {
        RuleFor(request => request.ItemId).GreaterThan(0);
    }
}
