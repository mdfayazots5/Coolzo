using Coolzo.Application.Features.CommunicationPreference.Commands.CreateOrUpdateCommunicationPreference;
using Coolzo.Application.Features.CommunicationPreference.Queries.GetCommunicationPreferenceByCustomer;
using Coolzo.Application.Features.CommunicationPreference.Queries.GetMyCommunicationPreference;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Admin;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/communication-preferences")]
public sealed class CommunicationPreferenceController : ApiControllerBase
{
    private readonly ISender _sender;

    public CommunicationPreferenceController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<CommunicationPreferenceResponse>>> GetMineAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetMyCommunicationPreferenceQuery(), cancellationToken);

        return Success(response);
    }

    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<CommunicationPreferenceResponse>>> UpdateMineAsync(
        [FromBody] CommunicationPreferenceUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateOrUpdateCommunicationPreferenceCommand(
                request.EmailEnabled,
                request.SmsEnabled,
                request.WhatsAppEnabled,
                request.PushEnabled,
                request.AllowPromotionalContent,
                request.EmailAddress,
                request.MobileNumber),
            cancellationToken);

        return Success(response, "Communication preferences updated successfully.");
    }

    [HttpGet("customer/{customerId:long}")]
    [Authorize(Policy = PermissionNames.CommunicationPreferenceRead)]
    public async Task<ActionResult<ApiResponse<CommunicationPreferenceResponse>>> GetByCustomerAsync(
        [FromRoute] long customerId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetCommunicationPreferenceByCustomerQuery(customerId), cancellationToken);

        return Success(response);
    }
}
