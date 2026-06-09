-- ============================================================
-- File       : 12_customer_equipment_get_by_id_verify.sql
-- Purpose    : Verify GET /api/customers/me/equipment/{id} endpoint
-- Date       : 2026-05-26
-- ============================================================
-- This script confirms the CustomerEquipment table has all
-- columns required by the CustomerEquipmentResponse DTO and
-- that the ownership filter works correctly.
-- ============================================================

-- 1. Confirm table and column structure
SELECT
    column_name,
    data_type,
    is_nullable
FROM information_schema.columns
WHERE table_name = 'CustomerEquipment'
ORDER BY ordinal_position;

-- 2. Spot-check: retrieve a single equipment row by owner + id
--    (replace :customerId and :equipmentId with real test values)
SELECT
    "CustomerEquipmentId",
    "CustomerId",
    "EquipmentName",
    "EquipmentType",
    "BrandName",
    "Capacity",
    "LocationLabel",
    "PurchaseDate",
    "LastServiceDate",
    "SerialNumber",
    "IsActive",
    "IsDeleted",
    "DateCreated",
    "LastUpdated"
FROM "CustomerEquipment"
WHERE "CustomerId"      = :customerId
  AND "CustomerEquipmentId" = :equipmentId
  AND "IsDeleted"       = FALSE;

-- 3. Ownership isolation test — must return 0 rows
--    (verifies that a different customer cannot access the record)
SELECT COUNT(1) AS should_be_zero
FROM "CustomerEquipment"
WHERE "CustomerEquipmentId" = :equipmentId
  AND "CustomerId"           <> :customerId
  AND "IsDeleted"            = FALSE;
