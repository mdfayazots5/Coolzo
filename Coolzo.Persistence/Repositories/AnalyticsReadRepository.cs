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

    public AnalyticsReadRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryReadModel> GetDashboardSummaryAsync(CancellationToken cancellationToken)
    {
        await using var command = await CreateStoredProcedureCommandAsync("dbo.uspGetDashboardSummary", cancellationToken);
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

        var statusDistribution = new List<AnalyticsBreakdownItemReadModel>();

        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                statusDistribution.Add(new AnalyticsBreakdownItemReadModel(
                    GetString(reader, "Label"),
                    GetDecimal(reader, "Value")));
            }
        }

        return new DashboardSummaryReadModel(
            totalBookings,
            totalServiceRequests,
            totalJobs,
            totalRevenue,
            totalAmcCustomers,
            totalSupportTickets,
            statusDistribution);
    }

    public async Task<BookingAnalyticsReadModel> GetBookingAnalyticsAsync(
        AnalyticsQueryFilter filter,
        int? bookingStatus,
        CancellationToken cancellationToken)
    {
        await using var command = await CreateStoredProcedureCommandAsync("dbo.uspGetBookingAnalytics", cancellationToken);
        AddDateParameters(command, filter);
        AddParameter(command, "@ServiceId", filter.ServiceId ?? 0L);
        AddParameter(command, "@Status", bookingStatus ?? 0);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        long totalBookings = 0;
        long pendingBookings = 0;
        long confirmedBookings = 0;
        long cancelledBookings = 0;
        decimal averageBookingsPerPeriod = 0;

        if (await reader.ReadAsync(cancellationToken))
        {
            totalBookings = GetInt64(reader, "TotalBookings");
            pendingBookings = GetInt64(reader, "PendingBookings");
            confirmedBookings = GetInt64(reader, "ConfirmedBookings");
            cancelledBookings = GetInt64(reader, "CancelledBookings");
            averageBookingsPerPeriod = GetDecimal(reader, "AverageBookingsPerPeriod");
        }

        var trends = await ReadTrendPointsAsync(reader, cancellationToken);
        var statusDistribution = await ReadBreakdownItemsAsync(reader, cancellationToken);
        var serviceDistribution = await ReadBreakdownItemsAsync(reader, cancellationToken);

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
        await using var command = await CreateStoredProcedureCommandAsync("dbo.uspGetRevenueAnalytics", cancellationToken);
        AddDateParameters(command, filter);
        AddParameter(command, "@ServiceId", filter.ServiceId ?? 0L);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        decimal totalRevenue = 0;
        decimal paidRevenue = 0;
        decimal outstandingRevenue = 0;
        long invoiceCount = 0;
        decimal averageInvoiceValue = 0;

        if (await reader.ReadAsync(cancellationToken))
        {
            totalRevenue = GetDecimal(reader, "TotalRevenue");
            paidRevenue = GetDecimal(reader, "PaidRevenue");
            outstandingRevenue = GetDecimal(reader, "OutstandingRevenue");
            invoiceCount = GetInt64(reader, "InvoiceCount");
            averageInvoiceValue = GetDecimal(reader, "AverageInvoiceValue");
        }

        var trends = await ReadTrendPointsAsync(reader, cancellationToken);
        var revenueByService = await ReadBreakdownItemsAsync(reader, cancellationToken);
        var revenueByCustomerSegment = await ReadBreakdownItemsAsync(reader, cancellationToken);

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
        await using var command = await CreateStoredProcedureCommandAsync("dbo.uspGetTechnicianPerformance", cancellationToken);
        AddDateParameters(command, filter);
        AddParameter(command, "@TechnicianId", filter.TechnicianId ?? 0L);
        AddParameter(command, "@Status", serviceRequestStatus ?? 0);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        long totalTechnicians = 0;
        long activeTechnicians = 0;
        long totalAssignedJobs = 0;
        long totalCompletedJobs = 0;
        decimal averageCompletionHours = 0;

        if (await reader.ReadAsync(cancellationToken))
        {
            totalTechnicians = GetInt64(reader, "TotalTechnicians");
            activeTechnicians = GetInt64(reader, "ActiveTechnicians");
            totalAssignedJobs = GetInt64(reader, "TotalAssignedJobs");
            totalCompletedJobs = GetInt64(reader, "TotalCompletedJobs");
            averageCompletionHours = GetDecimal(reader, "AverageCompletionHours");
        }

        var technicians = new List<TechnicianPerformanceItemReadModel>();

        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                technicians.Add(new TechnicianPerformanceItemReadModel(
                    GetInt64(reader, "TechnicianId"),
                    GetString(reader, "TechnicianCode"),
                    GetString(reader, "TechnicianName"),
                    GetInt64(reader, "JobsAssigned"),
                    GetInt64(reader, "JobsCompleted"),
                    GetDecimal(reader, "CompletionRatePercentage"),
                    GetDecimal(reader, "AverageCompletionHours"),
                    GetInt64(reader, "CurrentWorkload")));
            }
        }

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
        await using var command = await CreateStoredProcedureCommandAsync("dbo.uspGetCustomerAnalytics", cancellationToken);
        AddDateParameters(command, filter);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        long totalCustomers = 0;
        long newCustomers = 0;
        long returningCustomers = 0;
        long repeatCustomers = 0;
        long amcCustomers = 0;
        long nonAmcCustomers = 0;
        decimal repeatRatePercentage = 0;

        if (await reader.ReadAsync(cancellationToken))
        {
            totalCustomers = GetInt64(reader, "TotalCustomers");
            newCustomers = GetInt64(reader, "NewCustomers");
            returningCustomers = GetInt64(reader, "ReturningCustomers");
            repeatCustomers = GetInt64(reader, "RepeatCustomers");
            amcCustomers = GetInt64(reader, "AmcCustomers");
            nonAmcCustomers = GetInt64(reader, "NonAmcCustomers");
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
                    GetString(reader, "PeriodLabel"),
                    GetInt64(reader, "NewCustomers"),
                    GetInt64(reader, "ReturningCustomers")));
            }
        }

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
        await using var command = await CreateStoredProcedureCommandAsync("dbo.uspGetSupportAnalytics", cancellationToken);
        AddDateParameters(command, filter);
        AddParameter(command, "@Status", supportTicketStatus ?? 0);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        long totalTickets = 0;
        long openTickets = 0;
        long resolvedTickets = 0;
        long escalationCount = 0;
        decimal averageResolutionHours = 0;

        if (await reader.ReadAsync(cancellationToken))
        {
            totalTickets = GetInt64(reader, "TotalTickets");
            openTickets = GetInt64(reader, "OpenTickets");
            resolvedTickets = GetInt64(reader, "ResolvedTickets");
            escalationCount = GetInt64(reader, "EscalationCount");
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
                    GetString(reader, "PeriodLabel"),
                    GetInt64(reader, "ResolvedTickets"),
                    GetDecimal(reader, "AverageResolutionHours")));
            }
        }

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
        await using var command = await CreateStoredProcedureCommandAsync("dbo.uspGetInventoryAnalytics", cancellationToken);
        AddDateParameters(command, filter);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        long totalItems = 0;
        long lowStockItems = 0;
        decimal totalOnHandQuantity = 0;
        decimal consumedQuantity = 0;

        if (await reader.ReadAsync(cancellationToken))
        {
            totalItems = GetInt64(reader, "TotalItems");
            lowStockItems = GetInt64(reader, "LowStockItems");
            totalOnHandQuantity = GetDecimal(reader, "TotalOnHandQuantity");
            consumedQuantity = GetDecimal(reader, "ConsumedQuantity");
        }

        var lowStockSummaries = new List<LowStockInventoryItemReadModel>();

        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                lowStockSummaries.Add(new LowStockInventoryItemReadModel(
                    GetInt64(reader, "ItemId"),
                    GetString(reader, "ItemCode"),
                    GetString(reader, "ItemName"),
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
                    GetString(reader, "PeriodLabel"),
                    GetDecimal(reader, "QuantityConsumed")));
            }
        }

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
        await using var command = await CreateStoredProcedureCommandAsync("dbo.uspGetReportByDateRange", cancellationToken);
        AddDateParameters(command, filter);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        long totalBookings = 0;
        decimal totalRevenue = 0;
        long completedJobs = 0;
        long totalSupportTickets = 0;
        long activeTechnicians = 0;
        long newCustomers = 0;

        if (await reader.ReadAsync(cancellationToken))
        {
            totalBookings = GetInt64(reader, "TotalBookings");
            totalRevenue = GetDecimal(reader, "TotalRevenue");
            completedJobs = GetInt64(reader, "CompletedJobs");
            totalSupportTickets = GetInt64(reader, "TotalSupportTickets");
            activeTechnicians = GetInt64(reader, "ActiveTechnicians");
            newCustomers = GetInt64(reader, "NewCustomers");
        }

        var bookingTrends = await ReadTrendPointsAsync(reader, cancellationToken);
        var revenueTrends = await ReadTrendPointsAsync(reader, cancellationToken);
        var supportStatusDistribution = await ReadBreakdownItemsAsync(reader, cancellationToken);

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

    private async Task<DbCommand> CreateStoredProcedureCommandAsync(string procedureName, CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

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
        {
            return items;
        }

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new AnalyticsTrendPointReadModel(
                GetDateOnly(reader, "PeriodStartDate"),
                GetString(reader, "PeriodLabel"),
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
        {
            return items;
        }

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new AnalyticsBreakdownItemReadModel(
                GetString(reader, "Label"),
                GetDecimal(reader, "Value")));
        }

        return items;
    }

    private static void AddDateParameters(DbCommand command, AnalyticsQueryFilter filter)
    {
        AddParameter(command, "@DateFrom", filter.DateFrom.ToDateTime(TimeOnly.MinValue));
        AddParameter(command, "@DateTo", filter.DateTo.ToDateTime(TimeOnly.MinValue));
        AddParameter(command, "@TrendBy", filter.TrendBy);
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

        if (reader.IsDBNull(ordinal))
        {
            return 0;
        }

        var value = reader.GetValue(ordinal);

        return value switch
        {
            long longValue => longValue,
            int intValue => intValue,
            short shortValue => shortValue,
            decimal decimalValue => (long)decimalValue,
            _ => Convert.ToInt64(value)
        };
    }

    private static decimal GetDecimal(DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);

        if (reader.IsDBNull(ordinal))
        {
            return 0;
        }

        var value = reader.GetValue(ordinal);

        return value switch
        {
            decimal decimalValue => decimalValue,
            double doubleValue => Convert.ToDecimal(doubleValue),
            float floatValue => Convert.ToDecimal(floatValue),
            long longValue => longValue,
            int intValue => intValue,
            _ => Convert.ToDecimal(value)
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

        if (reader.IsDBNull(ordinal))
        {
            return DateOnly.MinValue;
        }

        var value = reader.GetValue(ordinal);

        return value switch
        {
            DateOnly dateOnlyValue => dateOnlyValue,
            DateTime dateTimeValue => DateOnly.FromDateTime(dateTimeValue),
            _ => DateOnly.FromDateTime(Convert.ToDateTime(value))
        };
    }
}
