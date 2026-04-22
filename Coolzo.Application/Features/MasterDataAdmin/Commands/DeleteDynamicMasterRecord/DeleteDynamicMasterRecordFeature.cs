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

namespace Coolzo.Application.Features.MasterDataAdmin.Commands.DeleteDynamicMasterRecord;

public sealed record DeleteDynamicMasterRecordCommand(long DynamicMasterRecordId) : IRequest<DynamicMasterRecordResponse>;

public sealed class DeleteDynamicMasterRecordCommandValidator : AbstractValidator<DeleteDynamicMasterRecordCommand>
{
    public DeleteDynamicMasterRecordCommandValidator()
    {
        RuleFor(request => request.DynamicMasterRecordId).GreaterThan(0);
    }
}

public sealed class DeleteDynamicMasterRecordCommandHandler : IRequestHandler<DeleteDynamicMasterRecordCommand, DynamicMasterRecordResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<DeleteDynamicMasterRecordCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDynamicMasterRecordCommandHandler(
        IAdminConfigurationRepository adminConfigurationRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<DeleteDynamicMasterRecordCommandHandler> logger)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<DynamicMasterRecordResponse> Handle(DeleteDynamicMasterRecordCommand request, CancellationToken cancellationToken)
    {
        var entity = await _adminConfigurationRepository.GetDynamicMasterRecordByIdAsync(request.DynamicMasterRecordId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested dynamic master record could not be found.", 404);

        entity.IsActive = false;
        entity.IsDeleted = true;
        entity.DateDeleted = _currentDateTime.UtcNow;
        entity.DeletedBy = _currentUserContext.UserName;
        entity.UpdatedBy = _currentUserContext.UserName;
        entity.LastUpdated = _currentDateTime.UtcNow;
        entity.IPAddress = _currentUserContext.IPAddress;

        await _adminActivityLogger.WriteAsync(
            "DeleteDynamicMasterRecord",
            nameof(DynamicMasterRecord),
            $"{entity.MasterType}:{entity.MasterCode}",
            entity.MasterLabel,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Dynamic master {DynamicMasterRecordId} deleted by {UserName}.", entity.DynamicMasterRecordId, _currentUserContext.UserName);

        return AdminResponseMapper.ToResponse(entity);
    }
}
