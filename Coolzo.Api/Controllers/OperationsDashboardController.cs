using Coolzo.Application.Features.OperationsDashboard.Queries.GetDashboardSummary;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize(Policy = PermissionNames.OperationsDashboardRead)]
[Route("api/operations")]
public sealed class OperationsDashboardController : ApiControllerBase
{
    private readonly ISender _sender;

    public OperationsDashboardController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("dashboard-summary")]
    [ProducesResponseType(typeof(ApiResponse<OperationsDashboardSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OperationsDashboardSummaryResponse>>> GetDashboardSummaryAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetDashboardSummaryQuery(), cancellationToken);

        return Success(response);
    }
}
