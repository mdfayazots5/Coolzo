using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Configurations;

internal static class Phase08AdminModelBuilderExtensions
{
    public static void ConfigurePhase08AdminEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemConfiguration>(builder =>
        {
            builder.ToTable("tblSystemConfiguration");
            builder.HasKey(entity => entity.SystemConfigurationId).HasName("PK_tblSystemConfiguration_SystemConfigurationId");
            builder.Property(entity => entity.SystemConfigurationId).ValueGeneratedOnAdd();
            builder.Property(entity => entity.ConfigurationGroup).HasMaxLength(128).IsRequired();
            builder.Property(entity => entity.ConfigurationKey).HasMaxLength(128).IsRequired();
            builder.Property(entity => entity.ConfigurationValue).HasMaxLength(1024).IsRequired();
            builder.Property(entity => entity.ValueType).HasMaxLength(64).IsRequired();
            builder.Property(entity => entity.Description).HasMaxLength(512).HasDefaultValue(string.Empty);
            builder.Property(entity => entity.IsSensitive).HasDefaultValue(false);
            builder.Property(entity => entity.IsActive).HasDefaultValue(true);
            builder.HasIndex(entity => new { entity.ConfigurationGroup, entity.ConfigurationKey })
                .IsUnique()
                .HasDatabaseName("UK_tblSystemConfiguration_ConfigurationGroup_ConfigurationKey");
            builder.ConfigureAuditColumns();
        });

        modelBuilder.Entity<BusinessHourConfiguration>(builder =>
        {
            builder.ToTable("tblBusinessHourConfiguration");
            builder.HasKey(entity => entity.BusinessHourConfigurationId).HasName("PK_tblBusinessHourConfiguration_BusinessHourConfigurationId");
            builder.Property(entity => entity.BusinessHourConfigurationId).ValueGeneratedOnAdd();
            builder.Property(entity => entity.DayOfWeekNumber).IsRequired();
            builder.Property(entity => entity.StartTimeLocal).HasColumnType("time");
            builder.Property(entity => entity.EndTimeLocal).HasColumnType("time");
            builder.Property(entity => entity.IsClosed).HasDefaultValue(false);
            builder.HasIndex(entity => entity.DayOfWeekNumber)
                .IsUnique()
                .HasDatabaseName("UK_tblBusinessHourConfiguration_DayOfWeekNumber");
            builder.ConfigureAuditColumns();
        });

        modelBuilder.Entity<HolidayConfiguration>(builder =>
        {
            builder.ToTable("tblHolidayConfiguration");
            builder.HasKey(entity => entity.HolidayConfigurationId).HasName("PK_tblHolidayConfiguration_HolidayConfigurationId");
            builder.Property(entity => entity.HolidayConfigurationId).ValueGeneratedOnAdd();
            builder.Property(entity => entity.HolidayDate).HasColumnType("date").IsRequired();
            builder.Property(entity => entity.HolidayName).HasMaxLength(128).IsRequired();
            builder.Property(entity => entity.IsRecurringAnnually).HasDefaultValue(false);
            builder.Property(entity => entity.IsActive).HasDefaultValue(true);
            builder.HasIndex(entity => entity.HolidayDate)
                .IsUnique()
                .HasDatabaseName("UK_tblHolidayConfiguration_HolidayDate");
            builder.ConfigureAuditColumns();
        });

        modelBuilder.Entity<CMSBlock>(builder =>
        {
            builder.ToTable("tblCMSBlock");
            builder.HasKey(entity => entity.CMSBlockId).HasName("PK_tblCMSBlock_CMSBlockId");
            builder.Property(entity => entity.CMSBlockId).ValueGeneratedOnAdd();
            builder.Property(entity => entity.BlockKey).HasMaxLength(128).IsRequired();
            builder.Property(entity => entity.Title).HasMaxLength(160).IsRequired();
            builder.Property(entity => entity.Summary).HasMaxLength(512).HasDefaultValue(string.Empty);
            builder.Property(entity => entity.Content).HasMaxLength(4000).IsRequired();
            builder.Property(entity => entity.PreviewImageUrl).HasMaxLength(512).HasDefaultValue(string.Empty);
            builder.Property(entity => entity.IsActive).HasDefaultValue(true);
            builder.Property(entity => entity.VersionNumber).HasDefaultValue(1);
            builder.HasIndex(entity => entity.BlockKey).IsUnique().HasDatabaseName("UK_tblCMSBlock_BlockKey");
            builder.ConfigureAuditColumns();
        });

