using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class SupportTicketRepository : ISupportTicketRepository
{
    private readonly CoolzoDbContext _dbContext;

    public SupportTicketRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddTicketAsync(SupportTicket supportTicket, CancellationToken cancellationToken)
    {
        return _dbContext.SupportTickets.AddAsync(supportTicket, cancellationToken).AsTask();
    }

    public Task<bool> TicketNumberExistsAsync(string ticketNumber, CancellationToken cancellationToken)
    {
        return _dbContext.SupportTickets.AnyAsync(
            entity => entity.TicketNumber == ticketNumber && !entity.IsDeleted,
            cancellationToken);
    }

    public Task<SupportTicket?> GetByIdAsync(long supportTicketId, CancellationToken cancellationToken)
    {
        return BuildSupportTicketQuery(asNoTracking: true)
            .FirstOrDefaultAsync(entity => entity.SupportTicketId == supportTicketId, cancellationToken);
    }

    public Task<SupportTicket?> GetByIdForUpdateAsync(long supportTicketId, CancellationToken cancellationToken)
    {
        return BuildSupportTicketQuery(asNoTracking: false)
            .FirstOrDefaultAsync(entity => entity.SupportTicketId == supportTicketId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<SupportTicket>> SearchAsync(
        string? ticketNumber,
        string? customerMobile,
        long? categoryId,
        long? priorityId,
        SupportTicketStatus? status,
        DateOnly? dateFrom,
        DateOnly? dateTo,
        SupportTicketLinkType? linkedEntityType,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;

        return await ApplySearchFilters(ticketNumber, customerMobile, categoryId, priorityId, status, dateFrom, dateTo, linkedEntityType)
            .OrderByDescending(entity => entity.LastUpdated ?? entity.DateCreated)
            .ThenByDescending(entity => entity.DateCreated)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountSearchAsync(
        string? ticketNumber,
        string? customerMobile,
        long? categoryId,
        long? priorityId,
        SupportTicketStatus? status,
        DateOnly? dateFrom,
        DateOnly? dateTo,
        SupportTicketLinkType? linkedEntityType,
        CancellationToken cancellationToken)
    {
        return ApplySearchFilters(ticketNumber, customerMobile, categoryId, priorityId, status, dateFrom, dateTo, linkedEntityType)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<SupportTicket>> ListByCustomerIdAsync(
        long customerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;

        return await BuildSupportTicketQuery(asNoTracking: true)
            .Where(entity => entity.CustomerId == customerId)
            .OrderByDescending(entity => entity.LastUpdated ?? entity.DateCreated)
            .ThenByDescending(entity => entity.DateCreated)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountByCustomerIdAsync(long customerId, CancellationToken cancellationToken)
    {
        return _dbContext.SupportTickets.CountAsync(
            entity => entity.CustomerId == customerId && !entity.IsDeleted,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<SupportTicketCategory>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.SupportTicketCategories
            .AsNoTracking()
            .Where(entity => entity.IsActive && !entity.IsDeleted)
            .OrderBy(entity => entity.SortOrder)
            .ThenBy(entity => entity.CategoryName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<SupportTicketPriority>> GetPrioritiesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.SupportTicketPriorities
            .AsNoTracking()
            .Where(entity => entity.IsActive && !entity.IsDeleted)
            .OrderBy(entity => entity.PriorityRank)
            .ThenBy(entity => entity.SortOrder)
            .ThenBy(entity => entity.PriorityName)
            .ToArrayAsync(cancellationToken);
    }

    public Task<SupportTicketCategory?> GetCategoryByIdAsync(long supportTicketCategoryId, CancellationToken cancellationToken)
    {
        return _dbContext.SupportTicketCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity => entity.SupportTicketCategoryId == supportTicketCategoryId &&
                    entity.IsActive &&
                    !entity.IsDeleted,
                cancellationToken);
    }

    public Task<SupportTicketPriority?> GetPriorityByIdAsync(long supportTicketPriorityId, CancellationToken cancellationToken)
    {
        return _dbContext.SupportTicketPriorities
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity => entity.SupportTicketPriorityId == supportTicketPriorityId &&
                    entity.IsActive &&
                    !entity.IsDeleted,
                cancellationToken);
    }

    public Task<Customer?> GetCustomerByIdAsync(long customerId, CancellationToken cancellationToken)
    {
        return _dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity => entity.CustomerId == customerId &&
                    entity.IsActive &&
                    !entity.IsDeleted,
                cancellationToken);
    }

    public Task<Customer?> GetCustomerByUserIdAsync(long userId, CancellationToken cancellationToken)
    {
        return _dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity => entity.UserId == userId &&
                    entity.IsActive &&
                    !entity.IsDeleted,
                cancellationToken);
    }

    public async Task<SupportLinkedEntityResolution?> ResolveLinkedEntityAsync(
        SupportTicketLinkType linkedEntityType,
        long linkedEntityId,
        CancellationToken cancellationToken)
    {
        return linkedEntityType switch
        {
            SupportTicketLinkType.Booking => await ResolveBookingAsync(linkedEntityId, cancellationToken),
            SupportTicketLinkType.ServiceRequest => await ResolveServiceRequestAsync(linkedEntityId, cancellationToken),
            SupportTicketLinkType.JobCard => await ResolveJobCardAsync(linkedEntityId, cancellationToken),
            SupportTicketLinkType.Invoice => await ResolveInvoiceAsync(linkedEntityId, cancellationToken),
            SupportTicketLinkType.CustomerAMC => await ResolveCustomerAmcAsync(linkedEntityId, cancellationToken),
            SupportTicketLinkType.WarrantyClaim => await ResolveWarrantyClaimAsync(linkedEntityId, cancellationToken),
            SupportTicketLinkType.RevisitRequest => await ResolveRevisitRequestAsync(linkedEntityId, cancellationToken),
            _ => null
        };
    }

    public async Task<SupportJobAlertView> GetJobAlertAsync(
        long serviceRequestId,
        long bookingId,
        long? jobCardId,
        CancellationToken cancellationToken)
    {
        var matchingTicketIds = _dbContext.SupportTicketLinks
            .AsNoTracking()
            .Where(link =>
                !link.IsDeleted &&
                (
                    (link.LinkedEntityType == SupportTicketLinkType.ServiceRequest && link.LinkedEntityId == serviceRequestId) ||
                    (link.LinkedEntityType == SupportTicketLinkType.Booking && link.LinkedEntityId == bookingId) ||
                    (jobCardId.HasValue && link.LinkedEntityType == SupportTicketLinkType.JobCard && link.LinkedEntityId == jobCardId.Value)
                ))
            .Select(link => link.SupportTicketId)
            .Distinct();
        var tickets = await _dbContext.SupportTickets
            .AsNoTracking()
            .Where(ticket => !ticket.IsDeleted && matchingTicketIds.Contains(ticket.SupportTicketId))
            .OrderByDescending(ticket => ticket.LastUpdated ?? ticket.DateCreated)
            .ThenByDescending(ticket => ticket.DateCreated)
            .ToArrayAsync(cancellationToken);
        var latestTicket = tickets.FirstOrDefault();

        return new SupportJobAlertView(
            tickets.Length > 0,
            tickets.Length,
            tickets.Count(ticket => ticket.CurrentStatus != SupportTicketStatus.Closed),
            latestTicket?.TicketNumber,
            latestTicket?.CurrentStatus.ToString(),
            latestTicket?.Subject);
    }

    private IQueryable<SupportTicket> ApplySearchFilters(
        string? ticketNumber,
        string? customerMobile,
        long? categoryId,
        long? priorityId,
        SupportTicketStatus? status,
        DateOnly? dateFrom,
        DateOnly? dateTo,
        SupportTicketLinkType? linkedEntityType)
    {
        var query = BuildSupportTicketQuery(asNoTracking: true);

        if (!string.IsNullOrWhiteSpace(ticketNumber))
        {
            query = query.Where(entity => entity.TicketNumber.Contains(ticketNumber));
        }

        if (!string.IsNullOrWhiteSpace(customerMobile))
        {
            query = query.Where(entity => entity.Customer != null && entity.Customer.MobileNumber.Contains(customerMobile));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(entity => entity.SupportTicketCategoryId == categoryId.Value);
        }

        if (priorityId.HasValue)
        {
            query = query.Where(entity => entity.SupportTicketPriorityId == priorityId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(entity => entity.CurrentStatus == status.Value);
        }

        if (dateFrom.HasValue)
        {
            var startDateUtc = dateFrom.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(entity => entity.DateCreated >= startDateUtc);
        }

        if (dateTo.HasValue)
        {
            var endDateExclusiveUtc = dateTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
            query = query.Where(entity => entity.DateCreated < endDateExclusiveUtc);
        }

        if (linkedEntityType.HasValue)
        {
            query = query.Where(entity =>
                entity.Links.Any(link => !link.IsDeleted && link.LinkedEntityType == linkedEntityType.Value));
        }

        return query;
    }

    private IQueryable<SupportTicket> BuildSupportTicketQuery(bool asNoTracking)
    {
        IQueryable<SupportTicket> query = _dbContext.SupportTickets
            .AsSplitQuery()
            .Include(entity => entity.Customer)
            .Include(entity => entity.Category)
            .Include(entity => entity.Priority)
            .Include(entity => entity.Links)
            .Include(entity => entity.Replies)
            .Include(entity => entity.Escalations)
            .Include(entity => entity.StatusHistories)
            .Include(entity => entity.Assignments)
                .ThenInclude(assignment => assignment.AssignedUser)
            .Where(entity => !entity.IsDeleted);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    private async Task<SupportLinkedEntityResolution?> ResolveBookingAsync(long linkedEntityId, CancellationToken cancellationToken)
    {
        var booking = await _dbContext.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.BookingId == linkedEntityId && !entity.IsDeleted, cancellationToken);

        return booking is null
            ? null
            : new SupportLinkedEntityResolution(
                booking.CustomerId,
                booking.BookingReference,
                $"{booking.ServiceNameSnapshot} booking for {booking.CustomerNameSnapshot}");
    }

    private async Task<SupportLinkedEntityResolution?> ResolveServiceRequestAsync(long linkedEntityId, CancellationToken cancellationToken)
    {
        var serviceRequest = await _dbContext.ServiceRequests
            .AsNoTracking()
            .Include(entity => entity.Booking)
            .FirstOrDefaultAsync(entity => entity.ServiceRequestId == linkedEntityId && !entity.IsDeleted, cancellationToken);

        return serviceRequest?.Booking is null
            ? null
            : new SupportLinkedEntityResolution(
                serviceRequest.Booking.CustomerId,
                serviceRequest.ServiceRequestNumber,
                $"Service request for {serviceRequest.Booking.BookingReference}");
    }

    private async Task<SupportLinkedEntityResolution?> ResolveJobCardAsync(long linkedEntityId, CancellationToken cancellationToken)
    {
        var jobCard = await _dbContext.JobCards
            .AsNoTracking()
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.Booking)
            .FirstOrDefaultAsync(entity => entity.JobCardId == linkedEntityId && !entity.IsDeleted, cancellationToken);

        return jobCard?.ServiceRequest?.Booking is null
            ? null
            : new SupportLinkedEntityResolution(
                jobCard.ServiceRequest.Booking.CustomerId,
                jobCard.JobCardNumber,
                $"Job card for {jobCard.ServiceRequest.ServiceRequestNumber}");
    }

    private async Task<SupportLinkedEntityResolution?> ResolveInvoiceAsync(long linkedEntityId, CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.InvoiceHeaders
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.InvoiceHeaderId == linkedEntityId && !entity.IsDeleted, cancellationToken);

        return invoice is null
            ? null
            : new SupportLinkedEntityResolution(
                invoice.CustomerId,
                invoice.InvoiceNumber,
                $"{invoice.CurrentStatus} invoice");
    }

    private async Task<SupportLinkedEntityResolution?> ResolveCustomerAmcAsync(long linkedEntityId, CancellationToken cancellationToken)
    {
        var customerAmc = await _dbContext.CustomerAmcs
            .AsNoTracking()
            .Include(entity => entity.AmcPlan)
            .FirstOrDefaultAsync(entity => entity.CustomerAmcId == linkedEntityId && !entity.IsDeleted, cancellationToken);

        return customerAmc is null
            ? null
            : new SupportLinkedEntityResolution(
                customerAmc.CustomerId,
                $"AMC-{customerAmc.CustomerAmcId}",
                customerAmc.AmcPlan?.PlanName ?? customerAmc.CurrentStatus.ToString());
    }

    private async Task<SupportLinkedEntityResolution?> ResolveWarrantyClaimAsync(long linkedEntityId, CancellationToken cancellationToken)
    {
        var warrantyClaim = await _dbContext.WarrantyClaims
            .AsNoTracking()
            .Include(entity => entity.InvoiceHeader)
            .FirstOrDefaultAsync(entity => entity.WarrantyClaimId == linkedEntityId && !entity.IsDeleted, cancellationToken);

        return warrantyClaim is null
            ? null
            : new SupportLinkedEntityResolution(
                warrantyClaim.CustomerId,
                $"WC-{warrantyClaim.WarrantyClaimId}",
                warrantyClaim.InvoiceHeader?.InvoiceNumber ?? warrantyClaim.CurrentStatus.ToString());
    }

    private async Task<SupportLinkedEntityResolution?> ResolveRevisitRequestAsync(long linkedEntityId, CancellationToken cancellationToken)
    {
        var revisitRequest = await _dbContext.RevisitRequests
            .AsNoTracking()
            .Include(entity => entity.Booking)
            .FirstOrDefaultAsync(entity => entity.RevisitRequestId == linkedEntityId && !entity.IsDeleted, cancellationToken);

        return revisitRequest is null
            ? null
            : new SupportLinkedEntityResolution(
                revisitRequest.CustomerId,
                $"RV-{revisitRequest.RevisitRequestId}",
                revisitRequest.Booking?.BookingReference ?? revisitRequest.CurrentStatus.ToString());
    }
}
