using Coolzo.Application.Features.ServiceHistory.Queries.GetServiceHistory;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.ServiceHistory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/service-history")]
public sealed class ServiceHistoryController : ApiControllerBase
{
    private readonly ISender _sender;
    private readonly Coolzo.Application.Features.Amc.ServiceLifecycleAccessService _serviceLifecycleAccessService;

    public ServiceHistoryController(
        ISender sender,
        Coolzo.Application.Features.Amc.ServiceLifecycleAccessService serviceLifecycleAccessService)
    {
        _sender = sender;
        _serviceLifecycleAccessService = serviceLifecycleAccessService;
    }

    [HttpGet("customer/{customerId:long}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ServiceHistoryItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ServiceHistoryItemResponse>>>> GetByCustomerAsync(
        [FromRoute] long customerId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetServiceHistoryQuery(customerId), cancellationToken);

        return Success(response);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ServiceHistoryItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ServiceHistoryItemResponse>>>> GetForCurrentCustomerAsync(
        CancellationToken cancellationToken)
    {
        var customerId = await _serviceLifecycleAccessService.GetCurrentCustomerIdAsync(cancellationToken);
        var response = await _sender.Send(new GetServiceHistoryQuery(customerId), cancellationToken);

        return Success(response);
    }
}
