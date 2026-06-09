-- =============================================
-- Migration : 18_slot_data_all_zones.sql
-- Description: 1. Add slot configurations for the 6 zones that have none
--                 (zones 4,6,7,8,9,10 — zones 3 & 5 already exist).
--              2. Generate tblSlotAvailability for ALL 8 zones for the
--                 next 90 days from today (2026-06-09 → 2026-09-07).
--                 Idempotent — skips rows that already exist.
-- Date       : 2026-06-09
-- =============================================

-- ─── Step 1: Slot Configurations for missing zones ───────────────────────────
-- Same 4 time windows as zones 3 & 5. Skip if already present.

INSERT INTO public."tblSlotConfiguration"
    ("CompanyId","SiteId","ZoneId","SlotLabel","StartTime","EndTime",
     "MaxBookingCount","IsActive","DisplayOnWeb","IsPublished","SortOrder",
     "IPAddress","CreatedBy","DateCreated","IsDeleted","BranchId")
SELECT
    1, 1, z."ZoneId", s."SlotLabel", s."StartTime", s."EndTime",
    4, true, true, true, s."SortOrder",
    '127.0.0.1', 'SlotSetup', NOW(), false, 1
FROM
    (VALUES
        ('09:00 AM - 11:00 AM', '09:00:00'::time, '11:00:00'::time, 1),
        ('11:30 AM - 01:30 PM', '11:30:00'::time, '13:30:00'::time, 2),
        ('02:30 PM - 04:30 PM', '14:30:00'::time, '16:30:00'::time, 3),
        ('05:00 PM - 07:00 PM', '17:00:00'::time, '19:00:00'::time, 4)
    ) AS s("SlotLabel", "StartTime", "EndTime", "SortOrder")
    CROSS JOIN (
        SELECT "ZoneId" FROM public."tblZone"
        WHERE "ZoneId" IN (4, 6, 7, 8, 9, 10) AND "IsDeleted" = false
    ) z
WHERE NOT EXISTS (
    SELECT 1 FROM public."tblSlotConfiguration" sc
    WHERE sc."ZoneId" = z."ZoneId"
      AND sc."SlotLabel" = s."SlotLabel"
      AND sc."IsDeleted" = false
);

-- ─── Step 2: Slot Availability — all zones, next 90 days ─────────────────────
-- Cross-join all active slot configs with every date in the window.
-- Skip any (SlotConfigurationId, SlotDate) pair that already exists.

INSERT INTO public."tblSlotAvailability"
    ("CompanyId","SiteId","SlotConfigurationId","ZoneId","SlotDate",
     "AvailableCapacity","ReservedCapacity","IsBlocked",
     "DisplayOnWeb","IsPublished","SortOrder",
     "IPAddress","CreatedBy","DateCreated","IsDeleted","BranchId")
SELECT
    1, 1,
    sc."SlotConfigurationId", sc."ZoneId", d::date,
    4, 0, false,
    true, true, 0,
    '127.0.0.1', 'SlotSetup', NOW(), false, 1
FROM
    public."tblSlotConfiguration" sc
    CROSS JOIN generate_series(
        CURRENT_DATE,
        CURRENT_DATE + INTERVAL '89 days',
        INTERVAL '1 day'
    ) AS d
WHERE
    sc."IsActive" = true
    AND sc."IsDeleted" = false
    AND NOT EXISTS (
        SELECT 1 FROM public."tblSlotAvailability" sa
        WHERE sa."SlotConfigurationId" = sc."SlotConfigurationId"
          AND sa."SlotDate" = d::date
          AND sa."IsDeleted" = false
    );
