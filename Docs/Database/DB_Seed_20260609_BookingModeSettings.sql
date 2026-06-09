SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-------------------------------------------------------------------------------------------------------------
-- Created By      : Coolzo System
-- Date Created    : 09 Jun 2026
-- Description     : Seed two booking-mode system settings.
--                   Booking.OpenBookingMode    = false  → slot selection required (default)
--                   Booking.EnforceSlotCapacity = true  → capacity limits enforced (default)
-- Usage           : Run once against the target database.
-------------------------------------------------------------------------------------------------------------

IF NOT EXISTS (
    SELECT 1 FROM dbo.tblSystemSetting
    WHERE SettingKey = 'Booking.OpenBookingMode' AND IsDeleted = 0
)
BEGIN
    INSERT INTO dbo.tblSystemSetting
        (SettingKey, SettingValue, DataType, IsSensitive,
         CreatedBy, DateCreated, IPAddress, IsDeleted)
    VALUES
        ('Booking.OpenBookingMode', 'false', 'Boolean', 0,
         'System', GETDATE(), '127.0.0.1', 0);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM dbo.tblSystemSetting
    WHERE SettingKey = 'Booking.EnforceSlotCapacity' AND IsDeleted = 0
)
BEGIN
    INSERT INTO dbo.tblSystemSetting
        (SettingKey, SettingValue, DataType, IsSensitive,
         CreatedBy, DateCreated, IPAddress, IsDeleted)
    VALUES
        ('Booking.EnforceSlotCapacity', 'true', 'Boolean', 0,
         'System', GETDATE(), '127.0.0.1', 0);
END
GO
