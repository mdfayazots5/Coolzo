-- =============================================================
-- Coolzo PostgreSQL Fix Script
-- File: 09_fix_money_and_analytics_procedures.sql
-- Date: 2026-05-12
--
-- Fixes three errors reported in postgres_sql.txt:
--
--   ERROR 1: 42846 - COALESCE could not convert type money to numeric
--   ERROR 2: 42883 - operator does not exist: numeric > money
--   ROOT CAUSE: EF Core HasColumnType("money") causes Npgsql to emit
--               0.0::money literals, but DB columns are NUMERIC(18,2).
--               PostgreSQL cannot implicitly cast between money and numeric.
--   FIX (SQL):  Ensure all monetary columns are NUMERIC(18,2) in the DB.
--   FIX (C#):   Change HasColumnType("money") to HasColumnType("numeric(18,2)")
--               in all EF Core configuration files (see companion note below).
--
--   ERROR 3: 42883 - procedure dbo.uspgetsupportanalytics does not exist
--   ROOT CAUSE: SQL Server analytics stored procedures were never ported to PG.
--   FIX:        Create dbo schema + all 7 missing analytics procedures using
--               PostgreSQL INOUT REFCURSOR pattern for multiple result sets.
-- =============================================================


-- =============================================================
-- SECTION 1: CREATE dbo SCHEMA (mirrors SQL Server dbo schema)
-- =============================================================
CREATE SCHEMA IF NOT EXISTS dbo;


-- =============================================================
-- SECTION 2: DEFENSIVE FIX - Ensure monetary columns are NUMERIC(18,2)
-- If columns were accidentally created as money type instead of NUMERIC,
-- these ALTER statements convert them. Safe to run even if already NUMERIC.
-- =============================================================

DO $$
DECLARE
    v_tables TEXT[] := ARRAY[
        'tblBooking',
        'tblBookingLine',
        'tblInvoiceHeader',
        'tblInvoiceLine',
        'tblQuotationHeader',
        'tblQuotationLine',
        'tblPaymentReceipt',
        'tblPaymentTransaction',
        'tblPurchaseOrder',
        'tblPurchaseOrderItem',
        'tblItemRate',
        'tblJobPartConsumption',
        'tblStockTransaction',
        'tblTechnicianEarnings',
        'tblService',
        'tblAmcPlan',
        'tblCustomerAMC',
        'tblPricingModel',
        'tblRevisitRequest',
        'tblWarranty',
        'tblInstallationQuotation',
        'tblInstallationInvoice',
        'tblInstallationInvoiceLine'
    ];
    v_table TEXT;
    v_col RECORD;
BEGIN
    FOREACH v_table IN ARRAY v_tables LOOP
        FOR v_col IN
            SELECT column_name
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name   = v_table
              AND data_type    = 'money'
        LOOP
            EXECUTE format(
                'ALTER TABLE public.%I ALTER COLUMN %I TYPE NUMERIC(18,2) USING %I::NUMERIC(18,2)',
                v_table, v_col.column_name, v_col.column_name
            );
            RAISE NOTICE 'Converted %.% from money to NUMERIC(18,2)', v_table, v_col.column_name;
        END LOOP;
    END LOOP;
END $$;


-- =============================================================
-- SECTION 3: ANALYTICS STORED PROCEDURES
--
-- Pattern: each procedure uses INOUT REFCURSOR parameters so that
-- Npgsql (called via CommandType.StoredProcedure) can fetch each
-- cursor as a separate result set. The C# AnalyticsReadRepository
-- must add the matching REFCURSOR parameters to the command before
-- executing (see AnalyticsReadRepository.cs update).
--
-- Cursor order matches the order C# reads result sets:
--   ref1 = first ReadAsync block (summary row)
--   ref2 = second NextResult block
--   ...
-- =============================================================


-- -------------------------------------------------------------
-- 3.1  uspGetBookingAnalytics
--      Input : DateFrom, DateTo, TrendBy, ServiceId, Status
--      RS 1  : summary   (TotalBookings, PendingBookings, ConfirmedBookings,
--                         CancelledBookings, AverageBookingsPerPeriod)
--      RS 2  : trends    (PeriodStartDate, PeriodLabel, Value)
--      RS 3  : status distribution  (Label, Value)
--      RS 4  : service distribution (Label, Value)
-- -------------------------------------------------------------
DROP PROCEDURE IF EXISTS dbo."uspGetBookingAnalytics"(
    TIMESTAMP WITHOUT TIME ZONE,
    TIMESTAMP WITHOUT TIME ZONE,
    TEXT, BIGINT, INTEGER,
    REFCURSOR, REFCURSOR, REFCURSOR, REFCURSOR);

CREATE OR REPLACE PROCEDURE dbo."uspGetBookingAnalytics"(
    IN  "DateFrom"  TIMESTAMP WITHOUT TIME ZONE,
    IN  "DateTo"    TIMESTAMP WITHOUT TIME ZONE,
    IN  "TrendBy"   TEXT,
    IN  "ServiceId" BIGINT,
    IN  "Status"    INTEGER,
    INOUT ref1      REFCURSOR,
    INOUT ref2      REFCURSOR,
    INOUT ref3      REFCURSOR,
    INOUT ref4      REFCURSOR
)
LANGUAGE plpgsql AS $$
DECLARE
    v_total     BIGINT;
    v_period    INTERVAL;
BEGIN
    -- Determine trend period length for AverageBookingsPerPeriod
    v_period := CASE "TrendBy"
        WHEN 'week'  THEN INTERVAL '7 days'
        WHEN 'month' THEN INTERVAL '30 days'
        ELSE              INTERVAL '1 day'
    END;

    SELECT COUNT(*) INTO v_total
    FROM public."tblBooking" b
    WHERE NOT b."IsDeleted"
      AND b."BookingDateUtc" >= "DateFrom"
      AND b."BookingDateUtc" <  "DateTo"
      AND ("ServiceId" = 0 OR b."SlotAvailabilityId" IN (
              SELECT sa."SlotAvailabilityId"
              FROM public."tblSlotAvailability" sa
              WHERE sa."ServiceId" = "ServiceId"))
      AND ("Status" = 0 OR b."BookingStatus" = "Status");

    -- RS 1: Summary
    OPEN ref1 FOR
    SELECT
        COUNT(*)::BIGINT                                                          AS "TotalBookings",
        SUM(CASE WHEN b."BookingStatus" = 1 THEN 1 ELSE 0 END)::BIGINT           AS "PendingBookings",
        SUM(CASE WHEN b."BookingStatus" = 2 THEN 1 ELSE 0 END)::BIGINT           AS "ConfirmedBookings",
        SUM(CASE WHEN b."BookingStatus" = 3 THEN 1 ELSE 0 END)::BIGINT           AS "CancelledBookings",
        CASE
            WHEN EXTRACT(EPOCH FROM ("DateTo" - "DateFrom")) / EXTRACT(EPOCH FROM v_period) > 0
            THEN ROUND(
                COUNT(*)::NUMERIC /
                NULLIF(EXTRACT(EPOCH FROM ("DateTo" - "DateFrom")) / EXTRACT(EPOCH FROM v_period), 0),
                2)
            ELSE 0
        END::NUMERIC(18,2)                                                        AS "AverageBookingsPerPeriod"
    FROM public."tblBooking" b
    WHERE NOT b."IsDeleted"
      AND b."BookingDateUtc" >= "DateFrom"
      AND b."BookingDateUtc" <  "DateTo"
      AND ("ServiceId" = 0 OR b."SlotAvailabilityId" IN (
              SELECT sa."SlotAvailabilityId"
              FROM public."tblSlotAvailability" sa
              WHERE sa."ServiceId" = "ServiceId"))
      AND ("Status" = 0 OR b."BookingStatus" = "Status");

    -- RS 2: Trends  (PeriodStartDate, PeriodLabel, Value)
    OPEN ref2 FOR
    SELECT
        DATE_TRUNC(
            CASE "TrendBy" WHEN 'week' THEN 'week' WHEN 'month' THEN 'month' ELSE 'day' END,
            b."BookingDateUtc"
        )::DATE                                                                   AS "PeriodStartDate",
        CASE "TrendBy"
            WHEN 'week'  THEN TO_CHAR(DATE_TRUNC('week',  b."BookingDateUtc"), 'DD Mon')
            WHEN 'month' THEN TO_CHAR(DATE_TRUNC('month', b."BookingDateUtc"), 'Mon YYYY')
            ELSE              TO_CHAR(b."BookingDateUtc"::DATE,                  'DD Mon')
        END                                                                       AS "PeriodLabel",
        COUNT(*)::NUMERIC(18,2)                                                   AS "Value"
    FROM public."tblBooking" b
    WHERE NOT b."IsDeleted"
      AND b."BookingDateUtc" >= "DateFrom"
      AND b."BookingDateUtc" <  "DateTo"
      AND ("ServiceId" = 0 OR b."SlotAvailabilityId" IN (
              SELECT sa."SlotAvailabilityId"
              FROM public."tblSlotAvailability" sa
              WHERE sa."ServiceId" = "ServiceId"))
      AND ("Status" = 0 OR b."BookingStatus" = "Status")
    GROUP BY 1, 2
    ORDER BY 1;

    -- RS 3: Status distribution (Label, Value)
    OPEN ref3 FOR
    SELECT
        CASE b."BookingStatus"
            WHEN 1 THEN 'Pending'
            WHEN 2 THEN 'Confirmed'
            WHEN 3 THEN 'Cancelled'
            ELSE        'Unknown'
        END                                                                       AS "Label",
        COUNT(*)::NUMERIC(18,2)                                                   AS "Value"
    FROM public."tblBooking" b
    WHERE NOT b."IsDeleted"
      AND b."BookingDateUtc" >= "DateFrom"
      AND b."BookingDateUtc" <  "DateTo"
    GROUP BY b."BookingStatus"
    ORDER BY b."BookingStatus";

    -- RS 4: Service distribution (Label, Value)
    OPEN ref4 FOR
    SELECT
        COALESCE(b."ServiceNameSnapshot", 'Unknown')                              AS "Label",
        COUNT(*)::NUMERIC(18,2)                                                   AS "Value"
    FROM public."tblBooking" b
    WHERE NOT b."IsDeleted"
      AND b."BookingDateUtc" >= "DateFrom"
      AND b."BookingDateUtc" <  "DateTo"
    GROUP BY b."ServiceNameSnapshot"
    ORDER BY COUNT(*) DESC;
END;
$$;


-- -------------------------------------------------------------
-- 3.2  uspGetRevenueAnalytics
--      Input : DateFrom, DateTo, TrendBy, ServiceId
--      RS 1  : summary   (TotalRevenue, PaidRevenue, OutstandingRevenue,
--                         InvoiceCount, AverageInvoiceValue)
--      RS 2  : trends    (PeriodStartDate, PeriodLabel, Value)
--      RS 3  : by service (Label, Value)
--      RS 4  : by customer segment (Label, Value)
-- -------------------------------------------------------------
DROP PROCEDURE IF EXISTS dbo."uspGetRevenueAnalytics"(
    TIMESTAMP WITHOUT TIME ZONE,
    TIMESTAMP WITHOUT TIME ZONE,
    TEXT, BIGINT,
    REFCURSOR, REFCURSOR, REFCURSOR, REFCURSOR);

CREATE OR REPLACE PROCEDURE dbo."uspGetRevenueAnalytics"(
    IN  "DateFrom"  TIMESTAMP WITHOUT TIME ZONE,
    IN  "DateTo"    TIMESTAMP WITHOUT TIME ZONE,
    IN  "TrendBy"   TEXT,
    IN  "ServiceId" BIGINT,
    INOUT ref1      REFCURSOR,
    INOUT ref2      REFCURSOR,
    INOUT ref3      REFCURSOR,
    INOUT ref4      REFCURSOR
)
LANGUAGE plpgsql AS $$
BEGIN
    -- RS 1: Summary
    OPEN ref1 FOR
    SELECT
        COALESCE(SUM(ih."GrandTotalAmount"), 0)::NUMERIC(18,2)   AS "TotalRevenue",
        COALESCE(SUM(ih."PaidAmount"),       0)::NUMERIC(18,2)   AS "PaidRevenue",
        COALESCE(SUM(ih."BalanceAmount"),     0)::NUMERIC(18,2)   AS "OutstandingRevenue",
        COUNT(*)::BIGINT                                          AS "InvoiceCount",
        CASE WHEN COUNT(*) > 0
             THEN ROUND(SUM(ih."GrandTotalAmount") / COUNT(*), 2)
             ELSE 0
        END::NUMERIC(18,2)                                        AS "AverageInvoiceValue"
    FROM public."tblInvoiceHeader" ih
    WHERE NOT ih."IsDeleted"
      AND ih."InvoiceDateUtc" >= "DateFrom"
      AND ih."InvoiceDateUtc" <  "DateTo"
      AND ("ServiceId" = 0 OR EXISTS (
              SELECT 1 FROM public."tblBooking" b
              INNER JOIN public."tblServiceRequest" sr ON b."BookingId" = sr."BookingId"
              INNER JOIN public."tblJobCard"         jc ON sr."ServiceRequestId" = jc."ServiceRequestId"
              INNER JOIN public."tblQuotationHeader" qh ON jc."JobCardId" = qh."JobCardId"
              WHERE qh."QuotationHeaderId" = ih."QuotationHeaderId"
                AND b."SlotAvailabilityId" IN (
                    SELECT sa."SlotAvailabilityId"
                    FROM public."tblSlotAvailability" sa
                    WHERE sa."ServiceId" = "ServiceId")));

    -- RS 2: Trends (PeriodStartDate, PeriodLabel, Value = revenue)
    OPEN ref2 FOR
    SELECT
        DATE_TRUNC(
            CASE "TrendBy" WHEN 'week' THEN 'week' WHEN 'month' THEN 'month' ELSE 'day' END,
            ih."InvoiceDateUtc"
        )::DATE                                                   AS "PeriodStartDate",
        CASE "TrendBy"
            WHEN 'week'  THEN TO_CHAR(DATE_TRUNC('week',  ih."InvoiceDateUtc"), 'DD Mon')
            WHEN 'month' THEN TO_CHAR(DATE_TRUNC('month', ih."InvoiceDateUtc"), 'Mon YYYY')
            ELSE              TO_CHAR(ih."InvoiceDateUtc"::DATE,                 'DD Mon')
        END                                                       AS "PeriodLabel",
        COALESCE(SUM(ih."GrandTotalAmount"), 0)::NUMERIC(18,2)   AS "Value"
    FROM public."tblInvoiceHeader" ih
    WHERE NOT ih."IsDeleted"
      AND ih."InvoiceDateUtc" >= "DateFrom"
      AND ih."InvoiceDateUtc" <  "DateTo"
    GROUP BY 1, 2
    ORDER BY 1;

    -- RS 3: Revenue by service (Label = ServiceNameSnapshot, Value = revenue)
    OPEN ref3 FOR
    SELECT
        COALESCE(b."ServiceNameSnapshot", 'Unknown')             AS "Label",
        COALESCE(SUM(ih."GrandTotalAmount"), 0)::NUMERIC(18,2)   AS "Value"
    FROM public."tblInvoiceHeader" ih
    INNER JOIN public."tblQuotationHeader" qh ON ih."QuotationHeaderId" = qh."QuotationHeaderId"
    INNER JOIN public."tblJobCard"         jc ON qh."JobCardId"         = jc."JobCardId"
    INNER JOIN public."tblServiceRequest"  sr ON jc."ServiceRequestId"  = sr."ServiceRequestId"
    INNER JOIN public."tblBooking"         b  ON sr."BookingId"         = b."BookingId"
    WHERE NOT ih."IsDeleted"
      AND ih."InvoiceDateUtc" >= "DateFrom"
      AND ih."InvoiceDateUtc" <  "DateTo"
    GROUP BY b."ServiceNameSnapshot"
    ORDER BY SUM(ih."GrandTotalAmount") DESC;

    -- RS 4: Revenue by customer segment (AMC vs Non-AMC)
    OPEN ref4 FOR
    SELECT
        CASE WHEN EXISTS (
                SELECT 1 FROM public."tblCustomerAMC" amc
                WHERE amc."CustomerId" = ih."CustomerId"
                  AND NOT amc."IsDeleted"
                  AND amc."CurrentStatus" = 1)
             THEN 'AMC Customer'
             ELSE 'Standard Customer'
        END                                                       AS "Label",
        COALESCE(SUM(ih."GrandTotalAmount"), 0)::NUMERIC(18,2)   AS "Value"
    FROM public."tblInvoiceHeader" ih
    WHERE NOT ih."IsDeleted"
      AND ih."InvoiceDateUtc" >= "DateFrom"
      AND ih."InvoiceDateUtc" <  "DateTo"
    GROUP BY 1
    ORDER BY 2 DESC;
END;
$$;


-- -------------------------------------------------------------
-- 3.3  uspGetTechnicianPerformance
--      Input : DateFrom, DateTo, TrendBy, TechnicianId, Status
--      RS 1  : summary     (TotalTechnicians, ActiveTechnicians,
--                           TotalAssignedJobs, TotalCompletedJobs,
--                           AverageCompletionHours)
--      RS 2  : per-tech    (TechnicianId, TechnicianCode, TechnicianName,
--                           JobsAssigned, JobsCompleted,
--                           CompletionRatePercentage, AverageCompletionHours,
--                           CurrentWorkload)
-- -------------------------------------------------------------
DROP PROCEDURE IF EXISTS dbo."uspGetTechnicianPerformance"(
    TIMESTAMP WITHOUT TIME ZONE,
    TIMESTAMP WITHOUT TIME ZONE,
    TEXT, BIGINT, INTEGER,
    REFCURSOR, REFCURSOR);

CREATE OR REPLACE PROCEDURE dbo."uspGetTechnicianPerformance"(
    IN  "DateFrom"      TIMESTAMP WITHOUT TIME ZONE,
    IN  "DateTo"        TIMESTAMP WITHOUT TIME ZONE,
    IN  "TrendBy"       TEXT,
    IN  "TechnicianId"  BIGINT,
    IN  "Status"        INTEGER,
    INOUT ref1          REFCURSOR,
    INOUT ref2          REFCURSOR
)
LANGUAGE plpgsql AS $$
BEGIN
    -- RS 1: Summary
    OPEN ref1 FOR
    WITH asgn AS (
        SELECT
            sra."TechnicianId",
            COUNT(*)                                                              AS jobs_assigned,
            SUM(CASE WHEN jc."WorkCompletedDateUtc" IS NOT NULL THEN 1 ELSE 0 END) AS jobs_completed,
            AVG(
                CASE WHEN jc."WorkCompletedDateUtc" IS NOT NULL
                          AND jc."WorkStartedDateUtc" IS NOT NULL
                     THEN EXTRACT(EPOCH FROM (jc."WorkCompletedDateUtc" - jc."WorkStartedDateUtc")) / 3600.0
                END
            )                                                                     AS avg_hours
        FROM public."tblServiceRequestAssignment" sra
        INNER JOIN public."tblServiceRequest" sr
            ON sra."ServiceRequestId" = sr."ServiceRequestId"
        LEFT JOIN public."tblJobCard" jc
            ON sr."ServiceRequestId" = jc."ServiceRequestId"
        WHERE NOT sra."IsDeleted"
          AND sra."AssignedDateUtc" >= "DateFrom"
          AND sra."AssignedDateUtc" <  "DateTo"
          AND ("TechnicianId" = 0 OR sra."TechnicianId" = "TechnicianId")
          AND ("Status" = 0 OR sr."CurrentStatus" = "Status")
        GROUP BY sra."TechnicianId"
    )
    SELECT
        COUNT(DISTINCT t."TechnicianId")::BIGINT          AS "TotalTechnicians",
        SUM(CASE WHEN t."IsActive" THEN 1 ELSE 0 END)::BIGINT AS "ActiveTechnicians",
        COALESCE(SUM(a.jobs_assigned), 0)::BIGINT         AS "TotalAssignedJobs",
        COALESCE(SUM(a.jobs_completed), 0)::BIGINT        AS "TotalCompletedJobs",
        COALESCE(ROUND(AVG(a.avg_hours)::NUMERIC, 2), 0)::NUMERIC(18,2) AS "AverageCompletionHours"
    FROM public."tblTechnician" t
    LEFT JOIN asgn a ON t."TechnicianId" = a."TechnicianId"
    WHERE NOT t."IsDeleted"
      AND ("TechnicianId" = 0 OR t."TechnicianId" = "TechnicianId");

    -- RS 2: Per-technician breakdown
    OPEN ref2 FOR
    SELECT
        t."TechnicianId"                                                          AS "TechnicianId",
        t."TechnicianCode"                                                        AS "TechnicianCode",
        t."TechnicianName"                                                        AS "TechnicianName",
        COUNT(sra."ServiceRequestAssignmentId")::BIGINT                           AS "JobsAssigned",
        SUM(CASE WHEN jc."WorkCompletedDateUtc" IS NOT NULL THEN 1 ELSE 0 END)::BIGINT AS "JobsCompleted",
        CASE WHEN COUNT(sra."ServiceRequestAssignmentId") > 0
             THEN ROUND(
                 SUM(CASE WHEN jc."WorkCompletedDateUtc" IS NOT NULL THEN 1.0 ELSE 0 END)
                 / COUNT(sra."ServiceRequestAssignmentId") * 100, 2)
             ELSE 0
        END::NUMERIC(18,2)                                                        AS "CompletionRatePercentage",
        COALESCE(ROUND(
            AVG(CASE WHEN jc."WorkCompletedDateUtc" IS NOT NULL
                          AND jc."WorkStartedDateUtc" IS NOT NULL
                     THEN EXTRACT(EPOCH FROM (jc."WorkCompletedDateUtc" - jc."WorkStartedDateUtc")) / 3600.0
                END
            )::NUMERIC, 2), 0)::NUMERIC(18,2)                                    AS "AverageCompletionHours",
        (SELECT COUNT(*)
         FROM public."tblServiceRequestAssignment" cw
         WHERE cw."TechnicianId" = t."TechnicianId"
           AND cw."IsActiveAssignment"
           AND NOT cw."IsDeleted")::BIGINT                                        AS "CurrentWorkload"
    FROM public."tblTechnician" t
    LEFT JOIN public."tblServiceRequestAssignment" sra
        ON t."TechnicianId" = sra."TechnicianId"
       AND NOT sra."IsDeleted"
       AND sra."AssignedDateUtc" >= "DateFrom"
       AND sra."AssignedDateUtc" <  "DateTo"
    LEFT JOIN public."tblServiceRequest" sr
        ON sra."ServiceRequestId" = sr."ServiceRequestId"
    LEFT JOIN public."tblJobCard" jc
        ON sr."ServiceRequestId" = jc."ServiceRequestId"
    WHERE NOT t."IsDeleted"
      AND ("TechnicianId" = 0 OR t."TechnicianId" = "TechnicianId")
      AND ("Status" = 0 OR sr."CurrentStatus" = "Status" OR sr."CurrentStatus" IS NULL)
    GROUP BY t."TechnicianId", t."TechnicianCode", t."TechnicianName"
    ORDER BY "JobsCompleted" DESC;
END;
$$;


-- -------------------------------------------------------------
-- 3.4  uspGetCustomerAnalytics
--      Input : DateFrom, DateTo, TrendBy
--      RS 1  : summary     (TotalCustomers, NewCustomers, ReturningCustomers,
--                           RepeatCustomers, AmcCustomers, NonAmcCustomers,
--                           RepeatRatePercentage)
--      RS 2  : segment distribution (Label, Value)
--      RS 3  : trends       (PeriodStartDate, PeriodLabel,
--                            NewCustomers, ReturningCustomers)
-- -------------------------------------------------------------
DROP PROCEDURE IF EXISTS dbo."uspGetCustomerAnalytics"(
    TIMESTAMP WITHOUT TIME ZONE,
    TIMESTAMP WITHOUT TIME ZONE,
    TEXT,
    REFCURSOR, REFCURSOR, REFCURSOR);

CREATE OR REPLACE PROCEDURE dbo."uspGetCustomerAnalytics"(
    IN  "DateFrom"  TIMESTAMP WITHOUT TIME ZONE,
    IN  "DateTo"    TIMESTAMP WITHOUT TIME ZONE,
    IN  "TrendBy"   TEXT,
    INOUT ref1      REFCURSOR,
    INOUT ref2      REFCURSOR,
    INOUT ref3      REFCURSOR
)
LANGUAGE plpgsql AS $$
BEGIN
    -- RS 1: Summary
    OPEN ref1 FOR
    WITH cust_stats AS (
        SELECT
            c."CustomerId",
            c."DateCreated",
            COUNT(b."BookingId")                                 AS booking_count,
            EXISTS (
                SELECT 1 FROM public."tblCustomerAMC" amc
                WHERE amc."CustomerId" = c."CustomerId"
                  AND NOT amc."IsDeleted"
                  AND amc."CurrentStatus" = 1)                   AS is_amc
        FROM public."tblCustomer" c
        LEFT JOIN public."tblBooking" b
            ON b."CustomerId" = c."CustomerId" AND NOT b."IsDeleted"
        WHERE NOT c."IsDeleted"
        GROUP BY c."CustomerId", c."DateCreated"
    )
    SELECT
        COUNT(*)::BIGINT                                          AS "TotalCustomers",
        SUM(CASE WHEN cs."DateCreated" >= "DateFrom"
                      AND cs."DateCreated" < "DateTo"
                 THEN 1 ELSE 0 END)::BIGINT                      AS "NewCustomers",
        SUM(CASE WHEN cs."DateCreated" <  "DateFrom"
                      AND cs.booking_count > 0
                 THEN 1 ELSE 0 END)::BIGINT                      AS "ReturningCustomers",
        SUM(CASE WHEN cs.booking_count > 1 THEN 1 ELSE 0 END)::BIGINT AS "RepeatCustomers",
        SUM(CASE WHEN cs.is_amc THEN 1 ELSE 0 END)::BIGINT      AS "AmcCustomers",
        SUM(CASE WHEN NOT cs.is_amc THEN 1 ELSE 0 END)::BIGINT  AS "NonAmcCustomers",
        CASE WHEN COUNT(*) > 0
             THEN ROUND(
                 SUM(CASE WHEN cs.booking_count > 1 THEN 1.0 ELSE 0 END)
                 / COUNT(*) * 100, 2)
             ELSE 0
        END::NUMERIC(18,2)                                        AS "RepeatRatePercentage"
    FROM cust_stats cs;

    -- RS 2: Segment distribution (Label, Value)
    OPEN ref2 FOR
    SELECT
        CASE WHEN EXISTS (
                SELECT 1 FROM public."tblCustomerAMC" amc
                WHERE amc."CustomerId" = c."CustomerId"
                  AND NOT amc."IsDeleted"
                  AND amc."CurrentStatus" = 1)
             THEN 'AMC'
             ELSE 'Standard'
        END                                                       AS "Label",
        COUNT(*)::NUMERIC(18,2)                                   AS "Value"
    FROM public."tblCustomer" c
    WHERE NOT c."IsDeleted"
    GROUP BY 1
    ORDER BY 2 DESC;

    -- RS 3: Trends (PeriodStartDate, PeriodLabel, NewCustomers, ReturningCustomers)
    OPEN ref3 FOR
    SELECT
        DATE_TRUNC(
            CASE "TrendBy" WHEN 'week' THEN 'week' WHEN 'month' THEN 'month' ELSE 'day' END,
            c."DateCreated"
        )::DATE                                                   AS "PeriodStartDate",
        CASE "TrendBy"
            WHEN 'week'  THEN TO_CHAR(DATE_TRUNC('week',  c."DateCreated"), 'DD Mon')
            WHEN 'month' THEN TO_CHAR(DATE_TRUNC('month', c."DateCreated"), 'Mon YYYY')
            ELSE              TO_CHAR(c."DateCreated"::DATE,                 'DD Mon')
        END                                                       AS "PeriodLabel",
        COUNT(CASE WHEN c."DateCreated" >= "DateFrom"
                        AND c."DateCreated" < "DateTo"
                   THEN 1 END)::BIGINT                            AS "NewCustomers",
        COUNT(CASE WHEN c."DateCreated" < "DateFrom"
                        AND EXISTS (
                            SELECT 1 FROM public."tblBooking" b
                            WHERE b."CustomerId" = c."CustomerId"
                              AND NOT b."IsDeleted"
                              AND b."BookingDateUtc" >= "DateFrom"
                              AND b."BookingDateUtc" <  "DateTo")
                   THEN 1 END)::BIGINT                            AS "ReturningCustomers"
    FROM public."tblCustomer" c
    WHERE NOT c."IsDeleted"
      AND c."DateCreated" >= "DateFrom"
      AND c."DateCreated" <  "DateTo"
    GROUP BY 1, 2
    ORDER BY 1;
END;
$$;


-- -------------------------------------------------------------
-- 3.5  uspGetSupportAnalytics
--      Input : DateFrom, DateTo, TrendBy, Status
--      RS 1  : summary     (TotalTickets, OpenTickets, ResolvedTickets,
--                           EscalationCount, AverageResolutionHours)
--      RS 2  : status distribution (Label, Value)
--      RS 3  : resolution trends   (PeriodStartDate, PeriodLabel,
--                                   ResolvedTickets, AverageResolutionHours)
-- -------------------------------------------------------------
DROP PROCEDURE IF EXISTS dbo."uspGetSupportAnalytics"(
    TIMESTAMP WITHOUT TIME ZONE,
    TIMESTAMP WITHOUT TIME ZONE,
    TEXT, INTEGER,
    REFCURSOR, REFCURSOR, REFCURSOR);

CREATE OR REPLACE PROCEDURE dbo."uspGetSupportAnalytics"(
    IN  "DateFrom"  TIMESTAMP WITHOUT TIME ZONE,
    IN  "DateTo"    TIMESTAMP WITHOUT TIME ZONE,
    IN  "TrendBy"   TEXT,
    IN  "Status"    INTEGER,
    INOUT ref1      REFCURSOR,
    INOUT ref2      REFCURSOR,
    INOUT ref3      REFCURSOR
)
LANGUAGE plpgsql AS $$
BEGIN
    -- RS 1: Summary
    -- Open = statuses 1,2,3,4,8 (not closed/resolved)
    -- Resolved = status 6
    -- Escalated = status 5 (escalation count)
    -- Resolution hours = time from ticket creation to first Resolved status
    OPEN ref1 FOR
    WITH resolution AS (
        SELECT
            sh."SupportTicketId",
            MIN(sh."StatusDateUtc") AS resolved_at
        FROM public."tblSupportTicketStatusHistory" sh
        WHERE sh."SupportTicketStatus" = 6
          AND NOT sh."IsDeleted"
        GROUP BY sh."SupportTicketId"
    )
    SELECT
        COUNT(*)::BIGINT                                                          AS "TotalTickets",
        SUM(CASE WHEN st."CurrentStatus" IN (1,2,3,4,8) THEN 1 ELSE 0 END)::BIGINT AS "OpenTickets",
        SUM(CASE WHEN st."CurrentStatus" IN (6,7)        THEN 1 ELSE 0 END)::BIGINT AS "ResolvedTickets",
        SUM(CASE WHEN st."CurrentStatus" = 5             THEN 1 ELSE 0 END)::BIGINT AS "EscalationCount",
        COALESCE(ROUND(
            AVG(CASE WHEN r.resolved_at IS NOT NULL
                     THEN EXTRACT(EPOCH FROM (r.resolved_at - st."DateCreated")) / 3600.0
                END)::NUMERIC, 2), 0)::NUMERIC(18,2)                              AS "AverageResolutionHours"
    FROM public."tblSupportTicket" st
    LEFT JOIN resolution r ON st."SupportTicketId" = r."SupportTicketId"
    WHERE NOT st."IsDeleted"
      AND st."DateCreated" >= "DateFrom"
      AND st."DateCreated" <  "DateTo"
      AND ("Status" = 0 OR st."CurrentStatus" = "Status");

    -- RS 2: Status distribution (Label, Value)
    OPEN ref2 FOR
    SELECT
        CASE st."CurrentStatus"
            WHEN 1 THEN 'Open'
            WHEN 2 THEN 'In Progress'
            WHEN 3 THEN 'Waiting For Customer'
            WHEN 4 THEN 'Customer Responded'
            WHEN 5 THEN 'Escalated'
            WHEN 6 THEN 'Resolved'
            WHEN 7 THEN 'Closed'
            WHEN 8 THEN 'Reopened'
            ELSE        'Unknown'
        END                                                                       AS "Label",
        COUNT(*)::NUMERIC(18,2)                                                   AS "Value"
    FROM public."tblSupportTicket" st
    WHERE NOT st."IsDeleted"
      AND st."DateCreated" >= "DateFrom"
      AND st."DateCreated" <  "DateTo"
    GROUP BY st."CurrentStatus"
    ORDER BY st."CurrentStatus";

    -- RS 3: Resolution trends (PeriodStartDate, PeriodLabel, ResolvedTickets, AverageResolutionHours)
    OPEN ref3 FOR
    WITH resolution AS (
        SELECT
            sh."SupportTicketId",
            MIN(sh."StatusDateUtc") AS resolved_at
        FROM public."tblSupportTicketStatusHistory" sh
        WHERE sh."SupportTicketStatus" = 6
          AND NOT sh."IsDeleted"
        GROUP BY sh."SupportTicketId"
    )
    SELECT
        DATE_TRUNC(
            CASE "TrendBy" WHEN 'week' THEN 'week' WHEN 'month' THEN 'month' ELSE 'day' END,
            st."DateCreated"
        )::DATE                                                                   AS "PeriodStartDate",
        CASE "TrendBy"
            WHEN 'week'  THEN TO_CHAR(DATE_TRUNC('week',  st."DateCreated"), 'DD Mon')
            WHEN 'month' THEN TO_CHAR(DATE_TRUNC('month', st."DateCreated"), 'Mon YYYY')
            ELSE              TO_CHAR(st."DateCreated"::DATE,                 'DD Mon')
        END                                                                       AS "PeriodLabel",
        SUM(CASE WHEN st."CurrentStatus" IN (6,7) THEN 1 ELSE 0 END)::BIGINT    AS "ResolvedTickets",
        COALESCE(ROUND(
            AVG(CASE WHEN r.resolved_at IS NOT NULL
                     THEN EXTRACT(EPOCH FROM (r.resolved_at - st."DateCreated")) / 3600.0
                END)::NUMERIC, 2), 0)::NUMERIC(18,2)                             AS "AverageResolutionHours"
    FROM public."tblSupportTicket" st
    LEFT JOIN resolution r ON st."SupportTicketId" = r."SupportTicketId"
    WHERE NOT st."IsDeleted"
      AND st."DateCreated" >= "DateFrom"
      AND st."DateCreated" <  "DateTo"
    GROUP BY 1, 2
    ORDER BY 1;
END;
$$;


-- -------------------------------------------------------------
-- 3.6  uspGetInventoryAnalytics
--      Input : DateFrom, DateTo, TrendBy
--      RS 1  : summary     (TotalItems, LowStockItems,
--                           TotalOnHandQuantity, ConsumedQuantity)
--      RS 2  : low stock   (ItemId, ItemCode, ItemName, QuantityOnHand,
--                           ReorderLevel, ShortageQuantity)
--      RS 3  : trends      (PeriodStartDate, PeriodLabel, QuantityConsumed)
-- -------------------------------------------------------------
DROP PROCEDURE IF EXISTS dbo."uspGetInventoryAnalytics"(
    TIMESTAMP WITHOUT TIME ZONE,
    TIMESTAMP WITHOUT TIME ZONE,
    TEXT,
    REFCURSOR, REFCURSOR, REFCURSOR);

CREATE OR REPLACE PROCEDURE dbo."uspGetInventoryAnalytics"(
    IN  "DateFrom"  TIMESTAMP WITHOUT TIME ZONE,
    IN  "DateTo"    TIMESTAMP WITHOUT TIME ZONE,
    IN  "TrendBy"   TEXT,
    INOUT ref1      REFCURSOR,
    INOUT ref2      REFCURSOR,
    INOUT ref3      REFCURSOR
)
LANGUAGE plpgsql AS $$
BEGIN
    -- RS 1: Summary
    -- QuantityOnHand = sum across tblWarehouseStock
    -- ConsumedQuantity = sum of JobConsumption transactions in date range
    OPEN ref1 FOR
    SELECT
        (SELECT COUNT(DISTINCT i."ItemId") FROM public."tblItem" i WHERE NOT i."IsDeleted")::BIGINT AS "TotalItems",
        (SELECT COUNT(DISTINCT ws."ItemId")
         FROM public."tblWarehouseStock" ws
         INNER JOIN public."tblItem" i ON ws."ItemId" = i."ItemId"
         WHERE NOT ws."IsDeleted"
           AND NOT i."IsDeleted"
           AND ws."QuantityOnHand" <= i."ReorderLevel")::BIGINT                   AS "LowStockItems",
        COALESCE((
            SELECT SUM(ws2."QuantityOnHand")
            FROM public."tblWarehouseStock" ws2
            WHERE NOT ws2."IsDeleted"), 0)::NUMERIC(18,2)                         AS "TotalOnHandQuantity",
        COALESCE((
            SELECT SUM(st."Quantity")
            FROM public."tblStockTransaction" st
            WHERE NOT st."IsDeleted"
              AND st."TransactionType" = 6
              AND st."TransactionDateUtc" >= "DateFrom"
              AND st."TransactionDateUtc" <  "DateTo"), 0)::NUMERIC(18,2)         AS "ConsumedQuantity";

    -- RS 2: Low-stock items
    OPEN ref2 FOR
    SELECT
        i."ItemId"                                                                AS "ItemId",
        i."ItemCode"                                                              AS "ItemCode",
        i."ItemName"                                                              AS "ItemName",
        COALESCE(SUM(ws."QuantityOnHand"), 0)::NUMERIC(18,2)                     AS "QuantityOnHand",
        i."ReorderLevel"::NUMERIC(18,2)                                           AS "ReorderLevel",
        GREATEST(0, i."ReorderLevel" - COALESCE(SUM(ws."QuantityOnHand"), 0))::NUMERIC(18,2) AS "ShortageQuantity"
    FROM public."tblItem" i
    LEFT JOIN public."tblWarehouseStock" ws
        ON ws."ItemId" = i."ItemId" AND NOT ws."IsDeleted"
    WHERE NOT i."IsDeleted"
    GROUP BY i."ItemId", i."ItemCode", i."ItemName", i."ReorderLevel"
    HAVING COALESCE(SUM(ws."QuantityOnHand"), 0) <= i."ReorderLevel"
    ORDER BY "ShortageQuantity" DESC;

    -- RS 3: Consumption trends (PeriodStartDate, PeriodLabel, QuantityConsumed)
    OPEN ref3 FOR
    SELECT
        DATE_TRUNC(
            CASE "TrendBy" WHEN 'week' THEN 'week' WHEN 'month' THEN 'month' ELSE 'day' END,
            st."TransactionDateUtc"
        )::DATE                                                                   AS "PeriodStartDate",
        CASE "TrendBy"
            WHEN 'week'  THEN TO_CHAR(DATE_TRUNC('week',  st."TransactionDateUtc"), 'DD Mon')
            WHEN 'month' THEN TO_CHAR(DATE_TRUNC('month', st."TransactionDateUtc"), 'Mon YYYY')
            ELSE              TO_CHAR(st."TransactionDateUtc"::DATE,               'DD Mon')
        END                                                                       AS "PeriodLabel",
        COALESCE(SUM(st."Quantity"), 0)::NUMERIC(18,2)                            AS "QuantityConsumed"
    FROM public."tblStockTransaction" st
    WHERE NOT st."IsDeleted"
      AND st."TransactionType" = 6  -- JobConsumption
      AND st."TransactionDateUtc" >= "DateFrom"
      AND st."TransactionDateUtc" <  "DateTo"
    GROUP BY 1, 2
    ORDER BY 1;
END;
$$;


-- -------------------------------------------------------------
-- 3.7  uspGetReportByDateRange
--      Input : DateFrom, DateTo, TrendBy
--      RS 1  : summary          (TotalBookings, TotalRevenue, CompletedJobs,
--                                TotalSupportTickets, ActiveTechnicians,
--                                NewCustomers)
--      RS 2  : booking trends   (PeriodStartDate, PeriodLabel, Value)
--      RS 3  : revenue trends   (PeriodStartDate, PeriodLabel, Value)
--      RS 4  : support status   (Label, Value)
-- -------------------------------------------------------------
DROP PROCEDURE IF EXISTS dbo."uspGetReportByDateRange"(
    TIMESTAMP WITHOUT TIME ZONE,
    TIMESTAMP WITHOUT TIME ZONE,
    TEXT,
    REFCURSOR, REFCURSOR, REFCURSOR, REFCURSOR);

CREATE OR REPLACE PROCEDURE dbo."uspGetReportByDateRange"(
    IN  "DateFrom"  TIMESTAMP WITHOUT TIME ZONE,
    IN  "DateTo"    TIMESTAMP WITHOUT TIME ZONE,
    IN  "TrendBy"   TEXT,
    INOUT ref1      REFCURSOR,
    INOUT ref2      REFCURSOR,
    INOUT ref3      REFCURSOR,
    INOUT ref4      REFCURSOR
)
LANGUAGE plpgsql AS $$
BEGIN
    -- RS 1: Summary
    OPEN ref1 FOR
    SELECT
        (SELECT COUNT(*) FROM public."tblBooking" b
         WHERE NOT b."IsDeleted"
           AND b."BookingDateUtc" >= "DateFrom"
           AND b."BookingDateUtc" <  "DateTo")::BIGINT                            AS "TotalBookings",
        COALESCE((
            SELECT SUM(ih."GrandTotalAmount")
            FROM public."tblInvoiceHeader" ih
            WHERE NOT ih."IsDeleted"
              AND ih."InvoiceDateUtc" >= "DateFrom"
              AND ih."InvoiceDateUtc" <  "DateTo"), 0)::NUMERIC(18,2)             AS "TotalRevenue",
        (SELECT COUNT(*) FROM public."tblJobCard" jc
         WHERE NOT jc."IsDeleted"
           AND jc."WorkCompletedDateUtc" IS NOT NULL
           AND jc."WorkCompletedDateUtc" >= "DateFrom"
           AND jc."WorkCompletedDateUtc" <  "DateTo")::BIGINT                     AS "CompletedJobs",
        (SELECT COUNT(*) FROM public."tblSupportTicket" st
         WHERE NOT st."IsDeleted"
           AND st."DateCreated" >= "DateFrom"
           AND st."DateCreated" <  "DateTo")::BIGINT                              AS "TotalSupportTickets",
        (SELECT COUNT(*) FROM public."tblTechnician" t
         WHERE NOT t."IsDeleted"
           AND t."IsActive")::BIGINT                                               AS "ActiveTechnicians",
        (SELECT COUNT(*) FROM public."tblCustomer" c
         WHERE NOT c."IsDeleted"
           AND c."DateCreated" >= "DateFrom"
           AND c."DateCreated" <  "DateTo")::BIGINT                               AS "NewCustomers";

    -- RS 2: Booking trends (PeriodStartDate, PeriodLabel, Value)
    OPEN ref2 FOR
    SELECT
        DATE_TRUNC(
            CASE "TrendBy" WHEN 'week' THEN 'week' WHEN 'month' THEN 'month' ELSE 'day' END,
            b."BookingDateUtc"
        )::DATE                                                                   AS "PeriodStartDate",
        CASE "TrendBy"
            WHEN 'week'  THEN TO_CHAR(DATE_TRUNC('week',  b."BookingDateUtc"), 'DD Mon')
            WHEN 'month' THEN TO_CHAR(DATE_TRUNC('month', b."BookingDateUtc"), 'Mon YYYY')
            ELSE              TO_CHAR(b."BookingDateUtc"::DATE,                  'DD Mon')
        END                                                                       AS "PeriodLabel",
        COUNT(*)::NUMERIC(18,2)                                                   AS "Value"
    FROM public."tblBooking" b
    WHERE NOT b."IsDeleted"
      AND b."BookingDateUtc" >= "DateFrom"
      AND b."BookingDateUtc" <  "DateTo"
    GROUP BY 1, 2
    ORDER BY 1;

    -- RS 3: Revenue trends (PeriodStartDate, PeriodLabel, Value)
    OPEN ref3 FOR
    SELECT
        DATE_TRUNC(
            CASE "TrendBy" WHEN 'week' THEN 'week' WHEN 'month' THEN 'month' ELSE 'day' END,
            ih."InvoiceDateUtc"
        )::DATE                                                                   AS "PeriodStartDate",
        CASE "TrendBy"
            WHEN 'week'  THEN TO_CHAR(DATE_TRUNC('week',  ih."InvoiceDateUtc"), 'DD Mon')
            WHEN 'month' THEN TO_CHAR(DATE_TRUNC('month', ih."InvoiceDateUtc"), 'Mon YYYY')
            ELSE              TO_CHAR(ih."InvoiceDateUtc"::DATE,                  'DD Mon')
        END                                                                       AS "PeriodLabel",
        COALESCE(SUM(ih."GrandTotalAmount"), 0)::NUMERIC(18,2)                   AS "Value"
    FROM public."tblInvoiceHeader" ih
    WHERE NOT ih."IsDeleted"
      AND ih."InvoiceDateUtc" >= "DateFrom"
      AND ih."InvoiceDateUtc" <  "DateTo"
    GROUP BY 1, 2
    ORDER BY 1;

    -- RS 4: Support status distribution (Label, Value)
    OPEN ref4 FOR
    SELECT
        CASE st."CurrentStatus"
            WHEN 1 THEN 'Open'
            WHEN 2 THEN 'In Progress'
            WHEN 3 THEN 'Waiting For Customer'
            WHEN 4 THEN 'Customer Responded'
            WHEN 5 THEN 'Escalated'
            WHEN 6 THEN 'Resolved'
            WHEN 7 THEN 'Closed'
            WHEN 8 THEN 'Reopened'
            ELSE        'Unknown'
        END                                                                       AS "Label",
        COUNT(*)::NUMERIC(18,2)                                                   AS "Value"
    FROM public."tblSupportTicket" st
    WHERE NOT st."IsDeleted"
      AND st."DateCreated" >= "DateFrom"
      AND st."DateCreated" <  "DateTo"
    GROUP BY st."CurrentStatus"
    ORDER BY st."CurrentStatus";
END;
$$;


-- =============================================================
-- SECTION 4: GRANT USAGE
-- =============================================================
GRANT USAGE ON SCHEMA dbo TO PUBLIC;

GRANT EXECUTE ON PROCEDURE dbo."uspGetBookingAnalytics"(
    TIMESTAMP WITHOUT TIME ZONE, TIMESTAMP WITHOUT TIME ZONE,
    TEXT, BIGINT, INTEGER,
    REFCURSOR, REFCURSOR, REFCURSOR, REFCURSOR) TO PUBLIC;

GRANT EXECUTE ON PROCEDURE dbo."uspGetRevenueAnalytics"(
    TIMESTAMP WITHOUT TIME ZONE, TIMESTAMP WITHOUT TIME ZONE,
    TEXT, BIGINT,
    REFCURSOR, REFCURSOR, REFCURSOR, REFCURSOR) TO PUBLIC;

GRANT EXECUTE ON PROCEDURE dbo."uspGetTechnicianPerformance"(
    TIMESTAMP WITHOUT TIME ZONE, TIMESTAMP WITHOUT TIME ZONE,
    TEXT, BIGINT, INTEGER,
    REFCURSOR, REFCURSOR) TO PUBLIC;

GRANT EXECUTE ON PROCEDURE dbo."uspGetCustomerAnalytics"(
    TIMESTAMP WITHOUT TIME ZONE, TIMESTAMP WITHOUT TIME ZONE,
    TEXT,
    REFCURSOR, REFCURSOR, REFCURSOR) TO PUBLIC;

GRANT EXECUTE ON PROCEDURE dbo."uspGetSupportAnalytics"(
    TIMESTAMP WITHOUT TIME ZONE, TIMESTAMP WITHOUT TIME ZONE,
    TEXT, INTEGER,
    REFCURSOR, REFCURSOR, REFCURSOR) TO PUBLIC;

GRANT EXECUTE ON PROCEDURE dbo."uspGetInventoryAnalytics"(
    TIMESTAMP WITHOUT TIME ZONE, TIMESTAMP WITHOUT TIME ZONE,
    TEXT,
    REFCURSOR, REFCURSOR, REFCURSOR) TO PUBLIC;

GRANT EXECUTE ON PROCEDURE dbo."uspGetReportByDateRange"(
    TIMESTAMP WITHOUT TIME ZONE, TIMESTAMP WITHOUT TIME ZONE,
    TEXT,
    REFCURSOR, REFCURSOR, REFCURSOR, REFCURSOR) TO PUBLIC;


-- =============================================================
-- VERIFICATION
-- =============================================================
DO $$
BEGIN
    RAISE NOTICE '=== Fix 09 applied successfully ===';
    RAISE NOTICE 'Schema dbo: %', (SELECT EXISTS(SELECT 1 FROM information_schema.schemata WHERE schema_name = 'dbo'));
    RAISE NOTICE 'Procedures created: %', (
        SELECT COUNT(*) FROM information_schema.routines
        WHERE routine_schema = 'dbo'
          AND routine_type   = 'PROCEDURE'
          AND routine_name IN (
              'uspGetBookingAnalytics', 'uspGetRevenueAnalytics',
              'uspGetTechnicianPerformance', 'uspGetCustomerAnalytics',
              'uspGetSupportAnalytics', 'uspGetInventoryAnalytics',
              'uspGetReportByDateRange')
    );
END $$;
