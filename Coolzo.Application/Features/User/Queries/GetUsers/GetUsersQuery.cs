using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.User;
using MediatR;

namespace Coolzo.Application.Features.User.Queries.GetUsers;

public sealed record GetUsersQuery(
    int PageNumber,
    int PageSize,
    string? SearchTerm,
    bool? IsActive,
    IReadOnlyCollection<long>? RoleIds,
    IReadOnlyCollection<int>? BranchIds,
    string? SortBy,
    string? SortOrder) : IRequest<PagedResult<UserResponse>>;
