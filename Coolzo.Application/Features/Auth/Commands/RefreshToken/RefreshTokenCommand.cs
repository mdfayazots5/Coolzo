using Coolzo.Contracts.Responses.Auth;
using MediatR;

namespace Coolzo.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthTokenResponse>;
