using Coolzo.Application.Features.Warranty.Commands.CreateWarrantyClaim;
using Coolzo.Application.Features.Warranty.Queries.GetWarrantyByInvoice;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Warranty;
using Coolzo.Contracts.Responses.Warranty;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/warranty")]
public sealed class WarrantyController : ApiControllerBase
{
    private readonly ISender _sender;

    public WarrantyController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("claim")]
    [ProducesResponseType(typeof(ApiResponse<WarrantyClaimResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<WarrantyClaimResponse>>> CreateClaimAsync(
        [FromBody] CreateWarrantyClaimRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new CreateWarrantyClaimCommand(request.InvoiceId, request.ClaimRemarks), cancellationToken);

        return Success(response, "Warranty claim created successfully.");
    }

    [HttpGet("invoice/{invoiceId:long}")]
    [ProducesResponseType(typeof(ApiResponse<WarrantyStatusResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<WarrantyStatusResponse>>> GetByInvoiceAsync(
        [FromRoute] long invoiceId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetWarrantyByInvoiceQuery(invoiceId), cancellationToken);

        return Success(response);
    }
}
