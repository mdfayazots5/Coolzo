using FluentValidation;

namespace Coolzo.Application.Features.ChecklistResponse.Queries.GetJobChecklist;

public sealed class GetJobChecklistQueryValidator : AbstractValidator<GetJobChecklistQuery>
{
    public GetJobChecklistQueryValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
    }
}
