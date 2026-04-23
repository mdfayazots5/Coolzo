using Asp.Versioning;
using Coolzo.Application.Features.Amc.Commands.AssignAmcToCustomer;
using Coolzo.Application.Features.Amc.Commands.CreateAmcPlan;
using Coolzo.Application.Features.Amc.Commands.GenerateAmcVisits;
using Coolzo.Application.Features.Amc.Commands.UpdateAmcPlan;
using Coolzo.Application.Features.Amc.Queries.GetAmcPlanById;
using Coolzo.Application.Features.Amc.Queries.GetAmcPlans;
using Coolzo.Application.Features.Amc.Queries.GetCustomerAmc;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Amc;
using Coolzo.Contracts.Responses.Amc;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/amc")]
public sealed class AmcController : ApiControllerBase
{
    private readonly ISender _sender;
    private readonly Coolzo.Application.Features.Amc.ServiceLifecycleAccessService _serviceLifecycleAccessService;

    public AmcController(
        ISender sender,
        Coolzo.Application.Features.Amc.ServiceLifecycleAccessService serviceLifecycleAccessService)
    {
        _sender = sender;
        _serviceLifecycleAccessService = serviceLifecycleAccessService;
    }

    [Authorize(Policy = PermissionNames.AmcCreate)]
    [HttpPost("plans")]
    [ProducesResponseType(typeof(ApiResponse<AmcPlanResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AmcPlanResponse>>> CreatePlanAsync(
        [FromBody] CreateAmcPlanRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateAmcPlanCommand(
                request.PlanName,
                request.PlanDescription,
                request.DurationInMonths,
                request.VisitCount,
                request.PriceAmount,
                request.IsActive,
                request.TermsAndConditions),
            cancellationToken);

        return Success(response, "AMC plan created successfully.");
    }

    [Authorize(Policy = PermissionNames.AmcCreate)]
    [HttpPut("plans/{amcPlanId:long}")]
    [ProducesResponseType(typeof(ApiResponse<AmcPlanResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AmcPlanResponse>>> UpdatePlanAsync(
        [FromRoute] long amcPlanId,
        [FromBody] UpdateAmcPlanRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateAmcPlanCommand(
                amcPlanId,
                request.PlanName,
                request.PlanDescription,
                request.DurationInMonths,
                request.VisitCount,
                request.PriceAmount,
                request.IsActive,
                request.TermsAndConditions),
            cancellationToken);

        return Success(response, "AMC plan updated successfully.");
    }

    [Authorize]
    [HttpGet("plans")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AmcPlanResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<AmcPlanResponse>>>> GetPlansAsync(
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetAmcPlansQuery(isActive, pageNumber, pageSize), cancellationToken);

        return Success(response);
    }

    [Authorize]
    [HttpGet("plans/{amcPlanId:long}")]
    [ProducesResponseType(typeof(ApiResponse<AmcPlanResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AmcPlanResponse>>> GetPlanByIdAsync(
        [FromRoute] long amcPlanId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetAmcPlanByIdQuery(amcPlanId), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.AmcAssign)]
    [HttpPost("assign")]
    [ProducesResponseType(typeof(ApiResponse<CustomerAmcResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerAmcResponse>>> AssignAsync(
        [FromBody] AssignAmcToCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new AssignAmcToCustomerCommand(
                request.CustomerId,
                request.AmcPlanId,
                request.JobCardId,
                request.InvoiceId,
                request.StartDateUtc,
                request.Remarks),
            cancellationToken);

        return Success(response, "AMC assigned to customer successfully.");
    }

    [Authorize(Policy = PermissionNames.AmcAssign)]
    [HttpPost("customer/{customerAmcId:long}/generate-visits")]
    [ProducesResponseType(typeof(ApiResponse<CustomerAmcResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerAmcResponse>>> GenerateVisitsAsync(
        [FromRoute] long customerAmcId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GenerateAmcVisitsCommand(customerAmcId), cancellationToken);

        return Success(response, "AMC visits generated successfully.");
    }

    [Authorize]
    [HttpGet("customer/{customerId:long}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<CustomerAmcResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CustomerAmcResponse>>>> GetCustomerSubscriptionsAsync(
        [FromRoute] long customerId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetCustomerAmcQuery(customerId), cancellationToken);

        return Success(response);
    }

    [Authorize]
    [HttpGet("customer/me")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<CustomerAmcResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CustomerAmcResponse>>>> GetCurrentCustomerSubscriptionsAsync(
        CancellationToken cancellationToken)
    {
        var customerId = await _serviceLifecycleAccessService.GetCurrentCustomerIdAsync(cancellationToken);
        var response = await _sender.Send(new GetCustomerAmcQuery(customerId), cancellationToken);

        return Success(response);
    }
}
