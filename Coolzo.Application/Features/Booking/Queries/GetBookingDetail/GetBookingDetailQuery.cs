using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.Booking.Queries.GetBookingDetail;

public sealed record GetBookingDetailQuery(long BookingId) : IRequest<BookingDetailResponse>;
