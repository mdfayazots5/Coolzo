using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.GapPhaseC;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.GapPhaseC.Installation;

public sealed record CreateInstallationExecutionOrderCommand(
    long InstallationId,
    long? TechnicianId,
    DateTime? ScheduledInstallationDateUtc,
    int HelperCount,
    string? ExecutionRemarks) : IRequest<InstallationSummaryResponse>;

public sealed class CreateInstallationExecutionOrderCommandValidator : AbstractValidator<CreateInstallationExecutionOrderCommand>
{
    public CreateInstallationExecutionOrderCommandValidator()
    {
        RuleFor(request => request.InstallationId).GreaterThan(0);
        RuleFor(request => request.HelperCount).GreaterThanOrEqualTo(0).LessThanOrEqualTo(12);
        RuleFor(request => request.ExecutionRemarks).MaximumLength(512);
    }
}

public sealed class CreateInstallationExecutionOrderCommandHandler : IRequestHandler<CreateInstallationExecutionOrderCommand, InstallationSummaryResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseAReferenceGenerator _gapPhaseAReferenceGenerator;
    private readonly IGapPhaseARepository _gapPhaseARepository;
    private readonly IInstallationLifecycleRepository _installationLifecycleRepository;
    private readonly InstallationLifecycleAccessService _accessService;
    private readonly InstallationLifecycleWorkflowService _workflowService;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInstallationExecutionOrderCommandHandler(
        IInstallationLifecycleRepository installationLifecycleRepository,
        IGapPhaseARepository gapPhaseARepository,
        IGapPhaseAReferenceGenerator gapPhaseAReferenceGenerator,
        InstallationLifecycleAccessService accessService,
        InstallationLifecycleWorkflowService workflowService,
        ITechnicianRepository technicianRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _installationLifecycleRepository = installationLifecycleRepository;
        _gapPhaseARepository = gapPhaseARepository;
        _gapPhaseAReferenceGenerator = gapPhaseAReferenceGenerator;
        _accessService = accessService;
        _workflowService = workflowService;
        _technicianRepository = technicianRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<InstallationSummaryResponse> Handle(CreateInstallationExecutionOrderCommand request, CancellationToken cancellationToken)
    {
        if (!_accessService.HasManagementAccess())
        {
            throw new AppException(ErrorCodes.Forbidden, "Only operations users can create installation orders.", 403);
        }

        var installation = await _installationLifecycleRepository.GetInstallationByIdForUpdateAsync(request.InstallationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The installation could not be found.", 404);

        if (installation.InstallationStatus != InstallationLifecycleStatus.ProposalApproved
            || installation.ApprovalStatus != InstallationApprovalStatus.Approved)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "Proposal approval is required before creating the installation order.", 409);
        }

        if (request.TechnicianId.HasValue)
        {
            _ = await _technicianRepository.GetByIdAsync(request.TechnicianId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.NotFound, "The selected technician could not be found.", 404);

            installation.AssignedTechnicianId = request.TechnicianId;
        }

        var actorName = InstallationLifecycleSupport.ResolveActorName(_currentUserContext, "InstallationExecution");
        var actorRole = InstallationLifecycleSupport.ResolveActorRole(_currentUserContext);
        var now = _currentDateTime.UtcNow;
        var order = new InstallationOrder
        {
            InstallationId = installation.InstallationId,
            LeadId = installation.LeadId,
            CustomerId = installation.CustomerId,
            CustomerAddressId = installation.CustomerAddressId,
            TechnicianId = installation.AssignedTechnicianId,
            InstallationOrderNumber = await GenerateOrderNumberAsync(cancellationToken),
            CurrentStatus = InstallationOrderStatus.InstallationScheduled,
            ScheduledInstallationDateUtc = request.ScheduledInstallationDateUtc ?? installation.ScheduledInstallationDateUtc ?? installation.SurveyDateUtc,
            InstallationChecklistJson = InstallationLifecycleSupport.BuildChecklistSnapshotJson(installation),
            SurveySummary = installation.Surveys
                .Where(survey => !survey.IsDeleted)
                .OrderByDescending(survey => survey.CompletedDateUtc ?? survey.SurveyDateUtc)
                .Select(survey => survey.SiteConditionSummary)
                .FirstOrDefault() ?? string.Empty,
            CommissioningRemarks = request.ExecutionRemarks?.Trim() ?? string.Empty,
            NumberOfUnits = installation.NumberOfUnits,
            InstallationType = installation.InstallationType,
            HelperCount = request.HelperCount,
            CreatedBy = actorName,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        installation.Orders.Add(order);
        installation.ScheduledInstallationDateUtc = order.ScheduledInstallationDateUtc;

        _workflowService.EnsureTransition(
            installation,
            InstallationLifecycleStatus.InstallationScheduled,
            "Installation execution order created.",
            actorName,
            actorRole,
            _currentUserContext.IPAddress,
            now);

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreateInstallationExecutionOrder",
                EntityName = nameof(InstallationLead),
                EntityId = installation.InstallationNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = order.InstallationOrderNumber,
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InstallationLifecycleSupport.MapSummary(installation);
    }

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var orderNumber = _gapPhaseAReferenceGenerator.GenerateInstallationOrderNumber();

            if (!await _gapPhaseARepository.InstallationOrderNumberExistsAsync(orderNumber, cancellationToken))
            {
                return orderNumber;
            }
        }
    }
}

