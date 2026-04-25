using Coolzo.Application.Features.CustomerApp;
using Coolzo.Application.Features.Booking.Queries.GetCustomerBookingDetail;
using Coolzo.Application.Features.Booking.Queries.GetCustomerBookings;
using Coolzo.Api.Utilities;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Customer;
using Coolzo.Contracts.Responses.Booking;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/customer-bookings")]
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

    [HttpPost("{bookingId:long}/reschedule")]
    [ProducesResponseType(typeof(ApiResponse<BookingDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BookingDetailResponse>>> RescheduleCustomerBookingAsync(
        [FromRoute] long bookingId,
        [FromBody] RescheduleCustomerBookingRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new RescheduleMyCustomerBookingCommand(bookingId, request.SlotAvailabilityId, request.Remarks),
            cancellationToken);

        return Success(response, "Customer booking rescheduled successfully.");
    }

    [HttpGet("{bookingId:long}/service-report")]
    [ProducesResponseType(typeof(ApiResponse<BookingDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BookingDetailResponse>>> GetServiceReportAsync(
        [FromRoute] long bookingId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetCustomerBookingDetailQuery(bookingId), cancellationToken);
        return Success(response);
    }

    [HttpGet("{bookingId:long}/service-report/pdf")]
    public async Task<IActionResult> DownloadServiceReportPdfAsync(
        [FromRoute] long bookingId,
        CancellationToken cancellationToken)
    {
        var booking = await _sender.Send(new GetCustomerBookingDetailQuery(bookingId), cancellationToken);
        var lines = new List<string>
        {
            $"Service Report {booking.ServiceRequestNumber ?? booking.BookingReference}",
            $"Service: {booking.ServiceName}",
            $"Address: {booking.AddressSummary}",
            $"Slot: {booking.SlotDate:dd MMM yyyy} {booking.SlotLabel}",
            $"Status: {booking.OperationalStatus ?? booking.Status}",
            $"Summary: {booking.CompletionSummary ?? "Service report available in booking timeline."}"
        };

        foreach (var item in booking.FieldTimeline)
        {
            var text = !string.IsNullOrWhiteSpace(item.EventTitle)
                ? item.EventTitle
                : item.Remarks;

            if (!string.IsNullOrWhiteSpace(text))
            {
                lines.Add($"- {text}");
            }
        }

        var fileBytes = SimplePdfDocumentBuilder.Build(lines);
        var fileName = $"service-report-{booking.BookingReference}.pdf";

        return File(fileBytes, "application/pdf", fileName);
    }
}
