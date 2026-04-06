using Asp.Versioning;
using Coolzo.Application.Features.Booking.Queries.GetCustomerBookingDetail;
using Coolzo.Application.Features.Booking.Queries.GetCustomerBookings;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Booking;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/customer-bookings")]
public sealed class CustomerBookingController : ApiControllerBase
{
    private readonly ISender _sender;

    public CustomerBookingController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{bookingId:long}")]
    [ProducesResponseType(typeof(ApiResponse<BookingDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BookingDetailResponse>>> GetCustomerBookingByIdAsync(
        [FromRoute] long bookingId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetCustomerBookingDetailQuery(bookingId), cancellationToken);

        return Success(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BookingListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<BookingListItemResponse>>>> GetCustomerBookingsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetCustomerBookingsQuery(pageNumber, pageSize), cancellationToken);

        return Success(response);
    }
}
