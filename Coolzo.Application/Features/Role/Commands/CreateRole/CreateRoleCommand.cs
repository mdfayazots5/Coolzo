using Coolzo.Contracts.Responses.Role;
using MediatR;

namespace Coolzo.Application.Features.Role.Commands.CreateRole;

public sealed record CreateRoleCommand(
    string RoleName,
    string DisplayName,
    string Description,
    bool IsActive,
    IReadOnlyCollection<long> PermissionIds) : IRequest<RoleResponse>;
