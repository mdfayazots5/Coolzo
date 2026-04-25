namespace Coolzo.Contracts.Responses.Billing;

public sealed record AccountsReceivableDashboardResponse(
    IReadOnlyCollection<AccountsReceivableAgingBucketResponse> Aging,
    IReadOnlyCollection<AccountsReceivableInvoiceResponse> OverdueInvoices,
    IReadOnlyCollection<AccountsReceivableOutstandingCustomerResponse> TopOutstandingCustomers,
    decimal TotalOutstanding);
