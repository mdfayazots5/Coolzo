using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class InstallationLeadConfiguration : IEntityTypeConfiguration<InstallationLead>
{
    public void Configure(EntityTypeBuilder<InstallationLead> builder)
    {
        builder.ToTable("tblInstallationLead");
        builder.HasKey(entity => entity.InstallationId).HasName("PK_tblInstallationLead_InstallationId");
        builder.Property(entity => entity.InstallationId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.InstallationNumber).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.InstallationType).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.SiteNotes).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ApprovalStatus).HasConversion<int>();
        builder.Property(entity => entity.InstallationStatus).HasConversion<int>();
        builder.HasIndex(entity => entity.InstallationNumber).IsUnique().HasDatabaseName("UK_tblInstallationLead_InstallationNumber");
        builder.HasOne(entity => entity.Lead)
            .WithMany()
            .HasForeignKey(entity => entity.LeadId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInstallationLead_LeadId_tblLeads_LeadId");
        builder.HasOne(entity => entity.Customer)
            .WithMany()
            .HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInstallationLead_CustomerId_tblCustomer_CustomerId");
        builder.HasOne(entity => entity.CustomerAddress)
            .WithMany()
            .HasForeignKey(entity => entity.CustomerAddressId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInstallationLead_CustomerAddressId_tblCustomerAddress_CustomerAddressId");
        builder.HasOne(entity => entity.AssignedTechnician)
            .WithMany()
            .HasForeignKey(entity => entity.AssignedTechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInstallationLead_AssignedTechnicianId_tblTechnician_TechnicianId");
        builder.ConfigureAuditColumns();
    }
}

public sealed class InstallationSurveyConfiguration : IEntityTypeConfiguration<InstallationSurvey>
{
    public void Configure(EntityTypeBuilder<InstallationSurvey> builder)
    {
        builder.ToTable("tblInstallationSurvey");
        builder.HasKey(entity => entity.InstallationSurveyId).HasName("PK_tblInstallationSurvey_InstallationSurveyId");
        builder.Property(entity => entity.InstallationSurveyId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.SiteConditionSummary).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.SafetyRiskNotes).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.RecommendedAction).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.MeasurementsJson).HasMaxLength(4000).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.PhotoUrlsJson).HasMaxLength(4000).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.EstimatedMaterialCost).HasColumnType("money");
        builder.HasOne(entity => entity.Installation)
            .WithMany(parent => parent.Surveys)
            .HasForeignKey(entity => entity.InstallationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInstallationSurvey_InstallationId_tblInstallationLead_InstallationId");
        builder.HasOne(entity => entity.Technician)
            .WithMany()
            .HasForeignKey(entity => entity.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInstallationSurvey_TechnicianId_tblTechnician_TechnicianId");
        builder.ConfigureAuditColumns();
    }
}

public sealed class InstallationSurveyItemConfiguration : IEntityTypeConfiguration<InstallationSurveyItem>
{
    public void Configure(EntityTypeBuilder<InstallationSurveyItem> builder)
    {
        builder.ToTable("tblInstallationSurveyItem");
        builder.HasKey(entity => entity.InstallationSurveyItemId).HasName("PK_tblInstallationSurveyItem_InstallationSurveyItemId");
        builder.Property(entity => entity.InstallationSurveyItemId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ItemTitle).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.ItemValue).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.Unit).HasMaxLength(32).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.Remarks).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.Survey)
            .WithMany(parent => parent.Items)
            .HasForeignKey(entity => entity.InstallationSurveyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInstallationSurveyItem_InstallationSurveyId_tblInstallationSurvey_InstallationSurveyId");
        builder.ConfigureAuditColumns();
    }
}

