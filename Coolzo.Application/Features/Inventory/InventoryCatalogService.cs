using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Models;

namespace Coolzo.Application.Features.Inventory;

public sealed class InventoryCatalogService
{
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IInventoryRepository _inventoryRepository;

    public InventoryCatalogService(
        IInventoryRepository inventoryRepository,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _inventoryRepository = inventoryRepository;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<ItemCategory> GetOrCreateItemCategoryAsync(
        string categoryCode,
        string categoryName,
        CancellationToken cancellationToken)
    {
        var normalizedCode = NormalizeRequired(categoryCode);
        var normalizedName = NormalizeRequired(categoryName);

        var itemCategory = await _inventoryRepository.GetItemCategoryByCodeAsync(normalizedCode, cancellationToken);

        if (itemCategory is not null)
        {
            itemCategory.CategoryName = normalizedName;
            itemCategory.LastUpdated = _currentDateTime.UtcNow;
            itemCategory.UpdatedBy = _currentUserContext.UserName;
            return itemCategory;
        }

        itemCategory = new ItemCategory
        {
            CategoryCode = normalizedCode,
            CategoryName = normalizedName,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _inventoryRepository.AddItemCategoryAsync(itemCategory, cancellationToken);
        return itemCategory;
    }

    public async Task<UnitOfMeasure> GetOrCreateUnitOfMeasureAsync(
        string unitCode,
        string unitName,
        CancellationToken cancellationToken)
    {
        var normalizedCode = NormalizeRequired(unitCode);
        var normalizedName = NormalizeRequired(unitName);

        var unitOfMeasure = await _inventoryRepository.GetUnitOfMeasureByCodeAsync(normalizedCode, cancellationToken);

        if (unitOfMeasure is not null)
        {
            unitOfMeasure.UnitName = normalizedName;
            unitOfMeasure.LastUpdated = _currentDateTime.UtcNow;
            unitOfMeasure.UpdatedBy = _currentUserContext.UserName;
            return unitOfMeasure;
        }

        unitOfMeasure = new UnitOfMeasure
        {
            UnitCode = normalizedCode,
            UnitName = normalizedName,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _inventoryRepository.AddUnitOfMeasureAsync(unitOfMeasure, cancellationToken);
        return unitOfMeasure;
    }

    public async Task<Supplier?> GetOrCreateSupplierAsync(
        string? supplierCode,
        string? supplierName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(supplierCode) || string.IsNullOrWhiteSpace(supplierName))
        {
            return null;
        }

        var normalizedCode = NormalizeRequired(supplierCode);
        var normalizedName = NormalizeRequired(supplierName);

        var supplier = await _inventoryRepository.GetSupplierByCodeAsync(normalizedCode, cancellationToken);

        if (supplier is not null)
        {
            supplier.SupplierName = normalizedName;
            supplier.LastUpdated = _currentDateTime.UtcNow;
            supplier.UpdatedBy = _currentUserContext.UserName;
            return supplier;
        }

        supplier = new Supplier
        {
            SupplierCode = normalizedCode,
            SupplierName = normalizedName,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _inventoryRepository.AddSupplierAsync(supplier, cancellationToken);
        return supplier;
    }

    public async Task<ItemRate> EnsureCurrentRateAsync(
        Item item,
        decimal purchasePrice,
        decimal sellingPrice,
        CancellationToken cancellationToken)
    {
        var currentRate = item.Rates
            .Where(rate => rate.IsActive && !rate.IsDeleted)
            .OrderByDescending(rate => rate.EffectiveFromUtc)
            .ThenByDescending(rate => rate.ItemRateId)
            .FirstOrDefault();

        if (currentRate is not null &&
            currentRate.PurchasePrice == purchasePrice &&
            currentRate.SellingPrice == sellingPrice)
        {
            return currentRate;
        }

        if (currentRate is not null)
        {
            currentRate.IsActive = false;
            currentRate.EffectiveToUtc = _currentDateTime.UtcNow;
            currentRate.LastUpdated = _currentDateTime.UtcNow;
            currentRate.UpdatedBy = _currentUserContext.UserName;
        }

        var nextRate = new ItemRate
        {
            ItemId = item.ItemId,
            PurchasePrice = purchasePrice,
            SellingPrice = sellingPrice,
            EffectiveFromUtc = _currentDateTime.UtcNow,
            IsActive = true,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _inventoryRepository.AddItemRateAsync(nextRate, cancellationToken);
        item.Rates.Add(nextRate);
        return nextRate;
    }

    private static string NormalizeRequired(string value)
    {
        return value.Trim();
    }
}
