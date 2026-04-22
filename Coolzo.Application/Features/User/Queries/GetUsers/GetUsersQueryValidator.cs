using FluentValidation;

namespace Coolzo.Application.Features.User.Queries.GetUsers;

public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(request => request.PageNumber)
            .GreaterThan(0);

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 200);

        RuleFor(request => request.SearchTerm)
            .MaximumLength(128);

        RuleFor(request => request.SortBy)
            .Must(value => value is null or "name" or "createdAt" or "lastLogin")
            .WithMessage("SortBy must be one of: name, createdAt, lastLogin.");

        RuleFor(request => request.SortOrder)
            .Must(value => value is null or "asc" or "desc")
            .WithMessage("SortOrder must be one of: asc, desc.");
    }
}
