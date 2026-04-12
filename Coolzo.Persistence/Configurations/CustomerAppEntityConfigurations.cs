using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class CustomerEquipmentConfiguration : IEntityTypeConfiguration<CustomerEquipment>
{
    public void Configure(EntityTypeBuilder<CustomerEquipment> builder)
    {
        builder.ToTable("tblCustomerEquipment");
        builder.HasKey(entity => entity.CustomerEquipmentId).HasName("PK_tblCustomerEquipment_CustomerEquipmentId");
        builder.Property(entity => entity.CustomerEquipmentId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.EquipmentName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.EquipmentType).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.BrandName).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.Capacity).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.LocationLabel).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.PurchaseDate).HasColumnType("date");
        builder.Property(entity => entity.LastServiceDate).HasColumnType("date");
        builder.Property(entity => entity.SerialNumber).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);
        builder.HasOne(entity => entity.Customer)
            .WithMany()
            .HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCustomerEquipment_CustomerId_tblCustomer_CustomerId");
        builder.HasIndex(entity => new { entity.CustomerId, entity.IsActive }).HasDatabaseName("IDX_tblCustomerEquipment_CustomerId_IsActive");
        builder.ConfigureAuditColumns();
    }
}

public sealed class CustomerNotificationConfiguration : IEntityTypeConfiguration<CustomerNotification>
{
    public void Configure(EntityTypeBuilder<CustomerNotification> builder)
    {
        builder.ToTable("tblCustomerNotification");
        builder.HasKey(entity => entity.CustomerNotificationId).HasName("PK_tblCustomerNotification_CustomerNotificationId");
        builder.Property(entity => entity.CustomerNotificationId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.Title).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Message).HasMaxLength(1000).IsRequired();
        builder.Property(entity => entity.NotificationType).HasMaxLength(32).HasDefaultValue("info");
        builder.Property(entity => entity.LinkUrl).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.IsRead).HasDefaultValue(false);
        builder.HasOne(entity => entity.Customer)
            .WithMany()
            .HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCustomerNotification_CustomerId_tblCustomer_CustomerId");
        builder.HasIndex(entity => new { entity.CustomerId, entity.IsRead, entity.DateCreated }).HasDatabaseName("IDX_tblCustomerNotification_CustomerId_IsRead_DateCreated");
        builder.ConfigureAuditColumns();
    }
}

public sealed class PromotionalOfferConfiguration : IEntityTypeConfiguration<PromotionalOffer>
{
    public void Configure(EntityTypeBuilder<PromotionalOffer> builder)
    {
        builder.ToTable("tblPromotionalOffer");
        builder.HasKey(entity => entity.PromotionalOfferId).HasName("PK_tblPromotionalOffer_PromotionalOfferId");
        builder.Property(entity => entity.PromotionalOfferId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.OfferCode).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Title).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(1000).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.DiscountType).HasMaxLength(32).HasDefaultValue("fixed");
        builder.Property(entity => entity.DiscountValue).HasColumnType("money");
        builder.Property(entity => entity.MinimumOrderValue).HasColumnType("money");
        builder.Property(entity => entity.ExpiryDate).HasColumnType("date");
        builder.Property(entity => entity.Category).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);
        builder.HasIndex(entity => entity.OfferCode).IsUnique().HasDatabaseName("UK_tblPromotionalOffer_OfferCode");
        builder.ConfigureAuditColumns();
    }
}

public sealed class CustomerReferralConfiguration : IEntityTypeConfiguration<CustomerReferral>
{
    public void Configure(EntityTypeBuilder<CustomerReferral> builder)
    {
        builder.ToTable("tblCustomerReferral");
        builder.HasKey(entity => entity.CustomerReferralId).HasName("PK_tblCustomerReferral_CustomerReferralId");
        builder.Property(entity => entity.CustomerReferralId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ReferralName).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.ReferralStatus).HasMaxLength(32).HasDefaultValue("Pending");
        builder.Property(entity => entity.RewardAmount).HasColumnType("money");
        builder.Property(entity => entity.ReferralDate).HasColumnType("date");
        builder.HasOne(entity => entity.Customer)
            .WithMany()
            .HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCustomerReferral_CustomerId_tblCustomer_CustomerId");
        builder.HasIndex(entity => new { entity.CustomerId, entity.ReferralDate }).HasDatabaseName("IDX_tblCustomerReferral_CustomerId_ReferralDate");
        builder.ConfigureAuditColumns();
    }
}

public sealed class CustomerLoyaltyTransactionConfiguration : IEntityTypeConfiguration<CustomerLoyaltyTransaction>
{
    public void Configure(EntityTypeBuilder<CustomerLoyaltyTransaction> builder)
    {
        builder.ToTable("tblCustomerLoyaltyTransaction");
        builder.HasKey(entity => entity.CustomerLoyaltyTransactionId).HasName("PK_tblCustomerLoyaltyTransaction_CustomerLoyaltyTransactionId");
        builder.Property(entity => entity.CustomerLoyaltyTransactionId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.TransactionType).HasMaxLength(32).HasDefaultValue("earn");
        builder.Property(entity => entity.Description).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.Customer)
            .WithMany()
            .HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCustomerLoyaltyTransaction_CustomerId_tblCustomer_CustomerId");
        builder.HasIndex(entity => new { entity.CustomerId, entity.DateCreated }).HasDatabaseName("IDX_tblCustomerLoyaltyTransaction_CustomerId_DateCreated");
        builder.ConfigureAuditColumns();
    }
}

public sealed class CustomerReviewConfiguration : IEntityTypeConfiguration<CustomerReview>
{
    public void Configure(EntityTypeBuilder<CustomerReview> builder)
    {
        builder.ToTable("tblCustomerReview");
        builder.HasKey(entity => entity.CustomerReviewId).HasName("PK_tblCustomerReview_CustomerReviewId");
        builder.Property(entity => entity.CustomerReviewId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.CustomerNameSnapshot).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.CustomerPhotoUrl).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.Comment).HasMaxLength(1000).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);
        builder.HasOne(entity => entity.Customer)
            .WithMany()
            .HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCustomerReview_CustomerId_tblCustomer_CustomerId");
        builder.HasIndex(entity => new { entity.ServiceId, entity.IsActive, entity.DateCreated }).HasDatabaseName("IDX_tblCustomerReview_ServiceId_IsActive_DateCreated");
        builder.ConfigureAuditColumns();
    }
}

public sealed class CustomerAppFeedbackConfiguration : IEntityTypeConfiguration<CustomerAppFeedback>
{
    public void Configure(EntityTypeBuilder<CustomerAppFeedback> builder)
    {
        builder.ToTable("tblCustomerAppFeedback");
        builder.HasKey(entity => entity.CustomerAppFeedbackId).HasName("PK_tblCustomerAppFeedback_CustomerAppFeedbackId");
        builder.Property(entity => entity.CustomerAppFeedbackId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.FeedbackType).HasMaxLength(64).HasDefaultValue("general");
        builder.Property(entity => entity.Message).HasMaxLength(2000).IsRequired();
        builder.Property(entity => entity.AppVersion).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.DeviceInfo).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.FeedbackStatus).HasMaxLength(32).HasDefaultValue("Submitted");
        builder.HasOne(entity => entity.Customer)
            .WithMany()
            .HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCustomerAppFeedback_CustomerId_tblCustomer_CustomerId");
        builder.HasIndex(entity => new { entity.CustomerId, entity.DateCreated }).HasDatabaseName("IDX_tblCustomerAppFeedback_CustomerId_DateCreated");
        builder.ConfigureAuditColumns();
    }
}
