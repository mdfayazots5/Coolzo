using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IRoleRepository
{
    Task AddAsync(Role role, CancellationToken cancellationToken);

    Task<int> CountAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Role>> GetByIdsAsync(IReadOnlyCollection<long> roleIds, CancellationToken cancellationToken);

    Task<Role?> GetByIdWithPermissionsAsync(long roleId, CancellationToken cancellationToken);

    Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Role>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}
