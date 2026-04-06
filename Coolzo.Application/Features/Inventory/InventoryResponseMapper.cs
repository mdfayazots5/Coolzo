using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Domain.Entities;
using DomainTechnician = Coolzo.Domain.Entities.Technician;

namespace Coolzo.Application.Features.Inventory;

internal static class InventoryResponseMapper
{
    public static ItemResponse ToItem(Item item)
    {
        var rate = item.Rates
            .Where(entity => entity.IsActive && !entity.IsDeleted)
            .OrderByDescending(entity => entity.EffectiveFromUtc)
            .ThenByDescending(entity => entity.ItemRateId)
            .FirstOrDefault();

        return new ItemResponse(
            item.ItemId,
            item.ItemCode,
            item.ItemName,
            item.ItemCategory?.CategoryCode ?? string.Empty,
            item.ItemCategory?.CategoryName ?? string.Empty,
            item.UnitOfMeasure?.UnitCode ?? string.Empty,
            item.UnitOfMeasure?.UnitName ?? string.Empty,
            item.SupplierId,
            item.Supplier?.SupplierCode,
            item.Supplier?.SupplierName,
            item.ItemDescription,
            rate?.PurchasePrice ?? 0.00m,
            rate?.SellingPrice ?? 0.00m,
            item.TaxPercentage,
            item.WarrantyDays,
            item.ReorderLevel,
            item.IsActive);
    }

    public static WarehouseResponse ToWarehouse(Warehouse warehouse)
    {
        var activeStocks = warehouse.WarehouseStocks
            .Where(entity => !entity.IsDeleted)
            .ToArray();

        return new WarehouseResponse(
            warehouse.WarehouseId,
            warehouse.WarehouseCode,
            warehouse.WarehouseName,
            warehouse.ContactPerson,
            warehouse.MobileNumber,
            warehouse.EmailAddress,
            warehouse.CityName,
            BuildWarehouseAddressSummary(warehouse),
            warehouse.IsActive,
            activeStocks.Length,
            activeStocks.Sum(entity => entity.QuantityOnHand));
    }

    public static WarehouseStockResponse ToWarehouseStock(Warehouse warehouse, IReadOnlyCollection<WarehouseStock> stockItems)
    {
        var items = stockItems
            .OrderBy(entity => entity.Item?.ItemName)
            .Select(ToWarehouseStockItem)
            .ToArray();

        return new WarehouseStockResponse(
            warehouse.WarehouseId,
            warehouse.WarehouseCode,
            warehouse.WarehouseName,
            items.Length,
            items.Sum(entity => entity.QuantityOnHand),
            items);
    }

    public static TechnicianStockResponse ToTechnicianStock(DomainTechnician technician, IReadOnlyCollection<TechnicianVanStock> stockItems)
    {
        var items = stockItems
            .OrderBy(entity => entity.Item?.ItemName)
            .Select(ToTechnicianStockItem)
            .ToArray();

        return new TechnicianStockResponse(
            technician.TechnicianId,
            technician.TechnicianCode,
            technician.TechnicianName,
            items.Length,
            items.Sum(entity => entity.QuantityOnHand),
            items);
    }

    public static StockTransactionResponse ToStockTransaction(StockTransaction stockTransaction)
    {
        return new StockTransactionResponse(
            stockTransaction.StockTransactionId,
            stockTransaction.ItemId,
            stockTransaction.Item?.ItemCode ?? string.Empty,
            stockTransaction.Item?.ItemName ?? string.Empty,
            stockTransaction.TransactionType.ToString(),
            stockTransaction.WarehouseId,
            stockTransaction.Warehouse?.WarehouseName,
            stockTransaction.TechnicianId,
            stockTransaction.Technician?.TechnicianName,
            stockTransaction.JobCardId,
            stockTransaction.JobCard?.JobCardNumber,
            stockTransaction.Quantity,
            stockTransaction.UnitCost,
            stockTransaction.Amount,
            stockTransaction.BalanceAfterTransaction,
            stockTransaction.ReferenceNumber,
            stockTransaction.TransactionGroupCode,
            stockTransaction.TransactionDateUtc,
            stockTransaction.Remarks,
            stockTransaction.Supplier?.SupplierName,
            stockTransaction.CreatedBy);
    }

