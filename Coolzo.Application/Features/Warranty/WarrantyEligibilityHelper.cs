using Coolzo.Domain.Entities;

namespace Coolzo.Application.Features.Warranty;

internal static class WarrantyEligibilityHelper
{
    public static BookingLine? GetPrimaryBookingLine(InvoiceHeader invoiceHeader)
    {
        return invoiceHeader.QuotationHeader?.JobCard?.ServiceRequest?.Booking?.BookingLines
            .Where(line => !line.IsDeleted)
            .OrderBy(line => line.BookingLineId)
            .FirstOrDefault();
    }

    public static (DateTime CoverageStartDateUtc, DateTime CoverageEndDateUtc, bool IsEligible) EvaluateCoverage(
        InvoiceHeader invoiceHeader,
        WarrantyRule warrantyRule,
        DateTime currentDateUtc)
    {
        var coverageStartDateUtc = invoiceHeader.QuotationHeader?.JobCard?.WorkCompletedDateUtc ?? invoiceHeader.InvoiceDateUtc;
        var coverageEndDateUtc = coverageStartDateUtc.AddDays(warrantyRule.WarrantyDurationDays);
        var isEligible = currentDateUtc <= coverageEndDateUtc;

        return (coverageStartDateUtc, coverageEndDateUtc, isEligible);
    }

    public static string ResolveServiceName(InvoiceHeader invoiceHeader)
    {
        var booking = invoiceHeader.QuotationHeader?.JobCard?.ServiceRequest?.Booking;
        var primaryLine = GetPrimaryBookingLine(invoiceHeader);

        return primaryLine?.Service?.ServiceName ?? booking?.ServiceNameSnapshot ?? string.Empty;
    }
}
