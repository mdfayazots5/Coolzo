using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.User;
using MediatR;

namespace Coolzo.Application.Features.User.Queries.GetUsers;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResult<UserResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.ListAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.IsActive,
            request.RoleIds,
            request.BranchIds,
            request.SortBy,
            request.SortOrder,
            cancellationToken);
        var totalCount = await _userRepository.CountAsync(
            request.SearchTerm,
            request.IsActive,
            request.RoleIds,
            request.BranchIds,
            cancellationToken);

        return new PagedResult<UserResponse>(
            users.Select(UserResponseMapper.ToResponse).ToArray(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
