using Coolzo.Contracts.Responses.Admin;
using Coolzo.Domain.Entities;

namespace Coolzo.Application.Features.ServiceCatalogAdmin;

/// <summary>Maps catalog entities to their admin response DTOs.</summary>
internal static class ServiceCatalogMapper
{
    public static ServiceCategoryAdminResponse ToCategoryResponse(ServiceCategory category, int serviceCount) =>
        new(
            category.ServiceCategoryId,
            category.CategoryCode,
            category.CategoryName,
            category.Description,
            category.ImageUrl,
            category.IsActive,
            category.SortOrder,
            serviceCount);

    public static ServiceAdminResponse ToServiceResponse(Service service) =>
        new(
            service.ServiceId,
            service.ServiceCategoryId,
            service.PricingModelId,
            service.PricingModel?.PricingModelName ?? string.Empty,
            service.ServiceCode,
            service.ServiceName,
            service.Summary,
            service.BasePrice,
            service.EstimatedDurationInMinutes,
            service.ImageUrl,
            service.IsActive,
            service.SortOrder);

    public static PricingModelLookupResponse ToPricingModelResponse(PricingModel model) =>
        new(model.PricingModelId, model.PricingModelName, model.BasePrice);

    /// <summary>Derives a stable uppercase code from a name when the admin leaves the code blank.</summary>
    public static string SlugCode(string name)
    {
        var chars = name.Trim().ToUpperInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray();
        var slug = new string(chars);
        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        return slug.Trim('-');
    }
}
