using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.GapPhaseE;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.GapPhaseE.Helpers;

public sealed record CreateHelperProfileCommand(
    long UserId,
    string HelperCode,
    string HelperName,
    string MobileNo,
    bool ActiveFlag) : IRequest<HelperDetailResponse>;

public sealed class CreateHelperProfileCommandValidator : AbstractValidator<CreateHelperProfileCommand>
{
    public CreateHelperProfileCommandValidator()
    {
        RuleFor(request => request.UserId).GreaterThan(0);
        RuleFor(request => request.HelperCode).NotEmpty().MaximumLength(32);
        RuleFor(request => request.HelperName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.MobileNo).Matches("^[0-9]{8,16}$");
    }
}

public sealed class CreateHelperProfileCommandHandler : IRequestHandler<CreateHelperProfileCommand, HelperDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseERepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public CreateHelperProfileCommandHandler(
        IUserRepository userRepository,
        IGapPhaseERepository repository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _repository = repository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<HelperDetailResponse> Handle(CreateHelperProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The helper user account could not be found.", 404);

        if (!user.IsActive)
        {
            throw new AppException(ErrorCodes.InactiveUser, "The helper user account is inactive.", 409);
        }

        if (await _repository.GetHelperProfileByUserIdAsync(request.UserId, cancellationToken) is not null)
        {
            throw new AppException(ErrorCodes.DuplicateValue, "A helper profile already exists for this user.", 409);
        }

        if (await _repository.HelperCodeExistsAsync(request.HelperCode.Trim(), null, cancellationToken))
        {
            throw new AppException(ErrorCodes.DuplicateValue, "A helper profile already exists for this helper code.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var actor = HelperWorkflowSupport.ResolveActor(_currentUserContext, "HelperManagement");
        var helperProfile = new HelperProfile
        {
            UserId = request.UserId,
            HelperCode = request.HelperCode.Trim(),
            HelperName = request.HelperName.Trim(),
            MobileNo = request.MobileNo.Trim(),
            ActiveFlag = request.ActiveFlag,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        await _repository.AddHelperProfileAsync(helperProfile, cancellationToken);
        await _auditLogRepository.AddAsync(
            HelperWorkflowSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "CreateHelperProfile",
                nameof(HelperProfile),
                request.HelperCode.Trim(),
                request.HelperName.Trim()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await HelperWorkflowSupport.BuildHelperDetailAsync(helperProfile, _repository, cancellationToken);
    }
}

public sealed record GetHelperListQuery(
    string? SearchTerm,
    int? BranchId) : IRequest<IReadOnlyCollection<HelperListItemResponse>>;

public sealed class GetHelperListQueryHandler : IRequestHandler<GetHelperListQuery, IReadOnlyCollection<HelperListItemResponse>>
{
    private readonly IGapPhaseERepository _repository;

    public GetHelperListQueryHandler(IGapPhaseERepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<HelperListItemResponse>> Handle(GetHelperListQuery request, CancellationToken cancellationToken)
    {
        var helpers = await _repository.SearchHelpersAsync(request.SearchTerm?.Trim(), request.BranchId, cancellationToken);
        var items = new List<HelperListItemResponse>(helpers.Count);

        foreach (var helper in helpers)
        {
            var assignment = await _repository.GetActiveHelperAssignmentAsync(helper.HelperProfileId, asNoTracking: true, cancellationToken);
            items.Add(HelperWorkflowSupport.MapHelperListItem(helper, assignment));
        }

        return items;
    }
}

public sealed record GetHelperDetailQuery(long HelperProfileId) : IRequest<HelperDetailResponse>;

public sealed class GetHelperDetailQueryHandler : IRequestHandler<GetHelperDetailQuery, HelperDetailResponse>
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseERepository _repository;

    public GetHelperDetailQueryHandler(
        IGapPhaseERepository repository,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _currentUserContext = currentUserContext;
    }

    public async Task<HelperDetailResponse> Handle(GetHelperDetailQuery request, CancellationToken cancellationToken)
    {
        await HelperWorkflowSupport.EnsureHelperAccessAsync(request.HelperProfileId, _repository, _currentUserContext, cancellationToken);

        var helperProfile = await _repository.GetHelperProfileAsync(request.HelperProfileId, asNoTracking: true, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The helper profile could not be found.", 404);

        return await HelperWorkflowSupport.BuildHelperDetailAsync(helperProfile, _repository, cancellationToken);
    }
}

public sealed record AssignHelperToJobCommand(
    long HelperProfileId,
    long TechnicianId,
    long ServiceRequestId,
    long? JobCardId,
    string? AssignmentRemarks) : IRequest<HelperDetailResponse>;

public sealed class AssignHelperToJobCommandValidator : AbstractValidator<AssignHelperToJobCommand>
{
    public AssignHelperToJobCommandValidator()
    {
        RuleFor(request => request.HelperProfileId).GreaterThan(0);
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.AssignmentRemarks).MaximumLength(512);
    }
}

public sealed class AssignHelperToJobCommandHandler : IRequestHandler<AssignHelperToJobCommand, HelperDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly HelperAssignmentValidationService _helperAssignmentValidationService;
    private readonly IGapPhaseERepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignHelperToJobCommandHandler(
        IGapPhaseERepository repository,
        HelperAssignmentValidationService helperAssignmentValidationService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _helperAssignmentValidationService = helperAssignmentValidationService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<HelperDetailResponse> Handle(AssignHelperToJobCommand request, CancellationToken cancellationToken)
    {
        var helperProfile = await _repository.GetHelperProfileAsync(request.HelperProfileId, asNoTracking: false, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The helper profile could not be found.", 404);
        await _helperAssignmentValidationService.EnsureAssignmentAllowedAsync(helperProfile, null, cancellationToken);

        var technician = await _repository.GetTechnicianAsync(request.TechnicianId, asNoTracking: true, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The technician could not be found.", 404);

        if (!technician.IsActive)
        {
            throw new AppException(ErrorCodes.TechnicianInactive, "The technician is inactive and cannot receive helper assignments.", 409);
        }

        var serviceRequest = await _repository.GetServiceRequestAsync(request.ServiceRequestId, asNoTracking: true, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The service request could not be found.", 404);
        var jobCardId = request.JobCardId ?? serviceRequest.JobCard?.JobCardId;
        var now = _currentDateTime.UtcNow;
        var actor = HelperWorkflowSupport.ResolveActor(_currentUserContext, "HelperManagement");

        await _repository.AddHelperAssignmentAsync(
            new HelperAssignment
            {
                HelperProfileId = helperProfile.HelperProfileId,
                TechnicianId = technician.TechnicianId,
                ServiceRequestId = serviceRequest.ServiceRequestId,
                JobCardId = jobCardId,
                AssignmentStatus = "Assigned",
                AssignmentRemarks = request.AssignmentRemarks?.Trim() ?? string.Empty,
                AssignedOnUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _auditLogRepository.AddAsync(
            HelperWorkflowSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "AssignHelperToJob",
                nameof(HelperAssignment),
                helperProfile.HelperCode,
                serviceRequest.ServiceRequestNumber),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await HelperWorkflowSupport.BuildHelperDetailAsync(helperProfile, _repository, cancellationToken);
    }
}

public sealed record ReleaseHelperAssignmentCommand(
    long HelperProfileId,
    string Remarks) : IRequest<HelperDetailResponse>;

public sealed class ReleaseHelperAssignmentCommandValidator : AbstractValidator<ReleaseHelperAssignmentCommand>
{
    public ReleaseHelperAssignmentCommandValidator()
    {
        RuleFor(request => request.HelperProfileId).GreaterThan(0);
        RuleFor(request => request.Remarks).NotEmpty().MaximumLength(512);
    }
}

public sealed class ReleaseHelperAssignmentCommandHandler : IRequestHandler<ReleaseHelperAssignmentCommand, HelperDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseERepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ReleaseHelperAssignmentCommandHandler(
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

    public async Task<HelperDetailResponse> Handle(ReleaseHelperAssignmentCommand request, CancellationToken cancellationToken)
    {
        var helperProfile = await _repository.GetHelperProfileAsync(request.HelperProfileId, asNoTracking: false, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The helper profile could not be found.", 404);
        var assignment = await _repository.GetActiveHelperAssignmentAsync(request.HelperProfileId, asNoTracking: false, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The helper does not have an active assignment.", 404);
        var now = _currentDateTime.UtcNow;

        assignment.AssignmentStatus = "Released";
        assignment.AssignmentRemarks = request.Remarks.Trim();
        assignment.ReleasedOnUtc = now;
        assignment.LastUpdated = now;
        assignment.UpdatedBy = HelperWorkflowSupport.ResolveActor(_currentUserContext, "HelperManagement");

        await _auditLogRepository.AddAsync(
            HelperWorkflowSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "ReleaseHelperAssignment",
                nameof(HelperAssignment),
                assignment.HelperAssignmentId.ToString(),
                request.Remarks.Trim()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await HelperWorkflowSupport.BuildHelperDetailAsync(helperProfile, _repository, cancellationToken);
    }
}

public sealed record GetHelperAssignmentDetailQuery(long HelperProfileId) : IRequest<HelperAssignmentDetailResponse>;

public sealed class GetHelperAssignmentDetailQueryHandler : IRequestHandler<GetHelperAssignmentDetailQuery, HelperAssignmentDetailResponse>
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseERepository _repository;

    public GetHelperAssignmentDetailQueryHandler(
        IGapPhaseERepository repository,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _currentUserContext = currentUserContext;
    }

    public async Task<HelperAssignmentDetailResponse> Handle(GetHelperAssignmentDetailQuery request, CancellationToken cancellationToken)
    {
        await HelperWorkflowSupport.EnsureHelperAccessAsync(request.HelperProfileId, _repository, _currentUserContext, cancellationToken);
        var assignment = await _repository.GetActiveHelperAssignmentAsync(request.HelperProfileId, asNoTracking: true, cancellationToken);
        return HelperWorkflowSupport.MapAssignment(assignment);
    }
}

public sealed record CheckInHelperAttendanceCommand(
    long HelperProfileId,
    string? LocationText) : IRequest<IReadOnlyCollection<HelperAttendanceResponse>>;

public sealed class CheckInHelperAttendanceCommandHandler : IRequestHandler<CheckInHelperAttendanceCommand, IReadOnlyCollection<HelperAttendanceResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseERepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CheckInHelperAttendanceCommandHandler(
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

    public async Task<IReadOnlyCollection<HelperAttendanceResponse>> Handle(CheckInHelperAttendanceCommand request, CancellationToken cancellationToken)
    {
        await HelperWorkflowSupport.EnsureHelperAccessAsync(request.HelperProfileId, _repository, _currentUserContext, cancellationToken);

        var helperProfile = await _repository.GetHelperProfileAsync(request.HelperProfileId, asNoTracking: true, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The helper profile could not be found.", 404);
        var activeAssignment = await _repository.GetActiveHelperAssignmentAsync(request.HelperProfileId, asNoTracking: true, cancellationToken)
            ?? throw new AppException(ErrorCodes.TechnicianJobAccessDenied, "The helper is not assigned to an active job.", 409);

        if (await _repository.GetOpenHelperAttendanceAsync(request.HelperProfileId, cancellationToken) is not null)
        {
            throw new AppException(ErrorCodes.Conflict, "The helper is already checked in.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var actor = HelperWorkflowSupport.ResolveActor(_currentUserContext, "HelperAttendance");

        await _repository.AddHelperAttendanceAsync(
            new HelperAttendance
            {
                HelperProfileId = helperProfile.HelperProfileId,
                AttendanceDate = DateOnly.FromDateTime(now),
                CheckInOnUtc = now,
                AttendanceStatus = "CheckedIn",
                LocationText = request.LocationText?.Trim() ?? string.Empty,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _auditLogRepository.AddAsync(
            HelperWorkflowSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "CheckInHelperAttendance",
                nameof(HelperAttendance),
                helperProfile.HelperCode,
                activeAssignment.ServiceRequest?.ServiceRequestNumber ?? string.Empty),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var attendances = await _repository.GetHelperAttendancesAsync(request.HelperProfileId, cancellationToken);
        return attendances.Select(HelperWorkflowSupport.MapAttendance).ToArray();
    }
}

public sealed record CheckOutHelperAttendanceCommand(
    long HelperProfileId,
    string? LocationText) : IRequest<IReadOnlyCollection<HelperAttendanceResponse>>;

public sealed class CheckOutHelperAttendanceCommandHandler : IRequestHandler<CheckOutHelperAttendanceCommand, IReadOnlyCollection<HelperAttendanceResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseERepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CheckOutHelperAttendanceCommandHandler(
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

    public async Task<IReadOnlyCollection<HelperAttendanceResponse>> Handle(CheckOutHelperAttendanceCommand request, CancellationToken cancellationToken)
    {
        await HelperWorkflowSupport.EnsureHelperAccessAsync(request.HelperProfileId, _repository, _currentUserContext, cancellationToken);

        var helperProfile = await _repository.GetHelperProfileAsync(request.HelperProfileId, asNoTracking: true, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The helper profile could not be found.", 404);
        var attendance = await _repository.GetOpenHelperAttendanceAsync(request.HelperProfileId, cancellationToken)
            ?? throw new AppException(ErrorCodes.Conflict, "The helper cannot check out before a valid check-in exists.", 409);
        var now = _currentDateTime.UtcNow;

        attendance.CheckOutOnUtc = now;
        attendance.AttendanceStatus = "CheckedOut";
        attendance.LocationText = request.LocationText?.Trim() ?? attendance.LocationText;
        attendance.LastUpdated = now;
        attendance.UpdatedBy = HelperWorkflowSupport.ResolveActor(_currentUserContext, "HelperAttendance");

        await _auditLogRepository.AddAsync(
            HelperWorkflowSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "CheckOutHelperAttendance",
                nameof(HelperAttendance),
                helperProfile.HelperCode,
                attendance.AttendanceDate.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var attendances = await _repository.GetHelperAttendancesAsync(request.HelperProfileId, cancellationToken);
        return attendances.Select(HelperWorkflowSupport.MapAttendance).ToArray();
    }
}

public sealed record GetHelperAttendanceListQuery(long HelperProfileId) : IRequest<IReadOnlyCollection<HelperAttendanceResponse>>;

public sealed class GetHelperAttendanceListQueryHandler : IRequestHandler<GetHelperAttendanceListQuery, IReadOnlyCollection<HelperAttendanceResponse>>
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseERepository _repository;

    public GetHelperAttendanceListQueryHandler(
        IGapPhaseERepository repository,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<HelperAttendanceResponse>> Handle(GetHelperAttendanceListQuery request, CancellationToken cancellationToken)
    {
        await HelperWorkflowSupport.EnsureHelperAccessAsync(request.HelperProfileId, _repository, _currentUserContext, cancellationToken);
        var attendances = await _repository.GetHelperAttendancesAsync(request.HelperProfileId, cancellationToken);
        return attendances.Select(HelperWorkflowSupport.MapAttendance).ToArray();
    }
}

public sealed record GetHelperTaskChecklistQuery(long HelperProfileId) : IRequest<IReadOnlyCollection<HelperTaskChecklistResponse>>;

public sealed class GetHelperTaskChecklistQueryHandler : IRequestHandler<GetHelperTaskChecklistQuery, IReadOnlyCollection<HelperTaskChecklistResponse>>
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseERepository _repository;

    public GetHelperTaskChecklistQueryHandler(
        IGapPhaseERepository repository,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<HelperTaskChecklistResponse>> Handle(GetHelperTaskChecklistQuery request, CancellationToken cancellationToken)
    {
        await HelperWorkflowSupport.EnsureHelperAccessAsync(request.HelperProfileId, _repository, _currentUserContext, cancellationToken);
        var assignment = await _repository.GetActiveHelperAssignmentAsync(request.HelperProfileId, asNoTracking: true, cancellationToken)
            ?? throw new AppException(ErrorCodes.TechnicianJobAccessDenied, "The helper does not have an active assignment.", 409);

        return await HelperWorkflowSupport.BuildTaskChecklistAsync(assignment, _repository, cancellationToken);
    }
}

public sealed record SaveHelperTaskResponseCommand(
    long HelperProfileId,
    long HelperTaskChecklistId,
    string ResponseStatus,
    string? ResponseRemarks) : IRequest<IReadOnlyCollection<HelperTaskChecklistResponse>>;

public sealed class SaveHelperTaskResponseCommandValidator : AbstractValidator<SaveHelperTaskResponseCommand>
{
    public SaveHelperTaskResponseCommandValidator()
    {
        RuleFor(request => request.HelperProfileId).GreaterThan(0);
        RuleFor(request => request.HelperTaskChecklistId).GreaterThan(0);
        RuleFor(request => request.ResponseStatus).NotEmpty().MaximumLength(32);
        RuleFor(request => request.ResponseRemarks).MaximumLength(512);
    }
}

public sealed class SaveHelperTaskResponseCommandHandler : IRequestHandler<SaveHelperTaskResponseCommand, IReadOnlyCollection<HelperTaskChecklistResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseERepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveHelperTaskResponseCommandHandler(
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

    public async Task<IReadOnlyCollection<HelperTaskChecklistResponse>> Handle(SaveHelperTaskResponseCommand request, CancellationToken cancellationToken)
    {
        await HelperWorkflowSupport.EnsureHelperAccessAsync(request.HelperProfileId, _repository, _currentUserContext, cancellationToken);
        var assignment = await _repository.GetActiveHelperAssignmentAsync(request.HelperProfileId, asNoTracking: false, cancellationToken)
            ?? throw new AppException(ErrorCodes.TechnicianJobAccessDenied, "The helper does not have an active assignment.", 409);

        await HelperWorkflowSupport.EnsureTaskBelongsToAssignmentAsync(assignment, request.HelperTaskChecklistId, _repository, cancellationToken);
        var response = await _repository.GetHelperTaskResponseAsync(assignment.HelperAssignmentId, request.HelperTaskChecklistId, asNoTracking: false, cancellationToken);
        var now = _currentDateTime.UtcNow;
        var actor = HelperWorkflowSupport.ResolveActor(_currentUserContext, "HelperTask");

        if (response is null)
        {
            response = new HelperTaskResponse
            {
                HelperAssignmentId = assignment.HelperAssignmentId,
                HelperTaskChecklistId = request.HelperTaskChecklistId,
                ResponseStatus = request.ResponseStatus.Trim(),
                ResponseRemarks = request.ResponseRemarks?.Trim() ?? string.Empty,
                RespondedOnUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            };

            await _repository.AddHelperTaskResponseAsync(response, cancellationToken);
        }
        else
        {
            response.ResponseStatus = request.ResponseStatus.Trim();
            response.ResponseRemarks = request.ResponseRemarks?.Trim() ?? response.ResponseRemarks;
            response.RespondedOnUtc = now;
            response.LastUpdated = now;
            response.UpdatedBy = actor;
        }

        await _auditLogRepository.AddAsync(
            HelperWorkflowSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "SaveHelperTaskResponse",
                nameof(HelperTaskResponse),
                assignment.HelperAssignmentId.ToString(),
                request.HelperTaskChecklistId.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await HelperWorkflowSupport.BuildTaskChecklistAsync(assignment, _repository, cancellationToken);
    }
}

public sealed record UploadHelperTaskPhotoCommand(
    long HelperProfileId,
    long HelperTaskChecklistId,
    string FileName,
    string ContentType,
    string Base64Content,
    string? ResponseRemarks) : IRequest<IReadOnlyCollection<HelperTaskChecklistResponse>>;

public sealed class UploadHelperTaskPhotoCommandValidator : AbstractValidator<UploadHelperTaskPhotoCommand>
{
    public UploadHelperTaskPhotoCommandValidator()
    {
        RuleFor(request => request.HelperProfileId).GreaterThan(0);
        RuleFor(request => request.HelperTaskChecklistId).GreaterThan(0);
        RuleFor(request => request.FileName).NotEmpty().MaximumLength(256);
        RuleFor(request => request.ContentType).NotEmpty().MaximumLength(128);
        RuleFor(request => request.Base64Content).NotEmpty();
        RuleFor(request => request.ResponseRemarks).MaximumLength(512);
    }
}

public sealed class UploadHelperTaskPhotoCommandHandler : IRequestHandler<UploadHelperTaskPhotoCommand, IReadOnlyCollection<HelperTaskChecklistResponse>>
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IJobAttachmentStorageService _jobAttachmentStorageService;
    private readonly IGapPhaseERepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UploadHelperTaskPhotoCommandHandler(
        IGapPhaseERepository repository,
        IJobAttachmentStorageService jobAttachmentStorageService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _jobAttachmentStorageService = jobAttachmentStorageService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<HelperTaskChecklistResponse>> Handle(UploadHelperTaskPhotoCommand request, CancellationToken cancellationToken)
    {
        await HelperWorkflowSupport.EnsureHelperAccessAsync(request.HelperProfileId, _repository, _currentUserContext, cancellationToken);
        var assignment = await _repository.GetActiveHelperAssignmentAsync(request.HelperProfileId, asNoTracking: false, cancellationToken)
            ?? throw new AppException(ErrorCodes.TechnicianJobAccessDenied, "The helper does not have an active assignment.", 409);
        await HelperWorkflowSupport.EnsureTaskBelongsToAssignmentAsync(assignment, request.HelperTaskChecklistId, _repository, cancellationToken);

        if (!AllowedContentTypes.Contains(request.ContentType))
        {
            throw new AppException(ErrorCodes.InvalidAttachmentContent, "Only JPEG, PNG, and WEBP helper task photos are supported.", 400);
        }

        byte[] fileBytes;

        try
        {
            fileBytes = Convert.FromBase64String(request.Base64Content);
        }
        catch (FormatException)
        {
            throw new AppException(ErrorCodes.InvalidAttachmentContent, "The helper task photo content is not valid base64.", 400);
        }

        if (fileBytes.LongLength > 5 * 1024 * 1024)
        {
            throw new AppException(ErrorCodes.AttachmentTooLarge, "Helper task photo size must not exceed 5 MB.", 409);
        }

        var storedFile = await _jobAttachmentStorageService.SaveAsync(request.FileName, request.ContentType, fileBytes, cancellationToken);
        var response = await _repository.GetHelperTaskResponseAsync(assignment.HelperAssignmentId, request.HelperTaskChecklistId, asNoTracking: false, cancellationToken);
        var now = _currentDateTime.UtcNow;
        var actor = HelperWorkflowSupport.ResolveActor(_currentUserContext, "HelperTask");

        if (response is null)
        {
            response = new HelperTaskResponse
            {
                HelperAssignmentId = assignment.HelperAssignmentId,
                HelperTaskChecklistId = request.HelperTaskChecklistId,
                ResponseStatus = "PhotoUploaded",
                ResponseRemarks = request.ResponseRemarks?.Trim() ?? string.Empty,
                ResponsePhotoUrl = storedFile.RelativePath,
                RespondedOnUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            };

            await _repository.AddHelperTaskResponseAsync(response, cancellationToken);
        }
        else
        {
            response.ResponseStatus = string.IsNullOrWhiteSpace(response.ResponseStatus) ? "PhotoUploaded" : response.ResponseStatus;
            response.ResponseRemarks = request.ResponseRemarks?.Trim() ?? response.ResponseRemarks;
            response.ResponsePhotoUrl = storedFile.RelativePath;
            response.RespondedOnUtc = now;
            response.LastUpdated = now;
            response.UpdatedBy = actor;
        }

        await _auditLogRepository.AddAsync(
            HelperWorkflowSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "UploadHelperTaskPhoto",
                nameof(HelperTaskResponse),
                assignment.HelperAssignmentId.ToString(),
                request.HelperTaskChecklistId.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await HelperWorkflowSupport.BuildTaskChecklistAsync(assignment, _repository, cancellationToken);
    }
}

internal static class HelperWorkflowSupport
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

    public static async Task EnsureHelperAccessAsync(
        long helperProfileId,
        IGapPhaseERepository repository,
        ICurrentUserContext currentUserContext,
        CancellationToken cancellationToken)
    {
        if (!currentUserContext.Roles.Contains(RoleNames.Helper, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        if (!currentUserContext.UserId.HasValue)
        {
            throw new AppException(ErrorCodes.Unauthorized, "The current helper request is unauthorized.", 401);
        }

        var currentHelper = await repository.GetHelperProfileByUserIdAsync(currentUserContext.UserId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.TechnicianJobAccessDenied, "The helper profile for this user could not be found.", 403);

        if (currentHelper.HelperProfileId != helperProfileId)
        {
            throw new AppException(ErrorCodes.Forbidden, "Helper access is limited to the current helper profile.", 403);
        }

        if (!currentHelper.ActiveFlag)
        {
            throw new AppException(ErrorCodes.InactiveUser, "The helper profile is inactive.", 403);
        }
    }

    public static HelperListItemResponse MapHelperListItem(HelperProfile helperProfile, HelperAssignment? assignment)
    {
        return new HelperListItemResponse(
            helperProfile.HelperProfileId,
            helperProfile.HelperCode,
            helperProfile.HelperName,
            helperProfile.MobileNo,
            helperProfile.ActiveFlag,
            assignment?.AssignmentStatus,
            assignment?.Technician?.TechnicianName,
            assignment?.ServiceRequest?.ServiceRequestNumber);
    }

    public static async Task<HelperDetailResponse> BuildHelperDetailAsync(
        HelperProfile helperProfile,
        IGapPhaseERepository repository,
        CancellationToken cancellationToken)
    {
        var assignment = await repository.GetActiveHelperAssignmentAsync(helperProfile.HelperProfileId, asNoTracking: true, cancellationToken);
        var attendances = await repository.GetHelperAttendancesAsync(helperProfile.HelperProfileId, cancellationToken);
        var tasks = assignment is null
            ? Array.Empty<HelperTaskChecklistResponse>()
            : await BuildTaskChecklistAsync(assignment, repository, cancellationToken);

        return new HelperDetailResponse(
            helperProfile.HelperProfileId,
            helperProfile.UserId,
            helperProfile.HelperCode,
            helperProfile.HelperName,
            helperProfile.MobileNo,
            helperProfile.ActiveFlag,
            MapAssignment(assignment),
            attendances.Select(MapAttendance).ToArray(),
            tasks);
    }

    public static HelperAssignmentDetailResponse MapAssignment(HelperAssignment? assignment)
    {
        if (assignment is null)
        {
            return new HelperAssignmentDetailResponse(
                null,
                "Unassigned",
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                string.Empty,
                null,
                null);
        }

        var booking = assignment.ServiceRequest?.Booking;
        var bookingLine = booking?.BookingLines.OrderBy(line => line.BookingLineId).FirstOrDefault();
        var addressSummary = booking is null
            ? null
            : string.Join(", ", new[]
            {
                booking.AddressLine1Snapshot,
                booking.CityNameSnapshot,
                booking.PincodeSnapshot
            }.Where(value => !string.IsNullOrWhiteSpace(value)));

        return new HelperAssignmentDetailResponse(
            assignment.HelperAssignmentId,
            assignment.AssignmentStatus,
            assignment.TechnicianId,
            assignment.Technician?.TechnicianName,
            assignment.ServiceRequestId,
            assignment.ServiceRequest?.ServiceRequestNumber,
            assignment.JobCardId,
            assignment.JobCard?.JobCardNumber,
            booking?.CustomerNameSnapshot,
            bookingLine?.Service?.ServiceName ?? booking?.ServiceNameSnapshot,
            addressSummary,
            assignment.AssignmentRemarks,
            assignment.AssignedOnUtc,
            assignment.ReleasedOnUtc);
    }

    public static HelperAttendanceResponse MapAttendance(HelperAttendance attendance)
    {
        return new HelperAttendanceResponse(
            attendance.HelperAttendanceId,
            attendance.AttendanceDate,
            attendance.CheckInOnUtc,
            attendance.CheckOutOnUtc,
            attendance.AttendanceStatus,
            attendance.LocationText);
    }

    public static async Task<IReadOnlyCollection<HelperTaskChecklistResponse>> BuildTaskChecklistAsync(
        HelperAssignment assignment,
        IGapPhaseERepository repository,
        CancellationToken cancellationToken)
    {
        var serviceTypeId = assignment.ServiceRequest?.Booking?.BookingLines.OrderBy(line => line.BookingLineId).Select(line => (long?)line.ServiceId).FirstOrDefault();
        var checklists = await repository.GetHelperTaskChecklistsAsync(serviceTypeId, cancellationToken);
        var responses = await repository.GetHelperTaskResponsesAsync(assignment.HelperAssignmentId, cancellationToken);
        var responseLookup = responses.ToDictionary(item => item.HelperTaskChecklistId);

        return checklists
            .Select(checklist =>
            {
                responseLookup.TryGetValue(checklist.HelperTaskChecklistId, out var response);

                return new HelperTaskChecklistResponse(
                    checklist.HelperTaskChecklistId,
                    checklist.TaskName,
                    checklist.TaskDescription,
                    checklist.MandatoryFlag,
                    checklist.SortOrder,
                    response?.ResponseStatus ?? "Pending",
                    response?.ResponseRemarks ?? string.Empty,
                    response?.ResponsePhotoUrl ?? string.Empty,
                    response?.RespondedOnUtc);
            })
            .ToArray();
    }

    public static async Task EnsureTaskBelongsToAssignmentAsync(
        HelperAssignment assignment,
        long helperTaskChecklistId,
        IGapPhaseERepository repository,
        CancellationToken cancellationToken)
    {
        var checklists = await BuildTaskChecklistAsync(assignment, repository, cancellationToken);

        if (!checklists.Any(item => item.HelperTaskChecklistId == helperTaskChecklistId))
        {
            throw new AppException(ErrorCodes.NotFound, "The requested helper task does not belong to the current assignment.", 404);
        }
    }
}
