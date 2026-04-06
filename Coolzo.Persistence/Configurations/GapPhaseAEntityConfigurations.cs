using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("tblLeads");
        builder.HasKey(entity => entity.LeadId).HasName("PK_tblLeads_LeadId");
        builder.Property(entity => entity.LeadId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.LeadNumber).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.CustomerName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.MobileNumber).HasMaxLength(16).IsRequired();
        builder.Property(entity => entity.EmailAddress).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.SourceChannel).HasConversion<int>();
        builder.Property(entity => entity.LeadStatus).HasConversion<int>();
        builder.Property(entity => entity.AddressLine1).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.AddressLine2).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.CityName).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.Pincode).HasMaxLength(16).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.InquiryNotes).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.LostReason).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.AssignedUser)
            .WithMany()
            .HasForeignKey(entity => entity.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblLeads_AssignedUserId_tblUser_UserId");
        builder.HasIndex(entity => entity.LeadNumber).IsUnique().HasDatabaseName("UK_tblLeads_LeadNumber");
        builder.HasIndex(entity => new { entity.MobileNumber, entity.DateCreated }).HasDatabaseName("IDX_tblLeads_MobileNumber_DateCreated");
        builder.ConfigureAuditColumns();
    }
}

public sealed class LeadSourceConfiguration : IEntityTypeConfiguration<LeadSource>
{
    public void Configure(EntityTypeBuilder<LeadSource> builder)
    {
        builder.ToTable("tblLeadSource");
        builder.HasKey(entity => entity.LeadSourceId).HasName("PK_tblLeadSource_LeadSourceId");
        builder.Property(entity => entity.LeadSourceId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.SourceCode).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.SourceName).HasMaxLength(128).IsRequired();
        builder.HasIndex(entity => entity.SourceCode).IsUnique().HasDatabaseName("UK_tblLeadSource_SourceCode");
        builder.ConfigureAuditColumns();
    }
}

