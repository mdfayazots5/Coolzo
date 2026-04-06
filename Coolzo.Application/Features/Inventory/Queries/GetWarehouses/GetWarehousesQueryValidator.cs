using FluentValidation;

namespace Coolzo.Application.Features.Inventory.Queries.GetWarehouses;

public sealed class GetWarehousesQueryValidator : AbstractValidator<GetWarehousesQuery>
{
    public GetWarehousesQueryValidator()
    {
        RuleFor(request => request.SearchTerm).MaximumLength(128);
    }
}
