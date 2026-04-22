using Coolzo.Contracts.Responses.User;
using MediatR;

namespace Coolzo.Application.Features.User.Commands.CreateUser;

public sealed record CreateUserCommand(
    string UserName,
    string Email,
    string FullName,
    string Password,
    bool IsActive,
    IReadOnlyCollection<long> RoleIds,
    int? BranchId) : IRequest<UserResponse>;
