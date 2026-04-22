namespace Coolzo.Contracts.Responses.Customer;

public sealed record CustomerAdminListItemResponse(
    long CustomerId,
    string CustomerName,
    string MobileNumber,
    string EmailAddress,
    bool IsActive,
    string RiskLevel,
    int TotalServicesCount,
    decimal TotalRevenueAmount,
    decimal OutstandingAmount,
    bool HasActiveAmc,
    int OpenSupportTicketCount,
    DateTime CustomerSinceUtc,
    DateTime? LastServiceDateUtc,
    string? PrimaryAddressSummary);

public sealed record CustomerNoteResponse(
    string NoteId,
    string Author,
    string Content,
    DateTime TimestampUtc,
    bool IsPrivate,
    string NoteType);

public sealed record CustomerAdminDetailResponse(
    long CustomerId,
    string CustomerName,
    string MobileNumber,
    string EmailAddress,
    bool IsActive,
    string RiskLevel,
    int TotalServicesCount,
    decimal TotalRevenueAmount,
    decimal OutstandingAmount,
    bool HasActiveAmc,
    int OpenSupportTicketCount,
    int TotalSupportTicketCount,
    DateTime CustomerSinceUtc,
    DateTime? LastServiceDateUtc,
    DateTime? LastInvoiceDateUtc,
    string? LastInvoiceStatus,
    string? PrimaryAddressSummary,
    int ActiveAmcCount,
    string? ActiveAmcPlanName,
    string? ActiveAmcStatus,
    int? VisitsIncluded,
    int? VisitsUsed,
    DateOnly? NextAmcVisitDate,
    IReadOnlyCollection<CustomerAddressResponse> Addresses,
    IReadOnlyCollection<CustomerEquipmentResponse> Equipment,
    IReadOnlyCollection<CustomerNoteResponse> Notes);
