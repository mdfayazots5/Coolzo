using Coolzo.Application.Features.Billing.Queries.GetAccountsReceivableDashboard;
using Coolzo.Application.Features.Billing.Queries.GetBillingStatus;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/billing")]
public sealed class BillingController : ApiControllerBase
{
    private readonly ISender _sender;

    public BillingController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.BillingRead)]
    [HttpGet("accounts-receivable")]
    [ProducesResponseType(typeof(ApiResponse<AccountsReceivableDashboardResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AccountsReceivableDashboardResponse>>> GetAccountsReceivableAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetAccountsReceivableDashboardQuery(), cancellationToken);

        return Success(response);
    }

    [HttpGet("status/{invoiceId:long}")]
    [ProducesResponseType(typeof(ApiResponse<BillingStatusResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BillingStatusResponse>>> GetStatusAsync(
        [FromRoute] long invoiceId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetBillingStatusQuery(invoiceId), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.BillingRead)]
    [HttpPost("payment-reminders/send")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<object>> SendPaymentReminderAsync(
        [FromBody] BillingPaymentReminderRequest request)
    {
        return Success<object>(
            new
            {
                invoiceId = request.InvoiceId,
                reminderStatus = "queued",
                queuedAtUtc = DateTime.UtcNow,
            },
            "Payment reminder request accepted.");
    }
}

public sealed record BillingPaymentReminderRequest(string InvoiceId);
