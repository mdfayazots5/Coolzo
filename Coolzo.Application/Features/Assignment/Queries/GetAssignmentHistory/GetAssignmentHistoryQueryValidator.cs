using FluentValidation;

namespace Coolzo.Application.Features.Assignment.Queries.GetAssignmentHistory;

public sealed class GetAssignmentHistoryQueryValidator : AbstractValidator<GetAssignmentHistoryQuery>
{
    public GetAssignmentHistoryQueryValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
    }
}
