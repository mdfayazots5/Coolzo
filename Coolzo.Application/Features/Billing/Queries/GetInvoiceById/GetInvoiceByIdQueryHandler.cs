using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Billing.Queries.GetInvoiceById;

public sealed class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDetailResponse>
{
    private readonly BillingAccessService _billingAccessService;
    private readonly IBillingRepository _billingRepository;

    public GetInvoiceByIdQueryHandler(
        IBillingRepository billingRepository,
        BillingAccessService billingAccessService)
    {
        _billingRepository = billingRepository;
        _billingAccessService = billingAccessService;
    }

    public async Task<InvoiceDetailResponse> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoiceHeader = await _billingRepository.GetInvoiceByIdAsync(request.InvoiceId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested invoice could not be found.", 404);
        var billingHistory = await _billingRepository.GetBillingHistoryByInvoiceIdAsync(request.InvoiceId, cancellationToken);

        await _billingAccessService.EnsureInvoiceReadAccessAsync(invoiceHeader, cancellationToken);

        return BillingResponseMapper.ToInvoiceDetail(invoiceHeader, billingHistory);
    }
}
