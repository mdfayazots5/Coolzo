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

namespace Coolzo.Application.Features.GapPhaseA.Installation;

public sealed record CreateInstallationOrderCommand(
    long? LeadId,
    long? ServiceRequestId,
    long CustomerId,
    long CustomerAddressId,
    long? TechnicianId,
    DateTime? ScheduledInstallationDateUtc,
    string? InstallationChecklistJson) : IRequest<InstallationOrderResponse>;

public sealed class CreateInstallationOrderCommandValidator : AbstractValidator<CreateInstallationOrderCommand>
{
    public CreateInstallationOrderCommandValidator()
    {
        RuleFor(request => request.CustomerId).GreaterThan(0);
        RuleFor(request => request.CustomerAddressId).GreaterThan(0);
        RuleFor(request => request.InstallationChecklistJson).MaximumLength(2000);
    }
}

public sealed class CreateInstallationOrderCommandHandler : IRequestHandler<CreateInstallationOrderCommand, InstallationOrderResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly IGapPhaseAReferenceGenerator _referenceGenerator;
    private readonly IGapPhaseARepository _repository;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInstallationOrderCommandHandler(
        IGapPhaseARepository repository,
        IGapPhaseAReferenceGenerator referenceGenerator,
        IBookingRepository bookingRepository,
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        GapPhaseAFeatureFlagService featureFlagService)
    {
        _repository = repository;
        _referenceGenerator = referenceGenerator;
        _bookingRepository = bookingRepository;
        _serviceRequestRepository = serviceRequestRepository;
        _technicianRepository = technicianRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _featureFlagService = featureFlagService;
    }

    public async Task<InstallationOrderResponse> Handle(CreateInstallationOrderCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.installation.enabled", cancellationToken);

        var customer = await _bookingRepository.GetCustomerByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The selected customer could not be found.", 404);

        if (request.ServiceRequestId.HasValue)
        {
            _ = await _serviceRequestRepository.GetByIdAsync(request.ServiceRequestId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.NotFound, "The linked service request could not be found.", 404);
        }

        if (request.TechnicianId.HasValue)
        {
            _ = await _technicianRepository.GetByIdAsync(request.TechnicianId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.NotFound, "The selected technician could not be found.", 404);
        }

        var orderNumber = await GenerateUniqueInstallationOrderNumberAsync(cancellationToken);
        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();
        var installationOrder = new InstallationOrder
        {
            LeadId = request.LeadId,
            ServiceRequestId = request.ServiceRequestId,
            CustomerId = customer.CustomerId,
            CustomerAddressId = request.CustomerAddressId,
            TechnicianId = request.TechnicianId,
            InstallationOrderNumber = orderNumber,
            CurrentStatus = InstallationOrderStatus.Draft,
            ScheduledInstallationDateUtc = request.ScheduledInstallationDateUtc,
            InstallationChecklistJson = request.InstallationChecklistJson?.Trim() ?? string.Empty,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        await _repository.AddInstallationOrderAsync(installationOrder, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreateInstallationOrder",
                EntityName = nameof(InstallationOrder),
                EntityId = orderNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = customer.CustomerName,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InstallationMapper.MapInstallationOrder(installationOrder);
    }

    private async Task<string> GenerateUniqueInstallationOrderNumberAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var orderNumber = _referenceGenerator.GenerateInstallationOrderNumber();

            if (!await _repository.InstallationOrderNumberExistsAsync(orderNumber, cancellationToken))
            {
                return orderNumber;
            }
        }
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "Installation" : _currentUserContext.UserName;
    }
}

public sealed record SubmitSurveyReportCommand(
    long InstallationOrderId,
    string SurveyDecision,
    string SiteConditionSummary,
    bool ElectricalReadiness,
    bool AccessReadiness,
    string? SafetyRiskNotes,
    string? RecommendedAction,
    decimal EstimatedMaterialCost,
    string? SyncDeviceReference,
    string? SyncReference) : IRequest<InstallationOrderResponse>;

