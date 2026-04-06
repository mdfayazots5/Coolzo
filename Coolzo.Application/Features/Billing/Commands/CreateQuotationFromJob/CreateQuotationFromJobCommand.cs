using Coolzo.Contracts.Requests.Billing;
using Coolzo.Contracts.Responses.Billing;
using MediatR;

namespace Coolzo.Application.Features.Billing.Commands.CreateQuotationFromJob;

public sealed record CreateQuotationFromJobCommand(
    long JobCardId,
    IReadOnlyCollection<QuotationLineRequest> Lines,
    decimal DiscountAmount,
    decimal TaxPercentage,
    string? Remarks) : IRequest<QuotationDetailResponse>;
