using Coolzo.Application.Features.JobAttachment.Queries.GetFieldMedia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace Coolzo.Api.Controllers;

/// <summary>
/// Read proxy for job/technician media held in the PRIVATE R2 bucket. The bucket is not publicly
/// readable; this endpoint streams the bytes by storage key so plain image controls (mobile/admin) can
/// load them via a stable relative URL — matching the prior wwwroot loading model (unguessable GUID
/// key, no auth header required) but durable and off ephemeral disk. The storage key is confined to the
/// job-media prefix so arbitrary objects cannot be fetched.
/// </summary>
[Route("api/field-media")]
public sealed class FieldMediaController : ApiControllerBase
{
    private readonly ISender _sender;

    public FieldMediaController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{**objectKey}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync([FromRoute] string objectKey, CancellationToken cancellationToken)
    {
        try
        {
            var content = await _sender.Send(new GetFieldMediaQuery(objectKey), cancellationToken);

            if (content is null)
            {
                return NotFound();
            }

            return File(content.Content, content.ContentType);
        }
        catch (ArgumentException)
        {
            // Malformed or out-of-prefix key — do not leak detail, treat as not found.
            return NotFound();
        }
    }
}
