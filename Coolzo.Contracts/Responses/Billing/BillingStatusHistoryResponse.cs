namespace Coolzo.Contracts.Responses.Billing;

public sealed record BillingStatusHistoryResponse(
    long BillingStatusHistoryId,
    string EntityType,
    string StatusName,
    string Remarks,
    DateTime StatusDateUtc,
    string ChangedBy);