public sealed class LeadStatusHistoryConfiguration : IEntityTypeConfiguration<LeadStatusHistory>
{
    public void Configure(EntityTypeBuilder<LeadStatusHistory> builder)
    {
        builder.ToTable("tblLeadStatusHistory");
        builder.HasKey(entity => entity.LeadStatusHistoryId).HasName("PK_tblLeadStatusHistory_LeadStatusHistoryId");
        builder.Property(entity => entity.LeadStatusHistoryId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.PreviousStatus).HasConversion<int>();
        builder.Property(entity => entity.CurrentStatus).HasConversion<int>();
        builder.Property(entity => entity.Remarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.Lead)
            .WithMany(lead => lead.StatusHistories)
            .HasForeignKey(entity => entity.LeadId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblLeadStatusHistory_LeadId_tblLeads_LeadId");
        builder.HasIndex(entity => new { entity.LeadId, entity.ChangedDateUtc }).HasDatabaseName("IDX_tblLeadStatusHistory_LeadId_ChangedDateUtc");
        builder.ConfigureAuditColumns();
    }
}

public sealed class LeadAssignmentConfiguration : IEntityTypeConfiguration<LeadAssignment>
{
    public void Configure(EntityTypeBuilder<LeadAssignment> builder)
    {
        builder.ToTable("tblLeadAssignment");
        builder.HasKey(entity => entity.LeadAssignmentId).HasName("PK_tblLeadAssignment_LeadAssignmentId");
        builder.Property(entity => entity.LeadAssignmentId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.Remarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.Lead)
            .WithMany(lead => lead.Assignments)
            .HasForeignKey(entity => entity.LeadId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblLeadAssignment_LeadId_tblLeads_LeadId");
        builder.HasOne(entity => entity.AssignedUser)
            .WithMany()
            .HasForeignKey(entity => entity.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblLeadAssignment_AssignedUserId_tblUser_UserId");
        builder.HasIndex(entity => new { entity.LeadId, entity.AssignedDateUtc }).HasDatabaseName("IDX_tblLeadAssignment_LeadId_AssignedDateUtc");
        builder.ConfigureAuditColumns();
    }
}

public sealed class LeadNoteConfiguration : IEntityTypeConfiguration<LeadNote>
{
    public void Configure(EntityTypeBuilder<LeadNote> builder)
    {
        builder.ToTable("tblLeadNote");
        builder.HasKey(entity => entity.LeadNoteId).HasName("PK_tblLeadNote_LeadNoteId");
        builder.Property(entity => entity.LeadNoteId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.NoteText).HasMaxLength(1024).IsRequired();
        builder.HasOne(entity => entity.Lead)
            .WithMany(lead => lead.Notes)
            .HasForeignKey(entity => entity.LeadId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblLeadNote_LeadId_tblLeads_LeadId");
        builder.HasIndex(entity => new { entity.LeadId, entity.NoteDateUtc }).HasDatabaseName("IDX_tblLeadNote_LeadId_NoteDateUtc");
        builder.ConfigureAuditColumns();
    }
}

public sealed class LeadConversionConfiguration : IEntityTypeConfiguration<LeadConversion>
{
    public void Configure(EntityTypeBuilder<LeadConversion> builder)
    {
        builder.ToTable("tblLeadConversion");
        builder.HasKey(entity => entity.LeadConversionId).HasName("PK_tblLeadConversion_LeadConversionId");
        builder.Property(entity => entity.LeadConversionId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ConversionType).HasConversion<int>();
        builder.Property(entity => entity.ReferenceNumber).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.Remarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.Lead)
            .WithMany(lead => lead.Conversions)
            .HasForeignKey(entity => entity.LeadId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblLeadConversion_LeadId_tblLeads_LeadId");
        builder.HasIndex(entity => new { entity.LeadId, entity.ConversionType }).HasDatabaseName("UK_tblLeadConversion_LeadId_ConversionType").IsUnique();
        builder.ConfigureAuditColumns();
    }
}

public sealed class InstallationOrderConfiguration : IEntityTypeConfiguration<InstallationOrder>
{
    public void Configure(EntityTypeBuilder<InstallationOrder> builder)
    {
        builder.ToTable("tblInstallationOrder");
        builder.HasKey(entity => entity.InstallationOrderId).HasName("PK_tblInstallationOrder_InstallationOrderId");
        builder.Property(entity => entity.InstallationOrderId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.InstallationOrderNumber).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.CurrentStatus).HasConversion<int>();
        builder.Property(entity => entity.InstallationType).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.InstallationChecklistJson).HasMaxLength(2000).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.SurveySummary).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.CommissioningRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasIndex(entity => entity.InstallationOrderNumber).IsUnique().HasDatabaseName("UK_tblInstallationOrder_InstallationOrderNumber");
        builder.HasIndex(entity => entity.InstallationId).HasDatabaseName("IDX_tblInstallationOrder_InstallationId");
        builder.HasIndex(entity => entity.TechnicianId).HasDatabaseName("IDX_tblInstallationOrder_TechnicianId");
        builder.HasIndex(entity => entity.InstallationProposalId).HasDatabaseName("IDX_tblInstallationOrder_InstallationProposalId");
        builder.HasOne(entity => entity.Installation)
            .WithMany(parent => parent.Orders)
            .HasForeignKey(entity => entity.InstallationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInstallationOrder_InstallationId_tblInstallationLead_InstallationId");
        builder.HasOne(entity => entity.Technician)
            .WithMany()
            .HasForeignKey(entity => entity.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInstallationOrder_TechnicianId_tblTechnician_TechnicianId");
        builder.ConfigureAuditColumns();
    }
}

public sealed class SiteSurveyReportConfiguration : IEntityTypeConfiguration<SiteSurveyReport>
{
    public void Configure(EntityTypeBuilder<SiteSurveyReport> builder)
    {
        builder.ToTable("tblSiteSurveyReport");
        builder.HasKey(entity => entity.SiteSurveyReportId).HasName("PK_tblSiteSurveyReport_SiteSurveyReportId");
        builder.Property(entity => entity.SiteSurveyReportId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.SurveyDecision).HasConversion<int>();
        builder.Property(entity => entity.SiteConditionSummary).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.SafetyRiskNotes).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.RecommendedAction).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.EstimatedMaterialCost).HasColumnType("money");
        builder.HasOne(entity => entity.InstallationOrder)
            .WithMany(order => order.SiteSurveyReports)
            .HasForeignKey(entity => entity.InstallationOrderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblSiteSurveyReport_InstallationOrderId_tblInstallationOrder_InstallationOrderId");
        builder.ConfigureAuditColumns();
    }
}

public sealed class CommissioningCertificateConfiguration : IEntityTypeConfiguration<CommissioningCertificate>
{
    public void Configure(EntityTypeBuilder<CommissioningCertificate> builder)
    {
        builder.ToTable("tblCommissioningCertificate");
        builder.HasKey(entity => entity.CommissioningCertificateId).HasName("PK_tblCommissioningCertificate_CommissioningCertificateId");
        builder.Property(entity => entity.CommissioningCertificateId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.CertificateNumber).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.WarrantyRegistrationNumber).HasMaxLength(32).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.CustomerConfirmationName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.CustomerSignatureName).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ChecklistJson).HasMaxLength(2000).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.Remarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.InstallationOrder)
            .WithMany()
            .HasForeignKey(entity => entity.InstallationOrderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCommissioningCertificate_InstallationOrderId_tblInstallationOrder_InstallationOrderId");
        builder.HasOne(entity => entity.Installation)
            .WithMany(parent => parent.CommissioningCertificates)
            .HasForeignKey(entity => entity.InstallationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCommissioningCertificate_InstallationId_tblInstallationLead_InstallationId");
        builder.HasIndex(entity => entity.CertificateNumber).IsUnique().HasDatabaseName("UK_tblCommissioningCertificate_CertificateNumber");
        builder.HasIndex(entity => entity.WarrantyRegistrationNumber).IsUnique().HasDatabaseName("UK_tblCommissioningCertificate_WarrantyRegistrationNumber");
        builder.ConfigureAuditColumns();
    }
}

