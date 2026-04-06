using Asp.Versioning;
using Coolzo.Application.Features.Billing.Commands.RecordPayment;
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
}
