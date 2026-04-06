using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class AdminConfigurationRepository : IAdminConfigurationRepository
{
    private readonly CoolzoDbContext _dbContext;

    public AdminConfigurationRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddSystemConfigurationAsync(SystemConfiguration systemConfiguration, CancellationToken cancellationToken)
    {
        return _dbContext.SystemConfigurations.AddAsync(systemConfiguration, cancellationToken).AsTask();
    }

    public Task<SystemConfiguration?> GetSystemConfigurationByIdAsync(long systemConfigurationId, CancellationToken cancellationToken)
    {
        return _dbContext.SystemConfigurations.FirstOrDefaultAsync(
            entity => entity.SystemConfigurationId == systemConfigurationId && !entity.IsDeleted,
            cancellationToken);
    }

    public Task<SystemConfiguration?> GetSystemConfigurationByGroupAndKeyAsync(string configurationGroup, string configurationKey, long? excludedId, CancellationToken cancellationToken)
    {
        return _dbContext.SystemConfigurations.FirstOrDefaultAsync(
            entity =>
                !entity.IsDeleted &&
                entity.ConfigurationGroup == configurationGroup &&
                entity.ConfigurationKey == configurationKey &&
                (!excludedId.HasValue || entity.SystemConfigurationId != excludedId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<SystemConfiguration>> SearchSystemConfigurationsAsync(
        string? configurationGroup,
        string? configurationKey,
        string? valueType,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.SystemConfigurations
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(configurationGroup))
        {
            query = query.Where(entity => entity.ConfigurationGroup == configurationGroup.Trim());
        }

        if (!string.IsNullOrWhiteSpace(configurationKey))
        {
            var normalizedKey = configurationKey.Trim();
            query = query.Where(entity => entity.ConfigurationKey.Contains(normalizedKey));
        }

        if (!string.IsNullOrWhiteSpace(valueType))
        {
            query = query.Where(entity => entity.ValueType == valueType.Trim());
        }

        if (isActive.HasValue)
        {
            query = query.Where(entity => entity.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(entity => entity.ConfigurationGroup)
            .ThenBy(entity => entity.ConfigurationKey)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddBusinessHourAsync(BusinessHourConfiguration businessHourConfiguration, CancellationToken cancellationToken)
    {
        return _dbContext.BusinessHourConfigurations.AddAsync(businessHourConfiguration, cancellationToken).AsTask();
    }

    public Task<BusinessHourConfiguration?> GetBusinessHourByDayOfWeekAsync(int dayOfWeekNumber, CancellationToken cancellationToken)
    {
        return _dbContext.BusinessHourConfigurations.FirstOrDefaultAsync(
            entity => entity.DayOfWeekNumber == dayOfWeekNumber && !entity.IsDeleted,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<BusinessHourConfiguration>> GetBusinessHoursAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.BusinessHourConfigurations
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted)
            .OrderBy(entity => entity.DayOfWeekNumber)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddHolidayAsync(HolidayConfiguration holidayConfiguration, CancellationToken cancellationToken)
    {
        return _dbContext.HolidayConfigurations.AddAsync(holidayConfiguration, cancellationToken).AsTask();
    }

    public Task<HolidayConfiguration?> GetHolidayByDateAsync(DateOnly holidayDate, long? excludedId, CancellationToken cancellationToken)
    {
        return _dbContext.HolidayConfigurations.FirstOrDefaultAsync(
            entity =>
                !entity.IsDeleted &&
                entity.HolidayDate == holidayDate &&
                (!excludedId.HasValue || entity.HolidayConfigurationId != excludedId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<HolidayConfiguration>> SearchHolidaysAsync(int? year, bool? isActive, CancellationToken cancellationToken)
    {
        var query = _dbContext.HolidayConfigurations
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted);

        if (year.HasValue)
        {
            query = query.Where(entity => entity.HolidayDate.Year == year.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(entity => entity.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(entity => entity.HolidayDate)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddDynamicMasterRecordAsync(DynamicMasterRecord dynamicMasterRecord, CancellationToken cancellationToken)
    {
        return _dbContext.DynamicMasterRecords.AddAsync(dynamicMasterRecord, cancellationToken).AsTask();
    }

    public Task<DynamicMasterRecord?> GetDynamicMasterRecordByIdAsync(long dynamicMasterRecordId, CancellationToken cancellationToken)
    {
        return _dbContext.DynamicMasterRecords.FirstOrDefaultAsync(
            entity => entity.DynamicMasterRecordId == dynamicMasterRecordId && !entity.IsDeleted,
            cancellationToken);
    }

    public Task<DynamicMasterRecord?> GetDynamicMasterRecordByTypeAndCodeAsync(string masterType, string masterCode, long? excludedId, CancellationToken cancellationToken)
    {
        return _dbContext.DynamicMasterRecords.FirstOrDefaultAsync(
            entity =>
                !entity.IsDeleted &&
                entity.MasterType == masterType &&
                entity.MasterCode == masterCode &&
                (!excludedId.HasValue || entity.DynamicMasterRecordId != excludedId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<DynamicMasterRecord>> SearchDynamicMasterRecordsAsync(
        string? masterType,
        string? search,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.DynamicMasterRecords
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(masterType))
        {
            query = query.Where(entity => entity.MasterType == masterType.Trim());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(
                entity =>
                    entity.MasterCode.Contains(normalizedSearch) ||
                    entity.MasterLabel.Contains(normalizedSearch) ||
                    entity.MasterValue.Contains(normalizedSearch));
        }

        if (isActive.HasValue)
        {
            query = query.Where(entity => entity.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(entity => entity.MasterType)
            .ThenBy(entity => entity.SortOrder)
            .ThenBy(entity => entity.MasterLabel)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddCmsBlockAsync(CMSBlock cmsBlock, CancellationToken cancellationToken)
    {
        return _dbContext.CMSBlocks.AddAsync(cmsBlock, cancellationToken).AsTask();
    }

    public Task<CMSBlock?> GetCmsBlockByIdAsync(long cmsBlockId, CancellationToken cancellationToken)
    {
        return _dbContext.CMSBlocks.FirstOrDefaultAsync(
            entity => entity.CMSBlockId == cmsBlockId && !entity.IsDeleted,
            cancellationToken);
    }

    public Task<CMSBlock?> GetCmsBlockByKeyAsync(string blockKey, bool publishedOnly, CancellationToken cancellationToken)
    {
        var query = _dbContext.CMSBlocks
            .AsNoTracking()
            .Where(entity => entity.BlockKey == blockKey && !entity.IsDeleted);

        if (publishedOnly)
        {
            query = query.Where(entity => entity.IsPublished && entity.IsActive);
        }

        return query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> CmsBlockKeyExistsAsync(string blockKey, long? excludedId, CancellationToken cancellationToken)
    {
        return _dbContext.CMSBlocks.AnyAsync(
            entity =>
                !entity.IsDeleted &&
                entity.BlockKey == blockKey &&
                (!excludedId.HasValue || entity.CMSBlockId != excludedId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<CMSBlock>> SearchCmsBlocksAsync(string? search, bool? isActive, bool? isPublished, CancellationToken cancellationToken)
    {
        var query = _dbContext.CMSBlocks
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(entity => entity.BlockKey.Contains(normalizedSearch) || entity.Title.Contains(normalizedSearch));
        }

        if (isActive.HasValue)
        {
            query = query.Where(entity => entity.IsActive == isActive.Value);
        }

        if (isPublished.HasValue)
        {
            query = query.Where(entity => entity.IsPublished == isPublished.Value);
        }

        return await query
            .OrderBy(entity => entity.SortOrder)
            .ThenBy(entity => entity.Title)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddCmsContentVersionAsync(CMSContentVersion cmsContentVersion, CancellationToken cancellationToken)
    {
        return _dbContext.CMSContentVersions.AddAsync(cmsContentVersion, cancellationToken).AsTask();
    }

    public Task AddCmsBannerAsync(CMSBanner cmsBanner, CancellationToken cancellationToken)
    {
        return _dbContext.CMSBanners.AddAsync(cmsBanner, cancellationToken).AsTask();
    }

    public Task<CMSBanner?> GetCmsBannerByIdAsync(long cmsBannerId, CancellationToken cancellationToken)
    {
        return _dbContext.CMSBanners.FirstOrDefaultAsync(
            entity => entity.CMSBannerId == cmsBannerId && !entity.IsDeleted,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<CMSBanner>> SearchCmsBannersAsync(
        string? search,
        bool? isActive,
        bool? isPublished,
        bool publicOnly,
        DateOnly? activeDate,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.CMSBanners
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(entity => entity.BannerTitle.Contains(normalizedSearch) || entity.DisplayArea.Contains(normalizedSearch));
        }

        if (isActive.HasValue)
        {
            query = query.Where(entity => entity.IsActive == isActive.Value);
        }

        if (isPublished.HasValue)
        {
            query = query.Where(entity => entity.IsPublished == isPublished.Value);
        }

        if (publicOnly)
        {
            var effectiveDate = activeDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            query = query.Where(
                entity =>
                    entity.IsActive &&
                    entity.IsPublished &&
                    (!entity.ActiveFromDate.HasValue || entity.ActiveFromDate.Value <= effectiveDate) &&
                    (!entity.ActiveToDate.HasValue || entity.ActiveToDate.Value >= effectiveDate));
        }

        return await query
            .OrderBy(entity => entity.SortOrder)
            .ThenBy(entity => entity.BannerTitle)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddCmsFaqAsync(CMSFaq cmsFaq, CancellationToken cancellationToken)
    {
        return _dbContext.CMSFaqs.AddAsync(cmsFaq, cancellationToken).AsTask();
    }

    public Task<CMSFaq?> GetCmsFaqByIdAsync(long cmsFaqId, CancellationToken cancellationToken)
    {
        return _dbContext.CMSFaqs.FirstOrDefaultAsync(
            entity => entity.CMSFaqId == cmsFaqId && !entity.IsDeleted,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<CMSFaq>> SearchCmsFaqsAsync(
        string? category,
        string? search,
        bool? isActive,
        bool? isPublished,
        bool publicOnly,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.CMSFaqs
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(entity => entity.Category == category.Trim());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(entity => entity.Question.Contains(normalizedSearch) || entity.Answer.Contains(normalizedSearch));
        }

        if (isActive.HasValue)
        {
            query = query.Where(entity => entity.IsActive == isActive.Value);
        }

        if (isPublished.HasValue)
        {
            query = query.Where(entity => entity.IsPublished == isPublished.Value);
        }

        if (publicOnly)
        {
            query = query.Where(entity => entity.IsActive && entity.IsPublished);
        }

        return await query
            .OrderBy(entity => entity.Category)
            .ThenBy(entity => entity.SortOrder)
            .ThenBy(entity => entity.Question)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<DisplayContentSetting>> SearchDisplayContentSettingsAsync(
        string? contentGroup,
        bool? isActive,
        bool publicOnly,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.DisplayContentSettings
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(contentGroup))
        {
            query = query.Where(entity => entity.ContentGroup == contentGroup.Trim());
        }

        if (isActive.HasValue)
        {
            query = query.Where(entity => entity.IsActive == isActive.Value);
        }

        if (publicOnly)
        {
            query = query.Where(entity => entity.IsActive && entity.IsPublished);
        }

        return await query
            .OrderBy(entity => entity.ContentGroup)
            .ThenBy(entity => entity.SortOrder)
            .ThenBy(entity => entity.ContentKey)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddNotificationTemplateAsync(NotificationTemplate notificationTemplate, CancellationToken cancellationToken)
    {
        return _dbContext.NotificationTemplates.AddAsync(notificationTemplate, cancellationToken).AsTask();
    }

    public Task<NotificationTemplate?> GetNotificationTemplateByIdAsync(long notificationTemplateId, CancellationToken cancellationToken)
    {
        return _dbContext.NotificationTemplates.FirstOrDefaultAsync(
            entity => entity.NotificationTemplateId == notificationTemplateId && !entity.IsDeleted,
            cancellationToken);
    }

    public Task<NotificationTemplate?> GetNotificationTemplateByCodeAsync(string templateCode, long? excludedId, CancellationToken cancellationToken)
    {
        return _dbContext.NotificationTemplates.FirstOrDefaultAsync(
            entity =>
                !entity.IsDeleted &&
                entity.TemplateCode == templateCode &&
                (!excludedId.HasValue || entity.NotificationTemplateId != excludedId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<NotificationTemplate>> SearchNotificationTemplatesAsync(
        string? search,
        string? channel,
        string? triggerCode,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.NotificationTemplates
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(entity => entity.TemplateCode.Contains(normalizedSearch) || entity.TemplateName.Contains(normalizedSearch));
        }

        if (!string.IsNullOrWhiteSpace(channel))
        {
            query = query.Where(entity => entity.Channel == channel.Trim());
        }

        if (!string.IsNullOrWhiteSpace(triggerCode))
        {
            query = query.Where(entity => entity.TriggerCode == triggerCode.Trim());
        }

        if (isActive.HasValue)
        {
            query = query.Where(entity => entity.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(entity => entity.TriggerCode)
            .ThenBy(entity => entity.Channel)
            .ThenBy(entity => entity.TemplateName)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddNotificationTriggerAsync(NotificationTriggerConfiguration notificationTriggerConfiguration, CancellationToken cancellationToken)
    {
        return _dbContext.NotificationTriggerConfigurations.AddAsync(notificationTriggerConfiguration, cancellationToken).AsTask();
    }

    public Task<NotificationTriggerConfiguration?> GetNotificationTriggerByIdAsync(long notificationTriggerConfigurationId, CancellationToken cancellationToken)
    {
        return _dbContext.NotificationTriggerConfigurations.FirstOrDefaultAsync(
            entity => entity.NotificationTriggerConfigurationId == notificationTriggerConfigurationId && !entity.IsDeleted,
            cancellationToken);
    }

    public Task<NotificationTriggerConfiguration?> GetNotificationTriggerByCodeAsync(string triggerCode, long? excludedId, CancellationToken cancellationToken)
    {
        return _dbContext.NotificationTriggerConfigurations.FirstOrDefaultAsync(
            entity =>
                !entity.IsDeleted &&
                entity.TriggerCode == triggerCode &&
                (!excludedId.HasValue || entity.NotificationTriggerConfigurationId != excludedId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<NotificationTriggerConfiguration>> SearchNotificationTriggersAsync(
        string? search,
        bool? isEnabled,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.NotificationTriggerConfigurations
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(entity => entity.TriggerCode.Contains(normalizedSearch) || entity.TriggerName.Contains(normalizedSearch));
        }

        if (isEnabled.HasValue)
        {
            query = query.Where(entity => entity.IsEnabled == isEnabled.Value);
        }

        return await query
            .OrderBy(entity => entity.TriggerName)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddCommunicationPreferenceAsync(CommunicationPreference communicationPreference, CancellationToken cancellationToken)
    {
        return _dbContext.CommunicationPreferences.AddAsync(communicationPreference, cancellationToken).AsTask();
    }

    public Task<CommunicationPreference?> GetCommunicationPreferenceByCustomerIdAsync(long customerId, CancellationToken cancellationToken)
    {
        return _dbContext.CommunicationPreferences.FirstOrDefaultAsync(
            entity => entity.CustomerId == customerId && !entity.IsDeleted,
            cancellationToken);
    }
}
