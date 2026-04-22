using Coolzo.Contracts.Responses.User;
using MediatR;

namespace Coolzo.Application.Features.User.Commands.ResetUserPin;

public sealed record ResetUserPinCommand(long UserId, string? Reason) : IRequest<UserPasswordResetResponse>;
