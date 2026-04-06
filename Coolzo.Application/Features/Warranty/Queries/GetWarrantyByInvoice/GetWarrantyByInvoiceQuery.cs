using Coolzo.Contracts.Responses.Warranty;
using MediatR;

namespace Coolzo.Application.Features.Warranty.Queries.GetWarrantyByInvoice;

public sealed record GetWarrantyByInvoiceQuery(long InvoiceId) : IRequest<WarrantyStatusResponse>;
