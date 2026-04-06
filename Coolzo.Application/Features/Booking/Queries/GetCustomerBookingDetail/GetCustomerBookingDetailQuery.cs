using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.Booking.Queries.GetCustomerBookingDetail;

public sealed record GetCustomerBookingDetailQuery(long BookingId) : IRequest<BookingDetailResponse>;
