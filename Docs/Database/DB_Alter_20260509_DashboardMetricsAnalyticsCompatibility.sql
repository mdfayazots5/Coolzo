CREATE OR ALTER PROCEDURE dbo.uspGetBookingAnalytics
    @DateFrom DATE = NULL,
    @DateTo DATE = NULL,
    @TrendBy NVARCHAR(16) = N'day',
    @ServiceId BIGINT = 0,
    @Status INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        vba.BookingId,
        vba.BookingDate,
        vba.BookingStatus,
        vba.ServiceName,
        CASE
            WHEN @TrendBy = N'month'
                THEN DATEFROMPARTS(YEAR(vba.BookingDate), MONTH(vba.BookingDate), 1)
            WHEN @TrendBy = N'week'
                THEN CAST(DATEADD(DAY, -(DATEDIFF(DAY, 0, vba.BookingDate) % 7), vba.BookingDate) AS DATE)
            ELSE vba.BookingDate
        END AS PeriodStartDate
    INTO #FilteredBookings
    FROM dbo.vwBookingAnalytics vba
    WHERE (@DateFrom IS NULL OR vba.BookingDate >= @DateFrom)
      AND (@DateTo IS NULL OR vba.BookingDate <= @DateTo)
      AND (@ServiceId = 0 OR vba.ServiceId = @ServiceId)
      AND (@Status = 0 OR vba.BookingStatus = @Status);

    SELECT DISTINCT
        fb.BookingId,
        fb.BookingStatus,
        fb.PeriodStartDate
    INTO #FilteredBookingSummary
    FROM #FilteredBookings fb;

    SELECT
        fbs.PeriodStartDate,
        COUNT(1) AS BookingCount
    INTO #BookingTrend
    FROM #FilteredBookingSummary fbs
    GROUP BY fbs.PeriodStartDate;

    SELECT
        COUNT(1) AS TotalBookings,
        SUM(CASE WHEN fbs.BookingStatus = 1 THEN 1 ELSE 0 END) AS PendingBookings,
        SUM(CASE WHEN fbs.BookingStatus = 2 THEN 1 ELSE 0 END) AS ConfirmedBookings,
        SUM(CASE WHEN fbs.BookingStatus = 3 THEN 1 ELSE 0 END) AS CancelledBookings,
        CAST(
            CASE
                WHEN COUNT(DISTINCT fbs.PeriodStartDate) = 0 THEN 0
                ELSE COUNT(1) * 1.0 / COUNT(DISTINCT fbs.PeriodStartDate)
            END AS DECIMAL(18, 2)) AS AverageBookingsPerPeriod
    FROM #FilteredBookingSummary fbs;

    SELECT
        bt.PeriodStartDate,
        CASE
            WHEN @TrendBy = N'month' THEN CONVERT(NVARCHAR(7), bt.PeriodStartDate, 126)
            ELSE CONVERT(NVARCHAR(10), bt.PeriodStartDate, 23)
        END AS PeriodLabel,
        CAST(bt.BookingCount AS DECIMAL(18, 2)) AS Value
    FROM #BookingTrend bt
    ORDER BY bt.PeriodStartDate;

    SELECT
        CASE fbs.BookingStatus
            WHEN 1 THEN N'Pending'
            WHEN 2 THEN N'Confirmed'
            WHEN 3 THEN N'Cancelled'
            ELSE N'Unknown'
        END AS Label,
        CAST(COUNT(1) AS DECIMAL(18, 2)) AS Value
    FROM #FilteredBookingSummary fbs
    GROUP BY fbs.BookingStatus
    ORDER BY Value DESC, Label;

    SELECT TOP (10)
        fb.ServiceName AS Label,
        CAST(COUNT(DISTINCT fb.BookingId) AS DECIMAL(18, 2)) AS Value
    FROM #FilteredBookings fb
    GROUP BY fb.ServiceName
    ORDER BY Value DESC, fb.ServiceName;
END;
GO

CREATE OR ALTER PROCEDURE dbo.uspGetRevenueAnalytics
    @DateFrom DATE = NULL,
    @DateTo DATE = NULL,
    @TrendBy NVARCHAR(16) = N'day',
    @ServiceId BIGINT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        vra.InvoiceHeaderId,
        vra.InvoiceDate,
        vra.ServiceName,
        vra.CustomerSegment,
        vra.RevenueAmount,
        vra.PaidAmount,
        vra.BalanceAmount,
        CASE
            WHEN @TrendBy = N'month'
                THEN DATEFROMPARTS(YEAR(vra.InvoiceDate), MONTH(vra.InvoiceDate), 1)
            WHEN @TrendBy = N'week'
                THEN CAST(DATEADD(DAY, -(DATEDIFF(DAY, 0, vra.InvoiceDate) % 7), vra.InvoiceDate) AS DATE)
            ELSE vra.InvoiceDate
        END AS PeriodStartDate
    INTO #FilteredRevenue
    FROM dbo.vwRevenueAnalytics vra
    WHERE (@DateFrom IS NULL OR vra.InvoiceDate >= @DateFrom)
      AND (@DateTo IS NULL OR vra.InvoiceDate <= @DateTo)
      AND (@ServiceId = 0 OR vra.ServiceId = @ServiceId);

    SELECT
        fr.PeriodStartDate,
        CAST(SUM(fr.RevenueAmount) AS DECIMAL(18, 2)) AS RevenueAmount
    INTO #RevenueTrend
    FROM #FilteredRevenue fr
    GROUP BY fr.PeriodStartDate;

    SELECT
        CAST(ISNULL(SUM(fr.RevenueAmount), 0) AS DECIMAL(18, 2)) AS TotalRevenue,
        CAST(ISNULL(SUM(fr.PaidAmount), 0) AS DECIMAL(18, 2)) AS PaidRevenue,
        CAST(ISNULL(SUM(fr.BalanceAmount), 0) AS DECIMAL(18, 2)) AS OutstandingRevenue,
        COUNT(DISTINCT fr.InvoiceHeaderId) AS InvoiceCount,
        CAST(
            CASE
                WHEN COUNT(DISTINCT fr.InvoiceHeaderId) = 0 THEN 0
                ELSE SUM(fr.RevenueAmount) / COUNT(DISTINCT fr.InvoiceHeaderId)
            END AS DECIMAL(18, 2)) AS AverageInvoiceValue
    FROM #FilteredRevenue fr;

    SELECT
        rt.PeriodStartDate,
        CASE
            WHEN @TrendBy = N'month' THEN CONVERT(NVARCHAR(7), rt.PeriodStartDate, 126)
            ELSE CONVERT(NVARCHAR(10), rt.PeriodStartDate, 23)
        END AS PeriodLabel,
        CAST(rt.RevenueAmount AS DECIMAL(18, 2)) AS Value
    FROM #RevenueTrend rt
    ORDER BY rt.PeriodStartDate;

    SELECT TOP (10)
        fr.ServiceName AS Label,
        CAST(SUM(fr.RevenueAmount) AS DECIMAL(18, 2)) AS Value
    FROM #FilteredRevenue fr
    GROUP BY fr.ServiceName
    ORDER BY Value DESC, fr.ServiceName;

    SELECT
        fr.CustomerSegment AS Label,
        CAST(SUM(fr.RevenueAmount) AS DECIMAL(18, 2)) AS Value
    FROM #FilteredRevenue fr
    GROUP BY fr.CustomerSegment
    ORDER BY Value DESC, fr.CustomerSegment;
END;
GO
