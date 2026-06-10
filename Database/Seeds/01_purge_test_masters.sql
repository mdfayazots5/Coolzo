-- =============================================================================
-- 01 — PURGE TEST MASTER DATA (soft delete)
-- Sets IsDeleted = true on all existing master rows so the public lookups
-- (which filter IsDeleted = false) stop returning test data. Non-destructive:
-- rows remain for audit / existing FK references (bookings keep working).
-- Idempotent. Run before the seed files.
-- =============================================================================
BEGIN;

UPDATE "tblSlotAvailability"  SET "IsDeleted" = true, "DateDeleted" = timezone('utc', now()), "DeletedBy" = 'System Seed' WHERE "IsDeleted" = false;
UPDATE "tblSlotConfiguration" SET "IsDeleted" = true, "DateDeleted" = timezone('utc', now()), "DeletedBy" = 'System Seed' WHERE "IsDeleted" = false;
UPDATE "tblService"           SET "IsDeleted" = true, "DateDeleted" = timezone('utc', now()), "DeletedBy" = 'System Seed' WHERE "IsDeleted" = false;
UPDATE "tblServiceCategory"   SET "IsDeleted" = true, "DateDeleted" = timezone('utc', now()), "DeletedBy" = 'System Seed' WHERE "IsDeleted" = false;
UPDATE "tblPricingModel"      SET "IsDeleted" = true, "DateDeleted" = timezone('utc', now()), "DeletedBy" = 'System Seed' WHERE "IsDeleted" = false;
UPDATE "tblAcType"            SET "IsDeleted" = true, "DateDeleted" = timezone('utc', now()), "DeletedBy" = 'System Seed' WHERE "IsDeleted" = false;
UPDATE "tblTonnage"           SET "IsDeleted" = true, "DateDeleted" = timezone('utc', now()), "DeletedBy" = 'System Seed' WHERE "IsDeleted" = false;
UPDATE "tblBrand"             SET "IsDeleted" = true, "DateDeleted" = timezone('utc', now()), "DeletedBy" = 'System Seed' WHERE "IsDeleted" = false;
UPDATE "tblZonePincode"       SET "IsDeleted" = true, "DateDeleted" = timezone('utc', now()), "DeletedBy" = 'System Seed' WHERE "IsDeleted" = false;
UPDATE "tblZone"              SET "IsDeleted" = true, "DateDeleted" = timezone('utc', now()), "DeletedBy" = 'System Seed' WHERE "IsDeleted" = false;

COMMIT;

-- -----------------------------------------------------------------------------
-- OPTIONAL HARD DELETE (DESTRUCTIVE — needs explicit approval; will fail if rows
-- are referenced by existing bookings/SRs via FK). Uncomment to use a clean slate.
-- -----------------------------------------------------------------------------
-- BEGIN;
-- DELETE FROM "tblSlotAvailability";
-- DELETE FROM "tblSlotConfiguration";
-- DELETE FROM "tblService";
-- DELETE FROM "tblServiceCategory";
-- DELETE FROM "tblPricingModel";
-- DELETE FROM "tblAcType";
-- DELETE FROM "tblTonnage";
-- DELETE FROM "tblBrand";
-- DELETE FROM "tblZonePincode";
-- DELETE FROM "tblZone";
-- COMMIT;
