using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Security;
using Coolzo.Contracts.Responses.Auth;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    private readonly AuthenticatedUserProfileFactory _authenticatedUserProfileFactory;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(
        AuthenticatedUserProfileFactory authenticatedUserProfileFactory,
        ICurrentUserContext currentUserContext,
        IUserRepository userRepository)
    {
        _authenticatedUserProfileFactory = authenticatedUserProfileFactory;
        _currentUserContext = currentUserContext;
        _userRepository = userRepository;
    }

    public async Task<CurrentUserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
        {
            throw new AppException(
                ErrorCodes.Unauthorized,
                "The current request is unauthorized.",
                401);
        }

        var user = await _userRepository.GetByIdWithRolesAsync(_currentUserContext.UserId.Value, cancellationToken);

        if (user is null)
        {
            throw new AppException(
                ErrorCodes.NotFound,
                "The current user could not be found.",
                404);
        }

        return (await _authenticatedUserProfileFactory.CreateAsync(user, cancellationToken)).CurrentUser;
    }
}