public sealed record StartInstallationCommand(long InstallationId, string? Remarks) : IRequest<InstallationSummaryResponse>;

public sealed class StartInstallationCommandValidator : AbstractValidator<StartInstallationCommand>
{
    public StartInstallationCommandValidator()
    {
        RuleFor(request => request.InstallationId).GreaterThan(0);
        RuleFor(request => request.Remarks).MaximumLength(256);
    }
}

public sealed class StartInstallationCommandHandler : IRequestHandler<StartInstallationCommand, InstallationSummaryResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IInstallationLifecycleRepository _installationLifecycleRepository;
    private readonly InstallationLifecycleAccessService _accessService;
    private readonly InstallationLifecycleWorkflowService _workflowService;
    private readonly IUnitOfWork _unitOfWork;

    public StartInstallationCommandHandler(
        IInstallationLifecycleRepository installationLifecycleRepository,
        InstallationLifecycleAccessService accessService,
        InstallationLifecycleWorkflowService workflowService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _installationLifecycleRepository = installationLifecycleRepository;
        _accessService = accessService;
        _workflowService = workflowService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<InstallationSummaryResponse> Handle(StartInstallationCommand request, CancellationToken cancellationToken)
    {
        var installation = await _installationLifecycleRepository.GetInstallationByIdForUpdateAsync(request.InstallationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The installation could not be found.", 404);

        await _accessService.EnsureExecutionAccessAsync(installation, cancellationToken);

        if (installation.InstallationStatus != InstallationLifecycleStatus.InstallationScheduled)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "The installation must be scheduled before it can be started.", 409);
        }

        var order = installation.Orders
            .Where(item => !item.IsDeleted)
            .OrderByDescending(item => item.DateCreated)
            .FirstOrDefault()
            ?? throw new AppException(ErrorCodes.NotFound, "The installation order could not be found.", 404);

        var actorName = InstallationLifecycleSupport.ResolveActorName(_currentUserContext, "InstallationExecution");
        var actorRole = InstallationLifecycleSupport.ResolveActorRole(_currentUserContext);
        var now = _currentDateTime.UtcNow;

        order.CurrentStatus = InstallationOrderStatus.InstallationInProgress;
        order.ExecutionStartedDateUtc = now;
        order.LastUpdated = now;
        order.UpdatedBy = actorName;
        installation.InstallationStartedDateUtc = now;

        _workflowService.EnsureTransition(
            installation,
            InstallationLifecycleStatus.InstallationInProgress,
            request.Remarks?.Trim() ?? "Installation started.",
            actorName,
            actorRole,
            _currentUserContext.IPAddress,
            now);

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "StartInstallation",
                EntityName = nameof(InstallationLead),
                EntityId = installation.InstallationNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = order.InstallationOrderNumber,
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InstallationLifecycleSupport.MapSummary(installation);
    }
}

public sealed record SaveInstallationChecklistCommand(
    long InstallationId,
    IReadOnlyCollection<Coolzo.Contracts.Requests.GapPhaseC.InstallationChecklistItemRequest> Items) : IRequest<InstallationSummaryResponse>;

