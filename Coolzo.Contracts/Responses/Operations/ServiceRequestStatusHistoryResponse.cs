namespace Coolzo.Contracts.Responses.Operations;

public sealed record ServiceRequestStatusHistoryResponse(
    string Status,
    string Remarks,
    DateTime StatusDateUtc);
