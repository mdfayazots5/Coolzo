using Coolzo.Domain.Enums;

namespace Coolzo.Application.Common.Models;

public sealed record CustomerManagementListItemView(
    long CustomerId,
    string CustomerName,
    string MobileNumber,
    string EmailAddress,
    bool IsGuestCustomer,
    bool IsActive,
    DateTime CustomerSinceUtc,
    int TotalServicesCount,
    decimal TotalRevenueAmount,
    decimal OutstandingAmount,
    bool HasActiveAmc,
    int OpenSupportTicketCount,
    DateTime? LastServiceDateUtc,
    DateTime? LastInvoiceDateUtc,
    InvoicePaymentStatus? LastInvoiceStatus,
    string? PrimaryAddressSummary);

public sealed record CustomerManagementDetailView(
    long CustomerId,
    string CustomerName,
    string MobileNumber,
    string EmailAddress,
    bool IsGuestCustomer,
    bool IsActive,
    DateTime CustomerSinceUtc,
    int TotalServicesCount,
    decimal TotalRevenueAmount,
    decimal OutstandingAmount,
    bool HasActiveAmc,
    int OpenSupportTicketCount,
    int TotalSupportTicketCount,
    DateTime? LastServiceDateUtc,
    DateTime? LastInvoiceDateUtc,
    InvoicePaymentStatus? LastInvoiceStatus,
    string? PrimaryAddressSummary,
    int ActiveAmcCount,
    string? ActiveAmcPlanName,
    AmcSubscriptionStatus? ActiveAmcStatus,
    int? VisitsIncluded,
    int? VisitsUsed,
    DateOnly? NextAmcVisitDate);
