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

namespace Coolzo.Application.Features.CMS.Commands.UpdateCMSBlock;

public sealed record UpdateCMSBlockCommand(
    long CMSBlockId,
    string BlockKey,
    string Title,
    string? Summary,
    string Content,
    string? PreviewImageUrl,
    bool IsActive,
    bool IsPublished,
    int SortOrder) : IRequest<CMSBlockResponse>;

public sealed class UpdateCMSBlockCommandValidator : AbstractValidator<UpdateCMSBlockCommand>
{
    public UpdateCMSBlockCommandValidator()
    {
        RuleFor(request => request.CMSBlockId).GreaterThan(0);
        RuleFor(request => request.BlockKey).NotEmpty().MaximumLength(128).Matches("^[A-Za-z0-9_.-]+$");
        RuleFor(request => request.Title).NotEmpty().MaximumLength(160);
        RuleFor(request => request.Summary).MaximumLength(512);
        RuleFor(request => request.Content).NotEmpty().MaximumLength(4000);
        RuleFor(request => request.PreviewImageUrl).MaximumLength(512);
        RuleFor(request => request.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateCMSBlockCommandHandler : IRequestHandler<UpdateCMSBlockCommand, CMSBlockResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<UpdateCMSBlockCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCMSBlockCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<UpdateCMSBlockCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<CMSBlockResponse> Handle(UpdateCMSBlockCommand request, CancellationToken cancellationToken)
    {
        var entity = await _adminConfigurationRepository.GetCmsBlockByIdAsync(request.CMSBlockId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested CMS block could not be found.", 404);

        if (await _adminConfigurationRepository.CmsBlockKeyExistsAsync(request.BlockKey.Trim(), request.CMSBlockId, cancellationToken))
        {
            throw new AppException(ErrorCodes.DuplicateValue, "The CMS block key already exists.", 409);
        }

        entity.BlockKey = request.BlockKey.Trim();
        entity.Title = request.Title.Trim();
        entity.Summary = request.Summary?.Trim() ?? string.Empty;
        entity.Content = request.Content.Trim();
        entity.PreviewImageUrl = request.PreviewImageUrl?.Trim() ?? string.Empty;
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
                ContentType = "CMSBlock",
                ContentId = entity.CMSBlockId,
                VersionNumber = entity.VersionNumber,
                SnapshotTitle = entity.Title,
                SnapshotContent = entity.Content,
                ChangeSummary = "Content updated",
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _adminActivityLogger.WriteAsync("UpdateCMSBlock", nameof(CMSBlock), entity.BlockKey, entity.Title, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CMS block {CMSBlockId} updated by {UserName}.", entity.CMSBlockId, _currentUserContext.UserName);

        return AdminResponseMapper.ToResponse(entity);
    }
}
