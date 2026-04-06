using Coolzo.Contracts.Responses.Inventory;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Commands.UpdateItem;

public sealed record UpdateItemCommand(
    long ItemId,
    string CategoryCode,
    string CategoryName,
    string UnitOfMeasureCode,
    string UnitOfMeasureName,
    string? SupplierCode,
    string? SupplierName,
    string ItemCode,
    string ItemName,
    string? ItemDescription,
    decimal PurchasePrice,
    decimal SellingPrice,
    decimal TaxPercentage,
    int WarrantyDays,
    decimal ReorderLevel,
    bool IsActive) : IRequest<ItemResponse>;
