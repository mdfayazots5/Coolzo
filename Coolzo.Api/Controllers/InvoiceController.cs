using Coolzo.Application.Features.Billing.Commands.GenerateInvoiceFromQuotation;
using Coolzo.Application.Features.Billing.Commands.RecordPayment;
using Coolzo.Application.Features.Billing.Queries.GetCustomerInvoices;
using Coolzo.Application.Features.Billing.Queries.GetInvoiceById;
using Coolzo.Application.Features.Billing.Queries.SearchInvoices;
using Coolzo.Api.Utilities;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Route("api/invoices")]
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
    [HttpPost("{id:long}/mark-paid")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InvoiceDetailResponse>>> MarkPaidAsync(
        [FromRoute] long id,
        [FromBody] InvoiceMarkPaidRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedMethod = NormalizePaymentMethod(request.Method);
        var referenceNumber = string.IsNullOrWhiteSpace(request.Reference)
            ? $"MANUAL-{id}-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : request.Reference.Trim();

        await _sender.Send(
            new RecordPaymentCommand(
                id,
                request.Amount,
                normalizedMethod,
                referenceNumber,
                request.Notes,
                $"invoice-mark-paid-{id}-{referenceNumber}",
                null,
                null,
                null,
                false,
                null),
            cancellationToken);

        var updated = await _sender.Send(new GetInvoiceByIdQuery(id), cancellationToken);
        return Success(updated, "Invoice payment recorded successfully.");
    }

    [Authorize]
    [HttpGet("{id:long}/pdf")]
    public async Task<IActionResult> DownloadPdfAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var invoice = await _sender.Send(new GetInvoiceByIdQuery(id), cancellationToken);
        var lines = new List<string>
        {
            $"Invoice {invoice.InvoiceNumber}",
            $"Customer: {invoice.CustomerName}",
            $"Service: {invoice.ServiceName}",
            $"Date: {invoice.InvoiceDateUtc:dd MMM yyyy}",
            $"Status: {invoice.CurrentStatus}",
            $"Subtotal: INR {invoice.SubTotalAmount:0.00}",
            $"Discount: INR {invoice.DiscountAmount:0.00}",
            $"Tax: INR {invoice.TaxAmount:0.00}",
            $"Total: INR {invoice.GrandTotalAmount:0.00}",
            $"Paid: INR {invoice.PaidAmount:0.00}",
            $"Outstanding: INR {invoice.BalanceAmount:0.00}"
        };

        foreach (var line in invoice.Lines)
        {
            lines.Add($"- {line.LineDescription}: INR {line.LineAmount:0.00}");
        }

        var fileBytes = SimplePdfDocumentBuilder.Build(lines);
        var fileName = $"invoice-{invoice.InvoiceNumber}.pdf";

        return File(fileBytes, "application/pdf", fileName);
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

    private static string NormalizePaymentMethod(string paymentMethod)
    {
        var normalized = paymentMethod.Trim().Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase).Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);

        return normalized.ToLowerInvariant() switch
        {
            "cash" => "Cash",
            "cheque" => "Cash",
            "banktransfer" => "Cash",
            "upi" => "Upi",
            "card" => "Card",
            "online" => "Upi",
            _ => paymentMethod,
        };
    }
}

public sealed record InvoiceMarkPaidRequest(
    decimal Amount,
    string Method,
    string? Reference,
    string? Notes);