public sealed class CancellationRecordConfiguration : IEntityTypeConfiguration<CancellationRecord>
{
    public void Configure(EntityTypeBuilder<CancellationRecord> builder)
    {
        builder.ToTable("tblCancellationRecord");
        builder.HasKey(entity => entity.CancellationRecordId).HasName("PK_tblCancellationRecord_CancellationRecordId");
        builder.Property(entity => entity.CancellationRecordId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.CancellationStatus).HasConversion<int>();
        builder.Property(entity => entity.PolicyCode).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ReasonCode).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ReasonDescription).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.CancelledByRole).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.CancellationSource).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.CancellationReasonCode).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.CancellationReasonText).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.CancellationFeeAmount).HasColumnType("money");
        builder.Property(entity => entity.RefundEligibleAmount).HasColumnType("money");
        builder.Property(entity => entity.RequestedByRole).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ApprovalRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.Booking)
            .WithMany()
            .HasForeignKey(entity => entity.BookingId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCancellationRecord_BookingId_tblBooking_BookingId");
        builder.HasOne(entity => entity.ServiceRequest)
            .WithMany()
            .HasForeignKey(entity => entity.ServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCancellationRecord_ServiceRequestId_tblServiceRequest_ServiceRequestId");
        builder.HasIndex(entity => entity.ServiceRequestId).IsUnique().HasDatabaseName("UK_tblCancellationRecord_ServiceRequestId");
        builder.HasIndex(entity => entity.BookingId).HasDatabaseName("IDX_tblCancellationRecord_BookingId");
        builder.HasIndex(entity => new { entity.BranchId, entity.CancellationStatus, entity.DateCreated }).HasDatabaseName("IDX_tblCancellationRecord_BranchId_Status_DateCreated");
        builder.ConfigureAuditColumns();
    }
}

