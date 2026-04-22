using Coolzo.Contracts.Responses.User;
using MediatR;

namespace Coolzo.Application.Features.User.Commands.ResetUserPassword;

public sealed record ResetUserPasswordCommand(long UserId, string? Reason) : IRequest<UserPasswordResetResponse>;
