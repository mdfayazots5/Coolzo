using System.Security.Cryptography;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Security;
using Coolzo.Contracts.Responses.Auth;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Auth.Commands.AuthSession;

using DomainUser = Coolzo.Domain.Entities.User;
using AuditLogEntity = Coolzo.Domain.Entities.AuditLog;
using OtpVerificationEntity = Coolzo.Domain.Entities.OtpVerification;
using RefreshTokenEntity = Coolzo.Domain.Entities.RefreshToken;
using UserSessionEntity = Coolzo.Domain.Entities.UserSession;

public sealed record LoginOtpCommand(string LoginId, string Otp) : IRequest<AuthTokenResponse>;

public sealed record VerifyOtpCommand(string Email, string Otp) : IRequest<AuthTokenResponse>;

public sealed record SendOtpCommand(string LoginId) : IRequest<AuthActionResponse>;

public sealed record ForgotPasswordCommand(string Email) : IRequest<AuthActionResponse>;

public sealed record ResetPasswordCommand(string Token, string Password) : IRequest<AuthActionResponse>;

public sealed record LogoutCommand(string RefreshToken) : IRequest<AuthActionResponse>;

public sealed record ForceLogoutCommand(long UserId) : IRequest<AuthActionResponse>;

internal static class AuthSessionPurpose
{
    public const string LoginOtp = "LoginOtp";
    public const string PasswordReset = "PasswordReset";
}

internal static class AuthSessionTokenFactory
{
    public static string CreateOtp()
    {
        return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
    }

    public static string CreatePasswordResetToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(8));
    }
}

