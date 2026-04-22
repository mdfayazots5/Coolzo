using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Role;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Role.Commands.UpdateRole;

public sealed class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleCommandHandler(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<RoleResponse> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(request.RoleId, cancellationToken);

        if (role is null)
        {
            throw new AppException(
                ErrorCodes.NotFound,
                "The role could not be found.",
                404);
        }

        var permissions = await _permissionRepository.GetByIdsAsync(request.PermissionIds.Distinct().ToArray(), cancellationToken);

        if (permissions.Count != request.PermissionIds.Distinct().Count())
        {
            throw new AppException(
                ErrorCodes.NotFound,
                "One or more permissions could not be found.",
                404);
        }

        role.DisplayName = request.DisplayName.Trim();
        role.Description = request.Description.Trim();
        role.IsActive = request.IsActive;
        role.LastUpdated = _currentDateTime.UtcNow;
        role.UpdatedBy = _currentUserContext.UserName;

        var requestedPermissionIds = request.PermissionIds.Distinct().ToHashSet();
        var retainedPermissionIds = new HashSet<long>();

        foreach (var rolePermission in role.RolePermissions.ToList())
        {
            if (!requestedPermissionIds.Contains(rolePermission.PermissionId))
            {
                role.RolePermissions.Remove(rolePermission);
                continue;
            }

            rolePermission.IsDeleted = false;
            rolePermission.LastUpdated = _currentDateTime.UtcNow;
            rolePermission.UpdatedBy = _currentUserContext.UserName;
            rolePermission.IPAddress = _currentUserContext.IPAddress;
            retainedPermissionIds.Add(rolePermission.PermissionId);
        }

        foreach (var permission in permissions.Where(permission => !retainedPermissionIds.Contains(permission.PermissionId)))
        {
            role.RolePermissions.Add(new RolePermission
            {
                PermissionId = permission.PermissionId,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            });
        }

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "UpdateRole",
                EntityName = "Role",
                EntityId = role.RoleId.ToString(),
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = role.DisplayName,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RoleResponse(
            role.RoleId,
            role.RoleName,
            role.DisplayName,
            role.Description,
            role.IsActive,
            role.UserRoles.Count(userRole =>
                !userRole.IsDeleted &&
                userRole.User is not null &&
                !userRole.User.IsDeleted),
            permissions.Select(permission => permission.PermissionId).ToArray(),
            permissions.Select(permission => permission.PermissionName).ToArray());
    }
}
