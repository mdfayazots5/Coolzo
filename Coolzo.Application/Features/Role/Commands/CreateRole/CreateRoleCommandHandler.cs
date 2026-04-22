using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Role;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Role.Commands.CreateRole;

public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(
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

    public async Task<RoleResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (await _roleRepository.GetByNameAsync(request.RoleName, cancellationToken) is not null)
        {
            throw new AppException(
                ErrorCodes.DuplicateValue,
                "The role name already exists.",
                409);
        }

        var permissions = await _permissionRepository.GetByIdsAsync(request.PermissionIds.Distinct().ToArray(), cancellationToken);

        if (permissions.Count != request.PermissionIds.Distinct().Count())
        {
            throw new AppException(
                ErrorCodes.NotFound,
                "One or more permissions could not be found.",
                404);
        }

        var role = new Domain.Entities.Role
        {
            RoleName = request.RoleName.Trim(),
            DisplayName = request.DisplayName.Trim(),
            Description = request.Description.Trim(),
            IsActive = request.IsActive,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        foreach (var permission in permissions)
        {
            role.RolePermissions.Add(new RolePermission
            {
                PermissionId = permission.PermissionId,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            });
        }

        await _roleRepository.AddAsync(role, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreateRole",
                EntityName = "Role",
                EntityId = role.RoleName,
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
            0,
            permissions.Select(permission => permission.PermissionId).ToArray(),
            permissions.Select(permission => permission.PermissionName).ToArray());
    }
}
