namespace Coolzo.Contracts.Requests.GapPhaseA;

public sealed record CreateCampaignRequest(
    string CampaignName,
    long ServiceId,
    long ZoneId,
    long SlotAvailabilityId,
    int PlannedBookingCount,
    DateTime StartDateUtc,
    DateTime EndDateUtc,
    string? Notes);
