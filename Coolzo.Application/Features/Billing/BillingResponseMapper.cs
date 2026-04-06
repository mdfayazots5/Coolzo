using Coolzo.Contracts.Responses.Billing;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using DomainBooking = Coolzo.Domain.Entities.Booking;

namespace Coolzo.Application.Features.Billing;

internal static class BillingResponseMapper
{
    public static QuotationListItemResponse ToQuotationListItem(QuotationHeader quotationHeader)
    {
        var serviceRequest = quotationHeader.JobCard?.ServiceRequest;

        return new QuotationListItemResponse(
            quotationHeader.QuotationHeaderId,
            quotationHeader.QuotationNumber,
            quotationHeader.JobCardId,
            quotationHeader.JobCard?.JobCardNumber ?? string.Empty,
            serviceRequest?.ServiceRequestId ?? 0,
            serviceRequest?.ServiceRequestNumber ?? string.Empty,
            serviceRequest?.Booking?.CustomerNameSnapshot ?? quotationHeader.Customer?.CustomerName ?? string.Empty,
            quotationHeader.CurrentStatus.ToString(),
            quotationHeader.GrandTotalAmount,
            quotationHeader.QuotationDateUtc);
    }

    public static QuotationDetailResponse ToQuotationDetail(QuotationHeader quotationHeader)
    {
        var serviceRequest = quotationHeader.JobCard?.ServiceRequest;
        var booking = serviceRequest?.Booking;

        return new QuotationDetailResponse(
            quotationHeader.QuotationHeaderId,
            quotationHeader.QuotationNumber,
            quotationHeader.JobCardId,
            quotationHeader.JobCard?.JobCardNumber ?? string.Empty,
            serviceRequest?.ServiceRequestId ?? 0,
            serviceRequest?.ServiceRequestNumber ?? string.Empty,
            booking?.BookingId ?? 0,
            booking?.BookingReference ?? string.Empty,
            quotationHeader.CustomerId,
            booking?.CustomerNameSnapshot ?? quotationHeader.Customer?.CustomerName ?? string.Empty,
            booking?.MobileNumberSnapshot ?? quotationHeader.Customer?.MobileNumber ?? string.Empty,
            BuildAddressSummary(booking),
            ResolveServiceName(booking),
            quotationHeader.CurrentStatus.ToString(),
            quotationHeader.QuotationDateUtc,
            quotationHeader.SubTotalAmount,
            quotationHeader.DiscountAmount,
            quotationHeader.TaxPercentage,
            quotationHeader.TaxAmount,
            quotationHeader.GrandTotalAmount,
            quotationHeader.CustomerDecisionRemarks,
            quotationHeader.ApprovedDateUtc,
            quotationHeader.RejectedDateUtc,
            quotationHeader.InvoiceHeader?.InvoiceHeaderId,
            quotationHeader.InvoiceHeader?.InvoiceNumber,
            quotationHeader.InvoiceHeader?.CurrentStatus.ToString(),
            quotationHeader.Lines
                .Where(line => !line.IsDeleted)
                .OrderBy(line => line.QuotationLineId)
                .Select(ToQuotationLineResponse)
                .ToArray(),
            quotationHeader.BillingStatusHistories
                .Where(history => !history.IsDeleted)
                .OrderBy(history => history.StatusDateUtc)
                .ThenBy(history => history.BillingStatusHistoryId)
                .Select(ToBillingStatusHistoryResponse)
                .ToArray());
    }

    public static InvoiceListItemResponse ToInvoiceListItem(InvoiceHeader invoiceHeader)
    {
        return new InvoiceListItemResponse(
            invoiceHeader.InvoiceHeaderId,
            invoiceHeader.InvoiceNumber,
            invoiceHeader.QuotationHeaderId,
            invoiceHeader.QuotationHeader?.QuotationNumber ?? string.Empty,
            invoiceHeader.QuotationHeader?.JobCard?.ServiceRequest?.Booking?.CustomerNameSnapshot ??
                invoiceHeader.Customer?.CustomerName ??
                string.Empty,
            invoiceHeader.CurrentStatus.ToString(),
            invoiceHeader.GrandTotalAmount,
            invoiceHeader.PaidAmount,
            invoiceHeader.BalanceAmount,
            invoiceHeader.InvoiceDateUtc);
    }

