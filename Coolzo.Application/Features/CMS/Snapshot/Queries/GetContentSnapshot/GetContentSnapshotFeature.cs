using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.CMS;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.CMS.Snapshot.Queries.GetContentSnapshot;

/// <summary>
/// Fallback read: returns the full snapshot document for a version straight from object storage.
/// The portal normally reads the static file directly; this endpoint serves clients that cannot.
/// </summary>
public sealed record GetContentSnapshotQuery(int Version) : IRequest<ContentSnapshotResponse>;

public sealed class GetContentSnapshotQueryHandler
    : IRequestHandler<GetContentSnapshotQuery, ContentSnapshotResponse>
{
    private readonly IObjectStorageService _objectStorageService;
    private readonly IPublishedSnapshotRepository _publishedSnapshotRepository;

    public GetContentSnapshotQueryHandler(
        IObjectStorageService objectStorageService,
        IPublishedSnapshotRepository publishedSnapshotRepository)
    {
        _objectStorageService = objectStorageService;
        _publishedSnapshotRepository = publishedSnapshotRepository;
    }

    public async Task<ContentSnapshotResponse> Handle(
        GetContentSnapshotQuery request,
        CancellationToken cancellationToken)
    {
        var record = await _publishedSnapshotRepository.GetByVersionAsync(request.Version, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, $"Snapshot version {request.Version} was not found.", 404);

        var payload = await _objectStorageService.GetObjectAsync(record.BucketKey, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, $"Snapshot artifact for version {request.Version} is missing from storage.", 404);

        return SnapshotSerializer.Deserialize(payload)
            ?? throw new AppException(ErrorCodes.NotFound, $"Snapshot artifact for version {request.Version} could not be read.", 404);
    }
}
