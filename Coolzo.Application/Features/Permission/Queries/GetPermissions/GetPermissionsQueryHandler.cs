using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Permission;
using MediatR;

namespace Coolzo.Application.Features.Permission.Queries.GetPermissions;

public sealed class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, PagedResult<PermissionResponse>>
{
    private readonly IPermissionRepository _permissionRepository;

    public GetPermissionsQueryHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<PagedResult<PermissionResponse>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _permissionRepository.ListAsync(request.PageNumber, request.PageSize, cancellationToken);
        var totalCount = await _permissionRepository.CountAsync(cancellationToken);

        return new PagedResult<PermissionResponse>(
            permissions.Select(
                permission => new PermissionResponse(
                    permission.PermissionId,
                    permission.PermissionName,
                    permission.DisplayName,
                    permission.ModuleName,
                    permission.ActionName,
                    permission.IsActive))
                .ToArray(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
