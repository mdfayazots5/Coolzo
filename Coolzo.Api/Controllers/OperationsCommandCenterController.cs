using Coolzo.Application.Features.OperationsDashboard;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize(Policy = PermissionNames.OperationsDashboardRead)]
[Route("api/dashboard")]
public sealed class OperationsCommandCenterController : ApiControllerBase
{
    private readonly ISender _sender;

    public OperationsCommandCenterController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("operations")]
    [ProducesResponseType(typeof(ApiResponse<OperationsDashboardResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OperationsDashboardResponse>>> GetOperationsDashboardAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetOperationsDashboardQuery(), cancellationToken);

        return Success(response);
    }

    [HttpGet("operations/pending-queue")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<OperationsPendingQueueItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<OperationsPendingQueueItemResponse>>>> GetPendingQueueAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetOperationsPendingQueueQuery(), cancellationToken);

        return Success(response);
    }

    [HttpGet("operations/technician-status")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<OperationsTechnicianStatusItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<OperationsTechnicianStatusItemResponse>>>> GetTechnicianStatusAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetOperationsTechnicianStatusQuery(), cancellationToken);

        return Success(response);
    }

    [HttpGet("operations/sla-alerts")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<OperationsSlaAlertItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<OperationsSlaAlertItemResponse>>>> GetSlaAlertsAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetOperationsSlaAlertsQuery(), cancellationToken);

        return Success(response);
    }

    [HttpGet("operations/zone-workload")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<OperationsZoneWorkloadItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<OperationsZoneWorkloadItemResponse>>>> GetZoneWorkloadAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetOperationsZoneWorkloadQuery(), cancellationToken);

        return Success(response);
    }

    [HttpGet("operations/day-summary")]
    [ProducesResponseType(typeof(ApiResponse<OperationsDaySummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OperationsDaySummaryResponse>>> GetDaySummaryAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetOperationsDaySummaryQuery(), cancellationToken);

        return Success(response);
    }

    [HttpGet("live-map")]
    [ProducesResponseType(typeof(ApiResponse<OperationsLiveMapResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OperationsLiveMapResponse>>> GetLiveMapAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetOperationsLiveMapQuery(), cancellationToken);

        return Success(response);
    }
}