public sealed class AuthSessionTokenIssuer
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly AuthenticatedUserProfileFactory _authenticatedUserProfileFactory;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IUserSessionRepository _userSessionRepository;

    public AuthSessionTokenIssuer(
        AuthenticatedUserProfileFactory authenticatedUserProfileFactory,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUserSessionRepository userSessionRepository,
        IAuditLogRepository auditLogRepository,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _authenticatedUserProfileFactory = authenticatedUserProfileFactory;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _userSessionRepository = userSessionRepository;
        _auditLogRepository = auditLogRepository;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<AuthTokenResponse> IssueAsync(DomainUser user, string actionName, CancellationToken cancellationToken)
    {
        if (!user.IsActive)
        {
            throw new AppException(ErrorCodes.InactiveUser, "The user account is inactive.", 403);
        }

        var profile = await _authenticatedUserProfileFactory.CreateAsync(user, cancellationToken);
        var accessToken = _tokenService.CreateAccessToken(user, profile.Roles, profile.Permissions);
        var refreshToken = _tokenService.CreateRefreshToken();
        var now = _currentDateTime.UtcNow;

        var userSession = new UserSessionEntity
        {
            UserId = user.UserId,
            AccessTokenJti = accessToken.TokenId,
            RefreshToken = refreshToken.Token,
            DeviceName = "Web",
            PlatformName = "API",
            SessionIpAddress = _currentUserContext.IPAddress,
            UserAgent = "CoolzoApi",
            LastSeenAtUtc = now,
            ExpiresAtUtc = refreshToken.ExpiresAtUtc,
            CreatedBy = user.UserName,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        await _userSessionRepository.AddAsync(userSession, cancellationToken);
        await _refreshTokenRepository.AddAsync(
            new RefreshTokenEntity
            {
                UserId = user.UserId,
                TokenValue = refreshToken.Token,
                ExpiresAtUtc = refreshToken.ExpiresAtUtc,
                UserSession = userSession,
                CreatedBy = user.UserName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLogEntity
            {
                UserId = user.UserId,
                ActionName = actionName,
                EntityName = "User",
                EntityId = user.UserId.ToString(),
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                CreatedBy = user.UserName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        return new AuthTokenResponse(accessToken.Token, refreshToken.Token, accessToken.ExpiresAtUtc, profile.CurrentUser);
    }
}

public sealed class LoginOtpCommandHandler : IRequestHandler<LoginOtpCommand, AuthTokenResponse>
{
    private readonly AuthSessionTokenIssuer _authSessionTokenIssuer;
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IOtpVerificationRepository _otpVerificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoginOtpCommandHandler(
        CustomerAccountLookupService customerAccountLookupService,
        IOtpVerificationRepository otpVerificationRepository,
        AuthSessionTokenIssuer authSessionTokenIssuer,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _otpVerificationRepository = otpVerificationRepository;
        _authSessionTokenIssuer = authSessionTokenIssuer;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
    }

    public async Task<AuthTokenResponse> Handle(LoginOtpCommand request, CancellationToken cancellationToken)
    {
        var customerAccount = await _customerAccountLookupService.FindByLoginIdAsync(request.LoginId.Trim(), cancellationToken)
            ?? throw new AppException(ErrorCodes.InvalidCredentials, "The verification code is invalid or expired.", 401);
        var user = customerAccount.User;
        var otp = await _otpVerificationRepository.GetActiveByUserAndCodeAsync(
            user.UserId,
            AuthSessionPurpose.LoginOtp,
            request.Otp.Trim(),
            _currentDateTime.UtcNow,
            cancellationToken)
            ?? throw new AppException(ErrorCodes.InvalidCredentials, "The verification code is invalid or expired.", 401);

        otp.IsConsumed = true;
        otp.ConsumedAtUtc = _currentDateTime.UtcNow;
        otp.UpdatedBy = user.UserName;
        otp.LastUpdated = _currentDateTime.UtcNow;
        user.LastLoginDateUtc = _currentDateTime.UtcNow;
        user.LastUpdated = _currentDateTime.UtcNow;
        user.UpdatedBy = user.UserName;

        var response = await _authSessionTokenIssuer.IssueAsync(user, "AuthLoginOtp", cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}

public sealed class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, AuthActionResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly CustomerAccountLookupService _customerAccountLookupService;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IOtpVerificationRepository _otpVerificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SendOtpCommandHandler(
        CustomerAccountLookupService customerAccountLookupService,
        IOtpVerificationRepository otpVerificationRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _customerAccountLookupService = customerAccountLookupService;
        _otpVerificationRepository = otpVerificationRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<AuthActionResponse> Handle(SendOtpCommand request, CancellationToken cancellationToken)
    {
        var customerAccount = await _customerAccountLookupService.FindByLoginIdAsync(request.LoginId.Trim(), cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The customer account could not be found.", 404);
        var now = _currentDateTime.UtcNow;

        await _otpVerificationRepository.AddAsync(
            new OtpVerificationEntity
            {
                UserId = customerAccount.User.UserId,
                OtpCode = AuthSessionTokenFactory.CreateOtp(),
                Purpose = AuthSessionPurpose.LoginOtp,
                ExpiresAtUtc = now.AddMinutes(10),
                CreatedBy = "AuthOtpSend",
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLogEntity
            {
                UserId = customerAccount.User.UserId,
                ActionName = "AuthOtpSend",
                EntityName = "User",
                EntityId = customerAccount.User.UserId.ToString(),
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                CreatedBy = "AuthOtpSend",
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthActionResponse(true, "OTP generated successfully.");
    }
}

public sealed class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, AuthTokenResponse>
{
    private readonly IMediator _mediator;

    public VerifyOtpCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task<AuthTokenResponse> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        return _mediator.Send(new LoginOtpCommand(request.Email, request.Otp), cancellationToken);
    }
}

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, AuthActionResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IOtpVerificationRepository _otpVerificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IOtpVerificationRepository otpVerificationRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _otpVerificationRepository = otpVerificationRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<AuthActionResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUserNameOrEmailAsync(request.Email.Trim(), cancellationToken);

        if (user is null || !user.IsActive)
        {
            return new AuthActionResponse(true, "If the account exists, password reset instructions have been queued.");
        }

        var now = _currentDateTime.UtcNow;
        var resetToken = AuthSessionTokenFactory.CreatePasswordResetToken();

        await _otpVerificationRepository.AddAsync(
            new OtpVerificationEntity
            {
                UserId = user.UserId,
                OtpCode = resetToken,
                Purpose = AuthSessionPurpose.PasswordReset,
                ExpiresAtUtc = now.AddMinutes(30),
                CreatedBy = "AuthForgotPassword",
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLogEntity
            {
                UserId = user.UserId,
                ActionName = "AuthForgotPassword",
                EntityName = "User",
                EntityId = user.UserId.ToString(),
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                CreatedBy = "AuthForgotPassword",
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthActionResponse(true, "If the account exists, password reset instructions have been queued.");
    }
}

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, AuthActionResponse>
{
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IOtpVerificationRepository _otpVerificationRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IOtpVerificationRepository otpVerificationRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime)
    {
        _userRepository = userRepository;
        _otpVerificationRepository = otpVerificationRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
    }

    public async Task<AuthActionResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var now = _currentDateTime.UtcNow;
        var otp = await _otpVerificationRepository.GetActiveByCodeAsync(
            AuthSessionPurpose.PasswordReset,
            request.Token.Trim(),
            now,
            cancellationToken)
            ?? throw new AppException(ErrorCodes.InvalidCredentials, "The reset token is invalid or expired.", 400);
        var user = await _userRepository.GetByIdAsync(otp.UserId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The user account could not be found.", 404);

        user.PasswordHash = _passwordHasher.HashPassword(request.Password);
        user.PasswordStorageMode = CustomerPasswordMode.Hashed;
        user.MustChangePassword = false;
        user.IsTemporaryPassword = false;
        user.PasswordLastChangedOnUtc = now;
        user.PasswordUpdatedBy = "AuthResetPassword";
        user.LastPasswordResetOnUtc = now;
        user.PasswordResetSource = CustomerPasswordChangeSource.ForgotPassword;
        user.LastUpdated = now;
        user.UpdatedBy = "AuthResetPassword";

        otp.IsConsumed = true;
        otp.ConsumedAtUtc = now;
        otp.LastUpdated = now;
        otp.UpdatedBy = "AuthResetPassword";

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthActionResponse(true, "Password reset successfully.");
    }
}

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, AuthActionResponse>
{
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
    }

    public async Task<AuthActionResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (refreshToken is not null && !refreshToken.IsRevoked)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAtUtc = _currentDateTime.UtcNow;
            refreshToken.LastUpdated = _currentDateTime.UtcNow;
            refreshToken.UpdatedBy = refreshToken.User?.UserName ?? "AuthLogout";

            if (refreshToken.UserSession is not null)
            {
                refreshToken.UserSession.IsActive = false;
                refreshToken.UserSession.LastUpdated = _currentDateTime.UtcNow;
                refreshToken.UserSession.UpdatedBy = refreshToken.UpdatedBy;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new AuthActionResponse(true, "Logout completed successfully.");
    }
}

public sealed class ForceLogoutCommandHandler : IRequestHandler<ForceLogoutCommand, AuthActionResponse>
{
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserSessionRepository _userSessionRepository;

    public ForceLogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserSessionRepository userSessionRepository,
        ICurrentUserContext currentUserContext,
        ICurrentDateTime currentDateTime)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userSessionRepository = userSessionRepository;
        _currentUserContext = currentUserContext;
        _currentDateTime = currentDateTime;
    }

    public async Task<AuthActionResponse> Handle(ForceLogoutCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserContext.Roles.Any(role => role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
        {
            throw new AppException(ErrorCodes.Forbidden, "Only Super Admin users can force logout another user.", 403);
        }

        var actorName = string.IsNullOrWhiteSpace(_currentUserContext.UserName)
            ? "AuthForceLogout"
            : _currentUserContext.UserName;
        var revokedAtUtc = _currentDateTime.UtcNow;

        await _refreshTokenRepository.RevokeActiveByUserIdAsync(request.UserId, revokedAtUtc, actorName, cancellationToken);
        await _userSessionRepository.DeactivateByUserIdAsync(request.UserId, revokedAtUtc, actorName, cancellationToken);

        return new AuthActionResponse(true, "User sessions have been terminated.");
    }
}
