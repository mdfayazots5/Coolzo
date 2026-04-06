using Coolzo.Contracts.Responses.Billing;
using MediatR;

namespace Coolzo.Application.Features.Billing.Commands.GenerateInvoiceFromQuotation;

public sealed record GenerateInvoiceFromQuotationCommand(long QuotationId, string? IdempotencyKey) : IRequest<InvoiceDetailResponse>;
