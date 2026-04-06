using Asp.Versioning;
using Coolzo.Application.Features.Revisit.Commands.CreateRevisitRequest;
using Coolzo.Application.Features.Revisit.Queries.GetRevisitByBooking;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Revisit;
using Coolzo.Contracts.Responses.Revisit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/revisit")]
public sealed class RevisitController : ApiControllerBase
{
    private readonly ISender _sender;

    public RevisitController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("request")]
    [ProducesResponseType(typeof(ApiResponse<RevisitRequestResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RevisitRequestResponse>>> CreateRequestAsync(
        [FromBody] RevisitRequestCreateRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateRevisitRequestCommand(
                request.OriginalJobCardId,
                request.RevisitType,
                request.PreferredVisitDateUtc,
                request.IssueSummary,
                request.RequestRemarks,
                request.CustomerAmcId,
                request.WarrantyClaimId,
                request.ChargeAmount),
            cancellationToken);

        return Success(response, "Revisit request created successfully.");
    }

    [HttpGet("booking/{bookingId:long}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<RevisitRequestResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<RevisitRequestResponse>>>> GetByBookingAsync(
        [FromRoute] long bookingId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetRevisitByBookingQuery(bookingId), cancellationToken);

        return Success(response);
    }
}
