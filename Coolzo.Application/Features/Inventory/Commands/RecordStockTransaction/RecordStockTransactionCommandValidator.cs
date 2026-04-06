using FluentValidation;

namespace Coolzo.Application.Features.Inventory.Commands.RecordStockTransaction;

public sealed class RecordStockTransactionCommandValidator : AbstractValidator<RecordStockTransactionCommand>
{
    public RecordStockTransactionCommandValidator()
    {
        RuleFor(request => request.WarehouseId).GreaterThan(0);
        RuleFor(request => request.ItemId).GreaterThan(0);
        RuleFor(request => request.TransactionType).NotEmpty().MaximumLength(64);
        RuleFor(request => request.Quantity).GreaterThan(0.00m);
        RuleFor(request => request.UnitCost).GreaterThanOrEqualTo(0.00m);
        RuleFor(request => request.ReferenceNumber).MaximumLength(64);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}
