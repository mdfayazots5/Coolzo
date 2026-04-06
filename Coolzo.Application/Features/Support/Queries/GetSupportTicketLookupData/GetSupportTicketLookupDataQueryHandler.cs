using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetSupportTicketLookupData;

public sealed class GetSupportTicketLookupDataQueryHandler : IRequestHandler<GetSupportTicketLookupDataQuery, SupportTicketLookupDataResponse>
{
    private readonly ISupportTicketRepository _supportTicketRepository;

    public GetSupportTicketLookupDataQueryHandler(ISupportTicketRepository supportTicketRepository)
    {
        _supportTicketRepository = supportTicketRepository;
    }

    public async Task<SupportTicketLookupDataResponse> Handle(GetSupportTicketLookupDataQuery request, CancellationToken cancellationToken)
    {
        var categories = await _supportTicketRepository.GetCategoriesAsync(cancellationToken);
        var priorities = await _supportTicketRepository.GetPrioritiesAsync(cancellationToken);

        return SupportTicketResponseMapper.ToLookupDataResponse(categories, priorities);
    }
}
