-- =============================================================
-- Coolzo PostgreSQL Final Fix Script
-- File: 11_final_analytics_fix.sql
-- Date: 2026-05-12
--
-- PURPOSE: Create all 7 analytics stored procedures in the dbo schema
-- with lowercase unquoted names so Npgsql 8 can find them.
--
-- ERROR BEING FIXED:
--   42883: procedure dbo.uspgetbookinganalytics(DateFrom => timestamp
--   without time zone, ...) does not exist
--
-- ROOT CAUSE: Previous scripts created procedures with QUOTED MIXED-CASE
-- names (dbo."uspGetBookingAnalytics"). Npgsql sends names without extra
-- quoting, PostgreSQL folds them to lowercase, so "uspGetBookingAnalytics"
-- ≠ "uspgetbookinganalytics". The procedures never matched.
--
-- THIS SCRIPT IS SAFE TO RE-RUN — all drops use exception handlers.
-- =============================================================


-- =============================================================
-- STEP 1: Ensure dbo schema exists
-- =============================================================
CREATE SCHEMA IF NOT EXISTS dbo;


-- =============================================================
-- STEP 2: Drop ALL variants (quoted mixed-case AND lowercase)
-- using safe exception handlers so a wrong-type or missing object
-- never aborts the script.
-- =============================================================

DO $$
DECLARE
    names TEXT[] := ARRAY[
        'uspGetBookingAnalytics',
        'uspGetRevenueAnalytics',
        'uspGetTechnicianPerformance',
        'uspGetCustomerAnalytics',
        'uspGetSupportAnalytics',
        'uspGetInventoryAnalytics',
        'uspGetReportByDateRange',
        'uspgetbookinganalytics',
        'uspgetrevenueanalytics',
        'uspgettechnicianperformance',
        'uspgetcustomeranalytics',
        'uspgetsupportanalytics',
        'uspgetinventoryanalytics',
        'uspgetreportbydaterange'
    ];
    n TEXT;
BEGIN
    FOREACH n IN ARRAY names LOOP
        BEGIN
            EXECUTE format('DROP PROCEDURE IF EXISTS dbo.%I CASCADE', n);
        EXCEPTION WHEN OTHERS THEN
            NULL;
        END;
    END LOOP;
    RAISE NOTICE 'Step 2 complete: old dbo procedures cleared.';
END $$;


-- =============================================================
-- STEP 3: Also ensure dashboard summary is a FUNCTION (not procedure)
-- In case file 10 Section 3 was not yet applied.
-- =============================================================

DO $$
BEGIN
    EXECUTE 'DROP PROCEDURE IF EXISTS public.uspgetdashboardsummary()';
EXCEPTION WHEN OTHERS THEN NULL;
END $$;
DROP FUNCTION IF EXISTS public.uspgetdashboardsummary();

