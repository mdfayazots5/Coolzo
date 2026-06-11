using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.Amc.Queries.GetAmcPlans;
using Coolzo.Application.Features.BookingLookup.Queries.GetAcTypes;
using Coolzo.Application.Features.BookingLookup.Queries.GetBrands;
using Coolzo.Application.Features.BookingLookup.Queries.GetServiceCategories;
using Coolzo.Application.Features.BookingLookup.Queries.GetServices;
using Coolzo.Application.Features.BookingLookup.Queries.GetTonnages;
using Coolzo.Contracts.Responses.CMS;
using Coolzo.Shared.Constants;
using MediatR;

namespace Coolzo.Application.Common.Services;

public sealed class ContentSnapshotBuilder : IContentSnapshotBuilder
{
    // Bundle the full public catalog; these lists are small masters, not paginated result sets.
    private const int MasterPageSize = 500;

    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly IScreenImageSlotRepository _screenImageSlotRepository;
    private readonly ISystemSettingRepository _systemSettingRepository;
    private readonly ISender _sender;

    public ContentSnapshotBuilder(
        IAdminConfigurationRepository adminConfigurationRepository,
        IScreenImageSlotRepository screenImageSlotRepository,
        ISystemSettingRepository systemSettingRepository,
        ISender sender)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _screenImageSlotRepository = screenImageSlotRepository;
        _systemSettingRepository = systemSettingRepository;
        _sender = sender;
    }

    public async Task<ContentSnapshotBody> BuildAsync(CancellationToken cancellationToken)
    {
        var theme = await BuildThemeAsync(cancellationToken);
        var content = await BuildContentAsync(cancellationToken);
        var images = await BuildImagesAsync(cancellationToken);
        var masters = await BuildMastersAsync(cancellationToken);

        return new ContentSnapshotBody(theme, masters, content, images);
    }

    /// <summary>
    /// Bundles the public catalog masters by reusing the existing booking-lookup / AMC queries — the
    /// same handlers the live endpoints use, so the snapshot can never drift from the live API shape.
    /// Only the static, public, AllowAnonymous lookups are included; zones-by-pincode and slots are
    /// parameterized/dynamic and stay live APIs.
    /// </summary>
    private async Task<SnapshotMastersDto> BuildMastersAsync(CancellationToken cancellationToken)
    {
        var serviceCategories = await _sender.Send(new GetServiceCategoriesQuery(null), cancellationToken);
        var services = await _sender.Send(new GetServicesQuery(null, null), cancellationToken);
        var acTypes = await _sender.Send(new GetAcTypesQuery(null), cancellationToken);
        var tonnages = await _sender.Send(new GetTonnagesQuery(null), cancellationToken);
        var brands = await _sender.Send(new GetBrandsQuery(null), cancellationToken);
        var amcPlans = await _sender.Send(new GetAmcPlansQuery(true, 1, MasterPageSize), cancellationToken);

        return new SnapshotMastersDto(
            serviceCategories,
            services,
            acTypes,
            tonnages,
            brands,
            amcPlans.Items);
    }

    private async Task<IReadOnlyDictionary<string, SnapshotImageDto>> BuildImagesAsync(CancellationToken cancellationToken)
    {
        var slots = await _screenImageSlotRepository.SearchAsync(null, true, cancellationToken);

        return slots
            .Where(slot => !string.IsNullOrWhiteSpace(slot.ImageUrl))
            .GroupBy(slot => $"{slot.PageKey}.{slot.SlotKey}")
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    var variants = group.ToDictionary(slot => slot.Breakpoint, slot => slot.ImageUrl);
                    var primary = group
                        .OrderBy(slot => slot.Breakpoint == SnapshotKeys.Breakpoints.Desktop ? 0 : 1)
                        .First();

                    return new SnapshotImageDto(primary.ImageUrl, primary.AltText, variants);
                });
    }

    private async Task<IReadOnlyDictionary<string, string>> BuildThemeAsync(CancellationToken cancellationToken)
    {
        var settings = await _systemSettingRepository.ListAsync(cancellationToken);

        return settings
            .Where(setting =>
                !setting.IsSensitive &&
                setting.SettingKey.StartsWith(SnapshotKeys.Theme.Prefix, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(setting => setting.SettingKey, setting => setting.SettingValue);
    }

    private async Task<SnapshotContentDto> BuildContentAsync(CancellationToken cancellationToken)
    {
        var blocks = await _adminConfigurationRepository.SearchCmsBlocksAsync(null, true, true, cancellationToken);
        var banners = await _adminConfigurationRepository.SearchCmsBannersAsync(null, true, true, true, null, cancellationToken);
        var faqs = await _adminConfigurationRepository.SearchCmsFaqsAsync(null, null, true, true, true, cancellationToken);

        var blockDtos = blocks
            .OrderBy(block => block.SortOrder)
            .Select(block => new SnapshotBlockDto(
                block.BlockKey,
                block.Title,
                block.Summary,
                block.Content,
                block.PreviewImageUrl,
                block.SortOrder))
            .ToArray();

        var bannerDtos = banners
            .OrderBy(banner => banner.SortOrder)
            .Select(banner => new SnapshotBannerDto(
                banner.BannerTitle,
                banner.BannerSubtitle,
                banner.ImageUrl,
                banner.RedirectUrl,
                banner.DisplayArea,
                banner.SortOrder))
            .ToArray();

        var faqDtos = faqs
            .OrderBy(faq => faq.SortOrder)
            .Select(faq => new SnapshotFaqDto(
                faq.Category,
                faq.Question,
                faq.Answer,
                faq.SortOrder))
            .ToArray();

        return new SnapshotContentDto(blockDtos, bannerDtos, faqDtos);
    }
}