        modelBuilder.Entity<CMSBanner>(builder =>
        {
            builder.ToTable("tblCMSBanner");
            builder.HasKey(entity => entity.CMSBannerId).HasName("PK_tblCMSBanner_CMSBannerId");
            builder.Property(entity => entity.CMSBannerId).ValueGeneratedOnAdd();
            builder.Property(entity => entity.BannerTitle).HasMaxLength(160).IsRequired();
            builder.Property(entity => entity.BannerSubtitle).HasMaxLength(512).HasDefaultValue(string.Empty);
            builder.Property(entity => entity.ImageUrl).HasMaxLength(512).HasDefaultValue(string.Empty);
            builder.Property(entity => entity.RedirectUrl).HasMaxLength(512).HasDefaultValue(string.Empty);
            builder.Property(entity => entity.DisplayArea).HasMaxLength(64).HasDefaultValue("Home");
            builder.Property(entity => entity.ActiveFromDate).HasColumnType("date");
            builder.Property(entity => entity.ActiveToDate).HasColumnType("date");
            builder.Property(entity => entity.IsActive).HasDefaultValue(true);
            builder.Property(entity => entity.VersionNumber).HasDefaultValue(1);
            builder.HasIndex(entity => new { entity.DisplayArea, entity.SortOrder })
                .HasDatabaseName("IDX_tblCMSBanner_DisplayArea_SortOrder");
            builder.ConfigureAuditColumns();
        });

        modelBuilder.Entity<CMSFaq>(builder =>
        {
            builder.ToTable("tblCMSFaq");
            builder.HasKey(entity => entity.CMSFaqId).HasName("PK_tblCMSFaq_CMSFaqId");
            builder.Property(entity => entity.CMSFaqId).ValueGeneratedOnAdd();
            builder.Property(entity => entity.Category).HasMaxLength(128).IsRequired();
            builder.Property(entity => entity.Question).HasMaxLength(512).IsRequired();
            builder.Property(entity => entity.Answer).HasMaxLength(4000).IsRequired();
            builder.Property(entity => entity.IsActive).HasDefaultValue(true);
            builder.Property(entity => entity.VersionNumber).HasDefaultValue(1);
            builder.HasIndex(entity => new { entity.Category, entity.SortOrder })
                .HasDatabaseName("IDX_tblCMSFaq_Category_SortOrder");
            builder.ConfigureAuditColumns();
        });

        modelBuilder.Entity<CMSContentVersion>(builder =>
        {
            builder.ToTable("tblCMSContentVersion");
            builder.HasKey(entity => entity.CMSContentVersionId).HasName("PK_tblCMSContentVersion_CMSContentVersionId");
            builder.Property(entity => entity.CMSContentVersionId).ValueGeneratedOnAdd();
            builder.Property(entity => entity.ContentType).HasMaxLength(64).IsRequired();
            builder.Property(entity => entity.SnapshotTitle).HasMaxLength(160).HasDefaultValue(string.Empty);
            builder.Property(entity => entity.SnapshotContent).HasMaxLength(4000).IsRequired();
            builder.Property(entity => entity.ChangeSummary).HasMaxLength(512).HasDefaultValue(string.Empty);
            builder.HasIndex(entity => new { entity.ContentType, entity.ContentId, entity.VersionNumber })
                .IsUnique()
                .HasDatabaseName("UK_tblCMSContentVersion_ContentType_ContentId_VersionNumber");
            builder.ConfigureAuditColumns();
        });

        modelBuilder.Entity<NotificationTemplate>(builder =>
        {
            builder.ToTable("tblNotificationTemplate");
            builder.HasKey(entity => entity.NotificationTemplateId).HasName("PK_tblNotificationTemplate_NotificationTemplateId");
            builder.Property(entity => entity.NotificationTemplateId).ValueGeneratedOnAdd();
            builder.Property(entity => entity.TemplateCode).HasMaxLength(128).IsRequired();
            builder.Property(entity => entity.TemplateName).HasMaxLength(160).IsRequired();
            builder.Property(entity => entity.TriggerCode).HasMaxLength(128).IsRequired();
            builder.Property(entity => entity.Channel).HasMaxLength(32).IsRequired();
            builder.Property(entity => entity.SubjectTemplate).HasMaxLength(512).HasDefaultValue(string.Empty);
            builder.Property(entity => entity.BodyTemplate).HasMaxLength(4000).IsRequired();
            builder.Property(entity => entity.AllowedMergeTags).HasMaxLength(1024).HasDefaultValue(string.Empty);
            builder.Property(entity => entity.IsActive).HasDefaultValue(true);
            builder.HasIndex(entity => entity.TemplateCode).IsUnique().HasDatabaseName("UK_tblNotificationTemplate_TemplateCode");
            builder.HasIndex(entity => new { entity.TriggerCode, entity.Channel })
                .HasDatabaseName("IDX_tblNotificationTemplate_TriggerCode_Channel");
            builder.ConfigureAuditColumns();
        });

