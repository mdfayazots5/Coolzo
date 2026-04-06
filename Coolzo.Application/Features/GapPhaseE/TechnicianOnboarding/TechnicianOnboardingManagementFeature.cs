using System.Text.Json;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.GapPhaseA;
using Coolzo.Contracts.Responses.GapPhaseE;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.GapPhaseE.TechnicianOnboarding;

public sealed record LegacyActivationBootstrap(
    string? AssessmentCode,
    decimal? ScorePercentage,
    string? TrainingName,
    string? CertificationNumber,
    decimal? TrainingScorePercentage,
    string? Remarks);

public sealed record CreateTechnicianDraftPhaseECommand(
    string TechnicianName,
    string MobileNumber,
    string? EmailAddress,
    long? BaseZoneId,
    int MaxDailyAssignments) : IRequest<TechnicianOnboardingDetailResponse>;

public sealed class CreateTechnicianDraftPhaseECommandValidator : AbstractValidator<CreateTechnicianDraftPhaseECommand>
{
    public CreateTechnicianDraftPhaseECommandValidator()
    {
        RuleFor(request => request.TechnicianName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.MobileNumber).Matches("^[0-9]{8,16}$");
        RuleFor(request => request.EmailAddress).EmailAddress().When(request => !string.IsNullOrWhiteSpace(request.EmailAddress));
        RuleFor(request => request.MaxDailyAssignments).GreaterThan(0).LessThanOrEqualTo(16);
    }
}

