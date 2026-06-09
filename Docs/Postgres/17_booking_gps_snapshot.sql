-- =============================================
-- Migration : 17_booking_gps_snapshot.sql
-- Description: Add LatitudeSnapshot and LongitudeSnapshot columns to tblBooking
--              to persist GPS coordinates captured at booking time.
--              tblCustomerAddress already has Latitude/Longitude (added 2026-04-25).
--              This migration adds the snapshot equivalents on tblBooking.
-- Date       : 2026-06-09
-- =============================================

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'tblBooking' AND column_name = 'LatitudeSnapshot'
    ) THEN
        ALTER TABLE public."tblBooking"
            ADD COLUMN "LatitudeSnapshot"  DOUBLE PRECISION NULL,
            ADD COLUMN "LongitudeSnapshot" DOUBLE PRECISION NULL;
    END IF;
END $$;
