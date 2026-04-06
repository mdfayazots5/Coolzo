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
        var users = await _userRepository.ListAsync(request.PageNumber, request.PageSize, cancellationToken);
        var totalCount = await _userRepository.CountAsync(cancellationToken);

        return new PagedResult<UserResponse>(
            users.Select(
                user => new UserResponse(
                    user.UserId,
                    user.UserName,
                    user.Email,
                    user.FullName,
                    user.IsActive,
                    user.UserRoles.Select(userRole => userRole.RoleId).ToArray(),
                    user.UserRoles
                        .Where(userRole => userRole.Role is not null)
                        .Select(userRole => userRole.Role!.DisplayName)
                        .ToArray(),
                    user.DateCreated))
                .ToArray(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
