using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.User;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.User.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<UserResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            throw new AppException(
                ErrorCodes.NotFound,
                "The user could not be found.",
                404);
        }

        if (await _userRepository.ExistsByEmailAsync(request.Email, request.UserId, cancellationToken))
        {
            throw new AppException(
                ErrorCodes.DuplicateValue,
                "The email already exists.",
                409);
        }

        var roles = await _roleRepository.GetByIdsAsync(request.RoleIds.Distinct().ToArray(), cancellationToken);

        if (roles.Count != request.RoleIds.Distinct().Count())
        {
            throw new AppException(
                ErrorCodes.NotFound,
                "One or more roles could not be found.",
                404);
        }

        user.Email = request.Email.Trim();
        user.FullName = request.FullName.Trim();
        user.IsActive = request.IsActive;
        user.BranchId = request.BranchId ?? user.BranchId;
        user.LastUpdated = _currentDateTime.UtcNow;
        user.UpdatedBy = _currentUserContext.UserName;

        user.UserRoles.Clear();

        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole
            {
                RoleId = role.RoleId,
                Role = role,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            });
        }

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "UpdateUser",
                EntityName = "User",
                EntityId = user.UserId.ToString(),
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = user.Email,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserResponseMapper.ToResponse(user);
    }
}