CREATE OR REPLACE FUNCTION public.uspgetdashboardsummary()
RETURNS TABLE (
    "TotalBookings"        BIGINT,
    "TotalServiceRequests" BIGINT,
    "TotalJobs"            BIGINT,
    "TotalRevenue"         NUMERIC(18,2),
    "TotalAmcCustomers"    BIGINT,
    "TotalSupportTickets"  BIGINT
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        (SELECT COUNT(*)::BIGINT FROM public."tblBooking"       WHERE NOT COALESCE("IsDeleted", FALSE)),
        (SELECT COUNT(*)::BIGINT FROM public."tblServiceRequest" WHERE NOT COALESCE("IsDeleted", FALSE)),
        (SELECT COUNT(*)::BIGINT FROM public."tblJobCard"        WHERE NOT COALESCE("IsDeleted", FALSE)),
        (SELECT COALESCE(SUM("GrandTotalAmount"), 0)::NUMERIC(18,2)
                FROM public."tblInvoiceHeader"                   WHERE NOT COALESCE("IsDeleted", FALSE)),
        (SELECT COUNT(*)::BIGINT FROM public."tblCustomerAMC"    WHERE NOT COALESCE("IsDeleted", FALSE)),
        (SELECT COUNT(*)::BIGINT FROM public."tblSupportTicket"  WHERE NOT COALESCE("IsDeleted", FALSE));
END;
$$;
GRANT EXECUTE ON FUNCTION public.uspgetdashboardsummary() TO PUBLIC;


-- =============================================================
-- STEP 4: Create all 7 analytics procedures with LOWERCASE names
--
-- Parameter names MUST match exactly what C# sends via Npgsql:
--   DateFrom, DateTo, TrendBy  ← AddDateParameters
--   ServiceId / TechnicianId / Status  ← AddParameter
--   ref1..refN  ← AddRefCursors
-- =============================================================


-- -----------------------------------------------------------
-- 4.1  dbo.uspgetbookinganalytics
--      C# call: BuildAnalyticsCommand("dbo.uspGetBookingAnalytics")
--               + @DateFrom, @DateTo, @TrendBy, @ServiceId, @Status
--               + ref1, ref2, ref3, ref4
--      RS 1: TotalBookings, PendingBookings, ConfirmedBookings,
--            CancelledBookings, AverageBookingsPerPeriod
--      RS 2: PeriodStartDate, PeriodLabel, Value (trend)
--      RS 3: Label, Value (status distribution)
--      RS 4: Label, Value (service distribution)
-- -----------------------------------------------------------
CREATE OR REPLACE PROCEDURE dbo.uspgetbookinganalytics(
    IN  datefrom  TIMESTAMP WITHOUT TIME ZONE,
    IN  dateto    TIMESTAMP WITHOUT TIME ZONE,
    IN  trendby   TEXT,
    IN  serviceid BIGINT,
    IN  status    INTEGER,
    INOUT ref1    REFCURSOR,
    INOUT ref2    REFCURSOR,
    INOUT ref3    REFCURSOR,
    INOUT ref4    REFCURSOR
)
LANGUAGE plpgsql AS $$
DECLARE
    v_period INTERVAL;
BEGIN
    v_period := CASE trendby
        WHEN 'week'  THEN INTERVAL '7 days'
        WHEN 'month' THEN INTERVAL '30 days'
        ELSE              INTERVAL '1 day'
    END;

    -- RS 1: Summary
    OPEN ref1 FOR
    SELECT
        COUNT(*)::BIGINT AS "TotalBookings",
        SUM(CASE WHEN b."BookingStatus" = 1 THEN 1 ELSE 0 END)::BIGINT AS "PendingBookings",
        SUM(CASE WHEN b."BookingStatus" = 2 THEN 1 ELSE 0 END)::BIGINT AS "ConfirmedBookings",
        SUM(CASE WHEN b."BookingStatus" = 3 THEN 1 ELSE 0 END)::BIGINT AS "CancelledBookings",
        CASE
            WHEN NULLIF(EXTRACT(EPOCH FROM v_period), 0) IS NOT NULL
             AND EXTRACT(EPOCH FROM (dateto - datefrom)) > 0
            THEN ROUND(COUNT(*)::NUMERIC /
                 (EXTRACT(EPOCH FROM (dateto - datefrom)) / EXTRACT(EPOCH FROM v_period)), 2)
            ELSE 0
        END::NUMERIC(18,2) AS "AverageBookingsPerPeriod"
    FROM public."tblBooking" b
    WHERE NOT b."IsDeleted"
      AND b."BookingDateUtc" >= datefrom
      AND b."BookingDateUtc" <  dateto
      AND (serviceid = 0 OR b."SlotAvailabilityId" IN (
              SELECT sa."SlotAvailabilityId" FROM public."tblSlotAvailability" sa
              WHERE sa."ServiceId" = serviceid))
      AND (status = 0 OR b."BookingStatus" = status);

    -- RS 2: Trends
    OPEN ref2 FOR
    SELECT
        DATE_TRUNC(CASE trendby WHEN 'week' THEN 'week' WHEN 'month' THEN 'month' ELSE 'day' END,
                   b."BookingDateUtc")::DATE AS "PeriodStartDate",
        CASE trendby
            WHEN 'week'  THEN TO_CHAR(DATE_TRUNC('week',  b."BookingDateUtc"), 'DD Mon')
            WHEN 'month' THEN TO_CHAR(DATE_TRUNC('month', b."BookingDateUtc"), 'Mon YYYY')
            ELSE              TO_CHAR(b."BookingDateUtc"::DATE, 'DD Mon')
        END AS "PeriodLabel",
        COUNT(*)::NUMERIC(18,2) AS "Value"
    FROM public."tblBooking" b
    WHERE NOT b."IsDeleted"
      AND b."BookingDateUtc" >= datefrom AND b."BookingDateUtc" < dateto
      AND (serviceid = 0 OR b."SlotAvailabilityId" IN (
              SELECT sa."SlotAvailabilityId" FROM public."tblSlotAvailability" sa
              WHERE sa."ServiceId" = serviceid))
      AND (status = 0 OR b."BookingStatus" = status)
    GROUP BY 1, 2 ORDER BY 1;

    -- RS 3: Status distribution
    OPEN ref3 FOR
    SELECT
        CASE b."BookingStatus"
            WHEN 1 THEN 'Pending' WHEN 2 THEN 'Confirmed'
            WHEN 3 THEN 'Cancelled' ELSE 'Unknown'
        END AS "Label",
        COUNT(*)::NUMERIC(18,2) AS "Value"
    FROM public."tblBooking" b
    WHERE NOT b."IsDeleted"
      AND b."BookingDateUtc" >= datefrom AND b."BookingDateUtc" < dateto
    GROUP BY b."BookingStatus" ORDER BY b."BookingStatus";

    -- RS 4: Service distribution
    OPEN ref4 FOR
    SELECT
        COALESCE(b."ServiceNameSnapshot", 'Unknown') AS "Label",
        COUNT(*)::NUMERIC(18,2) AS "Value"
    FROM public."tblBooking" b
    WHERE NOT b."IsDeleted"
      AND b."BookingDateUtc" >= datefrom AND b."BookingDateUtc" < dateto
    GROUP BY b."ServiceNameSnapshot"
    ORDER BY COUNT(*) DESC;
END;
$$;


-- -----------------------------------------------------------
-- 4.2  dbo.uspgetrevenueanalytics
--      C# call: + @DateFrom, @DateTo, @TrendBy, @ServiceId
--               + ref1, ref2, ref3, ref4
--      RS 1: TotalRevenue, PaidRevenue, OutstandingRevenue,
--            InvoiceCount, AverageInvoiceValue
--      RS 2: PeriodStartDate, PeriodLabel, Value
--      RS 3: Label, Value (by service)
--      RS 4: Label, Value (AMC vs Standard)
-- -----------------------------------------------------------
CREATE OR REPLACE PROCEDURE dbo.uspgetrevenueanalytics(
    IN  datefrom  TIMESTAMP WITHOUT TIME ZONE,
    IN  dateto    TIMESTAMP WITHOUT TIME ZONE,
    IN  trendby   TEXT,
    IN  serviceid BIGINT,
    INOUT ref1    REFCURSOR,
    INOUT ref2    REFCURSOR,
    INOUT ref3    REFCURSOR,
    INOUT ref4    REFCURSOR
)
LANGUAGE plpgsql AS $$
BEGIN
    -- RS 1: Summary
    OPEN ref1 FOR
    SELECT
        COALESCE(SUM(ih."GrandTotalAmount"), 0)::NUMERIC(18,2) AS "TotalRevenue",
        COALESCE(SUM(ih."PaidAmount"),       0)::NUMERIC(18,2) AS "PaidRevenue",
        COALESCE(SUM(ih."BalanceAmount"),     0)::NUMERIC(18,2) AS "OutstandingRevenue",
        COUNT(*)::BIGINT AS "InvoiceCount",
        CASE WHEN COUNT(*) > 0
             THEN ROUND(SUM(ih."GrandTotalAmount") / COUNT(*), 2)
             ELSE 0 END::NUMERIC(18,2) AS "AverageInvoiceValue"
    FROM public."tblInvoiceHeader" ih
    WHERE NOT ih."IsDeleted"
      AND ih."InvoiceDateUtc" >= datefrom AND ih."InvoiceDateUtc" < dateto
      AND (serviceid = 0 OR EXISTS (
              SELECT 1 FROM public."tblBooking" b
              JOIN public."tblServiceRequest"  sr ON b."BookingId"          = sr."BookingId"
              JOIN public."tblJobCard"          jc ON sr."ServiceRequestId"  = jc."ServiceRequestId"
              JOIN public."tblQuotationHeader"  qh ON jc."JobCardId"         = qh."JobCardId"
              WHERE qh."QuotationHeaderId" = ih."QuotationHeaderId"
                AND b."SlotAvailabilityId" IN (
                    SELECT sa."SlotAvailabilityId" FROM public."tblSlotAvailability" sa
                    WHERE sa."ServiceId" = serviceid)));

    -- RS 2: Trends
    OPEN ref2 FOR
    SELECT
        DATE_TRUNC(CASE trendby WHEN 'week' THEN 'week' WHEN 'month' THEN 'month' ELSE 'day' END,
                   ih."InvoiceDateUtc")::DATE AS "PeriodStartDate",
        CASE trendby
            WHEN 'week'  THEN TO_CHAR(DATE_TRUNC('week',  ih."InvoiceDateUtc"), 'DD Mon')
            WHEN 'month' THEN TO_CHAR(DATE_TRUNC('month', ih."InvoiceDateUtc"), 'Mon YYYY')
            ELSE              TO_CHAR(ih."InvoiceDateUtc"::DATE, 'DD Mon')
        END AS "PeriodLabel",
        COALESCE(SUM(ih."GrandTotalAmount"), 0)::NUMERIC(18,2) AS "Value"
    FROM public."tblInvoiceHeader" ih
    WHERE NOT ih."IsDeleted"
      AND ih."InvoiceDateUtc" >= datefrom AND ih."InvoiceDateUtc" < dateto
    GROUP BY 1, 2 ORDER BY 1;

    -- RS 3: By service
    OPEN ref3 FOR
    SELECT
        COALESCE(b."ServiceNameSnapshot", 'Unknown') AS "Label",
        COALESCE(SUM(ih."GrandTotalAmount"), 0)::NUMERIC(18,2) AS "Value"
    FROM public."tblInvoiceHeader" ih
    JOIN public."tblQuotationHeader" qh ON ih."QuotationHeaderId" = qh."QuotationHeaderId"
    JOIN public."tblJobCard"         jc ON qh."JobCardId"         = jc."JobCardId"
    JOIN public."tblServiceRequest"  sr ON jc."ServiceRequestId"  = sr."ServiceRequestId"
    JOIN public."tblBooking"         b  ON sr."BookingId"         = b."BookingId"
    WHERE NOT ih."IsDeleted"
      AND ih."InvoiceDateUtc" >= datefrom AND ih."InvoiceDateUtc" < dateto
    GROUP BY b."ServiceNameSnapshot"
    ORDER BY SUM(ih."GrandTotalAmount") DESC;

    -- RS 4: AMC vs Standard
    OPEN ref4 FOR
    SELECT
        CASE WHEN EXISTS (
                SELECT 1 FROM public."tblCustomerAMC" amc
                WHERE amc."CustomerId" = ih."CustomerId"
                  AND NOT amc."IsDeleted" AND amc."CurrentStatus" = 1)
             THEN 'AMC Customer' ELSE 'Standard Customer'
        END AS "Label",
        COALESCE(SUM(ih."GrandTotalAmount"), 0)::NUMERIC(18,2) AS "Value"
    FROM public."tblInvoiceHeader" ih
    WHERE NOT ih."IsDeleted"
      AND ih."InvoiceDateUtc" >= datefrom AND ih."InvoiceDateUtc" < dateto
    GROUP BY 1 ORDER BY 2 DESC;
END;
$$;


-- -----------------------------------------------------------
-- 4.3  dbo.uspgettechnicianperformance
--      C# call: + @DateFrom, @DateTo, @TrendBy, @TechnicianId, @Status
--               + ref1, ref2
--      RS 1: TotalTechnicians, ActiveTechnicians, TotalAssignedJobs,
--            TotalCompletedJobs, AverageCompletionHours
--      RS 2: per-technician row
-- -----------------------------------------------------------
CREATE OR REPLACE PROCEDURE dbo.uspgettechnicianperformance(
    IN  datefrom     TIMESTAMP WITHOUT TIME ZONE,
    IN  dateto       TIMESTAMP WITHOUT TIME ZONE,
    IN  trendby      TEXT,
    IN  technicianid BIGINT,
    IN  status       INTEGER,
    INOUT ref1       REFCURSOR,
    INOUT ref2       REFCURSOR
)
LANGUAGE plpgsql AS $$
BEGIN
    -- RS 1: Summary
    OPEN ref1 FOR
    WITH asgn AS (
        SELECT sra."TechnicianId",
               COUNT(*) AS jobs_assigned,
               SUM(CASE WHEN jc."WorkCompletedDateUtc" IS NOT NULL THEN 1 ELSE 0 END) AS jobs_completed,
               AVG(CASE WHEN jc."WorkCompletedDateUtc" IS NOT NULL
                             AND jc."WorkStartedDateUtc" IS NOT NULL
                        THEN EXTRACT(EPOCH FROM (jc."WorkCompletedDateUtc" - jc."WorkStartedDateUtc")) / 3600.0
                   END) AS avg_hours
        FROM public."tblServiceRequestAssignment" sra
        JOIN public."tblServiceRequest" sr ON sra."ServiceRequestId" = sr."ServiceRequestId"
        LEFT JOIN public."tblJobCard" jc    ON sr."ServiceRequestId"  = jc."ServiceRequestId"
        WHERE NOT sra."IsDeleted"
          AND sra."AssignedDateUtc" >= datefrom AND sra."AssignedDateUtc" < dateto
          AND (technicianid = 0 OR sra."TechnicianId" = technicianid)
          AND (status = 0 OR sr."CurrentStatus" = status)
        GROUP BY sra."TechnicianId"
    )
    SELECT
        COUNT(DISTINCT t."TechnicianId")::BIGINT AS "TotalTechnicians",
        SUM(CASE WHEN t."IsActive" THEN 1 ELSE 0 END)::BIGINT AS "ActiveTechnicians",
        COALESCE(SUM(a.jobs_assigned),  0)::BIGINT AS "TotalAssignedJobs",
        COALESCE(SUM(a.jobs_completed), 0)::BIGINT AS "TotalCompletedJobs",
        COALESCE(ROUND(AVG(a.avg_hours)::NUMERIC, 2), 0)::NUMERIC(18,2) AS "AverageCompletionHours"
    FROM public."tblTechnician" t
    LEFT JOIN asgn a ON t."TechnicianId" = a."TechnicianId"
    WHERE NOT t."IsDeleted"
      AND (technicianid = 0 OR t."TechnicianId" = technicianid);

    -- RS 2: Per-technician
    OPEN ref2 FOR
    SELECT
        t."TechnicianId",
        t."TechnicianCode",
        t."TechnicianName",
        COUNT(sra."ServiceRequestAssignmentId")::BIGINT AS "JobsAssigned",
        SUM(CASE WHEN jc."WorkCompletedDateUtc" IS NOT NULL THEN 1 ELSE 0 END)::BIGINT AS "JobsCompleted",
        CASE WHEN COUNT(sra."ServiceRequestAssignmentId") > 0
             THEN ROUND(SUM(CASE WHEN jc."WorkCompletedDateUtc" IS NOT NULL THEN 1.0 ELSE 0 END)
                  / COUNT(sra."ServiceRequestAssignmentId") * 100, 2)
             ELSE 0 END::NUMERIC(18,2) AS "CompletionRatePercentage",
        COALESCE(ROUND(AVG(
            CASE WHEN jc."WorkCompletedDateUtc" IS NOT NULL AND jc."WorkStartedDateUtc" IS NOT NULL
                 THEN EXTRACT(EPOCH FROM (jc."WorkCompletedDateUtc" - jc."WorkStartedDateUtc")) / 3600.0
            END)::NUMERIC, 2), 0)::NUMERIC(18,2) AS "AverageCompletionHours",
        (SELECT COUNT(*) FROM public."tblServiceRequestAssignment" cw
         WHERE cw."TechnicianId" = t."TechnicianId"
           AND cw."IsActiveAssignment" AND NOT cw."IsDeleted")::BIGINT AS "CurrentWorkload"
    FROM public."tblTechnician" t
    LEFT JOIN public."tblServiceRequestAssignment" sra
           ON t."TechnicianId" = sra."TechnicianId" AND NOT sra."IsDeleted"
          AND sra."AssignedDateUtc" >= datefrom AND sra."AssignedDateUtc" < dateto
    LEFT JOIN public."tblServiceRequest" sr ON sra."ServiceRequestId" = sr."ServiceRequestId"
    LEFT JOIN public."tblJobCard"         jc ON sr."ServiceRequestId"  = jc."ServiceRequestId"
    WHERE NOT t."IsDeleted"
      AND (technicianid = 0 OR t."TechnicianId" = technicianid)
      AND (status = 0 OR sr."CurrentStatus" = status OR sr."CurrentStatus" IS NULL)
    GROUP BY t."TechnicianId", t."TechnicianCode", t."TechnicianName"
    ORDER BY "JobsCompleted" DESC;
END;
$$;


-- -----------------------------------------------------------
-- 4.4  dbo.uspgetcustomeranalytics
--      C# call: + @DateFrom, @DateTo, @TrendBy + ref1, ref2, ref3
--      RS 1: TotalCustomers, NewCustomers, ReturningCustomers,
--            RepeatCustomers, AmcCustomers, NonAmcCustomers,
--            RepeatRatePercentage
--      RS 2: Label, Value (segment)
--      RS 3: PeriodStartDate, PeriodLabel, NewCustomers, ReturningCustomers
-- -----------------------------------------------------------
CREATE OR REPLACE PROCEDURE dbo.uspgetcustomeranalytics(
    IN  datefrom  TIMESTAMP WITHOUT TIME ZONE,
    IN  dateto    TIMESTAMP WITHOUT TIME ZONE,
    IN  trendby   TEXT,
    INOUT ref1    REFCURSOR,
    INOUT ref2    REFCURSOR,
    INOUT ref3    REFCURSOR
)
LANGUAGE plpgsql AS $$
BEGIN
    -- RS 1: Summary
    OPEN ref1 FOR
    WITH cust_stats AS (
        SELECT c."CustomerId", c."DateCreated",
               COUNT(b."BookingId") AS booking_count,
               EXISTS (SELECT 1 FROM public."tblCustomerAMC" amc
                       WHERE amc."CustomerId" = c."CustomerId"
                         AND NOT amc."IsDeleted" AND amc."CurrentStatus" = 1) AS is_amc
        FROM public."tblCustomer" c
        LEFT JOIN public."tblBooking" b ON b."CustomerId" = c."CustomerId" AND NOT b."IsDeleted"
        WHERE NOT c."IsDeleted"
        GROUP BY c."CustomerId", c."DateCreated"
    )
    SELECT
        COUNT(*)::BIGINT AS "TotalCustomers",
        SUM(CASE WHEN cs."DateCreated" >= datefrom AND cs."DateCreated" < dateto THEN 1 ELSE 0 END)::BIGINT AS "NewCustomers",
        SUM(CASE WHEN cs."DateCreated" <  datefrom AND cs.booking_count > 0     THEN 1 ELSE 0 END)::BIGINT AS "ReturningCustomers",
        SUM(CASE WHEN cs.booking_count > 1 THEN 1 ELSE 0 END)::BIGINT AS "RepeatCustomers",
        SUM(CASE WHEN     cs.is_amc THEN 1 ELSE 0 END)::BIGINT AS "AmcCustomers",
        SUM(CASE WHEN NOT cs.is_amc THEN 1 ELSE 0 END)::BIGINT AS "NonAmcCustomers",
        CASE WHEN COUNT(*) > 0
             THEN ROUND(SUM(CASE WHEN cs.booking_count > 1 THEN 1.0 ELSE 0 END) / COUNT(*) * 100, 2)
             ELSE 0 END::NUMERIC(18,2) AS "RepeatRatePercentage"
    FROM cust_stats cs;

    -- RS 2: Segment distribution
    OPEN ref2 FOR
    SELECT
        CASE WHEN EXISTS (SELECT 1 FROM public."tblCustomerAMC" amc
                          WHERE amc."CustomerId" = c."CustomerId"
                            AND NOT amc."IsDeleted" AND amc."CurrentStatus" = 1)
             THEN 'AMC' ELSE 'Standard' END AS "Label",
        COUNT(*)::NUMERIC(18,2) AS "Value"
    FROM public."tblCustomer" c
    WHERE NOT c."IsDeleted"
    GROUP BY 1 ORDER BY 2 DESC;

    -- RS 3: Trends
    OPEN ref3 FOR
    SELECT
        DATE_TRUNC(CASE trendby WHEN 'week' THEN 'week' WHEN 'month' THEN 'month' ELSE 'day' END,
                   c."DateCreated")::DATE AS "PeriodStartDate",
        CASE trendby
            WHEN 'week'  THEN TO_CHAR(DATE_TRUNC('week',  c."DateCreated"), 'DD Mon')
            WHEN 'month' THEN TO_CHAR(DATE_TRUNC('month', c."DateCreated"), 'Mon YYYY')
            ELSE              TO_CHAR(c."DateCreated"::DATE, 'DD Mon')
        END AS "PeriodLabel",
        COUNT(CASE WHEN c."DateCreated" >= datefrom AND c."DateCreated" < dateto THEN 1 END)::BIGINT AS "NewCustomers",
        COUNT(CASE WHEN c."DateCreated" < datefrom AND EXISTS (
                       SELECT 1 FROM public."tblBooking" b
                       WHERE b."CustomerId" = c."CustomerId" AND NOT b."IsDeleted"
                         AND b."BookingDateUtc" >= datefrom AND b."BookingDateUtc" < dateto)
               THEN 1 END)::BIGINT AS "ReturningCustomers"
    FROM public."tblCustomer" c
    WHERE NOT c."IsDeleted"
      AND c."DateCreated" >= datefrom AND c."DateCreated" < dateto
    GROUP BY 1, 2 ORDER BY 1;
END;
$$;


-- -----------------------------------------------------------
-- 4.5  dbo.uspgetsupportanalytics
--      C# call: + @DateFrom, @DateTo, @TrendBy, @Status
--               + ref1, ref2, ref3
--      RS 1: TotalTickets, OpenTickets, ResolvedTickets,
--            EscalationCount, AverageResolutionHours
--      RS 2: Label, Value (status distribution)
--      RS 3: PeriodStartDate, PeriodLabel, ResolvedTickets, AverageResolutionHours
-- -----------------------------------------------------------
CREATE OR REPLACE PROCEDURE dbo.uspgetsupportanalytics(
    IN  datefrom  TIMESTAMP WITHOUT TIME ZONE,
    IN  dateto    TIMESTAMP WITHOUT TIME ZONE,
    IN  trendby   TEXT,
    IN  status    INTEGER,
    INOUT ref1    REFCURSOR,
    INOUT ref2    REFCURSOR,
    INOUT ref3    REFCURSOR
)
LANGUAGE plpgsql AS $$
BEGIN
    -- RS 1: Summary
    OPEN ref1 FOR
    WITH resolution AS (
        SELECT sh."SupportTicketId", MIN(sh."StatusDateUtc") AS resolved_at
        FROM public."tblSupportTicketStatusHistory" sh
        WHERE sh."SupportTicketStatus" = 6 AND NOT sh."IsDeleted"
        GROUP BY sh."SupportTicketId"
    )
    SELECT
        COUNT(*)::BIGINT AS "TotalTickets",
        SUM(CASE WHEN st."CurrentStatus" IN (1,2,3,4,8) THEN 1 ELSE 0 END)::BIGINT AS "OpenTickets",
        SUM(CASE WHEN st."CurrentStatus" IN (6,7)        THEN 1 ELSE 0 END)::BIGINT AS "ResolvedTickets",
        SUM(CASE WHEN st."CurrentStatus" = 5             THEN 1 ELSE 0 END)::BIGINT AS "EscalationCount",
        COALESCE(ROUND(AVG(CASE WHEN r.resolved_at IS NOT NULL
            THEN EXTRACT(EPOCH FROM (r.resolved_at - st."DateCreated")) / 3600.0 END
        )::NUMERIC, 2), 0)::NUMERIC(18,2) AS "AverageResolutionHours"
    FROM public."tblSupportTicket" st
    LEFT JOIN resolution r ON st."SupportTicketId" = r."SupportTicketId"
    WHERE NOT st."IsDeleted"
      AND st."DateCreated" >= datefrom AND st."DateCreated" < dateto
      AND (status = 0 OR st."CurrentStatus" = status);

    -- RS 2: Status distribution
    OPEN ref2 FOR
    SELECT
        CASE st."CurrentStatus"
            WHEN 1 THEN 'Open'              WHEN 2 THEN 'In Progress'
            WHEN 3 THEN 'Waiting For Customer' WHEN 4 THEN 'Customer Responded'
            WHEN 5 THEN 'Escalated'         WHEN 6 THEN 'Resolved'
            WHEN 7 THEN 'Closed'            WHEN 8 THEN 'Reopened'
            ELSE 'Unknown'
        END AS "Label",
        COUNT(*)::NUMERIC(18,2) AS "Value"
    FROM public."tblSupportTicket" st
    WHERE NOT st."IsDeleted"
      AND st."DateCreated" >= datefrom AND st."DateCreated" < dateto
    GROUP BY st."CurrentStatus" ORDER BY st."CurrentStatus";

    -- RS 3: Resolution trends
    OPEN ref3 FOR
    WITH resolution AS (
        SELECT sh."SupportTicketId", MIN(sh."StatusDateUtc") AS resolved_at
        FROM public."tblSupportTicketStatusHistory" sh
        WHERE sh."SupportTicketStatus" = 6 AND NOT sh."IsDeleted"
        GROUP BY sh."SupportTicketId"
    )
    SELECT
        DATE_TRUNC(CASE trendby WHEN 'week' THEN 'week' WHEN 'month' THEN 'month' ELSE 'day' END,
                   st."DateCreated")::DATE AS "PeriodStartDate",
        CASE trendby
            WHEN 'week'  THEN TO_CHAR(DATE_TRUNC('week',  st."DateCreated"), 'DD Mon')
            WHEN 'month' THEN TO_CHAR(DATE_TRUNC('month', st."DateCreated"), 'Mon YYYY')
            ELSE              TO_CHAR(st."DateCreated"::DATE, 'DD Mon')
        END AS "PeriodLabel",
        SUM(CASE WHEN st."CurrentStatus" IN (6,7) THEN 1 ELSE 0 END)::BIGINT AS "ResolvedTickets",
        COALESCE(ROUND(AVG(CASE WHEN r.resolved_at IS NOT NULL
            THEN EXTRACT(EPOCH FROM (r.resolved_at - st."DateCreated")) / 3600.0 END
        )::NUMERIC, 2), 0)::NUMERIC(18,2) AS "AverageResolutionHours"
    FROM public."tblSupportTicket" st
    LEFT JOIN resolution r ON st."SupportTicketId" = r."SupportTicketId"
    WHERE NOT st."IsDeleted"
      AND st."DateCreated" >= datefrom AND st."DateCreated" < dateto
    GROUP BY 1, 2 ORDER BY 1;
END;
$$;


-- -----------------------------------------------------------
-- 4.6  dbo.uspgetinventoryanalytics
--      C# call: + @DateFrom, @DateTo, @TrendBy + ref1, ref2, ref3
--      RS 1: TotalItems, LowStockItems, TotalOnHandQuantity, ConsumedQuantity
--      RS 2: ItemId, ItemCode, ItemName, QuantityOnHand, ReorderLevel, ShortageQuantity
--      RS 3: PeriodStartDate, PeriodLabel, QuantityConsumed
-- -----------------------------------------------------------
CREATE OR REPLACE PROCEDURE dbo.uspgetinventoryanalytics(
    IN  datefrom  TIMESTAMP WITHOUT TIME ZONE,
    IN  dateto    TIMESTAMP WITHOUT TIME ZONE,
    IN  trendby   TEXT,
    INOUT ref1    REFCURSOR,
    INOUT ref2    REFCURSOR,
    INOUT ref3    REFCURSOR
)
LANGUAGE plpgsql AS $$
BEGIN
    -- RS 1: Summary
    OPEN ref1 FOR
    SELECT
        (SELECT COUNT(DISTINCT i."ItemId") FROM public."tblItem" i WHERE NOT i."IsDeleted")::BIGINT AS "TotalItems",
        (SELECT COUNT(DISTINCT ws."ItemId")
         FROM public."tblWarehouseStock" ws JOIN public."tblItem" i ON ws."ItemId" = i."ItemId"
         WHERE NOT ws."IsDeleted" AND NOT i."IsDeleted"
           AND ws."QuantityOnHand" <= i."ReorderLevel")::BIGINT AS "LowStockItems",
        COALESCE((SELECT SUM(ws2."QuantityOnHand") FROM public."tblWarehouseStock" ws2
                  WHERE NOT ws2."IsDeleted"), 0)::NUMERIC(18,2) AS "TotalOnHandQuantity",
        COALESCE((SELECT SUM(st."Quantity") FROM public."tblStockTransaction" st
                  WHERE NOT st."IsDeleted" AND st."TransactionType" = 6
                    AND st."TransactionDateUtc" >= datefrom
                    AND st."TransactionDateUtc" <  dateto), 0)::NUMERIC(18,2) AS "ConsumedQuantity";

    -- RS 2: Low-stock items
    OPEN ref2 FOR
    SELECT
        i."ItemId", i."ItemCode", i."ItemName",
        COALESCE(SUM(ws."QuantityOnHand"), 0)::NUMERIC(18,2) AS "QuantityOnHand",
        i."ReorderLevel"::NUMERIC(18,2) AS "ReorderLevel",
        GREATEST(0, i."ReorderLevel" - COALESCE(SUM(ws."QuantityOnHand"), 0))::NUMERIC(18,2) AS "ShortageQuantity"
    FROM public."tblItem" i
    LEFT JOIN public."tblWarehouseStock" ws ON ws."ItemId" = i."ItemId" AND NOT ws."IsDeleted"
    WHERE NOT i."IsDeleted"
    GROUP BY i."ItemId", i."ItemCode", i."ItemName", i."ReorderLevel"
    HAVING COALESCE(SUM(ws."QuantityOnHand"), 0) <= i."ReorderLevel"
    ORDER BY "ShortageQuantity" DESC;

    -- RS 3: Consumption trends
    OPEN ref3 FOR
    SELECT
        DATE_TRUNC(CASE trendby WHEN 'week' THEN 'week' WHEN 'month' THEN 'month' ELSE 'day' END,
                   st."TransactionDateUtc")::DATE AS "PeriodStartDate",
        CASE trendby
            WHEN 'week'  THEN TO_CHAR(DATE_TRUNC('week',  st."TransactionDateUtc"), 'DD Mon')
            WHEN 'month' THEN TO_CHAR(DATE_TRUNC('month', st."TransactionDateUtc"), 'Mon YYYY')
            ELSE              TO_CHAR(st."TransactionDateUtc"::DATE, 'DD Mon')
        END AS "PeriodLabel",
        COALESCE(SUM(st."Quantity"), 0)::NUMERIC(18,2) AS "QuantityConsumed"
    FROM public."tblStockTransaction" st
    WHERE NOT st."IsDeleted" AND st."TransactionType" = 6
      AND st."TransactionDateUtc" >= datefrom AND st."TransactionDateUtc" < dateto
    GROUP BY 1, 2 ORDER BY 1;
END;
$$;


-- -----------------------------------------------------------
-- 4.7  dbo.uspgetreportbydaterange
--      C# call: + @DateFrom, @DateTo, @TrendBy + ref1, ref2, ref3, ref4
--      RS 1: TotalBookings, TotalRevenue, CompletedJobs,
--            TotalSupportTickets, ActiveTechnicians, NewCustomers
--      RS 2: booking trends
--      RS 3: revenue trends
--      RS 4: support status distribution
-- -----------------------------------------------------------
CREATE OR REPLACE PROCEDURE dbo.uspgetreportbydaterange(
    IN  datefrom  TIMESTAMP WITHOUT TIME ZONE,
    IN  dateto    TIMESTAMP WITHOUT TIME ZONE,
    IN  trendby   TEXT,
    INOUT ref1    REFCURSOR,
    INOUT ref2    REFCURSOR,
    INOUT ref3    REFCURSOR,
    INOUT ref4    REFCURSOR
)
LANGUAGE plpgsql AS $$
BEGIN
    -- RS 1: Summary
    OPEN ref1 FOR
    SELECT
        (SELECT COUNT(*) FROM public."tblBooking"       b  WHERE NOT b."IsDeleted"  AND b."BookingDateUtc" >= datefrom  AND b."BookingDateUtc" < dateto)::BIGINT AS "TotalBookings",
        COALESCE((SELECT SUM(ih."GrandTotalAmount") FROM public."tblInvoiceHeader" ih WHERE NOT ih."IsDeleted" AND ih."InvoiceDateUtc" >= datefrom AND ih."InvoiceDateUtc" < dateto), 0)::NUMERIC(18,2) AS "TotalRevenue",
        (SELECT COUNT(*) FROM public."tblJobCard"       jc WHERE NOT jc."IsDeleted" AND jc."WorkCompletedDateUtc" IS NOT NULL AND jc."WorkCompletedDateUtc" >= datefrom AND jc."WorkCompletedDateUtc" < dateto)::BIGINT AS "CompletedJobs",
        (SELECT COUNT(*) FROM public."tblSupportTicket" st WHERE NOT st."IsDeleted" AND st."DateCreated" >= datefrom AND st."DateCreated" < dateto)::BIGINT AS "TotalSupportTickets",
        (SELECT COUNT(*) FROM public."tblTechnician"    t  WHERE NOT t."IsDeleted"  AND t."IsActive")::BIGINT AS "ActiveTechnicians",
        (SELECT COUNT(*) FROM public."tblCustomer"      c  WHERE NOT c."IsDeleted"  AND c."DateCreated" >= datefrom AND c."DateCreated" < dateto)::BIGINT AS "NewCustomers";

    -- RS 2: Booking trends
    OPEN ref2 FOR
    SELECT
        DATE_TRUNC(CASE trendby WHEN 'week' THEN 'week' WHEN 'month' THEN 'month' ELSE 'day' END,
                   b."BookingDateUtc")::DATE AS "PeriodStartDate",
        CASE trendby
            WHEN 'week'  THEN TO_CHAR(DATE_TRUNC('week',  b."BookingDateUtc"), 'DD Mon')
            WHEN 'month' THEN TO_CHAR(DATE_TRUNC('month', b."BookingDateUtc"), 'Mon YYYY')
            ELSE              TO_CHAR(b."BookingDateUtc"::DATE, 'DD Mon')
        END AS "PeriodLabel",
        COUNT(*)::NUMERIC(18,2) AS "Value"
    FROM public."tblBooking" b
    WHERE NOT b."IsDeleted"
      AND b."BookingDateUtc" >= datefrom AND b."BookingDateUtc" < dateto
    GROUP BY 1, 2 ORDER BY 1;

    -- RS 3: Revenue trends
    OPEN ref3 FOR
    SELECT
        DATE_TRUNC(CASE trendby WHEN 'week' THEN 'week' WHEN 'month' THEN 'month' ELSE 'day' END,
                   ih."InvoiceDateUtc")::DATE AS "PeriodStartDate",
        CASE trendby
            WHEN 'week'  THEN TO_CHAR(DATE_TRUNC('week',  ih."InvoiceDateUtc"), 'DD Mon')
            WHEN 'month' THEN TO_CHAR(DATE_TRUNC('month', ih."InvoiceDateUtc"), 'Mon YYYY')
            ELSE              TO_CHAR(ih."InvoiceDateUtc"::DATE, 'DD Mon')
        END AS "PeriodLabel",
        COALESCE(SUM(ih."GrandTotalAmount"), 0)::NUMERIC(18,2) AS "Value"
    FROM public."tblInvoiceHeader" ih
    WHERE NOT ih."IsDeleted"
      AND ih."InvoiceDateUtc" >= datefrom AND ih."InvoiceDateUtc" < dateto
    GROUP BY 1, 2 ORDER BY 1;

    -- RS 4: Support status distribution
    OPEN ref4 FOR
    SELECT
        CASE st."CurrentStatus"
            WHEN 1 THEN 'Open'              WHEN 2 THEN 'In Progress'
            WHEN 3 THEN 'Waiting For Customer' WHEN 4 THEN 'Customer Responded'
            WHEN 5 THEN 'Escalated'         WHEN 6 THEN 'Resolved'
            WHEN 7 THEN 'Closed'            WHEN 8 THEN 'Reopened'
            ELSE 'Unknown'
        END AS "Label",
        COUNT(*)::NUMERIC(18,2) AS "Value"
    FROM public."tblSupportTicket" st
    WHERE NOT st."IsDeleted"
      AND st."DateCreated" >= datefrom AND st."DateCreated" < dateto
    GROUP BY st."CurrentStatus" ORDER BY st."CurrentStatus";
END;
$$;


-- =============================================================
-- STEP 5: Sequence reset — fix duplicate key errors on all tables
-- =============================================================
DO $$
DECLARE
    rec      RECORD;
    seq_name TEXT;
    max_id   BIGINT;
BEGIN
    FOR rec IN
        SELECT t.table_name, c.column_name
        FROM information_schema.tables t
        JOIN information_schema.columns c
          ON c.table_name = t.table_name AND c.table_schema = t.table_schema
        WHERE t.table_schema = 'public'
          AND t.table_type   = 'BASE TABLE'
          AND c.column_default LIKE 'nextval%'
    LOOP
        seq_name := pg_get_serial_sequence(
            'public.' || quote_ident(rec.table_name), rec.column_name);
        IF seq_name IS NOT NULL THEN
            EXECUTE format('SELECT COALESCE(MAX(%I), 0) FROM public.%I',
                rec.column_name, rec.table_name) INTO max_id;
            PERFORM setval(seq_name, GREATEST(max_id, 1), true);
        END IF;
    END LOOP;
    RAISE NOTICE 'Step 5 complete: all sequences reset.';
END $$;


-- =============================================================
-- STEP 6: Grant permissions
-- =============================================================
GRANT USAGE ON SCHEMA dbo TO PUBLIC;

GRANT EXECUTE ON PROCEDURE dbo.uspgetbookinganalytics(
    TIMESTAMP WITHOUT TIME ZONE, TIMESTAMP WITHOUT TIME ZONE,
    TEXT, BIGINT, INTEGER, REFCURSOR, REFCURSOR, REFCURSOR, REFCURSOR) TO PUBLIC;

GRANT EXECUTE ON PROCEDURE dbo.uspgetrevenueanalytics(
    TIMESTAMP WITHOUT TIME ZONE, TIMESTAMP WITHOUT TIME ZONE,
    TEXT, BIGINT, REFCURSOR, REFCURSOR, REFCURSOR, REFCURSOR) TO PUBLIC;

GRANT EXECUTE ON PROCEDURE dbo.uspgettechnicianperformance(
    TIMESTAMP WITHOUT TIME ZONE, TIMESTAMP WITHOUT TIME ZONE,
    TEXT, BIGINT, INTEGER, REFCURSOR, REFCURSOR) TO PUBLIC;

GRANT EXECUTE ON PROCEDURE dbo.uspgetcustomeranalytics(
    TIMESTAMP WITHOUT TIME ZONE, TIMESTAMP WITHOUT TIME ZONE,
    TEXT, REFCURSOR, REFCURSOR, REFCURSOR) TO PUBLIC;

GRANT EXECUTE ON PROCEDURE dbo.uspgetsupportanalytics(
    TIMESTAMP WITHOUT TIME ZONE, TIMESTAMP WITHOUT TIME ZONE,
    TEXT, INTEGER, REFCURSOR, REFCURSOR, REFCURSOR) TO PUBLIC;

GRANT EXECUTE ON PROCEDURE dbo.uspgetinventoryanalytics(
    TIMESTAMP WITHOUT TIME ZONE, TIMESTAMP WITHOUT TIME ZONE,
    TEXT, REFCURSOR, REFCURSOR, REFCURSOR) TO PUBLIC;

GRANT EXECUTE ON PROCEDURE dbo.uspgetreportbydaterange(
    TIMESTAMP WITHOUT TIME ZONE, TIMESTAMP WITHOUT TIME ZONE,
    TEXT, REFCURSOR, REFCURSOR, REFCURSOR, REFCURSOR) TO PUBLIC;


-- =============================================================
-- STEP 7: Verification
-- =============================================================
DO $$
DECLARE
    proc_count INT;
    func_count INT;
BEGIN
    SELECT COUNT(*) INTO proc_count
    FROM information_schema.routines
    WHERE routine_schema = 'dbo' AND routine_type = 'PROCEDURE'
      AND routine_name IN (
          'uspgetbookinganalytics','uspgetrevenueanalytics',
          'uspgettechnicianperformance','uspgetcustomeranalytics',
          'uspgetsupportanalytics','uspgetinventoryanalytics',
          'uspgetreportbydaterange');

    SELECT COUNT(*) INTO func_count
    FROM information_schema.routines
    WHERE routine_schema = 'public' AND routine_type = 'FUNCTION'
      AND routine_name = 'uspgetdashboardsummary';

    RAISE NOTICE '=== 11_final_analytics_fix.sql complete ===';
    RAISE NOTICE 'dbo analytics procedures: % / 7', proc_count;
    RAISE NOTICE 'public.uspgetdashboardsummary function: %', func_count;

    IF proc_count < 7 THEN
        RAISE WARNING 'MISSING PROCEDURES — check errors above!';
    ELSE
        RAISE NOTICE 'All procedures OK.';
    END IF;
END $$;
