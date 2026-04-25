namespace Coolzo.Contracts.Responses.Billing;

public sealed record AccountsReceivableOutstandingCustomerResponse(
    string CustomerId,
    string CustomerName,
    string CustomerType,
    decimal OutstandingAmount,
    int OverdueInvoices);