    public static InvoiceDetailResponse ToInvoiceDetail(
        InvoiceHeader invoiceHeader,
        IReadOnlyCollection<BillingStatusHistory>? billingHistory = null)
    {
        var quotationHeader = invoiceHeader.QuotationHeader;
        var booking = quotationHeader?.JobCard?.ServiceRequest?.Booking;

        return new InvoiceDetailResponse(
            invoiceHeader.InvoiceHeaderId,
            invoiceHeader.InvoiceNumber,
            invoiceHeader.QuotationHeaderId,
            quotationHeader?.QuotationNumber ?? string.Empty,
            invoiceHeader.CustomerId,
            booking?.CustomerNameSnapshot ?? invoiceHeader.Customer?.CustomerName ?? string.Empty,
            booking?.MobileNumberSnapshot ?? invoiceHeader.Customer?.MobileNumber ?? string.Empty,
            BuildAddressSummary(booking),
            ResolveServiceName(booking),
            invoiceHeader.CurrentStatus.ToString(),
            invoiceHeader.InvoiceDateUtc,
            invoiceHeader.SubTotalAmount,
            invoiceHeader.DiscountAmount,
            invoiceHeader.TaxPercentage,
            invoiceHeader.TaxAmount,
            invoiceHeader.GrandTotalAmount,
            invoiceHeader.PaidAmount,
            invoiceHeader.BalanceAmount,
            invoiceHeader.LastPaymentDateUtc,
            invoiceHeader.Lines
                .Where(line => !line.IsDeleted)
                .OrderBy(line => line.InvoiceLineId)
                .Select(ToInvoiceLineResponse)
                .ToArray(),
            invoiceHeader.PaymentTransactions
                .Where(payment => !payment.IsDeleted)
                .OrderByDescending(payment => payment.PaymentDateUtc)
                .ThenByDescending(payment => payment.PaymentTransactionId)
                .Select(ToPaymentTransactionResponse)
                .ToArray(),
            (billingHistory ?? invoiceHeader.BillingStatusHistories.ToArray())
                .Where(history => !history.IsDeleted)
                .OrderBy(history => history.StatusDateUtc)
                .ThenBy(history => history.BillingStatusHistoryId)
                .Select(ToBillingStatusHistoryResponse)
                .ToArray());
    }

    public static PaymentTransactionResponse ToPaymentTransactionResponse(PaymentTransaction paymentTransaction)
    {
        return new PaymentTransactionResponse(
            paymentTransaction.PaymentTransactionId,
            paymentTransaction.InvoiceHeaderId,
            paymentTransaction.PaymentMethod.ToString(),
            paymentTransaction.ReferenceNumber,
            paymentTransaction.PaidAmount,
            paymentTransaction.PaymentDateUtc,
            paymentTransaction.TransactionRemarks,
            paymentTransaction.PaymentReceipt is null || paymentTransaction.PaymentReceipt.IsDeleted
                ? null
                : ToPaymentReceiptResponse(paymentTransaction.PaymentReceipt));
    }

    public static PaymentReceiptResponse ToPaymentReceiptResponse(PaymentReceipt paymentReceipt)
    {
        return new PaymentReceiptResponse(
            paymentReceipt.PaymentReceiptId,
            paymentReceipt.ReceiptNumber,
            paymentReceipt.InvoiceHeaderId,
            paymentReceipt.PaymentTransactionId,
            paymentReceipt.ReceiptDateUtc,
            paymentReceipt.ReceivedAmount,
            paymentReceipt.BalanceAmount,
            paymentReceipt.ReceiptRemarks);
    }

    public static BillingStatusResponse ToBillingStatusResponse(
        InvoiceHeader invoiceHeader,
        IReadOnlyCollection<BillingStatusHistory> billingHistory)
    {
        return new BillingStatusResponse(
            invoiceHeader.InvoiceHeaderId,
            invoiceHeader.InvoiceNumber,
            invoiceHeader.CurrentStatus.ToString(),
            invoiceHeader.GrandTotalAmount,
            invoiceHeader.PaidAmount,
            invoiceHeader.BalanceAmount,
            billingHistory
                .Where(history => !history.IsDeleted)
                .OrderBy(history => history.StatusDateUtc)
                .ThenBy(history => history.BillingStatusHistoryId)
                .Select(ToBillingStatusHistoryResponse)
                .ToArray());
    }

    private static QuotationLineResponse ToQuotationLineResponse(QuotationLine quotationLine)
    {
        return new QuotationLineResponse(
            quotationLine.QuotationLineId,
            quotationLine.LineType.ToString(),
            quotationLine.LineDescription,
            quotationLine.Quantity,
            quotationLine.UnitPrice,
            quotationLine.LineAmount);
    }

    private static InvoiceLineResponse ToInvoiceLineResponse(InvoiceLine invoiceLine)
    {
        return new InvoiceLineResponse(
            invoiceLine.InvoiceLineId,
            invoiceLine.QuotationLineId,
            invoiceLine.LineType.ToString(),
            invoiceLine.LineDescription,
            invoiceLine.Quantity,
            invoiceLine.UnitPrice,
            invoiceLine.LineAmount);
    }

    private static BillingStatusHistoryResponse ToBillingStatusHistoryResponse(BillingStatusHistory billingStatusHistory)
    {
        return new BillingStatusHistoryResponse(
            billingStatusHistory.BillingStatusHistoryId,
            billingStatusHistory.EntityType.ToString(),
            billingStatusHistory.StatusName,
            billingStatusHistory.Remarks,
            billingStatusHistory.StatusDateUtc,
            billingStatusHistory.CreatedBy);
    }

    private static string ResolveServiceName(DomainBooking? booking)
    {
        return booking?.BookingLines
            .Where(line => !line.IsDeleted)
            .OrderBy(line => line.BookingLineId)
            .Select(line => line.Service?.ServiceName ?? booking.ServiceNameSnapshot)
            .FirstOrDefault() ?? booking?.ServiceNameSnapshot ?? string.Empty;
    }

    private static string BuildAddressSummary(DomainBooking? booking)
    {
        if (booking is null)
        {
            return string.Empty;
        }

        var parts = new[]
        {
            booking.AddressLine1Snapshot,
            booking.AddressLine2Snapshot,
            booking.LandmarkSnapshot,
            booking.CityNameSnapshot,
            booking.PincodeSnapshot
        }
        .Where(value => !string.IsNullOrWhiteSpace(value));

        return string.Join(", ", parts);
    }
}
