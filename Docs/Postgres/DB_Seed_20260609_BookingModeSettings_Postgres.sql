-------------------------------------------------------------------------------------------------------------
-- Created By      : Coolzo System
-- Date Created    : 09 Jun 2026
-- Description     : Seed two booking-mode system settings (PostgreSQL / Supabase variant).
--                   Booking.OpenBookingMode     = false  -> slot selection required (default)
--                   Booking.EnforceSlotCapacity = true   -> capacity limits enforced (default)
-- Usage           : Run once against the target Postgres database (Provider=Postgres environments).
-- Note            : Postgres equivalent of DB_Seed_20260609_BookingModeSettings.sql (SQL Server).
--                   Idempotent via ON CONFLICT-style NOT EXISTS guard. Boolean stored as text
--                   'false'/'true' to match how the handlers bool.TryParse the SettingValue.
-------------------------------------------------------------------------------------------------------------

INSERT INTO public."tblSystemSetting"
    ("SettingKey", "SettingValue", "DataType", "IsSensitive",
     "CreatedBy", "DateCreated", "IPAddress", "IsDeleted")
SELECT 'Booking.OpenBookingMode', 'false', 'Boolean', FALSE,
       'System', NOW(), '127.0.0.1', FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM public."tblSystemSetting"
    WHERE "SettingKey" = 'Booking.OpenBookingMode' AND "IsDeleted" = FALSE
);

INSERT INTO public."tblSystemSetting"
    ("SettingKey", "SettingValue", "DataType", "IsSensitive",
     "CreatedBy", "DateCreated", "IPAddress", "IsDeleted")
SELECT 'Booking.EnforceSlotCapacity', 'true', 'Boolean', FALSE,
       'System', NOW(), '127.0.0.1', FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM public."tblSystemSetting"
    WHERE "SettingKey" = 'Booking.EnforceSlotCapacity' AND "IsDeleted" = FALSE
);