public sealed class CancellationPolicyConfiguration : IEntityTypeConfiguration<CancellationPolicy>
{
    public void Configure(EntityTypeBuilder<CancellationPolicy> builder)
    {
        builder.ToTable("tblCancellationPolicy");
        builder.HasKey(entity => entity.CancellationPolicyId).HasName("PK_tblCancellationPolicy_CancellationPolicyId");
        builder.Property(entity => entity.CancellationPolicyId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.PolicyCode).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.PolicyName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.CustomerTypeCode).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.Description).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.FeePercent).HasColumnType("decimal(5,2)");
        builder.HasIndex(entity => new { entity.BranchId, entity.PolicyCode, entity.CustomerTypeCode }).HasDatabaseName("IDX_tblCancellationPolicy_BranchId_PolicyCode_CustomerTypeCode");
        builder.ConfigureAuditColumns();
    }
}

public sealed class RefundRequestConfiguration : IEntityTypeConfiguration<RefundRequest>
{
    public void Configure(EntityTypeBuilder<RefundRequest> builder)
    {
        builder.ToTable("tblRefundRequest");
        builder.HasKey(entity => entity.RefundRequestId).HasName("PK_tblRefundRequest_RefundRequestId");
        builder.Property(entity => entity.RefundRequestId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.RefundStatus).HasConversion<int>();
        builder.Property(entity => entity.RefundRequestNo).HasMaxLength(32).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.RefundMethod).HasConversion<int>();
        builder.Property(entity => entity.RefundAmount).HasColumnType("money");
        builder.Property(entity => entity.RequestedAmount).HasColumnType("money");
        builder.Property(entity => entity.ApprovedAmount).HasColumnType("money");
        builder.Property(entity => entity.MaxAllowedAmount).HasColumnType("money");
        builder.Property(entity => entity.RefundReason).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ApprovalRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.CancellationRecord)
            .WithMany(entity => entity.RefundRequests)
            .HasForeignKey(entity => entity.CancellationRecordId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblRefundRequest_CancellationRecordId_tblCancellationRecord_CancellationRecordId");
        builder.HasOne(entity => entity.InvoiceHeader)
            .WithMany()
            .HasForeignKey(entity => entity.InvoiceHeaderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblRefundRequest_InvoiceHeaderId_tblInvoiceHeader_InvoiceHeaderId");
        builder.HasOne(entity => entity.PaymentTransaction)
            .WithMany()
            .HasForeignKey(entity => entity.PaymentTransactionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblRefundRequest_PaymentTransactionId_tblPaymentTransaction_PaymentTransactionId");
        builder.HasIndex(entity => entity.RefundRequestNo).IsUnique().HasDatabaseName("UK_tblRefundRequest_RefundRequestNo");
        builder.HasIndex(entity => new { entity.InvoiceHeaderId, entity.RequestedDateUtc }).HasDatabaseName("IDX_tblRefundRequest_InvoiceHeaderId_RequestedDateUtc");
        builder.HasIndex(entity => new { entity.BranchId, entity.RefundStatus, entity.DateCreated }).HasDatabaseName("IDX_tblRefundRequest_BranchId_Status_DateCreated");
        builder.ConfigureAuditColumns();
    }
}

public sealed class RefundApprovalConfiguration : IEntityTypeConfiguration<RefundApproval>
{
    public void Configure(EntityTypeBuilder<RefundApproval> builder)
    {
        builder.ToTable("tblRefundApproval");
        builder.HasKey(entity => entity.RefundApprovalId).HasName("PK_tblRefundApproval_RefundApprovalId");
        builder.Property(entity => entity.RefundApprovalId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ApprovalStatus).HasMaxLength(32).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ApprovalRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.RefundRequest)
            .WithMany(entity => entity.Approvals)
            .HasForeignKey(entity => entity.RefundRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblRefundApproval_RefundRequestId_tblRefundRequest_RefundRequestId");
        builder.HasIndex(entity => new { entity.RefundRequestId, entity.ApprovalLevel }).HasDatabaseName("IDX_tblRefundApproval_RefundRequestId_ApprovalLevel");
        builder.ConfigureAuditColumns();
    }
}

public sealed class RefundStatusHistoryConfiguration : IEntityTypeConfiguration<RefundStatusHistory>
{
    public void Configure(EntityTypeBuilder<RefundStatusHistory> builder)
    {
        builder.ToTable("tblRefundStatusHistory");
        builder.HasKey(entity => entity.RefundStatusHistoryId).HasName("PK_tblRefundStatusHistory_RefundStatusHistoryId");
        builder.Property(entity => entity.RefundStatusHistoryId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.FromStatus).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ToStatus).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.Remarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.RefundRequest)
            .WithMany(entity => entity.StatusHistory)
            .HasForeignKey(entity => entity.RefundRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblRefundStatusHistory_RefundRequestId_tblRefundRequest_RefundRequestId");
        builder.HasIndex(entity => new { entity.RefundRequestId, entity.ChangedOn }).HasDatabaseName("IDX_tblRefundStatusHistory_RefundRequestId_ChangedOn");
        builder.ConfigureAuditColumns();
    }
}

