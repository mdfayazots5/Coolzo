namespace Coolzo.Domain.Entities;

public sealed class SystemConfiguration : AuditableEntity
{
    public long SystemConfigurationId { get; set; }

    public string ConfigurationGroup { get; set; } = string.Empty;

    public string ConfigurationKey { get; set; } = string.Empty;

    public string ConfigurationValue { get; set; } = string.Empty;

    public string ValueType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsSensitive { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class BusinessHourConfiguration : AuditableEntity
{
    public long BusinessHourConfigurationId { get; set; }

    public int DayOfWeekNumber { get; set; }

    public TimeSpan? StartTimeLocal { get; set; }

    public TimeSpan? EndTimeLocal { get; set; }

    public bool IsClosed { get; set; }
}

public sealed class HolidayConfiguration : AuditableEntity
{
    public long HolidayConfigurationId { get; set; }

    public DateOnly HolidayDate { get; set; }

    public string HolidayName { get; set; } = string.Empty;

    public bool IsRecurringAnnually { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class CMSBlock : AuditableEntity
{
    public long CMSBlockId { get; set; }

    public string BlockKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string PreviewImageUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int VersionNumber { get; set; } = 1;
}

public sealed class CMSBanner : AuditableEntity
{
    public long CMSBannerId { get; set; }

    public string BannerTitle { get; set; } = string.Empty;

    public string BannerSubtitle { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string RedirectUrl { get; set; } = string.Empty;

    public string DisplayArea { get; set; } = "Home";

    public DateOnly? ActiveFromDate { get; set; }

    public DateOnly? ActiveToDate { get; set; }

    public bool IsActive { get; set; } = true;

    public int VersionNumber { get; set; } = 1;
}

public sealed class CMSFaq : AuditableEntity
{
    public long CMSFaqId { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int VersionNumber { get; set; } = 1;
}

public sealed class CMSContentVersion : AuditableEntity
{
    public long CMSContentVersionId { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public long ContentId { get; set; }

    public int VersionNumber { get; set; }

    public string SnapshotTitle { get; set; } = string.Empty;

    public string SnapshotContent { get; set; } = string.Empty;

    public string ChangeSummary { get; set; } = string.Empty;
}

public sealed class NotificationTemplate : AuditableEntity
{
    public long NotificationTemplateId { get; set; }

    public string TemplateCode { get; set; } = string.Empty;

    public string TemplateName { get; set; } = string.Empty;

    public string TriggerCode { get; set; } = string.Empty;

    public string Channel { get; set; } = string.Empty;

    public string SubjectTemplate { get; set; } = string.Empty;

    public string BodyTemplate { get; set; } = string.Empty;

    public string AllowedMergeTags { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public sealed class NotificationTriggerConfiguration : AuditableEntity
{
    public long NotificationTriggerConfigurationId { get; set; }

    public string TriggerCode { get; set; } = string.Empty;

    public string TriggerName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public bool EmailEnabled { get; set; } = true;

    public bool SmsEnabled { get; set; }

    public bool WhatsAppEnabled { get; set; }

    public bool PushEnabled { get; set; }

    public int ReminderLeadMinutes { get; set; }

    public int DelayMinutes { get; set; }
}

public sealed class CommunicationPreference : AuditableEntity
{
    public long CommunicationPreferenceId { get; set; }

    public long CustomerId { get; set; }

    public string EmailAddress { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public bool EmailEnabled { get; set; } = true;

    public bool SmsEnabled { get; set; } = true;

    public bool WhatsAppEnabled { get; set; }

    public bool PushEnabled { get; set; }

    public bool AllowPromotionalContent { get; set; }

    public Customer? Customer { get; set; }
}

public sealed class DynamicMasterRecord : AuditableEntity
{
    public long DynamicMasterRecordId { get; set; }

    public string MasterType { get; set; } = string.Empty;

    public string MasterCode { get; set; } = string.Empty;

    public string MasterLabel { get; set; } = string.Empty;

    public string MasterValue { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public sealed class DisplayContentSetting : AuditableEntity
{
    public long DisplayContentSettingId { get; set; }

    public string ContentGroup { get; set; } = string.Empty;

    public string ContentKey { get; set; } = string.Empty;

    public string ContentValue { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
