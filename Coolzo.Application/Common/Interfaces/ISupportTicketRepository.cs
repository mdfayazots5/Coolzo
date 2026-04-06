using Coolzo.Application.Common.Models;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;

namespace Coolzo.Application.Common.Interfaces;

public interface ISupportTicketRepository
{
    Task AddTicketAsync(SupportTicket supportTicket, CancellationToken cancellationToken);

    Task<bool> TicketNumberExistsAsync(string ticketNumber, CancellationToken cancellationToken);

    Task<SupportTicket?> GetByIdAsync(long supportTicketId, CancellationToken cancellationToken);

    Task<SupportTicket?> GetByIdForUpdateAsync(long supportTicketId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SupportTicket>> SearchAsync(
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
        CancellationToken cancellationToken);

    Task<int> CountSearchAsync(
        string? ticketNumber,
        string? customerMobile,
        long? categoryId,
        long? priorityId,
        SupportTicketStatus? status,
        DateOnly? dateFrom,
        DateOnly? dateTo,
        SupportTicketLinkType? linkedEntityType,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SupportTicket>> ListByCustomerIdAsync(
        long customerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountByCustomerIdAsync(long customerId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SupportTicketCategory>> GetCategoriesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SupportTicketPriority>> GetPrioritiesAsync(CancellationToken cancellationToken);

    Task<SupportTicketCategory?> GetCategoryByIdAsync(long supportTicketCategoryId, CancellationToken cancellationToken);

    Task<SupportTicketPriority?> GetPriorityByIdAsync(long supportTicketPriorityId, CancellationToken cancellationToken);

    Task<Customer?> GetCustomerByIdAsync(long customerId, CancellationToken cancellationToken);

    Task<Customer?> GetCustomerByUserIdAsync(long userId, CancellationToken cancellationToken);

    Task<SupportLinkedEntityResolution?> ResolveLinkedEntityAsync(
        SupportTicketLinkType linkedEntityType,
        long linkedEntityId,
        CancellationToken cancellationToken);

    Task<SupportJobAlertView> GetJobAlertAsync(
        long serviceRequestId,
        long bookingId,
        long? jobCardId,
        CancellationToken cancellationToken);
}
