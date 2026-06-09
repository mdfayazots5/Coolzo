using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.CMS;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Coolzo.Application.Features.CMS.Snapshot.Queries.GetSnapshotManifest;

public sealed record GetSnapshotManifestQuery : IRequest<SnapshotManifestResponse>;

public sealed class GetSnapshotManifestQueryHandler
    : IRequestHandler<GetSnapshotManifestQuery, SnapshotManifestResponse>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    private readonly IMemoryCache _memoryCache;
    private readonly IPublishedSnapshotRepository _publishedSnapshotRepository;

    public GetSnapshotManifestQueryHandler(
        IMemoryCache memoryCache,
        IPublishedSnapshotRepository publishedSnapshotRepository)
    {
        _memoryCache = memoryCache;
        _publishedSnapshotRepository = publishedSnapshotRepository;
    }

    public async Task<SnapshotManifestResponse> Handle(
        GetSnapshotManifestQuery request,
        CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue(SnapshotKeys.ManifestCacheKey, out SnapshotManifestResponse? cached) && cached is not null)
        {
            return cached;
        }

        var active = await _publishedSnapshotRepository.GetActiveAsync(cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "No published content snapshot is active.", 404);

        var manifest = new SnapshotManifestResponse(
            active.Version,
            active.BucketUrl,
            active.ChecksumHash,
            active.DatePublished ?? active.DateCreated);

        _memoryCache.Set(SnapshotKeys.ManifestCacheKey, manifest, CacheDuration);

        return manifest;
    }
}
