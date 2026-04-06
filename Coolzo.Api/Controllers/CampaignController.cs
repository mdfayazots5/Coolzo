using Asp.Versioning;
using Coolzo.Application.Features.GapPhaseA.Campaign;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.GapPhaseA;
using Coolzo.Contracts.Responses.GapPhaseA;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize(Policy = PermissionNames.BookingCreate)]
[Route("api/v{version:apiVersion}/campaigns")]
public sealed class CampaignController : ApiControllerBase
{
    private readonly ISender _sender;

    public CampaignController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CampaignResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CampaignResponse>>> CreateAsync(
        [FromBody] CreateCampaignRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateCampaignCommand(
                request.CampaignName,
                request.ServiceId,
                request.ZoneId,
                request.SlotAvailabilityId,
                request.PlannedBookingCount,
                request.StartDateUtc,
                request.EndDateUtc,
                request.Notes),
            cancellationToken);

        return Success(response, "Campaign created successfully.");
    }
}
