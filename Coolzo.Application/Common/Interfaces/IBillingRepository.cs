using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;

namespace Coolzo.Application.Common.Interfaces;

public interface IBillingRepository
{
    Task<JobCard?> GetJobCardByIdAsync(long jobCardId, CancellationToken cancellationToken);

    Task<JobCard?> GetJobCardByIdForUpdateAsync(long jobCardId, CancellationToken cancellationToken);

    Task AddQuotationAsync(QuotationHeader quotationHeader, CancellationToken cancellationToken);

    Task AddQuotationLineAsync(QuotationLine quotationLine, CancellationToken cancellationToken);

    Task<bool> QuotationNumberExistsAsync(string quotationNumber, CancellationToken cancellationToken);

    Task<QuotationHeader?> GetQuotationByIdAsync(long quotationHeaderId, CancellationToken cancellationToken);

    Task<QuotationHeader?> GetQuotationByIdForUpdateAsync(long quotationHeaderId, CancellationToken cancellationToken);

    Task<QuotationHeader?> GetQuotationByJobCardIdAsync(long jobCardId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<QuotationHeader>> SearchQuotationsAsync(
        QuotationStatus? status,
        long? customerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountQuotationsAsync(QuotationStatus? status, long? customerId, CancellationToken cancellationToken);

    Task AddInvoiceAsync(InvoiceHeader invoiceHeader, CancellationToken cancellationToken);

    Task AddInvoiceLineAsync(InvoiceLine invoiceLine, CancellationToken cancellationToken);

    Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, CancellationToken cancellationToken);

    Task<InvoiceHeader?> GetInvoiceByIdAsync(long invoiceHeaderId, CancellationToken cancellationToken);

    Task<InvoiceHeader?> GetInvoiceByIdForUpdateAsync(long invoiceHeaderId, CancellationToken cancellationToken);

    Task<InvoiceHeader?> GetInvoiceByQuotationIdAsync(long quotationHeaderId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<InvoiceHeader>> SearchInvoicesAsync(
        InvoicePaymentStatus? status,
        long? customerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountInvoicesAsync(InvoicePaymentStatus? status, long? customerId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<InvoiceHeader>> ListAccountsReceivableInvoicesAsync(CancellationToken cancellationToken);

    Task AddPaymentTransactionAsync(PaymentTransaction paymentTransaction, CancellationToken cancellationToken);

    Task AddPaymentReceiptAsync(PaymentReceipt paymentReceipt, CancellationToken cancellationToken);

    Task<bool> ReceiptNumberExistsAsync(string receiptNumber, CancellationToken cancellationToken);

    Task<PaymentTransaction?> GetPaymentByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken);

    Task<bool> PaymentReferenceExistsAsync(long invoiceHeaderId, string referenceNumber, CancellationToken cancellationToken);

    Task<bool> GatewayTransactionExistsAsync(string gatewayTransactionId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PaymentTransaction>> GetPaymentTransactionsByInvoiceIdAsync(long invoiceHeaderId, CancellationToken cancellationToken);

    Task AddBillingStatusHistoryAsync(BillingStatusHistory billingStatusHistory, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<BillingStatusHistory>> GetBillingHistoryByInvoiceIdAsync(long invoiceHeaderId, CancellationToken cancellationToken);
}