        modelBuilder.Entity<NotificationTriggerConfiguration>(builder =>
        {
            builder.ToTable("tblNotificationTriggerConfiguration");
            builder.HasKey(entity => entity.NotificationTriggerConfigurationId).HasName("PK_tblNotificationTriggerConfiguration_NotificationTriggerConfigurationId");
            builder.Property(entity => entity.NotificationTriggerConfigurationId).ValueGeneratedOnAdd();
            builder.Property(entity => entity.TriggerCode).HasMaxLength(128).IsRequired();
            builder.Property(entity => entity.TriggerName).HasMaxLength(160).IsRequired();
            builder.Property(entity => entity.Description).HasMaxLength(512).HasDefaultValue(string.Empty);
            builder.Property(entity => entity.IsEnabled).HasDefaultValue(true);
            builder.Property(entity => entity.EmailEnabled).HasDefaultValue(true);
            builder.Property(entity => entity.SmsEnabled).HasDefaultValue(false);
            builder.Property(entity => entity.WhatsAppEnabled).HasDefaultValue(false);
            builder.Property(entity => entity.PushEnabled).HasDefaultValue(false);
            builder.Property(entity => entity.ReminderLeadMinutes).HasDefaultValue(0);
            builder.Property(entity => entity.DelayMinutes).HasDefaultValue(0);
            builder.HasIndex(entity => entity.TriggerCode).IsUnique().HasDatabaseName("UK_tblNotificationTriggerConfiguration_TriggerCode");
            builder.ConfigureAuditColumns();
        });

        modelBuilder.Entity<CommunicationPreference>(builder =>
        {
            builder.ToTable("tblCommunicationPreference");
            builder.HasKey(entity => entity.CommunicationPreferenceId).HasName("PK_tblCommunicationPreference_CommunicationPreferenceId");
            builder.Property(entity => entity.CommunicationPreferenceId).ValueGeneratedOnAdd();
            builder.Property(entity => entity.EmailAddress).HasMaxLength(128).HasDefaultValue(string.Empty);
            builder.Property(entity => entity.MobileNumber).HasMaxLength(32).HasDefaultValue(string.Empty);
            builder.Property(entity => entity.EmailEnabled).HasDefaultValue(true);
            builder.Property(entity => entity.SmsEnabled).HasDefaultValue(true);
            builder.Property(entity => entity.WhatsAppEnabled).HasDefaultValue(false);
            builder.Property(entity => entity.PushEnabled).HasDefaultValue(false);
            builder.Property(entity => entity.AllowPromotionalContent).HasDefaultValue(false);
            builder.HasIndex(entity => entity.CustomerId).IsUnique().HasDatabaseName("UK_tblCommunicationPreference_CustomerId");
            builder.HasOne(entity => entity.Customer)
                .WithMany()
                .HasForeignKey(entity => entity.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_tblCommunicationPreference_CustomerId_tblCustomer_CustomerId");
            builder.ConfigureAuditColumns();
        });

        modelBuilder.Entity<DynamicMasterRecord>(builder =>
        {
            builder.ToTable("tblDynamicMasterRecord");
            builder.HasKey(entity => entity.DynamicMasterRecordId).HasName("PK_tblDynamicMasterRecord_DynamicMasterRecordId");
            builder.Property(entity => entity.DynamicMasterRecordId).ValueGeneratedOnAdd();
            builder.Property(entity => entity.MasterType).HasMaxLength(128).IsRequired();
            builder.Property(entity => entity.MasterCode).HasMaxLength(128).IsRequired();
            builder.Property(entity => entity.MasterLabel).HasMaxLength(160).IsRequired();
            builder.Property(entity => entity.MasterValue).HasMaxLength(512).IsRequired();
            builder.Property(entity => entity.Description).HasMaxLength(512).HasDefaultValue(string.Empty);
            builder.Property(entity => entity.IsActive).HasDefaultValue(true);
            builder.HasIndex(entity => new { entity.MasterType, entity.MasterCode })
                .IsUnique()
                .HasDatabaseName("UK_tblDynamicMasterRecord_MasterType_MasterCode");
            builder.HasIndex(entity => new { entity.MasterType, entity.SortOrder })
                .HasDatabaseName("IDX_tblDynamicMasterRecord_MasterType_SortOrder");
            builder.ConfigureAuditColumns();
        });

        modelBuilder.Entity<DisplayContentSetting>(builder =>
        {
            builder.ToTable("tblDisplayContentSetting");
            builder.HasKey(entity => entity.DisplayContentSettingId).HasName("PK_tblDisplayContentSetting_DisplayContentSettingId");
            builder.Property(entity => entity.DisplayContentSettingId).ValueGeneratedOnAdd();
            builder.Property(entity => entity.ContentGroup).HasMaxLength(128).IsRequired();
            builder.Property(entity => entity.ContentKey).HasMaxLength(128).IsRequired();
            builder.Property(entity => entity.ContentValue).HasMaxLength(2048).IsRequired();
            builder.Property(entity => entity.ContentType).HasMaxLength(64).IsRequired();
            builder.Property(entity => entity.IsActive).HasDefaultValue(true);
            builder.HasIndex(entity => new { entity.ContentGroup, entity.ContentKey })
                .IsUnique()
                .HasDatabaseName("UK_tblDisplayContentSetting_ContentGroup_ContentKey");
            builder.ConfigureAuditColumns();
        });
    }
}
