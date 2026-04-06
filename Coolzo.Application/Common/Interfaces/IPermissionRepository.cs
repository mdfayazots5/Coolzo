using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IPermissionRepository
{
    Task<int> CountAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Permission>> GetByIdsAsync(IReadOnlyCollection<long> permissionIds, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Permission>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}
