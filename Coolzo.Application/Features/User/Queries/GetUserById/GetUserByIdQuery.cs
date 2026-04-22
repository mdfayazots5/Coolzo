using Coolzo.Contracts.Responses.User;
using MediatR;

namespace Coolzo.Application.Features.User.Queries.GetUserById;

public sealed record GetUserByIdQuery(long UserId) : IRequest<UserDetailResponse>;
