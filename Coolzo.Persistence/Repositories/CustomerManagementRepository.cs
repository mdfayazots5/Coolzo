using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class CustomerManagementRepository : ICustomerManagementRepository
{
    private readonly CoolzoDbContext _dbContext;

    public CustomerManagementRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<CustomerManagementListItemView>> SearchAsync(
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;
        var query = ApplyCustomerFilters(searchTerm);

        return await query
            .OrderByDescending(customer => customer.DateCreated)
            .Skip(skip)
            .Take(pageSize)
            .Select(customer => new CustomerManagementListItemView(
                customer.CustomerId,
                customer.CustomerName,
                customer.MobileNumber,
                customer.EmailAddress,
                customer.IsGuestCustomer,
                customer.IsActive,
                customer.DateCreated,
                _dbContext.Bookings.Count(booking => !booking.IsDeleted && booking.CustomerId == customer.CustomerId),
                _dbContext.InvoiceHeaders
                    .Where(invoice => !invoice.IsDeleted && invoice.CustomerId == customer.CustomerId)
                    .Select(invoice => (decimal?)invoice.GrandTotalAmount)
                    .Sum() ?? 0m,
                _dbContext.InvoiceHeaders
                    .Where(invoice => !invoice.IsDeleted && invoice.CustomerId == customer.CustomerId)
                    .Select(invoice => (decimal?)invoice.BalanceAmount)
                    .Sum() ?? 0m,
                _dbContext.CustomerAmcs.Any(amc =>
                    !amc.IsDeleted &&
                    amc.CustomerId == customer.CustomerId &&
                    amc.CurrentStatus == AmcSubscriptionStatus.Active),
                _dbContext.SupportTickets.Count(ticket =>
                    !ticket.IsDeleted &&
                    ticket.CustomerId == customer.CustomerId &&
                    ticket.CurrentStatus != SupportTicketStatus.Closed &&
                    ticket.CurrentStatus != SupportTicketStatus.Resolved),
                _dbContext.Bookings
                    .Where(booking => !booking.IsDeleted && booking.CustomerId == customer.CustomerId)
                    .Select(booking => (DateTime?)booking.BookingDateUtc)
                    .Max(),
                _dbContext.InvoiceHeaders
                    .Where(invoice => !invoice.IsDeleted && invoice.CustomerId == customer.CustomerId)
                    .Select(invoice => (DateTime?)invoice.InvoiceDateUtc)
                    .Max(),
                _dbContext.InvoiceHeaders
                    .Where(invoice => !invoice.IsDeleted && invoice.CustomerId == customer.CustomerId)
                    .OrderByDescending(invoice => invoice.InvoiceDateUtc)
                    .Select(invoice => (InvoicePaymentStatus?)invoice.CurrentStatus)
                    .FirstOrDefault(),
                _dbContext.CustomerAddresses
                    .Where(address => !address.IsDeleted && address.IsActive && address.CustomerId == customer.CustomerId)
                    .OrderByDescending(address => address.IsDefault)
                    .ThenBy(address => address.CustomerAddressId)
                    .Select(address => address.AddressLine1 + ", " + address.CityName)
                    .FirstOrDefault()))
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountSearchAsync(string? searchTerm, CancellationToken cancellationToken)
    {
        return ApplyCustomerFilters(searchTerm).CountAsync(cancellationToken);
    }

    public async Task<CustomerManagementDetailView?> GetDetailAsync(long customerId, CancellationToken cancellationToken)
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.CustomerId == customerId && customer.IsActive && !customer.IsDeleted)
            .Select(customer => new CustomerManagementDetailView(
                customer.CustomerId,
                customer.CustomerName,
                customer.MobileNumber,
                customer.EmailAddress,
                customer.IsGuestCustomer,
                customer.IsActive,
                customer.DateCreated,
                _dbContext.Bookings.Count(booking => !booking.IsDeleted && booking.CustomerId == customer.CustomerId),
                _dbContext.InvoiceHeaders
                    .Where(invoice => !invoice.IsDeleted && invoice.CustomerId == customer.CustomerId)
                    .Select(invoice => (decimal?)invoice.GrandTotalAmount)
                    .Sum() ?? 0m,
                _dbContext.InvoiceHeaders
                    .Where(invoice => !invoice.IsDeleted && invoice.CustomerId == customer.CustomerId)
                    .Select(invoice => (decimal?)invoice.BalanceAmount)
                    .Sum() ?? 0m,
                _dbContext.CustomerAmcs.Any(amc =>
                    !amc.IsDeleted &&
                    amc.CustomerId == customer.CustomerId &&
                    amc.CurrentStatus == AmcSubscriptionStatus.Active),
                _dbContext.SupportTickets.Count(ticket =>
                    !ticket.IsDeleted &&
                    ticket.CustomerId == customer.CustomerId &&
                    ticket.CurrentStatus != SupportTicketStatus.Closed &&
                    ticket.CurrentStatus != SupportTicketStatus.Resolved),
                _dbContext.SupportTickets.Count(ticket => !ticket.IsDeleted && ticket.CustomerId == customer.CustomerId),
                _dbContext.Bookings
                    .Where(booking => !booking.IsDeleted && booking.CustomerId == customer.CustomerId)
                    .Select(booking => (DateTime?)booking.BookingDateUtc)
                    .Max(),
                _dbContext.InvoiceHeaders
                    .Where(invoice => !invoice.IsDeleted && invoice.CustomerId == customer.CustomerId)
                    .Select(invoice => (DateTime?)invoice.InvoiceDateUtc)
                    .Max(),
                _dbContext.InvoiceHeaders
                    .Where(invoice => !invoice.IsDeleted && invoice.CustomerId == customer.CustomerId)
                    .OrderByDescending(invoice => invoice.InvoiceDateUtc)
                    .Select(invoice => (InvoicePaymentStatus?)invoice.CurrentStatus)
                    .FirstOrDefault(),
                _dbContext.CustomerAddresses
                    .Where(address => !address.IsDeleted && address.IsActive && address.CustomerId == customer.CustomerId)
                    .OrderByDescending(address => address.IsDefault)
                    .ThenBy(address => address.CustomerAddressId)
                    .Select(address => address.AddressLine1 + ", " + address.CityName)
                    .FirstOrDefault(),
                _dbContext.CustomerAmcs.Count(amc =>
                    !amc.IsDeleted &&
                    amc.CustomerId == customer.CustomerId &&
                    amc.CurrentStatus == AmcSubscriptionStatus.Active),
                _dbContext.CustomerAmcs
                    .Where(amc =>
                        !amc.IsDeleted &&
                        amc.CustomerId == customer.CustomerId &&
                        amc.CurrentStatus == AmcSubscriptionStatus.Active)
                    .OrderByDescending(amc => amc.EndDateUtc)
                    .Select(amc => amc.AmcPlan != null ? amc.AmcPlan.PlanName : null)
                    .FirstOrDefault(),
                _dbContext.CustomerAmcs
                    .Where(amc =>
                        !amc.IsDeleted &&
                        amc.CustomerId == customer.CustomerId &&
                        amc.CurrentStatus == AmcSubscriptionStatus.Active)
                    .OrderByDescending(amc => amc.EndDateUtc)
                    .Select(amc => (AmcSubscriptionStatus?)amc.CurrentStatus)
                    .FirstOrDefault(),
                _dbContext.CustomerAmcs
                    .Where(amc =>
                        !amc.IsDeleted &&
                        amc.CustomerId == customer.CustomerId &&
                        amc.CurrentStatus == AmcSubscriptionStatus.Active)
                    .OrderByDescending(amc => amc.EndDateUtc)
                    .Select(amc => (int?)amc.TotalVisitCount)
                    .FirstOrDefault(),
                _dbContext.CustomerAmcs
                    .Where(amc =>
                        !amc.IsDeleted &&
                        amc.CustomerId == customer.CustomerId &&
                        amc.CurrentStatus == AmcSubscriptionStatus.Active)
                    .OrderByDescending(amc => amc.EndDateUtc)
                    .Select(amc => (int?)amc.ConsumedVisitCount)
                    .FirstOrDefault(),
                _dbContext.AmcVisitSchedules
                    .Where(visit =>
                        !visit.IsDeleted &&
                        visit.CustomerAmc != null &&
                        visit.CustomerAmc.CustomerId == customer.CustomerId &&
                        visit.CustomerAmc.CurrentStatus == AmcSubscriptionStatus.Active)
                    .OrderBy(visit => visit.ScheduledDate)
                    .Select(visit => (DateOnly?)visit.ScheduledDate)
                    .FirstOrDefault()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Customer?> GetCustomerForUpdateAsync(long customerId, CancellationToken cancellationToken)
    {
        return _dbContext.Customers
            .FirstOrDefaultAsync(customer => customer.CustomerId == customerId && customer.IsActive && !customer.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyCollection<AuditLog>> ListCustomerNotesAsync(long customerId, int take, CancellationToken cancellationToken)
    {
        var customerIdValue = customerId.ToString();

        return await _dbContext.AuditLogs
            .AsNoTracking()
            .Where(auditLog =>
                !auditLog.IsDeleted &&
                auditLog.EntityName == "Customer" &&
                auditLog.EntityId == customerIdValue &&
                auditLog.ActionName == "CustomerNoteAdded")
            .OrderByDescending(auditLog => auditLog.DateCreated)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }

    private IQueryable<Customer> ApplyCustomerFilters(string? searchTerm)
    {
        var query = _dbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.IsActive && !customer.IsDeleted);

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return query;
        }

        return query.Where(customer =>
            customer.CustomerName.Contains(searchTerm) ||
            customer.MobileNumber.Contains(searchTerm) ||
            customer.EmailAddress.Contains(searchTerm));
    }
}
