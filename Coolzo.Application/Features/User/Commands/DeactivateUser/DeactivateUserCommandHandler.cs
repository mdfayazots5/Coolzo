using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.User;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.User.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, UserResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _userSessionRepository;

    public DeactivateUserCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUserSessionRepository userSessionRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _userSessionRepository = userSessionRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<UserResponse> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            throw new AppException(
                ErrorCodes.NotFound,
                "The user could not be found.",
                404);
        }

        var actorName = ResolveActorName("DeactivateUser");
        var updatedAtUtc = _currentDateTime.UtcNow;

        user.IsActive = false;
        user.LastUpdated = updatedAtUtc;
        user.UpdatedBy = actorName;

        await _refreshTokenRepository.RevokeActiveByUserIdAsync(user.UserId, updatedAtUtc, actorName, cancellationToken);
        await _userSessionRepository.DeactivateByUserIdAsync(user.UserId, updatedAtUtc, actorName, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "DeactivateUser",
                EntityName = "User",
                EntityId = user.UserId.ToString(),
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = string.IsNullOrWhiteSpace(request.Reason)
                    ? "User account deactivated."
                    : $"User account deactivated. Reason={request.Reason.Trim()}",
                CreatedBy = actorName,
                DateCreated = updatedAtUtc,
                IPAddress = ResolveIpAddress()
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserResponseMapper.ToResponse(user);
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
