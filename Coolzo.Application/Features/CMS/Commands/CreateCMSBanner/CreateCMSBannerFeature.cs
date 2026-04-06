using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.CMS.Commands.CreateCMSBanner;

public sealed record CreateCMSBannerCommand(
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

public sealed class CreateCMSBannerCommandValidator : AbstractValidator<CreateCMSBannerCommand>
{
    public CreateCMSBannerCommandValidator()
    {
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

public sealed class CreateCMSBannerCommandHandler : IRequestHandler<CreateCMSBannerCommand, CMSBannerResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CreateCMSBannerCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCMSBannerCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<CreateCMSBannerCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<CMSBannerResponse> Handle(CreateCMSBannerCommand request, CancellationToken cancellationToken)
    {
        var entity = new CMSBanner
        {
            BannerTitle = request.BannerTitle.Trim(),
            BannerSubtitle = request.BannerSubtitle?.Trim() ?? string.Empty,
            ImageUrl = request.ImageUrl?.Trim() ?? string.Empty,
            RedirectUrl = request.RedirectUrl?.Trim() ?? string.Empty,
            DisplayArea = string.IsNullOrWhiteSpace(request.DisplayArea) ? "Home" : request.DisplayArea.Trim(),
            ActiveFromDate = request.ActiveFromDate,
            ActiveToDate = request.ActiveToDate,
            IsActive = request.IsActive,
            IsPublished = request.IsPublished,
            SortOrder = request.SortOrder,
            VersionNumber = 1,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _adminConfigurationRepository.AddCmsBannerAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _adminConfigurationRepository.AddCmsContentVersionAsync(
            new CMSContentVersion
            {
                ContentType = "CMSBanner",
                ContentId = entity.CMSBannerId,
                VersionNumber = entity.VersionNumber,
                SnapshotTitle = entity.BannerTitle,
                SnapshotContent = $"{entity.BannerSubtitle}\n{entity.ImageUrl}\n{entity.RedirectUrl}",
                ChangeSummary = "Initial version",
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _adminActivityLogger.WriteAsync("CreateCMSBanner", nameof(CMSBanner), entity.CMSBannerId.ToString(), entity.BannerTitle, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CMS banner {CMSBannerId} created by {UserName}.", entity.CMSBannerId, _currentUserContext.UserName);

        return AdminResponseMapper.ToResponse(entity);
    }
}
