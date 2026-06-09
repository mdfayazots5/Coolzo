-------------------------------------------------------------------------------------------------------------
-- Coolzo PostgreSQL | File 14 | Booking: optional equipment + slot
-------------------------------------------------------------------------------------------------------------
-- Date     : 08 Jun 2026
-- Purpose  : Align the booking-create contract with the redesigned 5-step wizard (2026-05-27), which no
--            longer captures Tonnage/Brand (technician verifies on-site) and dispatches emergency bookings
--            without a pre-selected slot.
-- Effect   : Drop NOT NULL on tblBooking.SlotAvailabilityId and tblBookingLine.TonnageId / BrandId.
-- Safety   : Idempotent — ALTER COLUMN ... DROP NOT NULL is a no-op if the column is already nullable.
--            No data is modified. Foreign keys (file 03) remain; they already permit NULL FK values.
-------------------------------------------------------------------------------------------------------------

ALTER TABLE public."tblBooking"     ALTER COLUMN "SlotAvailabilityId" DROP NOT NULL;
ALTER TABLE public."tblBookingLine" ALTER COLUMN "TonnageId"          DROP NOT NULL;
ALTER TABLE public."tblBookingLine" ALTER COLUMN "BrandId"            DROP NOT NULL;
