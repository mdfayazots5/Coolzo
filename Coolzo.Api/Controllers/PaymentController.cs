using Asp.Versioning;
using Coolzo.Api.Extensions;
using Coolzo.Api.Utilities;
using Coolzo.Application.Features.Billing.Commands.RecordPayment;
using Coolzo.Application.Features.Billing.Queries.GetInvoiceById;
using Coolzo.Application.Features.Billing.Queries.GetPaymentByInvoice;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Billing;
using Coolzo.Contracts.Responses.Billing;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/payments")]
public sealed class PaymentController : ApiControllerBase
{
    private readonly ISender _sender;

    public PaymentController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PaymentGatewaySessionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentGatewaySessionResponse>>> InitiateAsync(
        [FromBody] InitiatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var invoice = await _sender.Send(new GetInvoiceByIdQuery(request.InvoiceId), cancellationToken);
        var paymentId = $"invoice-{request.InvoiceId}";

        if (invoice.BalanceAmount > 0)
        {
            var referenceNumber = $"APP-{request.InvoiceId}-{DateTime.UtcNow:yyyyMMddHHmmss}";

            await _sender.Send(
                new RecordPaymentCommand(
                    request.InvoiceId,
                    invoice.BalanceAmount,
                    request.Method,
                    referenceNumber,
                    "Customer app gateway payment.",
                    $"customer-app-{request.InvoiceId}",
                    referenceNumber,
                    null,
                    invoice.GrandTotalAmount,
                    false,
                    null),
                cancellationToken);
        }

        var response = new PaymentGatewaySessionResponse(
            paymentId,
            $"/app/payment-status/success/{request.InvoiceId}",
            "Confirmed");

        return Success(response, "Payment gateway session created successfully.");
    }

    [HttpPost("collect")]
    [ProducesResponseType(typeof(ApiResponse<PaymentTransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentTransactionResponse>>> CollectAsync(
        [FromBody] RecordPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new RecordPaymentCommand(
                request.InvoiceId,
                request.PaidAmount,
                request.PaymentMethod,
                request.ReferenceNumber,
                request.Remarks,
                request.IdempotencyKey,
                request.GatewayTransactionId,
                request.Signature,
                request.ExpectedInvoiceAmount,
                request.IsWebhookEvent,
                request.WebhookReference),
            cancellationToken);

        return Success(response, "Payment recorded successfully.");
    }

    [HttpGet("invoice/{invoiceId:long}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<PaymentTransactionResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<PaymentTransactionResponse>>>> GetByInvoiceAsync(
        [FromRoute] long invoiceId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetPaymentByInvoiceQuery(invoiceId), cancellationToken);

        return Success(response);
    }

    [HttpGet("{paymentId}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentGatewayStatusResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentGatewayStatusResponse>>> GetPaymentStatusAsync(
        [FromRoute] string paymentId,
        CancellationToken cancellationToken)
    {
        if (!TryResolveInvoiceId(paymentId, out var invoiceId))
        {
            return NotFound(ApiResponseFactory.Failure<object?>(
                "not_found",
                "The payment session could not be found.",
                HttpContext.TraceIdentifier,
                Array.Empty<ApiError>()));
        }

        var invoice = await _sender.Send(new GetInvoiceByIdQuery(invoiceId), cancellationToken);
        var status = invoice.BalanceAmount <= 0 ? "Confirmed" : "Pending";
        var response = new PaymentGatewayStatusResponse(
            paymentId,
            invoiceId,
            status,
            $"/app/payment-status/{(status == "Confirmed" ? "success" : "failed")}/{invoiceId}");

        return Success(response);
    }

    [HttpGet("receipt/{invoiceId:long}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentReceiptResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentReceiptResponse>>> GetReceiptByInvoiceAsync(
        [FromRoute] long invoiceId,
        CancellationToken cancellationToken)
    {
        var payments = await _sender.Send(new GetPaymentByInvoiceQuery(invoiceId), cancellationToken);
        var receipt = payments
            .OrderByDescending(payment => payment.PaymentDateUtc)
            .Select(payment => payment.Receipt)
            .FirstOrDefault(item => item is not null);

        if (receipt is null)
        {
            return NotFound(ApiResponseFactory.Failure<object?>(
                "not_found",
                "No payment receipt was found for this invoice.",
                HttpContext.TraceIdentifier,
                Array.Empty<ApiError>()));
        }

        return Success(receipt);
    }

    [HttpGet("receipt/{invoiceId:long}/pdf")]
    public async Task<IActionResult> DownloadReceiptPdfAsync(
        [FromRoute] long invoiceId,
        CancellationToken cancellationToken)
    {
        var payments = await _sender.Send(new GetPaymentByInvoiceQuery(invoiceId), cancellationToken);
        var receipt = payments
            .OrderByDescending(payment => payment.PaymentDateUtc)
            .FirstOrDefault(payment => payment.Receipt is not null)?
            .Receipt;

        if (receipt is null)
        {
            return NotFound(ApiResponseFactory.Failure<object?>(
                "not_found",
                "No payment receipt was found for this invoice.",
                HttpContext.TraceIdentifier,
                Array.Empty<ApiError>()));
        }

        var lines = new List<string>
        {
            $"Receipt {receipt.ReceiptNumber}",
            $"Invoice Id: {receipt.InvoiceId}",
            $"Transaction Id: {receipt.PaymentTransactionId}",
            $"Receipt Date: {receipt.ReceiptDateUtc:dd MMM yyyy}",
            $"Received Amount: INR {receipt.ReceivedAmount:0.00}",
            $"Balance Amount: INR {receipt.BalanceAmount:0.00}",
            $"Remarks: {receipt.ReceiptRemarks}"
        };

        var fileBytes = SimplePdfDocumentBuilder.Build(lines);
        var fileName = $"receipt-{receipt.ReceiptNumber}.pdf";

        return File(fileBytes, "application/pdf", fileName);
    }

    private static bool TryResolveInvoiceId(string paymentId, out long invoiceId)
    {
        invoiceId = 0;
        var normalized = paymentId.StartsWith("invoice-", StringComparison.OrdinalIgnoreCase)
            ? paymentId["invoice-".Length..]
            : paymentId;

        return long.TryParse(normalized, out invoiceId);
    }
}
