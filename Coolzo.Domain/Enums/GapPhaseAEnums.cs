namespace Coolzo.Domain.Enums;

public enum LeadSourceChannel
{
    Web = 1,
    Phone = 2,
    WhatsApp = 3,
    MobileApp = 4,
    Manual = 5
}

public enum LeadStatus
{
    New = 1,
    Contacted = 2,
    Qualified = 3,
    Converted = 4,
    Lost = 5,
    Closed = 6
}

public enum LeadConversionType
{
    Booking = 1,
    ServiceRequest = 2
}

public enum InstallationOrderStatus
{
    Draft = 1,
    SurveyScheduled = 2,
    SurveyCompleted = 3,
    ApprovedForInstallation = 4,
    InstallationScheduled = 5,
    InstallationInProgress = 6,
    Commissioned = 7,
    Cancelled = 8,
    InstallationCompleted = 9
}

public enum SiteSurveyDecision
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum CancellationStatus
{
    Requested = 1,
    PendingApproval = 2,
    Approved = 3,
    Rejected = 4,
    Completed = 5,
    Cancelled = 6,
    RefundPending = 7,
    RefundApproved = 8,
    RefundRejected = 9,
    RefundProcessed = 10,
    Closed = 11
}

public enum RefundStatus
{
    Initiated = 1,
    PendingApproval = 2,
    Approved = 3,
    Processed = 4,
    Rejected = 5,
    Closed = 6
}

public enum RefundMethodType
{
    OriginalPaymentMethod = 1,
    CreditNote = 2,
    CashReturn = 3,
    ManualAdjustment = 4
}

public enum CustomerAbsentStatus
{
    Marked = 1,
    Rescheduled = 2,
    Cancelled = 3
}

public enum TechnicianDocumentStatus
{
    Uploaded = 1,
    Verified = 2,
    Rejected = 3
}

public enum SkillAssessmentResult
{
    Pending = 1,
    Passed = 2,
    Failed = 3
}

public enum CampaignStatus
{
    Draft = 1,
    Active = 2,
    Completed = 3,
    Closed = 4
}

public enum PartsReturnStatus
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    Rejected = 4,
    SupplierClaimRaised = 5,
    Closed = 6
}

public enum TechnicianEarningStatus
{
    Calculated = 1,
    Approved = 2,
    Paid = 3
}

public enum SystemAlertSeverity
{
    Info = 1,
    Warning = 2,
    Critical = 3
}

public enum SystemAlertStatus
{
    Open = 1,
    Acknowledged = 2,
    Resolved = 3
}

public enum OfflineSyncStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Conflict = 5
}

public enum PaymentWebhookAttemptStatus
{
    Pending = 1,
    Processed = 2,
    Failed = 3,
    RetryPending = 4
}

public enum WorkflowEntityType
{
    Lead = 1,
    InstallationOrder = 2,
    ServiceRequest = 3,
    Cancellation = 4,
    Refund = 5,
    TechnicianOnboarding = 6,
    Campaign = 7,
    PartsReturn = 8
}
