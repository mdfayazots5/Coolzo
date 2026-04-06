using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Security;
using Coolzo.Contracts.Responses.Auth;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthTokenResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly AuthenticatedUserProfileFactory _authenticatedUserProfileFactory;
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ICustomerPasswordPolicyService _customerPasswordPolicyService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _userSessionRepository;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ICustomerPasswordPolicyService customerPasswordPolicyService,
        CustomerAccountLookupService customerAccountLookupService,
        AuthenticatedUserProfileFactory authenticatedUserProfileFactory,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUserSessionRepository userSessionRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _customerPasswordPolicyService = customerPasswordPolicyService;
        _customerAccountLookupService = customerAccountLookupService;
        _authenticatedUserProfileFactory = authenticatedUserProfileFactory;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _userSessionRepository = userSessionRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<AuthTokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedLoginId = request.UserNameOrEmail.Trim();
        var user = await _userRepository.GetByUserNameOrEmailAsync(normalizedLoginId, cancellationToken);
        CustomerAccountLookupResult? customerAccount = null;
        var customerOnlyUser = user is not null && _customerAccountLookupService.IsCustomerUser(user);

        if (customerOnlyUser)
        {
            customerAccount = await _customerAccountLookupService.FindByUserIdAsync(user!.UserId, cancellationToken);

            if (customerAccount is null)
            {
                throw new AppException(
                    ErrorCodes.InvalidCredentials,
                    "Invalid username or password.",
                    401);
            }
        }

        if (user is null)
        {
            customerAccount = await _customerAccountLookupService.FindByLoginIdAsync(normalizedLoginId, cancellationToken);
        }

        if (customerAccount is not null)
        {
            user = customerAccount.User;
        }

        if (user is null)
        {
            throw new AppException(
                ErrorCodes.InvalidCredentials,
                "Invalid username or password.",
                401);
        }

        var isCustomerUser = customerAccount is not null;
        var passwordIsValid = isCustomerUser
            ? customerAccount is not null &&
                await _customerPasswordPolicyService.VerifyPasswordAsync(user, request.Password, cancellationToken)
            : _passwordHasher.VerifyPassword(user.PasswordHash, request.Password);

        if (!passwordIsValid)
        {
            throw new AppException(
                ErrorCodes.InvalidCredentials,
                "Invalid username or password.",
                401);
        }

        if (customerAccount is not null && !customerAccount.Customer.IsActive)
        {
            throw new AppException(
                ErrorCodes.InactiveUser,
                "The customer account is inactive.",
                403);
        }

        if (!user.IsActive)
        {
            throw new AppException(
                ErrorCodes.InactiveUser,
                "The user account is inactive.",
                403);
        }

        user.LastLoginDateUtc = _currentDateTime.UtcNow;
        user.LastUpdated = _currentDateTime.UtcNow;
        user.UpdatedBy = user.UserName;

        var authenticatedUserProfile = await _authenticatedUserProfileFactory.CreateAsync(user, cancellationToken);
        var accessToken = _tokenService.CreateAccessToken(
            user,
            authenticatedUserProfile.Roles,
            authenticatedUserProfile.Permissions);
        var refreshToken = _tokenService.CreateRefreshToken();

        var userSession = new UserSession
        {
            UserId = user.UserId,
            AccessTokenJti = accessToken.TokenId,
            RefreshToken = refreshToken.Token,
            DeviceName = "Web",
            PlatformName = "API",
            SessionIpAddress = _currentUserContext.IPAddress,
            UserAgent = "CoolzoApi",
            LastSeenAtUtc = _currentDateTime.UtcNow,
            ExpiresAtUtc = refreshToken.ExpiresAtUtc,
            CreatedBy = user.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        var refreshTokenEntity = new Coolzo.Domain.Entities.RefreshToken
        {
            UserId = user.UserId,
            TokenValue = refreshToken.Token,
            ExpiresAtUtc = refreshToken.ExpiresAtUtc,
            UserSession = userSession,
            CreatedBy = user.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _userSessionRepository.AddAsync(userSession, cancellationToken);
        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await _auditLogRepository.AddAsync(
            CreateAuditLog(user.UserId, "AuthLogin", "User", user.UserId.ToString(), "Success"),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthTokenResponse(
            accessToken.Token,
            refreshToken.Token,
            accessToken.ExpiresAtUtc,
            authenticatedUserProfile.CurrentUser);
    }

    private AuditLog CreateAuditLog(long userId, string actionName, string entityName, string entityId, string statusName)
    {
        return new AuditLog
        {
            UserId = userId,
            ActionName = actionName,
            EntityName = entityName,
            EntityId = entityId,
            TraceId = _currentUserContext.TraceId,
            StatusName = statusName,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };
    }
}