    public static JobPartConsumptionSummaryResponse ToJobPartConsumptionSummary(
        JobCard jobCard,
        IReadOnlyCollection<JobPartConsumption> items)
    {
        var mappedItems = items
            .OrderByDescending(entity => entity.ConsumedDateUtc)
            .ThenByDescending(entity => entity.JobPartConsumptionId)
            .Select(ToJobPartConsumption)
            .ToArray();

        var latestTechnician = mappedItems.FirstOrDefault();

        return new JobPartConsumptionSummaryResponse(
            jobCard.JobCardId,
            jobCard.JobCardNumber,
            latestTechnician?.TechnicianId,
            latestTechnician?.TechnicianName,
            mappedItems.Length,
            mappedItems.Sum(entity => entity.QuantityUsed),
            mappedItems.Sum(entity => entity.LineAmount),
            mappedItems);
    }

    private static WarehouseStockItemResponse ToWarehouseStockItem(WarehouseStock warehouseStock)
    {
        return new WarehouseStockItemResponse(
            warehouseStock.WarehouseStockId,
            warehouseStock.ItemId,
            warehouseStock.Item?.ItemCode ?? string.Empty,
            warehouseStock.Item?.ItemName ?? string.Empty,
            warehouseStock.Item?.ItemCategory?.CategoryName ?? string.Empty,
            warehouseStock.Item?.UnitOfMeasure?.UnitCode ?? string.Empty,
            warehouseStock.QuantityOnHand,
            warehouseStock.Item?.ReorderLevel ?? 0.00m,
            warehouseStock.QuantityOnHand <= (warehouseStock.Item?.ReorderLevel ?? 0.00m),
            warehouseStock.LastTransactionDateUtc);
    }

    private static TechnicianStockItemResponse ToTechnicianStockItem(TechnicianVanStock technicianVanStock)
    {
        return new TechnicianStockItemResponse(
            technicianVanStock.TechnicianVanStockId,
            technicianVanStock.ItemId,
            technicianVanStock.Item?.ItemCode ?? string.Empty,
            technicianVanStock.Item?.ItemName ?? string.Empty,
            technicianVanStock.Item?.ItemCategory?.CategoryName ?? string.Empty,
            technicianVanStock.Item?.UnitOfMeasure?.UnitCode ?? string.Empty,
            technicianVanStock.QuantityOnHand,
            technicianVanStock.Item?.ReorderLevel ?? 0.00m,
            technicianVanStock.QuantityOnHand <= (technicianVanStock.Item?.ReorderLevel ?? 0.00m),
            technicianVanStock.LastTransactionDateUtc);
    }

    private static JobPartConsumptionResponse ToJobPartConsumption(JobPartConsumption jobPartConsumption)
    {
        return new JobPartConsumptionResponse(
            jobPartConsumption.JobPartConsumptionId,
            jobPartConsumption.JobCardId,
            jobPartConsumption.JobCard?.JobCardNumber ?? string.Empty,
            jobPartConsumption.TechnicianId,
            jobPartConsumption.Technician?.TechnicianName ?? string.Empty,
            jobPartConsumption.ItemId,
            jobPartConsumption.Item?.ItemCode ?? string.Empty,
            jobPartConsumption.Item?.ItemName ?? string.Empty,
            jobPartConsumption.QuantityUsed,
            jobPartConsumption.UnitPrice,
            jobPartConsumption.LineAmount,
            jobPartConsumption.ConsumedDateUtc,
            jobPartConsumption.ConsumptionRemarks);
    }

    private static string BuildWarehouseAddressSummary(Warehouse warehouse)
    {
        return string.Join(
            ", ",
            new[]
            {
                warehouse.AddressLine1,
                warehouse.AddressLine2,
                warehouse.Landmark,
                warehouse.CityName,
                warehouse.Pincode
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }
}
