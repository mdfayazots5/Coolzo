using Coolzo.Contracts.Responses.FieldExecution;
using MediatR;

namespace Coolzo.Application.Features.JobAttachment.Commands.SaveJobAttachment;

public sealed record SaveJobAttachmentCommand(
    long ServiceRequestId,
    string AttachmentType,
    string FileName,
    string ContentType,
    string Base64Content,
    string? AttachmentRemarks) : IRequest<JobAttachmentResponse>;
