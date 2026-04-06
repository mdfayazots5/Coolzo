namespace Coolzo.Contracts.Responses.Admin;

public sealed record SystemConfigurationResponse(
    long SystemConfigurationId,
    string ConfigurationGroup,
    string ConfigurationKey,
    string ConfigurationValue,
    string ValueType,
    string Description,
    bool IsSensitive,
    bool IsActive,
    DateTime DateCreated,
    DateTime? LastUpdated);

public sealed record BusinessHourConfigurationResponse(
    long BusinessHourConfigurationId,
    int DayOfWeekNumber,
    string DayName,
    string? StartTimeLocal,
    string? EndTimeLocal,
    bool IsClosed);

public sealed record HolidayConfigurationResponse(
    long HolidayConfigurationId,
    string HolidayDate,
    string HolidayName,
    bool IsRecurringAnnually,
    bool IsActive);

public sealed record CMSBlockResponse(
    long CMSBlockId,
    string BlockKey,
    string Title,
    string Summary,
    string Content,
    string PreviewImageUrl,
    bool IsActive,
    bool IsPublished,
    int SortOrder,
    int VersionNumber,
    DateTime DateCreated,
    DateTime? LastUpdated);

public sealed record CMSBannerResponse(
    long CMSBannerId,
    string BannerTitle,
    string BannerSubtitle,
    string ImageUrl,
    string RedirectUrl,
    string DisplayArea,
    string? ActiveFromDate,
    string? ActiveToDate,
    bool IsActive,
    bool IsPublished,
    int SortOrder);

public sealed record CMSFaqResponse(
    long CMSFaqId,
    string Category,
    string Question,
    string Answer,
    bool IsActive,
    bool IsPublished,
    int SortOrder);

public sealed record DisplayContentSettingResponse(
    long DisplayContentSettingId,
    string ContentGroup,
    string ContentKey,
    string ContentValue,
    string ContentType,
    bool IsActive,
    bool IsPublished,
    int SortOrder);

public sealed record PublicHomeCMSContentResponse(
    IReadOnlyCollection<CMSBlockResponse> Blocks,
    IReadOnlyCollection<CMSBannerResponse> Banners,
    IReadOnlyCollection<CMSFaqResponse> Faqs,
    IReadOnlyCollection<DisplayContentSettingResponse> DisplaySettings);

public sealed record NotificationTemplateResponse(
    long NotificationTemplateId,
    string TemplateCode,
    string TemplateName,
    string TriggerCode,
    string Channel,
    string SubjectTemplate,
    string BodyTemplate,
    IReadOnlyCollection<string> AllowedMergeTags,
    bool IsActive,
    DateTime DateCreated,
    DateTime? LastUpdated);

public sealed record NotificationTriggerConfigurationResponse(
    long NotificationTriggerConfigurationId,
    string TriggerCode,
    string TriggerName,
    string Description,
    bool IsEnabled,
    bool EmailEnabled,
    bool SmsEnabled,
    bool WhatsAppEnabled,
    bool PushEnabled,
    int ReminderLeadMinutes,
    int DelayMinutes,
    DateTime DateCreated,
    DateTime? LastUpdated);

public sealed record CommunicationPreferenceResponse(
    long CommunicationPreferenceId,
    long CustomerId,
    string EmailAddress,
    string MobileNumber,
    bool EmailEnabled,
    bool SmsEnabled,
    bool WhatsAppEnabled,
    bool PushEnabled,
    bool AllowPromotionalContent,
    DateTime? LastUpdated);

public sealed record DynamicMasterRecordResponse(
    long DynamicMasterRecordId,
    string MasterType,
    string MasterCode,
    string MasterLabel,
    string MasterValue,
    string Description,
    bool IsActive,
    bool IsPublished,
    int SortOrder,
    DateTime DateCreated,
    DateTime? LastUpdated);
