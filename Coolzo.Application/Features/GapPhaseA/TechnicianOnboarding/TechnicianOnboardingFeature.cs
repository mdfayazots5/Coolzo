using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Requests.GapPhaseA;
using Coolzo.Contracts.Responses.GapPhaseA;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.GapPhaseA.TechnicianOnboarding;

public sealed record CreateTechnicianDraftCommand(
    string TechnicianName,
    string MobileNumber,
    string? EmailAddress,
    long? BaseZoneId,
    int MaxDailyAssignments) : IRequest<TechnicianOnboardingResponse>;

public sealed class CreateTechnicianDraftCommandValidator : AbstractValidator<CreateTechnicianDraftCommand>
{
    public CreateTechnicianDraftCommandValidator()
    {
        RuleFor(request => request.TechnicianName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.MobileNumber).Matches("^[0-9]{8,16}$");
        RuleFor(request => request.EmailAddress).EmailAddress().When(request => !string.IsNullOrWhiteSpace(request.EmailAddress));
        RuleFor(request => request.MaxDailyAssignments).GreaterThan(0).LessThanOrEqualTo(16);
    }
}

public sealed class CreateTechnicianDraftCommandHandler : IRequestHandler<CreateTechnicianDraftCommand, TechnicianOnboardingResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly IGapPhaseARepository _repository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTechnicianDraftCommandHandler(
        ITechnicianRepository technicianRepository,
        IGapPhaseARepository repository,
        GapPhaseAFeatureFlagService featureFlagService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianRepository = technicianRepository;
        _repository = repository;
        _featureFlagService = featureFlagService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<TechnicianOnboardingResponse> Handle(CreateTechnicianDraftCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.technician.onboarding.enabled", cancellationToken);

        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();
        var technician = new Coolzo.Domain.Entities.Technician
        {
            TechnicianCode = $"TECH-{now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
            TechnicianName = request.TechnicianName.Trim(),
            MobileNumber = request.MobileNumber.Trim(),
            EmailAddress = request.EmailAddress?.Trim() ?? string.Empty,
            BaseZoneId = request.BaseZoneId,
            MaxDailyAssignments = request.MaxDailyAssignments,
            IsActive = false,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        await _technicianRepository.AddAsync(technician, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreateTechnicianDraft",
                EntityName = nameof(Technician),
                EntityId = technician.TechnicianCode,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = technician.TechnicianName,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await TechnicianOnboardingMapper.BuildAsync(technician, _repository, cancellationToken);
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "TechnicianOnboarding" : _currentUserContext.UserName;
    }
}

public sealed record UploadTechnicianDocumentsCommand(
    long TechnicianId,
    IReadOnlyCollection<TechnicianDocumentInput> Documents) : IRequest<TechnicianOnboardingResponse>;

public sealed class UploadTechnicianDocumentsCommandValidator : AbstractValidator<UploadTechnicianDocumentsCommand>
{
    public UploadTechnicianDocumentsCommandValidator()
    {
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleFor(request => request.Documents).NotEmpty();
        RuleForEach(request => request.Documents).ChildRules(document =>
        {
            document.RuleFor(item => item.DocumentType).NotEmpty().MaximumLength(64);
            document.RuleFor(item => item.DocumentNumber).MaximumLength(128);
            document.RuleFor(item => item.DocumentUrl).MaximumLength(512);
        });
    }
}

public sealed class UploadTechnicianDocumentsCommandHandler : IRequestHandler<UploadTechnicianDocumentsCommand, TechnicianOnboardingResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly IGapPhaseARepository _repository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UploadTechnicianDocumentsCommandHandler(
        ITechnicianRepository technicianRepository,
        IGapPhaseARepository repository,
        GapPhaseAFeatureFlagService featureFlagService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianRepository = technicianRepository;
        _repository = repository;
        _featureFlagService = featureFlagService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<TechnicianOnboardingResponse> Handle(UploadTechnicianDocumentsCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.technician.onboarding.enabled", cancellationToken);

        var technician = await _technicianRepository.GetByIdForUpdateAsync(request.TechnicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);
        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();

        foreach (var document in request.Documents)
        {
            await _repository.AddTechnicianDocumentAsync(
                new TechnicianDocument
                {
                    TechnicianId = technician.TechnicianId,
                    DocumentType = document.DocumentType.Trim(),
                    DocumentNumber = document.DocumentNumber?.Trim() ?? string.Empty,
                    DocumentUrl = document.DocumentUrl?.Trim() ?? string.Empty,
                    ExpiryDateUtc = document.ExpiryDateUtc,
                    VerificationStatus = TechnicianDocumentStatus.Uploaded,
                    CreatedBy = actor,
                    DateCreated = now,
                    IPAddress = _currentUserContext.IPAddress
                },
                cancellationToken);
        }

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "UploadTechnicianDocuments",
                EntityName = nameof(TechnicianDocument),
                EntityId = technician.TechnicianCode,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = request.Documents.Count.ToString(),
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await TechnicianOnboardingMapper.BuildAsync(technician, _repository, cancellationToken);
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "TechnicianDocuments" : _currentUserContext.UserName;
    }
}

public sealed record ActivateTechnicianCommand(
    long TechnicianId,
    string AssessmentCode,
    decimal ScorePercentage,
    string TrainingName,
    string? CertificationNumber,
    decimal TrainingScorePercentage,
    string? Remarks) : IRequest<TechnicianOnboardingResponse>;

public sealed class ActivateTechnicianCommandValidator : AbstractValidator<ActivateTechnicianCommand>
{
    public ActivateTechnicianCommandValidator()
    {
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleFor(request => request.AssessmentCode).NotEmpty().MaximumLength(64);
        RuleFor(request => request.ScorePercentage).InclusiveBetween(0.00m, 100.00m);
        RuleFor(request => request.TrainingName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.CertificationNumber).MaximumLength(128);
        RuleFor(request => request.TrainingScorePercentage).InclusiveBetween(0.00m, 100.00m);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class ActivateTechnicianCommandHandler : IRequestHandler<ActivateTechnicianCommand, TechnicianOnboardingResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly IGapPhaseARepository _repository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateTechnicianCommandHandler(
        ITechnicianRepository technicianRepository,
        IGapPhaseARepository repository,
        GapPhaseAFeatureFlagService featureFlagService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianRepository = technicianRepository;
        _repository = repository;
        _featureFlagService = featureFlagService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<TechnicianOnboardingResponse> Handle(ActivateTechnicianCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.technician.onboarding.enabled", cancellationToken);

        var technician = await _technicianRepository.GetByIdForUpdateAsync(request.TechnicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);
        var documents = await _repository.GetTechnicianDocumentsAsync(technician.TechnicianId, cancellationToken);

        if (documents.Count == 0)
        {
            throw new AppException(ErrorCodes.MissingTechnicianOnboardingDocument, "At least one onboarding document is required before activation.", 409);
        }

        if (request.ScorePercentage < 70.00m || request.TrainingScorePercentage < 70.00m)
        {
            throw new AppException(ErrorCodes.SkillAssessmentFailed, "The technician failed the minimum onboarding score threshold.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var actor = ResolveActor();

        await _repository.AddSkillAssessmentAsync(
            new SkillAssessment
            {
                TechnicianId = technician.TechnicianId,
                AssessmentCode = request.AssessmentCode.Trim(),
                ScorePercentage = request.ScorePercentage,
                AssessmentResult = SkillAssessmentResult.Passed,
                Remarks = request.Remarks?.Trim() ?? "Technician passed onboarding assessment.",
                AssessedOnUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
        await _repository.AddTrainingRecordAsync(
            new TrainingRecord
            {
                TechnicianId = technician.TechnicianId,
                TrainingName = request.TrainingName.Trim(),
                CertificationNumber = request.CertificationNumber?.Trim() ?? string.Empty,
                ScorePercentage = request.TrainingScorePercentage,
                CompletionDateUtc = now,
                IsCompleted = true,
                Remarks = request.Remarks?.Trim() ?? "Technician onboarding training completed.",
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        technician.IsActive = true;
        technician.UpdatedBy = actor;
        technician.LastUpdated = now;

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "ActivateTechnician",
                EntityName = nameof(Technician),
                EntityId = technician.TechnicianCode,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = "Activated",
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await TechnicianOnboardingMapper.BuildAsync(technician, _repository, cancellationToken);
    }

    private string ResolveActor()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "TechnicianActivation" : _currentUserContext.UserName;
    }
}

internal static class TechnicianOnboardingMapper
{
    public static async Task<TechnicianOnboardingResponse> BuildAsync(Coolzo.Domain.Entities.Technician technician, IGapPhaseARepository repository, CancellationToken cancellationToken)
    {
        var documents = await repository.GetTechnicianDocumentsAsync(technician.TechnicianId, cancellationToken);
        var latestAssessment = await repository.GetLatestSkillAssessmentAsync(technician.TechnicianId, cancellationToken);
        var trainingRecords = await repository.GetTrainingRecordsAsync(technician.TechnicianId, cancellationToken);

        return new TechnicianOnboardingResponse(
            technician.TechnicianId,
            technician.TechnicianCode,
            technician.TechnicianName,
            technician.IsActive,
            documents.Count,
            latestAssessment?.AssessmentResult.ToString() ?? SkillAssessmentResult.Pending.ToString(),
            trainingRecords.Count(record => record.IsCompleted));
    }
}
