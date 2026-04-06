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

namespace Coolzo.Application.Features.NotificationTriggerConfiguration.Commands.CreateNotificationTriggerConfiguration;

public sealed record CreateNotificationTriggerConfigurationCommand(
    string TriggerCode,
    string TriggerName,
    string? Description,
    bool IsEnabled,
    bool EmailEnabled,
    bool SmsEnabled,
    bool WhatsAppEnabled,
    bool PushEnabled,
    int ReminderLeadMinutes,
    int DelayMinutes) : IRequest<NotificationTriggerConfigurationResponse>;

public sealed class CreateNotificationTriggerConfigurationCommandValidator : AbstractValidator<CreateNotificationTriggerConfigurationCommand>
{
    public CreateNotificationTriggerConfigurationCommandValidator()
    {
        RuleFor(request => request.TriggerCode).NotEmpty().MaximumLength(128).Matches("^[A-Za-z0-9_.-]+$");
        RuleFor(request => request.TriggerName).NotEmpty().MaximumLength(160);
        RuleFor(request => request.Description).MaximumLength(512);
        RuleFor(request => request.ReminderLeadMinutes).GreaterThanOrEqualTo(0);
        RuleFor(request => request.DelayMinutes).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateNotificationTriggerConfigurationCommandHandler : IRequestHandler<CreateNotificationTriggerConfigurationCommand, NotificationTriggerConfigurationResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CreateNotificationTriggerConfigurationCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateNotificationTriggerConfigurationCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<CreateNotificationTriggerConfigurationCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<NotificationTriggerConfigurationResponse> Handle(CreateNotificationTriggerConfigurationCommand request, CancellationToken cancellationToken)
    {
        var triggerCode = request.TriggerCode.Trim();

        if (await _adminConfigurationRepository.GetNotificationTriggerByCodeAsync(triggerCode, null, cancellationToken) is not null)
        {
            throw new AppException(ErrorCodes.DuplicateValue, "The notification trigger code already exists.", 409);
        }

        var entity = new Coolzo.Domain.Entities.NotificationTriggerConfiguration
        {
            TriggerCode = triggerCode,
            TriggerName = request.TriggerName.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            IsEnabled = request.IsEnabled,
            EmailEnabled = request.EmailEnabled,
            SmsEnabled = request.SmsEnabled,
            WhatsAppEnabled = request.WhatsAppEnabled,
            PushEnabled = request.PushEnabled,
            ReminderLeadMinutes = request.ReminderLeadMinutes,
            DelayMinutes = request.DelayMinutes,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _adminConfigurationRepository.AddNotificationTriggerAsync(entity, cancellationToken);
        await _adminActivityLogger.WriteAsync("CreateNotificationTriggerConfiguration", nameof(Coolzo.Domain.Entities.NotificationTriggerConfiguration), entity.TriggerCode, entity.TriggerName, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Notification trigger {TriggerCode} created by {UserName}.", entity.TriggerCode, _currentUserContext.UserName);

        return AdminResponseMapper.ToResponse(entity);
    }
}
