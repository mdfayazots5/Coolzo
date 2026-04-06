using Asp.Versioning;
using Coolzo.Application.Features.Inventory.Commands.ConsumeJobParts;
using Coolzo.Application.Features.Inventory.Queries.GetJobConsumption;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Inventory;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/jobs")]
public sealed class JobConsumptionController : ApiControllerBase
{
    private readonly ISender _sender;

    public JobConsumptionController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.JobConsumptionCreate)]
    [HttpPost("{jobCardId:long}/consume-parts")]
    [ProducesResponseType(typeof(ApiResponse<JobPartConsumptionSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<JobPartConsumptionSummaryResponse>>> ConsumePartsAsync(
        [FromRoute] long jobCardId,
        [FromBody] ConsumeJobPartsRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new ConsumeJobPartsCommand(
                jobCardId,
                request.Items
                    .Select(item => new ConsumeJobPartLineCommand(item.ItemId, item.QuantityUsed, item.ConsumptionRemarks))
                    .ToArray()),
            cancellationToken);

        return Success(response, "Job part consumption saved successfully.");
    }

    [Authorize(Policy = PermissionNames.JobConsumptionRead)]
    [HttpGet("{jobCardId:long}/consumption")]
    [ProducesResponseType(typeof(ApiResponse<JobPartConsumptionSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<JobPartConsumptionSummaryResponse>>> GetConsumptionAsync(
        [FromRoute] long jobCardId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetJobConsumptionQuery(jobCardId), cancellationToken);

        return Success(response);
    }
}