public sealed class CustomerAbsentRecordConfiguration : IEntityTypeConfiguration<CustomerAbsentRecord>
{
    public void Configure(EntityTypeBuilder<CustomerAbsentRecord> builder)
    {
        builder.ToTable("tblCustomerAbsentRecord");
        builder.HasKey(entity => entity.CustomerAbsentRecordId).HasName("PK_tblCustomerAbsentRecord_CustomerAbsentRecordId");
        builder.Property(entity => entity.CustomerAbsentRecordId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ContactAttemptLog).HasMaxLength(1024).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.AbsentReasonCode).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.AbsentReasonText).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ResolutionRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.CustomerAbsentStatus).HasConversion<int>();
        builder.HasOne(entity => entity.ServiceRequest)
            .WithMany()
            .HasForeignKey(entity => entity.ServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCustomerAbsentRecord_ServiceRequestId_tblServiceRequest_ServiceRequestId");
        builder.HasOne(entity => entity.Technician)
            .WithMany()
            .HasForeignKey(entity => entity.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCustomerAbsentRecord_TechnicianId_tblTechnician_TechnicianId");
        builder.HasIndex(entity => entity.ServiceRequestId).IsUnique().HasDatabaseName("UK_tblCustomerAbsentRecord_ServiceRequestId");
        builder.HasIndex(entity => new { entity.BranchId, entity.CustomerAbsentStatus, entity.MarkedOn }).HasDatabaseName("IDX_tblCustomerAbsentRecord_BranchId_Status_MarkedOn");
        builder.ConfigureAuditColumns();
    }
}

public sealed class TechnicianDocumentConfiguration : IEntityTypeConfiguration<TechnicianDocument>
{
    public void Configure(EntityTypeBuilder<TechnicianDocument> builder)
    {
        builder.ToTable("tblTechnicianDocument");
        builder.HasKey(entity => entity.TechnicianDocumentId).HasName("PK_tblTechnicianDocument_TechnicianDocumentId");
        builder.Property(entity => entity.TechnicianDocumentId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.DocumentType).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.DocumentNumber).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.DocumentUrl).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.StorageUrl).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.VerificationStatus).HasConversion<int>();
        builder.Property(entity => entity.VerificationRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.VerifiedOnUtc);
        builder.HasIndex(entity => new { entity.TechnicianId, entity.DocumentType }).HasDatabaseName("IDX_tblTechnicianDocument_TechnicianId_DocumentType");
        builder.ConfigureAuditColumns();
    }
}

public sealed class SkillAssessmentConfiguration : IEntityTypeConfiguration<SkillAssessment>
{
    public void Configure(EntityTypeBuilder<SkillAssessment> builder)
    {
        builder.ToTable("tblSkillAssessment");
        builder.HasKey(entity => entity.SkillAssessmentId).HasName("PK_tblSkillAssessment_SkillAssessmentId");
        builder.Property(entity => entity.SkillAssessmentId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.AssessmentCode).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.AssessmentName).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.AssessmentStatus).HasMaxLength(32).HasDefaultValue("Assigned");
        builder.Property(entity => entity.ScorePercentage).HasColumnType("decimal(5,2)");
        builder.Property(entity => entity.AssessmentResult).HasConversion<int>();
        builder.Property(entity => entity.Remarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasIndex(entity => new { entity.TechnicianId, entity.AssessedOnUtc }).HasDatabaseName("IDX_tblSkillAssessment_TechnicianId_AssessedOnUtc");
        builder.ConfigureAuditColumns();
    }
}

