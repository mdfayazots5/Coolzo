namespace Coolzo.Contracts.Responses.Booking;

public sealed record ServiceLookupResponse(
    long ServiceId,
    long ServiceCategoryId,
    string ServiceName,
    string Summary,
    decimal BasePrice,
    string PricingModelName);
