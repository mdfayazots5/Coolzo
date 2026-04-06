using FluentValidation;

namespace Coolzo.Application.Features.FieldExecution.Commands.SaveJobExecutionNote;

public sealed class SaveJobExecutionNoteCommandValidator : AbstractValidator<SaveJobExecutionNoteCommand>
{
    public SaveJobExecutionNoteCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.NoteText).NotEmpty().MaximumLength(512);
    }
}
