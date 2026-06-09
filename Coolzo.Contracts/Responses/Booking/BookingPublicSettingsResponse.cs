namespace Coolzo.Contracts.Responses.Booking;

public sealed record BookingPublicSettingsResponse(
    bool OpenBookingMode,
    bool EnforceSlotCapacity
);
