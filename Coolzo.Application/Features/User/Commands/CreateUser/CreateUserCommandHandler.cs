using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.User;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.User.Commands.CreateUser;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<UserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByUserNameAsync(request.UserName, null, cancellationToken))
        {
            throw new AppException(
                ErrorCodes.DuplicateValue,
                "The username already exists.",
                409);
        }

        if (await _userRepository.ExistsByEmailAsync(request.Email, null, cancellationToken))
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

        var user = new Domain.Entities.User
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            FullName = request.FullName.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            IsActive = request.IsActive,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole
            {
                RoleId = role.RoleId,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            });
        }

        await _userRepository.AddAsync(user, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreateUser",
                EntityName = "User",
                EntityId = user.UserName,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = user.Email,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserResponse(
            user.UserId,
            user.UserName,
            user.Email,
            user.FullName,
            user.IsActive,
            roles.Select(role => role.RoleId).ToArray(),
            roles.Select(role => role.DisplayName).ToArray(),
            user.DateCreated);
    }
}
