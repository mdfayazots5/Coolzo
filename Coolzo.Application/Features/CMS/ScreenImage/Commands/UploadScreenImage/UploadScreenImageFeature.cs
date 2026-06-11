using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Application.Features.CMS.ScreenImage.Common;
using Coolzo.Contracts.Responses.CMS;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.CMS.ScreenImage.Commands.UploadScreenImage;

public sealed record UploadScreenImageCommand(
    long ScreenImageSlotId,
    string FileName,
    string ContentType,
    string Base64Content,
    string? AltText) : IRequest<ScreenImageSlotResponse>;

public sealed class UploadScreenImageCommandValidator : AbstractValidator<UploadScreenImageCommand>
{
    private static readonly string[] AllowedContentTypes =
        { "image/png", "image/jpeg", "image/webp", "image/svg+xml" };

    public UploadScreenImageCommandValidator()
    {
        RuleFor(request => request.ScreenImageSlotId).GreaterThan(0);
        RuleFor(request => request.FileName).NotEmpty().MaximumLength(256);
        RuleFor(request => request.ContentType).NotEmpty()
            .Must(value => AllowedContentTypes.Contains(value.ToLowerInvariant()))
            .WithMessage("Image must be PNG, JPEG, WebP, or SVG.");
        RuleFor(request => request.Base64Content).NotEmpty();
        RuleFor(request => request.AltText).MaximumLength(256);
    }
}

public sealed class UploadScreenImageCommandHandler
    : IRequestHandler<UploadScreenImageCommand, ScreenImageSlotResponse>
{
    private const int MaxImageBytes = 5 * 1024 * 1024;

    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<UploadScreenImageCommandHandler> _logger;
    private readonly IObjectStorageService _objectStorageService;
    private readonly IScreenImageSlotRepository _screenImageSlotRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UploadScreenImageCommandHandler(
        IScreenImageSlotRepository screenImageSlotRepository,
        IObjectStorageService objectStorageService,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<UploadScreenImageCommandHandler> logger)
    {
        _screenImageSlotRepository = screenImageSlotRepository;
        _objectStorageService = objectStorageService;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<ScreenImageSlotResponse> Handle(
        UploadScreenImageCommand request,
        CancellationToken cancellationToken)
    {
        var slot = await _screenImageSlotRepository.GetByIdAsync(request.ScreenImageSlotId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "Screen image slot was not found.", 404);

        var fileBytes = DecodeBase64(request.Base64Content);

        if (fileBytes.Length > MaxImageBytes)
        {
            throw new AppException(ErrorCodes.ValidationFailure, "Image size must not exceed 5 MB.", 400);
        }

        var extension = Path.GetExtension(request.FileName);
        var objectKey = $"cms/images/{slot.PageKey}/{slot.SlotKey}-{slot.Breakpoint}-{Guid.NewGuid():N}{extension}".ToLowerInvariant();

        var stored = await _objectStorageService.PutObjectAsync(objectKey, request.ContentType, fileBytes, cancellationToken);

        var now = _currentDateTime.UtcNow;
        slot.ImageUrl = stored.PublicUrl;

        if (!string.IsNullOrWhiteSpace(request.AltText))
        {
            slot.AltText = request.AltText.Trim();
        }

        slot.UpdatedBy = _currentUserContext.UserName;
        slot.LastUpdated = now;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _adminActivityLogger.WriteAsync(
            "UploadScreenImage",
            nameof(Coolzo.Domain.Entities.ScreenImageSlot),
            slot.ScreenImageSlotId.ToString(),
            stored.PublicUrl,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Screen image uploaded for {PageKey}.{SlotKey}.{Breakpoint} by {UserName}.",
            slot.PageKey,
            slot.SlotKey,
            slot.Breakpoint,
            _currentUserContext.UserName);

        return ScreenImageSlotMapper.ToResponse(slot);
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
