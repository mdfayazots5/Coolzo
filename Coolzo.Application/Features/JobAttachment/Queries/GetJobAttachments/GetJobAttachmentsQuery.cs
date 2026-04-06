using Coolzo.Contracts.Responses.FieldExecution;
using MediatR;

namespace Coolzo.Application.Features.JobAttachment.Queries.GetJobAttachments;

public sealed record GetJobAttachmentsQuery(
    long ServiceRequestId) : IRequest<IReadOnlyCollection<JobAttachmentResponse>>;
