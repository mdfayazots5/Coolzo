using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.Booking.Queries.SearchBookings;

public sealed record SearchBookingsQuery(
    string? BookingReference,
    string? CustomerMobile,
    DateOnly? BookingDate,
    long? ServiceId,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<BookingListItemResponse>>;
