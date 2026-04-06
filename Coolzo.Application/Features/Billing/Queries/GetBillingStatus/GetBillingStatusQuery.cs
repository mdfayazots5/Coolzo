using Coolzo.Contracts.Responses.Billing;
using MediatR;

namespace Coolzo.Application.Features.Billing.Queries.GetBillingStatus;

public sealed record GetBillingStatusQuery(long InvoiceId) : IRequest<BillingStatusResponse>;
