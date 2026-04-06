using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class Lead : AuditableEntity
{
    public long LeadId { get; set; }

    public string LeadNumber { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public string EmailAddress { get; set; } = string.Empty;

    public LeadSourceChannel SourceChannel { get; set; } = LeadSourceChannel.Web;

    public LeadStatus LeadStatus { get; set; } = LeadStatus.New;

    public long? AssignedUserId { get; set; }

    public User? AssignedUser { get; set; }

    public long? ServiceId { get; set; }

    public long? AcTypeId { get; set; }

    public long? TonnageId { get; set; }

    public long? BrandId { get; set; }

    public long? SlotAvailabilityId { get; set; }

    public long? ConvertedBookingId { get; set; }

    public long? ConvertedServiceRequestId { get; set; }

    public string AddressLine1 { get; set; } = string.Empty;

    public string AddressLine2 { get; set; } = string.Empty;

    public string CityName { get; set; } = string.Empty;

    public string Pincode { get; set; } = string.Empty;

    public string InquiryNotes { get; set; } = string.Empty;

    public string LostReason { get; set; } = string.Empty;

    public DateTime? LastContactedDateUtc { get; set; }

    public DateTime? ConvertedDateUtc { get; set; }

    public DateTime? ClosedDateUtc { get; set; }

    public ICollection<LeadStatusHistory> StatusHistories { get; set; } = new List<LeadStatusHistory>();

    public ICollection<LeadAssignment> Assignments { get; set; } = new List<LeadAssignment>();

    public ICollection<LeadNote> Notes { get; set; } = new List<LeadNote>();

    public ICollection<LeadConversion> Conversions { get; set; } = new List<LeadConversion>();
}

public sealed class LeadStatusHistory : AuditableEntity
{
    public long LeadStatusHistoryId { get; set; }

    public long LeadId { get; set; }

    public LeadStatus PreviousStatus { get; set; } = LeadStatus.New;

    public LeadStatus CurrentStatus { get; set; } = LeadStatus.New;

    public string Remarks { get; set; } = string.Empty;

    public DateTime ChangedDateUtc { get; set; }

    public Lead? Lead { get; set; }
}

public sealed class LeadSource : AuditableEntity
{
    public long LeadSourceId { get; set; }

    public string SourceCode { get; set; } = string.Empty;

    public string SourceName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public sealed class LeadAssignment : AuditableEntity
{
    public long LeadAssignmentId { get; set; }

    public long LeadId { get; set; }

    public long AssignedUserId { get; set; }

    public long? PreviousAssignedUserId { get; set; }

    public string Remarks { get; set; } = string.Empty;

    public DateTime AssignedDateUtc { get; set; }

    public Lead? Lead { get; set; }

    public User? AssignedUser { get; set; }
}

public sealed class LeadNote : AuditableEntity
{
    public long LeadNoteId { get; set; }

    public long LeadId { get; set; }

    public string NoteText { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = true;

    public DateTime NoteDateUtc { get; set; }

    public Lead? Lead { get; set; }
}

public sealed class LeadConversion : AuditableEntity
{
    public long LeadConversionId { get; set; }

    public long LeadId { get; set; }

    public LeadConversionType ConversionType { get; set; } = LeadConversionType.Booking;

    public long? BookingId { get; set; }

    public long? ServiceRequestId { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;

    public string Remarks { get; set; } = string.Empty;

    public DateTime ConvertedDateUtc { get; set; }

    public Lead? Lead { get; set; }
}

public sealed class InstallationOrder : AuditableEntity
{
    public long InstallationOrderId { get; set; }

    public string InstallationOrderNumber { get; set; } = string.Empty;

    public long? InstallationId { get; set; }

    public long? LeadId { get; set; }

    public long? ServiceRequestId { get; set; }

    public long CustomerId { get; set; }

    public long CustomerAddressId { get; set; }

    public long? TechnicianId { get; set; }

    public long? InstallationProposalId { get; set; }

    public InstallationOrderStatus CurrentStatus { get; set; } = InstallationOrderStatus.Draft;

    public DateTime? ScheduledInstallationDateUtc { get; set; }

    public DateTime? ExecutionStartedDateUtc { get; set; }

    public DateTime? ExecutionCompletedDateUtc { get; set; }

    public int NumberOfUnits { get; set; }

    public string InstallationType { get; set; } = string.Empty;

    public int HelperCount { get; set; }

    public string InstallationChecklistJson { get; set; } = string.Empty;

    public string SurveySummary { get; set; } = string.Empty;

    public string CommissioningRemarks { get; set; } = string.Empty;

    public InstallationLead? Installation { get; set; }

    public Technician? Technician { get; set; }

    public ICollection<SiteSurveyReport> SiteSurveyReports { get; set; } = new List<SiteSurveyReport>();
}

public sealed class SiteSurveyReport : AuditableEntity
{
    public long SiteSurveyReportId { get; set; }

    public long InstallationOrderId { get; set; }

    public SiteSurveyDecision SurveyDecision { get; set; } = SiteSurveyDecision.Pending;

    public DateTime SurveyDateUtc { get; set; }

    public string SiteConditionSummary { get; set; } = string.Empty;

    public bool ElectricalReadiness { get; set; }

    public bool AccessReadiness { get; set; }

    public string SafetyRiskNotes { get; set; } = string.Empty;

    public string RecommendedAction { get; set; } = string.Empty;

    public decimal EstimatedMaterialCost { get; set; }

    public InstallationOrder? InstallationOrder { get; set; }
}

public sealed class CommissioningCertificate : AuditableEntity
{
    public long CommissioningCertificateId { get; set; }

    public long InstallationOrderId { get; set; }

    public long? InstallationId { get; set; }

    public string CertificateNumber { get; set; } = string.Empty;

    public string WarrantyRegistrationNumber { get; set; } = string.Empty;

    public DateTime CommissioningDateUtc { get; set; }

    public string CustomerConfirmationName { get; set; } = string.Empty;

    public string CustomerSignatureName { get; set; } = string.Empty;

    public string ChecklistJson { get; set; } = string.Empty;

    public string Remarks { get; set; } = string.Empty;

    public bool IsAccepted { get; set; }

    public InstallationLead? Installation { get; set; }

    public InstallationOrder? InstallationOrder { get; set; }
}

public sealed class CancellationRecord : AuditableEntity
{
    public long CancellationRecordId { get; set; }

    public long? BookingId { get; set; }

    public long? ServiceRequestId { get; set; }

    public long? CancelledByUserId { get; set; }

    public string CancelledByRole { get; set; } = string.Empty;

    public string CancellationSource { get; set; } = string.Empty;

    public string CancellationReasonCode { get; set; } = string.Empty;

    public string CancellationReasonText { get; set; } = string.Empty;

    public int TimeToSlotMinutes { get; set; }

    public CancellationStatus CancellationStatus { get; set; } = CancellationStatus.Requested;

    public string PolicyCode { get; set; } = string.Empty;

    public string ReasonCode { get; set; } = string.Empty;

    public string ReasonDescription { get; set; } = string.Empty;

    public decimal CancellationFeeAmount { get; set; }

    public decimal RefundEligibleAmount { get; set; }

    public bool RequiresApproval { get; set; }

    public string RequestedByRole { get; set; } = string.Empty;

    public DateTime RequestedDateUtc { get; set; }

    public long? ApprovedByUserId { get; set; }

    public DateTime? ApprovedDateUtc { get; set; }

    public string ApprovalRemarks { get; set; } = string.Empty;

    public Booking? Booking { get; set; }

    public ServiceRequest? ServiceRequest { get; set; }

    public ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();
}

public sealed class RefundRequest : AuditableEntity
{
    public long RefundRequestId { get; set; }

    public long? CancellationRecordId { get; set; }

    public long? InvoiceHeaderId { get; set; }

    public long? PaymentTransactionId { get; set; }

    public string RefundRequestNo { get; set; } = string.Empty;

    public RefundMethodType RefundMethod { get; set; } = RefundMethodType.OriginalPaymentMethod;

    public decimal RefundAmount { get; set; }

    public RefundStatus RefundStatus { get; set; } = RefundStatus.Initiated;

    public decimal RequestedAmount { get; set; }

    public decimal ApprovedAmount { get; set; }

    public decimal MaxAllowedAmount { get; set; }

    public string RefundReason { get; set; } = string.Empty;

    public string ApprovalRemarks { get; set; } = string.Empty;

    public DateTime RequestedDateUtc { get; set; }

    public long? ApprovedByUserId { get; set; }

    public DateTime? ApprovedDateUtc { get; set; }

    public bool ApprovalRequiredFlag { get; set; }

    public DateTime? ProcessedOn { get; set; }

    public CancellationRecord? CancellationRecord { get; set; }

    public InvoiceHeader? InvoiceHeader { get; set; }

    public PaymentTransaction? PaymentTransaction { get; set; }

    public ICollection<RefundApproval> Approvals { get; set; } = new List<RefundApproval>();

    public ICollection<RefundStatusHistory> StatusHistory { get; set; } = new List<RefundStatusHistory>();
}

public sealed class CancellationPolicy : AuditableEntity
{
    public long CancellationPolicyId { get; set; }

    public string PolicyCode { get; set; } = string.Empty;

    public string PolicyName { get; set; } = string.Empty;

    public int? MinTimeToSlotMinutes { get; set; }

    public int? MaxTimeToSlotMinutes { get; set; }

    public decimal FeePercent { get; set; }

    public bool AppliesWhenTechnicianDispatched { get; set; }

    public bool RequiresManagerApproval { get; set; }

    public bool EmergencyBookingOverride { get; set; }

    public string CustomerTypeCode { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

public sealed class RefundApproval : AuditableEntity
{
    public long RefundApprovalId { get; set; }

    public long RefundRequestId { get; set; }

    public int ApprovalLevel { get; set; }

    public long ApproverUserId { get; set; }

    public string ApprovalStatus { get; set; } = string.Empty;

    public string ApprovalRemarks { get; set; } = string.Empty;

    public DateTime? ApprovedOn { get; set; }

    public RefundRequest? RefundRequest { get; set; }
}

public sealed class RefundStatusHistory : AuditableEntity
{
    public long RefundStatusHistoryId { get; set; }

    public long RefundRequestId { get; set; }

    public string FromStatus { get; set; } = string.Empty;

    public string ToStatus { get; set; } = string.Empty;

    public long ChangedByUserId { get; set; }

    public DateTime ChangedOn { get; set; }

    public string Remarks { get; set; } = string.Empty;

    public RefundRequest? RefundRequest { get; set; }
}

public sealed class CustomerAbsentRecord : AuditableEntity
{
    public long CustomerAbsentRecordId { get; set; }

    public long ServiceRequestId { get; set; }

    public long TechnicianId { get; set; }

    public DateTime MarkedOn { get; set; }

    public int AttemptCount { get; set; }

    public string ContactAttemptLog { get; set; } = string.Empty;

    public string AbsentReasonCode { get; set; } = string.Empty;

    public string AbsentReasonText { get; set; } = string.Empty;

    public CustomerAbsentStatus CustomerAbsentStatus { get; set; } = CustomerAbsentStatus.Marked;

    public string ResolutionRemarks { get; set; } = string.Empty;

    public DateTime? ResolvedOn { get; set; }

    public ServiceRequest? ServiceRequest { get; set; }

    public Technician? Technician { get; set; }
}

public sealed class TechnicianDocument : AuditableEntity
{
    public long TechnicianDocumentId { get; set; }

    public long TechnicianId { get; set; }

    public string DocumentType { get; set; } = string.Empty;

    public string DocumentNumber { get; set; } = string.Empty;

    public string DocumentUrl { get; set; } = string.Empty;

    public string StorageUrl { get; set; } = string.Empty;

    public DateTime? ExpiryDateUtc { get; set; }

    public TechnicianDocumentStatus VerificationStatus { get; set; } = TechnicianDocumentStatus.Uploaded;

    public string VerificationRemarks { get; set; } = string.Empty;

    public long? VerifiedByUserId { get; set; }

    public DateTime? VerifiedOnUtc { get; set; }
}

public sealed class SkillAssessment : AuditableEntity
{
    public long SkillAssessmentId { get; set; }

    public long TechnicianId { get; set; }

    public string AssessmentCode { get; set; } = string.Empty;

    public long? SkillTagId { get; set; }

    public string AssessmentName { get; set; } = string.Empty;

    public string AssessmentStatus { get; set; } = "Assigned";

    public decimal ScorePercentage { get; set; }

    public SkillAssessmentResult AssessmentResult { get; set; } = SkillAssessmentResult.Pending;

    public bool PassFlag { get; set; }

    public long? AssessedByUserId { get; set; }

    public string Remarks { get; set; } = string.Empty;

    public DateTime? AssessedOnUtc { get; set; }
}

public sealed class TrainingRecord : AuditableEntity
{
    public long TrainingRecordId { get; set; }

    public long TechnicianId { get; set; }

    public string TrainingName { get; set; } = string.Empty;

    public string TrainingTitle { get; set; } = string.Empty;

    public string TrainingType { get; set; } = string.Empty;

    public string TrainingStatus { get; set; } = "Assigned";

    public string CertificationNumber { get; set; } = string.Empty;

    public decimal ScorePercentage { get; set; }

    public DateTime CompletionDateUtc { get; set; }

    public DateTime? TrainingCompletionDateUtc { get; set; }

    public bool IsCompleted { get; set; }

    public long? TrainerUserId { get; set; }

    public string CertificateUrl { get; set; } = string.Empty;

    public string Remarks { get; set; } = string.Empty;
}

public sealed class PartsReturn : AuditableEntity
{
    public long PartsReturnId { get; set; }

    public string PartsReturnNumber { get; set; } = string.Empty;

    public long ItemId { get; set; }

    public long? SupplierId { get; set; }

    public long? TechnicianId { get; set; }

    public long? JobCardId { get; set; }

    public decimal Quantity { get; set; }

    public string ReasonCode { get; set; } = string.Empty;

    public string DefectDescription { get; set; } = string.Empty;

    public PartsReturnStatus PartsReturnStatus { get; set; } = PartsReturnStatus.Draft;

    public string ApprovalRemarks { get; set; } = string.Empty;

    public string SupplierClaimReference { get; set; } = string.Empty;

    public DateTime RequestedDateUtc { get; set; }

    public DateTime? ApprovedDateUtc { get; set; }
}

public sealed class Campaign : AuditableEntity
{
    public long CampaignId { get; set; }

    public string CampaignCode { get; set; } = string.Empty;

    public string CampaignName { get; set; } = string.Empty;

    public long ServiceId { get; set; }

    public long ZoneId { get; set; }

    public long SlotAvailabilityId { get; set; }

    public CampaignStatus CampaignStatus { get; set; } = CampaignStatus.Draft;

    public int PlannedBookingCount { get; set; }

    public int AllocatedBookingCount { get; set; }

    public DateTime StartDateUtc { get; set; }

    public DateTime EndDateUtc { get; set; }

    public string Notes { get; set; } = string.Empty;
}

public sealed class TechnicianEarning : AuditableEntity
{
    public long TechnicianEarningId { get; set; }

    public long TechnicianId { get; set; }

    public long? ServiceRequestId { get; set; }

    public long? InstallationOrderId { get; set; }

    public string EarningType { get; set; } = string.Empty;

    public decimal EarningAmount { get; set; }

    public TechnicianEarningStatus EarningStatus { get; set; } = TechnicianEarningStatus.Calculated;

    public DateTime CalculatedDateUtc { get; set; }

    public DateTime? ApprovedDateUtc { get; set; }
}

public sealed class FeatureFlag : AuditableEntity
{
    public long FeatureFlagId { get; set; }

    public string FlagCode { get; set; } = string.Empty;

    public string FlagName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public int RolloutPercentage { get; set; } = 100;

    public DateTime? StartsOnUtc { get; set; }

    public DateTime? EndsOnUtc { get; set; }
}

public sealed class SystemAlert : AuditableEntity
{
    public long SystemAlertId { get; set; }

    public string AlertCode { get; set; } = string.Empty;

    public string AlertType { get; set; } = string.Empty;

    public string RelatedEntityName { get; set; } = string.Empty;

    public string RelatedEntityId { get; set; } = string.Empty;

    public SystemAlertSeverity Severity { get; set; } = SystemAlertSeverity.Info;

    public SystemAlertStatus AlertStatus { get; set; } = SystemAlertStatus.Open;

    public DateTime? SlaDueDateUtc { get; set; }

    public int EscalationLevel { get; set; }

    public string NotificationChain { get; set; } = string.Empty;

    public string AlertMessage { get; set; } = string.Empty;

    public string TriggerCode { get; set; } = string.Empty;

    public DateTime? LastNotifiedDateUtc { get; set; }
}

public sealed class PaymentWebhookAttempt : AuditableEntity
{
    public long PaymentWebhookAttemptId { get; set; }

    public long InvoiceHeaderId { get; set; }

    public string IdempotencyKey { get; set; } = string.Empty;

    public string GatewayTransactionId { get; set; } = string.Empty;

    public string WebhookReference { get; set; } = string.Empty;

    public string SignatureHash { get; set; } = string.Empty;

    public decimal PaidAmount { get; set; }

    public string PayloadSnapshot { get; set; } = string.Empty;

    public PaymentWebhookAttemptStatus AttemptStatus { get; set; } = PaymentWebhookAttemptStatus.Pending;

    public int RetryCount { get; set; }

    public DateTime LastAttemptDateUtc { get; set; }

    public DateTime? NextRetryDateUtc { get; set; }

    public string FailureReason { get; set; } = string.Empty;
}

public sealed class OfflineSyncQueueItem : AuditableEntity
{
    public long OfflineSyncQueueItemId { get; set; }

    public string DeviceReference { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string EntityReference { get; set; } = string.Empty;

    public string PayloadSnapshot { get; set; } = string.Empty;

    public OfflineSyncStatus SyncStatus { get; set; } = OfflineSyncStatus.Pending;

    public int RetryCount { get; set; }

    public DateTime LastAttemptDateUtc { get; set; }

    public DateTime? NextRetryDateUtc { get; set; }

    public string ConflictStrategy { get; set; } = string.Empty;

    public string FailureReason { get; set; } = string.Empty;
}

public sealed class WorkflowStatusHistory : AuditableEntity
{
    public long WorkflowStatusHistoryId { get; set; }

    public WorkflowEntityType EntityType { get; set; } = WorkflowEntityType.Lead;

    public string EntityReference { get; set; } = string.Empty;

    public string PreviousStatus { get; set; } = string.Empty;

    public string CurrentStatus { get; set; } = string.Empty;

    public string Remarks { get; set; } = string.Empty;

    public string ChangedByRole { get; set; } = string.Empty;

    public DateTime ChangedDateUtc { get; set; }
}
