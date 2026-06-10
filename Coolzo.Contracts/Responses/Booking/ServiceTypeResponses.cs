using Coolzo.Contracts.Responses.Admin;

namespace Coolzo.Contracts.Responses.Booking;

public sealed record ServiceTypeListItemResponse(
    long Id,
    string Name,
    string Description,
    string Category,
    decimal BasePrice,
    int EstimatedDurationInMinutes,
    string IconKey,
    string ImageUrl);

public sealed record ServiceTypeDetailResponse(
    long Id,
    string Name,
    string Description,
    string Category,
    decimal BasePrice,
    int EstimatedDurationInMinutes,
    string IconKey,
    string ImageUrl,
    IReadOnlyCollection<ServiceTypeSubTypeResponse> SubTypes,
    IReadOnlyCollection<CMSFaqResponse> Faqs);

public sealed record ServiceTypeSubTypeResponse(
    string Id,
    string Name,
    string Description);
