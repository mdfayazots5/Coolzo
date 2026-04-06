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

namespace Coolzo.Application.Features.MasterDataAdmin.Commands.CreateDynamicMasterRecord;

public sealed record CreateDynamicMasterRecordCommand(
    string MasterType,
    string MasterCode,
    string MasterLabel,
    string MasterValue,
    string? Description,
    bool IsActive,
    bool IsPublished,
    int SortOrder) : IRequest<DynamicMasterRecordResponse>;

public sealed class CreateDynamicMasterRecordCommandValidator : AbstractValidator<CreateDynamicMasterRecordCommand>
{
    public CreateDynamicMasterRecordCommandValidator()
    {
        RuleFor(request => request.MasterType).NotEmpty().MaximumLength(128).Matches("^[A-Za-z0-9_.-]+$");
        RuleFor(request => request.MasterCode).NotEmpty().MaximumLength(128).Matches("^[A-Za-z0-9_.-]+$");
        RuleFor(request => request.MasterLabel).NotEmpty().MaximumLength(160);
        RuleFor(request => request.MasterValue).NotEmpty().MaximumLength(512);
        RuleFor(request => request.Description).MaximumLength(512);
        RuleFor(request => request.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateDynamicMasterRecordCommandHandler : IRequestHandler<CreateDynamicMasterRecordCommand, DynamicMasterRecordResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CreateDynamicMasterRecordCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDynamicMasterRecordCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<CreateDynamicMasterRecordCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<DynamicMasterRecordResponse> Handle(CreateDynamicMasterRecordCommand request, CancellationToken cancellationToken)
    {
        var duplicate = await _adminConfigurationRepository.GetDynamicMasterRecordByTypeAndCodeAsync(
            request.MasterType.Trim(),
            request.MasterCode.Trim(),
            null,
            cancellationToken);

        if (duplicate is not null)
        {
            throw new AppException(ErrorCodes.DuplicateValue, "A dynamic master already exists for the selected type and code.", 409);
        }

        var entity = new DynamicMasterRecord
        {
            MasterType = request.MasterType.Trim(),
            MasterCode = request.MasterCode.Trim(),
            MasterLabel = request.MasterLabel.Trim(),
            MasterValue = request.MasterValue.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            IsActive = request.IsActive,
            IsPublished = request.IsPublished,
            SortOrder = request.SortOrder,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _adminConfigurationRepository.AddDynamicMasterRecordAsync(entity, cancellationToken);
        await _adminActivityLogger.WriteAsync(
            "CreateDynamicMasterRecord",
            nameof(DynamicMasterRecord),
            $"{entity.MasterType}:{entity.MasterCode}",
            entity.MasterLabel,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Dynamic master {MasterType}:{MasterCode} created by {UserName}.", entity.MasterType, entity.MasterCode, _currentUserContext.UserName);

        return AdminResponseMapper.ToResponse(entity);
    }
}
