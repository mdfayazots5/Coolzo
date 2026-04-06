using FluentValidation;

namespace Coolzo.Application.Features.Inventory.Queries.GetWarehouseStock;

public sealed class GetWarehouseStockQueryValidator : AbstractValidator<GetWarehouseStockQuery>
{
    public GetWarehouseStockQueryValidator()
    {
        RuleFor(request => request.WarehouseId).GreaterThan(0);
    }
}
