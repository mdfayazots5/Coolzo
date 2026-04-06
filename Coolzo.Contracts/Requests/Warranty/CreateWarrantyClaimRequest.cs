namespace Coolzo.Contracts.Requests.Warranty;

public sealed record CreateWarrantyClaimRequest(
    long InvoiceId,
    string? ClaimRemarks);