public sealed class SubmitSurveyReportCommandValidator : AbstractValidator<SubmitSurveyReportCommand>
{
    public SubmitSurveyReportCommandValidator()
    {
        RuleFor(request => request.InstallationOrderId).GreaterThan(0);
        RuleFor(request => request.SurveyDecision).NotEmpty().Must(BeValidDecision).WithMessage("Survey decision is invalid.");
        RuleFor(request => request.SiteConditionSummary).NotEmpty().MaximumLength(512);
        RuleFor(request => request.SafetyRiskNotes).MaximumLength(512);
        RuleFor(request => request.RecommendedAction).MaximumLength(512);
    }

    private static bool BeValidDecision(string surveyDecision)
    {
        return Enum.TryParse<SiteSurveyDecision>(surveyDecision, true, out _);
    }
}

public sealed class SubmitSurveyReportCommandHandler : IRequestHandler<SubmitSurveyReportCommand, InstallationOrderResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly IGapPhaseARepository _repository;
    private readonly GapPhaseAWorkflowService _workflowService;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitSurveyReportCommandHandler(
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

    public async Task<InstallationOrderResponse> Handle(SubmitSurveyReportCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.installation.enabled", cancellationToken);

        var installationOrder = await _repository.GetInstallationOrderByIdForUpdateAsync(request.InstallationOrderId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The installation order could not be found.", 404);

        if (installationOrder.CurrentStatus == InstallationOrderStatus.Draft)
        {
            await _workflowService.EnsureInstallationTransitionAsync(installationOrder, InstallationOrderStatus.SurveyScheduled, "Survey scheduled.", cancellationToken);
        }

        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();
        var surveyDecision = Enum.Parse<SiteSurveyDecision>(request.SurveyDecision, true);

        if (!string.IsNullOrWhiteSpace(request.SyncReference))
        {
            var existingQueueItem = await _repository.GetOfflineSyncQueueItemByReferenceAsync("SiteSurveyReport", request.SyncReference.Trim(), cancellationToken);

            if (existingQueueItem is not null && existingQueueItem.SyncStatus == OfflineSyncStatus.Completed)
            {
                throw new AppException(ErrorCodes.OfflineSyncConflict, "The submitted offline survey sync reference has already been processed.", 409);
            }

            if (existingQueueItem is null)
            {
                await _repository.AddOfflineSyncQueueItemAsync(
                    new OfflineSyncQueueItem
                    {
                        DeviceReference = request.SyncDeviceReference?.Trim() ?? "TechnicianApp",
                        EntityName = "SiteSurveyReport",
                        EntityReference = request.SyncReference.Trim(),
                        PayloadSnapshot = request.SiteConditionSummary,
                        SyncStatus = OfflineSyncStatus.Processing,
                        RetryCount = 0,
                        LastAttemptDateUtc = now,
                        ConflictStrategy = "LastWriteWins",
                        CreatedBy = actor,
                        DateCreated = now,
                        IPAddress = _currentUserContext.IPAddress
                    },
                    cancellationToken);
            }
        }

        installationOrder.SiteSurveyReports.Add(new SiteSurveyReport
        {
            SurveyDecision = surveyDecision,
            SurveyDateUtc = now,
            SiteConditionSummary = request.SiteConditionSummary.Trim(),
            ElectricalReadiness = request.ElectricalReadiness,
            AccessReadiness = request.AccessReadiness,
            SafetyRiskNotes = request.SafetyRiskNotes?.Trim() ?? string.Empty,
            RecommendedAction = request.RecommendedAction?.Trim() ?? string.Empty,
            EstimatedMaterialCost = request.EstimatedMaterialCost,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });

        installationOrder.SurveySummary = request.SiteConditionSummary.Trim();

        await _workflowService.EnsureInstallationTransitionAsync(installationOrder, InstallationOrderStatus.SurveyCompleted, "Survey submitted.", cancellationToken);

        if (surveyDecision == SiteSurveyDecision.Approved)
        {
            await _workflowService.EnsureInstallationTransitionAsync(installationOrder, InstallationOrderStatus.ApprovedForInstallation, "Survey approved for installation.", cancellationToken);
        }

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "SubmitSurveyReport",
                EntityName = nameof(InstallationOrder),
                EntityId = installationOrder.InstallationOrderNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = surveyDecision.ToString(),
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InstallationMapper.MapInstallationOrder(installationOrder);
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "Survey" : _currentUserContext.UserName;
    }
}

public sealed record CreateCommissioningCertificateCommand(
    long InstallationOrderId,
    string CustomerConfirmationName,
    string? ChecklistJson,
    string? Remarks,
    bool IsAccepted) : IRequest<CommissioningCertificateResponse>;

public sealed class CreateCommissioningCertificateCommandValidator : AbstractValidator<CreateCommissioningCertificateCommand>
{
    public CreateCommissioningCertificateCommandValidator()
    {
        RuleFor(request => request.InstallationOrderId).GreaterThan(0);
        RuleFor(request => request.CustomerConfirmationName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.ChecklistJson).MaximumLength(2000);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class CreateCommissioningCertificateCommandHandler : IRequestHandler<CreateCommissioningCertificateCommand, CommissioningCertificateResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly IGapPhaseAReferenceGenerator _referenceGenerator;
    private readonly IGapPhaseARepository _repository;
    private readonly GapPhaseAWorkflowService _workflowService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCommissioningCertificateCommandHandler(
        IGapPhaseARepository repository,
        IGapPhaseAReferenceGenerator referenceGenerator,
        GapPhaseAWorkflowService workflowService,
        GapPhaseAFeatureFlagService featureFlagService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _referenceGenerator = referenceGenerator;
        _workflowService = workflowService;
        _featureFlagService = featureFlagService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<CommissioningCertificateResponse> Handle(CreateCommissioningCertificateCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.installation.enabled", cancellationToken);

        var installationOrder = await _repository.GetInstallationOrderByIdForUpdateAsync(request.InstallationOrderId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The installation order could not be found.", 404);
        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();

        if (installationOrder.CurrentStatus == InstallationOrderStatus.ApprovedForInstallation)
        {
            await _workflowService.EnsureInstallationTransitionAsync(installationOrder, InstallationOrderStatus.InstallationScheduled, "Installation scheduled automatically during commissioning.", cancellationToken);
        }

        if (installationOrder.CurrentStatus == InstallationOrderStatus.InstallationScheduled)
        {
            await _workflowService.EnsureInstallationTransitionAsync(installationOrder, InstallationOrderStatus.InstallationInProgress, "Installation started for commissioning.", cancellationToken);
        }

        var certificate = new CommissioningCertificate
        {
            InstallationOrderId = installationOrder.InstallationOrderId,
            CertificateNumber = _referenceGenerator.GenerateCommissioningCertificateNumber(),
            CommissioningDateUtc = now,
            CustomerConfirmationName = request.CustomerConfirmationName.Trim(),
            ChecklistJson = request.ChecklistJson?.Trim() ?? installationOrder.InstallationChecklistJson,
            Remarks = request.Remarks?.Trim() ?? string.Empty,
            IsAccepted = request.IsAccepted,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        await _repository.AddCommissioningCertificateAsync(certificate, cancellationToken);
        await _workflowService.EnsureInstallationTransitionAsync(installationOrder, InstallationOrderStatus.Commissioned, "Commissioning certificate submitted.", cancellationToken);

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreateCommissioningCertificate",
                EntityName = nameof(CommissioningCertificate),
                EntityId = certificate.CertificateNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = installationOrder.InstallationOrderNumber,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CommissioningCertificateResponse(
            certificate.CommissioningCertificateId,
            certificate.InstallationOrderId,
            certificate.CertificateNumber,
            certificate.CommissioningDateUtc,
            certificate.IsAccepted);
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "Commissioning" : _currentUserContext.UserName;
    }
}

internal static class InstallationMapper
{
    public static InstallationOrderResponse MapInstallationOrder(InstallationOrder installationOrder)
    {
        return new InstallationOrderResponse(
            installationOrder.InstallationOrderId,
            installationOrder.InstallationOrderNumber,
            installationOrder.CurrentStatus.ToString(),
            installationOrder.CustomerId,
            installationOrder.ServiceRequestId,
            installationOrder.ScheduledInstallationDateUtc,
            installationOrder.SiteSurveyReports.Count(report => !report.IsDeleted));
    }
}
