using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetFeedbackList;

public sealed class GetFeedbackListQueryHandler : IRequestHandler<GetFeedbackListQuery, IReadOnlyCollection<SupportFeedbackResponse>>
{
    private readonly ICustomerAppRepository _customerAppRepository;

    public GetFeedbackListQueryHandler(ICustomerAppRepository customerAppRepository)
    {
        _customerAppRepository = customerAppRepository;
    }

    public async Task<IReadOnlyCollection<SupportFeedbackResponse>> Handle(GetFeedbackListQuery request, CancellationToken cancellationToken)
    {
        var feedback = await _customerAppRepository.ListFeedbackAsync(request.ServiceId, cancellationToken);
        return feedback
            .Select(FeedbackResponseMapper.ToResponse)
            .ToArray();
    }
}
