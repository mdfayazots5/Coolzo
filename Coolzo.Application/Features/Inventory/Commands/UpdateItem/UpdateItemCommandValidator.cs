using FluentValidation;

namespace Coolzo.Application.Features.Inventory.Commands.UpdateItem;

public sealed class UpdateItemCommandValidator : AbstractValidator<UpdateItemCommand>
{
    public UpdateItemCommandValidator()
    {
        RuleFor(request => request.ItemId).GreaterThan(0);
        RuleFor(request => request.CategoryCode).NotEmpty().MaximumLength(32);
        RuleFor(request => request.CategoryName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.UnitOfMeasureCode).NotEmpty().MaximumLength(16);
        RuleFor(request => request.UnitOfMeasureName).NotEmpty().MaximumLength(64);
        RuleFor(request => request.SupplierCode).MaximumLength(32);
        RuleFor(request => request.SupplierName).MaximumLength(128);
        RuleFor(request => request.ItemCode).NotEmpty().MaximumLength(64);
        RuleFor(request => request.ItemName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.ItemDescription).MaximumLength(512);
        RuleFor(request => request.PurchasePrice).GreaterThanOrEqualTo(0.00m);
        RuleFor(request => request.SellingPrice).GreaterThanOrEqualTo(0.00m);
        RuleFor(request => request.TaxPercentage).InclusiveBetween(0.00m, 100.00m);
        RuleFor(request => request.WarrantyDays).InclusiveBetween(0, 3650);
        RuleFor(request => request.ReorderLevel).GreaterThanOrEqualTo(0.00m);
    }
}