public sealed class InstallationProposalConfiguration : IEntityTypeConfiguration<InstallationProposal>
{
    public void Configure(EntityTypeBuilder<InstallationProposal> builder)
    {
        builder.ToTable("tblInstallationProposal");
        builder.HasKey(entity => entity.InstallationProposalId).HasName("PK_tblInstallationProposal_InstallationProposalId");
        builder.Property(entity => entity.InstallationProposalId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ProposalNumber).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.ProposalStatus).HasConversion<int>();
        builder.Property(entity => entity.SubTotalAmount).HasColumnType("money");
        builder.Property(entity => entity.TaxAmount).HasColumnType("money");
        builder.Property(entity => entity.TotalAmount).HasColumnType("money");
        builder.Property(entity => entity.ProposalRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.CustomerRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasIndex(entity => entity.ProposalNumber).IsUnique().HasDatabaseName("UK_tblInstallationProposal_ProposalNumber");
        builder.HasOne(entity => entity.Installation)
            .WithMany(parent => parent.Proposals)
            .HasForeignKey(entity => entity.InstallationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInstallationProposal_InstallationId_tblInstallationLead_InstallationId");
        builder.ConfigureAuditColumns();
    }
}

public sealed class InstallationProposalLineConfiguration : IEntityTypeConfiguration<InstallationProposalLine>
{
    public void Configure(EntityTypeBuilder<InstallationProposalLine> builder)
    {
        builder.ToTable("tblInstallationProposalLine");
        builder.HasKey(entity => entity.InstallationProposalLineId).HasName("PK_tblInstallationProposalLine_InstallationProposalLineId");
        builder.Property(entity => entity.InstallationProposalLineId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.LineDescription).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.UnitPrice).HasColumnType("money");
        builder.Property(entity => entity.LineTotal).HasColumnType("money");
        builder.Property(entity => entity.Remarks).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.Proposal)
            .WithMany(parent => parent.Lines)
            .HasForeignKey(entity => entity.InstallationProposalId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInstallationProposalLine_InstallationProposalId_tblInstallationProposal_InstallationProposalId");
        builder.ConfigureAuditColumns();
    }
}

public sealed class InstallationChecklistConfiguration : IEntityTypeConfiguration<InstallationChecklist>
{
    public void Configure(EntityTypeBuilder<InstallationChecklist> builder)
    {
        builder.ToTable("tblInstallationChecklist");
        builder.HasKey(entity => entity.InstallationChecklistId).HasName("PK_tblInstallationChecklist_InstallationChecklistId");
        builder.Property(entity => entity.InstallationChecklistId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ChecklistTitle).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.ChecklistDescription).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.Installation)
            .WithMany(parent => parent.Checklists)
            .HasForeignKey(entity => entity.InstallationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInstallationChecklist_InstallationId_tblInstallationLead_InstallationId");
        builder.ConfigureAuditColumns();
    }
}

public sealed class InstallationChecklistResponseConfiguration : IEntityTypeConfiguration<InstallationChecklistResponse>
{
    public void Configure(EntityTypeBuilder<InstallationChecklistResponse> builder)
    {
        builder.ToTable("tblInstallationChecklistResponse");
        builder.HasKey(entity => entity.InstallationChecklistResponseId).HasName("PK_tblInstallationChecklistResponse_InstallationChecklistResponseId");
        builder.Property(entity => entity.InstallationChecklistResponseId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ResponseRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.Checklist)
            .WithMany(parent => parent.Responses)
            .HasForeignKey(entity => entity.InstallationChecklistId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInstallationChecklistResponse_InstallationChecklistId_tblInstallationChecklist_InstallationChecklistId");
        builder.HasOne(entity => entity.Installation)
            .WithMany()
            .HasForeignKey(entity => entity.InstallationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInstallationChecklistResponse_InstallationId_tblInstallationLead_InstallationId");
        builder.ConfigureAuditColumns();
    }
}

public sealed class InstallationStatusHistoryConfiguration : IEntityTypeConfiguration<InstallationStatusHistory>
{
    public void Configure(EntityTypeBuilder<InstallationStatusHistory> builder)
    {
        builder.ToTable("tblInstallationStatusHistory");
        builder.HasKey(entity => entity.InstallationStatusHistoryId).HasName("PK_tblInstallationStatusHistory_InstallationStatusHistoryId");
        builder.Property(entity => entity.InstallationStatusHistoryId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.PreviousStatus).HasConversion<int>();
        builder.Property(entity => entity.CurrentStatus).HasConversion<int>();
        builder.Property(entity => entity.Remarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ChangedByRole).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.Installation)
            .WithMany(parent => parent.StatusHistories)
            .HasForeignKey(entity => entity.InstallationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInstallationStatusHistory_InstallationId_tblInstallationLead_InstallationId");
        builder.ConfigureAuditColumns();
    }
}
