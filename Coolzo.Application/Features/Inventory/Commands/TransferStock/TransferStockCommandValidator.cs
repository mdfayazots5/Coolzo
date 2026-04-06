using FluentValidation;

namespace Coolzo.Application.Features.Inventory.Commands.TransferStock;

public sealed class TransferStockCommandValidator : AbstractValidator<TransferStockCommand>
{
    public TransferStockCommandValidator()
    {
        RuleFor(request => request.SourceWarehouseId).GreaterThan(0);
        RuleFor(request => request.DestinationWarehouseId).GreaterThan(0);
        RuleFor(request => request.ItemId).GreaterThan(0);
        RuleFor(request => request.Quantity).GreaterThan(0.00m);
        RuleFor(request => request.UnitCost).GreaterThanOrEqualTo(0.00m);
        RuleFor(request => request.ReferenceNumber).MaximumLength(64);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}
