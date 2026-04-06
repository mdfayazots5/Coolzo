using Asp.Versioning;
using Coolzo.Application.Features.Analytics.Queries.GetBookingAnalytics;
using Coolzo.Application.Features.Analytics.Queries.GetCustomerAnalytics;
using Coolzo.Application.Features.Analytics.Queries.GetInventoryAnalytics;
using Coolzo.Application.Features.Analytics.Queries.GetRevenueAnalytics;
using Coolzo.Application.Features.Analytics.Queries.GetSupportAnalytics;
using Coolzo.Application.Features.Analytics.Queries.GetTechnicianPerformance;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Analytics;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize(Policy = PermissionNames.AnalyticsRead)]
[Route("api/v{version:apiVersion}/analytics")]
public sealed class AnalyticsController : ApiControllerBase
{
    private readonly ISender _sender;

    public AnalyticsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("bookings")]
    [ProducesResponseType(typeof(ApiResponse<BookingAnalyticsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BookingAnalyticsResponse>>> GetBookingsAsync(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] string? trendBy,
        [FromQuery] long? serviceId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetBookingAnalyticsQuery(dateFrom, dateTo, trendBy, serviceId, status),
            cancellationToken);

        return Success(response);
    }

    [HttpGet("revenue")]
    [ProducesResponseType(typeof(ApiResponse<RevenueAnalyticsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RevenueAnalyticsResponse>>> GetRevenueAsync(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] string? trendBy,
        [FromQuery] long? serviceId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetRevenueAnalyticsQuery(dateFrom, dateTo, trendBy, serviceId),
            cancellationToken);

        return Success(response);
    }

    [HttpGet("technicians")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianPerformanceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianPerformanceResponse>>> GetTechniciansAsync(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] string? trendBy,
        [FromQuery] long? technicianId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetTechnicianPerformanceQuery(dateFrom, dateTo, trendBy, technicianId, status),
            cancellationToken);

        return Success(response);
    }

    [HttpGet("customers")]
    [ProducesResponseType(typeof(ApiResponse<CustomerAnalyticsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerAnalyticsResponse>>> GetCustomersAsync(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] string? trendBy,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetCustomerAnalyticsQuery(dateFrom, dateTo, trendBy),
            cancellationToken);

        return Success(response);
    }

    [HttpGet("support")]
    [ProducesResponseType(typeof(ApiResponse<SupportAnalyticsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupportAnalyticsResponse>>> GetSupportAsync(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] string? trendBy,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetSupportAnalyticsQuery(dateFrom, dateTo, trendBy, status),
            cancellationToken);

        return Success(response);
    }

    [HttpGet("inventory")]
    [ProducesResponseType(typeof(ApiResponse<InventoryAnalyticsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InventoryAnalyticsResponse>>> GetInventoryAsync(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] string? trendBy,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetInventoryAnalyticsQuery(dateFrom, dateTo, trendBy),
            cancellationToken);

        return Success(response);
    }
}

