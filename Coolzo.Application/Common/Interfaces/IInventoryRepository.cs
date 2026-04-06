using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;

namespace Coolzo.Application.Common.Interfaces;

public interface IInventoryRepository
{
    Task<bool> ItemCodeExistsAsync(string itemCode, long? excludeItemId, CancellationToken cancellationToken);

    Task<ItemCategory?> GetItemCategoryByCodeAsync(string categoryCode, CancellationToken cancellationToken);

    Task<UnitOfMeasure?> GetUnitOfMeasureByCodeAsync(string unitCode, CancellationToken cancellationToken);

    Task<Supplier?> GetSupplierByCodeAsync(string supplierCode, CancellationToken cancellationToken);

    Task AddItemCategoryAsync(ItemCategory itemCategory, CancellationToken cancellationToken);

    Task AddUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure, CancellationToken cancellationToken);

    Task AddSupplierAsync(Supplier supplier, CancellationToken cancellationToken);

    Task AddItemAsync(Item item, CancellationToken cancellationToken);

    Task AddItemRateAsync(ItemRate itemRate, CancellationToken cancellationToken);

    Task<Item?> GetItemByIdAsync(long itemId, CancellationToken cancellationToken);

    Task<Item?> GetItemByIdForUpdateAsync(long itemId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Item>> SearchItemsAsync(
        string? searchTerm,
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountItemsAsync(string? searchTerm, bool? isActive, CancellationToken cancellationToken);

    Task<bool> WarehouseCodeExistsAsync(string warehouseCode, CancellationToken cancellationToken);

    Task AddWarehouseAsync(Warehouse warehouse, CancellationToken cancellationToken);

    Task<Warehouse?> GetWarehouseByIdAsync(long warehouseId, CancellationToken cancellationToken);

    Task<Warehouse?> GetWarehouseByIdForUpdateAsync(long warehouseId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Warehouse>> SearchWarehousesAsync(
        string? searchTerm,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<WarehouseStock?> GetWarehouseStockEntryForUpdateAsync(
        long warehouseId,
        long itemId,
        CancellationToken cancellationToken);

    Task AddWarehouseStockAsync(WarehouseStock warehouseStock, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<WarehouseStock>> GetWarehouseStockByWarehouseIdAsync(long warehouseId, CancellationToken cancellationToken);

    Task<Technician?> GetTechnicianByIdAsync(long technicianId, CancellationToken cancellationToken);

    Task<Technician?> GetTechnicianByIdForUpdateAsync(long technicianId, CancellationToken cancellationToken);

    Task<TechnicianVanStock?> GetTechnicianStockEntryForUpdateAsync(
        long technicianId,
        long itemId,
        CancellationToken cancellationToken);

    Task AddTechnicianStockAsync(TechnicianVanStock technicianVanStock, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TechnicianVanStock>> GetTechnicianStockByTechnicianIdAsync(long technicianId, CancellationToken cancellationToken);

    Task AddStockTransactionAsync(StockTransaction stockTransaction, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<StockTransaction>> SearchStockTransactionsAsync(
        StockTransactionType? transactionType,
        long? itemId,
        long? warehouseId,
        long? technicianId,
        long? jobCardId,
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountStockTransactionsAsync(
        StockTransactionType? transactionType,
        long? itemId,
        long? warehouseId,
        long? technicianId,
        long? jobCardId,
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        CancellationToken cancellationToken);

    Task<JobCard?> GetJobCardByIdAsync(long jobCardId, CancellationToken cancellationToken);

    Task<JobCard?> GetJobCardByIdForUpdateAsync(long jobCardId, CancellationToken cancellationToken);

    Task AddJobPartConsumptionAsync(JobPartConsumption jobPartConsumption, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<JobPartConsumption>> GetJobPartConsumptionsByJobCardIdAsync(long jobCardId, CancellationToken cancellationToken);
}
