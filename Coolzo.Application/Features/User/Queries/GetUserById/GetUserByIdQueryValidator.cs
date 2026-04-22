using FluentValidation;

namespace Coolzo.Application.Features.User.Queries.GetUserById;

public sealed class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(request => request.UserId)
            .GreaterThan(0);
    }
}
