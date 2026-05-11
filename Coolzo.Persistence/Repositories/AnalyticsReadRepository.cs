using System.Data;
using System.Data.Common;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace Coolzo.Persistence.Repositories;

public sealed class AnalyticsReadRepository : IAnalyticsReadRepository
{
    private readonly CoolzoDbContext _dbContext;

    public AnalyticsReadRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryReadModel> GetDashboardSummaryAsync(CancellationToken cancellationToken)
    {
        // Npgsql 8: CommandType.StoredProcedure generates CALL (not SELECT * FROM).
        // Dashboard uses a PostgreSQL FUNCTION, so it must be called via CommandType.Text.
        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM public.uspgetdashboardsummary()";
        command.CommandType = System.Data.CommandType.Text;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        long totalBookings = 0;
        long totalServiceRequests = 0;
        long totalJobs = 0;
        decimal totalRevenue = 0;
        long totalAmcCustomers = 0;
        long totalSupportTickets = 0;

        if (await reader.ReadAsync(cancellationToken))
        {
            totalBookings = GetInt64(reader, "TotalBookings");
            totalServiceRequests = GetInt64(reader, "TotalServiceRequests");
            totalJobs = GetInt64(reader, "TotalJobs");
            totalRevenue = GetDecimal(reader, "TotalRevenue");
            totalAmcCustomers = GetInt64(reader, "TotalAmcCustomers");
            totalSupportTickets = GetInt64(reader, "TotalSupportTickets");
        }

        return new DashboardSummaryReadModel(
            totalBookings,
            totalServiceRequests,
            totalJobs,
            totalRevenue,
            totalAmcCustomers,
            totalSupportTickets,
            Array.Empty<AnalyticsBreakdownItemReadModel>());
    }

    public async Task<BookingAnalyticsReadModel> GetBookingAnalyticsAsync(
        AnalyticsQueryFilter filter,
        int? bookingStatus,
        CancellationToken cancellationToken)
    {
        await using var conn = await OpenNpgsqlConnectionAsync(cancellationToken);
        await using var tx   = await conn.BeginTransactionAsync(cancellationToken);
        await using var cmd  = BuildAnalyticsCommand("dbo.uspGetBookingAnalytics", conn, tx);

        AddDateParameters(cmd, filter);
        AddParameter(cmd, "@ServiceId", filter.ServiceId ?? 0L);
        AddParameter(cmd, "@Status",    bookingStatus ?? 0);
        AddRefCursors(cmd, 4);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        long totalBookings = 0, pendingBookings = 0, confirmedBookings = 0, cancelledBookings = 0;
        decimal averageBookingsPerPeriod = 0;

        if (await reader.ReadAsync(cancellationToken))
        {
            totalBookings            = GetInt64(reader,   "TotalBookings");
            pendingBookings          = GetInt64(reader,   "PendingBookings");
            confirmedBookings        = GetInt64(reader,   "ConfirmedBookings");
            cancelledBookings        = GetInt64(reader,   "CancelledBookings");
            averageBookingsPerPeriod = GetDecimal(reader, "AverageBookingsPerPeriod");
        }

        var trends             = await ReadTrendPointsAsync(reader, cancellationToken);
        var statusDistribution = await ReadBreakdownItemsAsync(reader, cancellationToken);
        var serviceDistribution = await ReadBreakdownItemsAsync(reader, cancellationToken);

        await tx.CommitAsync(cancellationToken);

        return new BookingAnalyticsReadModel(
            totalBookings,
            pendingBookings,
            confirmedBookings,
            cancelledBookings,
            averageBookingsPerPeriod,
            trends,
            statusDistribution,
            serviceDistribution);
    }

