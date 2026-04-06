using Coolzo.Contracts.Responses.User;
using MediatR;

namespace Coolzo.Application.Features.User.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    long UserId,
    string Email,
    string FullName,
    bool IsActive,
    IReadOnlyCollection<long> RoleIds) : IRequest<UserResponse>;
