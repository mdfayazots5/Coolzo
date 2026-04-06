using Coolzo.Contracts.Responses.Role;
using MediatR;

namespace Coolzo.Application.Features.Role.Commands.UpdateRole;

public sealed record UpdateRoleCommand(
    long RoleId,
    string DisplayName,
    string Description,
    bool IsActive,
    IReadOnlyCollection<long> PermissionIds) : IRequest<RoleResponse>;
