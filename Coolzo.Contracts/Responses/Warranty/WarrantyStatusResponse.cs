namespace Coolzo.Contracts.Responses.Warranty;

public sealed record WarrantyStatusResponse(
    long InvoiceId,
    string InvoiceNumber,
    long CustomerId,
    string CustomerName,
    string ServiceName,
    bool IsWarrantyAvailable,
    bool IsEligible,
    string EligibilityMessage,
    DateTime? CoverageStartDateUtc,
    DateTime? CoverageEndDateUtc,
    string? WarrantyRuleName,
    IReadOnlyCollection<WarrantyClaimResponse> Claims);
