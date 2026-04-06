namespace Coolzo.Contracts.Responses.Warranty;

public sealed record WarrantyClaimResponse(
    long WarrantyClaimId,
    long InvoiceId,
    string InvoiceNumber,
    long CustomerId,
    string CustomerName,
    long? WarrantyRuleId,
    string? WarrantyRuleName,
    DateTime CoverageStartDateUtc,
    DateTime CoverageEndDateUtc,
    bool IsEligible,
    string CurrentStatus,
    string ServiceName,
    string ClaimRemarks,
    DateTime ClaimDateUtc,
    long? RevisitRequestId);
