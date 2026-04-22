using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Role;
using MediatR;

namespace Coolzo.Application.Features.Role.Queries.GetRoles;

public sealed class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, PagedResult<RoleResponse>>
{
    private readonly IRoleRepository _roleRepository;

    public GetRolesQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<PagedResult<RoleResponse>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.ListAsync(request.PageNumber, request.PageSize, cancellationToken);
        var totalCount = await _roleRepository.CountAsync(cancellationToken);

        return new PagedResult<RoleResponse>(
            roles.Select(
                role => new RoleResponse(
                    role.RoleId,
                    role.RoleName,
                    role.DisplayName,
                    role.Description,
                    role.IsActive,
                    role.UserRoles.Count(userRole =>
                        !userRole.IsDeleted &&
                        userRole.User is not null &&
                        !userRole.User.IsDeleted),
                    role.RolePermissions.Select(rolePermission => rolePermission.PermissionId).ToArray(),
                    role.RolePermissions
                        .Where(rolePermission => rolePermission.Permission is not null)
                        .Select(rolePermission => rolePermission.Permission!.PermissionName)
                        .ToArray()))
                .ToArray(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
