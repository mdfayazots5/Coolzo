using Coolzo.Application.Features.TechnicianJob.Queries.GetTechnicianJobList;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.TechnicianJobs;
using MediatR;

namespace Coolzo.Application.Features.TechnicianJob.Queries.GetTechnicianOwnJobList;

public sealed class GetTechnicianOwnJobListQueryHandler : IRequestHandler<GetTechnicianOwnJobListQuery, PagedResult<TechnicianJobListItemResponse>>
{
    private readonly ISender _sender;

    public GetTechnicianOwnJobListQueryHandler(ISender sender)
    {
        _sender = sender;
    }

    public Task<PagedResult<TechnicianJobListItemResponse>> Handle(GetTechnicianOwnJobListQuery request, CancellationToken cancellationToken)
    {
        return _sender.Send(
            new GetTechnicianJobListQuery(request.Status, request.SlotDate, request.PageNumber, request.PageSize),
            cancellationToken);
    }
}
