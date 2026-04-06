using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetAvailableSlots;

public sealed record GetAvailableSlotsQuery(long ZoneId, DateOnly SlotDate) : IRequest<IReadOnlyCollection<SlotAvailabilityResponse>>;
