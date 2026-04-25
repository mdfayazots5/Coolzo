using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Requests.Technician;
using Coolzo.Contracts.Responses.GapPhaseE;
using Coolzo.Contracts.Responses.Technician;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;
using TechnicianEntity = Coolzo.Domain.Entities.Technician;

namespace Coolzo.Application.Features.Technician.Management;

public sealed record SearchTechniciansQuery(
    string? SearchTerm,
    bool ActiveOnly,
    string? ZoneName,
    string? SkillName,
    string? Availability,
    decimal? MinimumRating) : IRequest<IReadOnlyCollection<TechnicianListItemResponse>>;

public sealed class SearchTechniciansQueryValidator : AbstractValidator<SearchTechniciansQuery>
{
    public SearchTechniciansQueryValidator()
    {
        RuleFor(request => request.MinimumRating).InclusiveBetween(0, 5).When(request => request.MinimumRating.HasValue);
        RuleFor(request => request.Availability)
            .Must(value => value is null ||
                value.Equals("available", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("on-job", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("off-duty", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("on-leave", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Availability filter is invalid.");
    }
}

public sealed class SearchTechniciansQueryHandler : IRequestHandler<SearchTechniciansQuery, IReadOnlyCollection<TechnicianListItemResponse>>
{
    private readonly ITechnicianRepository _technicianRepository;

    public SearchTechniciansQueryHandler(ITechnicianRepository technicianRepository)
    {
        _technicianRepository = technicianRepository;
    }

    public async Task<IReadOnlyCollection<TechnicianListItemResponse>> Handle(SearchTechniciansQuery request, CancellationToken cancellationToken)
    {
        var technicians = await _technicianRepository.SearchManagementAsync(
            request.SearchTerm,
            request.ActiveOnly,
            request.ZoneName,
            request.SkillName,
            request.Availability,
            request.MinimumRating,
            cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return technicians.Select(technician => TechnicianManagementSupport.ToListItem(technician, today)).ToArray();
    }
}

public sealed record GetTechnicianProfileQuery(long TechnicianId) : IRequest<TechnicianDetailResponse>;

public sealed class GetTechnicianProfileQueryHandler : IRequestHandler<GetTechnicianProfileQuery, TechnicianDetailResponse>
{
    private readonly TechnicianOnboardingEligibilityService _eligibilityService;
    private readonly IGapPhaseERepository _gapPhaseERepository;
    private readonly ITechnicianRepository _technicianRepository;

    public GetTechnicianProfileQueryHandler(
        ITechnicianRepository technicianRepository,
        IGapPhaseERepository gapPhaseERepository,
        TechnicianOnboardingEligibilityService eligibilityService)
    {
        _technicianRepository = technicianRepository;
        _gapPhaseERepository = gapPhaseERepository;
        _eligibilityService = eligibilityService;
    }

    public async Task<TechnicianDetailResponse> Handle(GetTechnicianProfileQuery request, CancellationToken cancellationToken)
    {
        var technician = await _technicianRepository.GetManagementDetailAsync(request.TechnicianId, asNoTracking: true, cancellationToken: cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);

        var (documents, assessments, trainingRecords) = await TechnicianManagementSupport.LoadOnboardingArtifactsAsync(
            request.TechnicianId,
            _gapPhaseERepository,
            cancellationToken);
        var eligibility = _eligibilityService.Evaluate(technician, documents, assessments, trainingRecords);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return TechnicianManagementSupport.ToDetailResponse(technician, eligibility, documents, assessments, trainingRecords, today);
    }
}

public sealed record CreateTechnicianProfileCommand(
    string TechnicianName,
    string MobileNumber,
    string? EmailAddress,
    long? BaseZoneId,
    int MaxDailyAssignments,
    IReadOnlyCollection<TechnicianSkillRequest>? Skills,
    IReadOnlyCollection<long>? ZoneIds) : IRequest<TechnicianDetailResponse>;

public sealed class CreateTechnicianProfileCommandValidator : AbstractValidator<CreateTechnicianProfileCommand>
{
    public CreateTechnicianProfileCommandValidator()
    {
        RuleFor(request => request.TechnicianName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.MobileNumber).Matches("^[0-9]{8,16}$");
        RuleFor(request => request.EmailAddress).EmailAddress().When(request => !string.IsNullOrWhiteSpace(request.EmailAddress));
        RuleFor(request => request.MaxDailyAssignments).InclusiveBetween(1, 16);
        When(request => request.Skills is not null, () =>
        {
            RuleForEach(request => request.Skills!).ChildRules(skill =>
            {
                skill.RuleFor(item => item.SkillCode).MaximumLength(64);
                skill.RuleFor(item => item.SkillName).NotEmpty().MaximumLength(128);
                skill.RuleFor(item => item.SkillCategory).NotEmpty().MaximumLength(32);
            });
        });
    }
}

public sealed class CreateTechnicianProfileCommandHandler : IRequestHandler<CreateTechnicianProfileCommand, TechnicianDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly TechnicianOnboardingEligibilityService _eligibilityService;
    private readonly IGapPhaseERepository _gapPhaseERepository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTechnicianProfileCommandHandler(
        ITechnicianRepository technicianRepository,
        IGapPhaseERepository gapPhaseERepository,
        TechnicianOnboardingEligibilityService eligibilityService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianRepository = technicianRepository;
        _gapPhaseERepository = gapPhaseERepository;
        _eligibilityService = eligibilityService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<TechnicianDetailResponse> Handle(CreateTechnicianProfileCommand request, CancellationToken cancellationToken)
    {
        var normalizedMobileNumber = request.MobileNumber.Trim();

        if (await _technicianRepository.MobileExistsAsync(normalizedMobileNumber, null, cancellationToken))
        {
            throw new AppException(ErrorCodes.DuplicateValue, "A technician already exists for this mobile number.", 409);
        }

        var zoneIds = TechnicianManagementSupport.NormalizeZoneIds(request.ZoneIds, request.BaseZoneId);
        await TechnicianManagementSupport.EnsureZonesExistAsync(zoneIds, _technicianRepository, cancellationToken);

        var now = _currentDateTime.UtcNow;
        var actor = TechnicianManagementSupport.ResolveActor(_currentUserContext, "TechnicianManagement");

        var technician = new TechnicianEntity
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

        TechnicianManagementSupport.ApplySkills(technician, request.Skills, actor, now, _currentUserContext.IPAddress);
        TechnicianManagementSupport.ApplyZones(technician, zoneIds, request.BaseZoneId, actor, now, _currentUserContext.IPAddress);

        await _technicianRepository.AddAsync(technician, cancellationToken);
        await _auditLogRepository.AddAsync(
            TechnicianManagementSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "CreateTechnicianProfile",
                nameof(TechnicianEntity),
                technician.TechnicianCode,
                technician.TechnicianName),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await TechnicianManagementSupport.BuildDetailResponseAsync(
            technician.TechnicianId,
            _technicianRepository,
            _gapPhaseERepository,
            _eligibilityService,
            cancellationToken);
    }
}

public sealed record UpdateTechnicianProfileCommand(
    long TechnicianId,
    string TechnicianName,
    string MobileNumber,
    string? EmailAddress,
    long? BaseZoneId,
    int MaxDailyAssignments,
    bool IsActive) : IRequest<TechnicianDetailResponse>;

public sealed class UpdateTechnicianProfileCommandValidator : AbstractValidator<UpdateTechnicianProfileCommand>
{
    public UpdateTechnicianProfileCommandValidator()
    {
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleFor(request => request.TechnicianName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.MobileNumber).Matches("^[0-9]{8,16}$");
        RuleFor(request => request.EmailAddress).EmailAddress().When(request => !string.IsNullOrWhiteSpace(request.EmailAddress));
        RuleFor(request => request.MaxDailyAssignments).InclusiveBetween(1, 16);
    }
}

public sealed class UpdateTechnicianProfileCommandHandler : IRequestHandler<UpdateTechnicianProfileCommand, TechnicianDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly TechnicianOnboardingEligibilityService _eligibilityService;
    private readonly IGapPhaseERepository _gapPhaseERepository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTechnicianProfileCommandHandler(
        ITechnicianRepository technicianRepository,
        IGapPhaseERepository gapPhaseERepository,
        TechnicianOnboardingEligibilityService eligibilityService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianRepository = technicianRepository;
        _gapPhaseERepository = gapPhaseERepository;
        _eligibilityService = eligibilityService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<TechnicianDetailResponse> Handle(UpdateTechnicianProfileCommand request, CancellationToken cancellationToken)
    {
        var technician = await _technicianRepository.GetManagementDetailAsync(request.TechnicianId, asNoTracking: false, cancellationToken: cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);

        var normalizedMobileNumber = request.MobileNumber.Trim();

        if (await _technicianRepository.MobileExistsAsync(normalizedMobileNumber, request.TechnicianId, cancellationToken))
        {
            throw new AppException(ErrorCodes.DuplicateValue, "A technician already exists for this mobile number.", 409);
        }

        if (request.BaseZoneId.HasValue)
        {
            await TechnicianManagementSupport.EnsureZonesExistAsync([request.BaseZoneId.Value], _technicianRepository, cancellationToken);
        }

        technician.TechnicianName = request.TechnicianName.Trim();
        technician.MobileNumber = normalizedMobileNumber;
        technician.EmailAddress = request.EmailAddress?.Trim() ?? string.Empty;
        technician.BaseZoneId = request.BaseZoneId;
        technician.MaxDailyAssignments = request.MaxDailyAssignments;
        technician.IsActive = request.IsActive;
        technician.LastUpdated = _currentDateTime.UtcNow;
        technician.UpdatedBy = TechnicianManagementSupport.ResolveActor(_currentUserContext, "TechnicianManagement");
        technician.IPAddress = _currentUserContext.IPAddress;

        await _auditLogRepository.AddAsync(
            TechnicianManagementSupport.CreateAuditLog(
                _currentUserContext,
                _currentDateTime.UtcNow,
                "UpdateTechnicianProfile",
                nameof(TechnicianEntity),
                technician.TechnicianId.ToString(),
                technician.TechnicianName),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await TechnicianManagementSupport.BuildDetailResponseAsync(
            request.TechnicianId,
            _technicianRepository,
            _gapPhaseERepository,
            _eligibilityService,
            cancellationToken);
    }
}

public sealed record UpdateTechnicianSkillsCommand(
    long TechnicianId,
    IReadOnlyCollection<TechnicianSkillRequest> Skills) : IRequest<IReadOnlyCollection<TechnicianSkillResponse>>;

public sealed class UpdateTechnicianSkillsCommandValidator : AbstractValidator<UpdateTechnicianSkillsCommand>
{
    public UpdateTechnicianSkillsCommandValidator()
    {
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleForEach(request => request.Skills).ChildRules(skill =>
        {
            skill.RuleFor(item => item.SkillCode).MaximumLength(64);
            skill.RuleFor(item => item.SkillName).NotEmpty().MaximumLength(128);
            skill.RuleFor(item => item.SkillCategory).NotEmpty().MaximumLength(32);
        });
    }
}

public sealed class UpdateTechnicianSkillsCommandHandler : IRequestHandler<UpdateTechnicianSkillsCommand, IReadOnlyCollection<TechnicianSkillResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTechnicianSkillsCommandHandler(
        ITechnicianRepository technicianRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianRepository = technicianRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<TechnicianSkillResponse>> Handle(UpdateTechnicianSkillsCommand request, CancellationToken cancellationToken)
    {
        var technician = await _technicianRepository.GetManagementDetailAsync(request.TechnicianId, asNoTracking: false, cancellationToken: cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);

        var now = _currentDateTime.UtcNow;
        var actor = TechnicianManagementSupport.ResolveActor(_currentUserContext, "TechnicianManagement");

        foreach (var existingSkill in technician.Skills.Where(entity => !entity.IsDeleted))
        {
            existingSkill.IsDeleted = true;
            existingSkill.DateDeleted = now;
            existingSkill.DeletedBy = actor;
            existingSkill.LastUpdated = now;
            existingSkill.UpdatedBy = actor;
        }

        TechnicianManagementSupport.ApplySkills(technician, request.Skills, actor, now, _currentUserContext.IPAddress);

        await _auditLogRepository.AddAsync(
            TechnicianManagementSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "UpdateTechnicianSkills",
                nameof(TechnicianEntity),
                technician.TechnicianId.ToString(),
                string.Join(", ", request.Skills.Select(item => item.SkillName.Trim()))),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return technician.Skills
            .Where(entity => !entity.IsDeleted)
            .OrderBy(entity => entity.SkillName)
            .Select(TechnicianManagementSupport.ToSkillResponse)
            .ToArray();
    }
}

public sealed record UpdateTechnicianZonesCommand(
    long TechnicianId,
    IReadOnlyCollection<long> ZoneIds,
    long? PrimaryZoneId) : IRequest<IReadOnlyCollection<TechnicianZoneResponse>>;

public sealed class UpdateTechnicianZonesCommandValidator : AbstractValidator<UpdateTechnicianZonesCommand>
{
    public UpdateTechnicianZonesCommandValidator()
    {
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleFor(request => request.ZoneIds).NotEmpty();
        RuleFor(request => request.PrimaryZoneId)
            .Must((request, primaryZoneId) => !primaryZoneId.HasValue || request.ZoneIds.Contains(primaryZoneId.Value))
            .WithMessage("Primary zone must be one of the assigned zones.");
    }
}

public sealed class UpdateTechnicianZonesCommandHandler : IRequestHandler<UpdateTechnicianZonesCommand, IReadOnlyCollection<TechnicianZoneResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTechnicianZonesCommandHandler(
        ITechnicianRepository technicianRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianRepository = technicianRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<TechnicianZoneResponse>> Handle(UpdateTechnicianZonesCommand request, CancellationToken cancellationToken)
    {
        var technician = await _technicianRepository.GetManagementDetailAsync(request.TechnicianId, asNoTracking: false, cancellationToken: cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);

        var normalizedZoneIds = TechnicianManagementSupport.NormalizeZoneIds(request.ZoneIds, request.PrimaryZoneId ?? technician.BaseZoneId);
        var resolvedZones = await TechnicianManagementSupport.EnsureZonesExistAsync(normalizedZoneIds, _technicianRepository, cancellationToken);
        var now = _currentDateTime.UtcNow;
        var actor = TechnicianManagementSupport.ResolveActor(_currentUserContext, "TechnicianManagement");

        foreach (var existingZone in technician.Zones.Where(entity => !entity.IsDeleted))
        {
            existingZone.IsDeleted = true;
            existingZone.DateDeleted = now;
            existingZone.DeletedBy = actor;
            existingZone.LastUpdated = now;
            existingZone.UpdatedBy = actor;
        }

        TechnicianManagementSupport.ApplyZones(
            technician,
            normalizedZoneIds,
            request.PrimaryZoneId ?? technician.BaseZoneId,
            actor,
            now,
            _currentUserContext.IPAddress);

        await _auditLogRepository.AddAsync(
            TechnicianManagementSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "UpdateTechnicianZones",
                nameof(TechnicianEntity),
                technician.TechnicianId.ToString(),
                string.Join(", ", resolvedZones.Select(entity => entity.ZoneName))),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var zoneLookup = resolvedZones.ToDictionary(entity => entity.ZoneId);
        return technician.Zones
            .Where(entity => !entity.IsDeleted && zoneLookup.ContainsKey(entity.ZoneId))
            .OrderByDescending(entity => entity.IsPrimaryZone)
            .ThenBy(entity => zoneLookup[entity.ZoneId].ZoneName)
            .Select(entity => TechnicianManagementSupport.ToZoneResponse(entity, zoneLookup[entity.ZoneId]))
            .ToArray();
    }
}

public sealed record GetTechnicianPerformanceQuery(
    long TechnicianId,
    DateOnly? FromDate,
    DateOnly? ToDate) : IRequest<TechnicianPerformanceResponse>;

public sealed class GetTechnicianPerformanceQueryHandler : IRequestHandler<GetTechnicianPerformanceQuery, TechnicianPerformanceResponse>
{
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GetTechnicianPerformanceQueryHandler(
        ITechnicianRepository technicianRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianRepository = technicianRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<TechnicianPerformanceResponse> Handle(GetTechnicianPerformanceQuery request, CancellationToken cancellationToken)
    {
        var technician = await _technicianRepository.GetByIdAsync(request.TechnicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);

        var toDate = request.ToDate ?? DateOnly.FromDateTime(_currentDateTime.UtcNow);
        var fromDate = request.FromDate ?? toDate.AddDays(-6);

        if (fromDate > toDate)
        {
            throw new AppException(ErrorCodes.ValidationFailure, "Performance date range is invalid.", 400);
        }

        var metrics = await _technicianRepository.BuildPerformanceMetricsAsync(request.TechnicianId, fromDate, toDate, cancellationToken);
        var teamAverage = await _technicianRepository.GetTeamAverageSlaComplianceAsync(fromDate, toDate, cancellationToken);
        var now = _currentDateTime.UtcNow;
        var actor = TechnicianManagementSupport.ResolveActor(_currentUserContext, "TechnicianManagement");
        var summary = await _technicianRepository.GetPerformanceSummaryAsync(request.TechnicianId, toDate, asNoTracking: false, cancellationToken: cancellationToken);

        if (summary is null)
        {
            summary = new TechnicianPerformanceSummary
            {
                TechnicianId = technician.TechnicianId,
                SummaryDate = toDate,
                AverageRating = metrics.AverageRating,
                TotalJobs = metrics.TotalJobs,
                CompletedJobs = metrics.CompletedJobs,
                SlaCompliancePercent = metrics.SlaCompliancePercent,
                RevisitRatePercent = metrics.RevisitRatePercent,
                RevenueGenerated = metrics.RevenueGenerated,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            };

            await _technicianRepository.AddPerformanceSummaryAsync(summary, cancellationToken);
        }
        else
        {
            summary.AverageRating = metrics.AverageRating;
            summary.TotalJobs = metrics.TotalJobs;
            summary.CompletedJobs = metrics.CompletedJobs;
            summary.SlaCompliancePercent = metrics.SlaCompliancePercent;
            summary.RevisitRatePercent = metrics.RevisitRatePercent;
            summary.RevenueGenerated = metrics.RevenueGenerated;
            summary.LastUpdated = now;
            summary.UpdatedBy = actor;
            summary.IPAddress = _currentUserContext.IPAddress;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TechnicianManagementSupport.ToPerformanceResponse(metrics, teamAverage);
    }
}

public sealed record GetTechnicianAttendanceQuery(
    long TechnicianId,
    int Year,
    int Month) : IRequest<IReadOnlyCollection<TechnicianAttendanceResponse>>;

public sealed class GetTechnicianAttendanceQueryValidator : AbstractValidator<GetTechnicianAttendanceQuery>
{
    public GetTechnicianAttendanceQueryValidator()
    {
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleFor(request => request.Year).InclusiveBetween(2020, 2100);
        RuleFor(request => request.Month).InclusiveBetween(1, 12);
    }
}

public sealed class GetTechnicianAttendanceQueryHandler : IRequestHandler<GetTechnicianAttendanceQuery, IReadOnlyCollection<TechnicianAttendanceResponse>>
{
    private readonly ITechnicianRepository _technicianRepository;

    public GetTechnicianAttendanceQueryHandler(ITechnicianRepository technicianRepository)
    {
        _technicianRepository = technicianRepository;
    }

    public async Task<IReadOnlyCollection<TechnicianAttendanceResponse>> Handle(GetTechnicianAttendanceQuery request, CancellationToken cancellationToken)
    {
        var technician = await _technicianRepository.GetByIdAsync(request.TechnicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);

        _ = technician;

        var fromDate = new DateOnly(request.Year, request.Month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);
        var attendances = await _technicianRepository.GetAttendanceAsync(request.TechnicianId, fromDate, toDate, cancellationToken);

        return attendances.Select(TechnicianManagementSupport.ToAttendanceResponse).ToArray();
    }
}

public sealed record RequestTechnicianLeaveCommand(
    long TechnicianId,
    DateOnly LeaveDate,
    string? LeaveReason) : IRequest<TechnicianAttendanceResponse>;

public sealed class RequestTechnicianLeaveCommandValidator : AbstractValidator<RequestTechnicianLeaveCommand>
{
    public RequestTechnicianLeaveCommandValidator()
    {
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleFor(request => request.LeaveReason).MaximumLength(512);
    }
}

public sealed class RequestTechnicianLeaveCommandHandler : IRequestHandler<RequestTechnicianLeaveCommand, TechnicianAttendanceResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RequestTechnicianLeaveCommandHandler(
        ITechnicianRepository technicianRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianRepository = technicianRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<TechnicianAttendanceResponse> Handle(RequestTechnicianLeaveCommand request, CancellationToken cancellationToken)
    {
        var technician = await _technicianRepository.GetByIdAsync(request.TechnicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);

        _ = technician;

        var existingAttendance = await _technicianRepository.GetAttendanceByDateAsync(request.TechnicianId, request.LeaveDate, asNoTracking: false, cancellationToken: cancellationToken);
        if (existingAttendance is not null &&
            !existingAttendance.AttendanceStatus.Equals("LeaveRejected", StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException(ErrorCodes.Conflict, "A leave or attendance record already exists for the requested date.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var actor = TechnicianManagementSupport.ResolveActor(_currentUserContext, "TechnicianManagement");

        var attendance = existingAttendance ?? new TechnicianAttendance
        {
            TechnicianId = request.TechnicianId,
            AttendanceDate = request.LeaveDate,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        attendance.AttendanceStatus = "LeaveRequested";
        attendance.LeaveReason = request.LeaveReason?.Trim() ?? string.Empty;
        attendance.LocationText = string.Empty;
        attendance.CheckInOnUtc = null;
        attendance.CheckOutOnUtc = null;
        attendance.LastUpdated = now;
        attendance.UpdatedBy = actor;
        attendance.ReviewedByUserId = null;
        attendance.ReviewedOnUtc = null;

        if (existingAttendance is null)
        {
            await _technicianRepository.AddAttendanceAsync(attendance, cancellationToken);
        }

        await _auditLogRepository.AddAsync(
            TechnicianManagementSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "RequestTechnicianLeave",
                nameof(TechnicianAttendance),
                request.TechnicianId.ToString(),
                request.LeaveDate.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TechnicianManagementSupport.ToAttendanceResponse(attendance);
    }
}

public sealed record ReviewTechnicianLeaveCommand(
    long TechnicianId,
    long LeaveRequestId,
    string Decision,
    string? Remarks) : IRequest<TechnicianAttendanceResponse>;

public sealed class ReviewTechnicianLeaveCommandValidator : AbstractValidator<ReviewTechnicianLeaveCommand>
{
    public ReviewTechnicianLeaveCommandValidator()
    {
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleFor(request => request.LeaveRequestId).GreaterThan(0);
        RuleFor(request => request.Decision)
            .Must(value => value.Equals("approve", StringComparison.OrdinalIgnoreCase) || value.Equals("reject", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Decision must be approve or reject.");
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class ReviewTechnicianLeaveCommandHandler : IRequestHandler<ReviewTechnicianLeaveCommand, TechnicianAttendanceResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReviewTechnicianLeaveCommandHandler(
        ITechnicianRepository technicianRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianRepository = technicianRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<TechnicianAttendanceResponse> Handle(ReviewTechnicianLeaveCommand request, CancellationToken cancellationToken)
    {
        var attendance = await _technicianRepository.GetAttendanceByIdAsync(request.TechnicianId, request.LeaveRequestId, asNoTracking: false, cancellationToken: cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The leave request could not be found.", 404);

        if (!attendance.AttendanceStatus.Equals("LeaveRequested", StringComparison.OrdinalIgnoreCase) &&
            !attendance.AttendanceStatus.Equals("LeaveApproved", StringComparison.OrdinalIgnoreCase) &&
            !attendance.AttendanceStatus.Equals("LeaveRejected", StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException(ErrorCodes.Conflict, "The attendance record is not a leave request.", 409);
        }

        var now = _currentDateTime.UtcNow;
        attendance.AttendanceStatus = request.Decision.Equals("approve", StringComparison.OrdinalIgnoreCase)
            ? "LeaveApproved"
            : "LeaveRejected";
        attendance.ReviewedByUserId = _currentUserContext.UserId;
        attendance.ReviewedOnUtc = now;
        attendance.LeaveReason = string.IsNullOrWhiteSpace(request.Remarks)
            ? attendance.LeaveReason
            : request.Remarks.Trim();
        attendance.LastUpdated = now;
        attendance.UpdatedBy = TechnicianManagementSupport.ResolveActor(_currentUserContext, "TechnicianManagement");
        attendance.IPAddress = _currentUserContext.IPAddress;

        await _auditLogRepository.AddAsync(
            TechnicianManagementSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "ReviewTechnicianLeave",
                nameof(TechnicianAttendance),
                attendance.TechnicianAttendanceId.ToString(),
                attendance.AttendanceStatus),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TechnicianManagementSupport.ToAttendanceResponse(attendance);
    }
}

public sealed record GetTechnicianAvailabilityBoardQuery(
    long? ServiceRequestId) : IRequest<IReadOnlyCollection<TechnicianListItemResponse>>;

public sealed class GetTechnicianAvailabilityBoardQueryHandler : IRequestHandler<GetTechnicianAvailabilityBoardQuery, IReadOnlyCollection<TechnicianListItemResponse>>
{
    private readonly ITechnicianRepository _technicianRepository;

    public GetTechnicianAvailabilityBoardQueryHandler(ITechnicianRepository technicianRepository)
    {
        _technicianRepository = technicianRepository;
    }

    public async Task<IReadOnlyCollection<TechnicianListItemResponse>> Handle(GetTechnicianAvailabilityBoardQuery request, CancellationToken cancellationToken)
    {
        var technicians = await _technicianRepository.SearchManagementAsync(
            searchTerm: null,
            activeOnly: false,
            zoneName: null,
            skillName: null,
            availability: null,
            minimumRating: null,
            cancellationToken: cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (!request.ServiceRequestId.HasValue)
        {
            return technicians.Select(technician => TechnicianManagementSupport.ToListItem(technician, today)).ToArray();
        }

        var snapshots = await _technicianRepository.GetAvailabilityByServiceRequestIdAsync(request.ServiceRequestId.Value, cancellationToken);
        var snapshotLookup = snapshots.ToDictionary(entity => entity.TechnicianId);

        return technicians
            .Select(technician => TechnicianManagementSupport.ToAvailabilityBoardItem(
                technician,
                snapshotLookup.TryGetValue(technician.TechnicianId, out var snapshot) ? snapshot : null,
                today))
            .ToArray();
    }
}

public sealed record GetTechnicianGpsLogQuery(
    long TechnicianId,
    DateOnly TrackingDate) : IRequest<IReadOnlyCollection<TechnicianGpsLogResponse>>;

public sealed class GetTechnicianGpsLogQueryHandler : IRequestHandler<GetTechnicianGpsLogQuery, IReadOnlyCollection<TechnicianGpsLogResponse>>
{
    private readonly ITechnicianRepository _technicianRepository;

    public GetTechnicianGpsLogQueryHandler(ITechnicianRepository technicianRepository)
    {
        _technicianRepository = technicianRepository;
    }

    public async Task<IReadOnlyCollection<TechnicianGpsLogResponse>> Handle(GetTechnicianGpsLogQuery request, CancellationToken cancellationToken)
    {
        var technician = await _technicianRepository.GetByIdAsync(request.TechnicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);

        _ = technician;

        var logs = await _technicianRepository.GetGpsLogsAsync(request.TechnicianId, request.TrackingDate, cancellationToken);
        return logs.Select(TechnicianManagementSupport.ToGpsLogResponse).ToArray();
    }
}

internal static class TechnicianManagementSupport
{
    public static async Task<(IReadOnlyCollection<TechnicianDocument> Documents, IReadOnlyCollection<SkillAssessment> Assessments, IReadOnlyCollection<TrainingRecord> TrainingRecords)> LoadOnboardingArtifactsAsync(
        long technicianId,
        IGapPhaseERepository gapPhaseERepository,
        CancellationToken cancellationToken)
    {
        var documents = await SafeLoadAsync(() => gapPhaseERepository.GetTechnicianDocumentsAsync(technicianId, cancellationToken));
        var assessments = await SafeLoadAsync(() => gapPhaseERepository.GetSkillAssessmentsAsync(technicianId, cancellationToken));
        var trainingRecords = await SafeLoadAsync(() => gapPhaseERepository.GetTrainingRecordsAsync(technicianId, cancellationToken));

        return (documents, assessments, trainingRecords);
    }

    public static string ResolveActor(ICurrentUserContext currentUserContext, string fallback)
    {
        return string.IsNullOrWhiteSpace(currentUserContext.UserName) ? fallback : currentUserContext.UserName;
    }

    public static AuditLog CreateAuditLog(
        ICurrentUserContext currentUserContext,
        DateTime now,
        string actionName,
        string entityName,
        string entityId,
        string newValues)
    {
        return new AuditLog
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

    public static IReadOnlyCollection<long> NormalizeZoneIds(IReadOnlyCollection<long>? zoneIds, long? baseZoneId)
    {
        var normalized = (zoneIds ?? Array.Empty<long>())
            .Where(entity => entity > 0)
            .Distinct()
            .ToList();

        if (baseZoneId.HasValue && baseZoneId.Value > 0 && !normalized.Contains(baseZoneId.Value))
        {
            normalized.Add(baseZoneId.Value);
        }

        return normalized;
    }

    public static async Task<IReadOnlyCollection<Zone>> EnsureZonesExistAsync(
        IReadOnlyCollection<long> zoneIds,
        ITechnicianRepository technicianRepository,
        CancellationToken cancellationToken)
    {
        if (zoneIds.Count == 0)
        {
            return Array.Empty<Zone>();
        }

        var zones = await technicianRepository.GetZonesByIdsAsync(zoneIds, cancellationToken);
        if (zones.Count != zoneIds.Count)
        {
            throw new AppException(ErrorCodes.ValidationFailure, "One or more zone assignments are invalid.", 400);
        }

        return zones;
    }

    public static void ApplySkills(
        TechnicianEntity technician,
        IReadOnlyCollection<TechnicianSkillRequest>? skills,
        string actor,
        DateTime now,
        string ipAddress)
    {
        foreach (var skill in (skills ?? Array.Empty<TechnicianSkillRequest>())
                     .Where(item => !string.IsNullOrWhiteSpace(item.SkillName))
                     .GroupBy(item => item.SkillName.Trim(), StringComparer.OrdinalIgnoreCase)
                     .Select(group => group.First()))
        {
            technician.Skills.Add(
                new TechnicianSkill
                {
                    SkillCode = skill.SkillCode?.Trim() ?? string.Empty,
                    SkillName = skill.SkillName.Trim(),
                    SkillCategory = skill.SkillCategory.Trim(),
                    CertifiedOnUtc = skill.CertifiedOnUtc,
                    CreatedBy = actor,
                    DateCreated = now,
                    IPAddress = ipAddress
                });
        }
    }

    public static void ApplyZones(
        TechnicianEntity technician,
        IReadOnlyCollection<long> zoneIds,
        long? primaryZoneId,
        string actor,
        DateTime now,
        string ipAddress)
    {
        foreach (var zoneId in zoneIds)
        {
            technician.Zones.Add(
                new TechnicianZone
                {
                    ZoneId = zoneId,
                    IsPrimaryZone = primaryZoneId.HasValue
                        ? primaryZoneId.Value == zoneId
                        : zoneIds.FirstOrDefault() == zoneId,
                    CreatedBy = actor,
                    DateCreated = now,
                    IPAddress = ipAddress
                });
        }
    }

    public static TechnicianListItemResponse ToListItem(TechnicianEntity technician, DateOnly today)
    {
        var latestSummary = technician.PerformanceSummaries
            .Where(entity => !entity.IsDeleted)
            .OrderByDescending(entity => entity.SummaryDate)
            .ThenByDescending(entity => entity.TechnicianPerformanceSummaryId)
            .FirstOrDefault();
        var activeAssignment = technician.ServiceRequestAssignments
            .Where(entity => entity.IsActiveAssignment && !entity.IsDeleted)
            .OrderByDescending(entity => entity.AssignedDateUtc)
            .FirstOrDefault();
        var assignedZones = ResolveZoneNames(technician);
        var todayAssignments = technician.ServiceRequestAssignments.Count(
            entity => !entity.IsDeleted &&
                DateOnly.FromDateTime(entity.AssignedDateUtc) == today);
        var availabilityStatus = ResolveAvailabilityStatus(technician, today);

        return new TechnicianListItemResponse(
            technician.TechnicianId,
            technician.TechnicianCode,
            technician.TechnicianName,
            technician.MobileNumber,
            technician.EmailAddress,
            technician.IsActive,
            availabilityStatus,
            activeAssignment?.ServiceRequest?.ServiceRequestNumber,
            technician.BaseZone?.ZoneName,
            assignedZones,
            technician.Skills
                .Where(entity => !entity.IsDeleted)
                .OrderBy(entity => entity.SkillName)
                .Select(ToSkillResponse)
                .ToArray(),
            latestSummary?.AverageRating ?? 0m,
            todayAssignments,
            latestSummary?.SlaCompliancePercent ?? 0m,
            ResolveNextFreeSlot(technician, today, availabilityStatus));
    }

    public static TechnicianListItemResponse ToAvailabilityBoardItem(
        TechnicianEntity technician,
        TechnicianAvailabilitySnapshot? snapshot,
        DateOnly today)
    {
        var baseItem = ToListItem(technician, today);

        if (snapshot is null)
        {
            return baseItem;
        }

        var availabilityStatus = snapshot.IsAvailable
            ? "available"
            : snapshot.BookedAssignmentCount > 0
                ? "on-job"
                : baseItem.AvailabilityStatus;

        return baseItem with
        {
            AvailabilityStatus = availabilityStatus,
            TodayJobCount = snapshot.BookedAssignmentCount,
            NextFreeSlot = snapshot.IsAvailable
                ? $"{snapshot.RemainingCapacity} slots free"
                : snapshot.RemainingCapacity == 0
                    ? "Capacity full"
                    : ResolveNextFreeSlot(technician, today, availabilityStatus)
        };
    }

    public static TechnicianDetailResponse ToDetailResponse(
        TechnicianEntity technician,
        TechnicianOnboardingEligibilityResult eligibility,
        IReadOnlyCollection<TechnicianDocument> documents,
        IReadOnlyCollection<SkillAssessment> assessments,
        IReadOnlyCollection<TrainingRecord> trainingRecords,
        DateOnly today)
    {
        var activeAssignment = technician.ServiceRequestAssignments
            .Where(entity => entity.IsActiveAssignment && !entity.IsDeleted)
            .OrderByDescending(entity => entity.AssignedDateUtc)
            .FirstOrDefault();

        return new TechnicianDetailResponse(
            technician.TechnicianId,
            technician.TechnicianCode,
            technician.TechnicianName,
            technician.MobileNumber,
            technician.EmailAddress,
            technician.BaseZoneId,
            technician.BaseZone?.ZoneName,
            technician.IsActive,
            technician.MaxDailyAssignments,
            ResolveAvailabilityStatus(technician, today),
            activeAssignment?.ServiceRequest?.ServiceRequestNumber,
            technician.Zones
                .Where(entity => !entity.IsDeleted && entity.Zone is not null)
                .OrderByDescending(entity => entity.IsPrimaryZone)
                .ThenBy(entity => entity.Zone!.ZoneName)
                .Select(entity => ToZoneResponse(entity, entity.Zone!))
                .ToArray(),
            technician.Skills
                .Where(entity => !entity.IsDeleted)
                .OrderBy(entity => entity.SkillName)
                .Select(ToSkillResponse)
                .ToArray(),
            eligibility.OnboardingStatus,
            eligibility.PendingItems.ToArray(),
            documents.Count,
            documents.Count(document => document.VerificationStatus == Domain.Enums.TechnicianDocumentStatus.Verified),
            (assessments.OrderByDescending(item => item.AssessedOnUtc ?? DateTime.MinValue).FirstOrDefault()?.AssessmentResult ?? Domain.Enums.SkillAssessmentResult.Pending).ToString(),
            trainingRecords.Count(item => item.IsCompleted || item.TrainingStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)));
    }

    public static TechnicianSkillResponse ToSkillResponse(TechnicianSkill skill)
    {
        return new TechnicianSkillResponse(
            skill.TechnicianSkillId,
            skill.SkillCode,
            skill.SkillName,
            skill.SkillCategory,
            skill.CertifiedOnUtc);
    }

    public static TechnicianZoneResponse ToZoneResponse(TechnicianZone technicianZone, Zone zone)
    {
        return new TechnicianZoneResponse(
            technicianZone.TechnicianZoneId,
            zone.ZoneId,
            zone.ZoneName,
            technicianZone.IsPrimaryZone);
    }

    public static TechnicianPerformanceResponse ToPerformanceResponse(
        TechnicianPerformanceMetricsSnapshot metrics,
        decimal teamAverageSlaCompliancePercent)
    {
        return new TechnicianPerformanceResponse(
            metrics.AverageRating,
            metrics.TotalJobs,
            metrics.CompletedJobs,
            metrics.SlaCompliancePercent,
            metrics.RevisitRatePercent,
            metrics.RevenueGenerated,
            teamAverageSlaCompliancePercent,
            metrics.TrendPoints
                .Select(item => new TechnicianPerformanceTrendResponse(
                    item.MetricDate.ToString("dd MMM"),
                    item.JobsAssigned,
                    item.JobsCompleted,
                    item.SlaCompliancePercent))
                .ToArray());
    }

    public static TechnicianAttendanceResponse ToAttendanceResponse(TechnicianAttendance attendance)
    {
        return new TechnicianAttendanceResponse(
            attendance.TechnicianAttendanceId,
            attendance.AttendanceDate,
            attendance.AttendanceStatus,
            attendance.CheckInOnUtc,
            attendance.CheckOutOnUtc,
            attendance.LocationText,
            attendance.LeaveReason,
            attendance.ReviewedByUserId,
            attendance.ReviewedOnUtc);
    }

    public static TechnicianGpsLogResponse ToGpsLogResponse(TechnicianGpsLog gpsLog)
    {
        return new TechnicianGpsLogResponse(
            gpsLog.TechnicianGpsLogId,
            gpsLog.TrackedOnUtc,
            gpsLog.Latitude,
            gpsLog.Longitude,
            gpsLog.TrackingSource,
            gpsLog.LocationText,
            gpsLog.ServiceRequestId);
    }

    public static async Task<TechnicianDetailResponse> BuildDetailResponseAsync(
        long technicianId,
        ITechnicianRepository technicianRepository,
        IGapPhaseERepository gapPhaseERepository,
        TechnicianOnboardingEligibilityService eligibilityService,
        CancellationToken cancellationToken)
    {
        var technician = await technicianRepository.GetManagementDetailAsync(technicianId, asNoTracking: true, cancellationToken: cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);
        var (documents, assessments, trainingRecords) = await LoadOnboardingArtifactsAsync(
            technicianId,
            gapPhaseERepository,
            cancellationToken);
        var eligibility = eligibilityService.Evaluate(technician, documents, assessments, trainingRecords);

        return ToDetailResponse(technician, eligibility, documents, assessments, trainingRecords, DateOnly.FromDateTime(DateTime.UtcNow));
    }

    private static async Task<IReadOnlyCollection<TItem>> SafeLoadAsync<TItem>(Func<Task<IReadOnlyCollection<TItem>>> loader)
    {
        try
        {
            return await loader();
        }
        catch
        {
            return Array.Empty<TItem>();
        }
    }

    private static IReadOnlyCollection<string> ResolveZoneNames(TechnicianEntity technician)
    {
        var zones = technician.Zones
            .Where(entity => !entity.IsDeleted && entity.Zone is not null)
            .OrderByDescending(entity => entity.IsPrimaryZone)
            .ThenBy(entity => entity.Zone!.ZoneName)
            .Select(entity => entity.Zone!.ZoneName)
            .ToArray();

        if (zones.Length > 0)
        {
            return zones;
        }

        return string.IsNullOrWhiteSpace(technician.BaseZone?.ZoneName)
            ? ["Unassigned"]
            : [technician.BaseZone.ZoneName];
    }

    private static string ResolveAvailabilityStatus(TechnicianEntity technician, DateOnly today)
    {
        if (!technician.IsActive)
        {
            return "off-duty";
        }

        var attendance = technician.Attendances
            .Where(entity => !entity.IsDeleted && entity.AttendanceDate == today)
            .OrderByDescending(entity => entity.TechnicianAttendanceId)
            .FirstOrDefault();

        if (attendance is not null &&
            (attendance.AttendanceStatus.Equals("LeaveRequested", StringComparison.OrdinalIgnoreCase) ||
             attendance.AttendanceStatus.Equals("LeaveApproved", StringComparison.OrdinalIgnoreCase) ||
             attendance.AttendanceStatus.Equals("OnLeave", StringComparison.OrdinalIgnoreCase)))
        {
            return "on-leave";
        }

        if (technician.ServiceRequestAssignments.Any(entity => entity.IsActiveAssignment && !entity.IsDeleted))
        {
            return "on-job";
        }

        var availabilityEntry = technician.TechnicianAvailabilities
            .Where(entity => !entity.IsDeleted && entity.AvailableDate == today)
            .OrderByDescending(entity => entity.TechnicianAvailabilityId)
            .FirstOrDefault();

        if (availabilityEntry is not null && !availabilityEntry.IsAvailable)
        {
            return "off-duty";
        }

        return "available";
    }

    private static string? ResolveNextFreeSlot(TechnicianEntity technician, DateOnly today, string availabilityStatus)
    {
        if (availabilityStatus == "on-leave")
        {
            return "Leave";
        }

        if (availabilityStatus == "off-duty")
        {
            return "Off Duty";
        }

        var availabilityEntry = technician.TechnicianAvailabilities
            .Where(entity => !entity.IsDeleted && entity.AvailableDate == today)
            .OrderByDescending(entity => entity.TechnicianAvailabilityId)
            .FirstOrDefault();

        if (availabilityEntry is not null && availabilityEntry.AvailableSlotCount > availabilityEntry.BookedAssignmentCount)
        {
            return $"{Math.Max(availabilityEntry.AvailableSlotCount - availabilityEntry.BookedAssignmentCount, 0)} slots free";
        }

        var todayAssignments = technician.ServiceRequestAssignments.Count(
            entity => !entity.IsDeleted &&
                DateOnly.FromDateTime(entity.AssignedDateUtc) == today);
        var remainingCapacity = Math.Max(technician.MaxDailyAssignments - todayAssignments, 0);

        return remainingCapacity == 0 ? "Capacity full" : $"{remainingCapacity} slots free";
    }
}
