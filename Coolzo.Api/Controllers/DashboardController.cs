using Asp.Versioning;
using Coolzo.Application.Features.Dashboard.Queries.GetDashboardMetrics;
using Coolzo.Application.Features.Dashboard.Queries.GetDashboardSummary;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Analytics;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize(Policy = PermissionNames.DashboardRead)]
[Route("api/v{version:apiVersion}/dashboard")]
public sealed class DashboardController : ApiControllerBase
{
    private readonly ISender _sender;

    public DashboardController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardSummaryResponse>>> GetSummaryAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetDashboardSummaryQuery(), cancellationToken);

        return Success(response);
    }

    [HttpGet("metrics")]
    [ProducesResponseType(typeof(ApiResponse<DashboardMetricsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardMetricsResponse>>> GetMetricsAsync(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] string? trendBy,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetDashboardMetricsQuery(dateFrom, dateTo, trendBy),
            cancellationToken);

        return Success(response);
    }
}

