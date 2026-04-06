using Asp.Versioning;
using Coolzo.Application.Features.Booking.Commands.CreateCustomerBooking;
using Coolzo.Application.Features.Booking.Commands.CreateGuestBooking;
using Coolzo.Application.Features.Booking.Queries.GetBookingDetail;
using Coolzo.Application.Features.Booking.Queries.GetCustomerBookings;
using Coolzo.Application.Features.Booking.Queries.SearchBookings;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Booking;
using Coolzo.Contracts.Responses.Booking;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/bookings")]
public sealed class BookingController : ApiControllerBase
{
    private readonly ISender _sender;

    public BookingController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpPost("guest")]
    [ProducesResponseType(typeof(ApiResponse<BookingSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BookingSummaryResponse>>> CreateGuestBookingAsync(
        [FromBody] GuestBookingCreateRequest request,
        [FromHeader(Name = "X-Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateGuestBookingCommand(
                request.ServiceId,
                request.AcTypeId,
                request.TonnageId,
                request.BrandId,
                request.SlotAvailabilityId,
                request.CustomerName,
                request.MobileNumber,
                request.EmailAddress,
                request.AddressLine1,
                request.AddressLine2,
                request.Landmark,
                request.CityName,
                request.Pincode,
                request.AddressLabel,
                request.ModelName,
                request.IssueNotes,
                request.SourceChannel,
                idempotencyKey),
            cancellationToken);

        return Success(response, "Guest booking created successfully.");
    }

    [Authorize]
    [HttpPost("customer")]
    [ProducesResponseType(typeof(ApiResponse<BookingSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BookingSummaryResponse>>> CreateCustomerBookingAsync(
        [FromBody] CustomerBookingCreateRequest request,
        [FromHeader(Name = "X-Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateCustomerBookingCommand(
                request.ServiceId,
                request.AcTypeId,
                request.TonnageId,
                request.BrandId,
                request.SlotAvailabilityId,
                request.CustomerName,
                request.MobileNumber,
                request.EmailAddress,
                request.AddressLine1,
                request.AddressLine2,
                request.Landmark,
                request.CityName,
                request.Pincode,
                request.AddressLabel,
                request.ModelName,
                request.IssueNotes,
                request.SourceChannel,
                idempotencyKey),
            cancellationToken);

        return Success(response, "Customer booking created successfully.");
    }

    [Authorize(Policy = PermissionNames.BookingRead)]
    [HttpGet("{bookingId:long}")]
    [ProducesResponseType(typeof(ApiResponse<BookingDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BookingDetailResponse>>> GetBookingByIdAsync(
        [FromRoute] long bookingId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetBookingDetailQuery(bookingId), cancellationToken);

        return Success(response);
    }

    [Authorize]
    [HttpGet("my-bookings")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BookingListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<BookingListItemResponse>>>> GetMyBookingsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetCustomerBookingsQuery(pageNumber, pageSize), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.BookingRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BookingListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<BookingListItemResponse>>>> SearchAsync(
        [FromQuery] string? bookingReference,
        [FromQuery] string? customerMobile,
        [FromQuery] DateOnly? bookingDate,
        [FromQuery] long? serviceId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new SearchBookingsQuery(bookingReference, customerMobile, bookingDate, serviceId, pageNumber, pageSize),
            cancellationToken);

        return Success(response);
    }
}
