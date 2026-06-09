using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.Booking.Queries.GetBookingPublicSettings;

public sealed class GetBookingPublicSettingsQueryHandler
    : IRequestHandler<GetBookingPublicSettingsQuery, BookingPublicSettingsResponse>
{
    internal const string KeyOpenBookingMode    = "Booking.OpenBookingMode";
    internal const string KeyEnforceSlotCapacity = "Booking.EnforceSlotCapacity";

    private readonly ISystemSettingRepository _systemSettingRepository;

    public GetBookingPublicSettingsQueryHandler(ISystemSettingRepository systemSettingRepository)
    {
        _systemSettingRepository = systemSettingRepository;
    }

    public async Task<BookingPublicSettingsResponse> Handle(
        GetBookingPublicSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await _systemSettingRepository.GetByKeysAsync(
            new[] { KeyOpenBookingMode, KeyEnforceSlotCapacity },
            cancellationToken);

        var openBookingMode = settings.TryGetValue(KeyOpenBookingMode, out var obm)
            && bool.TryParse(obm.SettingValue, out var obmVal) && obmVal;

        var enforceSlotCapacity = !settings.TryGetValue(KeyEnforceSlotCapacity, out var esc)
            || !bool.TryParse(esc.SettingValue, out var escVal) || escVal;

        return new BookingPublicSettingsResponse(openBookingMode, enforceSlotCapacity);
    }
}
