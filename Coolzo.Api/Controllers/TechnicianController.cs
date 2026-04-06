using Asp.Versioning;
using Coolzo.Application.Features.Technician.Queries.GetTechnicianAvailability;
using Coolzo.Application.Features.Technician.Queries.GetTechnicianList;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize(Policy = PermissionNames.TechnicianRead)]
[Route("api/v{version:apiVersion}/technicians")]
public sealed class TechnicianController : ApiControllerBase
{
    private readonly ISender _sender;

    public TechnicianController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TechnicianListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TechnicianListItemResponse>>>> GetTechniciansAsync(
        [FromQuery] string? searchTerm,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetTechnicianListQuery(searchTerm, activeOnly), cancellationToken);

        return Success(response);
    }

    [HttpGet("availability")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TechnicianAvailabilityResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TechnicianAvailabilityResponse>>>> GetTechnicianAvailabilityAsync(
        [FromQuery] long serviceRequestId,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetTechnicianAvailabilityQuery(serviceRequestId), cancellationToken);

        return Success(response);
    }
}
