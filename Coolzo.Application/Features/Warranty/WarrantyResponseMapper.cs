using Coolzo.Contracts.Responses.Warranty;
using Coolzo.Domain.Entities;

namespace Coolzo.Application.Features.Warranty;

internal static class WarrantyResponseMapper
{
    public static WarrantyClaimResponse ToClaim(WarrantyClaim warrantyClaim)
    {
        return new WarrantyClaimResponse(
            warrantyClaim.WarrantyClaimId,
            warrantyClaim.InvoiceHeaderId,
            warrantyClaim.InvoiceHeader?.InvoiceNumber ?? string.Empty,
            warrantyClaim.CustomerId,
            warrantyClaim.Customer?.CustomerName ?? string.Empty,
            warrantyClaim.WarrantyRuleId,
            warrantyClaim.WarrantyRule?.RuleName,
            warrantyClaim.CoverageStartDateUtc,
            warrantyClaim.CoverageEndDateUtc,
            warrantyClaim.IsEligible,
            warrantyClaim.CurrentStatus.ToString(),
            warrantyClaim.InvoiceHeader is null ? string.Empty : WarrantyEligibilityHelper.ResolveServiceName(warrantyClaim.InvoiceHeader),
            warrantyClaim.ClaimRemarks,
            warrantyClaim.ClaimDateUtc,
            warrantyClaim.RevisitRequest?.RevisitRequestId);
    }

    public static WarrantyStatusResponse ToStatus(
        InvoiceHeader invoiceHeader,
        WarrantyRule? warrantyRule,
        DateTime? coverageStartDateUtc,
        DateTime? coverageEndDateUtc,
        bool isEligible,
        string eligibilityMessage,
        IReadOnlyCollection<WarrantyClaim> claims)
    {
        return new WarrantyStatusResponse(
            invoiceHeader.InvoiceHeaderId,
            invoiceHeader.InvoiceNumber,
            invoiceHeader.CustomerId,
            invoiceHeader.Customer?.CustomerName ?? string.Empty,
            WarrantyEligibilityHelper.ResolveServiceName(invoiceHeader),
            warrantyRule is not null,
            isEligible,
            eligibilityMessage,
            coverageStartDateUtc,
            coverageEndDateUtc,
            warrantyRule?.RuleName,
            claims
                .Where(claim => !claim.IsDeleted)
                .OrderByDescending(claim => claim.ClaimDateUtc)
                .Select(ToClaim)
                .ToArray());
    }
}
