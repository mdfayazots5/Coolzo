using Asp.Versioning;
using Coolzo.Application.Features.Billing.Commands.GenerateInvoiceFromQuotation;
using Coolzo.Application.Features.Billing.Queries.GetCustomerInvoices;
using Coolzo.Application.Features.Billing.Queries.GetInvoiceById;
using Coolzo.Application.Features.Billing.Queries.SearchInvoices;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/invoices")]
public sealed class InvoiceController : ApiControllerBase
{
    private readonly ISender _sender;

    public InvoiceController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.InvoiceCreate)]
    [HttpPost("from-quotation/{quotationId:long}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InvoiceDetailResponse>>> CreateFromQuotationAsync(
        [FromRoute] long quotationId,
        [FromHeader(Name = "X-Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GenerateInvoiceFromQuotationCommand(quotationId, idempotencyKey), cancellationToken);

        return Success(response, "Invoice generated successfully.");
    }

    [Authorize(Policy = PermissionNames.InvoiceRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InvoiceListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<InvoiceListItemResponse>>>> SearchAsync(
        [FromQuery] string? status,
        [FromQuery] long? customerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new SearchInvoicesQuery(status, customerId, pageNumber, pageSize), cancellationToken);

        return Success(response);
    }

    [Authorize]
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InvoiceDetailResponse>>> GetByIdAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetInvoiceByIdQuery(id), cancellationToken);

        return Success(response);
    }

    [Authorize]
    [HttpGet("customer")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InvoiceListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<InvoiceListItemResponse>>>> GetCustomerInvoicesAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetCustomerInvoicesQuery(pageNumber, pageSize), cancellationToken);

        return Success(response);
    }
}