public sealed class SaveInstallationChecklistCommandValidator : AbstractValidator<SaveInstallationChecklistCommand>
{
    public SaveInstallationChecklistCommandValidator()
    {
        RuleFor(request => request.InstallationId).GreaterThan(0);
        RuleFor(request => request.Items).NotEmpty();
    }
}

public sealed class SaveInstallationChecklistCommandHandler : IRequestHandler<SaveInstallationChecklistCommand, InstallationSummaryResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IInstallationLifecycleRepository _installationLifecycleRepository;
    private readonly InstallationLifecycleAccessService _accessService;
    private readonly IUnitOfWork _unitOfWork;

    public SaveInstallationChecklistCommandHandler(
        IInstallationLifecycleRepository installationLifecycleRepository,
        InstallationLifecycleAccessService accessService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _installationLifecycleRepository = installationLifecycleRepository;
        _accessService = accessService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<InstallationSummaryResponse> Handle(SaveInstallationChecklistCommand request, CancellationToken cancellationToken)
    {
        var installation = await _installationLifecycleRepository.GetInstallationByIdForUpdateAsync(request.InstallationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The installation could not be found.", 404);

        await _accessService.EnsureExecutionAccessAsync(installation, cancellationToken);

        var actorName = InstallationLifecycleSupport.ResolveActorName(_currentUserContext, "InstallationChecklist");
        var now = _currentDateTime.UtcNow;
        var existingChecklists = installation.Checklists
            .Where(item => !item.IsDeleted)
            .ToDictionary(item => item.ChecklistTitle, StringComparer.OrdinalIgnoreCase);

        foreach (var item in request.Items)
        {
            if (!existingChecklists.TryGetValue(item.ChecklistTitle.Trim(), out var checklist))
            {
                checklist = new InstallationChecklist
                {
                    ChecklistTitle = item.ChecklistTitle.Trim(),
                    ChecklistDescription = item.ChecklistDescription?.Trim() ?? string.Empty,
                    IsMandatory = item.IsMandatory,
                    CreatedBy = actorName,
                    DateCreated = now,
                    IPAddress = _currentUserContext.IPAddress
                };

                installation.Checklists.Add(checklist);
                existingChecklists[checklist.ChecklistTitle] = checklist;
            }
            else
            {
                checklist.ChecklistDescription = item.ChecklistDescription?.Trim() ?? checklist.ChecklistDescription;
                checklist.IsMandatory = item.IsMandatory;
                checklist.LastUpdated = now;
                checklist.UpdatedBy = actorName;
            }

            var response = checklist.Responses
                .Where(record => !record.IsDeleted)
                .OrderByDescending(record => record.ResponseDateUtc ?? record.DateCreated)
                .FirstOrDefault();

            if (response is null)
            {
                response = new InstallationChecklistResponse
                {
                    InstallationId = installation.InstallationId,
                    IsCompleted = item.IsCompleted,
                    ResponseRemarks = item.ResponseRemarks?.Trim() ?? string.Empty,
                    ResponseDateUtc = now,
                    CreatedBy = actorName,
                    DateCreated = now,
                    IPAddress = _currentUserContext.IPAddress
                };

                checklist.Responses.Add(response);
            }
            else
            {
                response.IsCompleted = item.IsCompleted;
                response.ResponseRemarks = item.ResponseRemarks?.Trim() ?? string.Empty;
                response.ResponseDateUtc = now;
                response.LastUpdated = now;
                response.UpdatedBy = actorName;
            }
        }

        var latestOrder = installation.Orders
            .Where(order => !order.IsDeleted)
            .OrderByDescending(order => order.DateCreated)
            .FirstOrDefault();

        if (latestOrder is not null)
        {
            latestOrder.InstallationChecklistJson = InstallationLifecycleSupport.BuildChecklistSnapshotJson(installation);
            latestOrder.LastUpdated = now;
            latestOrder.UpdatedBy = actorName;
        }

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "SaveInstallationChecklist",
                EntityName = nameof(InstallationLead),
                EntityId = installation.InstallationNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = request.Items.Count.ToString(),
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InstallationLifecycleSupport.MapSummary(installation);
    }
}

public sealed record CompleteInstallationCommand(long InstallationId, string WorkSummary) : IRequest<InstallationSummaryResponse>;

public sealed class CompleteInstallationCommandValidator : AbstractValidator<CompleteInstallationCommand>
{
    public CompleteInstallationCommandValidator()
    {
        RuleFor(request => request.InstallationId).GreaterThan(0);
        RuleFor(request => request.WorkSummary).NotEmpty().MaximumLength(512);
    }
}

public sealed class CompleteInstallationCommandHandler : IRequestHandler<CompleteInstallationCommand, InstallationSummaryResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IInstallationLifecycleRepository _installationLifecycleRepository;
    private readonly InstallationLifecycleAccessService _accessService;
    private readonly InstallationLifecycleWorkflowService _workflowService;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteInstallationCommandHandler(
        IInstallationLifecycleRepository installationLifecycleRepository,
        InstallationLifecycleAccessService accessService,
        InstallationLifecycleWorkflowService workflowService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _installationLifecycleRepository = installationLifecycleRepository;
        _accessService = accessService;
        _workflowService = workflowService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<InstallationSummaryResponse> Handle(CompleteInstallationCommand request, CancellationToken cancellationToken)
    {
        var installation = await _installationLifecycleRepository.GetInstallationByIdForUpdateAsync(request.InstallationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The installation could not be found.", 404);

        await _accessService.EnsureExecutionAccessAsync(installation, cancellationToken);

        if (installation.InstallationStatus != InstallationLifecycleStatus.InstallationInProgress)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "The installation must be in progress before completion.", 409);
        }

        var mandatoryChecklistMissing = installation.Checklists
            .Where(checklist => !checklist.IsDeleted && checklist.IsMandatory)
            .Any(checklist =>
            {
                var response = checklist.Responses
                    .Where(item => !item.IsDeleted)
                    .OrderByDescending(item => item.ResponseDateUtc ?? item.DateCreated)
                    .FirstOrDefault();

                return response is null || !response.IsCompleted;
            });

        if (mandatoryChecklistMissing)
        {
            throw new AppException(ErrorCodes.SubmissionRequirementMissing, "Complete all mandatory installation checklist items before completion.", 409);
        }

        var latestOrder = installation.Orders
            .Where(order => !order.IsDeleted)
            .OrderByDescending(order => order.DateCreated)
            .FirstOrDefault()
            ?? throw new AppException(ErrorCodes.NotFound, "The installation order could not be found.", 404);

        var actorName = InstallationLifecycleSupport.ResolveActorName(_currentUserContext, "InstallationExecution");
        var actorRole = InstallationLifecycleSupport.ResolveActorRole(_currentUserContext);
        var now = _currentDateTime.UtcNow;

        latestOrder.CurrentStatus = InstallationOrderStatus.InstallationCompleted;
        latestOrder.ExecutionCompletedDateUtc = now;
        latestOrder.CommissioningRemarks = request.WorkSummary.Trim();
        latestOrder.InstallationChecklistJson = InstallationLifecycleSupport.BuildChecklistSnapshotJson(installation);
        latestOrder.LastUpdated = now;
        latestOrder.UpdatedBy = actorName;
        installation.InstallationCompletedDateUtc = now;

        _workflowService.EnsureTransition(
            installation,
            InstallationLifecycleStatus.InstallationCompleted,
            request.WorkSummary.Trim(),
            actorName,
            actorRole,
            _currentUserContext.IPAddress,
            now);

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CompleteInstallation",
                EntityName = nameof(InstallationLead),
                EntityId = installation.InstallationNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = latestOrder.InstallationOrderNumber,
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InstallationLifecycleSupport.MapSummary(installation);
    }
}

