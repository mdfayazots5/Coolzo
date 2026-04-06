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

namespace Coolzo.Application.Features.SystemConfiguration.Commands.CreateSystemConfiguration;

public sealed record CreateSystemConfigurationCommand(
    string ConfigurationGroup,
    string ConfigurationKey,
    string ConfigurationValue,
    string ValueType,
    string? Description,
    bool IsSensitive,
    bool IsActive) : IRequest<SystemConfigurationResponse>;

public sealed class CreateSystemConfigurationCommandValidator : AbstractValidator<CreateSystemConfigurationCommand>
{
    public CreateSystemConfigurationCommandValidator()
    {
        RuleFor(request => request.ConfigurationGroup).NotEmpty().MaximumLength(128);
        RuleFor(request => request.ConfigurationKey).NotEmpty().MaximumLength(128);
        RuleFor(request => request.ConfigurationValue).NotEmpty().MaximumLength(1024);
        RuleFor(request => request.ValueType).NotEmpty().MaximumLength(64);
        RuleFor(request => request.Description).MaximumLength(512);
    }
}

public sealed class CreateSystemConfigurationCommandHandler : IRequestHandler<CreateSystemConfigurationCommand, SystemConfigurationResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CreateSystemConfigurationCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSystemConfigurationCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<CreateSystemConfigurationCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<SystemConfigurationResponse> Handle(CreateSystemConfigurationCommand request, CancellationToken cancellationToken)
    {
        var duplicate = await _adminConfigurationRepository.GetSystemConfigurationByGroupAndKeyAsync(
            request.ConfigurationGroup.Trim(),
            request.ConfigurationKey.Trim(),
            null,
            cancellationToken);

        if (duplicate is not null)
        {
            throw new AppException(ErrorCodes.DuplicateValue, "The system configuration key already exists in the selected group.", 409);
        }

        var entity = new Coolzo.Domain.Entities.SystemConfiguration
        {
            ConfigurationGroup = request.ConfigurationGroup.Trim(),
            ConfigurationKey = request.ConfigurationKey.Trim(),
            ConfigurationValue = request.ConfigurationValue.Trim(),
            ValueType = request.ValueType.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            IsSensitive = request.IsSensitive,
            IsActive = request.IsActive,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _adminConfigurationRepository.AddSystemConfigurationAsync(entity, cancellationToken);
        await _adminActivityLogger.WriteAsync(
            "CreateSystemConfiguration",
            nameof(Coolzo.Domain.Entities.SystemConfiguration),
            $"{entity.ConfigurationGroup}:{entity.ConfigurationKey}",
            entity.IsSensitive ? "Sensitive value created." : entity.ConfigurationValue,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("System configuration {ConfigurationGroup}:{ConfigurationKey} created by {UserName}.", entity.ConfigurationGroup, entity.ConfigurationKey, _currentUserContext.UserName);

        return AdminResponseMapper.ToResponse(entity);
    }
}
