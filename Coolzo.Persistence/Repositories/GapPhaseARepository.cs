using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class GapPhaseARepository : IGapPhaseARepository
{
    private readonly CoolzoDbContext _dbContext;

    public GapPhaseARepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddLeadAsync(Lead lead, CancellationToken cancellationToken)
    {
        return _dbContext.Leads.AddAsync(lead, cancellationToken).AsTask();
    }

    public Task<bool> LeadNumberExistsAsync(string leadNumber, CancellationToken cancellationToken)
    {
        return _dbContext.Leads.AnyAsync(entity => !entity.IsDeleted && entity.LeadNumber == leadNumber, cancellationToken);
    }

    public Task<bool> ActiveLeadExistsByMobileAsync(string mobileNumber, DateTime createdFromUtc, CancellationToken cancellationToken)
    {
        return _dbContext.Leads.AnyAsync(
            entity =>
                !entity.IsDeleted &&
                entity.MobileNumber == mobileNumber &&
                entity.DateCreated >= createdFromUtc &&
                entity.LeadStatus != LeadStatus.Lost &&
                entity.LeadStatus != LeadStatus.Closed,
            cancellationToken);
    }

    public Task<Lead?> GetLeadByIdAsync(long leadId, CancellationToken cancellationToken)
    {
        return BuildLeadQuery(asNoTracking: true)
            .FirstOrDefaultAsync(entity => entity.LeadId == leadId, cancellationToken);
    }

    public Task<Lead?> GetLeadByIdForUpdateAsync(long leadId, CancellationToken cancellationToken)
    {
        return BuildLeadQuery(asNoTracking: false)
            .FirstOrDefaultAsync(entity => entity.LeadId == leadId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Lead>> SearchLeadsAsync(
        string? searchTerm,
        LeadStatus? leadStatus,
        LeadSourceChannel? sourceChannel,
        DateOnly? createdFrom,
        DateOnly? createdTo,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;

        return await ApplyLeadSearchFilters(BuildLeadQuery(asNoTracking: true), searchTerm, leadStatus, sourceChannel, createdFrom, createdTo)
            .OrderByDescending(entity => entity.DateCreated)
            .ThenByDescending(entity => entity.LeadId)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountLeadsAsync(
        string? searchTerm,
        LeadStatus? leadStatus,
        LeadSourceChannel? sourceChannel,
        DateOnly? createdFrom,
        DateOnly? createdTo,
        CancellationToken cancellationToken)
    {
        return ApplyLeadSearchFilters(_dbContext.Leads.Where(entity => !entity.IsDeleted), searchTerm, leadStatus, sourceChannel, createdFrom, createdTo)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Lead>> ListLeadsCreatedBetweenAsync(DateTime fromUtc, DateTime toUtcExclusive, CancellationToken cancellationToken)
    {
        return await _dbContext.Leads
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted && entity.DateCreated >= fromUtc && entity.DateCreated < toUtcExclusive)
            .OrderBy(entity => entity.DateCreated)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddLeadAssignmentAsync(LeadAssignment leadAssignment, CancellationToken cancellationToken)
    {
        return _dbContext.LeadAssignments.AddAsync(leadAssignment, cancellationToken).AsTask();
    }

    public Task AddLeadNoteAsync(LeadNote leadNote, CancellationToken cancellationToken)
    {
        return _dbContext.LeadNotes.AddAsync(leadNote, cancellationToken).AsTask();
    }

    public Task AddLeadConversionAsync(LeadConversion leadConversion, CancellationToken cancellationToken)
    {
        return _dbContext.LeadConversions.AddAsync(leadConversion, cancellationToken).AsTask();
    }

    public Task AddInstallationOrderAsync(InstallationOrder installationOrder, CancellationToken cancellationToken)
    {
        return _dbContext.InstallationOrders.AddAsync(installationOrder, cancellationToken).AsTask();
    }

    public Task<bool> InstallationOrderNumberExistsAsync(string installationOrderNumber, CancellationToken cancellationToken)
    {
        return _dbContext.InstallationOrders.AnyAsync(
            entity => !entity.IsDeleted && entity.InstallationOrderNumber == installationOrderNumber,
            cancellationToken);
    }

    public Task<InstallationOrder?> GetInstallationOrderByIdAsync(long installationOrderId, CancellationToken cancellationToken)
    {
        return BuildInstallationOrderQuery(asNoTracking: true)
            .FirstOrDefaultAsync(entity => entity.InstallationOrderId == installationOrderId, cancellationToken);
    }

    public Task<InstallationOrder?> GetInstallationOrderByIdForUpdateAsync(long installationOrderId, CancellationToken cancellationToken)
    {
        return BuildInstallationOrderQuery(asNoTracking: false)
            .FirstOrDefaultAsync(entity => entity.InstallationOrderId == installationOrderId, cancellationToken);
    }

    public Task AddCommissioningCertificateAsync(CommissioningCertificate commissioningCertificate, CancellationToken cancellationToken)
    {
        return _dbContext.CommissioningCertificates.AddAsync(commissioningCertificate, cancellationToken).AsTask();
    }

    public Task<CancellationRecord?> GetCancellationByServiceRequestIdAsync(long serviceRequestId, CancellationToken cancellationToken)
    {
        return _dbContext.CancellationRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.ServiceRequestId == serviceRequestId, cancellationToken);
    }

    public Task<CancellationRecord?> GetCancellationByBookingIdAsync(long bookingId, CancellationToken cancellationToken)
    {
        return _dbContext.CancellationRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.BookingId == bookingId, cancellationToken);
    }

    public Task AddCancellationRecordAsync(CancellationRecord cancellationRecord, CancellationToken cancellationToken)
    {
        return _dbContext.CancellationRecords.AddAsync(cancellationRecord, cancellationToken).AsTask();
    }

    public Task<CancellationRecord?> GetCancellationRecordByIdAsync(long cancellationRecordId, CancellationToken cancellationToken)
    {
        return _dbContext.CancellationRecords
            .AsNoTracking()
            .Include(entity => entity.Booking)
            .Include(entity => entity.ServiceRequest)
            .Include(entity => entity.RefundRequests)
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.CancellationRecordId == cancellationRecordId, cancellationToken);
    }

    public Task<CancellationRecord?> GetCancellationRecordByIdForUpdateAsync(long cancellationRecordId, CancellationToken cancellationToken)
    {
        return _dbContext.CancellationRecords
            .Include(entity => entity.Booking)
            .Include(entity => entity.ServiceRequest)
            .Include(entity => entity.RefundRequests)
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.CancellationRecordId == cancellationRecordId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<CancellationRecord>> SearchCancellationsAsync(
        long? bookingId,
        long? serviceRequestId,
        string? cancellationStatus,
        string? cancellationSource,
        string? cancellationReasonCode,
        int? branchId,
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.CancellationRecords
            .AsNoTracking()
            .Include(entity => entity.Booking)
            .Include(entity => entity.ServiceRequest)
            .Include(entity => entity.RefundRequests)
            .Where(entity => !entity.IsDeleted);

        if (bookingId.HasValue)
        {
            query = query.Where(entity => entity.BookingId == bookingId.Value);
        }

        if (serviceRequestId.HasValue)
        {
            query = query.Where(entity => entity.ServiceRequestId == serviceRequestId.Value);
        }

        if (!string.IsNullOrWhiteSpace(cancellationStatus) &&
            Enum.TryParse<CancellationStatus>(cancellationStatus, true, out var parsedCancellationStatus))
        {
            query = query.Where(entity => entity.CancellationStatus == parsedCancellationStatus);
        }

        if (!string.IsNullOrWhiteSpace(cancellationSource))
        {
            query = query.Where(entity => entity.CancellationSource == cancellationSource);
        }

        if (!string.IsNullOrWhiteSpace(cancellationReasonCode))
        {
            query = query.Where(entity => entity.CancellationReasonCode == cancellationReasonCode || entity.ReasonCode == cancellationReasonCode);
        }

        if (branchId.HasValue)
        {
            query = query.Where(entity => entity.BranchId == branchId.Value);
        }

        if (fromDateUtc.HasValue)
        {
            query = query.Where(entity => entity.DateCreated >= fromDateUtc.Value);
        }

        if (toDateUtc.HasValue)
        {
            query = query.Where(entity => entity.DateCreated <= toDateUtc.Value);
        }

        return await query
            .OrderByDescending(entity => entity.DateCreated)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddRefundRequestAsync(RefundRequest refundRequest, CancellationToken cancellationToken)
    {
        return _dbContext.RefundRequests.AddAsync(refundRequest, cancellationToken).AsTask();
    }

    public Task<RefundRequest?> GetRefundRequestByIdAsync(long refundRequestId, CancellationToken cancellationToken)
    {
        return _dbContext.RefundRequests
            .AsNoTracking()
            .Include(entity => entity.CancellationRecord)
            .Include(entity => entity.InvoiceHeader)
            .Include(entity => entity.PaymentTransaction)
            .Include(entity => entity.Approvals)
            .Include(entity => entity.StatusHistory)
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.RefundRequestId == refundRequestId, cancellationToken);
    }

    public Task<RefundRequest?> GetRefundRequestByIdForUpdateAsync(long refundRequestId, CancellationToken cancellationToken)
    {
        return _dbContext.RefundRequests
            .Include(entity => entity.CancellationRecord)
            .Include(entity => entity.InvoiceHeader)
            .Include(entity => entity.PaymentTransaction)
            .Include(entity => entity.Approvals)
            .Include(entity => entity.StatusHistory)
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.RefundRequestId == refundRequestId, cancellationToken);
    }

    public Task<RefundRequest?> GetRefundRequestByCancellationRecordIdAsync(long cancellationRecordId, CancellationToken cancellationToken)
    {
        return _dbContext.RefundRequests
            .AsNoTracking()
            .Include(entity => entity.Approvals)
            .Include(entity => entity.StatusHistory)
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.CancellationRecordId == cancellationRecordId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<RefundRequest>> SearchRefundRequestsAsync(
        string? refundStatus,
        long? customerId,
        int? branchId,
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.RefundRequests
            .AsNoTracking()
            .Include(entity => entity.CancellationRecord)
            .Include(entity => entity.InvoiceHeader)
            .Include(entity => entity.PaymentTransaction)
            .Include(entity => entity.Approvals)
            .Include(entity => entity.StatusHistory)
            .Where(entity => !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(refundStatus) && Enum.TryParse<RefundStatus>(refundStatus, true, out var parsedRefundStatus))
        {
            query = query.Where(entity => entity.RefundStatus == parsedRefundStatus);
        }

        if (customerId.HasValue)
        {
            query = query.Where(entity => entity.InvoiceHeader != null && entity.InvoiceHeader.CustomerId == customerId.Value);
        }

        if (branchId.HasValue)
        {
            query = query.Where(entity => entity.BranchId == branchId.Value);
        }

        if (fromDateUtc.HasValue)
        {
            query = query.Where(entity => entity.DateCreated >= fromDateUtc.Value);
        }

        if (toDateUtc.HasValue)
        {
            query = query.Where(entity => entity.DateCreated <= toDateUtc.Value);
        }

        return await query
            .OrderByDescending(entity => entity.DateCreated)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalApprovedRefundAmountAsync(long invoiceHeaderId, CancellationToken cancellationToken)
    {
        return await _dbContext.RefundRequests
            .Where(entity =>
                !entity.IsDeleted &&
                entity.InvoiceHeaderId == invoiceHeaderId &&
                (entity.RefundStatus == RefundStatus.Approved || entity.RefundStatus == RefundStatus.Processed))
            .SumAsync(entity => (decimal?)entity.ApprovedAmount, cancellationToken) ?? 0.00m;
    }

    public Task AddRefundApprovalAsync(RefundApproval refundApproval, CancellationToken cancellationToken)
    {
        return _dbContext.RefundApprovals.AddAsync(refundApproval, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<RefundApproval>> GetRefundApprovalsAsync(long refundRequestId, CancellationToken cancellationToken)
    {
        return await _dbContext.RefundApprovals
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted && entity.RefundRequestId == refundRequestId)
            .OrderBy(entity => entity.ApprovalLevel)
            .ThenBy(entity => entity.DateCreated)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddRefundStatusHistoryAsync(RefundStatusHistory refundStatusHistory, CancellationToken cancellationToken)
    {
        return _dbContext.RefundStatusHistories.AddAsync(refundStatusHistory, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<RefundStatusHistory>> GetRefundStatusHistoriesAsync(long refundRequestId, CancellationToken cancellationToken)
    {
        return await _dbContext.RefundStatusHistories
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted && entity.RefundRequestId == refundRequestId)
            .OrderBy(entity => entity.ChangedOn)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddCustomerAbsentRecordAsync(CustomerAbsentRecord customerAbsentRecord, CancellationToken cancellationToken)
    {
        return _dbContext.CustomerAbsentRecords.AddAsync(customerAbsentRecord, cancellationToken).AsTask();
    }

    public Task<CustomerAbsentRecord?> GetCustomerAbsentByServiceRequestIdAsync(long serviceRequestId, CancellationToken cancellationToken)
    {
        return _dbContext.CustomerAbsentRecords
            .AsNoTracking()
            .Include(entity => entity.ServiceRequest)
            .Include(entity => entity.Technician)
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.ServiceRequestId == serviceRequestId, cancellationToken);
    }

    public Task<CustomerAbsentRecord?> GetCustomerAbsentByServiceRequestIdForUpdateAsync(long serviceRequestId, CancellationToken cancellationToken)
    {
        return _dbContext.CustomerAbsentRecords
            .Include(entity => entity.ServiceRequest)
            .Include(entity => entity.Technician)
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.ServiceRequestId == serviceRequestId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<CustomerAbsentRecord>> SearchCustomerAbsentRecordsAsync(
        string? customerAbsentStatus,
        int? branchId,
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.CustomerAbsentRecords
            .AsNoTracking()
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.Booking)
            .Include(entity => entity.Technician)
            .Where(entity => !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(customerAbsentStatus) &&
            Enum.TryParse<CustomerAbsentStatus>(customerAbsentStatus, true, out var parsedCustomerAbsentStatus))
        {
            query = query.Where(entity => entity.CustomerAbsentStatus == parsedCustomerAbsentStatus);
        }

        if (branchId.HasValue)
        {
            query = query.Where(entity => entity.BranchId == branchId.Value);
        }

        if (fromDateUtc.HasValue)
        {
            query = query.Where(entity => entity.MarkedOn >= fromDateUtc.Value);
        }

        if (toDateUtc.HasValue)
        {
            query = query.Where(entity => entity.MarkedOn <= toDateUtc.Value);
        }

        return await query
            .OrderByDescending(entity => entity.MarkedOn)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CancellationPolicy>> GetCancellationPoliciesAsync(
        int branchId,
        int companyId,
        int siteId,
        string? customerTypeCode,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.CancellationPolicies
            .AsNoTracking()
            .Where(entity =>
                !entity.IsDeleted &&
                entity.CompanyId == companyId &&
                entity.SiteId == siteId &&
                (entity.BranchId == branchId || entity.BranchId == 1));

        if (!string.IsNullOrWhiteSpace(customerTypeCode))
        {
            query = query.Where(entity => entity.CustomerTypeCode == string.Empty || entity.CustomerTypeCode == customerTypeCode);
        }
        else
        {
            query = query.Where(entity => entity.CustomerTypeCode == string.Empty);
        }

        return await query
            .OrderByDescending(entity => entity.BranchId == branchId)
            .ThenBy(entity => entity.MinTimeToSlotMinutes)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddTechnicianDocumentAsync(TechnicianDocument technicianDocument, CancellationToken cancellationToken)
    {
        return _dbContext.TechnicianDocuments.AddAsync(technicianDocument, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<TechnicianDocument>> GetTechnicianDocumentsAsync(long technicianId, CancellationToken cancellationToken)
    {
        return await _dbContext.TechnicianDocuments
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted && entity.TechnicianId == technicianId)
            .OrderByDescending(entity => entity.DateCreated)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddSkillAssessmentAsync(SkillAssessment skillAssessment, CancellationToken cancellationToken)
    {
        return _dbContext.SkillAssessments.AddAsync(skillAssessment, cancellationToken).AsTask();
    }

    public Task<SkillAssessment?> GetLatestSkillAssessmentAsync(long technicianId, CancellationToken cancellationToken)
    {
        return _dbContext.SkillAssessments
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted && entity.TechnicianId == technicianId)
            .OrderByDescending(entity => entity.AssessedOnUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task AddTrainingRecordAsync(TrainingRecord trainingRecord, CancellationToken cancellationToken)
    {
        return _dbContext.TrainingRecords.AddAsync(trainingRecord, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<TrainingRecord>> GetTrainingRecordsAsync(long technicianId, CancellationToken cancellationToken)
    {
        return await _dbContext.TrainingRecords
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted && entity.TechnicianId == technicianId)
            .OrderByDescending(entity => entity.CompletionDateUtc)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddCampaignAsync(Campaign campaign, CancellationToken cancellationToken)
    {
        return _dbContext.Campaigns.AddAsync(campaign, cancellationToken).AsTask();
    }

    public Task<bool> CampaignCodeExistsAsync(string campaignCode, CancellationToken cancellationToken)
    {
        return _dbContext.Campaigns.AnyAsync(entity => !entity.IsDeleted && entity.CampaignCode == campaignCode, cancellationToken);
    }

    public Task<Campaign?> GetCampaignByIdAsync(long campaignId, CancellationToken cancellationToken)
    {
        return _dbContext.Campaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.CampaignId == campaignId, cancellationToken);
    }

    public Task AddPartsReturnAsync(PartsReturn partsReturn, CancellationToken cancellationToken)
    {
        return _dbContext.PartsReturns.AddAsync(partsReturn, cancellationToken).AsTask();
    }

    public Task<bool> PartsReturnNumberExistsAsync(string partsReturnNumber, CancellationToken cancellationToken)
    {
        return _dbContext.PartsReturns.AnyAsync(
            entity => !entity.IsDeleted && entity.PartsReturnNumber == partsReturnNumber,
            cancellationToken);
    }

    public Task<PartsReturn?> GetPartsReturnByIdAsync(long partsReturnId, CancellationToken cancellationToken)
    {
        return _dbContext.PartsReturns
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.PartsReturnId == partsReturnId, cancellationToken);
    }

    public Task<PartsReturn?> GetPartsReturnByIdForUpdateAsync(long partsReturnId, CancellationToken cancellationToken)
    {
        return _dbContext.PartsReturns
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.PartsReturnId == partsReturnId, cancellationToken);
    }

    public Task AddTechnicianEarningAsync(TechnicianEarning technicianEarning, CancellationToken cancellationToken)
    {
        return _dbContext.TechnicianEarnings.AddAsync(technicianEarning, cancellationToken).AsTask();
    }

    public Task<FeatureFlag?> GetFeatureFlagByCodeAsync(string flagCode, CancellationToken cancellationToken)
    {
        return _dbContext.FeatureFlags
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.FlagCode == flagCode, cancellationToken);
    }

    public async Task<IReadOnlyCollection<FeatureFlag>> GetFeatureFlagsAsync(bool? isEnabled, CancellationToken cancellationToken)
    {
        var query = _dbContext.FeatureFlags
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted);

        if (isEnabled.HasValue)
        {
            query = query.Where(entity => entity.IsEnabled == isEnabled.Value);
        }

        return await query
            .OrderBy(entity => entity.FlagCode)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddFeatureFlagAsync(FeatureFlag featureFlag, CancellationToken cancellationToken)
    {
        return _dbContext.FeatureFlags.AddAsync(featureFlag, cancellationToken).AsTask();
    }

    public Task AddSystemAlertAsync(SystemAlert systemAlert, CancellationToken cancellationToken)
    {
        return _dbContext.SystemAlerts.AddAsync(systemAlert, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<SystemAlert>> GetOpenAlertsDueAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        return await _dbContext.SystemAlerts
            .Where(entity =>
                !entity.IsDeleted &&
                entity.AlertStatus == SystemAlertStatus.Open &&
                entity.SlaDueDateUtc.HasValue &&
                entity.SlaDueDateUtc <= utcNow)
            .OrderBy(entity => entity.SlaDueDateUtc)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountOpenAlertsAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SystemAlerts.CountAsync(
            entity => !entity.IsDeleted && entity.AlertStatus == SystemAlertStatus.Open,
            cancellationToken);
    }

    public Task AddWorkflowStatusHistoryAsync(WorkflowStatusHistory workflowStatusHistory, CancellationToken cancellationToken)
    {
        return _dbContext.WorkflowStatusHistories.AddAsync(workflowStatusHistory, cancellationToken).AsTask();
    }

    public Task AddPaymentWebhookAttemptAsync(PaymentWebhookAttempt paymentWebhookAttempt, CancellationToken cancellationToken)
    {
        return _dbContext.PaymentWebhookAttempts.AddAsync(paymentWebhookAttempt, cancellationToken).AsTask();
    }

    public Task<PaymentWebhookAttempt?> GetWebhookAttemptByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        return _dbContext.PaymentWebhookAttempts
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public Task<PaymentWebhookAttempt?> GetWebhookAttemptByGatewayTransactionIdAsync(string gatewayTransactionId, CancellationToken cancellationToken)
    {
        return _dbContext.PaymentWebhookAttempts
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.GatewayTransactionId == gatewayTransactionId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<PaymentWebhookAttempt>> GetPendingWebhookAttemptsAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        return await _dbContext.PaymentWebhookAttempts
            .Where(entity =>
                !entity.IsDeleted &&
                (entity.AttemptStatus == PaymentWebhookAttemptStatus.Pending || entity.AttemptStatus == PaymentWebhookAttemptStatus.RetryPending) &&
                (!entity.NextRetryDateUtc.HasValue || entity.NextRetryDateUtc <= utcNow))
            .OrderBy(entity => entity.NextRetryDateUtc ?? entity.LastAttemptDateUtc)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddOfflineSyncQueueItemAsync(OfflineSyncQueueItem offlineSyncQueueItem, CancellationToken cancellationToken)
    {
        return _dbContext.OfflineSyncQueueItems.AddAsync(offlineSyncQueueItem, cancellationToken).AsTask();
    }

    public Task<OfflineSyncQueueItem?> GetOfflineSyncQueueItemByReferenceAsync(string entityName, string entityReference, CancellationToken cancellationToken)
    {
        return _dbContext.OfflineSyncQueueItems
            .FirstOrDefaultAsync(
                entity =>
                    !entity.IsDeleted &&
                    entity.EntityName == entityName &&
                    entity.EntityReference == entityReference,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<OfflineSyncQueueItem>> GetPendingOfflineSyncQueueItemsAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        return await _dbContext.OfflineSyncQueueItems
            .Where(entity =>
                !entity.IsDeleted &&
                (entity.SyncStatus == OfflineSyncStatus.Pending || entity.SyncStatus == OfflineSyncStatus.Failed) &&
                (!entity.NextRetryDateUtc.HasValue || entity.NextRetryDateUtc <= utcNow))
            .OrderBy(entity => entity.NextRetryDateUtc ?? entity.LastAttemptDateUtc)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountPendingOfflineSyncItemsAsync(CancellationToken cancellationToken)
    {
        return _dbContext.OfflineSyncQueueItems.CountAsync(
            entity =>
                !entity.IsDeleted &&
                (entity.SyncStatus == OfflineSyncStatus.Pending || entity.SyncStatus == OfflineSyncStatus.Failed),
            cancellationToken);
    }

    private IQueryable<Lead> BuildLeadQuery(bool asNoTracking)
    {
        IQueryable<Lead> query = _dbContext.Leads
            .Include(entity => entity.AssignedUser)
            .Include(entity => entity.StatusHistories.Where(history => !history.IsDeleted))
            .Include(entity => entity.Assignments.Where(assignment => !assignment.IsDeleted))
                .ThenInclude(assignment => assignment.AssignedUser)
            .Include(entity => entity.Notes.Where(note => !note.IsDeleted))
            .Include(entity => entity.Conversions.Where(conversion => !conversion.IsDeleted))
            .Where(entity => !entity.IsDeleted);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    private static IQueryable<Lead> ApplyLeadSearchFilters(
        IQueryable<Lead> query,
        string? searchTerm,
        LeadStatus? leadStatus,
        LeadSourceChannel? sourceChannel,
        DateOnly? createdFrom,
        DateOnly? createdTo)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearch = searchTerm.Trim();
            query = query.Where(
                entity =>
                    entity.LeadNumber.Contains(normalizedSearch) ||
                    entity.CustomerName.Contains(normalizedSearch) ||
                    entity.MobileNumber.Contains(normalizedSearch) ||
                    entity.EmailAddress.Contains(normalizedSearch));
        }

        if (leadStatus.HasValue)
        {
            query = query.Where(entity => entity.LeadStatus == leadStatus.Value);
        }

        if (sourceChannel.HasValue)
        {
            query = query.Where(entity => entity.SourceChannel == sourceChannel.Value);
        }

        if (createdFrom.HasValue)
        {
            var fromUtc = createdFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(entity => entity.DateCreated >= fromUtc);
        }

        if (createdTo.HasValue)
        {
            var toUtcExclusive = createdTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(entity => entity.DateCreated < toUtcExclusive);
        }

        return query;
    }

    private IQueryable<InstallationOrder> BuildInstallationOrderQuery(bool asNoTracking)
    {
        IQueryable<InstallationOrder> query = _dbContext.InstallationOrders
            .Include(entity => entity.SiteSurveyReports.Where(report => !report.IsDeleted))
            .Where(entity => !entity.IsDeleted);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }
}
