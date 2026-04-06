using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Billing.Queries.GetPaymentByInvoice;

public sealed class GetPaymentByInvoiceQueryHandler : IRequestHandler<GetPaymentByInvoiceQuery, IReadOnlyCollection<PaymentTransactionResponse>>
{
    private readonly BillingAccessService _billingAccessService;
    private readonly IBillingRepository _billingRepository;

    public GetPaymentByInvoiceQueryHandler(
        IBillingRepository billingRepository,
        BillingAccessService billingAccessService)
    {
        _billingRepository = billingRepository;
        _billingAccessService = billingAccessService;
    }

    public async Task<IReadOnlyCollection<PaymentTransactionResponse>> Handle(GetPaymentByInvoiceQuery request, CancellationToken cancellationToken)
    {
        var invoiceHeader = await _billingRepository.GetInvoiceByIdAsync(request.InvoiceId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested invoice could not be found.", 404);

        await _billingAccessService.EnsureInvoiceReadAccessAsync(invoiceHeader, cancellationToken);

        var payments = await _billingRepository.GetPaymentTransactionsByInvoiceIdAsync(request.InvoiceId, cancellationToken);

        return payments.Select(BillingResponseMapper.ToPaymentTransactionResponse).ToArray();
    }
}
