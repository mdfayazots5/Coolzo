using Coolzo.Domain.Entities;
using Coolzo.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Context;

public sealed class CoolzoDbContext : DbContext
{
    public CoolzoDbContext(DbContextOptions<CoolzoDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<AcType> AcTypes => Set<AcType>();

    public DbSet<Brand> Brands => Set<Brand>();

    public DbSet<BillingStatusHistory> BillingStatusHistories => Set<BillingStatusHistory>();

    public DbSet<ComplaintIssueMaster> ComplaintIssueMasters => Set<ComplaintIssueMaster>();

    public DbSet<AssignmentLog> AssignmentLogs => Set<AssignmentLog>();

    public DbSet<AmcPlan> AmcPlans => Set<AmcPlan>();

    public DbSet<AmcVisitSchedule> AmcVisitSchedules => Set<AmcVisitSchedule>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<BookingLine> BookingLines => Set<BookingLine>();

    public DbSet<BookingStatusHistory> BookingStatusHistories => Set<BookingStatusHistory>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<CustomerAmc> CustomerAmcs => Set<CustomerAmc>();

    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();

    public DbSet<Lead> Leads => Set<Lead>();

    public DbSet<LeadSource> LeadSources => Set<LeadSource>();

    public DbSet<LeadStatusHistory> LeadStatusHistories => Set<LeadStatusHistory>();

    public DbSet<LeadAssignment> LeadAssignments => Set<LeadAssignment>();

    public DbSet<LeadNote> LeadNotes => Set<LeadNote>();

    public DbSet<LeadConversion> LeadConversions => Set<LeadConversion>();

    public DbSet<DiagnosisResultMaster> DiagnosisResultMasters => Set<DiagnosisResultMaster>();

    public DbSet<Item> Items => Set<Item>();

    public DbSet<ItemCategory> ItemCategories => Set<ItemCategory>();

    public DbSet<ItemRate> ItemRates => Set<ItemRate>();

    public DbSet<JobAttachment> JobAttachments => Set<JobAttachment>();

    public DbSet<JobCard> JobCards => Set<JobCard>();

    public DbSet<JobChecklistResponse> JobChecklistResponses => Set<JobChecklistResponse>();

    public DbSet<JobDiagnosis> JobDiagnoses => Set<JobDiagnosis>();

    public DbSet<JobExecutionNote> JobExecutionNotes => Set<JobExecutionNote>();

    public DbSet<JobExecutionTimeline> JobExecutionTimelines => Set<JobExecutionTimeline>();

    public DbSet<JobPartConsumption> JobPartConsumptions => Set<JobPartConsumption>();

    public DbSet<InvoiceHeader> InvoiceHeaders => Set<InvoiceHeader>();

    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();

    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<PaymentReceipt> PaymentReceipts => Set<PaymentReceipt>();

    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    public DbSet<PaymentWebhookAttempt> PaymentWebhookAttempts => Set<PaymentWebhookAttempt>();

    public DbSet<PricingModel> PricingModels => Set<PricingModel>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<QuotationHeader> QuotationHeaders => Set<QuotationHeader>();

    public DbSet<QuotationLine> QuotationLines => Set<QuotationLine>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<Service> Services => Set<Service>();

    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

    public DbSet<ServiceRequestAssignment> ServiceRequestAssignments => Set<ServiceRequestAssignment>();

    public DbSet<ServiceRequestStatusHistory> ServiceRequestStatusHistories => Set<ServiceRequestStatusHistory>();

    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();

    public DbSet<SlotAvailability> SlotAvailabilities => Set<SlotAvailability>();

    public DbSet<SlotConfiguration> SlotConfigurations => Set<SlotConfiguration>();

    public DbSet<ServiceChecklistMaster> ServiceChecklistMasters => Set<ServiceChecklistMaster>();

    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();

    public DbSet<BusinessHourConfiguration> BusinessHourConfigurations => Set<BusinessHourConfiguration>();

    public DbSet<HolidayConfiguration> HolidayConfigurations => Set<HolidayConfiguration>();

    public DbSet<CMSBlock> CMSBlocks => Set<CMSBlock>();

    public DbSet<CMSBanner> CMSBanners => Set<CMSBanner>();

    public DbSet<CMSFaq> CMSFaqs => Set<CMSFaq>();

    public DbSet<CMSContentVersion> CMSContentVersions => Set<CMSContentVersion>();

    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

    public DbSet<NotificationTriggerConfiguration> NotificationTriggerConfigurations => Set<NotificationTriggerConfiguration>();

    public DbSet<CommunicationPreference> CommunicationPreferences => Set<CommunicationPreference>();

    public DbSet<DynamicMasterRecord> DynamicMasterRecords => Set<DynamicMasterRecord>();

    public DbSet<DisplayContentSetting> DisplayContentSettings => Set<DisplayContentSetting>();

    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();

    public DbSet<SupportTicketAssignment> SupportTicketAssignments => Set<SupportTicketAssignment>();

    public DbSet<SupportTicketCategory> SupportTicketCategories => Set<SupportTicketCategory>();

    public DbSet<SupportTicketEscalation> SupportTicketEscalations => Set<SupportTicketEscalation>();

    public DbSet<SupportTicketLink> SupportTicketLinks => Set<SupportTicketLink>();

    public DbSet<SupportTicketPriority> SupportTicketPriorities => Set<SupportTicketPriority>();

    public DbSet<SupportTicketReply> SupportTicketReplies => Set<SupportTicketReply>();

    public DbSet<SupportTicketStatusHistory> SupportTicketStatusHistories => Set<SupportTicketStatusHistory>();

    public DbSet<SystemAlert> SystemAlerts => Set<SystemAlert>();

    public DbSet<RevisitRequest> RevisitRequests => Set<RevisitRequest>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<Tonnage> Tonnages => Set<Tonnage>();

    public DbSet<Technician> Technicians => Set<Technician>();

    public DbSet<TechnicianAvailability> TechnicianAvailabilities => Set<TechnicianAvailability>();

    public DbSet<TechnicianDocument> TechnicianDocuments => Set<TechnicianDocument>();

    public DbSet<SkillAssessment> SkillAssessments => Set<SkillAssessment>();

    public DbSet<TrainingRecord> TrainingRecords => Set<TrainingRecord>();

    public DbSet<TechnicianActivationLog> TechnicianActivationLogs => Set<TechnicianActivationLog>();

    public DbSet<HelperProfile> HelperProfiles => Set<HelperProfile>();

    public DbSet<HelperAssignment> HelperAssignments => Set<HelperAssignment>();

    public DbSet<HelperTaskChecklist> HelperTaskChecklists => Set<HelperTaskChecklist>();

    public DbSet<HelperTaskResponse> HelperTaskResponses => Set<HelperTaskResponse>();

    public DbSet<HelperAttendance> HelperAttendances => Set<HelperAttendance>();

    public DbSet<TechnicianEarning> TechnicianEarnings => Set<TechnicianEarning>();

    public DbSet<TechnicianVanStock> TechnicianVanStocks => Set<TechnicianVanStock>();

    public DbSet<TechnicianSkillMapping> TechnicianSkillMappings => Set<TechnicianSkillMapping>();

    public DbSet<User> Users => Set<User>();

    public DbSet<UserPasswordHistory> UserPasswordHistories => Set<UserPasswordHistory>();

    public DbSet<WarrantyClaim> WarrantyClaims => Set<WarrantyClaim>();

    public DbSet<WarrantyRule> WarrantyRules => Set<WarrantyRule>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<UserSession> UserSessions => Set<UserSession>();

    public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();

    public DbSet<InstallationLead> InstallationLeads => Set<InstallationLead>();

    public DbSet<InstallationSurvey> InstallationSurveys => Set<InstallationSurvey>();

    public DbSet<InstallationSurveyItem> InstallationSurveyItems => Set<InstallationSurveyItem>();

    public DbSet<InstallationProposal> InstallationProposals => Set<InstallationProposal>();

    public DbSet<InstallationProposalLine> InstallationProposalLines => Set<InstallationProposalLine>();

    public DbSet<InstallationChecklist> InstallationChecklists => Set<InstallationChecklist>();

    public DbSet<InstallationChecklistResponse> InstallationChecklistResponses => Set<InstallationChecklistResponse>();

    public DbSet<InstallationStatusHistory> InstallationStatusHistories => Set<InstallationStatusHistory>();

    public DbSet<InstallationOrder> InstallationOrders => Set<InstallationOrder>();

    public DbSet<SiteSurveyReport> SiteSurveyReports => Set<SiteSurveyReport>();

    public DbSet<CommissioningCertificate> CommissioningCertificates => Set<CommissioningCertificate>();

    public DbSet<CancellationRecord> CancellationRecords => Set<CancellationRecord>();

    public DbSet<CancellationPolicy> CancellationPolicies => Set<CancellationPolicy>();

    public DbSet<RefundRequest> RefundRequests => Set<RefundRequest>();

    public DbSet<RefundApproval> RefundApprovals => Set<RefundApproval>();

    public DbSet<RefundStatusHistory> RefundStatusHistories => Set<RefundStatusHistory>();

    public DbSet<CustomerAbsentRecord> CustomerAbsentRecords => Set<CustomerAbsentRecord>();

    public DbSet<PartsReturn> PartsReturns => Set<PartsReturn>();

    public DbSet<Campaign> Campaigns => Set<Campaign>();

    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();

    public DbSet<OfflineSyncQueueItem> OfflineSyncQueueItems => Set<OfflineSyncQueueItem>();

    public DbSet<WorkflowStatusHistory> WorkflowStatusHistories => Set<WorkflowStatusHistory>();

    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    public DbSet<WarehouseStock> WarehouseStocks => Set<WarehouseStock>();

    public DbSet<Zone> Zones => Set<Zone>();

    public DbSet<ZonePincode> ZonePincodes => Set<ZonePincode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoolzoDbContext).Assembly);
        modelBuilder.ConfigurePhase08AdminEntities();
    }
}
