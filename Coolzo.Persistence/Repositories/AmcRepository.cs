using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class AmcRepository : IAmcRepository
{
    private readonly CoolzoDbContext _dbContext;

    public AmcRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAmcPlanAsync(AmcPlan amcPlan, CancellationToken cancellationToken)
    {
        return _dbContext.AmcPlans.AddAsync(amcPlan, cancellationToken).AsTask();
    }

    public Task<bool> AmcPlanNameExistsAsync(string planName, CancellationToken cancellationToken)
    {
        return _dbContext.AmcPlans.AnyAsync(
            entity => entity.PlanName == planName && !entity.IsDeleted,
            cancellationToken);
    }

    public Task<AmcPlan?> GetAmcPlanByIdAsync(long amcPlanId, CancellationToken cancellationToken)
    {
        return _dbContext.AmcPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.AmcPlanId == amcPlanId && !entity.IsDeleted, cancellationToken);
    }

    public Task<AmcPlan?> GetAmcPlanByIdForUpdateAsync(long amcPlanId, CancellationToken cancellationToken)
    {
        return _dbContext.AmcPlans
            .FirstOrDefaultAsync(entity => entity.AmcPlanId == amcPlanId && !entity.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyCollection<AmcPlan>> SearchAmcPlansAsync(
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;
        var query = _dbContext.AmcPlans.AsNoTracking().Where(entity => !entity.IsDeleted);

        if (isActive.HasValue)
        {
            query = query.Where(entity => entity.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(entity => entity.PlanName)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountAmcPlansAsync(bool? isActive, CancellationToken cancellationToken)
    {
        var query = _dbContext.AmcPlans.Where(entity => !entity.IsDeleted);

        if (isActive.HasValue)
        {
            query = query.Where(entity => entity.IsActive == isActive.Value);
        }

        return query.CountAsync(cancellationToken);
    }

    public Task<Customer?> GetCustomerByIdAsync(long customerId, CancellationToken cancellationToken)
    {
        return _dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.CustomerId == customerId && !entity.IsDeleted, cancellationToken);
    }

    public Task<JobCard?> GetJobCardByIdAsync(long jobCardId, CancellationToken cancellationToken)
    {
        return BuildJobCardQuery(asNoTracking: true)
            .FirstOrDefaultAsync(entity => entity.JobCardId == jobCardId, cancellationToken);
    }

    public Task<InvoiceHeader?> GetInvoiceByIdAsync(long invoiceId, CancellationToken cancellationToken)
    {
        return BuildInvoiceQuery(asNoTracking: true)
            .FirstOrDefaultAsync(entity => entity.InvoiceHeaderId == invoiceId, cancellationToken);
    }

    public Task AddCustomerAmcAsync(CustomerAmc customerAmc, CancellationToken cancellationToken)
    {
        return _dbContext.CustomerAmcs.AddAsync(customerAmc, cancellationToken).AsTask();
    }

    public Task<CustomerAmc?> GetCustomerAmcByIdAsync(long customerAmcId, CancellationToken cancellationToken)
    {
        return BuildCustomerAmcQuery(asNoTracking: true)
            .FirstOrDefaultAsync(entity => entity.CustomerAmcId == customerAmcId, cancellationToken);
    }

    public Task<CustomerAmc?> GetCustomerAmcByIdForUpdateAsync(long customerAmcId, CancellationToken cancellationToken)
    {
        return BuildCustomerAmcQuery(asNoTracking: false)
            .FirstOrDefaultAsync(entity => entity.CustomerAmcId == customerAmcId, cancellationToken);
    }

    public Task<bool> HasActiveCustomerAmcAsync(
        long customerId,
        long amcPlanId,
        DateTime coverageDateUtc,
        CancellationToken cancellationToken)
    {
        return _dbContext.CustomerAmcs.AnyAsync(
            entity =>
                entity.CustomerId == customerId &&
                entity.AmcPlanId == amcPlanId &&
                !entity.IsDeleted &&
                entity.StartDateUtc <= coverageDateUtc &&
                entity.EndDateUtc >= coverageDateUtc &&
                entity.CurrentStatus == Domain.Enums.AmcSubscriptionStatus.Active,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<CustomerAmc>> GetCustomerAmcByCustomerIdAsync(long customerId, CancellationToken cancellationToken)
    {
        return await BuildCustomerAmcQuery(asNoTracking: true)
            .Where(entity => entity.CustomerId == customerId)
            .OrderByDescending(entity => entity.StartDateUtc)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddAmcVisitScheduleAsync(AmcVisitSchedule amcVisitSchedule, CancellationToken cancellationToken)
    {
        return _dbContext.AmcVisitSchedules.AddAsync(amcVisitSchedule, cancellationToken).AsTask();
    }

    public Task<bool> HasAmcVisitSchedulesAsync(long customerAmcId, CancellationToken cancellationToken)
    {
        return _dbContext.AmcVisitSchedules.AnyAsync(
            entity => entity.CustomerAmcId == customerAmcId && !entity.IsDeleted,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<AmcVisitSchedule>> GetAmcVisitSchedulesByCustomerAmcIdAsync(
        long customerAmcId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.AmcVisitSchedules
            .AsNoTracking()
            .Include(entity => entity.ServiceRequest)
            .Where(entity => entity.CustomerAmcId == customerAmcId && !entity.IsDeleted)
            .OrderBy(entity => entity.VisitNumber)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<WarrantyRule?> GetMatchingWarrantyRuleAsync(
        long serviceId,
        long? acTypeId,
        long? brandId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.WarrantyRules
            .AsNoTracking()
            .Where(entity =>
                !entity.IsDeleted &&
                entity.IsActive &&
                (!entity.ServiceId.HasValue || entity.ServiceId == serviceId) &&
                (!entity.AcTypeId.HasValue || entity.AcTypeId == acTypeId) &&
                (!entity.BrandId.HasValue || entity.BrandId == brandId))
            .OrderByDescending(entity => entity.BrandId.HasValue ? 1 : 0)
            .ThenByDescending(entity => entity.AcTypeId.HasValue ? 1 : 0)
            .ThenByDescending(entity => entity.ServiceId.HasValue ? 1 : 0)
            .ThenBy(entity => entity.WarrantyRuleId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<WarrantyClaim>> GetWarrantyClaimsByInvoiceIdAsync(long invoiceId, CancellationToken cancellationToken)
    {
        return await BuildWarrantyClaimQuery(asNoTracking: true)
            .Where(entity => entity.InvoiceHeaderId == invoiceId)
            .OrderByDescending(entity => entity.ClaimDateUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<WarrantyClaim>> GetWarrantyClaimsByCustomerIdAsync(long customerId, CancellationToken cancellationToken)
    {
        return await BuildWarrantyClaimQuery(asNoTracking: true)
            .Where(entity => entity.CustomerId == customerId)
            .OrderByDescending(entity => entity.ClaimDateUtc)
            .ToArrayAsync(cancellationToken);
    }

    public Task<bool> HasOpenWarrantyClaimAsync(long invoiceId, CancellationToken cancellationToken)
    {
        return _dbContext.WarrantyClaims.AnyAsync(
            entity =>
                entity.InvoiceHeaderId == invoiceId &&
                !entity.IsDeleted &&
                entity.CurrentStatus != Domain.Enums.WarrantyClaimStatus.Rejected &&
                entity.CurrentStatus != Domain.Enums.WarrantyClaimStatus.Closed,
            cancellationToken);
    }

    public Task AddWarrantyClaimAsync(WarrantyClaim warrantyClaim, CancellationToken cancellationToken)
    {
        return _dbContext.WarrantyClaims.AddAsync(warrantyClaim, cancellationToken).AsTask();
    }

    public Task<WarrantyClaim?> GetWarrantyClaimByIdAsync(long warrantyClaimId, CancellationToken cancellationToken)
    {
        return BuildWarrantyClaimQuery(asNoTracking: true)
            .FirstOrDefaultAsync(entity => entity.WarrantyClaimId == warrantyClaimId, cancellationToken);
    }

    public Task<Booking?> GetBookingByIdAsync(long bookingId, CancellationToken cancellationToken)
    {
        return BuildBookingQuery(asNoTracking: true)
            .FirstOrDefaultAsync(entity => entity.BookingId == bookingId, cancellationToken);
    }

    public Task<ServiceRequest?> GetServiceRequestByIdAsync(long serviceRequestId, CancellationToken cancellationToken)
    {
        return BuildServiceRequestQuery(asNoTracking: true)
            .FirstOrDefaultAsync(entity => entity.ServiceRequestId == serviceRequestId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Booking>> GetBookingsByCustomerIdAsync(long customerId, CancellationToken cancellationToken)
    {
        return await BuildBookingQuery(asNoTracking: true)
            .Where(entity => entity.CustomerId == customerId)
            .OrderByDescending(entity => entity.BookingDateUtc)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddRevisitRequestAsync(RevisitRequest revisitRequest, CancellationToken cancellationToken)
    {
        return _dbContext.RevisitRequests.AddAsync(revisitRequest, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<RevisitRequest>> GetRevisitRequestsByBookingIdAsync(long bookingId, CancellationToken cancellationToken)
    {
        return await BuildRevisitQuery(asNoTracking: true)
            .Where(entity => entity.BookingId == bookingId)
            .OrderByDescending(entity => entity.RequestedDateUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<RevisitRequest>> GetRevisitRequestsByCustomerIdAsync(long customerId, CancellationToken cancellationToken)
    {
        return await BuildRevisitQuery(asNoTracking: true)
            .Where(entity => entity.CustomerId == customerId)
            .OrderByDescending(entity => entity.RequestedDateUtc)
            .ToArrayAsync(cancellationToken);
    }

    public Task<AmcVisitSchedule?> GetLinkedAmcVisitByServiceRequestIdAsync(long serviceRequestId, CancellationToken cancellationToken)
    {
        return _dbContext.AmcVisitSchedules
            .AsNoTracking()
            .Include(entity => entity.CustomerAmc)
                .ThenInclude(customerAmc => customerAmc!.AmcPlan)
            .FirstOrDefaultAsync(
                entity => entity.ServiceRequestId == serviceRequestId && !entity.IsDeleted,
                cancellationToken);
    }

    public Task<RevisitRequest?> GetLinkedRevisitByServiceRequestIdAsync(long serviceRequestId, CancellationToken cancellationToken)
    {
        return BuildRevisitQuery(asNoTracking: true)
            .FirstOrDefaultAsync(
                entity =>
                    entity.ServiceRequestId == serviceRequestId ||
                    entity.OriginalServiceRequestId == serviceRequestId,
                cancellationToken);
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
                .ThenInclude(serviceRequest => serviceRequest!.Booking)
                    .ThenInclude(booking => booking!.BookingLines)
                        .ThenInclude(line => line.AcType)
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.Booking)
                    .ThenInclude(booking => booking!.BookingLines)
                        .ThenInclude(line => line.Brand)
            .Include(entity => entity.Quotations)
                .ThenInclude(quotation => quotation.InvoiceHeader)
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
            .Include(entity => entity.QuotationHeader)
                .ThenInclude(quotation => quotation!.JobCard)
                    .ThenInclude(jobCard => jobCard!.ServiceRequest)
                        .ThenInclude(serviceRequest => serviceRequest!.Booking)
                            .ThenInclude(booking => booking!.BookingLines)
                                .ThenInclude(line => line.Service)
            .Include(entity => entity.QuotationHeader)
                .ThenInclude(quotation => quotation!.JobCard)
                    .ThenInclude(jobCard => jobCard!.ServiceRequest)
                        .ThenInclude(serviceRequest => serviceRequest!.Booking)
                            .ThenInclude(booking => booking!.BookingLines)
                                .ThenInclude(line => line.AcType)
            .Include(entity => entity.QuotationHeader)
                .ThenInclude(quotation => quotation!.JobCard)
                    .ThenInclude(jobCard => jobCard!.ServiceRequest)
                        .ThenInclude(serviceRequest => serviceRequest!.Booking)
                            .ThenInclude(booking => booking!.BookingLines)
                                .ThenInclude(line => line.Brand)
            .Where(entity => !entity.IsDeleted);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    private IQueryable<CustomerAmc> BuildCustomerAmcQuery(bool asNoTracking)
    {
        IQueryable<CustomerAmc> query = _dbContext.CustomerAmcs
            .AsSplitQuery()
            .Include(entity => entity.Customer)
            .Include(entity => entity.AmcPlan)
            .Include(entity => entity.JobCard)
            .Include(entity => entity.InvoiceHeader)
            .Include(entity => entity.Visits.Where(visit => !visit.IsDeleted))
                .ThenInclude(visit => visit.ServiceRequest)
            .Where(entity => !entity.IsDeleted);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    private IQueryable<WarrantyClaim> BuildWarrantyClaimQuery(bool asNoTracking)
    {
        IQueryable<WarrantyClaim> query = _dbContext.WarrantyClaims
            .AsSplitQuery()
            .Include(entity => entity.Customer)
            .Include(entity => entity.WarrantyRule)
            .Include(entity => entity.RevisitRequest)
            .Include(entity => entity.InvoiceHeader)
            .Include(entity => entity.JobCard)
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

    private IQueryable<Booking> BuildBookingQuery(bool asNoTracking)
    {
        IQueryable<Booking> query = _dbContext.Bookings
            .AsSplitQuery()
            .Include(entity => entity.BookingLines)
                .ThenInclude(line => line.Service)
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.JobCard)
                    .ThenInclude(jobCard => jobCard!.Quotations)
                        .ThenInclude(quotation => quotation.InvoiceHeader)
            .Where(entity => !entity.IsDeleted);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    private IQueryable<ServiceRequest> BuildServiceRequestQuery(bool asNoTracking)
    {
        IQueryable<ServiceRequest> query = _dbContext.ServiceRequests
            .AsSplitQuery()
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.BookingLines)
                    .ThenInclude(line => line.Service)
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.BookingLines)
                    .ThenInclude(line => line.AcType)
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.BookingLines)
                    .ThenInclude(line => line.Brand)
            .Include(entity => entity.JobCard)
            .Where(entity => !entity.IsDeleted);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    private IQueryable<RevisitRequest> BuildRevisitQuery(bool asNoTracking)
    {
        IQueryable<RevisitRequest> query = _dbContext.RevisitRequests
            .AsSplitQuery()
            .Include(entity => entity.Booking)
            .Include(entity => entity.Customer)
            .Include(entity => entity.OriginalJobCard)
            .Include(entity => entity.OriginalServiceRequest)
            .Include(entity => entity.CustomerAmc)
                .ThenInclude(customerAmc => customerAmc!.AmcPlan)
            .Include(entity => entity.WarrantyClaim)
            .Where(entity => !entity.IsDeleted);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }
}
