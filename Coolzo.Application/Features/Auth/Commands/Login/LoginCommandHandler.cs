using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Security;
using Coolzo.Application.Features.Auth.Commands.AuthSession;
using Coolzo.Contracts.Responses.Auth;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Auth.Commands.Login;

using DomainUser = Coolzo.Domain.Entities.User;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthTokenResponse>
{
    private static readonly string[] TwoFactorRoles =
    [
        RoleNames.SuperAdmin,
        RoleNames.Admin,
        "FinanceManager"
    ];

    private readonly IAuditLogRepository _auditLogRepository;
    private readonly AuthenticatedUserProfileFactory _authenticatedUserProfileFactory;
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ICustomerPasswordPolicyService _customerPasswordPolicyService;
    private readonly IOtpVerificationRepository _otpVerificationRepository;
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
        IOtpVerificationRepository otpVerificationRepository,
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
        _otpVerificationRepository = otpVerificationRepository;
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

        var authenticatedUserProfile = await _authenticatedUserProfileFactory.CreateAsync(user, cancellationToken);
        if (RequiresTwoFactor(authenticatedUserProfile.Roles))
        {
            var challengeExpiresAtUtc = await CreateTwoFactorChallengeAsync(user, cancellationToken);

            return new AuthTokenResponse(
                string.Empty,
                string.Empty,
                challengeExpiresAtUtc,
                authenticatedUserProfile.CurrentUser,
                true);
        }

        user.LastLoginDateUtc = _currentDateTime.UtcNow;
        user.LastUpdated = _currentDateTime.UtcNow;
        user.UpdatedBy = user.UserName;

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

    private async Task<DateTime> CreateTwoFactorChallengeAsync(DomainUser user, CancellationToken cancellationToken)
    {
        var now = _currentDateTime.UtcNow;
        var expiresAtUtc = now.AddMinutes(10);

        await _otpVerificationRepository.AddAsync(
            new OtpVerification
            {
                UserId = user.UserId,
                OtpCode = AuthSessionTokenFactory.CreateOtp(),
                Purpose = AuthSessionPurpose.LoginOtp,
                ExpiresAtUtc = expiresAtUtc,
                CreatedBy = user.UserName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _auditLogRepository.AddAsync(
            CreateAuditLog(user.UserId, "AuthLoginOtpChallenge", "User", user.UserId.ToString(), "Pending2FA"),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return expiresAtUtc;
    }

    private static bool RequiresTwoFactor(IReadOnlyCollection<string> roles)
    {
        return roles.Any(
            role => TwoFactorRoles.Any(requiredRole => role.Equals(requiredRole, StringComparison.OrdinalIgnoreCase)));
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

public sealed class LoginFieldCommandHandler : IRequestHandler<LoginFieldCommand, AuthTokenResponse>
{
    private readonly AuthSessionTokenIssuer _authSessionTokenIssuer;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IGapPhaseERepository _gapPhaseERepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public LoginFieldCommandHandler(
        IUserRepository userRepository,
        ITechnicianRepository technicianRepository,
        IGapPhaseERepository gapPhaseERepository,
        IPasswordHasher passwordHasher,
        AuthSessionTokenIssuer authSessionTokenIssuer,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime)
    {
        _userRepository = userRepository;
        _technicianRepository = technicianRepository;
        _gapPhaseERepository = gapPhaseERepository;
        _passwordHasher = passwordHasher;
        _authSessionTokenIssuer = authSessionTokenIssuer;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
    }

    public async Task<AuthTokenResponse> Handle(LoginFieldCommand request, CancellationToken cancellationToken)
    {
        var employeeId = request.EmployeeId.Trim();
        var user = await ResolveFieldUserAsync(employeeId, cancellationToken);

        if (user is null || !_passwordHasher.VerifyPassword(user.PasswordHash, request.Pin))
        {
            throw new AppException(
                ErrorCodes.InvalidCredentials,
                "Invalid employee ID or access PIN.",
                401);
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

        var response = await _authSessionTokenIssuer.IssueAsync(user, "AuthFieldLogin", cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }

    private async Task<DomainUser?> ResolveFieldUserAsync(string employeeId, CancellationToken cancellationToken)
    {
        var directUser = await _userRepository.GetByUserNameOrEmailAsync(employeeId, cancellationToken);
        if (IsFieldUser(directUser))
        {
            return directUser;
        }

        var technicians = await _technicianRepository.SearchAsync(employeeId, activeOnly: true, cancellationToken);
        var technician = technicians.FirstOrDefault(entity =>
            entity.UserId.HasValue &&
            entity.TechnicianCode.Equals(employeeId, StringComparison.OrdinalIgnoreCase));

        if (technician?.UserId is long technicianUserId)
        {
            var technicianUser = await _userRepository.GetByIdWithRolesAsync(technicianUserId, cancellationToken);
            if (IsFieldUser(technicianUser))
            {
                return technicianUser;
            }
        }

        var helpers = await _gapPhaseERepository.SearchHelpersAsync(employeeId, branchId: null, cancellationToken);
        var helper = helpers.FirstOrDefault(entity =>
            entity.ActiveFlag &&
            entity.HelperCode.Equals(employeeId, StringComparison.OrdinalIgnoreCase));

        if (helper is null)
        {
            return null;
        }

        var helperUser = await _userRepository.GetByIdWithRolesAsync(helper.UserId, cancellationToken);
        return IsFieldUser(helperUser)
            ? helperUser
            : null;
    }

    private static bool IsFieldUser(DomainUser? user)
    {
        if (user is null)
        {
            return false;
        }

        return user.UserRoles.Any(userRole =>
            !userRole.IsDeleted &&
            userRole.Role is not null &&
            userRole.Role.IsActive &&
            !userRole.Role.IsDeleted &&
            (string.Equals(userRole.Role.RoleName, RoleNames.Technician, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(userRole.Role.RoleName, RoleNames.Helper, StringComparison.OrdinalIgnoreCase)));
    }
}
