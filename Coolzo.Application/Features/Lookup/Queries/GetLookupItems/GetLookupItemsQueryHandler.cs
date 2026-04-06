using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Common;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Lookup.Queries.GetLookupItems;

public sealed class GetLookupItemsQueryHandler : IRequestHandler<GetLookupItemsQuery, IReadOnlyCollection<LookupItemResponse>>
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRoleRepository _roleRepository;

    public GetLookupItemsQueryHandler(IRoleRepository roleRepository, IPermissionRepository permissionRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
    }

    public async Task<IReadOnlyCollection<LookupItemResponse>> Handle(GetLookupItemsQuery request, CancellationToken cancellationToken)
    {
        if (string.Equals(request.LookupType, "roles", StringComparison.OrdinalIgnoreCase))
        {
            var roles = await _roleRepository.ListAsync(1, 500, cancellationToken);

            return roles
                .Select(role => new LookupItemResponse(role.RoleId, role.DisplayName))
                .ToArray();
        }

        if (string.Equals(request.LookupType, "permissions", StringComparison.OrdinalIgnoreCase))
        {
            var permissions = await _permissionRepository.ListAsync(1, 500, cancellationToken);

            return permissions
                .Select(permission => new LookupItemResponse(permission.PermissionId, permission.DisplayName))
                .ToArray();
        }

        throw new AppException(
            ErrorCodes.InvalidLookupType,
            "The requested lookup type is not supported.",
            400);
    }
}
