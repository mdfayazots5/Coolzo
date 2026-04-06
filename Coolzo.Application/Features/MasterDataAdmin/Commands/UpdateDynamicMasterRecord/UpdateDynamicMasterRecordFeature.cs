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

namespace Coolzo.Application.Features.MasterDataAdmin.Commands.UpdateDynamicMasterRecord;

public sealed record UpdateDynamicMasterRecordCommand(
    long DynamicMasterRecordId,
    string MasterType,
    string MasterCode,
    string MasterLabel,
    string MasterValue,
    string? Description,
    bool IsActive,
    bool IsPublished,
    int SortOrder) : IRequest<DynamicMasterRecordResponse>;

public sealed class UpdateDynamicMasterRecordCommandValidator : AbstractValidator<UpdateDynamicMasterRecordCommand>
{
    public UpdateDynamicMasterRecordCommandValidator()
    {
        RuleFor(request => request.DynamicMasterRecordId).GreaterThan(0);
        RuleFor(request => request.MasterType).NotEmpty().MaximumLength(128).Matches("^[A-Za-z0-9_.-]+$");
        RuleFor(request => request.MasterCode).NotEmpty().MaximumLength(128).Matches("^[A-Za-z0-9_.-]+$");
        RuleFor(request => request.MasterLabel).NotEmpty().MaximumLength(160);
        RuleFor(request => request.MasterValue).NotEmpty().MaximumLength(512);
        RuleFor(request => request.Description).MaximumLength(512);
        RuleFor(request => request.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateDynamicMasterRecordCommandHandler : IRequestHandler<UpdateDynamicMasterRecordCommand, DynamicMasterRecordResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<UpdateDynamicMasterRecordCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDynamicMasterRecordCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<UpdateDynamicMasterRecordCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<DynamicMasterRecordResponse> Handle(UpdateDynamicMasterRecordCommand request, CancellationToken cancellationToken)
    {
        var entity = await _adminConfigurationRepository.GetDynamicMasterRecordByIdAsync(request.DynamicMasterRecordId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested dynamic master record could not be found.", 404);

        var duplicate = await _adminConfigurationRepository.GetDynamicMasterRecordByTypeAndCodeAsync(
            request.MasterType.Trim(),
            request.MasterCode.Trim(),
            request.DynamicMasterRecordId,
            cancellationToken);

        if (duplicate is not null)
        {
            throw new AppException(ErrorCodes.DuplicateValue, "A dynamic master already exists for the selected type and code.", 409);
        }

        entity.MasterType = request.MasterType.Trim();
        entity.MasterCode = request.MasterCode.Trim();
        entity.MasterLabel = request.MasterLabel.Trim();
        entity.MasterValue = request.MasterValue.Trim();
        entity.Description = request.Description?.Trim() ?? string.Empty;
        entity.IsActive = request.IsActive;
        entity.IsPublished = request.IsPublished;
        entity.SortOrder = request.SortOrder;
        entity.UpdatedBy = _currentUserContext.UserName;
        entity.LastUpdated = _currentDateTime.UtcNow;
        entity.IPAddress = _currentUserContext.IPAddress;

        await _adminActivityLogger.WriteAsync(
            "UpdateDynamicMasterRecord",
            nameof(DynamicMasterRecord),
            $"{entity.MasterType}:{entity.MasterCode}",
            entity.MasterLabel,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Dynamic master {DynamicMasterRecordId} updated by {UserName}.", entity.DynamicMasterRecordId, _currentUserContext.UserName);

        return AdminResponseMapper.ToResponse(entity);
    }
}
