using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Application.Features.CMS.ScreenImage.Common;
using Coolzo.Contracts.Responses.CMS;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.CMS.ScreenImage.Commands.UpsertScreenImageSlot;

public sealed record UpsertScreenImageSlotCommand(
    long? ScreenImageSlotId,
    string PageKey,
    string SlotKey,
    string Breakpoint,
    int RecommendedWidth,
    int RecommendedHeight,
    string? AltText,
    string? SuggestedAIPrompt,
    bool IsActive) : IRequest<ScreenImageSlotResponse>;

public sealed class UpsertScreenImageSlotCommandValidator : AbstractValidator<UpsertScreenImageSlotCommand>
{
    public UpsertScreenImageSlotCommandValidator()
    {
        RuleFor(request => request.PageKey).NotEmpty().MaximumLength(64).Matches("^[A-Za-z0-9_.-]+$");
        RuleFor(request => request.SlotKey).NotEmpty().MaximumLength(64).Matches("^[A-Za-z0-9_.-]+$");
        RuleFor(request => request.Breakpoint).NotEmpty().Must(value => SnapshotKeys.Breakpoints.All.Contains(value))
            .WithMessage("Breakpoint must be one of: desktop, tablet, mobile.");
        RuleFor(request => request.RecommendedWidth).GreaterThanOrEqualTo(0);
        RuleFor(request => request.RecommendedHeight).GreaterThanOrEqualTo(0);
        RuleFor(request => request.AltText).MaximumLength(256);
        RuleFor(request => request.SuggestedAIPrompt).MaximumLength(1024);
    }
}

public sealed class UpsertScreenImageSlotCommandHandler
    : IRequestHandler<UpsertScreenImageSlotCommand, ScreenImageSlotResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<UpsertScreenImageSlotCommandHandler> _logger;
    private readonly IScreenImageSlotRepository _screenImageSlotRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpsertScreenImageSlotCommandHandler(
        IScreenImageSlotRepository screenImageSlotRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<UpsertScreenImageSlotCommandHandler> logger)
    {
        _screenImageSlotRepository = screenImageSlotRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<ScreenImageSlotResponse> Handle(
        UpsertScreenImageSlotCommand request,
        CancellationToken cancellationToken)
    {
        var pageKey = request.PageKey.Trim();
        var slotKey = request.SlotKey.Trim();
        var breakpoint = request.Breakpoint.Trim().ToLowerInvariant();
        var now = _currentDateTime.UtcNow;

        if (await _screenImageSlotRepository.ExistsAsync(pageKey, slotKey, breakpoint, request.ScreenImageSlotId, cancellationToken))
        {
            throw new AppException(ErrorCodes.DuplicateValue, "A slot for this page/slot/breakpoint already exists.", 409);
        }

        ScreenImageSlot entity;

        if (request.ScreenImageSlotId.HasValue)
        {
            entity = await _screenImageSlotRepository.GetByIdAsync(request.ScreenImageSlotId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.NotFound, "Screen image slot was not found.", 404);

            entity.PageKey = pageKey;
            entity.SlotKey = slotKey;
            entity.Breakpoint = breakpoint;
            entity.RecommendedWidth = request.RecommendedWidth;
            entity.RecommendedHeight = request.RecommendedHeight;
            entity.AltText = request.AltText?.Trim() ?? string.Empty;
            entity.SuggestedAIPrompt = request.SuggestedAIPrompt?.Trim() ?? string.Empty;
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = _currentUserContext.UserName;
            entity.LastUpdated = now;
        }
        else
        {
            entity = new ScreenImageSlot
            {
                PageKey = pageKey,
                SlotKey = slotKey,
                Breakpoint = breakpoint,
                RecommendedWidth = request.RecommendedWidth,
                RecommendedHeight = request.RecommendedHeight,
                AltText = request.AltText?.Trim() ?? string.Empty,
                SuggestedAIPrompt = request.SuggestedAIPrompt?.Trim() ?? string.Empty,
                ImageUrl = string.Empty,
                IsActive = request.IsActive,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            };

            await _screenImageSlotRepository.AddAsync(entity, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _adminActivityLogger.WriteAsync(
            "UpsertScreenImageSlot",
            nameof(ScreenImageSlot),
            entity.ScreenImageSlotId.ToString(),
            entity.AltText,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Screen image slot {PageKey}.{SlotKey}.{Breakpoint} upserted by {UserName}.",
            pageKey,
            slotKey,
            breakpoint,
            _currentUserContext.UserName);

        return ScreenImageSlotMapper.ToResponse(entity);
    }
}
