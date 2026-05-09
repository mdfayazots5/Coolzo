using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetFeedbackDetail;

public sealed class GetFeedbackDetailQueryHandler : IRequestHandler<GetFeedbackDetailQuery, SupportFeedbackResponse>
{
    private readonly ICustomerAppRepository _customerAppRepository;

    public GetFeedbackDetailQueryHandler(ICustomerAppRepository customerAppRepository)
    {
        _customerAppRepository = customerAppRepository;
    }

    public async Task<SupportFeedbackResponse> Handle(GetFeedbackDetailQuery request, CancellationToken cancellationToken)
    {
        var feedback = await _customerAppRepository.GetFeedbackByIdAsync(request.CustomerReviewId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested feedback could not be found.", 404);

        return FeedbackResponseMapper.ToResponse(feedback);
    }
}
