namespace Coolzo.Contracts.Responses.Billing;

public sealed record AccountsReceivableAgingBucketResponse(
    string Label,
    int Count,
    decimal Amount,
    string Color);
