-- =============================================================================
-- 06 — SLOTS: tblSlotConfiguration (templates) + tblSlotAvailability (per-date)
-- Three windows per zone (Morning 08-12 / Afternoon 12-16 / Evening 16-19),
-- then per-date availability for the next 14 days across every active zone.
-- Capacity = MaxBookingCount (PLACEHOLDER 5) — set to real technician capacity.
-- Requires file 05 (zones). Idempotent. Re-run daily/weekly to extend the window.
-- =============================================================================
BEGIN;

-- ── Slot configurations: 3 windows per active zone ──────────────────────────
INSERT INTO "tblSlotConfiguration"
  ("ZoneId","SlotLabel","StartTime","EndTime","MaxBookingCount","IsActive",
   "CompanyId","SiteId","BranchId","DisplayOnWeb","IsPublished","SortOrder","IPAddress","CreatedBy","DateCreated","IsDeleted")
SELECT z."ZoneId", w.label, w.starttime, w.endtime, 5, true,
   1,1,1,true,true,w.sort,'127.0.0.1','System Seed',timezone('utc',now()),false
FROM "tblZone" z
CROSS JOIN (VALUES
  ('Morning',   TIME '08:00:00', TIME '12:00:00', 1),
  ('Afternoon', TIME '12:00:00', TIME '16:00:00', 2),
  ('Evening',   TIME '16:00:00', TIME '19:00:00', 3)
) AS w(label, starttime, endtime, sort)
WHERE z."IsDeleted" = false
AND NOT EXISTS (
  SELECT 1 FROM "tblSlotConfiguration" sc
  WHERE sc."ZoneId" = z."ZoneId" AND sc."StartTime" = w.starttime AND sc."EndTime" = w.endtime AND sc."IsDeleted" = false
);

-- ── Slot availability: next 14 days for every active configuration ───────────
INSERT INTO "tblSlotAvailability"
  ("ZoneId","SlotConfigurationId","SlotDate","AvailableCapacity","ReservedCapacity","IsBlocked",
   "CompanyId","SiteId","BranchId","DisplayOnWeb","IsPublished","SortOrder","IPAddress","CreatedBy","DateCreated","IsDeleted")
SELECT sc."ZoneId", sc."SlotConfigurationId", d::date, sc."MaxBookingCount", 0, false,
   1,1,1,true,true,0,'127.0.0.1','System Seed',timezone('utc',now()),false
FROM "tblSlotConfiguration" sc
CROSS JOIN generate_series(CURRENT_DATE, CURRENT_DATE + INTERVAL '13 days', INTERVAL '1 day') AS d
WHERE sc."IsDeleted" = false
AND NOT EXISTS (
  SELECT 1 FROM "tblSlotAvailability" sa
  WHERE sa."ZoneId" = sc."ZoneId" AND sa."SlotConfigurationId" = sc."SlotConfigurationId"
    AND sa."SlotDate" = d::date AND sa."IsDeleted" = false
);

COMMIT;
