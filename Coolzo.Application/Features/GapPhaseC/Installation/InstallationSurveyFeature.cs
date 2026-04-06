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

public sealed record ScheduleInstallationSurveyCommand(
    long InstallationId,
    DateTime SurveyDateUtc,
    long? TechnicianId,
    string? Remarks) : IRequest<InstallationSummaryResponse>;

public sealed class ScheduleInstallationSurveyCommandValidator : AbstractValidator<ScheduleInstallationSurveyCommand>
{
    public ScheduleInstallationSurveyCommandValidator()
    {
        RuleFor(request => request.InstallationId).GreaterThan(0);
        RuleFor(request => request.SurveyDateUtc).NotEmpty();
        RuleFor(request => request.Remarks).MaximumLength(256);
    }
}

public sealed class ScheduleInstallationSurveyCommandHandler : IRequestHandler<ScheduleInstallationSurveyCommand, InstallationSummaryResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IInstallationLifecycleRepository _installationLifecycleRepository;
    private readonly InstallationLifecycleAccessService _accessService;
    private readonly InstallationLifecycleWorkflowService _workflowService;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleInstallationSurveyCommandHandler(
        IInstallationLifecycleRepository installationLifecycleRepository,
        InstallationLifecycleAccessService accessService,
        InstallationLifecycleWorkflowService workflowService,
        ITechnicianRepository technicianRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _installationLifecycleRepository = installationLifecycleRepository;
        _accessService = accessService;
        _workflowService = workflowService;
        _technicianRepository = technicianRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<InstallationSummaryResponse> Handle(ScheduleInstallationSurveyCommand request, CancellationToken cancellationToken)
    {
        if (!_accessService.HasManagementAccess())
        {
            throw new AppException(ErrorCodes.Forbidden, "Only operations users can schedule installation surveys.", 403);
        }

        var installation = await _installationLifecycleRepository.GetInstallationByIdForUpdateAsync(request.InstallationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The installation could not be found.", 404);

        if (request.TechnicianId.HasValue)
        {
            _ = await _technicianRepository.GetByIdAsync(request.TechnicianId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.NotFound, "The selected technician could not be found.", 404);

            installation.AssignedTechnicianId = request.TechnicianId;
        }

        var actorName = InstallationLifecycleSupport.ResolveActorName(_currentUserContext, "InstallationSurvey");
        var actorRole = InstallationLifecycleSupport.ResolveActorRole(_currentUserContext);
        var now = _currentDateTime.UtcNow;
        var survey = installation.Surveys
            .Where(item => !item.IsDeleted)
            .OrderByDescending(item => item.DateCreated)
            .FirstOrDefault(item => item.CompletedDateUtc is null);

        if (survey is null)
        {
            survey = new InstallationSurvey
            {
                SurveyDateUtc = request.SurveyDateUtc,
                TechnicianId = request.TechnicianId ?? installation.AssignedTechnicianId,
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            };

            installation.Surveys.Add(survey);
        }
        else
        {
            survey.SurveyDateUtc = request.SurveyDateUtc;
            survey.TechnicianId = request.TechnicianId ?? survey.TechnicianId;
            survey.LastUpdated = now;
            survey.UpdatedBy = actorName;
        }

        installation.SurveyDateUtc = request.SurveyDateUtc;
        installation.AssignedTechnicianId = request.TechnicianId ?? installation.AssignedTechnicianId;

        if (installation.InstallationStatus == InstallationLifecycleStatus.LeadCreated)
        {
            _workflowService.EnsureTransition(
                installation,
                InstallationLifecycleStatus.SurveyScheduled,
                request.Remarks?.Trim() ?? "Survey scheduled.",
                actorName,
                actorRole,
                _currentUserContext.IPAddress,
                now);
        }

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "ScheduleInstallationSurvey",
                EntityName = nameof(InstallationLead),
                EntityId = installation.InstallationNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = request.SurveyDateUtc.ToString("O"),
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InstallationLifecycleSupport.MapSummary(installation);
    }
}

public sealed record SubmitInstallationSurveyCommand(
    long InstallationId,
    string SiteConditionSummary,
    bool ElectricalReadiness,
    bool AccessReadiness,
    string? SafetyRiskNotes,
    string? RecommendedAction,
    decimal EstimatedMaterialCost,
    string? MeasurementsJson,
    string? PhotoUrlsJson,
    IReadOnlyCollection<Coolzo.Contracts.Requests.GapPhaseC.InstallationSurveyItemRequest> Items) : IRequest<InstallationSummaryResponse>;

public sealed class SubmitInstallationSurveyCommandValidator : AbstractValidator<SubmitInstallationSurveyCommand>
{
    public SubmitInstallationSurveyCommandValidator()
    {
        RuleFor(request => request.InstallationId).GreaterThan(0);
        RuleFor(request => request.SiteConditionSummary).NotEmpty().MaximumLength(512);
        RuleFor(request => request.SafetyRiskNotes).MaximumLength(512);
        RuleFor(request => request.RecommendedAction).MaximumLength(512);
        RuleFor(request => request.MeasurementsJson).MaximumLength(4000);
        RuleFor(request => request.PhotoUrlsJson).MaximumLength(4000);
    }
}

public sealed class SubmitInstallationSurveyCommandHandler : IRequestHandler<SubmitInstallationSurveyCommand, InstallationSummaryResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IInstallationLifecycleRepository _installationLifecycleRepository;
    private readonly InstallationLifecycleAccessService _accessService;
    private readonly InstallationLifecycleWorkflowService _workflowService;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitInstallationSurveyCommandHandler(
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

    public async Task<InstallationSummaryResponse> Handle(SubmitInstallationSurveyCommand request, CancellationToken cancellationToken)
    {
        var installation = await _installationLifecycleRepository.GetInstallationByIdForUpdateAsync(request.InstallationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The installation could not be found.", 404);

        await _accessService.EnsureExecutionAccessAsync(installation, cancellationToken);

        var survey = installation.Surveys
            .Where(item => !item.IsDeleted)
            .OrderByDescending(item => item.DateCreated)
            .FirstOrDefault()
            ?? throw new AppException(ErrorCodes.SubmissionRequirementMissing, "Schedule a survey before submitting the report.", 409);

        var actorName = InstallationLifecycleSupport.ResolveActorName(_currentUserContext, "InstallationSurvey");
        var actorRole = InstallationLifecycleSupport.ResolveActorRole(_currentUserContext);
        var now = _currentDateTime.UtcNow;

        survey.SiteConditionSummary = request.SiteConditionSummary.Trim();
        survey.ElectricalReadiness = request.ElectricalReadiness;
        survey.AccessReadiness = request.AccessReadiness;
        survey.SafetyRiskNotes = request.SafetyRiskNotes?.Trim() ?? string.Empty;
        survey.RecommendedAction = request.RecommendedAction?.Trim() ?? string.Empty;
        survey.EstimatedMaterialCost = request.EstimatedMaterialCost;
        survey.MeasurementsJson = request.MeasurementsJson?.Trim() ?? string.Empty;
        survey.PhotoUrlsJson = request.PhotoUrlsJson?.Trim() ?? string.Empty;
        survey.CompletedDateUtc = now;
        survey.LastUpdated = now;
        survey.UpdatedBy = actorName;
        survey.Items.Clear();

        foreach (var item in request.Items)
        {
            survey.Items.Add(new InstallationSurveyItem
            {
                ItemTitle = item.ItemTitle.Trim(),
                ItemValue = item.ItemValue?.Trim() ?? string.Empty,
                Unit = item.Unit?.Trim() ?? string.Empty,
                Remarks = item.Remarks?.Trim() ?? string.Empty,
                IsMandatory = item.IsMandatory,
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });
        }

        installation.SurveyDateUtc = survey.SurveyDateUtc;

        _workflowService.EnsureTransition(
            installation,
            InstallationLifecycleStatus.SurveyCompleted,
            "Survey report submitted.",
            actorName,
            actorRole,
            _currentUserContext.IPAddress,
            now);

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "SubmitInstallationSurvey",
                EntityName = nameof(InstallationLead),
                EntityId = installation.InstallationNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = request.SiteConditionSummary.Trim(),
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InstallationLifecycleSupport.MapSummary(installation);
    }
}
