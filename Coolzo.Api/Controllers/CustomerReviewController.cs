using Asp.Versioning;
using Coolzo.Application.Features.CustomerApp;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Customer;
using Coolzo.Contracts.Responses.Customer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/customer-reviews")]
public sealed class CustomerReviewController : ApiControllerBase
{
    private readonly ISender _sender;

    public CustomerReviewController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<CustomerReviewResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CustomerReviewResponse>>>> GetReviewsAsync(
        [FromQuery] long? serviceId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetCustomerReviewsQuery(serviceId), cancellationToken);
        return Success(response);
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerReviewResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerReviewResponse>>> CreateReviewAsync(
        [FromBody] CreateCustomerReviewRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateMyCustomerReviewCommand(
                request.Rating,
                request.Comment,
                request.BookingId,
                request.ServiceId,
                request.CustomerPhotoUrl),
            cancellationToken);

        return Success(response, "Customer review submitted successfully.");
    }
}
