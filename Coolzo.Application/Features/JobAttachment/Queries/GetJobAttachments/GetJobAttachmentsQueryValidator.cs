using FluentValidation;

namespace Coolzo.Application.Features.JobAttachment.Queries.GetJobAttachments;

public sealed class GetJobAttachmentsQueryValidator : AbstractValidator<GetJobAttachmentsQuery>
{
    public GetJobAttachmentsQueryValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
    }
}
