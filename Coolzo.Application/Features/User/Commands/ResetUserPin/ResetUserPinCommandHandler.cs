using System.Security.Cryptography;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.User;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;
using UserEntity = Coolzo.Domain.Entities.User;

namespace Coolzo.Application.Features.User.Commands.ResetUserPin;

public sealed class ResetUserPinCommandHandler : IRequestHandler<ResetUserPinCommand, UserPasswordResetResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserPasswordHistoryRepository _userPasswordHistoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _userSessionRepository;

    public ResetUserPinCommandHandler(
        IUserRepository userRepository,
        IUserPasswordHistoryRepository userPasswordHistoryRepository,
        IPasswordHasher passwordHasher,
        IRefreshTokenRepository refreshTokenRepository,
        IUserSessionRepository userSessionRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _userPasswordHistoryRepository = userPasswordHistoryRepository;
        _passwordHasher = passwordHasher;
        _refreshTokenRepository = refreshTokenRepository;
        _userSessionRepository = userSessionRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<UserPasswordResetResponse> Handle(ResetUserPinCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            throw new AppException(ErrorCodes.NotFound, "The user could not be found.", 404);
        }

        if (!IsFieldUser(user))
        {
            throw new AppException(ErrorCodes.ValidationFailure, "PIN reset is only available for technician and helper accounts.", 400);
        }

        var actorName = ResolveActorName("AdminResetUserPin");
        var ipAddress = ResolveIpAddress();
        var updatedAtUtc = _currentDateTime.UtcNow;
        var temporaryPin = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var hashedPin = _passwordHasher.HashPassword(temporaryPin);

        user.PasswordHash = hashedPin;
        user.PasswordStorageMode = CustomerPasswordMode.Hashed;
        user.MustChangePassword = true;
        user.PasswordLastChangedOnUtc = updatedAtUtc;
        user.PasswordExpiryOnUtc = updatedAtUtc.AddDays(30);
        user.PasswordUpdatedBy = actorName;
        user.IsTemporaryPassword = true;
        user.LastPasswordResetOnUtc = updatedAtUtc;
        user.PasswordResetSource = CustomerPasswordChangeSource.AdminReset;
        user.LastUpdated = updatedAtUtc;
        user.UpdatedBy = actorName;

        await _userPasswordHistoryRepository.AddAsync(
            new UserPasswordHistory
            {
                User = user,
                PasswordValue = hashedPin,
                PasswordStorageMode = CustomerPasswordMode.Hashed,
                ChangeSource = CustomerPasswordChangeSource.AdminReset,
                ChangedOnUtc = updatedAtUtc,
                CreatedBy = actorName,
                DateCreated = updatedAtUtc,
                IPAddress = ipAddress
            },
            cancellationToken);
        await _refreshTokenRepository.RevokeActiveByUserIdAsync(user.UserId, updatedAtUtc, actorName, cancellationToken);
        await _userSessionRepository.DeactivateByUserIdAsync(user.UserId, updatedAtUtc, actorName, cancellationToken);
        await _auditLogRepository.AddAsync(
            CreateAuditLog(user.UserId, actorName, ipAddress, request.Reason),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserPasswordResetResponse(
            true,
            true,
            true,
            true,
            true,
            user.PasswordExpiryOnUtc,
            temporaryPin);
    }

    private AuditLog CreateAuditLog(long userId, string actorName, string ipAddress, string? reason)
    {
        var newValues = "Source=AdminReset;CredentialType=Pin;Generated=True";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            newValues = $"{newValues};Reason={reason.Trim()}";
        }

        return new AuditLog
        {
            UserId = _currentUserContext.UserId,
            ActionName = "AdminResetUserPin",
            EntityName = "User",
            EntityId = userId.ToString(),
            TraceId = _currentUserContext.TraceId,
            StatusName = "Success",
            NewValues = newValues,
            CreatedBy = actorName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = ipAddress
        };
    }

    private static bool IsFieldUser(UserEntity user)
    {
        return user.UserRoles.Any(userRole =>
            !userRole.IsDeleted &&
            userRole.Role is not null &&
            userRole.Role.IsActive &&
            !userRole.Role.IsDeleted &&
            (string.Equals(userRole.Role.RoleName, RoleNames.Technician, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(userRole.Role.RoleName, RoleNames.Helper, StringComparison.OrdinalIgnoreCase)));
    }

    private string ResolveActorName(string fallbackActorName)
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName)
            ? fallbackActorName
            : _currentUserContext.UserName;
    }

    private string ResolveIpAddress()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.IPAddress)
            ? "127.0.0.1"
            : _currentUserContext.IPAddress;
    }
}
