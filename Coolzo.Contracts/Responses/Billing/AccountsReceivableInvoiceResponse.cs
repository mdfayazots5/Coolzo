namespace Coolzo.Contracts.Responses.Billing;

public sealed record AccountsReceivableInvoiceResponse(
    string Id,
    string InvoiceNumber,
    string CustomerId,
    string CustomerName,
    string DueDate,
    decimal BalanceDue);
