using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.GapPhaseA;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.GapPhaseA.PartsReturn;

public sealed record CreatePartsReturnCommand(
    long ItemId,
    decimal Quantity,
    string ReasonCode,
    string DefectDescription,
    long? TechnicianId,
    long? JobCardId) : IRequest<PartsReturnResponse>;

public sealed class CreatePartsReturnCommandValidator : AbstractValidator<CreatePartsReturnCommand>
{
    public CreatePartsReturnCommandValidator()
    {
        RuleFor(request => request.ItemId).GreaterThan(0);
        RuleFor(request => request.Quantity).GreaterThan(0.00m);
        RuleFor(request => request.ReasonCode).NotEmpty().MaximumLength(64);
        RuleFor(request => request.DefectDescription).NotEmpty().MaximumLength(512);
    }
}

public sealed class CreatePartsReturnCommandHandler : IRequestHandler<CreatePartsReturnCommand, PartsReturnResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly IGapPhaseAReferenceGenerator _referenceGenerator;
    private readonly IGapPhaseARepository _repository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePartsReturnCommandHandler(
        IInventoryRepository inventoryRepository,
        IGapPhaseARepository repository,
        IGapPhaseAReferenceGenerator referenceGenerator,
        GapPhaseAFeatureFlagService featureFlagService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _inventoryRepository = inventoryRepository;
        _repository = repository;
        _referenceGenerator = referenceGenerator;
        _featureFlagService = featureFlagService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<PartsReturnResponse> Handle(CreatePartsReturnCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.partsreturn.enabled", cancellationToken);

        var item = await _inventoryRepository.GetItemByIdAsync(request.ItemId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The selected item could not be found.", 404);
        if (request.TechnicianId.HasValue)
        {
            _ = await _inventoryRepository.GetTechnicianByIdAsync(request.TechnicianId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.NotFound, "The selected technician could not be found.", 404);
        }

        var partsReturnNumber = await GenerateUniquePartsReturnNumberAsync(cancellationToken);
        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();
        var partsReturn = new Domain.Entities.PartsReturn
        {
            PartsReturnNumber = partsReturnNumber,
            ItemId = item.ItemId,
            SupplierId = item.SupplierId,
            TechnicianId = request.TechnicianId,
            JobCardId = request.JobCardId,
            Quantity = request.Quantity,
            ReasonCode = request.ReasonCode.Trim(),
            DefectDescription = request.DefectDescription.Trim(),
            PartsReturnStatus = PartsReturnStatus.Submitted,
            RequestedDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        await _repository.AddPartsReturnAsync(partsReturn, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreatePartsReturn",
                EntityName = nameof(Domain.Entities.PartsReturn),
                EntityId = partsReturn.PartsReturnNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = item.ItemCode,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PartsReturnMapper.Map(partsReturn);
    }

    private async Task<string> GenerateUniquePartsReturnNumberAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var partsReturnNumber = _referenceGenerator.GeneratePartsReturnNumber();

            if (!await _repository.PartsReturnNumberExistsAsync(partsReturnNumber, cancellationToken))
            {
                return partsReturnNumber;
            }
        }
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "PartsReturn" : _currentUserContext.UserName;
    }
}

public sealed record ApprovePartsReturnCommand(
    long PartsReturnId,
    string? Remarks) : IRequest<PartsReturnResponse>;

public sealed class ApprovePartsReturnCommandValidator : AbstractValidator<ApprovePartsReturnCommand>
{
    public ApprovePartsReturnCommandValidator()
    {
        RuleFor(request => request.PartsReturnId).GreaterThan(0);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class ApprovePartsReturnCommandHandler : IRequestHandler<ApprovePartsReturnCommand, PartsReturnResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly IGapPhaseARepository _repository;
    private readonly GapPhaseAWorkflowService _workflowService;
    private readonly IUnitOfWork _unitOfWork;

    public ApprovePartsReturnCommandHandler(
        IGapPhaseARepository repository,
        GapPhaseAWorkflowService workflowService,
        GapPhaseAFeatureFlagService featureFlagService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _workflowService = workflowService;
        _featureFlagService = featureFlagService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<PartsReturnResponse> Handle(ApprovePartsReturnCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.partsreturn.enabled", cancellationToken);

        var partsReturn = await _repository.GetPartsReturnByIdForUpdateAsync(request.PartsReturnId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The parts return request could not be found.", 404);
        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();

        partsReturn.ApprovedDateUtc = now;
        partsReturn.ApprovalRemarks = request.Remarks?.Trim() ?? string.Empty;
        await _workflowService.EnsurePartsReturnTransitionAsync(partsReturn, PartsReturnStatus.Approved, partsReturn.ApprovalRemarks, cancellationToken);

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "ApprovePartsReturn",
                EntityName = nameof(Domain.Entities.PartsReturn),
                EntityId = partsReturn.PartsReturnNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = partsReturn.PartsReturnStatus.ToString(),
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PartsReturnMapper.Map(partsReturn);
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "PartsReturnApproval" : _currentUserContext.UserName;
    }
}

public sealed record CreateSupplierClaimCommand(
    long PartsReturnId,
    string SupplierClaimReference,
    string? Remarks) : IRequest<SupplierClaimResponse>;

public sealed class CreateSupplierClaimCommandValidator : AbstractValidator<CreateSupplierClaimCommand>
{
    public CreateSupplierClaimCommandValidator()
    {
        RuleFor(request => request.PartsReturnId).GreaterThan(0);
        RuleFor(request => request.SupplierClaimReference).NotEmpty().MaximumLength(128);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class CreateSupplierClaimCommandHandler : IRequestHandler<CreateSupplierClaimCommand, SupplierClaimResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly IGapPhaseARepository _repository;
    private readonly GapPhaseAWorkflowService _workflowService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSupplierClaimCommandHandler(
        IGapPhaseARepository repository,
        GapPhaseAWorkflowService workflowService,
        GapPhaseAFeatureFlagService featureFlagService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _workflowService = workflowService;
        _featureFlagService = featureFlagService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<SupplierClaimResponse> Handle(CreateSupplierClaimCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.partsreturn.enabled", cancellationToken);

        var partsReturn = await _repository.GetPartsReturnByIdForUpdateAsync(request.PartsReturnId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The parts return request could not be found.", 404);

        if (partsReturn.PartsReturnStatus == PartsReturnStatus.SupplierClaimRaised)
        {
            throw new AppException(ErrorCodes.PartsReturnClaimExists, "A supplier claim has already been raised for this parts return.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();
        partsReturn.SupplierClaimReference = request.SupplierClaimReference.Trim();
        partsReturn.ApprovalRemarks = request.Remarks?.Trim() ?? partsReturn.ApprovalRemarks;
        await _workflowService.EnsurePartsReturnTransitionAsync(partsReturn, PartsReturnStatus.SupplierClaimRaised, partsReturn.ApprovalRemarks, cancellationToken);

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreateSupplierClaim",
                EntityName = nameof(Domain.Entities.PartsReturn),
                EntityId = partsReturn.PartsReturnNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = partsReturn.SupplierClaimReference,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SupplierClaimResponse(
            partsReturn.PartsReturnId,
            partsReturn.PartsReturnNumber,
            partsReturn.PartsReturnStatus.ToString(),
            partsReturn.SupplierClaimReference);
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "SupplierClaim" : _currentUserContext.UserName;
    }
}

internal static class PartsReturnMapper
{
    public static PartsReturnResponse Map(Domain.Entities.PartsReturn partsReturn)
    {
        return new PartsReturnResponse(
            partsReturn.PartsReturnId,
            partsReturn.PartsReturnNumber,
            partsReturn.PartsReturnStatus.ToString(),
            partsReturn.Quantity,
            partsReturn.SupplierClaimReference);
    }
}
