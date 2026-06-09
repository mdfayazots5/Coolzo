using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.Booking.Queries.GetBookingPublicSettings;

public sealed record GetBookingPublicSettingsQuery() : IRequest<BookingPublicSettingsResponse>;
