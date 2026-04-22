using FluentValidation;

namespace Coolzo.Application.Features.ServiceRequest.Commands.SaveServiceRequestNote;

public sealed class SaveServiceRequestNoteCommandValidator : AbstractValidator<SaveServiceRequestNoteCommand>
{
    public SaveServiceRequestNoteCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.NoteText).NotEmpty().MaximumLength(512);
    }
}
