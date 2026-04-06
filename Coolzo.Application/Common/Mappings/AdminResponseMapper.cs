using Coolzo.Contracts.Responses.Admin;
using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Mappings;

internal static class AdminResponseMapper
{
    public static SystemConfigurationResponse ToResponse(SystemConfiguration systemConfiguration, bool maskSensitive = true)
    {
        return new SystemConfigurationResponse(
            systemConfiguration.SystemConfigurationId,
            systemConfiguration.ConfigurationGroup,
            systemConfiguration.ConfigurationKey,
            maskSensitive && systemConfiguration.IsSensitive ? "******" : systemConfiguration.ConfigurationValue,
            systemConfiguration.ValueType,
            systemConfiguration.Description,
            systemConfiguration.IsSensitive,
            systemConfiguration.IsActive,
            systemConfiguration.DateCreated,
            systemConfiguration.LastUpdated);
    }

    public static BusinessHourConfigurationResponse ToResponse(BusinessHourConfiguration businessHourConfiguration)
    {
        return new BusinessHourConfigurationResponse(
            businessHourConfiguration.BusinessHourConfigurationId,
            businessHourConfiguration.DayOfWeekNumber,
            Enum.IsDefined(typeof(DayOfWeek), businessHourConfiguration.DayOfWeekNumber)
                ? ((DayOfWeek)businessHourConfiguration.DayOfWeekNumber).ToString()
                : "Unknown",
            ToTimeText(businessHourConfiguration.StartTimeLocal),
            ToTimeText(businessHourConfiguration.EndTimeLocal),
            businessHourConfiguration.IsClosed);
    }

    public static HolidayConfigurationResponse ToResponse(HolidayConfiguration holidayConfiguration)
    {
        return new HolidayConfigurationResponse(
            holidayConfiguration.HolidayConfigurationId,
            holidayConfiguration.HolidayDate.ToString("yyyy-MM-dd"),
            holidayConfiguration.HolidayName,
            holidayConfiguration.IsRecurringAnnually,
            holidayConfiguration.IsActive);
    }

    public static CMSBlockResponse ToResponse(CMSBlock cmsBlock, bool includeDraftContent = true)
    {
        return new CMSBlockResponse(
            cmsBlock.CMSBlockId,
            cmsBlock.BlockKey,
            cmsBlock.Title,
            cmsBlock.Summary,
            includeDraftContent || cmsBlock.IsPublished ? cmsBlock.Content : string.Empty,
            cmsBlock.PreviewImageUrl,
            cmsBlock.IsActive,
            cmsBlock.IsPublished,
            cmsBlock.SortOrder,
            cmsBlock.VersionNumber,
            cmsBlock.DateCreated,
            cmsBlock.LastUpdated);
    }

    public static CMSBannerResponse ToResponse(CMSBanner cmsBanner)
    {
        return new CMSBannerResponse(
            cmsBanner.CMSBannerId,
            cmsBanner.BannerTitle,
            cmsBanner.BannerSubtitle,
            cmsBanner.ImageUrl,
            cmsBanner.RedirectUrl,
            cmsBanner.DisplayArea,
            cmsBanner.ActiveFromDate?.ToString("yyyy-MM-dd"),
            cmsBanner.ActiveToDate?.ToString("yyyy-MM-dd"),
            cmsBanner.IsActive,
            cmsBanner.IsPublished,
            cmsBanner.SortOrder);
    }

    public static CMSFaqResponse ToResponse(CMSFaq cmsFaq)
    {
        return new CMSFaqResponse(
            cmsFaq.CMSFaqId,
            cmsFaq.Category,
            cmsFaq.Question,
            cmsFaq.Answer,
            cmsFaq.IsActive,
            cmsFaq.IsPublished,
            cmsFaq.SortOrder);
    }

    public static DisplayContentSettingResponse ToResponse(DisplayContentSetting displayContentSetting)
    {
        return new DisplayContentSettingResponse(
            displayContentSetting.DisplayContentSettingId,
            displayContentSetting.ContentGroup,
            displayContentSetting.ContentKey,
            displayContentSetting.ContentValue,
            displayContentSetting.ContentType,
            displayContentSetting.IsActive,
            displayContentSetting.IsPublished,
            displayContentSetting.SortOrder);
    }

    public static NotificationTemplateResponse ToResponse(NotificationTemplate notificationTemplate)
    {
        return new NotificationTemplateResponse(
            notificationTemplate.NotificationTemplateId,
            notificationTemplate.TemplateCode,
            notificationTemplate.TemplateName,
            notificationTemplate.TriggerCode,
            notificationTemplate.Channel,
            notificationTemplate.SubjectTemplate,
            notificationTemplate.BodyTemplate,
            SplitMergeTags(notificationTemplate.AllowedMergeTags),
            notificationTemplate.IsActive,
            notificationTemplate.DateCreated,
            notificationTemplate.LastUpdated);
    }

    public static NotificationTriggerConfigurationResponse ToResponse(NotificationTriggerConfiguration notificationTriggerConfiguration)
    {
        return new NotificationTriggerConfigurationResponse(
            notificationTriggerConfiguration.NotificationTriggerConfigurationId,
            notificationTriggerConfiguration.TriggerCode,
            notificationTriggerConfiguration.TriggerName,
            notificationTriggerConfiguration.Description,
            notificationTriggerConfiguration.IsEnabled,
            notificationTriggerConfiguration.EmailEnabled,
            notificationTriggerConfiguration.SmsEnabled,
            notificationTriggerConfiguration.WhatsAppEnabled,
            notificationTriggerConfiguration.PushEnabled,
            notificationTriggerConfiguration.ReminderLeadMinutes,
            notificationTriggerConfiguration.DelayMinutes,
            notificationTriggerConfiguration.DateCreated,
            notificationTriggerConfiguration.LastUpdated);
    }

    public static CommunicationPreferenceResponse ToResponse(CommunicationPreference communicationPreference)
    {
        return new CommunicationPreferenceResponse(
            communicationPreference.CommunicationPreferenceId,
            communicationPreference.CustomerId,
            communicationPreference.EmailAddress,
            communicationPreference.MobileNumber,
            communicationPreference.EmailEnabled,
            communicationPreference.SmsEnabled,
            communicationPreference.WhatsAppEnabled,
            communicationPreference.PushEnabled,
            communicationPreference.AllowPromotionalContent,
            communicationPreference.LastUpdated ?? communicationPreference.DateCreated);
    }

    public static DynamicMasterRecordResponse ToResponse(DynamicMasterRecord dynamicMasterRecord)
    {
        return new DynamicMasterRecordResponse(
            dynamicMasterRecord.DynamicMasterRecordId,
            dynamicMasterRecord.MasterType,
            dynamicMasterRecord.MasterCode,
            dynamicMasterRecord.MasterLabel,
            dynamicMasterRecord.MasterValue,
            dynamicMasterRecord.Description,
            dynamicMasterRecord.IsActive,
            dynamicMasterRecord.IsPublished,
            dynamicMasterRecord.SortOrder,
            dynamicMasterRecord.DateCreated,
            dynamicMasterRecord.LastUpdated);
    }

    private static IReadOnlyCollection<string> SplitMergeTags(string source)
    {
        return source
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? ToTimeText(TimeSpan? source)
    {
        return source?.ToString(@"hh\:mm");
    }
}
