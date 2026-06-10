-- =============================================================================
-- 09 — BOOKING MODE FLAGS (tblSystemSetting)
-- Two flags read by the booking handlers + public wizard:
--   Booking.OpenBookingMode     false => customer MUST pick a slot (we have slots seeded)
--   Booking.EnforceSlotCapacity true  => per-slot capacity is respected (full slots disabled)
-- Upsert by unique SettingKey; reactivates if previously soft-deleted.
-- =============================================================================
BEGIN;

-- NOTE: tblSystemSetting audit columns EXCLUDE BranchId (ConfigureAuditColumns includeBranchId:false).
INSERT INTO "tblSystemSetting"
  ("SettingKey","SettingValue","DataType","IsSensitive",
   "CompanyId","SiteId","DisplayOnWeb","IsPublished","SortOrder","IPAddress","CreatedBy","DateCreated","IsDeleted")
VALUES
  ('Booking.OpenBookingMode',     'false', 'Boolean', false, 1,1,true,true,0,'127.0.0.1','System Seed',timezone('utc',now()),false),
  ('Booking.EnforceSlotCapacity', 'true',  'Boolean', false, 1,1,true,true,0,'127.0.0.1','System Seed',timezone('utc',now()),false)
ON CONFLICT ("SettingKey") DO UPDATE SET
  "SettingValue" = EXCLUDED."SettingValue",
  "DataType"     = EXCLUDED."DataType",
  "LastUpdated"  = timezone('utc', now()),
  "UpdatedBy"    = 'System Seed',
  "IsDeleted"    = false;

COMMIT;