public sealed class TrainingRecordConfiguration : IEntityTypeConfiguration<TrainingRecord>
{
    public void Configure(EntityTypeBuilder<TrainingRecord> builder)
    {
        builder.ToTable("tblTrainingRecord");
        builder.HasKey(entity => entity.TrainingRecordId).HasName("PK_tblTrainingRecord_TrainingRecordId");
        builder.Property(entity => entity.TrainingRecordId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.TrainingName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.TrainingTitle).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.TrainingType).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.TrainingStatus).HasMaxLength(32).HasDefaultValue("Assigned");
        builder.Property(entity => entity.CertificationNumber).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.CertificateUrl).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ScorePercentage).HasColumnType("decimal(5,2)");
        builder.Property(entity => entity.Remarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasIndex(entity => new { entity.TechnicianId, entity.CompletionDateUtc }).HasDatabaseName("IDX_tblTrainingRecord_TechnicianId_CompletionDateUtc");
        builder.ConfigureAuditColumns();
    }
}

public sealed class PartsReturnConfiguration : IEntityTypeConfiguration<PartsReturn>
{
    public void Configure(EntityTypeBuilder<PartsReturn> builder)
    {
        builder.ToTable("tblPartsReturn");
        builder.HasKey(entity => entity.PartsReturnId).HasName("PK_tblPartsReturn_PartsReturnId");
        builder.Property(entity => entity.PartsReturnId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.PartsReturnNumber).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.Quantity).HasColumnType("decimal(18,2)");
        builder.Property(entity => entity.ReasonCode).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.DefectDescription).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.PartsReturnStatus).HasConversion<int>();
        builder.Property(entity => entity.ApprovalRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.SupplierClaimReference).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.HasIndex(entity => entity.PartsReturnNumber).IsUnique().HasDatabaseName("UK_tblPartsReturn_PartsReturnNumber");
        builder.ConfigureAuditColumns();
    }
}

public sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("tblCampaign");
        builder.HasKey(entity => entity.CampaignId).HasName("PK_tblCampaign_CampaignId");
        builder.Property(entity => entity.CampaignId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.CampaignCode).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.CampaignName).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.CampaignStatus).HasConversion<int>();
        builder.Property(entity => entity.Notes).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasIndex(entity => entity.CampaignCode).IsUnique().HasDatabaseName("UK_tblCampaign_CampaignCode");
        builder.ConfigureAuditColumns();
    }
}

public sealed class TechnicianEarningConfiguration : IEntityTypeConfiguration<TechnicianEarning>
{
    public void Configure(EntityTypeBuilder<TechnicianEarning> builder)
    {
        builder.ToTable("tblTechnicianEarnings");
        builder.HasKey(entity => entity.TechnicianEarningId).HasName("PK_tblTechnicianEarnings_TechnicianEarningId");
        builder.Property(entity => entity.TechnicianEarningId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.EarningType).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.EarningAmount).HasColumnType("money");
        builder.Property(entity => entity.EarningStatus).HasConversion<int>();
        builder.HasIndex(entity => new { entity.TechnicianId, entity.CalculatedDateUtc }).HasDatabaseName("IDX_tblTechnicianEarnings_TechnicianId_CalculatedDateUtc");
        builder.ConfigureAuditColumns();
    }
}

public sealed class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
{
    public void Configure(EntityTypeBuilder<FeatureFlag> builder)
    {
        builder.ToTable("tblFeatureFlag");
        builder.HasKey(entity => entity.FeatureFlagId).HasName("PK_tblFeatureFlag_FeatureFlagId");
        builder.Property(entity => entity.FeatureFlagId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.FlagCode).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.FlagName).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasIndex(entity => entity.FlagCode).IsUnique().HasDatabaseName("UK_tblFeatureFlag_FlagCode");
        builder.ConfigureAuditColumns();
    }
}

