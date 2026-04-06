namespace Coolzo.Contracts.Requests.Booking;

public sealed record ServiceLookupRequest(long? ServiceCategoryId, string? Search);
