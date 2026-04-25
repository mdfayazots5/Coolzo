using Coolzo.Application.Features.Lookup.Queries.GetLookupItems;
using Coolzo.Contracts.Common;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/lookups")]
public sealed class LookupController : ApiControllerBase
{
    private readonly ISender _sender;

    public LookupController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{lookupType}")]
    [Authorize(Policy = PermissionNames.LookupRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<LookupItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<LookupItemResponse>>>> GetAsync(
        [FromRoute] string lookupType,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetLookupItemsQuery(lookupType), cancellationToken);

        return Success(response);
    }
}
