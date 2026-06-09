using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.CMS;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Coolzo.Application.Features.CMS.Snapshot.Commands.RollbackContentSnapshot;

public sealed record RollbackContentSnapshotCommand(int Version) : IRequest<SnapshotManifestResponse>;

public sealed class RollbackContentSnapshotCommandValidator : AbstractValidator<RollbackContentSnapshotCommand>
{
    public RollbackContentSnapshotCommandValidator()
    {
        RuleFor(request => request.Version).GreaterThan(0);
    }
}

public sealed class RollbackContentSnapshotCommandHandler
    : IRequestHandler<RollbackContentSnapshotCommand, SnapshotManifestResponse>
{
    private const string JsonContentType = "application/json";

    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<RollbackContentSnapshotCommandHandler> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IObjectStorageService _objectStorageService;
    private readonly IPublishedSnapshotRepository _publishedSnapshotRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RollbackContentSnapshotCommandHandler(
        IObjectStorageService objectStorageService,
        IPublishedSnapshotRepository publishedSnapshotRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IMemoryCache memoryCache,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<RollbackContentSnapshotCommandHandler> logger)
    {
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
        RollbackContentSnapshotCommand request,
        CancellationToken cancellationToken)
    {
        var target = await _publishedSnapshotRepository.GetByVersionAsync(request.Version, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, $"Snapshot version {request.Version} was not found.", 404);

        var payload = await _objectStorageService.GetObjectAsync(target.BucketKey, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, $"Snapshot artifact for version {request.Version} is missing from storage.", 404);

        var now = _currentDateTime.UtcNow;

        var activeSnapshots = await _publishedSnapshotRepository.GetActiveListAsync(cancellationToken);

        foreach (var activeSnapshot in activeSnapshots)
        {
            activeSnapshot.IsActive = false;
            activeSnapshot.UpdatedBy = _currentUserContext.UserName;
            activeSnapshot.LastUpdated = now;
        }

        target.IsActive = true;
        target.UpdatedBy = _currentUserContext.UserName;
        target.LastUpdated = now;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Re-point "latest" to the rolled-back artifact after the DB committed.
        await _objectStorageService.PutObjectAsync(SnapshotKeys.Storage.LatestObjectKey, JsonContentType, payload, cancellationToken);

        _memoryCache.Remove(SnapshotKeys.ManifestCacheKey);

        await _adminActivityLogger.WriteAsync(
            "RollbackContentSnapshot",
            nameof(PublishedSnapshot),
            request.Version.ToString(),
            $"Rolled back active snapshot to v{request.Version}.",
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Content snapshot rolled back to v{Version} by {UserName}.",
            request.Version,
            _currentUserContext.UserName);

        return new SnapshotManifestResponse(target.Version, target.BucketUrl, target.ChecksumHash, now);
    }
}