public sealed record GenerateInstallationCommissioningCommand(
    long InstallationId,
    string CustomerConfirmationName,
    string CustomerSignatureName,
    string? ChecklistJson,
    string? Remarks,
    bool IsAccepted) : IRequest<InstallationSummaryResponse>;

public sealed class GenerateInstallationCommissioningCommandValidator : AbstractValidator<GenerateInstallationCommissioningCommand>
{
    public GenerateInstallationCommissioningCommandValidator()
    {
        RuleFor(request => request.InstallationId).GreaterThan(0);
        RuleFor(request => request.CustomerConfirmationName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.CustomerSignatureName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.ChecklistJson).MaximumLength(4000);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class GenerateInstallationCommissioningCommandHandler : IRequestHandler<GenerateInstallationCommissioningCommand, InstallationSummaryResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseAReferenceGenerator _gapPhaseAReferenceGenerator;
    private readonly IInstallationLifecycleReferenceGenerator _installationLifecycleReferenceGenerator;
    private readonly IInstallationLifecycleRepository _installationLifecycleRepository;
    private readonly InstallationLifecycleAccessService _accessService;
    private readonly InstallationLifecycleWorkflowService _workflowService;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateInstallationCommissioningCommandHandler(
        IInstallationLifecycleRepository installationLifecycleRepository,
        IGapPhaseAReferenceGenerator gapPhaseAReferenceGenerator,
        IInstallationLifecycleReferenceGenerator installationLifecycleReferenceGenerator,
        InstallationLifecycleAccessService accessService,
        InstallationLifecycleWorkflowService workflowService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _installationLifecycleRepository = installationLifecycleRepository;
        _gapPhaseAReferenceGenerator = gapPhaseAReferenceGenerator;
        _installationLifecycleReferenceGenerator = installationLifecycleReferenceGenerator;
        _accessService = accessService;
        _workflowService = workflowService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<InstallationSummaryResponse> Handle(GenerateInstallationCommissioningCommand request, CancellationToken cancellationToken)
    {
        var installation = await _installationLifecycleRepository.GetInstallationByIdForUpdateAsync(request.InstallationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The installation could not be found.", 404);

        await _accessService.EnsureExecutionAccessAsync(installation, cancellationToken);

        if (installation.InstallationStatus != InstallationLifecycleStatus.InstallationCompleted)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "The installation must be completed before commissioning.", 409);
        }

        var latestOrder = installation.Orders
            .Where(order => !order.IsDeleted)
            .OrderByDescending(order => order.DateCreated)
            .FirstOrDefault()
            ?? throw new AppException(ErrorCodes.NotFound, "The installation order could not be found.", 404);

        var actorName = InstallationLifecycleSupport.ResolveActorName(_currentUserContext, "InstallationCommissioning");
        var actorRole = InstallationLifecycleSupport.ResolveActorRole(_currentUserContext);
        var now = _currentDateTime.UtcNow;
        var certificate = new CommissioningCertificate
        {
            InstallationId = installation.InstallationId,
            InstallationOrderId = latestOrder.InstallationOrderId,
            CertificateNumber = _gapPhaseAReferenceGenerator.GenerateCommissioningCertificateNumber(),
            WarrantyRegistrationNumber = await GenerateWarrantyRegistrationNumberAsync(cancellationToken),
            CommissioningDateUtc = now,
            CustomerConfirmationName = request.CustomerConfirmationName.Trim(),
            CustomerSignatureName = request.CustomerSignatureName.Trim(),
            ChecklistJson = request.ChecklistJson?.Trim() ?? InstallationLifecycleSupport.BuildChecklistSnapshotJson(installation),
            Remarks = request.Remarks?.Trim() ?? string.Empty,
            IsAccepted = request.IsAccepted,
            CreatedBy = actorName,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        latestOrder.CurrentStatus = InstallationOrderStatus.Commissioned;
        latestOrder.CommissioningRemarks = certificate.Remarks;
        latestOrder.LastUpdated = now;
        latestOrder.UpdatedBy = actorName;
        installation.CommissioningCertificates.Add(certificate);
        installation.CommissionedDateUtc = now;

        _workflowService.EnsureTransition(
            installation,
            InstallationLifecycleStatus.Commissioned,
            "Commissioning certificate generated.",
            actorName,
            actorRole,
            _currentUserContext.IPAddress,
            now);

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "GenerateInstallationCommissioning",
                EntityName = nameof(InstallationLead),
                EntityId = installation.InstallationNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = certificate.CertificateNumber,
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InstallationLifecycleSupport.MapSummary(installation);
    }

    private async Task<string> GenerateWarrantyRegistrationNumberAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var warrantyRegistrationNumber = _installationLifecycleReferenceGenerator.GenerateWarrantyRegistrationNumber();

            if (!await _installationLifecycleRepository.WarrantyRegistrationNumberExistsAsync(warrantyRegistrationNumber, cancellationToken))
            {
                return warrantyRegistrationNumber;
            }
        }
    }
}
