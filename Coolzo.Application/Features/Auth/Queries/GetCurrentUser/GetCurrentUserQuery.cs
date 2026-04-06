using Coolzo.Contracts.Responses.Auth;
using MediatR;

namespace Coolzo.Application.Features.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<CurrentUserResponse>;
