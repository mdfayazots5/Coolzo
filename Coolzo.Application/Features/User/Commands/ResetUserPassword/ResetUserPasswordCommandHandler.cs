using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.User;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.User.Commands.ResetUserPassword;

public sealed class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, UserPasswordResetResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ICustomerPasswordPolicyService _customerPasswordPolicyService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _userSessionRepository;

    public ResetUserPasswordCommandHandler(
        IUserRepository userRepository,
        ICustomerPasswordPolicyService customerPasswordPolicyService,
        IRefreshTokenRepository refreshTokenRepository,
        IUserSessionRepository userSessionRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _customerPasswordPolicyService = customerPasswordPolicyService;
        _refreshTokenRepository = refreshTokenRepository;
        _userSessionRepository = userSessionRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<UserPasswordResetResponse> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            throw new AppException(
                ErrorCodes.NotFound,
                "The user could not be found.",
                404);
        }

        var preparedPassword = await _customerPasswordPolicyService.PreparePasswordAsync(
            null,
            CustomerPasswordChangeSource.AdminReset,
            user.UserId,
            cancellationToken);
        var actorName = ResolveActorName("AdminResetUserPassword");
        var ipAddress = ResolveIpAddress();
        var updatedAtUtc = _currentDateTime.UtcNow;

        await _customerPasswordPolicyService.ApplyPasswordAsync(
            user,
            preparedPassword,
            actorName,
            ipAddress,
            cancellationToken);
        await _refreshTokenRepository.RevokeActiveByUserIdAsync(user.UserId, updatedAtUtc, actorName, cancellationToken);
        await _userSessionRepository.DeactivateByUserIdAsync(user.UserId, updatedAtUtc, actorName, cancellationToken);
        await _auditLogRepository.AddAsync(
            CreateAuditLog(user.UserId, preparedPassword, actorName, ipAddress, request.Reason),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserPasswordResetResponse(
            true,
            preparedPassword.PasswordGenerated,
            preparedPassword.RequiresPasswordDelivery,
            preparedPassword.MustChangePassword,
            preparedPassword.IsTemporaryPassword,
            preparedPassword.PasswordExpiryOnUtc,
            preparedPassword.RequiresPasswordDelivery ? preparedPassword.RawPassword : null);
    }

    private AuditLog CreateAuditLog(
        long userId,
        Coolzo.Application.Common.Security.PreparedCustomerPassword preparedPassword,
        string actorName,
        string ipAddress,
        string? reason)
    {
        var newValues = $"Source={preparedPassword.ChangeSource};Mode={preparedPassword.PasswordStorageMode};Generated={preparedPassword.PasswordGenerated}";

        if (!string.IsNullOrWhiteSpace(reason))
        {
            newValues = $"{newValues};Reason={reason.Trim()}";
        }

        return new AuditLog
        {
            UserId = _currentUserContext.UserId,
            ActionName = "AdminResetUserPassword",
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
