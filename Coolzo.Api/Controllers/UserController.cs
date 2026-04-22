using Asp.Versioning;
using Coolzo.Application.Features.User.Commands.DeactivateUser;
using Coolzo.Application.Features.User.Commands.CreateUser;
using Coolzo.Application.Features.User.Commands.ReactivateUser;
using Coolzo.Application.Features.User.Commands.ResetUserPassword;
using Coolzo.Application.Features.User.Commands.ResetUserPin;
using Coolzo.Application.Features.User.Commands.UpdateUser;
using Coolzo.Application.Features.User.Queries.GetUserById;
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
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] long[]? roleIds = null,
        [FromQuery] int[]? branchIds = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new GetUsersQuery(pageNumber, pageSize, searchTerm, isActive, roleIds, branchIds, sortBy, sortOrder),
            cancellationToken);

        return Success(response);
    }

    [HttpGet("{userId:long}")]
    [Authorize(Policy = PermissionNames.UserRead)]
    [ProducesResponseType(typeof(ApiResponse<UserDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserDetailResponse>>> GetByIdAsync(
        [FromRoute] long userId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetUserByIdQuery(userId), cancellationToken);

        return Success(response);
    }

    [HttpPost]
    [Authorize(Policy = PermissionNames.UserCreate)]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> CreateAsync([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateUserCommand(request.UserName, request.Email, request.FullName, request.Password, request.IsActive, request.RoleIds, request.BranchId),
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
            new UpdateUserCommand(userId, request.Email, request.FullName, request.IsActive, request.RoleIds, request.BranchId),
            cancellationToken);

        return Success(response, "User updated successfully.");
    }

    [HttpPost("{userId:long}/deactivate")]
    [Authorize(Policy = PermissionNames.UserUpdate)]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> DeactivateAsync(
        [FromRoute] long userId,
        [FromBody] DeactivateUserRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new DeactivateUserCommand(userId, request.Reason), cancellationToken);

        return Success(response, "User deactivated successfully.");
    }

    [HttpPost("{userId:long}/reactivate")]
    [Authorize(Policy = PermissionNames.UserUpdate)]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> ReactivateAsync(
        [FromRoute] long userId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new ReactivateUserCommand(userId), cancellationToken);

        return Success(response, "User reactivated successfully.");
    }

    [HttpPost("{userId:long}/reset-password")]
    [Authorize(Policy = PermissionNames.UserUpdate)]
    [ProducesResponseType(typeof(ApiResponse<UserPasswordResetResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserPasswordResetResponse>>> ResetPasswordAsync(
        [FromRoute] long userId,
        [FromBody] ResetUserPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new ResetUserPasswordCommand(userId, request.Reason), cancellationToken);

        return Success(response, "User password reset successfully.");
    }

    [HttpPost("{userId:long}/reset-pin")]
    [Authorize(Policy = PermissionNames.UserUpdate)]
    [ProducesResponseType(typeof(ApiResponse<UserPasswordResetResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserPasswordResetResponse>>> ResetPinAsync(
        [FromRoute] long userId,
        [FromBody] ResetUserPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new ResetUserPinCommand(userId, request.Reason), cancellationToken);

        return Success(response, "User PIN reset successfully.");
    }
}
