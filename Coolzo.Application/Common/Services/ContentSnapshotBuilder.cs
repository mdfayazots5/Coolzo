using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.CMS;
using Coolzo.Shared.Constants;

namespace Coolzo.Application.Common.Services;

public sealed class ContentSnapshotBuilder : IContentSnapshotBuilder
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly IScreenImageSlotRepository _screenImageSlotRepository;
    private readonly ISystemSettingRepository _systemSettingRepository;

    public ContentSnapshotBuilder(
        IAdminConfigurationRepository adminConfigurationRepository,
        IScreenImageSlotRepository screenImageSlotRepository,
        ISystemSettingRepository systemSettingRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _screenImageSlotRepository = screenImageSlotRepository;
        _systemSettingRepository = systemSettingRepository;
    }

    public async Task<ContentSnapshotBody> BuildAsync(CancellationToken cancellationToken)
    {
        var theme = await BuildThemeAsync(cancellationToken);
        var content = await BuildContentAsync(cancellationToken);
        var images = await BuildImagesAsync(cancellationToken);

        // Masters are populated from a later phase; the contract shape is fixed now.
        var masters = new SnapshotMastersDto(Array.Empty<SnapshotBrandDto>());

        return new ContentSnapshotBody(theme, masters, content, images);
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
