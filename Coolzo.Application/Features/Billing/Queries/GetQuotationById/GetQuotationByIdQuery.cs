using Coolzo.Contracts.Responses.Billing;
using MediatR;

namespace Coolzo.Application.Features.Billing.Queries.GetQuotationById;

public sealed record GetQuotationByIdQuery(long QuotationId) : IRequest<QuotationDetailResponse>;
