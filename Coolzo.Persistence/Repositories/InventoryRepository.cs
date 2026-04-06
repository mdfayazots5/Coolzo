using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class InventoryRepository : IInventoryRepository
{
    private readonly CoolzoDbContext _dbContext;

    public InventoryRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ItemCodeExistsAsync(string itemCode, long? excludeItemId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Items.Where(entity => entity.ItemCode == itemCode && !entity.IsDeleted);

        if (excludeItemId.HasValue)
        {
            query = query.Where(entity => entity.ItemId != excludeItemId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<ItemCategory?> GetItemCategoryByCodeAsync(string categoryCode, CancellationToken cancellationToken)
    {
        return _dbContext.ItemCategories
            .FirstOrDefaultAsync(entity => entity.CategoryCode == categoryCode && !entity.IsDeleted, cancellationToken);
    }

    public Task<UnitOfMeasure?> GetUnitOfMeasureByCodeAsync(string unitCode, CancellationToken cancellationToken)
    {
        return _dbContext.UnitOfMeasures
            .FirstOrDefaultAsync(entity => entity.UnitCode == unitCode && !entity.IsDeleted, cancellationToken);
    }

    public Task<Supplier?> GetSupplierByCodeAsync(string supplierCode, CancellationToken cancellationToken)
    {
        return _dbContext.Suppliers
            .FirstOrDefaultAsync(entity => entity.SupplierCode == supplierCode && !entity.IsDeleted, cancellationToken);
    }

    public Task AddItemCategoryAsync(ItemCategory itemCategory, CancellationToken cancellationToken)
    {
        return _dbContext.ItemCategories.AddAsync(itemCategory, cancellationToken).AsTask();
    }

    public Task AddUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure, CancellationToken cancellationToken)
    {
        return _dbContext.UnitOfMeasures.AddAsync(unitOfMeasure, cancellationToken).AsTask();
    }

    public Task AddSupplierAsync(Supplier supplier, CancellationToken cancellationToken)
    {
        return _dbContext.Suppliers.AddAsync(supplier, cancellationToken).AsTask();
    }

    public Task AddItemAsync(Item item, CancellationToken cancellationToken)
    {
        return _dbContext.Items.AddAsync(item, cancellationToken).AsTask();
    }

    public Task AddItemRateAsync(ItemRate itemRate, CancellationToken cancellationToken)
    {
        return _dbContext.ItemRates.AddAsync(itemRate, cancellationToken).AsTask();
    }

    public Task<Item?> GetItemByIdAsync(long itemId, CancellationToken cancellationToken)
    {
        return BuildItemQuery(asNoTracking: true)
            .FirstOrDefaultAsync(entity => entity.ItemId == itemId, cancellationToken);
    }

    public Task<Item?> GetItemByIdForUpdateAsync(long itemId, CancellationToken cancellationToken)
    {
        return BuildItemQuery(asNoTracking: false)
            .FirstOrDefaultAsync(entity => entity.ItemId == itemId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Item>> SearchItemsAsync(
        string? searchTerm,
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;
        var query = BuildItemQuery(asNoTracking: true);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(entity =>
                entity.ItemCode.Contains(searchTerm) ||
                entity.ItemName.Contains(searchTerm) ||
                entity.ItemCategory!.CategoryName.Contains(searchTerm));
        }

        if (isActive.HasValue)
        {
            query = query.Where(entity => entity.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(entity => entity.ItemName)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountItemsAsync(string? searchTerm, bool? isActive, CancellationToken cancellationToken)
    {
        var query = _dbContext.Items
            .Include(entity => entity.ItemCategory)
            .Where(entity => !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(entity =>
                entity.ItemCode.Contains(searchTerm) ||
                entity.ItemName.Contains(searchTerm) ||
                entity.ItemCategory!.CategoryName.Contains(searchTerm));
        }

        if (isActive.HasValue)
        {
            query = query.Where(entity => entity.IsActive == isActive.Value);
        }

        return query.CountAsync(cancellationToken);
    }

    public Task<bool> WarehouseCodeExistsAsync(string warehouseCode, CancellationToken cancellationToken)
    {
        return _dbContext.Warehouses.AnyAsync(
            entity => entity.WarehouseCode == warehouseCode && !entity.IsDeleted,
            cancellationToken);
    }

    public Task AddWarehouseAsync(Warehouse warehouse, CancellationToken cancellationToken)
    {
        return _dbContext.Warehouses.AddAsync(warehouse, cancellationToken).AsTask();
    }

    public Task<Warehouse?> GetWarehouseByIdAsync(long warehouseId, CancellationToken cancellationToken)
    {
        return _dbContext.Warehouses
            .AsNoTracking()
            .Include(entity => entity.WarehouseStocks.Where(stock => !stock.IsDeleted))
            .FirstOrDefaultAsync(entity => entity.WarehouseId == warehouseId && !entity.IsDeleted, cancellationToken);
    }

    public Task<Warehouse?> GetWarehouseByIdForUpdateAsync(long warehouseId, CancellationToken cancellationToken)
    {
        return _dbContext.Warehouses
            .FirstOrDefaultAsync(entity => entity.WarehouseId == warehouseId && !entity.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Warehouse>> SearchWarehousesAsync(
        string? searchTerm,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Warehouses
            .AsNoTracking()
            .Include(entity => entity.WarehouseStocks.Where(stock => !stock.IsDeleted))
            .Where(entity => !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(entity =>
                entity.WarehouseCode.Contains(searchTerm) ||
                entity.WarehouseName.Contains(searchTerm) ||
                entity.CityName.Contains(searchTerm));
        }

        if (isActive.HasValue)
        {
            query = query.Where(entity => entity.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(entity => entity.WarehouseName)
            .ToArrayAsync(cancellationToken);
    }

    public Task<WarehouseStock?> GetWarehouseStockEntryForUpdateAsync(
        long warehouseId,
        long itemId,
        CancellationToken cancellationToken)
    {
        return _dbContext.WarehouseStocks
            .FirstOrDefaultAsync(
                entity => entity.WarehouseId == warehouseId && entity.ItemId == itemId && !entity.IsDeleted,
                cancellationToken);
    }

    public Task AddWarehouseStockAsync(WarehouseStock warehouseStock, CancellationToken cancellationToken)
    {
        return _dbContext.WarehouseStocks.AddAsync(warehouseStock, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<WarehouseStock>> GetWarehouseStockByWarehouseIdAsync(long warehouseId, CancellationToken cancellationToken)
    {
        return await _dbContext.WarehouseStocks
            .AsNoTracking()
            .Include(entity => entity.Item)
                .ThenInclude(item => item!.ItemCategory)
            .Include(entity => entity.Item)
                .ThenInclude(item => item!.UnitOfMeasure)
            .Where(entity => entity.WarehouseId == warehouseId && !entity.IsDeleted)
            .OrderBy(entity => entity.Item!.ItemName)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Technician?> GetTechnicianByIdAsync(long technicianId, CancellationToken cancellationToken)
    {
        return _dbContext.Technicians
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.TechnicianId == technicianId && !entity.IsDeleted, cancellationToken);
    }

    public Task<Technician?> GetTechnicianByIdForUpdateAsync(long technicianId, CancellationToken cancellationToken)
    {
        return _dbContext.Technicians
            .FirstOrDefaultAsync(entity => entity.TechnicianId == technicianId && !entity.IsDeleted, cancellationToken);
    }

    public Task<TechnicianVanStock?> GetTechnicianStockEntryForUpdateAsync(
        long technicianId,
        long itemId,
        CancellationToken cancellationToken)
    {
        return _dbContext.TechnicianVanStocks
            .FirstOrDefaultAsync(
                entity => entity.TechnicianId == technicianId && entity.ItemId == itemId && !entity.IsDeleted,
                cancellationToken);
    }

    public Task AddTechnicianStockAsync(TechnicianVanStock technicianVanStock, CancellationToken cancellationToken)
    {
        return _dbContext.TechnicianVanStocks.AddAsync(technicianVanStock, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<TechnicianVanStock>> GetTechnicianStockByTechnicianIdAsync(long technicianId, CancellationToken cancellationToken)
    {
        return await _dbContext.TechnicianVanStocks
            .AsNoTracking()
            .Include(entity => entity.Item)
                .ThenInclude(item => item!.ItemCategory)
            .Include(entity => entity.Item)
                .ThenInclude(item => item!.UnitOfMeasure)
            .Where(entity => entity.TechnicianId == technicianId && !entity.IsDeleted)
            .OrderBy(entity => entity.Item!.ItemName)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddStockTransactionAsync(StockTransaction stockTransaction, CancellationToken cancellationToken)
    {
        return _dbContext.StockTransactions.AddAsync(stockTransaction, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<StockTransaction>> SearchStockTransactionsAsync(
        StockTransactionType? transactionType,
        long? itemId,
        long? warehouseId,
        long? technicianId,
        long? jobCardId,
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;
        var query = BuildStockTransactionQuery()
            .AsNoTracking();

        query = ApplyStockTransactionFilters(query, transactionType, itemId, warehouseId, technicianId, jobCardId, fromDateUtc, toDateUtc);

        return await query
            .OrderByDescending(entity => entity.TransactionDateUtc)
            .ThenByDescending(entity => entity.StockTransactionId)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountStockTransactionsAsync(
        StockTransactionType? transactionType,
        long? itemId,
        long? warehouseId,
        long? technicianId,
        long? jobCardId,
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        CancellationToken cancellationToken)
    {
        var query = ApplyStockTransactionFilters(
            _dbContext.StockTransactions.Where(entity => !entity.IsDeleted),
            transactionType,
            itemId,
            warehouseId,
            technicianId,
            jobCardId,
            fromDateUtc,
            toDateUtc);

        return query.CountAsync(cancellationToken);
    }

    public Task<JobCard?> GetJobCardByIdAsync(long jobCardId, CancellationToken cancellationToken)
    {
        return BuildJobCardQuery(asNoTracking: true)
            .FirstOrDefaultAsync(entity => entity.JobCardId == jobCardId, cancellationToken);
    }

    public Task<JobCard?> GetJobCardByIdForUpdateAsync(long jobCardId, CancellationToken cancellationToken)
    {
        return BuildJobCardQuery(asNoTracking: false)
            .FirstOrDefaultAsync(entity => entity.JobCardId == jobCardId, cancellationToken);
    }

    public Task AddJobPartConsumptionAsync(JobPartConsumption jobPartConsumption, CancellationToken cancellationToken)
    {
        return _dbContext.JobPartConsumptions.AddAsync(jobPartConsumption, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<JobPartConsumption>> GetJobPartConsumptionsByJobCardIdAsync(long jobCardId, CancellationToken cancellationToken)
    {
        return await _dbContext.JobPartConsumptions
            .AsNoTracking()
            .Include(entity => entity.JobCard)
            .Include(entity => entity.Item)
            .Include(entity => entity.Technician)
            .Where(entity => entity.JobCardId == jobCardId && !entity.IsDeleted)
            .OrderByDescending(entity => entity.ConsumedDateUtc)
            .ThenByDescending(entity => entity.JobPartConsumptionId)
            .ToArrayAsync(cancellationToken);
    }

    private IQueryable<Item> BuildItemQuery(bool asNoTracking)
    {
        IQueryable<Item> query = _dbContext.Items
            .AsSplitQuery()
            .Include(entity => entity.ItemCategory)
            .Include(entity => entity.UnitOfMeasure)
            .Include(entity => entity.Supplier)
            .Include(entity => entity.Rates.Where(rate => !rate.IsDeleted))
            .Where(entity => !entity.IsDeleted);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    private IQueryable<StockTransaction> BuildStockTransactionQuery()
    {
        return _dbContext.StockTransactions
            .AsSplitQuery()
            .Include(entity => entity.Item)
            .Include(entity => entity.Warehouse)
            .Include(entity => entity.Technician)
            .Include(entity => entity.JobCard)
            .Include(entity => entity.Supplier)
            .Where(entity => !entity.IsDeleted);
    }

    private IQueryable<JobCard> BuildJobCardQuery(bool asNoTracking)
    {
        IQueryable<JobCard> query = _dbContext.JobCards
            .AsSplitQuery()
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.Assignments.Where(assignment => !assignment.IsDeleted))
            .Include(entity => entity.PartConsumptions.Where(part => !part.IsDeleted))
                .ThenInclude(part => part.Item)
            .Include(entity => entity.PartConsumptions.Where(part => !part.IsDeleted))
                .ThenInclude(part => part.Technician)
            .Where(entity => !entity.IsDeleted);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    private static IQueryable<StockTransaction> ApplyStockTransactionFilters(
        IQueryable<StockTransaction> query,
        StockTransactionType? transactionType,
        long? itemId,
        long? warehouseId,
        long? technicianId,
        long? jobCardId,
        DateTime? fromDateUtc,
        DateTime? toDateUtc)
    {
        if (transactionType.HasValue)
        {
            query = query.Where(entity => entity.TransactionType == transactionType.Value);
        }

        if (itemId.HasValue)
        {
            query = query.Where(entity => entity.ItemId == itemId.Value);
        }

        if (warehouseId.HasValue)
        {
            query = query.Where(entity => entity.WarehouseId == warehouseId.Value);
        }

        if (technicianId.HasValue)
        {
            query = query.Where(entity => entity.TechnicianId == technicianId.Value);
        }

        if (jobCardId.HasValue)
        {
            query = query.Where(entity => entity.JobCardId == jobCardId.Value);
        }

        if (fromDateUtc.HasValue)
        {
            query = query.Where(entity => entity.TransactionDateUtc >= fromDateUtc.Value);
        }

        if (toDateUtc.HasValue)
        {
            query = query.Where(entity => entity.TransactionDateUtc <= toDateUtc.Value);
        }

        return query;
    }
}
