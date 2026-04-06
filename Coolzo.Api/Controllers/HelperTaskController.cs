using Asp.Versioning;
using Coolzo.Application.Features.GapPhaseE.Helpers;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.GapPhaseE;
using Coolzo.Contracts.Responses.GapPhaseE;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/helpers/{helperProfileId:long}/tasks")]
public sealed class HelperTaskController : ApiControllerBase
{
    private readonly ISender _sender;

    public HelperTaskController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<HelperTaskChecklistResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<HelperTaskChecklistResponse>>>> GetListAsync(
        [FromRoute] long helperProfileId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetHelperTaskChecklistQuery(helperProfileId), cancellationToken);
        return Success(response);
    }

    [HttpPost("{taskId:long}/respond")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<HelperTaskChecklistResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<HelperTaskChecklistResponse>>>> RespondAsync(
        [FromRoute] long helperProfileId,
        [FromRoute] long taskId,
        [FromBody] SaveHelperTaskResponseRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new SaveHelperTaskResponseCommand(helperProfileId, taskId, request.ResponseStatus, request.ResponseRemarks),
            cancellationToken);
        return Success(response, "Helper task response saved successfully.");
    }

    [HttpPost("{taskId:long}/upload-photo")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<HelperTaskChecklistResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<HelperTaskChecklistResponse>>>> UploadPhotoAsync(
        [FromRoute] long helperProfileId,
        [FromRoute] long taskId,
        [FromBody] UploadHelperTaskPhotoRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UploadHelperTaskPhotoCommand(
                helperProfileId,
                taskId,
                request.FileName,
                request.ContentType,
                request.Base64Content,
                request.ResponseRemarks),
            cancellationToken);

        return Success(response, "Helper task photo uploaded successfully.");
    }
}
