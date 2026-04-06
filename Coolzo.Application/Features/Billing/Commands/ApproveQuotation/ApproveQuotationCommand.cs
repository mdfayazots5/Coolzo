using Coolzo.Contracts.Responses.Billing;
using MediatR;

namespace Coolzo.Application.Features.Billing.Commands.ApproveQuotation;

public sealed record ApproveQuotationCommand(long QuotationId, string? Remarks) : IRequest<QuotationDetailResponse>;
