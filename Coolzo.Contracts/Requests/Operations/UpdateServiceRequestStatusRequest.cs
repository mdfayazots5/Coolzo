namespace Coolzo.Contracts.Requests.Operations;

public sealed record UpdateServiceRequestStatusRequest(
    string Status,
    string? Remarks);
