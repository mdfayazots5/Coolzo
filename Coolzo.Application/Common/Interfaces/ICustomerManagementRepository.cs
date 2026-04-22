using Coolzo.Application.Common.Models;
using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface ICustomerManagementRepository
{
    Task<IReadOnlyCollection<CustomerManagementListItemView>> SearchAsync(
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountSearchAsync(string? searchTerm, CancellationToken cancellationToken);

    Task<CustomerManagementDetailView?> GetDetailAsync(long customerId, CancellationToken cancellationToken);

    Task<Customer?> GetCustomerForUpdateAsync(long customerId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<AuditLog>> ListCustomerNotesAsync(long customerId, int take, CancellationToken cancellationToken);
}
