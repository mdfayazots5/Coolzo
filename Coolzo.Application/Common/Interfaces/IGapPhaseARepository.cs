using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;

namespace Coolzo.Application.Common.Interfaces;

public interface IGapPhaseARepository
{
    Task AddLeadAsync(Lead lead, CancellationToken cancellationToken);

    Task<bool> LeadNumberExistsAsync(string leadNumber, CancellationToken cancellationToken);

    Task<bool> ActiveLeadExistsByMobileAsync(string mobileNumber, DateTime createdFromUtc, CancellationToken cancellationToken);

    Task<Lead?> GetLeadByIdAsync(long leadId, CancellationToken cancellationToken);

    Task<Lead?> GetLeadByIdForUpdateAsync(long leadId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Lead>> SearchLeadsAsync(
        string? searchTerm,
        LeadStatus? leadStatus,
        LeadSourceChannel? sourceChannel,
        DateOnly? createdFrom,
        DateOnly? createdTo,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountLeadsAsync(
        string? searchTerm,
        LeadStatus? leadStatus,
        LeadSourceChannel? sourceChannel,
        DateOnly? createdFrom,
        DateOnly? createdTo,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Lead>> ListLeadsCreatedBetweenAsync(DateTime fromUtc, DateTime toUtcExclusive, CancellationToken cancellationToken);

    Task AddLeadAssignmentAsync(LeadAssignment leadAssignment, CancellationToken cancellationToken);

    Task AddLeadNoteAsync(LeadNote leadNote, CancellationToken cancellationToken);

    Task AddLeadConversionAsync(LeadConversion leadConversion, CancellationToken cancellationToken);

    Task AddInstallationOrderAsync(InstallationOrder installationOrder, CancellationToken cancellationToken);

    Task<bool> InstallationOrderNumberExistsAsync(string installationOrderNumber, CancellationToken cancellationToken);

    Task<InstallationOrder?> GetInstallationOrderByIdAsync(long installationOrderId, CancellationToken cancellationToken);

    Task<InstallationOrder?> GetInstallationOrderByIdForUpdateAsync(long installationOrderId, CancellationToken cancellationToken);

    Task AddCommissioningCertificateAsync(CommissioningCertificate commissioningCertificate, CancellationToken cancellationToken);

    Task<CancellationRecord?> GetCancellationByServiceRequestIdAsync(long serviceRequestId, CancellationToken cancellationToken);

    Task<CancellationRecord?> GetCancellationByBookingIdAsync(long bookingId, CancellationToken cancellationToken);

    Task AddCancellationRecordAsync(CancellationRecord cancellationRecord, CancellationToken cancellationToken);

    Task<CancellationRecord?> GetCancellationRecordByIdAsync(long cancellationRecordId, CancellationToken cancellationToken);

    Task<CancellationRecord?> GetCancellationRecordByIdForUpdateAsync(long cancellationRecordId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CancellationRecord>> SearchCancellationsAsync(
        long? bookingId,
        long? serviceRequestId,
        string? cancellationStatus,
        string? cancellationSource,
        string? cancellationReasonCode,
        int? branchId,
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        CancellationToken cancellationToken);

    Task AddRefundRequestAsync(RefundRequest refundRequest, CancellationToken cancellationToken);

    Task<RefundRequest?> GetRefundRequestByIdAsync(long refundRequestId, CancellationToken cancellationToken);

    Task<RefundRequest?> GetRefundRequestByIdForUpdateAsync(long refundRequestId, CancellationToken cancellationToken);

    Task<RefundRequest?> GetRefundRequestByCancellationRecordIdAsync(long cancellationRecordId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RefundRequest>> SearchRefundRequestsAsync(
        string? refundStatus,
        long? customerId,
        int? branchId,
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        CancellationToken cancellationToken);

    Task<decimal> GetTotalApprovedRefundAmountAsync(long invoiceHeaderId, CancellationToken cancellationToken);

    Task AddRefundApprovalAsync(RefundApproval refundApproval, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RefundApproval>> GetRefundApprovalsAsync(long refundRequestId, CancellationToken cancellationToken);

    Task AddRefundStatusHistoryAsync(RefundStatusHistory refundStatusHistory, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RefundStatusHistory>> GetRefundStatusHistoriesAsync(long refundRequestId, CancellationToken cancellationToken);

    Task AddCustomerAbsentRecordAsync(CustomerAbsentRecord customerAbsentRecord, CancellationToken cancellationToken);

    Task<CustomerAbsentRecord?> GetCustomerAbsentByServiceRequestIdAsync(long serviceRequestId, CancellationToken cancellationToken);

    Task<CustomerAbsentRecord?> GetCustomerAbsentByServiceRequestIdForUpdateAsync(long serviceRequestId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CustomerAbsentRecord>> SearchCustomerAbsentRecordsAsync(
        string? customerAbsentStatus,
        int? branchId,
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CancellationPolicy>> GetCancellationPoliciesAsync(
        int branchId,
        int companyId,
        int siteId,
        string? customerTypeCode,
        CancellationToken cancellationToken);

    Task AddTechnicianDocumentAsync(TechnicianDocument technicianDocument, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TechnicianDocument>> GetTechnicianDocumentsAsync(long technicianId, CancellationToken cancellationToken);

    Task AddSkillAssessmentAsync(SkillAssessment skillAssessment, CancellationToken cancellationToken);

    Task<SkillAssessment?> GetLatestSkillAssessmentAsync(long technicianId, CancellationToken cancellationToken);

    Task AddTrainingRecordAsync(TrainingRecord trainingRecord, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TrainingRecord>> GetTrainingRecordsAsync(long technicianId, CancellationToken cancellationToken);

    Task AddCampaignAsync(Campaign campaign, CancellationToken cancellationToken);

    Task<bool> CampaignCodeExistsAsync(string campaignCode, CancellationToken cancellationToken);

    Task<Campaign?> GetCampaignByIdAsync(long campaignId, CancellationToken cancellationToken);

    Task AddPartsReturnAsync(PartsReturn partsReturn, CancellationToken cancellationToken);

    Task<bool> PartsReturnNumberExistsAsync(string partsReturnNumber, CancellationToken cancellationToken);

    Task<PartsReturn?> GetPartsReturnByIdAsync(long partsReturnId, CancellationToken cancellationToken);

    Task<PartsReturn?> GetPartsReturnByIdForUpdateAsync(long partsReturnId, CancellationToken cancellationToken);

    Task AddTechnicianEarningAsync(TechnicianEarning technicianEarning, CancellationToken cancellationToken);

    Task<FeatureFlag?> GetFeatureFlagByCodeAsync(string flagCode, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<FeatureFlag>> GetFeatureFlagsAsync(bool? isEnabled, CancellationToken cancellationToken);

    Task AddFeatureFlagAsync(FeatureFlag featureFlag, CancellationToken cancellationToken);

    Task AddSystemAlertAsync(SystemAlert systemAlert, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SystemAlert>> GetOpenAlertsDueAsync(DateTime utcNow, CancellationToken cancellationToken);

    Task<int> CountOpenAlertsAsync(CancellationToken cancellationToken);

    Task AddWorkflowStatusHistoryAsync(WorkflowStatusHistory workflowStatusHistory, CancellationToken cancellationToken);

    Task AddPaymentWebhookAttemptAsync(PaymentWebhookAttempt paymentWebhookAttempt, CancellationToken cancellationToken);

    Task<PaymentWebhookAttempt?> GetWebhookAttemptByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken);

    Task<PaymentWebhookAttempt?> GetWebhookAttemptByGatewayTransactionIdAsync(string gatewayTransactionId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PaymentWebhookAttempt>> GetPendingWebhookAttemptsAsync(DateTime utcNow, CancellationToken cancellationToken);

    Task AddOfflineSyncQueueItemAsync(OfflineSyncQueueItem offlineSyncQueueItem, CancellationToken cancellationToken);

    Task<OfflineSyncQueueItem?> GetOfflineSyncQueueItemByReferenceAsync(string entityName, string entityReference, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<OfflineSyncQueueItem>> GetPendingOfflineSyncQueueItemsAsync(DateTime utcNow, CancellationToken cancellationToken);

    Task<int> CountPendingOfflineSyncItemsAsync(CancellationToken cancellationToken);
}
