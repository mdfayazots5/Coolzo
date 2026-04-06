using FluentValidation;

namespace Coolzo.Application.Features.Inventory.Queries.GetTechnicianStock;

public sealed class GetTechnicianStockQueryValidator : AbstractValidator<GetTechnicianStockQuery>
{
    public GetTechnicianStockQueryValidator()
    {
        RuleFor(request => request.TechnicianId).GreaterThan(0);
    }
}