    public async Task<RevenueAnalyticsReadModel> GetRevenueAnalyticsAsync(
        AnalyticsQueryFilter filter,
        CancellationToken cancellationToken)
    {
        await using var conn = await OpenNpgsqlConnectionAsync(cancellationToken);
        await using var tx   = await conn.BeginTransactionAsync(cancellationToken);
        await using var cmd  = BuildAnalyticsCommand("dbo.uspGetRevenueAnalytics", conn, tx);

        AddDateParameters(cmd, filter);
        AddParameter(cmd, "@ServiceId", filter.ServiceId ?? 0L);
        AddRefCursors(cmd, 4);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        decimal totalRevenue = 0, paidRevenue = 0, outstandingRevenue = 0, averageInvoiceValue = 0;
        long invoiceCount = 0;

        if (await reader.ReadAsync(cancellationToken))
        {
            totalRevenue       = GetDecimal(reader, "TotalRevenue");
            paidRevenue        = GetDecimal(reader, "PaidRevenue");
            outstandingRevenue = GetDecimal(reader, "OutstandingRevenue");
            invoiceCount       = GetInt64(reader,   "InvoiceCount");
            averageInvoiceValue = GetDecimal(reader, "AverageInvoiceValue");
        }

        var trends                   = await ReadTrendPointsAsync(reader, cancellationToken);
        var revenueByService         = await ReadBreakdownItemsAsync(reader, cancellationToken);
        var revenueByCustomerSegment = await ReadBreakdownItemsAsync(reader, cancellationToken);

        await tx.CommitAsync(cancellationToken);

        return new RevenueAnalyticsReadModel(
            totalRevenue,
            paidRevenue,
            outstandingRevenue,
            invoiceCount,
            averageInvoiceValue,
            trends,
            revenueByService,
            revenueByCustomerSegment);
    }

