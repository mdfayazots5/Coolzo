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

namespace Coolzo.Application.Features.CMS.Commands.CreateCMSBlock;

public sealed record CreateCMSBlockCommand(
    string BlockKey,
    string Title,
    string? Summary,
    string Content,
    string? PreviewImageUrl,
    bool IsActive,
    bool IsPublished,
    int SortOrder) : IRequest<CMSBlockResponse>;

public sealed class CreateCMSBlockCommandValidator : AbstractValidator<CreateCMSBlockCommand>
{
    public CreateCMSBlockCommandValidator()
    {
        RuleFor(request => request.BlockKey).NotEmpty().MaximumLength(128).Matches("^[A-Za-z0-9_.-]+$");
        RuleFor(request => request.Title).NotEmpty().MaximumLength(160);
        RuleFor(request => request.Summary).MaximumLength(512);
        RuleFor(request => request.Content).NotEmpty().MaximumLength(4000);
        RuleFor(request => request.PreviewImageUrl).MaximumLength(512);
        RuleFor(request => request.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateCMSBlockCommandHandler : IRequestHandler<CreateCMSBlockCommand, CMSBlockResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CreateCMSBlockCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCMSBlockCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<CreateCMSBlockCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<CMSBlockResponse> Handle(CreateCMSBlockCommand request, CancellationToken cancellationToken)
    {
        if (await _adminConfigurationRepository.CmsBlockKeyExistsAsync(request.BlockKey.Trim(), null, cancellationToken))
        {
            throw new AppException(ErrorCodes.DuplicateValue, "The CMS block key already exists.", 409);
        }

        var entity = new CMSBlock
        {
            BlockKey = request.BlockKey.Trim(),
            Title = request.Title.Trim(),
            Summary = request.Summary?.Trim() ?? string.Empty,
            Content = request.Content.Trim(),
            PreviewImageUrl = request.PreviewImageUrl?.Trim() ?? string.Empty,
            IsActive = request.IsActive,
            IsPublished = request.IsPublished,
            SortOrder = request.SortOrder,
            VersionNumber = 1,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _adminConfigurationRepository.AddCmsBlockAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _adminConfigurationRepository.AddCmsContentVersionAsync(
            CreateVersionRecord("CMSBlock", entity.CMSBlockId, entity.VersionNumber, entity.Title, entity.Content),
            cancellationToken);
        await _adminActivityLogger.WriteAsync("CreateCMSBlock", nameof(CMSBlock), entity.BlockKey, entity.Title, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CMS block {BlockKey} created by {UserName}.", entity.BlockKey, _currentUserContext.UserName);

        return AdminResponseMapper.ToResponse(entity);
    }

    private CMSContentVersion CreateVersionRecord(string contentType, long contentId, int versionNumber, string title, string content)
    {
        return new CMSContentVersion
        {
            ContentType = contentType,
            ContentId = contentId,
            VersionNumber = versionNumber,
            SnapshotTitle = title,
            SnapshotContent = content,
            ChangeSummary = "Initial version",
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };
    }
}
