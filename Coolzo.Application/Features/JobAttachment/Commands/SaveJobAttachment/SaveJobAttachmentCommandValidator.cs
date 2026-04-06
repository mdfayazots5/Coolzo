using FluentValidation;

namespace Coolzo.Application.Features.JobAttachment.Commands.SaveJobAttachment;

public sealed class SaveJobAttachmentCommandValidator : AbstractValidator<SaveJobAttachmentCommand>
{
    public SaveJobAttachmentCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.AttachmentType).NotEmpty();
        RuleFor(request => request.FileName).NotEmpty().MaximumLength(256);
        RuleFor(request => request.ContentType).NotEmpty().MaximumLength(128);
        RuleFor(request => request.Base64Content).NotEmpty();
    }
}
