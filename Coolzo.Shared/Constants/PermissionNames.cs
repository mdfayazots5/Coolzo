namespace Coolzo.Shared.Constants;

public static class PermissionNames
{
    public const string AuthLogin = "auth.login";
    public const string AuthRefresh = "auth.refresh";
    public const string BookingCreate = "booking.create";
    public const string BookingRead = "booking.read";
    public const string ServiceRequestCreate = "serviceRequest.create";
    public const string ServiceRequestRead = "serviceRequest.read";
    public const string ServiceRequestUpdate = "serviceRequest.update";
    public const string AssignmentManage = "assignment.manage";
    public const string TechnicianRead = "technician.read";
    public const string OperationsDashboardRead = "operationsDashboard.read";
    public const string DashboardRead = "dashboard.read";
    public const string AnalyticsRead = "analytics.read";
    public const string ReportRead = "report.read";
    public const string UserRead = "user.read";
    public const string UserCreate = "user.create";
    public const string UserUpdate = "user.update";
    public const string RoleRead = "role.read";
    public const string RoleCreate = "role.create";
    public const string RoleUpdate = "role.update";
    public const string PermissionRead = "permission.read";
    public const string LookupRead = "lookup.read";
    public const string LookupManage = "lookup.manage";
    public const string ConfigurationRead = "configuration.read";
    public const string ConfigurationManage = "configuration.manage";
    public const string CmsRead = "cms.read";
    public const string CmsManage = "cms.manage";
    public const string NotificationTemplateRead = "notificationTemplate.read";
    public const string NotificationTemplateManage = "notificationTemplate.manage";
    public const string NotificationTriggerRead = "notificationTrigger.read";
    public const string NotificationTriggerManage = "notificationTrigger.manage";
    public const string CommunicationPreferenceRead = "communicationPreference.read";
    public const string CommunicationPreferenceManage = "communicationPreference.manage";
    public const string HealthRead = "health.read";
    public const string QuotationRead = "quotation.read";
    public const string QuotationCreate = "quotation.create";
    public const string QuotationApprove = "quotation.approve";
    public const string InvoiceRead = "invoice.read";
    public const string InvoiceCreate = "invoice.create";
    public const string PaymentRead = "payment.read";
    public const string PaymentCollect = "payment.collect";
    public const string BillingRead = "billing.read";
    public const string AmcRead = "amc.read";
    public const string AmcCreate = "amc.create";
    public const string AmcAssign = "amc.assign";
    public const string WarrantyRead = "warranty.read";
    public const string WarrantyClaim = "warranty.claim";
    public const string RevisitRead = "revisit.read";
    public const string RevisitCreate = "revisit.create";
    public const string ServiceHistoryRead = "serviceHistory.read";
    public const string ItemRead = "item.read";
    public const string ItemCreate = "item.create";
    public const string WarehouseRead = "warehouse.read";
    public const string WarehouseCreate = "warehouse.create";
    public const string StockRead = "stock.read";
    public const string StockManage = "stock.manage";
    public const string JobConsumptionRead = "jobConsumption.read";
    public const string JobConsumptionCreate = "jobConsumption.create";
    public const string SupportRead = "support.read";
    public const string SupportManage = "support.manage";

    public static IReadOnlyCollection<string> All =>
    [
        AuthLogin,
        AuthRefresh,
        BookingCreate,
        BookingRead,
        ServiceRequestCreate,
        ServiceRequestRead,
        ServiceRequestUpdate,
        AssignmentManage,
        TechnicianRead,
        OperationsDashboardRead,
        DashboardRead,
        AnalyticsRead,
        ReportRead,
        UserRead,
        UserCreate,
        UserUpdate,
        RoleRead,
        RoleCreate,
        RoleUpdate,
        PermissionRead,
        LookupRead,
        LookupManage,
        ConfigurationRead,
        ConfigurationManage,
        CmsRead,
        CmsManage,
        NotificationTemplateRead,
        NotificationTemplateManage,
        NotificationTriggerRead,
        NotificationTriggerManage,
        CommunicationPreferenceRead,
        CommunicationPreferenceManage,
        HealthRead,
        QuotationRead,
        QuotationCreate,
        QuotationApprove,
        InvoiceRead,
        InvoiceCreate,
        PaymentRead,
        PaymentCollect,
        BillingRead,
        AmcRead,
        AmcCreate,
        AmcAssign,
        WarrantyRead,
        WarrantyClaim,
        RevisitRead,
        RevisitCreate,
        ServiceHistoryRead,
        ItemRead,
        ItemCreate,
        WarehouseRead,
        WarehouseCreate,
        StockRead,
        StockManage,
        JobConsumptionRead,
        JobConsumptionCreate,
        SupportRead,
        SupportManage
    ];
}
