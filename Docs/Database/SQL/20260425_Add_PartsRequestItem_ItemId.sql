/*
  Coolzo - Inventory Parts Request ItemId Backfill
  Date: 2026-04-25
  Purpose:
    - Enforce IDs-only inventory routing by persisting ItemId on tblPartsRequestItem
    - Backfill existing parts-request item rows from tblItem.ItemCode
*/

SET NOCOUNT ON;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.tblPartsRequestItem')
      AND name = N'ItemId'
)
BEGIN
    ALTER TABLE dbo.tblPartsRequestItem
    ADD ItemId BIGINT NULL;
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.tblPartsRequestItem')
      AND name = N'IDX_tblPartsRequestItem_ItemId'
)
BEGIN
    CREATE INDEX IDX_tblPartsRequestItem_ItemId
        ON dbo.tblPartsRequestItem(ItemId);
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_tblPartsRequestItem_ItemId_tblItem_ItemId'
)
BEGIN
    ALTER TABLE dbo.tblPartsRequestItem
    ADD CONSTRAINT FK_tblPartsRequestItem_ItemId_tblItem_ItemId
        FOREIGN KEY (ItemId) REFERENCES dbo.tblItem(ItemId);
END

EXEC(N'
UPDATE requestItem
SET requestItem.ItemId = inventoryItem.ItemId
FROM dbo.tblPartsRequestItem requestItem
INNER JOIN dbo.tblItem inventoryItem
    ON inventoryItem.ItemCode = requestItem.PartCode
   AND inventoryItem.IsDeleted = 0
WHERE requestItem.ItemId IS NULL
  AND requestItem.IsDeleted = 0;
');

;WITH InventorySeedItems AS
(
    SELECT TOP (4)
        ItemId,
        ItemCode,
        ItemName,
        ROW_NUMBER() OVER (ORDER BY ItemId) AS RowNo
    FROM dbo.tblItem
    WHERE IsDeleted = 0
    ORDER BY ItemId
),
SeedRequestItems AS
(
    SELECT
        PartsRequestItemId,
        ROW_NUMBER() OVER (ORDER BY PartsRequestItemId) AS RowNo
    FROM dbo.tblPartsRequestItem
    WHERE IsDeleted = 0
      AND CreatedBy = N'Seed20260425'
      AND ItemId IS NULL
)
UPDATE requestItem
SET requestItem.ItemId = inventoryItem.ItemId,
    requestItem.PartCode = inventoryItem.ItemCode,
    requestItem.PartName = inventoryItem.ItemName
FROM dbo.tblPartsRequestItem requestItem
INNER JOIN SeedRequestItems seedItem
    ON seedItem.PartsRequestItemId = requestItem.PartsRequestItemId
INNER JOIN InventorySeedItems inventoryItem
    ON inventoryItem.RowNo = seedItem.RowNo;
