using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.Booking.Queries.SearchBookings;

public sealed class SearchBookingsQueryHandler : IRequestHandler<SearchBookingsQuery, PagedResult<BookingListItemResponse>>
{
    private readonly IBookingRepository _bookingRepository;

    public SearchBookingsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<PagedResult<BookingListItemResponse>> Handle(SearchBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _bookingRepository.SearchAsync(
            request.BookingReference,
            request.CustomerMobile,
            request.BookingDate,
            request.ServiceId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var totalCount = await _bookingRepository.CountSearchAsync(
            request.BookingReference,
            request.CustomerMobile,
            request.BookingDate,
            request.ServiceId,
            cancellationToken);

        return new PagedResult<BookingListItemResponse>(
            bookings.Select(BookingResponseMapper.ToListItem).ToArray(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
