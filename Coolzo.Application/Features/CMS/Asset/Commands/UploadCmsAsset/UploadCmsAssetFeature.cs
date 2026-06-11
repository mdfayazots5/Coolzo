using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.CMS;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.CMS.Asset.Commands.UploadCmsAsset;

/// <summary>
/// Uploads a standalone CMS asset (e.g. the brand logo) to object storage and returns its public URL.
/// Unlike screen-image upload, this is not tied to a slot; the caller stores the returned URL wherever
/// it is needed (e.g. the theme.logoUrl token).
/// </summary>
public sealed record UploadCmsAssetCommand(
    string FileName,
    string ContentType,
    string Base64Content,
    string? AssetKey) : IRequest<CmsAssetUploadResponse>;

public sealed class UploadCmsAssetCommandValidator : AbstractValidator<UploadCmsAssetCommand>
{
    private static readonly string[] AllowedContentTypes =
        { "image/png", "image/jpeg", "image/webp", "image/svg+xml" };

    public UploadCmsAssetCommandValidator()
    {
        RuleFor(request => request.FileName).NotEmpty().MaximumLength(256);
        RuleFor(request => request.ContentType).NotEmpty()
            .Must(value => AllowedContentTypes.Contains(value.ToLowerInvariant()))
            .WithMessage("Image must be PNG, JPEG, WebP, or SVG.");
        RuleFor(request => request.Base64Content).NotEmpty();
        RuleFor(request => request.AssetKey).MaximumLength(64);
    }
}

public sealed class UploadCmsAssetCommandHandler
    : IRequestHandler<UploadCmsAssetCommand, CmsAssetUploadResponse>
{
    private const int MaxImageBytes = 5 * 1024 * 1024;

    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<UploadCmsAssetCommandHandler> _logger;
    private readonly IObjectStorageService _objectStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public UploadCmsAssetCommandHandler(
        IObjectStorageService objectStorageService,
        IUnitOfWork unitOfWork,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<UploadCmsAssetCommandHandler> logger)
    {
        _objectStorageService = objectStorageService;
        _unitOfWork = unitOfWork;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<CmsAssetUploadResponse> Handle(
        UploadCmsAssetCommand request,
        CancellationToken cancellationToken)
    {
        var fileBytes = DecodeBase64(request.Base64Content);

        if (fileBytes.Length > MaxImageBytes)
        {
            throw new AppException(ErrorCodes.ValidationFailure, "Image size must not exceed 5 MB.", 400);
        }

        var assetKey = string.IsNullOrWhiteSpace(request.AssetKey) ? "asset" : request.AssetKey.Trim();
        var extension = Path.GetExtension(request.FileName);
        var objectKey = $"cms/assets/{assetKey}-{Guid.NewGuid():N}{extension}".ToLowerInvariant();

        var stored = await _objectStorageService.PutObjectAsync(objectKey, request.ContentType, fileBytes, cancellationToken);

        await _adminActivityLogger.WriteAsync(
            "UploadCmsAsset",
            "CmsAsset",
            assetKey,
            stored.PublicUrl,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "CMS asset '{AssetKey}' uploaded by {UserName}.",
            assetKey,
            _currentUserContext.UserName);

        return new CmsAssetUploadResponse(stored.PublicUrl);
    }

    private static byte[] DecodeBase64(string base64Content)
    {
        try
        {
            return Convert.FromBase64String(base64Content);
        }
        catch (FormatException)
        {
            throw new AppException(ErrorCodes.ValidationFailure, "The image content is not valid base64.", 400);
        }
    }
}
