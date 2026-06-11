using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using MediatR;

namespace Coolzo.Application.Features.JobAttachment.Queries.GetFieldMedia;

public sealed class GetFieldMediaQueryHandler : IRequestHandler<GetFieldMediaQuery, JobMediaContent?>
{
    private readonly IJobAttachmentStorageService _jobAttachmentStorageService;

    public GetFieldMediaQueryHandler(IJobAttachmentStorageService jobAttachmentStorageService)
    {
        _jobAttachmentStorageService = jobAttachmentStorageService;
    }

    public async Task<JobMediaContent?> Handle(GetFieldMediaQuery request, CancellationToken cancellationToken)
    {
        return await _jobAttachmentStorageService.GetByKeyAsync(request.ObjectKey, cancellationToken);
    }
}
