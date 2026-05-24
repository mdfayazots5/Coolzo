DROP PROCEDURE IF EXISTS public.uspgetdashboardsummary();
DROP PROCEDURE IF EXISTS dbo.uspgetdashboardsummary();

CREATE OR REPLACE PROCEDURE public.uspgetdashboardsummary(
    OUT "TotalBookings" BIGINT,
    OUT "TotalServiceRequests" BIGINT,
    OUT "TotalJobs" BIGINT,
    OUT "TotalRevenue" NUMERIC(18,2),
    OUT "TotalAmcCustomers" BIGINT,
    OUT "TotalSupportTickets" BIGINT
)
LANGUAGE plpgsql
AS $$
BEGIN
    SELECT COUNT(*)
    INTO "TotalBookings"
    FROM public."tblBooking"
    WHERE NOT COALESCE("IsDeleted", FALSE);

    SELECT COUNT(*)
    INTO "TotalServiceRequests"
    FROM public."tblServiceRequest"
    WHERE NOT COALESCE("IsDeleted", FALSE);

    SELECT COUNT(*)
    INTO "TotalJobs"
    FROM public."tblJobCard"
    WHERE NOT COALESCE("IsDeleted", FALSE);

    SELECT COALESCE(SUM("GrandTotalAmount"), 0)
    INTO "TotalRevenue"
    FROM public."tblInvoiceHeader"
    WHERE NOT COALESCE("IsDeleted", FALSE);

    SELECT COUNT(*)
    INTO "TotalAmcCustomers"
    FROM public."tblCustomerAMC"
    WHERE NOT COALESCE("IsDeleted", FALSE);

    SELECT COUNT(*)
    INTO "TotalSupportTickets"
    FROM public."tblSupportTicket"
    WHERE NOT COALESCE("IsDeleted", FALSE);
END;
$$;

-- Manual verification:
-- CALL public.uspGetDashboardSummary();
--
-- Scope note:
-- This compatibility script fixes the scalar summary path used by GET /api/dashboard/summary.
-- Other analytics endpoints still rely on additional dbo.usp* routines that are not part of this script.
