using System.Data;
using System.Data.Common;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class AnalyticsReadRepository : IAnalyticsReadRepository
{
    private readonly CoolzoDbContext _dbContext;

    public AnalyticsReadRepository(CoolzoDbContext dbContext) => _dbContext = dbContext;

    // =========================================================
    // Dashboard Summary
    // =========================================================
    public async Task<DashboardSummaryReadModel> GetDashboardSummaryAsync(CancellationToken ct)
    {
        var conn = await OpenAsync(ct);
        await using var cmd = Cmd(conn, @"
            SELECT
                (SELECT COUNT(*)::BIGINT FROM public.""tblBooking""        WHERE NOT COALESCE(""IsDeleted"", FALSE)),
                (SELECT COUNT(*)::BIGINT FROM public.""tblServiceRequest"" WHERE NOT COALESCE(""IsDeleted"", FALSE)),
                (SELECT COUNT(*)::BIGINT FROM public.""tblJobCard""        WHERE NOT COALESCE(""IsDeleted"", FALSE)),
                (SELECT COALESCE(SUM(""GrandTotalAmount""), 0)::NUMERIC(18,2)
                        FROM public.""tblInvoiceHeader""                   WHERE NOT COALESCE(""IsDeleted"", FALSE)),
                (SELECT COUNT(*)::BIGINT FROM public.""tblCustomerAMC""    WHERE NOT COALESCE(""IsDeleted"", FALSE)),
                (SELECT COUNT(*)::BIGINT FROM public.""tblSupportTicket""  WHERE NOT COALESCE(""IsDeleted"", FALSE))");

        await using var r = await cmd.ExecuteReaderAsync(ct);
        long tb = 0, tsr = 0, tj = 0, tamc = 0, tst = 0; decimal rev = 0;
        if (await r.ReadAsync(ct))
        {
            tb   = r.IsDBNull(0) ? 0 : r.GetInt64(0);
            tsr  = r.IsDBNull(1) ? 0 : r.GetInt64(1);
            tj   = r.IsDBNull(2) ? 0 : r.GetInt64(2);
            rev  = r.IsDBNull(3) ? 0 : r.GetDecimal(3);
            tamc = r.IsDBNull(4) ? 0 : r.GetInt64(4);
            tst  = r.IsDBNull(5) ? 0 : r.GetInt64(5);
        }
        return new DashboardSummaryReadModel(tb, tsr, tj, rev, tamc, tst,
            Array.Empty<AnalyticsBreakdownItemReadModel>());
    }

    // =========================================================
    // Booking Analytics
    // =========================================================
    public async Task<BookingAnalyticsReadModel> GetBookingAnalyticsAsync(
        AnalyticsQueryFilter filter, int? bookingStatus, CancellationToken ct)
    {
        var conn   = await OpenAsync(ct);
        var df     = filter.DateFrom.ToDateTime(TimeOnly.MinValue);
        var dt     = filter.DateTo.ToDateTime(TimeOnly.MinValue);
        var svcId  = filter.ServiceId ?? 0L;
        var status = bookingStatus ?? 0;
        var trend  = NormaliseTrend(filter.TrendBy);

        long totalBookings = 0, pending = 0, confirmed = 0, cancelled = 0;
        decimal avg = 0;

        await using (var cmd = Cmd(conn, @"
            SELECT
                COUNT(*)::BIGINT AS ""TotalBookings"",
                SUM(CASE WHEN b.""BookingStatus"" = 1 THEN 1 ELSE 0 END)::BIGINT AS ""PendingBookings"",
                SUM(CASE WHEN b.""BookingStatus"" = 2 THEN 1 ELSE 0 END)::BIGINT AS ""ConfirmedBookings"",
                SUM(CASE WHEN b.""BookingStatus"" = 3 THEN 1 ELSE 0 END)::BIGINT AS ""CancelledBookings"",
                CASE WHEN EXTRACT(EPOCH FROM (@dt - @df)) > 0
                     THEN ROUND(COUNT(*)::NUMERIC /
                          (EXTRACT(EPOCH FROM (@dt - @df)) /
                           EXTRACT(EPOCH FROM CASE @trend
                               WHEN 'week'  THEN INTERVAL '7 days'
                               WHEN 'month' THEN INTERVAL '30 days'
                               ELSE              INTERVAL '1 day' END)), 2)
                     ELSE 0 END::NUMERIC(18,2) AS ""AverageBookingsPerPeriod""
            FROM public.""tblBooking"" b
            WHERE NOT COALESCE(b.""IsDeleted"", FALSE)
              AND b.""BookingDateUtc"" >= @df AND b.""BookingDateUtc"" < @dt
              AND (@svcId = 0 OR EXISTS (
                      SELECT 1 FROM public.""tblBookingLine"" bl
                      WHERE bl.""BookingId"" = b.""BookingId"" AND bl.""ServiceId"" = @svcId))
              AND (@status = 0 OR b.""BookingStatus"" = @status)"))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt); P(cmd, "@trend", trend);
            P(cmd, "@svcId", svcId); P(cmd, "@status", status);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            if (await r.ReadAsync(ct))
            {
                totalBookings = I64(r, "TotalBookings"); pending    = I64(r, "PendingBookings");
                confirmed     = I64(r, "ConfirmedBookings"); cancelled = I64(r, "CancelledBookings");
                avg           = Dec(r, "AverageBookingsPerPeriod");
            }
        }

        var trends = new List<AnalyticsTrendPointReadModel>();
        await using (var cmd = Cmd(conn, TrendSql("b.\"BookingDateUtc\"", "public.\"tblBooking\" b",
            "NOT COALESCE(b.\"IsDeleted\", FALSE) AND b.\"BookingDateUtc\" >= @df AND b.\"BookingDateUtc\" < @dt\n" +
            "              AND (@svcId = 0 OR EXISTS (\n" +
            "                      SELECT 1 FROM public.\"tblBookingLine\" bl\n" +
            "                      WHERE bl.\"BookingId\" = b.\"BookingId\" AND bl.\"ServiceId\" = @svcId))\n" +
            "              AND (@status = 0 OR b.\"BookingStatus\" = @status)", "COUNT(*)::NUMERIC(18,2)")))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt); P(cmd, "@trend", trend);
            P(cmd, "@svcId", svcId); P(cmd, "@status", status);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                trends.Add(new AnalyticsTrendPointReadModel(DO(r, "PeriodStartDate"), Str(r, "PeriodLabel"), Dec(r, "Value")));
        }

        var statusDist = new List<AnalyticsBreakdownItemReadModel>();
        await using (var cmd = Cmd(conn, @"
            SELECT CASE b.""BookingStatus""
                       WHEN 1 THEN 'Pending' WHEN 2 THEN 'Confirmed'
                       WHEN 3 THEN 'Cancelled' ELSE 'Unknown' END AS ""Label"",
                   COUNT(*)::NUMERIC(18,2) AS ""Value""
            FROM public.""tblBooking"" b
            WHERE NOT COALESCE(b.""IsDeleted"", FALSE)
              AND b.""BookingDateUtc"" >= @df AND b.""BookingDateUtc"" < @dt
            GROUP BY b.""BookingStatus"" ORDER BY b.""BookingStatus"""))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) statusDist.Add(BD(r));
        }

        var svcDist = new List<AnalyticsBreakdownItemReadModel>();
        await using (var cmd = Cmd(conn, @"
            SELECT COALESCE(b.""ServiceNameSnapshot"", 'Unknown') AS ""Label"",
                   COUNT(*)::NUMERIC(18,2) AS ""Value""
            FROM public.""tblBooking"" b
            WHERE NOT COALESCE(b.""IsDeleted"", FALSE)
              AND b.""BookingDateUtc"" >= @df AND b.""BookingDateUtc"" < @dt
            GROUP BY b.""ServiceNameSnapshot"" ORDER BY COUNT(*) DESC"))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) svcDist.Add(BD(r));
        }

        return new BookingAnalyticsReadModel(totalBookings, pending, confirmed, cancelled, avg,
            trends, statusDist, svcDist);
    }

    // =========================================================
    // Revenue Analytics
    // =========================================================
    public async Task<RevenueAnalyticsReadModel> GetRevenueAnalyticsAsync(
        AnalyticsQueryFilter filter, CancellationToken ct)
    {
        var conn  = await OpenAsync(ct);
        var df    = filter.DateFrom.ToDateTime(TimeOnly.MinValue);
        var dt    = filter.DateTo.ToDateTime(TimeOnly.MinValue);
        var svcId = filter.ServiceId ?? 0L;
        var trend = NormaliseTrend(filter.TrendBy);

        decimal totalRevenue = 0, paidRevenue = 0, outstandingRevenue = 0, avgInvoice = 0;
        long invoiceCount = 0;

        await using (var cmd = Cmd(conn, @"
            SELECT
                COALESCE(SUM(ih.""GrandTotalAmount""), 0)::NUMERIC(18,2) AS ""TotalRevenue"",
                COALESCE(SUM(ih.""PaidAmount""),       0)::NUMERIC(18,2) AS ""PaidRevenue"",
                COALESCE(SUM(ih.""BalanceAmount""),    0)::NUMERIC(18,2) AS ""OutstandingRevenue"",
                COUNT(*)::BIGINT                                          AS ""InvoiceCount"",
                CASE WHEN COUNT(*) > 0
                     THEN ROUND(SUM(ih.""GrandTotalAmount"") / COUNT(*), 2)
                     ELSE 0 END::NUMERIC(18,2)                           AS ""AverageInvoiceValue""
            FROM public.""tblInvoiceHeader"" ih
            WHERE NOT COALESCE(ih.""IsDeleted"", FALSE)
              AND ih.""InvoiceDateUtc"" >= @df AND ih.""InvoiceDateUtc"" < @dt
              AND (@svcId = 0 OR EXISTS (
                      SELECT 1 FROM public.""tblBooking"" b
                      JOIN public.""tblServiceRequest""  sr ON b.""BookingId""         = sr.""BookingId""
                      JOIN public.""tblJobCard""          jc ON sr.""ServiceRequestId"" = jc.""ServiceRequestId""
                      JOIN public.""tblQuotationHeader""  qh ON jc.""JobCardId""        = qh.""JobCardId""
                      JOIN public.""tblBookingLine""      bl ON bl.""BookingId""        = b.""BookingId""
                      WHERE qh.""QuotationHeaderId"" = ih.""QuotationHeaderId""
                        AND bl.""ServiceId"" = @svcId))"))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt); P(cmd, "@svcId", svcId);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            if (await r.ReadAsync(ct))
            {
                totalRevenue      = Dec(r, "TotalRevenue");
                paidRevenue       = Dec(r, "PaidRevenue");
                outstandingRevenue = Dec(r, "OutstandingRevenue");
                invoiceCount      = I64(r, "InvoiceCount");
                avgInvoice        = Dec(r, "AverageInvoiceValue");
            }
        }

        var trends = new List<AnalyticsTrendPointReadModel>();
        await using (var cmd = Cmd(conn, TrendSql("ih.\"InvoiceDateUtc\"", "public.\"tblInvoiceHeader\" ih",
            "NOT COALESCE(ih.\"IsDeleted\", FALSE) AND ih.\"InvoiceDateUtc\" >= @df AND ih.\"InvoiceDateUtc\" < @dt",
            "COALESCE(SUM(ih.\"GrandTotalAmount\"), 0)::NUMERIC(18,2)")))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt); P(cmd, "@trend", trend);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                trends.Add(new AnalyticsTrendPointReadModel(DO(r, "PeriodStartDate"), Str(r, "PeriodLabel"), Dec(r, "Value")));
        }

        var byService = new List<AnalyticsBreakdownItemReadModel>();
        await using (var cmd = Cmd(conn, @"
            SELECT COALESCE(b.""ServiceNameSnapshot"", 'Unknown') AS ""Label"",
                   COALESCE(SUM(ih.""GrandTotalAmount""), 0)::NUMERIC(18,2) AS ""Value""
            FROM public.""tblInvoiceHeader"" ih
            JOIN public.""tblQuotationHeader"" qh ON ih.""QuotationHeaderId"" = qh.""QuotationHeaderId""
            JOIN public.""tblJobCard""         jc ON qh.""JobCardId""         = jc.""JobCardId""
            JOIN public.""tblServiceRequest""  sr ON jc.""ServiceRequestId""  = sr.""ServiceRequestId""
            JOIN public.""tblBooking""         b  ON sr.""BookingId""         = b.""BookingId""
            WHERE NOT COALESCE(ih.""IsDeleted"", FALSE)
              AND ih.""InvoiceDateUtc"" >= @df AND ih.""InvoiceDateUtc"" < @dt
            GROUP BY b.""ServiceNameSnapshot"" ORDER BY SUM(ih.""GrandTotalAmount"") DESC"))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) byService.Add(BD(r));
        }

        var bySegment = new List<AnalyticsBreakdownItemReadModel>();
        await using (var cmd = Cmd(conn, @"
            SELECT CASE WHEN EXISTS (
                            SELECT 1 FROM public.""tblCustomerAMC"" amc
                            WHERE amc.""CustomerId"" = ih.""CustomerId""
                              AND NOT COALESCE(amc.""IsDeleted"", FALSE)
                              AND amc.""CurrentStatus"" = 1)
                        THEN 'AMC Customer' ELSE 'Standard Customer' END AS ""Label"",
                   COALESCE(SUM(ih.""GrandTotalAmount""), 0)::NUMERIC(18,2) AS ""Value""
            FROM public.""tblInvoiceHeader"" ih
            WHERE NOT COALESCE(ih.""IsDeleted"", FALSE)
              AND ih.""InvoiceDateUtc"" >= @df AND ih.""InvoiceDateUtc"" < @dt
            GROUP BY 1 ORDER BY 2 DESC"))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) bySegment.Add(BD(r));
        }

        return new RevenueAnalyticsReadModel(totalRevenue, paidRevenue, outstandingRevenue,
            invoiceCount, avgInvoice, trends, byService, bySegment);
    }

    // =========================================================
    // Technician Performance
    // =========================================================
    public async Task<TechnicianPerformanceReadModel> GetTechnicianPerformanceAsync(
        AnalyticsQueryFilter filter, int? serviceRequestStatus, CancellationToken ct)
    {
        var conn   = await OpenAsync(ct);
        var df     = filter.DateFrom.ToDateTime(TimeOnly.MinValue);
        var dt     = filter.DateTo.ToDateTime(TimeOnly.MinValue);
        var techId = filter.TechnicianId ?? 0L;
        var status = serviceRequestStatus ?? 0;

        long totalTech = 0, activeTech = 0, assigned = 0, completed = 0;
        decimal avgHours = 0;

        await using (var cmd = Cmd(conn, @"
            WITH asgn AS (
                SELECT sra.""TechnicianId"",
                       COUNT(*) AS jobs_assigned,
                       SUM(CASE WHEN jc.""WorkCompletedDateUtc"" IS NOT NULL THEN 1 ELSE 0 END) AS jobs_completed,
                       AVG(CASE WHEN jc.""WorkCompletedDateUtc"" IS NOT NULL
                                     AND jc.""WorkStartedDateUtc"" IS NOT NULL
                                THEN EXTRACT(EPOCH FROM (jc.""WorkCompletedDateUtc"" - jc.""WorkStartedDateUtc"")) / 3600.0
                           END) AS avg_hours
                FROM public.""tblServiceRequestAssignment"" sra
                JOIN public.""tblServiceRequest"" sr ON sra.""ServiceRequestId"" = sr.""ServiceRequestId""
                LEFT JOIN public.""tblJobCard""   jc ON sr.""ServiceRequestId""  = jc.""ServiceRequestId""
                WHERE NOT COALESCE(sra.""IsDeleted"", FALSE)
                  AND sra.""AssignedDateUtc"" >= @df AND sra.""AssignedDateUtc"" < @dt
                  AND (@techId = 0 OR sra.""TechnicianId"" = @techId)
                  AND (@status = 0 OR sr.""CurrentStatus"" = @status)
                GROUP BY sra.""TechnicianId""
            )
            SELECT
                COUNT(DISTINCT t.""TechnicianId"")::BIGINT                            AS ""TotalTechnicians"",
                SUM(CASE WHEN t.""IsActive"" THEN 1 ELSE 0 END)::BIGINT              AS ""ActiveTechnicians"",
                COALESCE(SUM(a.jobs_assigned),  0)::BIGINT                            AS ""TotalAssignedJobs"",
                COALESCE(SUM(a.jobs_completed), 0)::BIGINT                            AS ""TotalCompletedJobs"",
                COALESCE(ROUND(AVG(a.avg_hours)::NUMERIC, 2), 0)::NUMERIC(18,2)      AS ""AverageCompletionHours""
            FROM public.""tblTechnician"" t
            LEFT JOIN asgn a ON t.""TechnicianId"" = a.""TechnicianId""
            WHERE NOT COALESCE(t.""IsDeleted"", FALSE)
              AND (@techId = 0 OR t.""TechnicianId"" = @techId)"))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt); P(cmd, "@techId", techId); P(cmd, "@status", status);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            if (await r.ReadAsync(ct))
            {
                totalTech  = I64(r, "TotalTechnicians"); activeTech = I64(r, "ActiveTechnicians");
                assigned   = I64(r, "TotalAssignedJobs"); completed  = I64(r, "TotalCompletedJobs");
                avgHours   = Dec(r, "AverageCompletionHours");
            }
        }

        var technicians = new List<TechnicianPerformanceItemReadModel>();
        await using (var cmd = Cmd(conn, @"
            SELECT
                t.""TechnicianId"", t.""TechnicianCode"", t.""TechnicianName"",
                COUNT(sra.""ServiceRequestAssignmentId"")::BIGINT AS ""JobsAssigned"",
                SUM(CASE WHEN jc.""WorkCompletedDateUtc"" IS NOT NULL THEN 1 ELSE 0 END)::BIGINT AS ""JobsCompleted"",
                CASE WHEN COUNT(sra.""ServiceRequestAssignmentId"") > 0
                     THEN ROUND(SUM(CASE WHEN jc.""WorkCompletedDateUtc"" IS NOT NULL THEN 1.0 ELSE 0 END) /
                                COUNT(sra.""ServiceRequestAssignmentId"") * 100, 2)
                     ELSE 0 END::NUMERIC(18,2) AS ""CompletionRatePercentage"",
                COALESCE(ROUND(AVG(
                    CASE WHEN jc.""WorkCompletedDateUtc"" IS NOT NULL AND jc.""WorkStartedDateUtc"" IS NOT NULL
                         THEN EXTRACT(EPOCH FROM (jc.""WorkCompletedDateUtc"" - jc.""WorkStartedDateUtc"")) / 3600.0 END
                )::NUMERIC, 2), 0)::NUMERIC(18,2) AS ""AverageCompletionHours"",
                (SELECT COUNT(*) FROM public.""tblServiceRequestAssignment"" cw
                 WHERE cw.""TechnicianId"" = t.""TechnicianId""
                   AND cw.""IsActiveAssignment"" AND NOT COALESCE(cw.""IsDeleted"", FALSE))::BIGINT AS ""CurrentWorkload""
            FROM public.""tblTechnician"" t
            LEFT JOIN public.""tblServiceRequestAssignment"" sra
                   ON t.""TechnicianId"" = sra.""TechnicianId""
                  AND NOT COALESCE(sra.""IsDeleted"", FALSE)
                  AND sra.""AssignedDateUtc"" >= @df AND sra.""AssignedDateUtc"" < @dt
            LEFT JOIN public.""tblServiceRequest"" sr ON sra.""ServiceRequestId"" = sr.""ServiceRequestId""
            LEFT JOIN public.""tblJobCard""         jc ON sr.""ServiceRequestId""  = jc.""ServiceRequestId""
            WHERE NOT COALESCE(t.""IsDeleted"", FALSE)
              AND (@techId = 0 OR t.""TechnicianId"" = @techId)
              AND (@status = 0 OR sr.""CurrentStatus"" = @status OR sr.""CurrentStatus"" IS NULL)
            GROUP BY t.""TechnicianId"", t.""TechnicianCode"", t.""TechnicianName""
            ORDER BY ""JobsCompleted"" DESC"))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt); P(cmd, "@techId", techId); P(cmd, "@status", status);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                technicians.Add(new TechnicianPerformanceItemReadModel(
                    I64(r, "TechnicianId"), Str(r, "TechnicianCode"), Str(r, "TechnicianName"),
                    I64(r, "JobsAssigned"), I64(r, "JobsCompleted"),
                    Dec(r, "CompletionRatePercentage"), Dec(r, "AverageCompletionHours"),
                    I64(r, "CurrentWorkload")));
        }

        return new TechnicianPerformanceReadModel(totalTech, activeTech, assigned, completed,
            avgHours, technicians);
    }

    // =========================================================
    // Customer Analytics
    // =========================================================
    public async Task<CustomerAnalyticsReadModel> GetCustomerAnalyticsAsync(
        AnalyticsQueryFilter filter, CancellationToken ct)
    {
        var conn  = await OpenAsync(ct);
        var df    = filter.DateFrom.ToDateTime(TimeOnly.MinValue);
        var dt    = filter.DateTo.ToDateTime(TimeOnly.MinValue);
        var trend = NormaliseTrend(filter.TrendBy);

        long total = 0, newC = 0, returning = 0, repeat = 0, amc = 0, nonAmc = 0;
        decimal repeatRate = 0;

        await using (var cmd = Cmd(conn, @"
            WITH cust_stats AS (
                SELECT c.""CustomerId"", c.""DateCreated"",
                       COUNT(b.""BookingId"") AS booking_count,
                       EXISTS (SELECT 1 FROM public.""tblCustomerAMC"" a
                               WHERE a.""CustomerId"" = c.""CustomerId""
                                 AND NOT COALESCE(a.""IsDeleted"", FALSE)
                                 AND a.""CurrentStatus"" = 1) AS is_amc
                FROM public.""tblCustomer"" c
                LEFT JOIN public.""tblBooking"" b
                       ON b.""CustomerId"" = c.""CustomerId"" AND NOT COALESCE(b.""IsDeleted"", FALSE)
                WHERE NOT COALESCE(c.""IsDeleted"", FALSE)
                GROUP BY c.""CustomerId"", c.""DateCreated""
            )
            SELECT
                COUNT(*)::BIGINT AS ""TotalCustomers"",
                SUM(CASE WHEN cs.""DateCreated"" >= @df AND cs.""DateCreated"" < @dt THEN 1 ELSE 0 END)::BIGINT AS ""NewCustomers"",
                SUM(CASE WHEN cs.""DateCreated"" <  @df AND cs.booking_count > 0     THEN 1 ELSE 0 END)::BIGINT AS ""ReturningCustomers"",
                SUM(CASE WHEN cs.booking_count > 1 THEN 1 ELSE 0 END)::BIGINT AS ""RepeatCustomers"",
                SUM(CASE WHEN     cs.is_amc THEN 1 ELSE 0 END)::BIGINT AS ""AmcCustomers"",
                SUM(CASE WHEN NOT cs.is_amc THEN 1 ELSE 0 END)::BIGINT AS ""NonAmcCustomers"",
                CASE WHEN COUNT(*) > 0
                     THEN ROUND(SUM(CASE WHEN cs.booking_count > 1 THEN 1.0 ELSE 0 END) / COUNT(*) * 100, 2)
                     ELSE 0 END::NUMERIC(18,2) AS ""RepeatRatePercentage""
            FROM cust_stats cs"))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            if (await r.ReadAsync(ct))
            {
                total      = I64(r, "TotalCustomers"); newC      = I64(r, "NewCustomers");
                returning  = I64(r, "ReturningCustomers"); repeat = I64(r, "RepeatCustomers");
                amc        = I64(r, "AmcCustomers"); nonAmc     = I64(r, "NonAmcCustomers");
                repeatRate = Dec(r, "RepeatRatePercentage");
            }
        }

        var segmentDist = new List<AnalyticsBreakdownItemReadModel>();
        await using (var cmd = Cmd(conn, @"
            SELECT CASE WHEN EXISTS (
                            SELECT 1 FROM public.""tblCustomerAMC"" a
                            WHERE a.""CustomerId"" = c.""CustomerId""
                              AND NOT COALESCE(a.""IsDeleted"", FALSE) AND a.""CurrentStatus"" = 1)
                        THEN 'AMC' ELSE 'Standard' END AS ""Label"",
                   COUNT(*)::NUMERIC(18,2) AS ""Value""
            FROM public.""tblCustomer"" c
            WHERE NOT COALESCE(c.""IsDeleted"", FALSE)
            GROUP BY 1 ORDER BY 2 DESC"))
        {
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) segmentDist.Add(BD(r));
        }

        var trends = new List<CustomerTrendPointReadModel>();
        await using (var cmd = Cmd(conn, @"
            SELECT
                DATE_TRUNC(CASE @trend WHEN 'week' THEN 'week' WHEN 'month' THEN 'month' ELSE 'day' END,
                           c.""DateCreated"")::DATE AS ""PeriodStartDate"",
                CASE @trend
                    WHEN 'week'  THEN TO_CHAR(DATE_TRUNC('week',  c.""DateCreated""), 'DD Mon')
                    WHEN 'month' THEN TO_CHAR(DATE_TRUNC('month', c.""DateCreated""), 'Mon YYYY')
                    ELSE              TO_CHAR(c.""DateCreated""::DATE, 'DD Mon')
                END AS ""PeriodLabel"",
                COUNT(CASE WHEN c.""DateCreated"" >= @df AND c.""DateCreated"" < @dt THEN 1 END)::BIGINT AS ""NewCustomers"",
                COUNT(CASE WHEN c.""DateCreated"" < @df AND EXISTS (
                               SELECT 1 FROM public.""tblBooking"" b
                               WHERE b.""CustomerId"" = c.""CustomerId""
                                 AND NOT COALESCE(b.""IsDeleted"", FALSE)
                                 AND b.""BookingDateUtc"" >= @df AND b.""BookingDateUtc"" < @dt)
                       THEN 1 END)::BIGINT AS ""ReturningCustomers""
            FROM public.""tblCustomer"" c
            WHERE NOT COALESCE(c.""IsDeleted"", FALSE)
              AND c.""DateCreated"" >= @df AND c.""DateCreated"" < @dt
            GROUP BY 1, 2 ORDER BY 1"))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt); P(cmd, "@trend", trend);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                trends.Add(new CustomerTrendPointReadModel(
                    DO(r, "PeriodStartDate"), Str(r, "PeriodLabel"),
                    I64(r, "NewCustomers"), I64(r, "ReturningCustomers")));
        }

        return new CustomerAnalyticsReadModel(total, newC, returning, repeat, amc, nonAmc,
            repeatRate, segmentDist, trends);
    }

    // =========================================================
    // Support Analytics
    // =========================================================
    public async Task<SupportAnalyticsReadModel> GetSupportAnalyticsAsync(
        AnalyticsQueryFilter filter, int? supportTicketStatus, CancellationToken ct)
    {
        var conn   = await OpenAsync(ct);
        var df     = filter.DateFrom.ToDateTime(TimeOnly.MinValue);
        var dt     = filter.DateTo.ToDateTime(TimeOnly.MinValue);
        var status = supportTicketStatus ?? 0;
        var trend  = NormaliseTrend(filter.TrendBy);

        long totalTickets = 0, openTickets = 0, resolved = 0, escalations = 0;
        decimal avgResHours = 0;

        await using (var cmd = Cmd(conn, @"
            WITH resolution AS (
                SELECT sh.""SupportTicketId"", MIN(sh.""StatusDateUtc"") AS resolved_at
                FROM public.""tblSupportTicketStatusHistory"" sh
                WHERE sh.""SupportTicketStatus"" = 6 AND NOT COALESCE(sh.""IsDeleted"", FALSE)
                GROUP BY sh.""SupportTicketId""
            )
            SELECT
                COUNT(*)::BIGINT AS ""TotalTickets"",
                SUM(CASE WHEN st.""CurrentStatus"" IN (1,2,3,4,8) THEN 1 ELSE 0 END)::BIGINT AS ""OpenTickets"",
                SUM(CASE WHEN st.""CurrentStatus"" IN (6,7)        THEN 1 ELSE 0 END)::BIGINT AS ""ResolvedTickets"",
                SUM(CASE WHEN st.""CurrentStatus"" = 5             THEN 1 ELSE 0 END)::BIGINT AS ""EscalationCount"",
                COALESCE(ROUND(AVG(CASE WHEN r.resolved_at IS NOT NULL
                    THEN EXTRACT(EPOCH FROM (r.resolved_at - st.""DateCreated"")) / 3600.0
                    END)::NUMERIC, 2), 0)::NUMERIC(18,2) AS ""AverageResolutionHours""
            FROM public.""tblSupportTicket"" st
            LEFT JOIN resolution r ON st.""SupportTicketId"" = r.""SupportTicketId""
            WHERE NOT COALESCE(st.""IsDeleted"", FALSE)
              AND st.""DateCreated"" >= @df AND st.""DateCreated"" < @dt
              AND (@status = 0 OR st.""CurrentStatus"" = @status)"))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt); P(cmd, "@status", status);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            if (await r.ReadAsync(ct))
            {
                totalTickets = I64(r, "TotalTickets"); openTickets = I64(r, "OpenTickets");
                resolved     = I64(r, "ResolvedTickets"); escalations = I64(r, "EscalationCount");
                avgResHours  = Dec(r, "AverageResolutionHours");
            }
        }

        var statusDist = new List<AnalyticsBreakdownItemReadModel>();
        await using (var cmd = Cmd(conn, @"
            SELECT CASE st.""CurrentStatus""
                       WHEN 1 THEN 'Open'               WHEN 2 THEN 'In Progress'
                       WHEN 3 THEN 'Waiting For Customer' WHEN 4 THEN 'Customer Responded'
                       WHEN 5 THEN 'Escalated'          WHEN 6 THEN 'Resolved'
                       WHEN 7 THEN 'Closed'             WHEN 8 THEN 'Reopened'
                       ELSE 'Unknown' END AS ""Label"",
                   COUNT(*)::NUMERIC(18,2) AS ""Value""
            FROM public.""tblSupportTicket"" st
            WHERE NOT COALESCE(st.""IsDeleted"", FALSE)
              AND st.""DateCreated"" >= @df AND st.""DateCreated"" < @dt
            GROUP BY st.""CurrentStatus"" ORDER BY st.""CurrentStatus"""))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) statusDist.Add(BD(r));
        }

        var trends = new List<SupportResolutionTrendPointReadModel>();
        await using (var cmd = Cmd(conn, @"
            WITH resolution AS (
                SELECT sh.""SupportTicketId"", MIN(sh.""StatusDateUtc"") AS resolved_at
                FROM public.""tblSupportTicketStatusHistory"" sh
                WHERE sh.""SupportTicketStatus"" = 6 AND NOT COALESCE(sh.""IsDeleted"", FALSE)
                GROUP BY sh.""SupportTicketId""
            )
            SELECT
                DATE_TRUNC(CASE @trend WHEN 'week' THEN 'week' WHEN 'month' THEN 'month' ELSE 'day' END,
                           st.""DateCreated"")::DATE AS ""PeriodStartDate"",
                CASE @trend
                    WHEN 'week'  THEN TO_CHAR(DATE_TRUNC('week',  st.""DateCreated""), 'DD Mon')
                    WHEN 'month' THEN TO_CHAR(DATE_TRUNC('month', st.""DateCreated""), 'Mon YYYY')
                    ELSE              TO_CHAR(st.""DateCreated""::DATE, 'DD Mon')
                END AS ""PeriodLabel"",
                SUM(CASE WHEN st.""CurrentStatus"" IN (6,7) THEN 1 ELSE 0 END)::BIGINT AS ""ResolvedTickets"",
                COALESCE(ROUND(AVG(CASE WHEN r.resolved_at IS NOT NULL
                    THEN EXTRACT(EPOCH FROM (r.resolved_at - st.""DateCreated"")) / 3600.0
                    END)::NUMERIC, 2), 0)::NUMERIC(18,2) AS ""AverageResolutionHours""
            FROM public.""tblSupportTicket"" st
            LEFT JOIN resolution r ON st.""SupportTicketId"" = r.""SupportTicketId""
            WHERE NOT COALESCE(st.""IsDeleted"", FALSE)
              AND st.""DateCreated"" >= @df AND st.""DateCreated"" < @dt
            GROUP BY 1, 2 ORDER BY 1"))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt); P(cmd, "@trend", trend);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                trends.Add(new SupportResolutionTrendPointReadModel(
                    DO(r, "PeriodStartDate"), Str(r, "PeriodLabel"),
                    I64(r, "ResolvedTickets"), Dec(r, "AverageResolutionHours")));
        }

        return new SupportAnalyticsReadModel(totalTickets, openTickets, resolved, escalations,
            avgResHours, statusDist, trends);
    }

    // =========================================================
    // Inventory Analytics
    // =========================================================
    public async Task<InventoryAnalyticsReadModel> GetInventoryAnalyticsAsync(
        AnalyticsQueryFilter filter, CancellationToken ct)
    {
        var conn  = await OpenAsync(ct);
        var df    = filter.DateFrom.ToDateTime(TimeOnly.MinValue);
        var dt    = filter.DateTo.ToDateTime(TimeOnly.MinValue);
        var trend = NormaliseTrend(filter.TrendBy);

        long totalItems = 0, lowStockItems = 0;
        decimal totalOnHand = 0, consumed = 0;

        await using (var cmd = Cmd(conn, @"
            SELECT
                (SELECT COUNT(DISTINCT i.""ItemId"") FROM public.""tblItem"" i
                 WHERE NOT COALESCE(i.""IsDeleted"", FALSE))::BIGINT AS ""TotalItems"",
                (SELECT COUNT(DISTINCT ws.""ItemId"")
                 FROM public.""tblWarehouseStock"" ws
                 JOIN public.""tblItem"" i ON ws.""ItemId"" = i.""ItemId""
                 WHERE NOT COALESCE(ws.""IsDeleted"", FALSE) AND NOT COALESCE(i.""IsDeleted"", FALSE)
                   AND ws.""QuantityOnHand"" <= i.""ReorderLevel"")::BIGINT AS ""LowStockItems"",
                COALESCE((SELECT SUM(ws2.""QuantityOnHand"") FROM public.""tblWarehouseStock"" ws2
                          WHERE NOT COALESCE(ws2.""IsDeleted"", FALSE)), 0)::NUMERIC(18,2) AS ""TotalOnHandQuantity"",
                COALESCE((SELECT SUM(st.""Quantity"") FROM public.""tblStockTransaction"" st
                          WHERE NOT COALESCE(st.""IsDeleted"", FALSE) AND st.""TransactionType"" = 6
                            AND st.""TransactionDateUtc"" >= @df
                            AND st.""TransactionDateUtc"" <  @dt), 0)::NUMERIC(18,2) AS ""ConsumedQuantity"""))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            if (await r.ReadAsync(ct))
            {
                totalItems    = I64(r, "TotalItems");
                lowStockItems = I64(r, "LowStockItems");
                totalOnHand   = Dec(r, "TotalOnHandQuantity");
                consumed      = Dec(r, "ConsumedQuantity");
            }
        }

        var lowStock = new List<LowStockInventoryItemReadModel>();
        await using (var cmd = Cmd(conn, @"
            SELECT i.""ItemId"", i.""ItemCode"", i.""ItemName"",
                   COALESCE(SUM(ws.""QuantityOnHand""), 0)::NUMERIC(18,2) AS ""QuantityOnHand"",
                   i.""ReorderLevel""::NUMERIC(18,2) AS ""ReorderLevel"",
                   GREATEST(0, i.""ReorderLevel"" - COALESCE(SUM(ws.""QuantityOnHand""), 0))::NUMERIC(18,2) AS ""ShortageQuantity""
            FROM public.""tblItem"" i
            LEFT JOIN public.""tblWarehouseStock"" ws
                   ON ws.""ItemId"" = i.""ItemId"" AND NOT COALESCE(ws.""IsDeleted"", FALSE)
            WHERE NOT COALESCE(i.""IsDeleted"", FALSE)
            GROUP BY i.""ItemId"", i.""ItemCode"", i.""ItemName"", i.""ReorderLevel""
            HAVING COALESCE(SUM(ws.""QuantityOnHand""), 0) <= i.""ReorderLevel""
            ORDER BY ""ShortageQuantity"" DESC"))
        {
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                lowStock.Add(new LowStockInventoryItemReadModel(
                    I64(r, "ItemId"), Str(r, "ItemCode"), Str(r, "ItemName"),
                    Dec(r, "QuantityOnHand"), Dec(r, "ReorderLevel"), Dec(r, "ShortageQuantity")));
        }

        var trends = new List<InventoryConsumptionTrendPointReadModel>();
        await using (var cmd = Cmd(conn, TrendSql("st.\"TransactionDateUtc\"",
            "public.\"tblStockTransaction\" st",
            "NOT COALESCE(st.\"IsDeleted\", FALSE) AND st.\"TransactionType\" = 6\n" +
            "              AND st.\"TransactionDateUtc\" >= @df AND st.\"TransactionDateUtc\" < @dt",
            "COALESCE(SUM(st.\"Quantity\"), 0)::NUMERIC(18,2)", valueAlias: "QuantityConsumed")))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt); P(cmd, "@trend", trend);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                trends.Add(new InventoryConsumptionTrendPointReadModel(
                    DO(r, "PeriodStartDate"), Str(r, "PeriodLabel"), Dec(r, "QuantityConsumed")));
        }

        return new InventoryAnalyticsReadModel(totalItems, lowStockItems, totalOnHand, consumed,
            lowStock, trends);
    }

    // =========================================================
    // Date-Range Report
    // =========================================================
    public async Task<DateRangeReportReadModel> GetReportByDateRangeAsync(
        AnalyticsQueryFilter filter, CancellationToken ct)
    {
        var conn  = await OpenAsync(ct);
        var df    = filter.DateFrom.ToDateTime(TimeOnly.MinValue);
        var dt    = filter.DateTo.ToDateTime(TimeOnly.MinValue);
        var trend = NormaliseTrend(filter.TrendBy);

        long totalBookings = 0, completedJobs = 0, supportTickets = 0, activeTech = 0, newCust = 0;
        decimal totalRevenue = 0;

        await using (var cmd = Cmd(conn, @"
            SELECT
                (SELECT COUNT(*)::BIGINT FROM public.""tblBooking"" b
                 WHERE NOT COALESCE(b.""IsDeleted"", FALSE)
                   AND b.""BookingDateUtc"" >= @df AND b.""BookingDateUtc"" < @dt) AS ""TotalBookings"",
                COALESCE((SELECT SUM(ih.""GrandTotalAmount"") FROM public.""tblInvoiceHeader"" ih
                          WHERE NOT COALESCE(ih.""IsDeleted"", FALSE)
                            AND ih.""InvoiceDateUtc"" >= @df AND ih.""InvoiceDateUtc"" < @dt), 0)::NUMERIC(18,2) AS ""TotalRevenue"",
                (SELECT COUNT(*)::BIGINT FROM public.""tblJobCard"" jc
                 WHERE NOT COALESCE(jc.""IsDeleted"", FALSE)
                   AND jc.""WorkCompletedDateUtc"" IS NOT NULL
                   AND jc.""WorkCompletedDateUtc"" >= @df AND jc.""WorkCompletedDateUtc"" < @dt) AS ""CompletedJobs"",
                (SELECT COUNT(*)::BIGINT FROM public.""tblSupportTicket"" st
                 WHERE NOT COALESCE(st.""IsDeleted"", FALSE)
                   AND st.""DateCreated"" >= @df AND st.""DateCreated"" < @dt) AS ""TotalSupportTickets"",
                (SELECT COUNT(*)::BIGINT FROM public.""tblTechnician"" t
                 WHERE NOT COALESCE(t.""IsDeleted"", FALSE) AND t.""IsActive"") AS ""ActiveTechnicians"",
                (SELECT COUNT(*)::BIGINT FROM public.""tblCustomer"" c
                 WHERE NOT COALESCE(c.""IsDeleted"", FALSE)
                   AND c.""DateCreated"" >= @df AND c.""DateCreated"" < @dt) AS ""NewCustomers"""))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            if (await r.ReadAsync(ct))
            {
                totalBookings = I64(r, "TotalBookings"); totalRevenue = Dec(r, "TotalRevenue");
                completedJobs = I64(r, "CompletedJobs"); supportTickets = I64(r, "TotalSupportTickets");
                activeTech    = I64(r, "ActiveTechnicians"); newCust = I64(r, "NewCustomers");
            }
        }

        var bookingTrends = new List<AnalyticsTrendPointReadModel>();
        await using (var cmd = Cmd(conn, TrendSql("b.\"BookingDateUtc\"", "public.\"tblBooking\" b",
            "NOT COALESCE(b.\"IsDeleted\", FALSE) AND b.\"BookingDateUtc\" >= @df AND b.\"BookingDateUtc\" < @dt",
            "COUNT(*)::NUMERIC(18,2)")))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt); P(cmd, "@trend", trend);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                bookingTrends.Add(new AnalyticsTrendPointReadModel(DO(r, "PeriodStartDate"), Str(r, "PeriodLabel"), Dec(r, "Value")));
        }

        var revenueTrends = new List<AnalyticsTrendPointReadModel>();
        await using (var cmd = Cmd(conn, TrendSql("ih.\"InvoiceDateUtc\"", "public.\"tblInvoiceHeader\" ih",
            "NOT COALESCE(ih.\"IsDeleted\", FALSE) AND ih.\"InvoiceDateUtc\" >= @df AND ih.\"InvoiceDateUtc\" < @dt",
            "COALESCE(SUM(ih.\"GrandTotalAmount\"), 0)::NUMERIC(18,2)")))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt); P(cmd, "@trend", trend);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                revenueTrends.Add(new AnalyticsTrendPointReadModel(DO(r, "PeriodStartDate"), Str(r, "PeriodLabel"), Dec(r, "Value")));
        }

        var supportDist = new List<AnalyticsBreakdownItemReadModel>();
        await using (var cmd = Cmd(conn, @"
            SELECT CASE st.""CurrentStatus""
                       WHEN 1 THEN 'Open'               WHEN 2 THEN 'In Progress'
                       WHEN 3 THEN 'Waiting For Customer' WHEN 4 THEN 'Customer Responded'
                       WHEN 5 THEN 'Escalated'          WHEN 6 THEN 'Resolved'
                       WHEN 7 THEN 'Closed'             WHEN 8 THEN 'Reopened'
                       ELSE 'Unknown' END AS ""Label"",
                   COUNT(*)::NUMERIC(18,2) AS ""Value""
            FROM public.""tblSupportTicket"" st
            WHERE NOT COALESCE(st.""IsDeleted"", FALSE)
              AND st.""DateCreated"" >= @df AND st.""DateCreated"" < @dt
            GROUP BY st.""CurrentStatus"" ORDER BY st.""CurrentStatus"""))
        {
            P(cmd, "@df", df); P(cmd, "@dt", dt);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) supportDist.Add(BD(r));
        }

        return new DateRangeReportReadModel(filter.DateFrom, filter.DateTo,
            totalBookings, totalRevenue, completedJobs, supportTickets, activeTech, newCust,
            bookingTrends, revenueTrends, supportDist);
    }

    // =========================================================
    // Infrastructure helpers
    // =========================================================

    private async Task<DbConnection> OpenAsync(CancellationToken ct)
    {
        var conn = _dbContext.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);
        return conn;
    }

    private static DbCommand Cmd(DbConnection conn, string sql)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        return cmd;
    }

    private static void P(DbCommand cmd, string name, object? value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(p);
    }

    private static string NormaliseTrend(string? t) =>
        t is "week" or "month" ? t : "day";

    // Builds a standard period-trend SELECT. valueAlias defaults to "Value".
    private static string TrendSql(string dateCol, string fromClause, string whereClause,
        string valueExpr, string valueAlias = "Value") =>
        $@"
        SELECT
            DATE_TRUNC(CASE @trend WHEN 'week' THEN 'week' WHEN 'month' THEN 'month' ELSE 'day' END,
                       {dateCol})::DATE AS ""PeriodStartDate"",
            CASE @trend
                WHEN 'week'  THEN TO_CHAR(DATE_TRUNC('week',  {dateCol}), 'DD Mon')
                WHEN 'month' THEN TO_CHAR(DATE_TRUNC('month', {dateCol}), 'Mon YYYY')
                ELSE              TO_CHAR({dateCol}::DATE, 'DD Mon')
            END AS ""PeriodLabel"",
            {valueExpr} AS ""{valueAlias}""
        FROM {fromClause}
        WHERE {whereClause}
        GROUP BY 1, 2 ORDER BY 1";

    // Reads a standard Label/Value breakdown row.
    private static AnalyticsBreakdownItemReadModel BD(DbDataReader r) =>
        new(Str(r, "Label"), Dec(r, "Value"));

    private static long I64(DbDataReader r, string col)
    {
        var ord = r.GetOrdinal(col);
        if (r.IsDBNull(ord)) return 0;
        return r.GetValue(ord) switch
        {
            long    l => l,
            int     i => i,
            short   s => s,
            decimal d => (long)d,
            var v     => Convert.ToInt64(v)
        };
    }

    private static decimal Dec(DbDataReader r, string col)
    {
        var ord = r.GetOrdinal(col);
        if (r.IsDBNull(ord)) return 0;
        return r.GetValue(ord) switch
        {
            decimal d => d,
            double  d => Convert.ToDecimal(d),
            float   f => Convert.ToDecimal(f),
            long    l => l,
            int     i => i,
            var v     => Convert.ToDecimal(v)
        };
    }

    private static string Str(DbDataReader r, string col)
    {
        var ord = r.GetOrdinal(col);
        return r.IsDBNull(ord) ? string.Empty : r.GetString(ord);
    }

    private static DateOnly DO(DbDataReader r, string col)
    {
        var ord = r.GetOrdinal(col);
        if (r.IsDBNull(ord)) return DateOnly.MinValue;
        return r.GetValue(ord) switch
        {
            DateOnly  d => d,
            DateTime  d => DateOnly.FromDateTime(d),
            var v       => DateOnly.FromDateTime(Convert.ToDateTime(v))
        };
    }
}
