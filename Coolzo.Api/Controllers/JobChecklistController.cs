using Coolzo.Application.Features.ChecklistResponse.Commands.SaveJobChecklistResponse;
using Coolzo.Application.Features.ChecklistResponse.Queries.GetJobChecklist;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.FieldExecution;
using Coolzo.Contracts.Responses.FieldExecution;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize(Roles = RoleNames.Technician)]
[Route("api/technician-jobs")]
public sealed class JobChecklistController : ApiControllerBase
{
    private readonly ISender _sender;

    public JobChecklistController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{id:long}/checklist")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<JobChecklistItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<JobChecklistItemResponse>>>> GetChecklistAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetJobChecklistQuery(id), cancellationToken);

        return Success(response);
    }

    [HttpPost("{id:long}/checklist")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<JobChecklistItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<JobChecklistItemResponse>>>> SaveChecklistAsync(
        [FromRoute] long id,
        [FromBody] SaveJobChecklistResponseRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new SaveJobChecklistResponseCommand(id, request.Items), cancellationToken);

        return Success(response, "Job checklist saved successfully.");
    }
}
