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

namespace Coolzo.Application.Features.SystemConfiguration.Commands.UpdateSystemConfiguration;

public sealed record UpdateSystemConfigurationCommand(
    long SystemConfigurationId,
    string ConfigurationGroup,
    string ConfigurationKey,
    string ConfigurationValue,
    string ValueType,
    string? Description,
    bool IsSensitive,
    bool IsActive) : IRequest<SystemConfigurationResponse>;

public sealed class UpdateSystemConfigurationCommandValidator : AbstractValidator<UpdateSystemConfigurationCommand>
{
    public UpdateSystemConfigurationCommandValidator()
    {
        RuleFor(request => request.SystemConfigurationId).GreaterThan(0);
        RuleFor(request => request.ConfigurationGroup).NotEmpty().MaximumLength(128);
        RuleFor(request => request.ConfigurationKey).NotEmpty().MaximumLength(128);
        RuleFor(request => request.ConfigurationValue).NotEmpty().MaximumLength(1024);
        RuleFor(request => request.ValueType).NotEmpty().MaximumLength(64);
        RuleFor(request => request.Description).MaximumLength(512);
    }
}

public sealed class UpdateSystemConfigurationCommandHandler : IRequestHandler<UpdateSystemConfigurationCommand, SystemConfigurationResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<UpdateSystemConfigurationCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSystemConfigurationCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<UpdateSystemConfigurationCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<SystemConfigurationResponse> Handle(UpdateSystemConfigurationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _adminConfigurationRepository.GetSystemConfigurationByIdAsync(request.SystemConfigurationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested system configuration could not be found.", 404);

        var duplicate = await _adminConfigurationRepository.GetSystemConfigurationByGroupAndKeyAsync(
            request.ConfigurationGroup.Trim(),
            request.ConfigurationKey.Trim(),
            request.SystemConfigurationId,
            cancellationToken);

        if (duplicate is not null)
        {
            throw new AppException(ErrorCodes.DuplicateValue, "The system configuration key already exists in the selected group.", 409);
        }

        entity.ConfigurationGroup = request.ConfigurationGroup.Trim();
        entity.ConfigurationKey = request.ConfigurationKey.Trim();
        entity.ConfigurationValue = request.ConfigurationValue.Trim();
        entity.ValueType = request.ValueType.Trim();
        entity.Description = request.Description?.Trim() ?? string.Empty;
        entity.IsSensitive = request.IsSensitive;
        entity.IsActive = request.IsActive;
        entity.UpdatedBy = _currentUserContext.UserName;
        entity.LastUpdated = _currentDateTime.UtcNow;
        entity.IPAddress = _currentUserContext.IPAddress;

        await _adminActivityLogger.WriteAsync(
            "UpdateSystemConfiguration",
            nameof(Coolzo.Domain.Entities.SystemConfiguration),
            $"{entity.ConfigurationGroup}:{entity.ConfigurationKey}",
            entity.IsSensitive ? "Sensitive value updated." : entity.ConfigurationValue,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("System configuration {SystemConfigurationId} updated by {UserName}.", entity.SystemConfigurationId, _currentUserContext.UserName);

        return AdminResponseMapper.ToResponse(entity);
    }
}
