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

namespace Coolzo.Application.Features.NotificationTriggerConfiguration.Commands.UpdateNotificationTriggerConfiguration;

public sealed record UpdateNotificationTriggerConfigurationCommand(
    long NotificationTriggerConfigurationId,
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

public sealed class UpdateNotificationTriggerConfigurationCommandValidator : AbstractValidator<UpdateNotificationTriggerConfigurationCommand>
{
    public UpdateNotificationTriggerConfigurationCommandValidator()
    {
        RuleFor(request => request.NotificationTriggerConfigurationId).GreaterThan(0);
        RuleFor(request => request.TriggerCode).NotEmpty().MaximumLength(128).Matches("^[A-Za-z0-9_.-]+$");
        RuleFor(request => request.TriggerName).NotEmpty().MaximumLength(160);
        RuleFor(request => request.Description).MaximumLength(512);
        RuleFor(request => request.ReminderLeadMinutes).GreaterThanOrEqualTo(0);
        RuleFor(request => request.DelayMinutes).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateNotificationTriggerConfigurationCommandHandler : IRequestHandler<UpdateNotificationTriggerConfigurationCommand, NotificationTriggerConfigurationResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<UpdateNotificationTriggerConfigurationCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateNotificationTriggerConfigurationCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<UpdateNotificationTriggerConfigurationCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<NotificationTriggerConfigurationResponse> Handle(UpdateNotificationTriggerConfigurationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _adminConfigurationRepository.GetNotificationTriggerByIdAsync(request.NotificationTriggerConfigurationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested notification trigger could not be found.", 404);

        var triggerCode = request.TriggerCode.Trim();

        if (await _adminConfigurationRepository.GetNotificationTriggerByCodeAsync(triggerCode, request.NotificationTriggerConfigurationId, cancellationToken) is not null)
        {
            throw new AppException(ErrorCodes.DuplicateValue, "The notification trigger code already exists.", 409);
        }

        entity.TriggerCode = triggerCode;
        entity.TriggerName = request.TriggerName.Trim();
        entity.Description = request.Description?.Trim() ?? string.Empty;
        entity.IsEnabled = request.IsEnabled;
        entity.EmailEnabled = request.EmailEnabled;
        entity.SmsEnabled = request.SmsEnabled;
        entity.WhatsAppEnabled = request.WhatsAppEnabled;
        entity.PushEnabled = request.PushEnabled;
        entity.ReminderLeadMinutes = request.ReminderLeadMinutes;
        entity.DelayMinutes = request.DelayMinutes;
        entity.UpdatedBy = _currentUserContext.UserName;
        entity.LastUpdated = _currentDateTime.UtcNow;
        entity.IPAddress = _currentUserContext.IPAddress;

        await _adminActivityLogger.WriteAsync("UpdateNotificationTriggerConfiguration", nameof(Coolzo.Domain.Entities.NotificationTriggerConfiguration), entity.TriggerCode, entity.TriggerName, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Notification trigger {NotificationTriggerConfigurationId} updated by {UserName}.", entity.NotificationTriggerConfigurationId, _currentUserContext.UserName);

        return AdminResponseMapper.ToResponse(entity);
    }
}
