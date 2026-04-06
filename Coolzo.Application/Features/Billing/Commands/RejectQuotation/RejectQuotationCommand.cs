using Coolzo.Contracts.Responses.Billing;
using MediatR;

namespace Coolzo.Application.Features.Billing.Commands.RejectQuotation;

public sealed record RejectQuotationCommand(long QuotationId, string? Remarks) : IRequest<QuotationDetailResponse>;
