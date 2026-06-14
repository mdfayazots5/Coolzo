namespace Coolzo.Contracts.Requests.Admin;

/// <summary>Create/update payload for a service category (tblServiceCategory).</summary>
public sealed record ServiceCategoryUpsertRequest(
    string CategoryName,
    string? CategoryCode,
    string? Description,
    string? ImageUrl,
    bool IsActive,
    int SortOrder);

/// <summary>Create/update payload for a bookable service (tblService).</summary>
public sealed record ServiceUpsertRequest(
    long ServiceCategoryId,
    long PricingModelId,
    string ServiceName,
    string? ServiceCode,
    string? Summary,
    decimal BasePrice,
    int EstimatedDurationInMinutes,
    string? ImageUrl,
    bool IsActive,
    int SortOrder);
