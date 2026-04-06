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

namespace Coolzo.Application.Features.CMS.Commands.UpdateCMSFaq;

public sealed record UpdateCMSFaqCommand(
    long CMSFaqId,
    string Category,
    string Question,
    string Answer,
    bool IsActive,
    bool IsPublished,
    int SortOrder) : IRequest<CMSFaqResponse>;

public sealed class UpdateCMSFaqCommandValidator : AbstractValidator<UpdateCMSFaqCommand>
{
    public UpdateCMSFaqCommandValidator()
    {
        RuleFor(request => request.CMSFaqId).GreaterThan(0);
        RuleFor(request => request.Category).NotEmpty().MaximumLength(128);
        RuleFor(request => request.Question).NotEmpty().MaximumLength(512);
        RuleFor(request => request.Answer).NotEmpty().MaximumLength(4000);
        RuleFor(request => request.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateCMSFaqCommandHandler : IRequestHandler<UpdateCMSFaqCommand, CMSFaqResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<UpdateCMSFaqCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCMSFaqCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<UpdateCMSFaqCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<CMSFaqResponse> Handle(UpdateCMSFaqCommand request, CancellationToken cancellationToken)
    {
        var entity = await _adminConfigurationRepository.GetCmsFaqByIdAsync(request.CMSFaqId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested CMS FAQ could not be found.", 404);

        entity.Category = request.Category.Trim();
        entity.Question = request.Question.Trim();
        entity.Answer = request.Answer.Trim();
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
                ContentType = "CMSFaq",
                ContentId = entity.CMSFaqId,
                VersionNumber = entity.VersionNumber,
                SnapshotTitle = entity.Question,
                SnapshotContent = entity.Answer,
                ChangeSummary = "Content updated",
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _adminActivityLogger.WriteAsync("UpdateCMSFaq", nameof(CMSFaq), entity.CMSFaqId.ToString(), entity.Question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CMS FAQ {CMSFaqId} updated by {UserName}.", entity.CMSFaqId, _currentUserContext.UserName);

        return AdminResponseMapper.ToResponse(entity);
    }
}
