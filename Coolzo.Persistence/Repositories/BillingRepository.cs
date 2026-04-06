using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class BillingRepository : IBillingRepository
{
    private readonly CoolzoDbContext _dbContext;

    public BillingRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<JobCard?> GetJobCardByIdAsync(long jobCardId, CancellationToken cancellationToken)
    {
        return BuildJobCardQuery(true)
            .FirstOrDefaultAsync(entity => entity.JobCardId == jobCardId, cancellationToken);
    }

    public Task<JobCard?> GetJobCardByIdForUpdateAsync(long jobCardId, CancellationToken cancellationToken)
    {
        return BuildJobCardQuery(false)
            .FirstOrDefaultAsync(entity => entity.JobCardId == jobCardId, cancellationToken);
    }

    public Task AddQuotationAsync(QuotationHeader quotationHeader, CancellationToken cancellationToken)
    {
        return _dbContext.QuotationHeaders.AddAsync(quotationHeader, cancellationToken).AsTask();
    }

    public Task AddQuotationLineAsync(QuotationLine quotationLine, CancellationToken cancellationToken)
    {
        return _dbContext.QuotationLines.AddAsync(quotationLine, cancellationToken).AsTask();
    }

    public Task<bool> QuotationNumberExistsAsync(string quotationNumber, CancellationToken cancellationToken)
    {
        return _dbContext.QuotationHeaders.AnyAsync(
            entity => entity.QuotationNumber == quotationNumber && !entity.IsDeleted,
            cancellationToken);
    }

    public Task<QuotationHeader?> GetQuotationByIdAsync(long quotationHeaderId, CancellationToken cancellationToken)
    {
        return BuildQuotationQuery(true)
            .FirstOrDefaultAsync(entity => entity.QuotationHeaderId == quotationHeaderId, cancellationToken);
    }

    public Task<QuotationHeader?> GetQuotationByIdForUpdateAsync(long quotationHeaderId, CancellationToken cancellationToken)
    {
        return BuildQuotationQuery(false)
            .FirstOrDefaultAsync(entity => entity.QuotationHeaderId == quotationHeaderId, cancellationToken);
    }

    public Task<QuotationHeader?> GetQuotationByJobCardIdAsync(long jobCardId, CancellationToken cancellationToken)
    {
        return BuildQuotationQuery(true)
            .FirstOrDefaultAsync(entity => entity.JobCardId == jobCardId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<QuotationHeader>> SearchQuotationsAsync(
        QuotationStatus? status,
        long? customerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;

        return await ApplyQuotationFilters(status, customerId)
            .OrderByDescending(entity => entity.QuotationDateUtc)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountQuotationsAsync(QuotationStatus? status, long? customerId, CancellationToken cancellationToken)
    {
        return ApplyQuotationFilters(status, customerId).CountAsync(cancellationToken);
    }

    public Task AddInvoiceAsync(InvoiceHeader invoiceHeader, CancellationToken cancellationToken)
    {
        return _dbContext.InvoiceHeaders.AddAsync(invoiceHeader, cancellationToken).AsTask();
    }

    public Task AddInvoiceLineAsync(InvoiceLine invoiceLine, CancellationToken cancellationToken)
    {
        return _dbContext.InvoiceLines.AddAsync(invoiceLine, cancellationToken).AsTask();
    }

    public Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, CancellationToken cancellationToken)
    {
        return _dbContext.InvoiceHeaders.AnyAsync(
            entity => entity.InvoiceNumber == invoiceNumber && !entity.IsDeleted,
            cancellationToken);
    }

    public Task<InvoiceHeader?> GetInvoiceByIdAsync(long invoiceHeaderId, CancellationToken cancellationToken)
    {
        return BuildInvoiceQuery(true)
            .FirstOrDefaultAsync(entity => entity.InvoiceHeaderId == invoiceHeaderId, cancellationToken);
    }

    public Task<InvoiceHeader?> GetInvoiceByIdForUpdateAsync(long invoiceHeaderId, CancellationToken cancellationToken)
    {
        return BuildInvoiceQuery(false)
            .FirstOrDefaultAsync(entity => entity.InvoiceHeaderId == invoiceHeaderId, cancellationToken);
    }

    public Task<InvoiceHeader?> GetInvoiceByQuotationIdAsync(long quotationHeaderId, CancellationToken cancellationToken)
    {
        return BuildInvoiceQuery(true)
            .FirstOrDefaultAsync(entity => entity.QuotationHeaderId == quotationHeaderId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<InvoiceHeader>> SearchInvoicesAsync(
        InvoicePaymentStatus? status,
        long? customerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;

        return await ApplyInvoiceFilters(status, customerId)
            .OrderByDescending(entity => entity.InvoiceDateUtc)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountInvoicesAsync(InvoicePaymentStatus? status, long? customerId, CancellationToken cancellationToken)
    {
        return ApplyInvoiceFilters(status, customerId).CountAsync(cancellationToken);
    }

    public Task AddPaymentTransactionAsync(PaymentTransaction paymentTransaction, CancellationToken cancellationToken)
    {
        return _dbContext.PaymentTransactions.AddAsync(paymentTransaction, cancellationToken).AsTask();
    }

    public Task AddPaymentReceiptAsync(PaymentReceipt paymentReceipt, CancellationToken cancellationToken)
    {
        return _dbContext.PaymentReceipts.AddAsync(paymentReceipt, cancellationToken).AsTask();
    }

    public Task<bool> ReceiptNumberExistsAsync(string receiptNumber, CancellationToken cancellationToken)
    {
        return _dbContext.PaymentReceipts.AnyAsync(
            entity => entity.ReceiptNumber == receiptNumber && !entity.IsDeleted,
            cancellationToken);
    }

    public Task<PaymentTransaction?> GetPaymentByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        return _dbContext.PaymentTransactions
            .AsNoTracking()
            .Include(entity => entity.PaymentReceipt)
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.IdempotencyKey == idempotencyKey,
                cancellationToken);
    }

    public Task<bool> PaymentReferenceExistsAsync(long invoiceHeaderId, string referenceNumber, CancellationToken cancellationToken)
    {
        return _dbContext.PaymentTransactions.AnyAsync(
            entity =>
                !entity.IsDeleted &&
                entity.InvoiceHeaderId == invoiceHeaderId &&
                entity.ReferenceNumber == referenceNumber,
            cancellationToken);
    }

    public Task<bool> GatewayTransactionExistsAsync(string gatewayTransactionId, CancellationToken cancellationToken)
    {
        return _dbContext.PaymentTransactions.AnyAsync(
            entity => !entity.IsDeleted && entity.GatewayTransactionId == gatewayTransactionId,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<PaymentTransaction>> GetPaymentTransactionsByInvoiceIdAsync(
        long invoiceHeaderId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.PaymentTransactions
            .AsNoTracking()
            .Include(entity => entity.PaymentReceipt)
            .Where(entity => entity.InvoiceHeaderId == invoiceHeaderId && !entity.IsDeleted)
            .OrderByDescending(entity => entity.PaymentDateUtc)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddBillingStatusHistoryAsync(BillingStatusHistory billingStatusHistory, CancellationToken cancellationToken)
    {
        return _dbContext.BillingStatusHistories.AddAsync(billingStatusHistory, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<BillingStatusHistory>> GetBillingHistoryByInvoiceIdAsync(
        long invoiceHeaderId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.BillingStatusHistories
            .AsNoTracking()
            .Include(entity => entity.QuotationHeader)
                .ThenInclude(quotation => quotation!.InvoiceHeader)
            .Include(entity => entity.PaymentTransaction)
            .Where(entity =>
                !entity.IsDeleted &&
                (
                    entity.InvoiceHeaderId == invoiceHeaderId ||
                    (entity.QuotationHeader != null && entity.QuotationHeader.InvoiceHeader != null &&
                        entity.QuotationHeader.InvoiceHeader.InvoiceHeaderId == invoiceHeaderId) ||
                    (entity.PaymentTransaction != null && entity.PaymentTransaction.InvoiceHeaderId == invoiceHeaderId)
                ))
            .OrderBy(entity => entity.StatusDateUtc)
            .ToArrayAsync(cancellationToken);
    }

    private IQueryable<QuotationHeader> ApplyQuotationFilters(QuotationStatus? status, long? customerId)
    {
        var query = BuildQuotationQuery(true);

        if (status.HasValue)
        {
            query = query.Where(entity => entity.CurrentStatus == status.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(entity => entity.CustomerId == customerId.Value);
        }

        return query;
    }

    private IQueryable<InvoiceHeader> ApplyInvoiceFilters(InvoicePaymentStatus? status, long? customerId)
    {
        var query = BuildInvoiceQuery(true);

        if (status.HasValue)
        {
            query = query.Where(entity => entity.CurrentStatus == status.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(entity => entity.CustomerId == customerId.Value);
        }

        return query;
    }

    private IQueryable<JobCard> BuildJobCardQuery(bool asNoTracking)
    {
        IQueryable<JobCard> query = _dbContext.JobCards
            .AsSplitQuery()
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.Booking)
                    .ThenInclude(booking => booking!.BookingLines)
                        .ThenInclude(line => line.Service)
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.Assignments)
                    .ThenInclude(assignment => assignment.Technician)
            .Where(entity => !entity.IsDeleted);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    private IQueryable<QuotationHeader> BuildQuotationQuery(bool asNoTracking)
    {
        IQueryable<QuotationHeader> query = _dbContext.QuotationHeaders
            .AsSplitQuery()
            .Include(entity => entity.Customer)
            .Include(entity => entity.Lines)
            .Include(entity => entity.InvoiceHeader)
            .Include(entity => entity.BillingStatusHistories)
            .Include(entity => entity.JobCard)
                .ThenInclude(jobCard => jobCard!.ServiceRequest)
                    .ThenInclude(serviceRequest => serviceRequest!.Booking)
                        .ThenInclude(booking => booking!.BookingLines)
                            .ThenInclude(line => line.Service)
            .Include(entity => entity.JobCard)
                .ThenInclude(jobCard => jobCard!.ServiceRequest)
                    .ThenInclude(serviceRequest => serviceRequest!.Booking)
                        .ThenInclude(booking => booking!.SlotAvailability)
                            .ThenInclude(slotAvailability => slotAvailability!.SlotConfiguration)
            .Include(entity => entity.JobCard)
                .ThenInclude(jobCard => jobCard!.ServiceRequest)
                    .ThenInclude(serviceRequest => serviceRequest!.Assignments)
                        .ThenInclude(assignment => assignment.Technician)
            .Where(entity => !entity.IsDeleted);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    private IQueryable<InvoiceHeader> BuildInvoiceQuery(bool asNoTracking)
    {
        IQueryable<InvoiceHeader> query = _dbContext.InvoiceHeaders
            .AsSplitQuery()
            .Include(entity => entity.Customer)
            .Include(entity => entity.Lines)
            .Include(entity => entity.PaymentTransactions)
                .ThenInclude(transaction => transaction.PaymentReceipt)
            .Include(entity => entity.BillingStatusHistories)
            .Include(entity => entity.QuotationHeader)
                .ThenInclude(quotation => quotation!.JobCard)
                    .ThenInclude(jobCard => jobCard!.ServiceRequest)
                        .ThenInclude(serviceRequest => serviceRequest!.Booking)
                            .ThenInclude(booking => booking!.BookingLines)
                                .ThenInclude(line => line.Service)
            .Where(entity => !entity.IsDeleted);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }
}
