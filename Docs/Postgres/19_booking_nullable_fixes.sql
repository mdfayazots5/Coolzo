-- =============================================
-- Migration : 19_booking_nullable_fixes.sql
-- Description: Fix NOT NULL constraints that block valid bookings:
--              1. tblBookingLine.TonnageId and BrandId — optional (verified on-site by technician)
--              2. tblBooking.SlotAvailabilityId — NULL for emergency bookings (dispatched without slot)
-- Date       : 2026-06-09
-- =============================================

-- tblBookingLine: tonnage and brand are collected on-site, not required at booking time
ALTER TABLE public."tblBookingLine"
    ALTER COLUMN "TonnageId" DROP NOT NULL,
    ALTER COLUMN "BrandId"   DROP NOT NULL;

-- tblBooking: emergency bookings dispatch without a pre-selected slot
ALTER TABLE public."tblBooking"
    ALTER COLUMN "SlotAvailabilityId" DROP NOT NULL;
