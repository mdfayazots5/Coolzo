using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Security;
using Coolzo.Contracts.Responses.Auth;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthTokenResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly AuthenticatedUserProfileFactory _authenticatedUserProfileFactory;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        AuthenticatedUserProfileFactory authenticatedUserProfileFactory,
        ITokenService tokenService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _authenticatedUserProfileFactory = authenticatedUserProfileFactory;
        _tokenService = tokenService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<AuthTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshTokenEntity = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (refreshTokenEntity is null ||
            refreshTokenEntity.IsRevoked ||
            refreshTokenEntity.ExpiresAtUtc <= _currentDateTime.UtcNow ||
            refreshTokenEntity.User is null)
        {
            throw new AppException(
                ErrorCodes.InvalidRefreshToken,
                "The refresh token is invalid or expired.",
                401);
        }

        if (!refreshTokenEntity.User.IsActive)
        {
            throw new AppException(
                ErrorCodes.InactiveUser,
                "The user account is inactive.",
                403);
        }

        var authenticatedUserProfile = await _authenticatedUserProfileFactory.CreateAsync(refreshTokenEntity.User, cancellationToken);
        var accessToken = _tokenService.CreateAccessToken(
            refreshTokenEntity.User,
            authenticatedUserProfile.Roles,
            authenticatedUserProfile.Permissions);
        var newRefreshToken = _tokenService.CreateRefreshToken();

        refreshTokenEntity.IsRevoked = true;
        refreshTokenEntity.RevokedAtUtc = _currentDateTime.UtcNow;
        refreshTokenEntity.ReplacedByToken = newRefreshToken.Token;
        refreshTokenEntity.LastUpdated = _currentDateTime.UtcNow;
        refreshTokenEntity.UpdatedBy = refreshTokenEntity.User.UserName;

        if (refreshTokenEntity.UserSession is not null)
        {
            refreshTokenEntity.UserSession.AccessTokenJti = accessToken.TokenId;
            refreshTokenEntity.UserSession.RefreshToken = newRefreshToken.Token;
            refreshTokenEntity.UserSession.LastSeenAtUtc = _currentDateTime.UtcNow;
            refreshTokenEntity.UserSession.ExpiresAtUtc = newRefreshToken.ExpiresAtUtc;
            refreshTokenEntity.UserSession.LastUpdated = _currentDateTime.UtcNow;
            refreshTokenEntity.UserSession.UpdatedBy = refreshTokenEntity.User.UserName;
        }

        await _refreshTokenRepository.AddAsync(
            new Coolzo.Domain.Entities.RefreshToken
            {
                UserId = refreshTokenEntity.UserId,
                UserSessionId = refreshTokenEntity.UserSessionId,
                TokenValue = newRefreshToken.Token,
                ExpiresAtUtc = newRefreshToken.ExpiresAtUtc,
                CreatedBy = refreshTokenEntity.User.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = refreshTokenEntity.UserId,
                ActionName = "AuthRefreshToken",
                EntityName = "RefreshToken",
                EntityId = refreshTokenEntity.RefreshTokenId.ToString(),
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                CreatedBy = refreshTokenEntity.User.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthTokenResponse(
            accessToken.Token,
            newRefreshToken.Token,
            accessToken.ExpiresAtUtc,
            authenticatedUserProfile.CurrentUser);
    }
}
