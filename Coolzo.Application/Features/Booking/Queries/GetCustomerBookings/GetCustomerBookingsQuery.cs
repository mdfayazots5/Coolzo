using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.Booking.Queries.GetCustomerBookings;

public sealed record GetCustomerBookingsQuery(int PageNumber, int PageSize) : IRequest<PagedResult<BookingListItemResponse>>;
