using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetAvailableSlots;

public sealed class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, IReadOnlyCollection<SlotAvailabilityResponse>>
{
    private readonly IBookingLookupRepository _bookingLookupRepository;

    public GetAvailableSlotsQueryHandler(IBookingLookupRepository bookingLookupRepository)
    {
        _bookingLookupRepository = bookingLookupRepository;
    }

    public async Task<IReadOnlyCollection<SlotAvailabilityResponse>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        var slots = await _bookingLookupRepository.ListAvailableSlotsAsync(request.ZoneId, request.SlotDate, cancellationToken);

        return slots
            .Select(slot => new SlotAvailabilityResponse(
                slot.SlotAvailabilityId,
                slot.ZoneId,
                slot.SlotDate,
                slot.SlotConfiguration?.SlotLabel ?? "Preferred Slot",
                slot.SlotConfiguration?.StartTime.ToString("HH\\:mm") ?? string.Empty,
                slot.SlotConfiguration?.EndTime.ToString("HH\\:mm") ?? string.Empty,
                slot.AvailableCapacity,
                slot.ReservedCapacity,
                !slot.IsBlocked && slot.ReservedCapacity < slot.AvailableCapacity))
            .ToArray();
    }
}
