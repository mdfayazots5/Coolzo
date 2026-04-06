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

namespace Coolzo.Application.Features.NotificationTemplate.Commands.CreateNotificationTemplate;

public sealed record CreateNotificationTemplateCommand(
    string TemplateCode,
    string TemplateName,
    string TriggerCode,
    string Channel,
    string? SubjectTemplate,
    string BodyTemplate,
    IReadOnlyCollection<string> AllowedMergeTags,
    bool IsActive) : IRequest<NotificationTemplateResponse>;

public sealed class CreateNotificationTemplateCommandValidator : AbstractValidator<CreateNotificationTemplateCommand>
{
    public CreateNotificationTemplateCommandValidator()
    {
        RuleFor(request => request.TemplateCode).NotEmpty().MaximumLength(128).Matches("^[A-Za-z0-9_.-]+$");
        RuleFor(request => request.TemplateName).NotEmpty().MaximumLength(160);
        RuleFor(request => request.TriggerCode).NotEmpty().MaximumLength(128).Matches("^[A-Za-z0-9_.-]+$");
        RuleFor(request => request.Channel).NotEmpty().MaximumLength(32).Matches("^(email|sms|whatsapp|push)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        RuleFor(request => request.SubjectTemplate).MaximumLength(512);
        RuleFor(request => request.BodyTemplate).NotEmpty().MaximumLength(4000);
    }
}

public sealed class CreateNotificationTemplateCommandHandler : IRequestHandler<CreateNotificationTemplateCommand, NotificationTemplateResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CreateNotificationTemplateCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateNotificationTemplateCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<CreateNotificationTemplateCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<NotificationTemplateResponse> Handle(CreateNotificationTemplateCommand request, CancellationToken cancellationToken)
    {
        var templateCode = request.TemplateCode.Trim();

        if (await _adminConfigurationRepository.GetNotificationTemplateByCodeAsync(templateCode, null, cancellationToken) is not null)
        {
            throw new AppException(ErrorCodes.DuplicateValue, "The notification template code already exists.", 409);
        }

        var allowedMergeTags = NotificationTemplateMergeTagCatalog.Normalize(request.AllowedMergeTags);
        NotificationTemplateMergeTagCatalog.ValidateAllowedTags(allowedMergeTags);
        NotificationTemplateMergeTagCatalog.ValidateTemplateTokens(request.SubjectTemplate, request.BodyTemplate);

        var entity = new Coolzo.Domain.Entities.NotificationTemplate
        {
            TemplateCode = templateCode,
            TemplateName = request.TemplateName.Trim(),
            TriggerCode = request.TriggerCode.Trim(),
            Channel = request.Channel.Trim().ToLowerInvariant(),
            SubjectTemplate = request.SubjectTemplate?.Trim() ?? string.Empty,
            BodyTemplate = request.BodyTemplate.Trim(),
            AllowedMergeTags = string.Join(", ", allowedMergeTags),
            IsActive = request.IsActive,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _adminConfigurationRepository.AddNotificationTemplateAsync(entity, cancellationToken);
        await _adminActivityLogger.WriteAsync("CreateNotificationTemplate", nameof(Coolzo.Domain.Entities.NotificationTemplate), entity.TemplateCode, entity.TemplateName, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Notification template {TemplateCode} created by {UserName}.", entity.TemplateCode, _currentUserContext.UserName);

        return AdminResponseMapper.ToResponse(entity);
    }
}
