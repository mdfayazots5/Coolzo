using Coolzo.Application.Features.CustomerApp;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Customer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Route("api/technicians")]
public sealed class TechnicianPublicController : ApiControllerBase
{
    private readonly ISender _sender;

    public TechnicianPublicController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpGet("{technicianId:long}/public")]
    [ProducesResponseType(typeof(ApiResponse<CustomerVisibleTechnicianResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerVisibleTechnicianResponse>>> GetPublicProfileAsync(
        [FromRoute] long technicianId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetCustomerVisibleTechnicianQuery(technicianId), cancellationToken);
        return Success(response);
    }
}