public sealed class CreateTechnicianDraftPhaseECommandHandler : IRequestHandler<CreateTechnicianDraftPhaseECommand, TechnicianOnboardingDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly IGapPhaseERepository _repository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TechnicianOnboardingEligibilityService _eligibilityService;

    public CreateTechnicianDraftPhaseECommandHandler(
        ITechnicianRepository technicianRepository,
        IGapPhaseERepository repository,
        GapPhaseAFeatureFlagService featureFlagService,
        TechnicianOnboardingEligibilityService eligibilityService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianRepository = technicianRepository;
        _repository = repository;
        _featureFlagService = featureFlagService;
        _eligibilityService = eligibilityService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<TechnicianOnboardingDetailResponse> Handle(CreateTechnicianDraftPhaseECommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.technician.onboarding.enabled", cancellationToken);

        var normalizedMobileNumber = request.MobileNumber.Trim();

        if (await _repository.TechnicianMobileExistsAsync(normalizedMobileNumber, null, cancellationToken))
        {
            throw new AppException(ErrorCodes.DuplicateValue, "A technician draft already exists for this mobile number.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var actor = TechnicianOnboardingPhaseESupport.ResolveActor(_currentUserContext, "TechnicianOnboarding");
        var technician = new Coolzo.Domain.Entities.Technician
        {
            TechnicianCode = $"TECH-{now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
            TechnicianName = request.TechnicianName.Trim(),
            MobileNumber = normalizedMobileNumber,
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
            TechnicianOnboardingPhaseESupport.CreateAuditLog(
                _currentUserContext,
                now,
                "CreateTechnicianDraft",
                nameof(Coolzo.Domain.Entities.Technician),
                technician.TechnicianCode,
                technician.TechnicianName),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await TechnicianOnboardingPhaseESupport.BuildDetailAsync(technician, _repository, _eligibilityService, cancellationToken);
    }
}

public sealed record GetTechnicianOnboardingListQuery(
    string? SearchTerm,
    string? Status,
    int? BranchId) : IRequest<IReadOnlyCollection<TechnicianOnboardingListItemResponse>>;

public sealed class GetTechnicianOnboardingListQueryHandler : IRequestHandler<GetTechnicianOnboardingListQuery, IReadOnlyCollection<TechnicianOnboardingListItemResponse>>
{
    private readonly TechnicianOnboardingEligibilityService _eligibilityService;
    private readonly IGapPhaseERepository _repository;

    public GetTechnicianOnboardingListQueryHandler(
        IGapPhaseERepository repository,
        TechnicianOnboardingEligibilityService eligibilityService)
    {
        _repository = repository;
        _eligibilityService = eligibilityService;
    }

    public async Task<IReadOnlyCollection<TechnicianOnboardingListItemResponse>> Handle(GetTechnicianOnboardingListQuery request, CancellationToken cancellationToken)
    {
        var technicians = await _repository.SearchTechniciansAsync(request.SearchTerm?.Trim(), request.BranchId, cancellationToken);
        var items = new List<TechnicianOnboardingListItemResponse>(technicians.Count);

        foreach (var technician in technicians)
        {
            var documents = await _repository.GetTechnicianDocumentsAsync(technician.TechnicianId, cancellationToken);
            var assessments = await _repository.GetSkillAssessmentsAsync(technician.TechnicianId, cancellationToken);
            var trainingRecords = await _repository.GetTrainingRecordsAsync(technician.TechnicianId, cancellationToken);
            var eligibility = _eligibilityService.Evaluate(technician, documents, assessments, trainingRecords);
            var item = TechnicianOnboardingPhaseESupport.MapListItem(technician, documents, assessments, trainingRecords, eligibility);

            if (!string.IsNullOrWhiteSpace(request.Status) &&
                !item.OnboardingStatus.Equals(request.Status.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            items.Add(item);
        }

        return items;
    }
}

public sealed record GetTechnicianOnboardingDetailQuery(long TechnicianId) : IRequest<TechnicianOnboardingDetailResponse>;

public sealed class GetTechnicianOnboardingDetailQueryHandler : IRequestHandler<GetTechnicianOnboardingDetailQuery, TechnicianOnboardingDetailResponse>
{
    private readonly TechnicianOnboardingEligibilityService _eligibilityService;
    private readonly IGapPhaseERepository _repository;

    public GetTechnicianOnboardingDetailQueryHandler(
        IGapPhaseERepository repository,
        TechnicianOnboardingEligibilityService eligibilityService)
    {
        _repository = repository;
        _eligibilityService = eligibilityService;
    }

    public async Task<TechnicianOnboardingDetailResponse> Handle(GetTechnicianOnboardingDetailQuery request, CancellationToken cancellationToken)
    {
        var technician = await _repository.GetTechnicianAsync(request.TechnicianId, asNoTracking: true, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);

        return await TechnicianOnboardingPhaseESupport.BuildDetailAsync(technician, _repository, _eligibilityService, cancellationToken);
    }
}

public sealed record UploadTechnicianDocumentsPhaseECommand(
    long TechnicianId,
    IReadOnlyCollection<Coolzo.Contracts.Requests.GapPhaseA.TechnicianDocumentInput> Documents) : IRequest<TechnicianOnboardingDetailResponse>;

public sealed class UploadTechnicianDocumentsPhaseECommandValidator : AbstractValidator<UploadTechnicianDocumentsPhaseECommand>
{
    public UploadTechnicianDocumentsPhaseECommandValidator()
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

public sealed class UploadTechnicianDocumentsPhaseECommandHandler : IRequestHandler<UploadTechnicianDocumentsPhaseECommand, TechnicianOnboardingDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly TechnicianOnboardingEligibilityService _eligibilityService;
    private readonly IGapPhaseERepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UploadTechnicianDocumentsPhaseECommandHandler(
        IGapPhaseERepository repository,
        GapPhaseAFeatureFlagService featureFlagService,
        TechnicianOnboardingEligibilityService eligibilityService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _featureFlagService = featureFlagService;
        _eligibilityService = eligibilityService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<TechnicianOnboardingDetailResponse> Handle(UploadTechnicianDocumentsPhaseECommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.technician.onboarding.enabled", cancellationToken);

        var technician = await _repository.GetTechnicianAsync(request.TechnicianId, asNoTracking: false, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);
        var now = _currentDateTime.UtcNow;
        var actor = TechnicianOnboardingPhaseESupport.ResolveActor(_currentUserContext, "TechnicianDocuments");

        foreach (var document in request.Documents)
        {
            var storageUrl = document.DocumentUrl?.Trim() ?? string.Empty;
            await _repository.AddTechnicianDocumentAsync(
                new TechnicianDocument
                {
                    TechnicianId = technician.TechnicianId,
                    DocumentType = document.DocumentType.Trim(),
                    DocumentNumber = document.DocumentNumber?.Trim() ?? string.Empty,
                    DocumentUrl = storageUrl,
                    StorageUrl = storageUrl,
                    ExpiryDateUtc = document.ExpiryDateUtc,
                    VerificationStatus = TechnicianDocumentStatus.Uploaded,
                    VerificationRemarks = string.Empty,
                    CreatedBy = actor,
                    DateCreated = now,
                    IPAddress = _currentUserContext.IPAddress
                },
                cancellationToken);
        }

        await _auditLogRepository.AddAsync(
            TechnicianOnboardingPhaseESupport.CreateAuditLog(
                _currentUserContext,
                now,
                "UploadTechnicianDocuments",
                nameof(TechnicianDocument),
                technician.TechnicianCode,
                request.Documents.Count.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await TechnicianOnboardingPhaseESupport.BuildDetailAsync(technician, _repository, _eligibilityService, cancellationToken);
    }
}

public sealed record GetTechnicianDocumentListQuery(long TechnicianId) : IRequest<IReadOnlyCollection<TechnicianDocumentDetailResponse>>;

public sealed class GetTechnicianDocumentListQueryHandler : IRequestHandler<GetTechnicianDocumentListQuery, IReadOnlyCollection<TechnicianDocumentDetailResponse>>
{
    private readonly IGapPhaseERepository _repository;

    public GetTechnicianDocumentListQueryHandler(IGapPhaseERepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<TechnicianDocumentDetailResponse>> Handle(GetTechnicianDocumentListQuery request, CancellationToken cancellationToken)
    {
        var documents = await _repository.GetTechnicianDocumentsAsync(request.TechnicianId, cancellationToken);
        return documents.Select(TechnicianOnboardingPhaseESupport.MapDocument).ToArray();
    }
}

public sealed record VerifyTechnicianDocumentCommand(
    long TechnicianId,
    long TechnicianDocumentId,
    string? Remarks) : IRequest<IReadOnlyCollection<TechnicianDocumentDetailResponse>>;

public sealed record RejectTechnicianDocumentCommand(
    long TechnicianId,
    long TechnicianDocumentId,
    string Remarks) : IRequest<IReadOnlyCollection<TechnicianDocumentDetailResponse>>;

public sealed class RejectTechnicianDocumentCommandValidator : AbstractValidator<RejectTechnicianDocumentCommand>
{
    public RejectTechnicianDocumentCommandValidator()
    {
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleFor(request => request.TechnicianDocumentId).GreaterThan(0);
        RuleFor(request => request.Remarks).NotEmpty().MaximumLength(512);
    }
}

public sealed class VerifyTechnicianDocumentCommandHandler : IRequestHandler<VerifyTechnicianDocumentCommand, IReadOnlyCollection<TechnicianDocumentDetailResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseERepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyTechnicianDocumentCommandHandler(
        IGapPhaseERepository repository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<TechnicianDocumentDetailResponse>> Handle(VerifyTechnicianDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetTechnicianDocumentAsync(request.TechnicianId, request.TechnicianDocumentId, asNoTracking: false, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician document could not be found.", 404);
        var now = _currentDateTime.UtcNow;

        document.VerificationStatus = TechnicianDocumentStatus.Verified;
        document.VerificationRemarks = request.Remarks?.Trim() ?? "Document verified.";
        document.VerifiedByUserId = _currentUserContext.UserId;
        document.VerifiedOnUtc = now;
        document.LastUpdated = now;
        document.UpdatedBy = TechnicianOnboardingPhaseESupport.ResolveActor(_currentUserContext, "DocumentVerifier");

        await _auditLogRepository.AddAsync(
            TechnicianOnboardingPhaseESupport.CreateAuditLog(
                _currentUserContext,
                now,
                "VerifyTechnicianDocument",
                nameof(TechnicianDocument),
                document.TechnicianDocumentId.ToString(),
                document.DocumentType),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var documents = await _repository.GetTechnicianDocumentsAsync(request.TechnicianId, cancellationToken);
        return documents.Select(TechnicianOnboardingPhaseESupport.MapDocument).ToArray();
    }
}

public sealed class RejectTechnicianDocumentCommandHandler : IRequestHandler<RejectTechnicianDocumentCommand, IReadOnlyCollection<TechnicianDocumentDetailResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseERepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RejectTechnicianDocumentCommandHandler(
        IGapPhaseERepository repository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<TechnicianDocumentDetailResponse>> Handle(RejectTechnicianDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetTechnicianDocumentAsync(request.TechnicianId, request.TechnicianDocumentId, asNoTracking: false, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician document could not be found.", 404);
        var now = _currentDateTime.UtcNow;

        document.VerificationStatus = TechnicianDocumentStatus.Rejected;
        document.VerificationRemarks = request.Remarks.Trim();
        document.VerifiedByUserId = _currentUserContext.UserId;
        document.VerifiedOnUtc = now;
        document.LastUpdated = now;
        document.UpdatedBy = TechnicianOnboardingPhaseESupport.ResolveActor(_currentUserContext, "DocumentVerifier");

        await _auditLogRepository.AddAsync(
            TechnicianOnboardingPhaseESupport.CreateAuditLog(
                _currentUserContext,
                now,
                "RejectTechnicianDocument",
                nameof(TechnicianDocument),
                document.TechnicianDocumentId.ToString(),
                document.DocumentType),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var documents = await _repository.GetTechnicianDocumentsAsync(request.TechnicianId, cancellationToken);
        return documents.Select(TechnicianOnboardingPhaseESupport.MapDocument).ToArray();
    }
}

public sealed record CreateSkillAssessmentPhaseECommand(
    long TechnicianId,
    long? SkillTagId,
    string AssessmentCode,
    string AssessmentName,
    string? Remarks) : IRequest<IReadOnlyCollection<SkillAssessmentDetailResponse>>;

public sealed class CreateSkillAssessmentPhaseECommandValidator : AbstractValidator<CreateSkillAssessmentPhaseECommand>
{
    public CreateSkillAssessmentPhaseECommandValidator()
    {
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleFor(request => request.AssessmentCode).NotEmpty().MaximumLength(64);
        RuleFor(request => request.AssessmentName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class CreateSkillAssessmentPhaseECommandHandler : IRequestHandler<CreateSkillAssessmentPhaseECommand, IReadOnlyCollection<SkillAssessmentDetailResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseERepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSkillAssessmentPhaseECommandHandler(
        IGapPhaseERepository repository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<SkillAssessmentDetailResponse>> Handle(CreateSkillAssessmentPhaseECommand request, CancellationToken cancellationToken)
    {
        var technician = await _repository.GetTechnicianAsync(request.TechnicianId, asNoTracking: true, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);
        var now = _currentDateTime.UtcNow;
        var actor = TechnicianOnboardingPhaseESupport.ResolveActor(_currentUserContext, "SkillAssessment");

        await _repository.AddSkillAssessmentAsync(
            new SkillAssessment
            {
                TechnicianId = technician.TechnicianId,
                SkillTagId = request.SkillTagId,
                AssessmentCode = request.AssessmentCode.Trim(),
                AssessmentName = request.AssessmentName.Trim(),
                AssessmentStatus = "Assigned",
                ScorePercentage = 0.00m,
                AssessmentResult = SkillAssessmentResult.Pending,
                PassFlag = false,
                Remarks = request.Remarks?.Trim() ?? string.Empty,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _auditLogRepository.AddAsync(
            TechnicianOnboardingPhaseESupport.CreateAuditLog(
                _currentUserContext,
                now,
                "CreateSkillAssessment",
                nameof(SkillAssessment),
                technician.TechnicianCode,
                request.AssessmentCode),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var assessments = await _repository.GetSkillAssessmentsAsync(request.TechnicianId, cancellationToken);
        return assessments.Select(TechnicianOnboardingPhaseESupport.MapAssessment).ToArray();
    }
}

public sealed record GetSkillAssessmentListQuery(long TechnicianId) : IRequest<IReadOnlyCollection<SkillAssessmentDetailResponse>>;

public sealed class GetSkillAssessmentListQueryHandler : IRequestHandler<GetSkillAssessmentListQuery, IReadOnlyCollection<SkillAssessmentDetailResponse>>
{
    private readonly IGapPhaseERepository _repository;

    public GetSkillAssessmentListQueryHandler(IGapPhaseERepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<SkillAssessmentDetailResponse>> Handle(GetSkillAssessmentListQuery request, CancellationToken cancellationToken)
    {
        var assessments = await _repository.GetSkillAssessmentsAsync(request.TechnicianId, cancellationToken);
        return assessments.Select(TechnicianOnboardingPhaseESupport.MapAssessment).ToArray();
    }
}

public sealed record SubmitSkillAssessmentResultCommand(
    long TechnicianId,
    long SkillAssessmentId,
    decimal ScorePercentage,
    bool PassFlag,
    string? Remarks) : IRequest<IReadOnlyCollection<SkillAssessmentDetailResponse>>;

public sealed class SubmitSkillAssessmentResultCommandValidator : AbstractValidator<SubmitSkillAssessmentResultCommand>
{
    public SubmitSkillAssessmentResultCommandValidator()
    {
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleFor(request => request.SkillAssessmentId).GreaterThan(0);
        RuleFor(request => request.ScorePercentage).InclusiveBetween(0.00m, 100.00m);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class SubmitSkillAssessmentResultCommandHandler : IRequestHandler<SubmitSkillAssessmentResultCommand, IReadOnlyCollection<SkillAssessmentDetailResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseERepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitSkillAssessmentResultCommandHandler(
        IGapPhaseERepository repository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<SkillAssessmentDetailResponse>> Handle(SubmitSkillAssessmentResultCommand request, CancellationToken cancellationToken)
    {
        var assessment = await _repository.GetSkillAssessmentAsync(request.TechnicianId, request.SkillAssessmentId, asNoTracking: false, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The skill assessment could not be found.", 404);
        var now = _currentDateTime.UtcNow;

        assessment.ScorePercentage = request.ScorePercentage;
        assessment.PassFlag = request.PassFlag;
        assessment.AssessmentResult = request.PassFlag ? SkillAssessmentResult.Passed : SkillAssessmentResult.Failed;
        assessment.AssessmentStatus = "Completed";
        assessment.AssessedByUserId = _currentUserContext.UserId;
        assessment.AssessedOnUtc = now;
        assessment.Remarks = request.Remarks?.Trim() ?? assessment.Remarks;
        assessment.LastUpdated = now;
        assessment.UpdatedBy = TechnicianOnboardingPhaseESupport.ResolveActor(_currentUserContext, "SkillAssessment");

        await _auditLogRepository.AddAsync(
            TechnicianOnboardingPhaseESupport.CreateAuditLog(
                _currentUserContext,
                now,
                "SubmitSkillAssessmentResult",
                nameof(SkillAssessment),
                assessment.SkillAssessmentId.ToString(),
                assessment.AssessmentCode),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var assessments = await _repository.GetSkillAssessmentsAsync(request.TechnicianId, cancellationToken);
        return assessments.Select(TechnicianOnboardingPhaseESupport.MapAssessment).ToArray();
    }
}

public sealed record CreateTrainingRecordPhaseECommand(
    long TechnicianId,
    string TrainingTitle,
    string TrainingType,
    string? Remarks) : IRequest<IReadOnlyCollection<TrainingRecordDetailResponse>>;

public sealed class CreateTrainingRecordPhaseECommandValidator : AbstractValidator<CreateTrainingRecordPhaseECommand>
{
    public CreateTrainingRecordPhaseECommandValidator()
    {
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleFor(request => request.TrainingTitle).NotEmpty().MaximumLength(128);
        RuleFor(request => request.TrainingType).NotEmpty().MaximumLength(64);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class CreateTrainingRecordPhaseECommandHandler : IRequestHandler<CreateTrainingRecordPhaseECommand, IReadOnlyCollection<TrainingRecordDetailResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseERepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTrainingRecordPhaseECommandHandler(
        IGapPhaseERepository repository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<TrainingRecordDetailResponse>> Handle(CreateTrainingRecordPhaseECommand request, CancellationToken cancellationToken)
    {
        var technician = await _repository.GetTechnicianAsync(request.TechnicianId, asNoTracking: true, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);
        var now = _currentDateTime.UtcNow;
        var actor = TechnicianOnboardingPhaseESupport.ResolveActor(_currentUserContext, "TrainingRecord");

        await _repository.AddTrainingRecordAsync(
            new TrainingRecord
            {
                TechnicianId = technician.TechnicianId,
                TrainingName = request.TrainingTitle.Trim(),
                TrainingTitle = request.TrainingTitle.Trim(),
                TrainingType = request.TrainingType.Trim(),
                TrainingStatus = "Assigned",
                CertificationNumber = string.Empty,
                ScorePercentage = 0.00m,
                CompletionDateUtc = DateTime.MinValue,
                TrainingCompletionDateUtc = null,
                IsCompleted = false,
                Remarks = request.Remarks?.Trim() ?? string.Empty,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _auditLogRepository.AddAsync(
            TechnicianOnboardingPhaseESupport.CreateAuditLog(
                _currentUserContext,
                now,
                "CreateTrainingRecord",
                nameof(TrainingRecord),
                technician.TechnicianCode,
                request.TrainingTitle),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var trainingRecords = await _repository.GetTrainingRecordsAsync(request.TechnicianId, cancellationToken);
        return trainingRecords.Select(TechnicianOnboardingPhaseESupport.MapTrainingRecord).ToArray();
    }
}

public sealed record GetTrainingRecordListQuery(long TechnicianId) : IRequest<IReadOnlyCollection<TrainingRecordDetailResponse>>;

public sealed class GetTrainingRecordListQueryHandler : IRequestHandler<GetTrainingRecordListQuery, IReadOnlyCollection<TrainingRecordDetailResponse>>
{
    private readonly IGapPhaseERepository _repository;

    public GetTrainingRecordListQueryHandler(IGapPhaseERepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<TrainingRecordDetailResponse>> Handle(GetTrainingRecordListQuery request, CancellationToken cancellationToken)
    {
        var trainingRecords = await _repository.GetTrainingRecordsAsync(request.TechnicianId, cancellationToken);
        return trainingRecords.Select(TechnicianOnboardingPhaseESupport.MapTrainingRecord).ToArray();
    }
}

public sealed record CompleteTrainingRecordCommand(
    long TechnicianId,
    long TrainingRecordId,
    string? CertificationNumber,
    decimal? ScorePercentage,
    string? CertificateUrl,
    string? Remarks) : IRequest<IReadOnlyCollection<TrainingRecordDetailResponse>>;

public sealed class CompleteTrainingRecordCommandValidator : AbstractValidator<CompleteTrainingRecordCommand>
{
    public CompleteTrainingRecordCommandValidator()
    {
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleFor(request => request.TrainingRecordId).GreaterThan(0);
        RuleFor(request => request.CertificationNumber).MaximumLength(128);
        RuleFor(request => request.CertificateUrl).MaximumLength(512);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class CompleteTrainingRecordCommandHandler : IRequestHandler<CompleteTrainingRecordCommand, IReadOnlyCollection<TrainingRecordDetailResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseERepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteTrainingRecordCommandHandler(
        IGapPhaseERepository repository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<TrainingRecordDetailResponse>> Handle(CompleteTrainingRecordCommand request, CancellationToken cancellationToken)
    {
        var trainingRecord = await _repository.GetTrainingRecordAsync(request.TechnicianId, request.TrainingRecordId, asNoTracking: false, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The training record could not be found.", 404);
        var now = _currentDateTime.UtcNow;

        trainingRecord.CertificationNumber = request.CertificationNumber?.Trim() ?? trainingRecord.CertificationNumber;
        trainingRecord.ScorePercentage = request.ScorePercentage ?? trainingRecord.ScorePercentage;
        trainingRecord.CertificateUrl = request.CertificateUrl?.Trim() ?? trainingRecord.CertificateUrl;
        trainingRecord.IsCompleted = true;
        trainingRecord.TrainingStatus = "Completed";
        trainingRecord.TrainingCompletionDateUtc = now;
        trainingRecord.CompletionDateUtc = now;
        trainingRecord.TrainerUserId = _currentUserContext.UserId;
        trainingRecord.Remarks = request.Remarks?.Trim() ?? trainingRecord.Remarks;
        trainingRecord.LastUpdated = now;
        trainingRecord.UpdatedBy = TechnicianOnboardingPhaseESupport.ResolveActor(_currentUserContext, "TrainingRecord");

        await _auditLogRepository.AddAsync(
            TechnicianOnboardingPhaseESupport.CreateAuditLog(
                _currentUserContext,
                now,
                "CompleteTrainingRecord",
                nameof(TrainingRecord),
                trainingRecord.TrainingRecordId.ToString(),
                trainingRecord.TrainingTitle),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var trainingRecords = await _repository.GetTrainingRecordsAsync(request.TechnicianId, cancellationToken);
        return trainingRecords.Select(TechnicianOnboardingPhaseESupport.MapTrainingRecord).ToArray();
    }
}

public sealed record ActivateTechnicianPhaseECommand(
    long TechnicianId,
    string ActivationReason,
    LegacyActivationBootstrap? LegacyBootstrap) : IRequest<TechnicianOnboardingDetailResponse>;

public sealed class ActivateTechnicianPhaseECommandValidator : AbstractValidator<ActivateTechnicianPhaseECommand>
{
    public ActivateTechnicianPhaseECommandValidator()
    {
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleFor(request => request.ActivationReason).NotEmpty().MaximumLength(512);
    }
}

public sealed class ActivateTechnicianPhaseECommandHandler : IRequestHandler<ActivateTechnicianPhaseECommand, TechnicianOnboardingDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly TechnicianOnboardingEligibilityService _eligibilityService;
    private readonly IGapPhaseERepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateTechnicianPhaseECommandHandler(
        IGapPhaseERepository repository,
        TechnicianOnboardingEligibilityService eligibilityService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _eligibilityService = eligibilityService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<TechnicianOnboardingDetailResponse> Handle(ActivateTechnicianPhaseECommand request, CancellationToken cancellationToken)
    {
        var technician = await _repository.GetTechnicianAsync(request.TechnicianId, asNoTracking: false, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);

        if (technician.IsActive)
        {
            throw new AppException(ErrorCodes.Conflict, "The technician is already active.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var actor = TechnicianOnboardingPhaseESupport.ResolveActor(_currentUserContext, "TechnicianActivation");
        var documents = (await _repository.GetTechnicianDocumentsAsync(technician.TechnicianId, cancellationToken)).ToList();
        var skillAssessments = (await _repository.GetSkillAssessmentsAsync(technician.TechnicianId, cancellationToken)).ToList();
        var trainingRecords = (await _repository.GetTrainingRecordsAsync(technician.TechnicianId, cancellationToken)).ToList();

        if (!skillAssessments.Any(assessment => assessment.AssessmentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) && assessment.PassFlag) &&
            request.LegacyBootstrap is not null &&
            !string.IsNullOrWhiteSpace(request.LegacyBootstrap.AssessmentCode) &&
            request.LegacyBootstrap.ScorePercentage.HasValue)
        {
            var legacyAssessment = new SkillAssessment
            {
                TechnicianId = technician.TechnicianId,
                AssessmentCode = request.LegacyBootstrap.AssessmentCode.Trim(),
                AssessmentName = request.LegacyBootstrap.AssessmentCode.Trim(),
                AssessmentStatus = "Completed",
                ScorePercentage = request.LegacyBootstrap.ScorePercentage.Value,
                AssessmentResult = request.LegacyBootstrap.ScorePercentage.Value >= 70.00m ? SkillAssessmentResult.Passed : SkillAssessmentResult.Failed,
                PassFlag = request.LegacyBootstrap.ScorePercentage.Value >= 70.00m,
                AssessedByUserId = _currentUserContext.UserId,
                AssessedOnUtc = now,
                Remarks = request.LegacyBootstrap.Remarks?.Trim() ?? "Legacy activation bootstrap assessment.",
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            };

            await _repository.AddSkillAssessmentAsync(legacyAssessment, cancellationToken);
            skillAssessments.Add(legacyAssessment);
        }

        if (!trainingRecords.Any(record => record.IsCompleted || record.TrainingStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)) &&
            request.LegacyBootstrap is not null &&
            !string.IsNullOrWhiteSpace(request.LegacyBootstrap.TrainingName))
        {
            var legacyTraining = new TrainingRecord
            {
                TechnicianId = technician.TechnicianId,
                TrainingName = request.LegacyBootstrap.TrainingName.Trim(),
                TrainingTitle = request.LegacyBootstrap.TrainingName.Trim(),
                TrainingType = "LegacyActivation",
                TrainingStatus = "Completed",
                CertificationNumber = request.LegacyBootstrap.CertificationNumber?.Trim() ?? string.Empty,
                ScorePercentage = request.LegacyBootstrap.TrainingScorePercentage ?? 100.00m,
                CompletionDateUtc = now,
                TrainingCompletionDateUtc = now,
                IsCompleted = true,
                TrainerUserId = _currentUserContext.UserId,
                Remarks = request.LegacyBootstrap.Remarks?.Trim() ?? "Legacy activation bootstrap training.",
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            };

            await _repository.AddTrainingRecordAsync(legacyTraining, cancellationToken);
            trainingRecords.Add(legacyTraining);
        }

        var eligibility = _eligibilityService.Evaluate(technician, documents, skillAssessments, trainingRecords);

        if (!eligibility.IsEligible)
        {
            throw new AppException(
                ErrorCodes.Conflict,
                $"The technician cannot be activated until onboarding is complete. Pending: {string.Join("; ", eligibility.PendingItems)}",
                409);
        }

        technician.IsActive = true;
        technician.LastUpdated = now;
        technician.UpdatedBy = actor;

        await _repository.AddTechnicianActivationLogAsync(
            new TechnicianActivationLog
            {
                TechnicianId = technician.TechnicianId,
                ActivationAction = "Activated",
                ActivationReason = request.ActivationReason.Trim(),
                ActivatedByUserId = _currentUserContext.UserId,
                ActivatedOnUtc = now,
                EligibilitySnapshot = JsonSerializer.Serialize(new
                {
                    eligibility.IsEligible,
                    eligibility.OnboardingStatus,
                    eligibility.PendingItems,
                    UploadedDocumentCount = documents.Count,
                    VerifiedDocumentCount = documents.Count(document => document.VerificationStatus == TechnicianDocumentStatus.Verified),
                    CompletedTrainingCount = trainingRecords.Count(record => record.IsCompleted || record.TrainingStatus == "Completed")
                }),
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _auditLogRepository.AddAsync(
            TechnicianOnboardingPhaseESupport.CreateAuditLog(
                _currentUserContext,
                now,
                "ActivateTechnician",
                nameof(Coolzo.Domain.Entities.Technician),
                technician.TechnicianCode,
                request.ActivationReason),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await TechnicianOnboardingPhaseESupport.BuildDetailAsync(technician, _repository, _eligibilityService, cancellationToken);
    }
}

public sealed record DeactivateTechnicianCommand(
    long TechnicianId,
    string ActivationReason) : IRequest<TechnicianOnboardingDetailResponse>;

public sealed class DeactivateTechnicianCommandValidator : AbstractValidator<DeactivateTechnicianCommand>
{
    public DeactivateTechnicianCommandValidator()
    {
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleFor(request => request.ActivationReason).NotEmpty().MaximumLength(512);
    }
}

public sealed class DeactivateTechnicianCommandHandler : IRequestHandler<DeactivateTechnicianCommand, TechnicianOnboardingDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly TechnicianOnboardingEligibilityService _eligibilityService;
    private readonly IGapPhaseERepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateTechnicianCommandHandler(
        IGapPhaseERepository repository,
        TechnicianOnboardingEligibilityService eligibilityService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _eligibilityService = eligibilityService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<TechnicianOnboardingDetailResponse> Handle(DeactivateTechnicianCommand request, CancellationToken cancellationToken)
    {
        var technician = await _repository.GetTechnicianAsync(request.TechnicianId, asNoTracking: false, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);
        var now = _currentDateTime.UtcNow;
        var actor = TechnicianOnboardingPhaseESupport.ResolveActor(_currentUserContext, "TechnicianActivation");

        technician.IsActive = false;
        technician.LastUpdated = now;
        technician.UpdatedBy = actor;

        await _repository.AddTechnicianActivationLogAsync(
            new TechnicianActivationLog
            {
                TechnicianId = technician.TechnicianId,
                ActivationAction = "Deactivated",
                ActivationReason = request.ActivationReason.Trim(),
                ActivatedByUserId = _currentUserContext.UserId,
                ActivatedOnUtc = now,
                EligibilitySnapshot = JsonSerializer.Serialize(new
                {
                    Reason = request.ActivationReason.Trim(),
                    WasActive = true
                }),
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _auditLogRepository.AddAsync(
            TechnicianOnboardingPhaseESupport.CreateAuditLog(
                _currentUserContext,
                now,
                "DeactivateTechnician",
                nameof(Coolzo.Domain.Entities.Technician),
                technician.TechnicianCode,
                request.ActivationReason),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await TechnicianOnboardingPhaseESupport.BuildDetailAsync(technician, _repository, _eligibilityService, cancellationToken);
    }
}

public sealed record GetTechnicianActivationHistoryQuery(long TechnicianId) : IRequest<IReadOnlyCollection<TechnicianActivationLogResponse>>;

public sealed class GetTechnicianActivationHistoryQueryHandler : IRequestHandler<GetTechnicianActivationHistoryQuery, IReadOnlyCollection<TechnicianActivationLogResponse>>
{
    private readonly IGapPhaseERepository _repository;

    public GetTechnicianActivationHistoryQueryHandler(IGapPhaseERepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<TechnicianActivationLogResponse>> Handle(GetTechnicianActivationHistoryQuery request, CancellationToken cancellationToken)
    {
        var logs = await _repository.GetTechnicianActivationLogsAsync(request.TechnicianId, cancellationToken);
        return logs.Select(TechnicianOnboardingPhaseESupport.MapActivationLog).ToArray();
    }
}

internal static class TechnicianOnboardingPhaseESupport
{
    public static string ResolveActor(ICurrentUserContext currentUserContext, string fallback)
    {
        return string.IsNullOrWhiteSpace(currentUserContext.UserName) ? fallback : currentUserContext.UserName;
    }

    public static Coolzo.Domain.Entities.AuditLog CreateAuditLog(
        ICurrentUserContext currentUserContext,
        DateTime now,
        string actionName,
        string entityName,
        string entityId,
        string newValues)
    {
        return new Coolzo.Domain.Entities.AuditLog
        {
            UserId = currentUserContext.UserId,
            ActionName = actionName,
            EntityName = entityName,
            EntityId = entityId,
            TraceId = currentUserContext.TraceId,
            StatusName = "Success",
            NewValues = newValues,
            CreatedBy = ResolveActor(currentUserContext, actionName),
            DateCreated = now,
            IPAddress = currentUserContext.IPAddress
        };
    }

    public static async Task<TechnicianOnboardingDetailResponse> BuildDetailAsync(
        Coolzo.Domain.Entities.Technician technician,
        IGapPhaseERepository repository,
        TechnicianOnboardingEligibilityService eligibilityService,
        CancellationToken cancellationToken)
    {
        var documents = await repository.GetTechnicianDocumentsAsync(technician.TechnicianId, cancellationToken);
        var assessments = await repository.GetSkillAssessmentsAsync(technician.TechnicianId, cancellationToken);
        var trainingRecords = await repository.GetTrainingRecordsAsync(technician.TechnicianId, cancellationToken);
        var activationLogs = await repository.GetTechnicianActivationLogsAsync(technician.TechnicianId, cancellationToken);
        var eligibility = eligibilityService.Evaluate(technician, documents, assessments, trainingRecords);

        return new TechnicianOnboardingDetailResponse(
            technician.TechnicianId,
            technician.TechnicianCode,
            technician.TechnicianName,
            technician.MobileNumber,
            technician.EmailAddress,
            technician.BaseZoneId,
            technician.MaxDailyAssignments,
            technician.IsActive,
            eligibility.OnboardingStatus,
            eligibility.IsEligible,
            eligibility.PendingItems,
            documents.Select(MapDocument).ToArray(),
            assessments.Select(MapAssessment).ToArray(),
            trainingRecords.Select(MapTrainingRecord).ToArray(),
            activationLogs.Select(MapActivationLog).ToArray());
    }

    public static TechnicianOnboardingListItemResponse MapListItem(
        Coolzo.Domain.Entities.Technician technician,
        IReadOnlyCollection<TechnicianDocument> documents,
        IReadOnlyCollection<SkillAssessment> assessments,
        IReadOnlyCollection<TrainingRecord> trainingRecords,
        TechnicianOnboardingEligibilityResult eligibility)
    {
        return new TechnicianOnboardingListItemResponse(
            technician.TechnicianId,
            technician.TechnicianCode,
            technician.TechnicianName,
            technician.MobileNumber,
            technician.EmailAddress,
            technician.IsActive,
            eligibility.OnboardingStatus,
            documents.Count,
            documents.Count(document => document.VerificationStatus == TechnicianDocumentStatus.Verified),
            assessments.OrderByDescending(item => item.AssessedOnUtc ?? item.DateCreated).FirstOrDefault()?.AssessmentResult.ToString() ?? SkillAssessmentResult.Pending.ToString(),
            trainingRecords.Count(item => item.IsCompleted || item.TrainingStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)),
            eligibility.IsEligible);
    }

    public static TechnicianDocumentDetailResponse MapDocument(TechnicianDocument document)
    {
        return new TechnicianDocumentDetailResponse(
            document.TechnicianDocumentId,
            document.DocumentType,
            document.DocumentNumber,
            string.IsNullOrWhiteSpace(document.StorageUrl) ? document.DocumentUrl : document.StorageUrl,
            document.VerificationStatus.ToString(),
            document.VerificationRemarks,
            document.ExpiryDateUtc,
            document.VerifiedByUserId,
            document.VerifiedOnUtc);
    }

    public static SkillAssessmentDetailResponse MapAssessment(SkillAssessment assessment)
    {
        return new SkillAssessmentDetailResponse(
            assessment.SkillAssessmentId,
            assessment.SkillTagId,
            assessment.AssessmentCode,
            string.IsNullOrWhiteSpace(assessment.AssessmentName) ? assessment.AssessmentCode : assessment.AssessmentName,
            assessment.AssessmentStatus,
            assessment.ScorePercentage,
            assessment.AssessmentResult.ToString(),
            assessment.PassFlag,
            assessment.AssessedByUserId,
            assessment.AssessedOnUtc,
            assessment.Remarks);
    }

    public static TrainingRecordDetailResponse MapTrainingRecord(TrainingRecord trainingRecord)
    {
        return new TrainingRecordDetailResponse(
            trainingRecord.TrainingRecordId,
            string.IsNullOrWhiteSpace(trainingRecord.TrainingTitle) ? trainingRecord.TrainingName : trainingRecord.TrainingTitle,
            trainingRecord.TrainingType,
            trainingRecord.TrainingStatus,
            trainingRecord.CertificationNumber,
            trainingRecord.ScorePercentage,
            trainingRecord.IsCompleted,
            trainingRecord.TrainingCompletionDateUtc ?? (trainingRecord.CompletionDateUtc == DateTime.MinValue ? null : trainingRecord.CompletionDateUtc),
            trainingRecord.TrainerUserId,
            trainingRecord.CertificateUrl,
            trainingRecord.Remarks);
    }

    public static TechnicianActivationLogResponse MapActivationLog(TechnicianActivationLog log)
    {
        return new TechnicianActivationLogResponse(
            log.TechnicianActivationLogId,
            log.ActivationAction,
            log.ActivationReason,
            log.ActivatedByUserId,
            log.ActivatedOnUtc,
            log.EligibilitySnapshot);
    }

    public static TechnicianOnboardingResponse ToLegacyResponse(TechnicianOnboardingDetailResponse detail)
    {
        return new TechnicianOnboardingResponse(
            detail.TechnicianId,
            detail.TechnicianCode,
            detail.TechnicianName,
            detail.IsActive,
            detail.Documents.Count,
            detail.SkillAssessments.OrderByDescending(item => item.AssessedOnUtc ?? DateTime.MinValue).FirstOrDefault()?.AssessmentResult ?? SkillAssessmentResult.Pending.ToString(),
            detail.TrainingRecords.Count(item => item.IsCompleted));
    }
}
