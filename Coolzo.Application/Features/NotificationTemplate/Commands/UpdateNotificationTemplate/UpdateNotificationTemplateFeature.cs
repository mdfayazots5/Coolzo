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

namespace Coolzo.Application.Features.NotificationTemplate.Commands.UpdateNotificationTemplate;

public sealed record UpdateNotificationTemplateCommand(
    long NotificationTemplateId,
    string TemplateCode,
    string TemplateName,
    string TriggerCode,
    string Channel,
    string? SubjectTemplate,
    string BodyTemplate,
    IReadOnlyCollection<string> AllowedMergeTags,
    bool IsActive) : IRequest<NotificationTemplateResponse>;

public sealed class UpdateNotificationTemplateCommandValidator : AbstractValidator<UpdateNotificationTemplateCommand>
{
    public UpdateNotificationTemplateCommandValidator()
    {
        RuleFor(request => request.NotificationTemplateId).GreaterThan(0);
        RuleFor(request => request.TemplateCode).NotEmpty().MaximumLength(128).Matches("^[A-Za-z0-9_.-]+$");
        RuleFor(request => request.TemplateName).NotEmpty().MaximumLength(160);
        RuleFor(request => request.TriggerCode).NotEmpty().MaximumLength(128).Matches("^[A-Za-z0-9_.-]+$");
        RuleFor(request => request.Channel).NotEmpty().MaximumLength(32).Matches("^(email|sms|whatsapp|push)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        RuleFor(request => request.SubjectTemplate).MaximumLength(512);
        RuleFor(request => request.BodyTemplate).NotEmpty().MaximumLength(4000);
    }
}

public sealed class UpdateNotificationTemplateCommandHandler : IRequestHandler<UpdateNotificationTemplateCommand, NotificationTemplateResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<UpdateNotificationTemplateCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateNotificationTemplateCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<UpdateNotificationTemplateCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<NotificationTemplateResponse> Handle(UpdateNotificationTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = await _adminConfigurationRepository.GetNotificationTemplateByIdAsync(request.NotificationTemplateId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested notification template could not be found.", 404);

        var templateCode = request.TemplateCode.Trim();

        if (await _adminConfigurationRepository.GetNotificationTemplateByCodeAsync(templateCode, request.NotificationTemplateId, cancellationToken) is not null)
        {
            throw new AppException(ErrorCodes.DuplicateValue, "The notification template code already exists.", 409);
        }

        var allowedMergeTags = NotificationTemplateMergeTagCatalog.Normalize(request.AllowedMergeTags);
        NotificationTemplateMergeTagCatalog.ValidateAllowedTags(allowedMergeTags);
        NotificationTemplateMergeTagCatalog.ValidateTemplateTokens(request.SubjectTemplate, request.BodyTemplate);

        entity.TemplateCode = templateCode;
        entity.TemplateName = request.TemplateName.Trim();
        entity.TriggerCode = request.TriggerCode.Trim();
        entity.Channel = request.Channel.Trim().ToLowerInvariant();
        entity.SubjectTemplate = request.SubjectTemplate?.Trim() ?? string.Empty;
        entity.BodyTemplate = request.BodyTemplate.Trim();
        entity.AllowedMergeTags = string.Join(", ", allowedMergeTags);
        entity.IsActive = request.IsActive;
        entity.UpdatedBy = _currentUserContext.UserName;
        entity.LastUpdated = _currentDateTime.UtcNow;
        entity.IPAddress = _currentUserContext.IPAddress;

        await _adminActivityLogger.WriteAsync("UpdateNotificationTemplate", nameof(Coolzo.Domain.Entities.NotificationTemplate), entity.TemplateCode, entity.TemplateName, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Notification template {NotificationTemplateId} updated by {UserName}.", entity.NotificationTemplateId, _currentUserContext.UserName);

        return AdminResponseMapper.ToResponse(entity);
    }
}
