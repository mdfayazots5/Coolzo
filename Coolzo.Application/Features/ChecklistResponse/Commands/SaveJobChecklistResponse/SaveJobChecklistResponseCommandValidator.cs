using FluentValidation;

namespace Coolzo.Application.Features.ChecklistResponse.Commands.SaveJobChecklistResponse;

public sealed class SaveJobChecklistResponseCommandValidator : AbstractValidator<SaveJobChecklistResponseCommand>
{
    public SaveJobChecklistResponseCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.Items).NotEmpty();
    }
}
