using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using DomainTechnician = Coolzo.Domain.Entities.Technician;

namespace Coolzo.Application.Features.Inventory;

public sealed class InventoryStockService
{
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IInventoryRepository _inventoryRepository;

    public InventoryStockService(
        IInventoryRepository inventoryRepository,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _inventoryRepository = inventoryRepository;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<StockTransaction> RecordWarehouseTransactionAsync(
        Warehouse warehouse,
        Item item,
        StockTransactionType transactionType,
        decimal quantity,
        decimal unitCost,
        long? supplierId,
        string? referenceNumber,
        string? remarks,
        CancellationToken cancellationToken)
    {
        if (!IsWarehouseTransaction(transactionType))
        {
            throw new AppException(
                ErrorCodes.InvalidStockTransactionType,
                "The supplied stock transaction type is not allowed for direct warehouse entry.",
                400);
        }

        var warehouseStock = await GetOrCreateWarehouseStockAsync(warehouse.WarehouseId, item.ItemId, cancellationToken);
        var signedQuantity = ResolveSignedQuantity(transactionType, quantity);
        var nextBalance = warehouseStock.QuantityOnHand + signedQuantity;

        if (nextBalance < 0)
        {
            throw new AppException(
                ErrorCodes.StockInsufficient,
                "Warehouse stock cannot become negative.",
                409);
        }

        warehouseStock.QuantityOnHand = nextBalance;
        warehouseStock.LastTransactionDateUtc = _currentDateTime.UtcNow;
        warehouseStock.LastUpdated = _currentDateTime.UtcNow;
        warehouseStock.UpdatedBy = _currentUserContext.UserName;

        var stockTransaction = CreateTransaction(
            item.ItemId,
            transactionType,
            quantity,
            unitCost,
            nextBalance,
            referenceNumber,
            remarks);

        stockTransaction.WarehouseId = warehouse.WarehouseId;
        stockTransaction.SupplierId = supplierId;

        await _inventoryRepository.AddStockTransactionAsync(stockTransaction, cancellationToken);
        warehouseStock.Item ??= item;
        stockTransaction.Item = item;
        stockTransaction.Warehouse = warehouse;

        return stockTransaction;
    }

    public async Task<IReadOnlyCollection<StockTransaction>> TransferWarehouseStockAsync(
        Warehouse sourceWarehouse,
        Warehouse destinationWarehouse,
        Item item,
        decimal quantity,
        decimal unitCost,
        string? referenceNumber,
        string? remarks,
        CancellationToken cancellationToken)
    {
        if (sourceWarehouse.WarehouseId == destinationWarehouse.WarehouseId)
        {
            throw new AppException(
                ErrorCodes.InvalidStockLocation,
                "Stock transfer requires different source and destination warehouses.",
                400);
        }

        var transactionGroupCode = BuildTransactionGroupCode();
        var sourceStock = await GetOrCreateWarehouseStockAsync(sourceWarehouse.WarehouseId, item.ItemId, cancellationToken);
        var destinationStock = await GetOrCreateWarehouseStockAsync(destinationWarehouse.WarehouseId, item.ItemId, cancellationToken);

        EnsureSufficientQuantity(sourceStock.QuantityOnHand, quantity, "source warehouse");

        sourceStock.QuantityOnHand -= quantity;
        sourceStock.LastTransactionDateUtc = _currentDateTime.UtcNow;
        sourceStock.LastUpdated = _currentDateTime.UtcNow;
        sourceStock.UpdatedBy = _currentUserContext.UserName;

        destinationStock.QuantityOnHand += quantity;
        destinationStock.LastTransactionDateUtc = _currentDateTime.UtcNow;
        destinationStock.LastUpdated = _currentDateTime.UtcNow;
        destinationStock.UpdatedBy = _currentUserContext.UserName;

        var transferOut = CreateTransaction(
            item.ItemId,
            StockTransactionType.TransferOut,
            quantity,
            unitCost,
            sourceStock.QuantityOnHand,
            referenceNumber,
            remarks,
            transactionGroupCode);
        transferOut.WarehouseId = sourceWarehouse.WarehouseId;
        transferOut.Item = item;
        transferOut.Warehouse = sourceWarehouse;

        var transferIn = CreateTransaction(
            item.ItemId,
            StockTransactionType.TransferIn,
            quantity,
            unitCost,
            destinationStock.QuantityOnHand,
            referenceNumber,
            remarks,
            transactionGroupCode);
        transferIn.WarehouseId = destinationWarehouse.WarehouseId;
        transferIn.Item = item;
        transferIn.Warehouse = destinationWarehouse;

        await _inventoryRepository.AddStockTransactionAsync(transferOut, cancellationToken);
        await _inventoryRepository.AddStockTransactionAsync(transferIn, cancellationToken);

        return new[] { transferOut, transferIn };
    }

    public async Task<IReadOnlyCollection<StockTransaction>> AssignStockToTechnicianAsync(
        Warehouse sourceWarehouse,
        DomainTechnician technician,
        Item item,
        decimal quantity,
        decimal unitCost,
        string? referenceNumber,
        string? remarks,
        CancellationToken cancellationToken)
    {
        var transactionGroupCode = BuildTransactionGroupCode();
        var sourceStock = await GetOrCreateWarehouseStockAsync(sourceWarehouse.WarehouseId, item.ItemId, cancellationToken);
        var technicianStock = await GetOrCreateTechnicianStockAsync(technician.TechnicianId, item.ItemId, cancellationToken);

        EnsureSufficientQuantity(sourceStock.QuantityOnHand, quantity, "warehouse");

        sourceStock.QuantityOnHand -= quantity;
        sourceStock.LastTransactionDateUtc = _currentDateTime.UtcNow;
        sourceStock.LastUpdated = _currentDateTime.UtcNow;
        sourceStock.UpdatedBy = _currentUserContext.UserName;

        technicianStock.QuantityOnHand += quantity;
        technicianStock.LastTransactionDateUtc = _currentDateTime.UtcNow;
        technicianStock.LastUpdated = _currentDateTime.UtcNow;
        technicianStock.UpdatedBy = _currentUserContext.UserName;

        var transferOut = CreateTransaction(
            item.ItemId,
            StockTransactionType.TransferOut,
            quantity,
            unitCost,
            sourceStock.QuantityOnHand,
            referenceNumber,
            remarks,
            transactionGroupCode);
        transferOut.WarehouseId = sourceWarehouse.WarehouseId;
        transferOut.Item = item;
        transferOut.Warehouse = sourceWarehouse;

        var transferIn = CreateTransaction(
            item.ItemId,
            StockTransactionType.TransferIn,
            quantity,
            unitCost,
            technicianStock.QuantityOnHand,
            referenceNumber,
            remarks,
            transactionGroupCode);
        transferIn.TechnicianId = technician.TechnicianId;
        transferIn.Item = item;
        transferIn.Technician = technician;

        await _inventoryRepository.AddStockTransactionAsync(transferOut, cancellationToken);
        await _inventoryRepository.AddStockTransactionAsync(transferIn, cancellationToken);

        return new[] { transferOut, transferIn };
    }

    public async Task<(IReadOnlyCollection<JobPartConsumption> Consumptions, IReadOnlyCollection<StockTransaction> Transactions)> ConsumeJobPartsAsync(
        JobCard jobCard,
        DomainTechnician technician,
        IReadOnlyCollection<(Item Item, decimal QuantityUsed, decimal UnitPrice, string? Remarks)> lines,
        CancellationToken cancellationToken)
    {
        var transactionGroupCode = BuildTransactionGroupCode();
        var consumptions = new List<JobPartConsumption>();
        var transactions = new List<StockTransaction>();

        foreach (var line in lines)
        {
            var technicianStock = await GetOrCreateTechnicianStockAsync(technician.TechnicianId, line.Item.ItemId, cancellationToken);
            EnsureSufficientQuantity(technicianStock.QuantityOnHand, line.QuantityUsed, "technician stock");

            technicianStock.QuantityOnHand -= line.QuantityUsed;
            technicianStock.LastTransactionDateUtc = _currentDateTime.UtcNow;
            technicianStock.LastUpdated = _currentDateTime.UtcNow;
            technicianStock.UpdatedBy = _currentUserContext.UserName;

            var stockTransaction = CreateTransaction(
                line.Item.ItemId,
                StockTransactionType.JobConsumption,
                line.QuantityUsed,
                line.UnitPrice,
                technicianStock.QuantityOnHand,
                jobCard.JobCardNumber,
                line.Remarks,
                transactionGroupCode);
            stockTransaction.TechnicianId = technician.TechnicianId;
            stockTransaction.JobCardId = jobCard.JobCardId;
            stockTransaction.Item = line.Item;
            stockTransaction.Technician = technician;
            stockTransaction.JobCard = jobCard;

            await _inventoryRepository.AddStockTransactionAsync(stockTransaction, cancellationToken);

            var jobPartConsumption = new JobPartConsumption
            {
                JobCardId = jobCard.JobCardId,
                TechnicianId = technician.TechnicianId,
                ItemId = line.Item.ItemId,
                StockTransaction = stockTransaction,
                QuantityUsed = line.QuantityUsed,
                UnitPrice = line.UnitPrice,
                LineAmount = decimal.Round(line.QuantityUsed * line.UnitPrice, 2),
                ConsumedDateUtc = _currentDateTime.UtcNow,
                ConsumptionRemarks = NormalizeText(line.Remarks),
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress,
                Item = line.Item,
                Technician = technician,
                JobCard = jobCard
            };

            await _inventoryRepository.AddJobPartConsumptionAsync(jobPartConsumption, cancellationToken);
            consumptions.Add(jobPartConsumption);
            transactions.Add(stockTransaction);
        }

        return (consumptions, transactions);
    }

    private async Task<WarehouseStock> GetOrCreateWarehouseStockAsync(
        long warehouseId,
        long itemId,
        CancellationToken cancellationToken)
    {
        var warehouseStock = await _inventoryRepository.GetWarehouseStockEntryForUpdateAsync(warehouseId, itemId, cancellationToken);

        if (warehouseStock is not null)
        {
            return warehouseStock;
        }

        warehouseStock = new WarehouseStock
        {
            WarehouseId = warehouseId,
            ItemId = itemId,
            QuantityOnHand = 0.00m,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _inventoryRepository.AddWarehouseStockAsync(warehouseStock, cancellationToken);
        return warehouseStock;
    }

    private async Task<TechnicianVanStock> GetOrCreateTechnicianStockAsync(
        long technicianId,
        long itemId,
        CancellationToken cancellationToken)
    {
        var technicianStock = await _inventoryRepository.GetTechnicianStockEntryForUpdateAsync(technicianId, itemId, cancellationToken);

        if (technicianStock is not null)
        {
            return technicianStock;
        }

        technicianStock = new TechnicianVanStock
        {
            TechnicianId = technicianId,
            ItemId = itemId,
            QuantityOnHand = 0.00m,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _inventoryRepository.AddTechnicianStockAsync(technicianStock, cancellationToken);
        return technicianStock;
    }

    private StockTransaction CreateTransaction(
        long itemId,
        StockTransactionType transactionType,
        decimal quantity,
        decimal unitCost,
        decimal balanceAfterTransaction,
        string? referenceNumber,
        string? remarks,
        string? transactionGroupCode = null)
    {
        return new StockTransaction
        {
            ItemId = itemId,
            TransactionType = transactionType,
            Quantity = quantity,
            UnitCost = unitCost,
            Amount = decimal.Round(quantity * unitCost, 2),
            BalanceAfterTransaction = balanceAfterTransaction,
            ReferenceNumber = NormalizeText(referenceNumber),
            TransactionGroupCode = transactionGroupCode ?? BuildTransactionGroupCode(),
            TransactionDateUtc = _currentDateTime.UtcNow,
            Remarks = NormalizeText(remarks),
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };
    }

    private static decimal ResolveSignedQuantity(StockTransactionType transactionType, decimal quantity)
    {
        return transactionType switch
        {
            StockTransactionType.PurchaseIn or
            StockTransactionType.AdjustmentIn or
            StockTransactionType.ReturnIn or
            StockTransactionType.TransferIn => quantity,
            StockTransactionType.AdjustmentOut or
            StockTransactionType.TransferOut or
            StockTransactionType.ReturnOut or
            StockTransactionType.JobConsumption => quantity * -1,
            _ => quantity
        };
    }

    private static bool IsWarehouseTransaction(StockTransactionType transactionType)
    {
        return transactionType is
            StockTransactionType.PurchaseIn or
            StockTransactionType.AdjustmentIn or
            StockTransactionType.AdjustmentOut or
            StockTransactionType.ReturnIn or
            StockTransactionType.ReturnOut;
    }

    private static void EnsureSufficientQuantity(decimal currentQuantity, decimal requestedQuantity, string sourceName)
    {
        if (currentQuantity < requestedQuantity)
        {
            throw new AppException(
                ErrorCodes.StockInsufficient,
                $"Insufficient stock is available in the {sourceName}.",
                409);
        }
    }

    private string BuildTransactionGroupCode()
    {
        return $"INV-{_currentDateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..36];
    }

    private static string NormalizeText(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }
}
