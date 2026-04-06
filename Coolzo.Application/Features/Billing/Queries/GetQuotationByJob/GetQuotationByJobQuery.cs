using Coolzo.Contracts.Responses.Billing;
using MediatR;

namespace Coolzo.Application.Features.Billing.Queries.GetQuotationByJob;

public sealed record GetQuotationByJobQuery(long JobCardId) : IRequest<QuotationDetailResponse>;
