-- ============================================================================
-- Remove Bengaluru Zones — Live Database Migration
-- Created   : 09 Jun 2026
-- Requires  : 15_hyderabad_zones.sql must be run FIRST (ZoneId 3–10 must exist)
-- Description: Re-maps every FK that pointed at the placeholder Bengaluru zones
--              (ZoneId 1 = Central Bengaluru, ZoneId 2 = East Bengaluru) to the
--              correct Hyderabad zones, then hard-deletes the BLR zones and their
--              pincodes.
--
--   ZoneId 1 (Central Bengaluru)  →  ZoneId 3  (HYD-CENTRAL)
--   ZoneId 2 (East Bengaluru)     →  ZoneId 5  (HYD-SECUNDERABAD)
--
-- Run       : psql -U <user> -d <db> -f 16_remove_bengaluru.sql
-- ============================================================================

BEGIN;

-- ── 1. Slot configurations (ZoneId FK) ──────────────────────────────────────
UPDATE public."tblSlotConfiguration"
SET    "ZoneId" = CASE WHEN "ZoneId" = 1 THEN 3 ELSE 5 END
WHERE  "ZoneId" IN (1, 2);

-- ── 2. Slot availability (ZoneId FK) ────────────────────────────────────────
UPDATE public."tblSlotAvailability"
SET    "ZoneId" = CASE WHEN "ZoneId" = 1 THEN 3 ELSE 5 END
WHERE  "ZoneId" IN (1, 2);

-- ── 3. Technicians — base zone ───────────────────────────────────────────────
UPDATE public."tblTechnician"
SET    "BaseZoneId" = CASE WHEN "BaseZoneId" = 1 THEN 3 ELSE 5 END
WHERE  "BaseZoneId" IN (1, 2);

-- ── 4. Technician zone assignments ───────────────────────────────────────────
UPDATE public."tblTechnicianZone"
SET    "ZoneId" = CASE WHEN "ZoneId" = 1 THEN 3 ELSE 5 END
WHERE  "ZoneId" IN (1, 2);

-- ── 5. Customer addresses ─────────────────────────────────────────────────────
UPDATE public."tblCustomerAddress"
SET    "ZoneId"     = 3,
       "CityName"   = 'Hyderabad',
       "Pincode"    = '500001',
       "UpdatedBy"  = 'RemoveBengaluruMigration',
       "LastUpdated" = NOW()
WHERE  "ZoneId" IN (1, 2);

-- ── 6. Bookings — FK + all Bengaluru string snapshots ────────────────────────
UPDATE public."tblBooking"
SET    "ZoneId"            = 3,
       "ZoneNameSnapshot"  = 'Central Hyderabad',
       "CityNameSnapshot"  = 'Hyderabad',
       "PincodeSnapshot"   = '500001',
       "UpdatedBy"         = 'RemoveBengaluruMigration',
       "LastUpdated"       = NOW()
WHERE  "ZoneId" IN (1, 2);

-- ── 7. Service request assignments — update remarks text ─────────────────────
UPDATE public."tblServiceRequestAssignment"
SET    "AssignmentRemarks" = REPLACE(REPLACE("AssignmentRemarks", 'East Bengaluru', 'Secunderabad'), 'Central Bengaluru', 'Central Hyderabad')
WHERE  "AssignmentRemarks" LIKE '%Bengaluru%';

-- ── 8. Delete Bengaluru pincodes ─────────────────────────────────────────────
DELETE FROM public."tblZonePincode"
WHERE  "ZoneId" IN (1, 2);

-- ── 9. Delete Bengaluru zones ────────────────────────────────────────────────
DELETE FROM public."tblZone"
WHERE  "ZoneId" IN (1, 2);

COMMIT;


-- ── Verify: should return 0 rows after successful run ────────────────────────
SELECT 'tblZone BLR remaining'        AS check_name, COUNT(1) AS remaining FROM public."tblZone"          WHERE "ZoneId" IN (1, 2)
UNION ALL
SELECT 'tblZonePincode BLR remaining',                COUNT(1)              FROM public."tblZonePincode"    WHERE "ZoneId" IN (1, 2)
UNION ALL
SELECT 'tblSlotConfiguration BLR',                    COUNT(1)              FROM public."tblSlotConfiguration" WHERE "ZoneId" IN (1, 2)
UNION ALL
SELECT 'tblSlotAvailability BLR',                     COUNT(1)              FROM public."tblSlotAvailability"   WHERE "ZoneId" IN (1, 2)
UNION ALL
SELECT 'tblTechnician BLR BaseZone',                  COUNT(1)              FROM public."tblTechnician"      WHERE "BaseZoneId" IN (1, 2)
UNION ALL
SELECT 'tblTechnicianZone BLR',                       COUNT(1)              FROM public."tblTechnicianZone"  WHERE "ZoneId" IN (1, 2)
UNION ALL
SELECT 'tblCustomerAddress BLR',                      COUNT(1)              FROM public."tblCustomerAddress" WHERE "ZoneId" IN (1, 2)
UNION ALL
SELECT 'tblBooking BLR',                              COUNT(1)              FROM public."tblBooking"         WHERE "ZoneId" IN (1, 2);
