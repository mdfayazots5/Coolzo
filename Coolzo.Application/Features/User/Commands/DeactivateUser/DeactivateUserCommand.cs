using Coolzo.Contracts.Responses.User;
using MediatR;

namespace Coolzo.Application.Features.User.Commands.DeactivateUser;

public sealed record DeactivateUserCommand(long UserId, string? Reason) : IRequest<UserResponse>;
