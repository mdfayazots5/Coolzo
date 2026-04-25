using Coolzo.Application.Features.Support.Queries.GetSupportTicketLookupData;
using Coolzo.Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/support-ticket-lookups")]
public sealed class SupportTicketLookupController : ApiControllerBase
{
    private readonly ISender _sender;

    public SupportTicketLookupController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<LookupItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<LookupItemResponse>>>> GetCategoriesAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetSupportTicketLookupDataQuery(), cancellationToken);

        return Success(response.Categories);
    }

    [HttpGet("~/api/support/categories")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<LookupItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<LookupItemResponse>>>> GetSupportCategoriesAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetSupportTicketLookupDataQuery(), cancellationToken);

        return Success(response.Categories);
    }

    [HttpGet("priorities")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<LookupItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<LookupItemResponse>>>> GetPrioritiesAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetSupportTicketLookupDataQuery(), cancellationToken);

        return Success(response.Priorities);
    }

    [HttpGet("statuses")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<LookupItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<LookupItemResponse>>>> GetStatusesAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetSupportTicketLookupDataQuery(), cancellationToken);

        return Success(response.Statuses);
    }
}
