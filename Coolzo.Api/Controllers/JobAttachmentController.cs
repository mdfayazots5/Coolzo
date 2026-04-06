using Asp.Versioning;
using Coolzo.Application.Features.JobAttachment.Commands.SaveJobAttachment;
using Coolzo.Application.Features.JobAttachment.Queries.GetJobAttachments;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.FieldExecution;
using Coolzo.Contracts.Responses.FieldExecution;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize(Roles = RoleNames.Technician)]
[Route("api/v{version:apiVersion}/technician-jobs")]
public sealed class JobAttachmentController : ApiControllerBase
{
    private readonly ISender _sender;

    public JobAttachmentController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("{id:long}/attachments")]
    [ProducesResponseType(typeof(ApiResponse<JobAttachmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<JobAttachmentResponse>>> SaveAttachmentAsync(
        [FromRoute] long id,
        [FromBody] SaveJobAttachmentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new SaveJobAttachmentCommand(
                id,
                request.AttachmentType,
                request.FileName,
                request.ContentType,
                request.Base64Content,
                request.AttachmentRemarks),
            cancellationToken);

        return Success(response, "Job attachment saved successfully.");
    }

    [HttpGet("{id:long}/attachments")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<JobAttachmentResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<JobAttachmentResponse>>>> GetAttachmentsAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetJobAttachmentsQuery(id), cancellationToken);

        return Success(response);
    }
}
