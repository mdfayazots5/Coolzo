using Coolzo.Contracts.Responses.Billing;
using MediatR;

namespace Coolzo.Application.Features.Billing.Queries.GetPaymentByInvoice;

public sealed record GetPaymentByInvoiceQuery(long InvoiceId) : IRequest<IReadOnlyCollection<PaymentTransactionResponse>>;
