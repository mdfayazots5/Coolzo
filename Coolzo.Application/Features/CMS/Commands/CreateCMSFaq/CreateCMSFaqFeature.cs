using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.CMS.Commands.CreateCMSFaq;

public sealed record CreateCMSFaqCommand(
    string Category,
    string Question,
    string Answer,
    bool IsActive,
    bool IsPublished,
    int SortOrder) : IRequest<CMSFaqResponse>;

public sealed class CreateCMSFaqCommandValidator : AbstractValidator<CreateCMSFaqCommand>
{
    public CreateCMSFaqCommandValidator()
    {
        RuleFor(request => request.Category).NotEmpty().MaximumLength(128);
        RuleFor(request => request.Question).NotEmpty().MaximumLength(512);
        RuleFor(request => request.Answer).NotEmpty().MaximumLength(4000);
        RuleFor(request => request.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateCMSFaqCommandHandler : IRequestHandler<CreateCMSFaqCommand, CMSFaqResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CreateCMSFaqCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCMSFaqCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<CreateCMSFaqCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<CMSFaqResponse> Handle(CreateCMSFaqCommand request, CancellationToken cancellationToken)
    {
        var entity = new CMSFaq
        {
            Category = request.Category.Trim(),
            Question = request.Question.Trim(),
            Answer = request.Answer.Trim(),
            IsActive = request.IsActive,
            IsPublished = request.IsPublished,
            SortOrder = request.SortOrder,
            VersionNumber = 1,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _adminConfigurationRepository.AddCmsFaqAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _adminConfigurationRepository.AddCmsContentVersionAsync(
            new CMSContentVersion
            {
                ContentType = "CMSFaq",
                ContentId = entity.CMSFaqId,
                VersionNumber = entity.VersionNumber,
                SnapshotTitle = entity.Question,
                SnapshotContent = entity.Answer,
                ChangeSummary = "Initial version",
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _adminActivityLogger.WriteAsync("CreateCMSFaq", nameof(CMSFaq), entity.CMSFaqId.ToString(), entity.Question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CMS FAQ {CMSFaqId} created by {UserName}.", entity.CMSFaqId, _currentUserContext.UserName);

        return AdminResponseMapper.ToResponse(entity);
    }
}
