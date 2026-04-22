using Asp.Versioning;
using Coolzo.Api.Mapping;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.Role.Commands.CreateRole;
using Coolzo.Application.Features.Role.Commands.UpdateRole;
using Coolzo.Application.Features.Role.Queries.GetRoles;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Role;
using Coolzo.Contracts.Responses.Auth;
using Coolzo.Contracts.Responses.Role;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/roles")]
public sealed class RoleController : ApiControllerBase
{
    private readonly IRoleRepository _roleRepository;
    private readonly ISender _sender;

    public RoleController(ISender sender, IRoleRepository roleRepository)
    {
        _sender = sender;
        _roleRepository = roleRepository;
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.RoleRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RoleResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<RoleResponse>>>> GetAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetRolesQuery(pageNumber, pageSize), cancellationToken);

        return Success(response);
    }

    [HttpPost]
    [Authorize(Policy = PermissionNames.RoleCreate)]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RoleResponse>>> CreateAsync([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateRoleCommand(request.RoleName, request.DisplayName, request.Description, request.IsActive, request.PermissionIds),
            cancellationToken);

        return Success(response, "Role created successfully.");
    }

    [HttpPut("{roleId:long}")]
    [Authorize(Policy = PermissionNames.RoleUpdate)]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RoleResponse>>> UpdateAsync(
        [FromRoute] long roleId,
        [FromBody] UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateRoleCommand(roleId, request.DisplayName, request.Description, request.IsActive, request.PermissionIds),
            cancellationToken);

        return Success(response, "Role updated successfully.");
    }

    [HttpGet("{roleId:long}/permissions")]
    [Authorize(Policy = PermissionNames.RoleRead)]
    [ProducesResponseType(typeof(ApiResponse<RolePermissionSnapshotResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RolePermissionSnapshotResponse>>> GetPermissionsAsync(
        [FromRoute] long roleId,
        CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(roleId, cancellationToken);

        if (role is null)
        {
            return NotFound();
        }

        var permissions = role.RolePermissions
            .Where(rolePermission => rolePermission.Permission is not null && rolePermission.Permission.IsActive && !rolePermission.Permission.IsDeleted)
            .Select(rolePermission => rolePermission.Permission!.PermissionName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var response = new RolePermissionSnapshotResponse(
            role.RoleId,
            role.RolePermissions.Select(rolePermission => rolePermission.PermissionId).ToArray(),
            PermissionModuleMatrixMapper.Build(permissions, [role.RoleName]),
            PermissionDataScopeMapper.Resolve([role.RoleName]),
            permissions,
            role.RoleName,
            role.DisplayName);

        return Success(response);
    }

    [HttpPut("{roleId:long}/permissions")]
    [Authorize(Policy = PermissionNames.RoleUpdate)]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RoleResponse>>> UpdatePermissionsAsync(
        [FromRoute] long roleId,
        [FromBody] UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(roleId, cancellationToken);

        if (role is null)
        {
            return NotFound();
        }

        var response = await _sender.Send(
            new UpdateRoleCommand(roleId, role.DisplayName, role.Description, role.IsActive, request.PermissionIds),
            cancellationToken);

        return Success(response, "Role permissions updated successfully.");
    }
}
