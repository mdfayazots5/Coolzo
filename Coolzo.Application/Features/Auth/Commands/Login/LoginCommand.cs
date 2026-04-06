using Coolzo.Contracts.Responses.Auth;
using MediatR;

namespace Coolzo.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string UserNameOrEmail, string Password) : IRequest<AuthTokenResponse>;
