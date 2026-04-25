using Coolzo.Application.Features.TechnicianJob.Queries.GetTechnicianJobDetail;
using Coolzo.Application.Features.TechnicianJob.Queries.GetTechnicianJobList;
using Coolzo.Application.Features.TechnicianJob.Queries.GetTechnicianOwnJobList;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.TechnicianJobs;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize(Roles = RoleNames.Technician)]
[Route("api/technician-jobs")]
public sealed class TechnicianJobController : ApiControllerBase
{
    private readonly ISender _sender;

    public TechnicianJobController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TechnicianJobListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<TechnicianJobListItemResponse>>>> GetTechnicianJobsAsync(
        [FromQuery] string? status,
        [FromQuery] DateOnly? slotDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new GetTechnicianJobListQuery(status, slotDate, pageNumber, pageSize),
            cancellationToken);

        return Success(response);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianJobDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianJobDetailResponse>>> GetTechnicianJobByIdAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetTechnicianJobDetailQuery(id), cancellationToken);

        return Success(response);
    }

    [HttpGet("my-jobs")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TechnicianJobListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<TechnicianJobListItemResponse>>>> GetMyJobsAsync(
        [FromQuery] string? status,
        [FromQuery] DateOnly? slotDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new GetTechnicianOwnJobListQuery(status, slotDate, pageNumber, pageSize),
            cancellationToken);

        return Success(response);
    }
}
