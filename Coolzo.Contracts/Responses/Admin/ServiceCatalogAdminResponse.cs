namespace Coolzo.Contracts.Responses.Admin;

/// <summary>A service category (tblServiceCategory) as managed in the admin catalog.</summary>
public sealed record ServiceCategoryAdminResponse(
    long ServiceCategoryId,
    string CategoryCode,
    string CategoryName,
    string Description,
    string? ImageUrl,
    string? ImageAIPrompt,
    bool IsActive,
    int SortOrder,
    int ServiceCount);

/// <summary>A bookable service (tblService) with the full editable field set for the admin catalog.</summary>
public sealed record ServiceAdminResponse(
    long ServiceId,
    long ServiceCategoryId,
    long PricingModelId,
    string PricingModelName,
    string ServiceCode,
    string ServiceName,
    string Summary,
    decimal BasePrice,
    int EstimatedDurationInMinutes,
    string? ImageUrl,
    string? ImageAIPrompt,
    bool IsActive,
    int SortOrder);

/// <summary>Pricing model option for the service editor's pricing dropdown.</summary>
public sealed record PricingModelLookupResponse(
    long PricingModelId,
    string PricingModelName,
    decimal BasePrice);

/// <summary>
/// Full admin catalog snapshot: every category and service (active AND inactive) plus the pricing-model
/// options, in one payload for the catalog accordion.
/// </summary>
public sealed record ServiceCatalogAdminResponse(
    IReadOnlyCollection<ServiceCategoryAdminResponse> Categories,
    IReadOnlyCollection<ServiceAdminResponse> Services,
    IReadOnlyCollection<PricingModelLookupResponse> PricingModels);
