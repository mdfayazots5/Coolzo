using Asp.Versioning;
using Coolzo.Application.Features.User.Commands.CreateUser;
using Coolzo.Application.Features.User.Commands.UpdateUser;
using Coolzo.Application.Features.User.Queries.GetUsers;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.User;
using Coolzo.Contracts.Responses.User;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/users")]
public sealed class UserController : ApiControllerBase
{
    private readonly ISender _sender;

    public UserController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.UserRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<UserResponse>>>> GetAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetUsersQuery(pageNumber, pageSize), cancellationToken);

        return Success(response);
    }

    [HttpPost]
    [Authorize(Policy = PermissionNames.UserCreate)]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> CreateAsync([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateUserCommand(request.UserName, request.Email, request.FullName, request.Password, request.IsActive, request.RoleIds),
            cancellationToken);

        return Success(response, "User created successfully.");
    }

    [HttpPut("{userId:long}")]
    [Authorize(Policy = PermissionNames.UserUpdate)]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateAsync(
        [FromRoute] long userId,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateUserCommand(userId, request.Email, request.FullName, request.IsActive, request.RoleIds),
            cancellationToken);

        return Success(response, "User updated successfully.");
    }
}
