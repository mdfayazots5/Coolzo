namespace Coolzo.Contracts.Requests.Revisit;

public sealed record RevisitRequestCreateRequest(
    long OriginalJobCardId,
    string RevisitType,
    DateTime? PreferredVisitDateUtc,
    string IssueSummary,
    string? RequestRemarks,
    long? CustomerAmcId,
    long? WarrantyClaimId,
    decimal? ChargeAmount);
