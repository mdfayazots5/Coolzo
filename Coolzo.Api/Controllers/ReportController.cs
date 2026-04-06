using Asp.Versioning;
using Coolzo.Application.Features.Reporting.Queries.GetReportByDateRange;
using Coolzo.Application.Features.Reporting.Queries.GetReportExport;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Analytics;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize(Policy = PermissionNames.ReportRead)]
[Route("api/v{version:apiVersion}/reports")]
public sealed class ReportController : ApiControllerBase
{
    private readonly ISender _sender;

    public ReportController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("date-range")]
    [ProducesResponseType(typeof(ApiResponse<DateRangeReportResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DateRangeReportResponse>>> GetDateRangeAsync(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] string? trendBy,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetReportByDateRangeQuery(dateFrom, dateTo, trendBy),
            cancellationToken);

        return Success(response);
    }

    [HttpGet("export")]
    [ProducesResponseType(typeof(ApiResponse<ReportExportResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ReportExportResponse>>> ExportAsync(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] string? trendBy,
        [FromQuery] string? format,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetReportExportQuery(dateFrom, dateTo, trendBy, format),
            cancellationToken);

        return Success(response);
    }
}

