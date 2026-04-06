using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Billing.Queries.GetQuotationById;

public sealed class GetQuotationByIdQueryHandler : IRequestHandler<GetQuotationByIdQuery, QuotationDetailResponse>
{
    private readonly BillingAccessService _billingAccessService;
    private readonly IBillingRepository _billingRepository;

    public GetQuotationByIdQueryHandler(
        IBillingRepository billingRepository,
        BillingAccessService billingAccessService)
    {
        _billingRepository = billingRepository;
        _billingAccessService = billingAccessService;
    }

    public async Task<QuotationDetailResponse> Handle(GetQuotationByIdQuery request, CancellationToken cancellationToken)
    {
        var quotationHeader = await _billingRepository.GetQuotationByIdAsync(request.QuotationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested quotation could not be found.", 404);

        await _billingAccessService.EnsureQuotationReadAccessAsync(quotationHeader, cancellationToken);

        return BillingResponseMapper.ToQuotationDetail(quotationHeader);
    }
}
