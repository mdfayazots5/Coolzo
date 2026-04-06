using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Booking;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Booking.Queries.GetCustomerBookings;

public sealed class GetCustomerBookingsQueryHandler : IRequestHandler<GetCustomerBookingsQuery, PagedResult<BookingListItemResponse>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public GetCustomerBookingsQueryHandler(IBookingRepository bookingRepository, ICurrentUserContext currentUserContext)
    {
        _bookingRepository = bookingRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<PagedResult<BookingListItemResponse>> Handle(GetCustomerBookingsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
        {
            throw new AppException(ErrorCodes.Unauthorized, "Customer authentication is required.", 401);
        }

        var customer = await _bookingRepository.GetCustomerByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.BookingAccessDenied, "The current customer could not be resolved.", 403);

        var bookings = await _bookingRepository.ListByCustomerIdAsync(customer.CustomerId, request.PageNumber, request.PageSize, cancellationToken);
        var totalCount = await _bookingRepository.CountByCustomerIdAsync(customer.CustomerId, cancellationToken);

        return new PagedResult<BookingListItemResponse>(
            bookings.Select(BookingResponseMapper.ToListItem).ToArray(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
