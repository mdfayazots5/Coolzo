namespace Coolzo.Contracts.Requests.Admin;

public sealed record SystemConfigurationUpsertRequest(
    string ConfigurationGroup,
    string ConfigurationKey,
    string ConfigurationValue,
    string ValueType,
    string? Description,
    bool IsSensitive,
    bool IsActive);

public sealed record BusinessHourItemRequest(
    int DayOfWeekNumber,
    TimeSpan? StartTimeLocal,
    TimeSpan? EndTimeLocal,
    bool IsClosed);

public sealed record SaveBusinessHoursRequest(IReadOnlyCollection<BusinessHourItemRequest> BusinessHours);

public sealed record CreateHolidayConfigurationRequest(
    DateOnly HolidayDate,
    string HolidayName,
    bool IsRecurringAnnually,
    bool IsActive);

public sealed record DynamicMasterRecordUpsertRequest(
    string MasterType,
    string MasterCode,
    string MasterLabel,
    string MasterValue,
    string? Description,
    bool IsActive,
    bool IsPublished,
    int SortOrder);

public sealed record CMSBlockUpsertRequest(
    string BlockKey,
    string Title,
    string? Summary,
    string Content,
    string? PreviewImageUrl,
    bool IsActive,
    bool IsPublished,
    int SortOrder);

public sealed record CMSBannerUpsertRequest(
    string BannerTitle,
    string? BannerSubtitle,
    string? ImageUrl,
    string? RedirectUrl,
    string? DisplayArea,
    DateOnly? ActiveFromDate,
    DateOnly? ActiveToDate,
    bool IsActive,
    bool IsPublished,
    int SortOrder);

public sealed record CMSFaqUpsertRequest(
    string Category,
    string Question,
    string Answer,
    bool IsActive,
    bool IsPublished,
    int SortOrder);

public sealed record NotificationTemplateUpsertRequest(
    string TemplateCode,
    string TemplateName,
    string TriggerCode,
    string Channel,
    string? SubjectTemplate,
    string BodyTemplate,
    IReadOnlyCollection<string> AllowedMergeTags,
    bool IsActive);

public sealed record NotificationTriggerUpsertRequest(
    string TriggerCode,
    string TriggerName,
    string? Description,
    bool IsEnabled,
    bool EmailEnabled,
    bool SmsEnabled,
    bool WhatsAppEnabled,
    bool PushEnabled,
    int ReminderLeadMinutes,
    int DelayMinutes);

public sealed record CommunicationPreferenceUpdateRequest(
    bool EmailEnabled,
    bool SmsEnabled,
    bool WhatsAppEnabled,
    bool PushEnabled,
    bool AllowPromotionalContent,
    string? EmailAddress,
    string? MobileNumber);
