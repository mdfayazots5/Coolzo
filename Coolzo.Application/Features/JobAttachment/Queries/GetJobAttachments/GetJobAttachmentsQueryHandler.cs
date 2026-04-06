using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.TechnicianJob;
using Coolzo.Contracts.Responses.FieldExecution;
using MediatR;

namespace Coolzo.Application.Features.JobAttachment.Queries.GetJobAttachments;

public sealed class GetJobAttachmentsQueryHandler : IRequestHandler<GetJobAttachmentsQuery, IReadOnlyCollection<JobAttachmentResponse>>
{
    private readonly ITechnicianJobAccessService _technicianJobAccessService;

    public GetJobAttachmentsQueryHandler(ITechnicianJobAccessService technicianJobAccessService)
    {
        _technicianJobAccessService = technicianJobAccessService;
    }

    public async Task<IReadOnlyCollection<JobAttachmentResponse>> Handle(GetJobAttachmentsQuery request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestAsync(request.ServiceRequestId, cancellationToken);
        return TechnicianJobResponseMapper.ToAttachments(serviceRequest.JobCard);
    }
}
