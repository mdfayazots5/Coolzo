using Coolzo.Application.Features.Permission.Queries.GetPermissions;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Permission;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/permissions")]
public sealed class PermissionController : ApiControllerBase
{
    private readonly ISender _sender;

    public PermissionController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.PermissionRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PermissionResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<PermissionResponse>>>> GetAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetPermissionsQuery(pageNumber, pageSize), cancellationToken);

        return Success(response);
    }
}
