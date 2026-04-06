using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface ITokenService
{
    AccessTokenResult CreateAccessToken(User user, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions);

    RefreshTokenResult CreateRefreshToken();
}
