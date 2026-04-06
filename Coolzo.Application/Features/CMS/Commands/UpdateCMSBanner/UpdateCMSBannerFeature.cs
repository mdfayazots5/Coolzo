using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.CMS.Commands.UpdateCMSBanner;

public sealed record UpdateCMSBannerCommand(
    long CMSBannerId,
    string BannerTitle,
    string? BannerSubtitle,
    string? ImageUrl,
    string? RedirectUrl,
    string? DisplayArea,
    DateOnly? ActiveFromDate,
    DateOnly? ActiveToDate,
    bool IsActive,
    bool IsPublished,
    int SortOrder) : IRequest<CMSBannerResponse>;

public sealed class UpdateCMSBannerCommandValidator : AbstractValidator<UpdateCMSBannerCommand>
{
    public UpdateCMSBannerCommandValidator()
    {
        RuleFor(request => request.CMSBannerId).GreaterThan(0);
        RuleFor(request => request.BannerTitle).NotEmpty().MaximumLength(160);
        RuleFor(request => request.BannerSubtitle).MaximumLength(512);
        RuleFor(request => request.ImageUrl).MaximumLength(512);
        RuleFor(request => request.RedirectUrl).MaximumLength(512);
        RuleFor(request => request.DisplayArea).MaximumLength(64);
        RuleFor(request => request.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(request => request)
            .Must(request => !request.ActiveFromDate.HasValue || !request.ActiveToDate.HasValue || request.ActiveFromDate.Value <= request.ActiveToDate.Value)
            .WithMessage("Banner active dates are invalid.");
    }
}

public sealed class UpdateCMSBannerCommandHandler : IRequestHandler<UpdateCMSBannerCommand, CMSBannerResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<UpdateCMSBannerCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCMSBannerCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<UpdateCMSBannerCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<CMSBannerResponse> Handle(UpdateCMSBannerCommand request, CancellationToken cancellationToken)
    {
        var entity = await _adminConfigurationRepository.GetCmsBannerByIdAsync(request.CMSBannerId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested CMS banner could not be found.", 404);

        entity.BannerTitle = request.BannerTitle.Trim();
        entity.BannerSubtitle = request.BannerSubtitle?.Trim() ?? string.Empty;
        entity.ImageUrl = request.ImageUrl?.Trim() ?? string.Empty;
        entity.RedirectUrl = request.RedirectUrl?.Trim() ?? string.Empty;
        entity.DisplayArea = string.IsNullOrWhiteSpace(request.DisplayArea) ? "Home" : request.DisplayArea.Trim();
        entity.ActiveFromDate = request.ActiveFromDate;
        entity.ActiveToDate = request.ActiveToDate;
        entity.IsActive = request.IsActive;
        entity.IsPublished = request.IsPublished;
        entity.SortOrder = request.SortOrder;
        entity.VersionNumber += 1;
        entity.UpdatedBy = _currentUserContext.UserName;
        entity.LastUpdated = _currentDateTime.UtcNow;
        entity.IPAddress = _currentUserContext.IPAddress;

        await _adminConfigurationRepository.AddCmsContentVersionAsync(
            new CMSContentVersion
            {
                ContentType = "CMSBanner",
                ContentId = entity.CMSBannerId,
                VersionNumber = entity.VersionNumber,
                SnapshotTitle = entity.BannerTitle,
                SnapshotContent = $"{entity.BannerSubtitle}\n{entity.ImageUrl}\n{entity.RedirectUrl}",
                ChangeSummary = "Content updated",
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _adminActivityLogger.WriteAsync("UpdateCMSBanner", nameof(CMSBanner), entity.CMSBannerId.ToString(), entity.BannerTitle, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CMS banner {CMSBannerId} updated by {UserName}.", entity.CMSBannerId, _currentUserContext.UserName);

        return AdminResponseMapper.ToResponse(entity);
    }
}
