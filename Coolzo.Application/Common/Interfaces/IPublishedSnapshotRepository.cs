using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IPublishedSnapshotRepository
{
    Task AddAsync(PublishedSnapshot publishedSnapshot, CancellationToken cancellationToken);

    Task<PublishedSnapshot?> GetActiveAsync(CancellationToken cancellationToken);

    Task<PublishedSnapshot?> GetByVersionAsync(int version, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PublishedSnapshot>> GetActiveListAsync(CancellationToken cancellationToken);

    Task<int> GetMaxVersionAsync(CancellationToken cancellationToken);
}
