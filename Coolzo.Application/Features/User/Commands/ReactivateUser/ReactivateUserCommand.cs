using Coolzo.Contracts.Responses.User;
using MediatR;

namespace Coolzo.Application.Features.User.Commands.ReactivateUser;

public sealed record ReactivateUserCommand(long UserId) : IRequest<UserResponse>;
