using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IAdminConfigurationRepository
{
    Task AddSystemConfigurationAsync(SystemConfiguration systemConfiguration, CancellationToken cancellationToken);

    Task<SystemConfiguration?> GetSystemConfigurationByIdAsync(long systemConfigurationId, CancellationToken cancellationToken);

    Task<SystemConfiguration?> GetSystemConfigurationByGroupAndKeyAsync(string configurationGroup, string configurationKey, long? excludedId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SystemConfiguration>> SearchSystemConfigurationsAsync(
        string? configurationGroup,
        string? configurationKey,
        string? valueType,
        bool? isActive,
        CancellationToken cancellationToken);

    Task AddBusinessHourAsync(BusinessHourConfiguration businessHourConfiguration, CancellationToken cancellationToken);

    Task<BusinessHourConfiguration?> GetBusinessHourByDayOfWeekAsync(int dayOfWeekNumber, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<BusinessHourConfiguration>> GetBusinessHoursAsync(CancellationToken cancellationToken);

    Task AddHolidayAsync(HolidayConfiguration holidayConfiguration, CancellationToken cancellationToken);

    Task<HolidayConfiguration?> GetHolidayByDateAsync(DateOnly holidayDate, long? excludedId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<HolidayConfiguration>> SearchHolidaysAsync(int? year, bool? isActive, CancellationToken cancellationToken);

    Task AddDynamicMasterRecordAsync(DynamicMasterRecord dynamicMasterRecord, CancellationToken cancellationToken);

    Task<DynamicMasterRecord?> GetDynamicMasterRecordByIdAsync(long dynamicMasterRecordId, CancellationToken cancellationToken);

    Task<DynamicMasterRecord?> GetDynamicMasterRecordByTypeAndCodeAsync(string masterType, string masterCode, long? excludedId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DynamicMasterRecord>> SearchDynamicMasterRecordsAsync(
        string? masterType,
        string? search,
        bool? isActive,
        CancellationToken cancellationToken);

    Task AddCmsBlockAsync(CMSBlock cmsBlock, CancellationToken cancellationToken);

    Task<CMSBlock?> GetCmsBlockByIdAsync(long cmsBlockId, CancellationToken cancellationToken);

    Task<CMSBlock?> GetCmsBlockByKeyAsync(string blockKey, bool publishedOnly, CancellationToken cancellationToken);

    Task<bool> CmsBlockKeyExistsAsync(string blockKey, long? excludedId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CMSBlock>> SearchCmsBlocksAsync(string? search, bool? isActive, bool? isPublished, CancellationToken cancellationToken);

    Task AddCmsContentVersionAsync(CMSContentVersion cmsContentVersion, CancellationToken cancellationToken);

    Task AddCmsBannerAsync(CMSBanner cmsBanner, CancellationToken cancellationToken);

    Task<CMSBanner?> GetCmsBannerByIdAsync(long cmsBannerId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CMSBanner>> SearchCmsBannersAsync(
        string? search,
        bool? isActive,
        bool? isPublished,
        bool publicOnly,
        DateOnly? activeDate,
        CancellationToken cancellationToken);

    Task AddCmsFaqAsync(CMSFaq cmsFaq, CancellationToken cancellationToken);

    Task<CMSFaq?> GetCmsFaqByIdAsync(long cmsFaqId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CMSFaq>> SearchCmsFaqsAsync(
        string? category,
        string? search,
        bool? isActive,
        bool? isPublished,
        bool publicOnly,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DisplayContentSetting>> SearchDisplayContentSettingsAsync(
        string? contentGroup,
        bool? isActive,
        bool publicOnly,
        CancellationToken cancellationToken);

    Task AddNotificationTemplateAsync(NotificationTemplate notificationTemplate, CancellationToken cancellationToken);

    Task<NotificationTemplate?> GetNotificationTemplateByIdAsync(long notificationTemplateId, CancellationToken cancellationToken);

    Task<NotificationTemplate?> GetNotificationTemplateByCodeAsync(string templateCode, long? excludedId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<NotificationTemplate>> SearchNotificationTemplatesAsync(
        string? search,
        string? channel,
        string? triggerCode,
        bool? isActive,
        CancellationToken cancellationToken);

    Task AddNotificationTriggerAsync(NotificationTriggerConfiguration notificationTriggerConfiguration, CancellationToken cancellationToken);

    Task<NotificationTriggerConfiguration?> GetNotificationTriggerByIdAsync(long notificationTriggerConfigurationId, CancellationToken cancellationToken);

    Task<NotificationTriggerConfiguration?> GetNotificationTriggerByCodeAsync(string triggerCode, long? excludedId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<NotificationTriggerConfiguration>> SearchNotificationTriggersAsync(
        string? search,
        bool? isEnabled,
        CancellationToken cancellationToken);

    Task AddCommunicationPreferenceAsync(CommunicationPreference communicationPreference, CancellationToken cancellationToken);

    Task<CommunicationPreference?> GetCommunicationPreferenceByCustomerIdAsync(long customerId, CancellationToken cancellationToken);
}
