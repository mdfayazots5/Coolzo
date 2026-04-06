using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class BookingRepository : IBookingRepository
{
    private readonly CoolzoDbContext _dbContext;

    public BookingRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken)
    {
        return _dbContext.Customers.AddAsync(customer, cancellationToken).AsTask();
    }

    public Task AddCustomerAddressAsync(CustomerAddress customerAddress, CancellationToken cancellationToken)
    {
        return _dbContext.CustomerAddresses.AddAsync(customerAddress, cancellationToken).AsTask();
    }

    public Task AddBookingAsync(Booking booking, CancellationToken cancellationToken)
    {
        return _dbContext.Bookings.AddAsync(booking, cancellationToken).AsTask();
    }

    public Task<bool> BookingReferenceExistsAsync(string bookingReference, CancellationToken cancellationToken)
    {
        return _dbContext.Bookings.AnyAsync(entity => entity.BookingReference == bookingReference, cancellationToken);
    }

    public Task<Customer?> GetCustomerByIdAsync(long customerId, CancellationToken cancellationToken)
    {
        return _dbContext.Customers
            .Include(entity => entity.CustomerAddresses)
            .FirstOrDefaultAsync(entity => entity.CustomerId == customerId && entity.IsActive && !entity.IsDeleted, cancellationToken);
    }

    public Task<Customer?> GetCustomerByUserIdAsync(long userId, CancellationToken cancellationToken)
    {
        return _dbContext.Customers
            .Include(entity => entity.CustomerAddresses)
            .FirstOrDefaultAsync(entity => entity.UserId == userId && entity.IsActive && !entity.IsDeleted, cancellationToken);
    }

    public Task<Customer?> GetCustomerByMobileAsync(string mobileNumber, CancellationToken cancellationToken)
    {
        return _dbContext.Customers
            .Include(entity => entity.CustomerAddresses)
            .FirstOrDefaultAsync(entity => entity.MobileNumber == mobileNumber && entity.IsActive && !entity.IsDeleted, cancellationToken);
    }

    public Task<CustomerAddress?> GetCustomerAddressAsync(long customerId, string addressLine1, string pincode, CancellationToken cancellationToken)
    {
        return _dbContext.CustomerAddresses
            .FirstOrDefaultAsync(
                entity => entity.CustomerId == customerId &&
                    entity.AddressLine1 == addressLine1 &&
                    entity.Pincode == pincode &&
                    entity.IsActive &&
                    !entity.IsDeleted,
                cancellationToken);
    }

    public Task<Booking?> GetByIdAsync(long bookingId, CancellationToken cancellationToken)
    {
        return BuildBookingQuery()
            .FirstOrDefaultAsync(entity => entity.BookingId == bookingId, cancellationToken);
    }

    public Task<Booking?> GetByIdForUpdateAsync(long bookingId, CancellationToken cancellationToken)
    {
        return BuildBookingQuery(false)
            .FirstOrDefaultAsync(entity => entity.BookingId == bookingId, cancellationToken);
    }

    public Task<Booking?> GetByIdForCustomerAsync(long bookingId, long customerId, CancellationToken cancellationToken)
    {
        return BuildBookingQuery()
            .FirstOrDefaultAsync(entity => entity.BookingId == bookingId && entity.CustomerId == customerId, cancellationToken);
    }

    public Task<Booking?> GetByIdForCustomerForUpdateAsync(long bookingId, long customerId, CancellationToken cancellationToken)
    {
        return BuildBookingQuery(false)
            .FirstOrDefaultAsync(entity => entity.BookingId == bookingId && entity.CustomerId == customerId, cancellationToken);
    }

    public Task<Booking?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        return BuildBookingQuery()
            .FirstOrDefaultAsync(
                entity => entity.IdempotencyKey == idempotencyKey && !entity.IsDeleted,
                cancellationToken);
    }

    public Task<bool> HasDuplicateBookingAsync(string mobileNumber, long slotAvailabilityId, long serviceId, CancellationToken cancellationToken)
    {
        return _dbContext.Bookings.AnyAsync(
            entity =>
                !entity.IsDeleted &&
                entity.MobileNumberSnapshot == mobileNumber &&
                entity.SlotAvailabilityId == slotAvailabilityId &&
                entity.BookingLines.Any(line => !line.IsDeleted && line.ServiceId == serviceId),
            cancellationToken);
    }

    public Task<bool> HasSlotConflictAsync(string mobileNumber, DateOnly slotDate, long slotConfigurationId, CancellationToken cancellationToken)
    {
        return _dbContext.Bookings.AnyAsync(
            entity =>
                !entity.IsDeleted &&
                entity.MobileNumberSnapshot == mobileNumber &&
                entity.SlotAvailability != null &&
                entity.SlotAvailability.SlotDate == slotDate &&
                entity.SlotAvailability.SlotConfigurationId == slotConfigurationId,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<Booking>> ListByCustomerIdAsync(long customerId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;

        return await BuildBookingQuery()
            .Where(entity => entity.CustomerId == customerId)
            .OrderByDescending(entity => entity.BookingDateUtc)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountByCustomerIdAsync(long customerId, CancellationToken cancellationToken)
    {
        return _dbContext.Bookings.CountAsync(entity => entity.CustomerId == customerId && !entity.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Booking>> SearchAsync(
        string? bookingReference,
        string? customerMobile,
        DateOnly? bookingDate,
        long? serviceId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;
        var query = ApplyFilters(bookingReference, customerMobile, bookingDate, serviceId);

        return await query
            .OrderByDescending(entity => entity.BookingDateUtc)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountSearchAsync(
        string? bookingReference,
        string? customerMobile,
        DateOnly? bookingDate,
        long? serviceId,
        CancellationToken cancellationToken)
    {
        return ApplyFilters(bookingReference, customerMobile, bookingDate, serviceId).CountAsync(cancellationToken);
    }

    private IQueryable<Booking> ApplyFilters(string? bookingReference, string? customerMobile, DateOnly? bookingDate, long? serviceId)
    {
        var query = BuildBookingQuery();

        if (!string.IsNullOrWhiteSpace(bookingReference))
        {
            query = query.Where(entity => entity.BookingReference.Contains(bookingReference));
        }

        if (!string.IsNullOrWhiteSpace(customerMobile))
        {
            query = query.Where(entity => entity.MobileNumberSnapshot.Contains(customerMobile));
        }

        if (bookingDate.HasValue)
        {
            query = query.Where(entity => entity.SlotAvailability != null && entity.SlotAvailability.SlotDate == bookingDate.Value);
        }

        if (serviceId.HasValue)
        {
            query = query.Where(entity => entity.BookingLines.Any(line => line.ServiceId == serviceId.Value));
        }

        return query;
    }

    private IQueryable<Booking> BuildBookingQuery(bool asNoTracking = true)
    {
        IQueryable<Booking> query = _dbContext.Bookings
            .Include(entity => entity.Customer)
            .Include(entity => entity.CustomerAddress)
            .Include(entity => entity.SlotAvailability)
                .ThenInclude(slot => slot!.SlotConfiguration)
            .Include(entity => entity.BookingLines)
                .ThenInclude(line => line.Service)
            .Include(entity => entity.BookingLines)
                .ThenInclude(line => line.AcType)
            .Include(entity => entity.BookingLines)
                .ThenInclude(line => line.Tonnage)
            .Include(entity => entity.BookingLines)
                .ThenInclude(line => line.Brand)
            .Include(entity => entity.BookingStatusHistories)
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.Assignments)
                    .ThenInclude(assignment => assignment.Technician)
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.JobCard)
                    .ThenInclude(jobCard => jobCard!.Quotations)
                        .ThenInclude(quotation => quotation.InvoiceHeader)
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.JobCard)
                    .ThenInclude(jobCard => jobCard!.JobDiagnosis)
                        .ThenInclude(diagnosis => diagnosis!.ComplaintIssueMaster)
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.JobCard)
                    .ThenInclude(jobCard => jobCard!.JobDiagnosis)
                        .ThenInclude(diagnosis => diagnosis!.DiagnosisResultMaster)
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.JobCard)
                    .ThenInclude(jobCard => jobCard!.ExecutionNotes)
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.JobCard)
                    .ThenInclude(jobCard => jobCard!.ExecutionTimelines)
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.StatusHistories)
            .Where(entity => !entity.IsDeleted);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }
}
