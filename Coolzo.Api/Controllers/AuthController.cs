using Asp.Versioning;
using Coolzo.Application.Features.Auth.Commands.Login;
using Coolzo.Application.Features.Auth.Commands.RefreshToken;
using Coolzo.Application.Features.Auth.Queries.GetCurrentUser;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Auth;
using Coolzo.Contracts.Responses.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ApiControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthTokenResponse>>> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new LoginCommand(request.UserNameOrEmail, request.Password), cancellationToken);

        return Success(response, "Login completed successfully.");
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthTokenResponse>>> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new RefreshTokenCommand(request.AccessToken, request.RefreshToken), cancellationToken);

        return Success(response, "Token refreshed successfully.");
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CurrentUserResponse>>> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetCurrentUserQuery(), cancellationToken);

        return Success(response);
    }
}