    public async Task<TechnicianPerformanceReadModel> GetTechnicianPerformanceAsync(
        AnalyticsQueryFilter filter,
        int? serviceRequestStatus,
        CancellationToken cancellationToken)
    {
        await using var conn = await OpenNpgsqlConnectionAsync(cancellationToken);
        await using var tx   = await conn.BeginTransactionAsync(cancellationToken);
        await using var cmd  = BuildAnalyticsCommand("dbo.uspGetTechnicianPerformance", conn, tx);

        AddDateParameters(cmd, filter);
        AddParameter(cmd, "@TechnicianId", filter.TechnicianId ?? 0L);
        AddParameter(cmd, "@Status",       serviceRequestStatus ?? 0);
        AddRefCursors(cmd, 2);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        long totalTechnicians = 0, activeTechnicians = 0, totalAssignedJobs = 0, totalCompletedJobs = 0;
        decimal averageCompletionHours = 0;

        if (await reader.ReadAsync(cancellationToken))
        {
            totalTechnicians      = GetInt64(reader,   "TotalTechnicians");
            activeTechnicians     = GetInt64(reader,   "ActiveTechnicians");
            totalAssignedJobs     = GetInt64(reader,   "TotalAssignedJobs");
            totalCompletedJobs    = GetInt64(reader,   "TotalCompletedJobs");
            averageCompletionHours = GetDecimal(reader, "AverageCompletionHours");
        }

        var technicians = new List<TechnicianPerformanceItemReadModel>();

        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                technicians.Add(new TechnicianPerformanceItemReadModel(
                    GetInt64(reader,   "TechnicianId"),
                    GetString(reader,  "TechnicianCode"),
                    GetString(reader,  "TechnicianName"),
                    GetInt64(reader,   "JobsAssigned"),
                    GetInt64(reader,   "JobsCompleted"),
                    GetDecimal(reader, "CompletionRatePercentage"),
                    GetDecimal(reader, "AverageCompletionHours"),
                    GetInt64(reader,   "CurrentWorkload")));
            }
        }

        await tx.CommitAsync(cancellationToken);

        return new TechnicianPerformanceReadModel(
            totalTechnicians,
            activeTechnicians,
            totalAssignedJobs,
            totalCompletedJobs,
            averageCompletionHours,
            technicians);
    }

    public async Task<CustomerAnalyticsReadModel> GetCustomerAnalyticsAsync(
        AnalyticsQueryFilter filter,
        CancellationToken cancellationToken)
    {
        await using var conn = await OpenNpgsqlConnectionAsync(cancellationToken);
        await using var tx   = await conn.BeginTransactionAsync(cancellationToken);
        await using var cmd  = BuildAnalyticsCommand("dbo.uspGetCustomerAnalytics", conn, tx);

        AddDateParameters(cmd, filter);
        AddRefCursors(cmd, 3);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        long totalCustomers = 0, newCustomers = 0, returningCustomers = 0;
        long repeatCustomers = 0, amcCustomers = 0, nonAmcCustomers = 0;
        decimal repeatRatePercentage = 0;

        if (await reader.ReadAsync(cancellationToken))
        {
            totalCustomers      = GetInt64(reader,   "TotalCustomers");
            newCustomers        = GetInt64(reader,   "NewCustomers");
            returningCustomers  = GetInt64(reader,   "ReturningCustomers");
            repeatCustomers     = GetInt64(reader,   "RepeatCustomers");
            amcCustomers        = GetInt64(reader,   "AmcCustomers");
            nonAmcCustomers     = GetInt64(reader,   "NonAmcCustomers");
            repeatRatePercentage = GetDecimal(reader, "RepeatRatePercentage");
        }

        var segmentDistribution = await ReadBreakdownItemsAsync(reader, cancellationToken);
        var trends = new List<CustomerTrendPointReadModel>();

        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                trends.Add(new CustomerTrendPointReadModel(
                    GetDateOnly(reader, "PeriodStartDate"),
                    GetString(reader,  "PeriodLabel"),
                    GetInt64(reader,   "NewCustomers"),
                    GetInt64(reader,   "ReturningCustomers")));
            }
        }

        await tx.CommitAsync(cancellationToken);

        return new CustomerAnalyticsReadModel(
            totalCustomers,
            newCustomers,
            returningCustomers,
            repeatCustomers,
            amcCustomers,
            nonAmcCustomers,
            repeatRatePercentage,
            segmentDistribution,
            trends);
    }

    public async Task<SupportAnalyticsReadModel> GetSupportAnalyticsAsync(
        AnalyticsQueryFilter filter,
        int? supportTicketStatus,
        CancellationToken cancellationToken)
    {
        await using var conn = await OpenNpgsqlConnectionAsync(cancellationToken);
        await using var tx   = await conn.BeginTransactionAsync(cancellationToken);
        await using var cmd  = BuildAnalyticsCommand("dbo.uspGetSupportAnalytics", conn, tx);

        AddDateParameters(cmd, filter);
        AddParameter(cmd, "@Status", supportTicketStatus ?? 0);
        AddRefCursors(cmd, 3);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        long totalTickets = 0, openTickets = 0, resolvedTickets = 0, escalationCount = 0;
        decimal averageResolutionHours = 0;

        if (await reader.ReadAsync(cancellationToken))
        {
            totalTickets           = GetInt64(reader,   "TotalTickets");
            openTickets            = GetInt64(reader,   "OpenTickets");
            resolvedTickets        = GetInt64(reader,   "ResolvedTickets");
            escalationCount        = GetInt64(reader,   "EscalationCount");
            averageResolutionHours = GetDecimal(reader, "AverageResolutionHours");
        }

        var statusDistribution = await ReadBreakdownItemsAsync(reader, cancellationToken);
        var trends = new List<SupportResolutionTrendPointReadModel>();

        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                trends.Add(new SupportResolutionTrendPointReadModel(
                    GetDateOnly(reader, "PeriodStartDate"),
                    GetString(reader,  "PeriodLabel"),
                    GetInt64(reader,   "ResolvedTickets"),
                    GetDecimal(reader, "AverageResolutionHours")));
            }
        }

        await tx.CommitAsync(cancellationToken);

        return new SupportAnalyticsReadModel(
            totalTickets,
            openTickets,
            resolvedTickets,
            escalationCount,
            averageResolutionHours,
            statusDistribution,
            trends);
    }

    public async Task<InventoryAnalyticsReadModel> GetInventoryAnalyticsAsync(
        AnalyticsQueryFilter filter,
        CancellationToken cancellationToken)
    {
        await using var conn = await OpenNpgsqlConnectionAsync(cancellationToken);
        await using var tx   = await conn.BeginTransactionAsync(cancellationToken);
        await using var cmd  = BuildAnalyticsCommand("dbo.uspGetInventoryAnalytics", conn, tx);

        AddDateParameters(cmd, filter);
        AddRefCursors(cmd, 3);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        long totalItems = 0, lowStockItems = 0;
        decimal totalOnHandQuantity = 0, consumedQuantity = 0;

        if (await reader.ReadAsync(cancellationToken))
        {
            totalItems          = GetInt64(reader,   "TotalItems");
            lowStockItems       = GetInt64(reader,   "LowStockItems");
            totalOnHandQuantity = GetDecimal(reader, "TotalOnHandQuantity");
            consumedQuantity    = GetDecimal(reader, "ConsumedQuantity");
        }

        var lowStockSummaries = new List<LowStockInventoryItemReadModel>();

        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                lowStockSummaries.Add(new LowStockInventoryItemReadModel(
                    GetInt64(reader,   "ItemId"),
                    GetString(reader,  "ItemCode"),
                    GetString(reader,  "ItemName"),
                    GetDecimal(reader, "QuantityOnHand"),
                    GetDecimal(reader, "ReorderLevel"),
                    GetDecimal(reader, "ShortageQuantity")));
            }
        }

        var trends = new List<InventoryConsumptionTrendPointReadModel>();

        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                trends.Add(new InventoryConsumptionTrendPointReadModel(
                    GetDateOnly(reader, "PeriodStartDate"),
                    GetString(reader,  "PeriodLabel"),
                    GetDecimal(reader, "QuantityConsumed")));
            }
        }

        await tx.CommitAsync(cancellationToken);

        return new InventoryAnalyticsReadModel(
            totalItems,
            lowStockItems,
            totalOnHandQuantity,
            consumedQuantity,
            lowStockSummaries,
            trends);
    }

    public async Task<DateRangeReportReadModel> GetReportByDateRangeAsync(
        AnalyticsQueryFilter filter,
        CancellationToken cancellationToken)
    {
        await using var conn = await OpenNpgsqlConnectionAsync(cancellationToken);
        await using var tx   = await conn.BeginTransactionAsync(cancellationToken);
        await using var cmd  = BuildAnalyticsCommand("dbo.uspGetReportByDateRange", conn, tx);

        AddDateParameters(cmd, filter);
        AddRefCursors(cmd, 4);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        long totalBookings = 0, completedJobs = 0, totalSupportTickets = 0;
        long activeTechnicians = 0, newCustomers = 0;
        decimal totalRevenue = 0;

        if (await reader.ReadAsync(cancellationToken))
        {
            totalBookings        = GetInt64(reader,   "TotalBookings");
            totalRevenue         = GetDecimal(reader, "TotalRevenue");
            completedJobs        = GetInt64(reader,   "CompletedJobs");
            totalSupportTickets  = GetInt64(reader,   "TotalSupportTickets");
            activeTechnicians    = GetInt64(reader,   "ActiveTechnicians");
            newCustomers         = GetInt64(reader,   "NewCustomers");
        }

        var bookingTrends            = await ReadTrendPointsAsync(reader, cancellationToken);
        var revenueTrends            = await ReadTrendPointsAsync(reader, cancellationToken);
        var supportStatusDistribution = await ReadBreakdownItemsAsync(reader, cancellationToken);

        await tx.CommitAsync(cancellationToken);

        return new DateRangeReportReadModel(
            filter.DateFrom,
            filter.DateTo,
            totalBookings,
            totalRevenue,
            completedJobs,
            totalSupportTickets,
            activeTechnicians,
            newCustomers,
            bookingTrends,
            revenueTrends,
            supportStatusDistribution);
    }

    // ----------------------------------------------------------
    // Infrastructure helpers
    // ----------------------------------------------------------

    private async Task<NpgsqlConnection> OpenNpgsqlConnectionAsync(CancellationToken cancellationToken)
    {
        var conn = (NpgsqlConnection)_dbContext.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);
        return conn;
    }

    private static NpgsqlCommand BuildAnalyticsCommand(
        string procedureName,
        NpgsqlConnection conn,
        NpgsqlTransaction tx)
    {
        var cmd = new NpgsqlCommand(procedureName, conn, tx);
        cmd.CommandType = CommandType.StoredProcedure;
        return cmd;
    }

    // Adds INOUT REFCURSOR parameters (ref1, ref2, …) so that Npgsql
    // automatically fetches each cursor as a separate result set.
    private static void AddRefCursors(NpgsqlCommand cmd, int count)
    {
        for (var i = 1; i <= count; i++)
        {
            cmd.Parameters.Add(new NpgsqlParameter
            {
                ParameterName = $"ref{i}",
                NpgsqlDbType  = NpgsqlDbType.Refcursor,
                Direction     = ParameterDirection.InputOutput,
                Value         = $"cursor_{i}_{Guid.NewGuid():N}"
            });
        }
    }

    private async Task<DbCommand> CreateStoredProcedureCommandAsync(
        string procedureName,
        CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = procedureName;
        command.CommandType = CommandType.StoredProcedure;
        return command;
    }

    private static async Task<IReadOnlyCollection<AnalyticsTrendPointReadModel>> ReadTrendPointsAsync(
        DbDataReader reader,
        CancellationToken cancellationToken)
    {
        var items = new List<AnalyticsTrendPointReadModel>();

        if (!await reader.NextResultAsync(cancellationToken))
            return items;

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new AnalyticsTrendPointReadModel(
                GetDateOnly(reader, "PeriodStartDate"),
                GetString(reader,  "PeriodLabel"),
                GetDecimal(reader, "Value")));
        }

        return items;
    }

    private static async Task<IReadOnlyCollection<AnalyticsBreakdownItemReadModel>> ReadBreakdownItemsAsync(
        DbDataReader reader,
        CancellationToken cancellationToken)
    {
        var items = new List<AnalyticsBreakdownItemReadModel>();

        if (!await reader.NextResultAsync(cancellationToken))
            return items;

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new AnalyticsBreakdownItemReadModel(
                GetString(reader,  "Label"),
                GetDecimal(reader, "Value")));
        }

        return items;
    }

    private static void AddDateParameters(DbCommand command, AnalyticsQueryFilter filter)
    {
        AddParameter(command, "@DateFrom", filter.DateFrom.ToDateTime(TimeOnly.MinValue));
        AddParameter(command, "@DateTo",   filter.DateTo.ToDateTime(TimeOnly.MinValue));
        AddParameter(command, "@TrendBy",  filter.TrendBy);
    }

    private static void AddParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private static long GetInt64(DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal)) return 0;
        var value = reader.GetValue(ordinal);
        return value switch
        {
            long    l => l,
            int     i => i,
            short   s => s,
            decimal d => (long)d,
            _         => Convert.ToInt64(value)
        };
    }

    private static decimal GetDecimal(DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal)) return 0;
        var value = reader.GetValue(ordinal);
        return value switch
        {
            decimal d => d,
            double  d => Convert.ToDecimal(d),
            float   f => Convert.ToDecimal(f),
            long    l => l,
            int     i => i,
            _         => Convert.ToDecimal(value)
        };
    }

    private static string GetString(DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static DateOnly GetDateOnly(DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal)) return DateOnly.MinValue;
        var value = reader.GetValue(ordinal);
        return value switch
        {
            DateOnly  d => d,
            DateTime  d => DateOnly.FromDateTime(d),
            _           => DateOnly.FromDateTime(Convert.ToDateTime(value))
        };
    }
}
