using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.MasterDataAdmin.Commands.UploadMasterImage;

public sealed record UploadMasterImageCommand(
    string Folder,
    string FileName,
    string ContentType,
    string Base64Content) : IRequest<MasterImageUploadResponse>;

public sealed class UploadMasterImageCommandValidator : AbstractValidator<UploadMasterImageCommand>
{
    private static readonly string[] AllowedContentTypes =
        { "image/png", "image/jpeg", "image/webp", "image/svg+xml" };

    public UploadMasterImageCommandValidator()
    {
        RuleFor(request => request.Folder).NotEmpty().MaximumLength(64).Matches("^[a-z0-9-]+$")
            .WithMessage("Folder must be a lowercase slug (letters, digits, hyphen).");
        RuleFor(request => request.FileName).NotEmpty().MaximumLength(256);
        RuleFor(request => request.ContentType).NotEmpty()
            .Must(value => AllowedContentTypes.Contains(value.ToLowerInvariant()))
            .WithMessage("Image must be PNG, JPEG, WebP, or SVG.");
        RuleFor(request => request.Base64Content).NotEmpty();
    }
}

public sealed class UploadMasterImageCommandHandler
    : IRequestHandler<UploadMasterImageCommand, MasterImageUploadResponse>
{
    private const int MaxImageBytes = 5 * 1024 * 1024;

    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<UploadMasterImageCommandHandler> _logger;
    private readonly IObjectStorageService _objectStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public UploadMasterImageCommandHandler(
        IObjectStorageService objectStorageService,
        AdminActivityLogger adminActivityLogger,
        IUnitOfWork unitOfWork,
        ICurrentUserContext currentUserContext,
        IAppLogger<UploadMasterImageCommandHandler> logger)
    {
        _objectStorageService = objectStorageService;
        _adminActivityLogger = adminActivityLogger;
        _unitOfWork = unitOfWork;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<MasterImageUploadResponse> Handle(
        UploadMasterImageCommand request,
        CancellationToken cancellationToken)
    {
        var fileBytes = DecodeBase64(request.Base64Content);

        if (fileBytes.Length > MaxImageBytes)
        {
            throw new AppException(ErrorCodes.ValidationFailure, "Image size must not exceed 5 MB.", 400);
        }

        var extension = Path.GetExtension(request.FileName);
        var objectKey = $"catalog/{request.Folder}/{Guid.NewGuid():N}{extension}".ToLowerInvariant();

        var stored = await _objectStorageService.PutObjectAsync(
            objectKey,
            request.ContentType,
            fileBytes,
            cancellationToken);

        await _adminActivityLogger.WriteAsync(
            "UploadMasterImage",
            "DynamicMasterRecord",
            request.Folder,
            stored.PublicUrl,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Master image uploaded to {ObjectKey} by {UserName}.",
            objectKey,
            _currentUserContext.UserName);

        return new MasterImageUploadResponse(stored.PublicUrl);
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
