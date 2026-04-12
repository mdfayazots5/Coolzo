using Asp.Versioning;
using Coolzo.Application.Features.CustomerApp;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Customer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/customer-technicians")]
public sealed class CustomerTechnicianController : ApiControllerBase
{
    private readonly ISender _sender;

    public CustomerTechnicianController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{technicianId:long}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerVisibleTechnicianResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerVisibleTechnicianResponse>>> GetTechnicianAsync(
        [FromRoute] long technicianId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetCustomerVisibleTechnicianQuery(technicianId), cancellationToken);
        return Success(response);
    }
}
