using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Billing.Queries.GetQuotationByJob;

public sealed class GetQuotationByJobQueryHandler : IRequestHandler<GetQuotationByJobQuery, QuotationDetailResponse>
{
    private readonly BillingAccessService _billingAccessService;
    private readonly IBillingRepository _billingRepository;

    public GetQuotationByJobQueryHandler(
        IBillingRepository billingRepository,
        BillingAccessService billingAccessService)
    {
        _billingRepository = billingRepository;
        _billingAccessService = billingAccessService;
    }

    public async Task<QuotationDetailResponse> Handle(GetQuotationByJobQuery request, CancellationToken cancellationToken)
    {
        var quotationHeader = await _billingRepository.GetQuotationByJobCardIdAsync(request.JobCardId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "No quotation was found for the specified job card.", 404);

        await _billingAccessService.EnsureQuotationReadAccessAsync(quotationHeader, cancellationToken);

        return BillingResponseMapper.ToQuotationDetail(quotationHeader);
    }
}
