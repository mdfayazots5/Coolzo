using Asp.Versioning;
using Coolzo.Api.Mapping;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Admin;
using Coolzo.Contracts.Responses.Auth;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize(Roles = RoleNames.SuperAdmin)]
[Route("api/v{version:apiVersion}/admin")]
public sealed class AdminController : ApiControllerBase
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdminController(
        IRoleRepository roleRepository,
        AdminActivityLogger adminActivityLogger,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime)
    {
        _roleRepository = roleRepository;
        _adminActivityLogger = adminActivityLogger;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
    }

    [HttpPost("view-as-role")]
    [ProducesResponseType(typeof(ApiResponse<ViewAsRoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ViewAsRoleResponse>>> ViewAsRoleAsync(
        [FromBody] ViewAsRoleRequest request,
        CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(request.RoleId, cancellationToken);

        if (role is null)
        {
            return NotFound();
        }

        var permissions = role.RolePermissions
            .Where(rolePermission => rolePermission.Permission is not null && rolePermission.Permission.IsActive && !rolePermission.Permission.IsDeleted)
            .Select(rolePermission => rolePermission.Permission!.PermissionName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        await _adminActivityLogger.WriteAsync(
            "ViewAsRole",
            "Role",
            role.RoleId.ToString(),
            role.RoleName,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new ViewAsRoleResponse(
            role.RoleId,
            role.RoleName,
            role.DisplayName,
            PermissionModuleMatrixMapper.Build(permissions, [role.RoleName]),
            PermissionDataScopeMapper.Resolve([role.RoleName]),
            permissions,
            _currentDateTime.UtcNow);

        return Success(response, "View-as role session prepared.");
    }
}
