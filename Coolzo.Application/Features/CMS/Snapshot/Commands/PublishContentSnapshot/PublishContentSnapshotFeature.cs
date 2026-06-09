using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.CMS;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Models;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Coolzo.Application.Features.CMS.Snapshot.Commands.PublishContentSnapshot;

public sealed record PublishContentSnapshotCommand : IRequest<SnapshotManifestResponse>;

public sealed class PublishContentSnapshotCommandHandler
    : IRequestHandler<PublishContentSnapshotCommand, SnapshotManifestResponse>
{
    private const string JsonContentType = "application/json";

    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IContentSnapshotBuilder _contentSnapshotBuilder;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<PublishContentSnapshotCommandHandler> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IObjectStorageService _objectStorageService;
    private readonly IPublishedSnapshotRepository _publishedSnapshotRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PublishContentSnapshotCommandHandler(
        IContentSnapshotBuilder contentSnapshotBuilder,
        IObjectStorageService objectStorageService,
        IPublishedSnapshotRepository publishedSnapshotRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IMemoryCache memoryCache,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<PublishContentSnapshotCommandHandler> logger)
    {
        _contentSnapshotBuilder = contentSnapshotBuilder;
        _objectStorageService = objectStorageService;
        _publishedSnapshotRepository = publishedSnapshotRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _memoryCache = memoryCache;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<SnapshotManifestResponse> Handle(
        PublishContentSnapshotCommand request,
        CancellationToken cancellationToken)
    {
        var body = await _contentSnapshotBuilder.BuildAsync(cancellationToken);
        var version = await _publishedSnapshotRepository.GetMaxVersionAsync(cancellationToken) + 1;
        var publishedAtUtc = _currentDateTime.UtcNow;
        var checksum = SnapshotSerializer.ComputeBodyChecksum(body);

        var snapshot = new ContentSnapshotResponse(
            version,
            publishedAtUtc,
            checksum,
            body.Theme,
            body.Masters,
            body.Content,
            body.Images);

        var payload = SnapshotSerializer.Serialize(snapshot);
        var versionedKey = SnapshotKeys.Storage.VersionedObjectKey(version);

        // 1) Write the immutable versioned artifact first.
        var stored = await _objectStorageService.PutObjectAsync(versionedKey, JsonContentType, payload, cancellationToken);

        // 2) Commit the version row (deactivating any prior active version) before flipping "latest".
        var activeSnapshots = await _publishedSnapshotRepository.GetActiveListAsync(cancellationToken);

        foreach (var activeSnapshot in activeSnapshots)
        {
            activeSnapshot.IsActive = false;
            activeSnapshot.UpdatedBy = _currentUserContext.UserName;
            activeSnapshot.LastUpdated = publishedAtUtc;
        }

        var entity = new PublishedSnapshot
        {
            Version = version,
            BucketKey = versionedKey,
            BucketUrl = stored.PublicUrl,
            ChecksumHash = checksum,
            PayloadSizeBytes = payload.LongLength,
            IsActive = true,
            PublishedBy = _currentUserContext.UserName,
            DatePublished = publishedAtUtc,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = publishedAtUtc,
            IPAddress = _currentUserContext.IPAddress
        };

        await _publishedSnapshotRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 3) Flip "latest" only after the DB committed — this is the atomic activation switch for the portal.
        await _objectStorageService.PutObjectAsync(SnapshotKeys.Storage.LatestObjectKey, JsonContentType, payload, cancellationToken);

        _memoryCache.Remove(SnapshotKeys.ManifestCacheKey);

        await _adminActivityLogger.WriteAsync(
            "PublishContentSnapshot",
            nameof(PublishedSnapshot),
            version.ToString(),
            $"Published snapshot v{version} ({payload.LongLength} bytes).",
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Content snapshot v{Version} published by {UserName} ({Bytes} bytes).",
            version,
            _currentUserContext.UserName,
            payload.LongLength);

        return new SnapshotManifestResponse(version, stored.PublicUrl, checksum, publishedAtUtc);
    }
}
