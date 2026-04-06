using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Role;
using MediatR;

namespace Coolzo.Application.Features.Role.Queries.GetRoles;

public sealed record GetRolesQuery(int PageNumber, int PageSize) : IRequest<PagedResult<RoleResponse>>;
