using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Permission;
using MediatR;

namespace Coolzo.Application.Features.Permission.Queries.GetPermissions;

public sealed record GetPermissionsQuery(int PageNumber, int PageSize) : IRequest<PagedResult<PermissionResponse>>;
