using Coolzo.Contracts.Responses.Billing;
using MediatR;

namespace Coolzo.Application.Features.Billing.Queries.GetInvoiceById;

public sealed record GetInvoiceByIdQuery(long InvoiceId) : IRequest<InvoiceDetailResponse>;