public sealed class SystemAlertConfiguration : IEntityTypeConfiguration<SystemAlert>
{
    public void Configure(EntityTypeBuilder<SystemAlert> builder)
    {
        builder.ToTable("tblSystemAlert");
        builder.HasKey(entity => entity.SystemAlertId).HasName("PK_tblSystemAlert_SystemAlertId");
        builder.Property(entity => entity.SystemAlertId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.AlertCode).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.AlertType).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.RelatedEntityName).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.RelatedEntityId).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Severity).HasConversion<int>();
        builder.Property(entity => entity.AlertStatus).HasConversion<int>();
        builder.Property(entity => entity.NotificationChain).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.AlertMessage).HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.TriggerCode).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.HasIndex(entity => new { entity.AlertStatus, entity.SlaDueDateUtc }).HasDatabaseName("IDX_tblSystemAlert_AlertStatus_SlaDueDateUtc");
        builder.ConfigureAuditColumns();
    }
}

public sealed class PaymentWebhookAttemptConfiguration : IEntityTypeConfiguration<PaymentWebhookAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentWebhookAttempt> builder)
    {
        builder.ToTable("tblPaymentWebhookAttempt");
        builder.HasKey(entity => entity.PaymentWebhookAttemptId).HasName("PK_tblPaymentWebhookAttempt_PaymentWebhookAttemptId");
        builder.Property(entity => entity.PaymentWebhookAttemptId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.IdempotencyKey).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.GatewayTransactionId).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.WebhookReference).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.SignatureHash).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.PaidAmount).HasColumnType("money");
        builder.Property(entity => entity.PayloadSnapshot).HasMaxLength(4000).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.AttemptStatus).HasConversion<int>();
        builder.Property(entity => entity.FailureReason).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasIndex(entity => entity.IdempotencyKey).HasDatabaseName("IDX_tblPaymentWebhookAttempt_IdempotencyKey");
        builder.HasIndex(entity => new { entity.AttemptStatus, entity.NextRetryDateUtc }).HasDatabaseName("IDX_tblPaymentWebhookAttempt_AttemptStatus_NextRetryDateUtc");
        builder.ConfigureAuditColumns();
    }
}

public sealed class OfflineSyncQueueItemConfiguration : IEntityTypeConfiguration<OfflineSyncQueueItem>
{
    public void Configure(EntityTypeBuilder<OfflineSyncQueueItem> builder)
    {
        builder.ToTable("tblOfflineSyncQueueItem");
        builder.HasKey(entity => entity.OfflineSyncQueueItemId).HasName("PK_tblOfflineSyncQueueItem_OfflineSyncQueueItemId");
        builder.Property(entity => entity.OfflineSyncQueueItemId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.DeviceReference).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.EntityName).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.EntityReference).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.PayloadSnapshot).HasMaxLength(4000).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.SyncStatus).HasConversion<int>();
        builder.Property(entity => entity.ConflictStrategy).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.FailureReason).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasIndex(entity => new { entity.EntityName, entity.EntityReference }).HasDatabaseName("IDX_tblOfflineSyncQueueItem_EntityName_EntityReference");
        builder.HasIndex(entity => new { entity.SyncStatus, entity.NextRetryDateUtc }).HasDatabaseName("IDX_tblOfflineSyncQueueItem_SyncStatus_NextRetryDateUtc");
        builder.ConfigureAuditColumns();
    }
}

public sealed class WorkflowStatusHistoryConfiguration : IEntityTypeConfiguration<WorkflowStatusHistory>
{
    public void Configure(EntityTypeBuilder<WorkflowStatusHistory> builder)
    {
        builder.ToTable("tblWorkflowStatusHistory");
        builder.HasKey(entity => entity.WorkflowStatusHistoryId).HasName("PK_tblWorkflowStatusHistory_WorkflowStatusHistoryId");
        builder.Property(entity => entity.WorkflowStatusHistoryId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.EntityType).HasConversion<int>();
        builder.Property(entity => entity.EntityReference).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.PreviousStatus).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CurrentStatus).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Remarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ChangedByRole).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.HasIndex(entity => new { entity.EntityType, entity.EntityReference, entity.ChangedDateUtc }).HasDatabaseName("IDX_tblWorkflowStatusHistory_EntityType_EntityReference_ChangedDateUtc");
        builder.ConfigureAuditColumns();
    }
}
