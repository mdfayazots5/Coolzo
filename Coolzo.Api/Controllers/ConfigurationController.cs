using Asp.Versioning;
using Coolzo.Application.Features.Configuration.Queries.GetSystemSettings;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Configuration;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/configuration")]
public sealed class ConfigurationController : ApiControllerBase
{
    private readonly ISender _sender;

    public ConfigurationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("settings")]
    [Authorize(Policy = PermissionNames.ConfigurationRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SystemSettingResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<SystemSettingResponse>>>> GetSystemSettingsAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetSystemSettingsQuery(), cancellationToken);

        return Success(response);
    }
}
