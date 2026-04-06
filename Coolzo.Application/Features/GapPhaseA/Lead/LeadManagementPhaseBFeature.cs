using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.GapPhaseA;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.GapPhaseA.Lead;

public sealed record AssignLeadCommand(
    long LeadId,
    long AssignedUserId,
    string? Remarks) : IRequest<LeadResponse>;

public sealed class AssignLeadCommandValidator : AbstractValidator<AssignLeadCommand>
{
    public AssignLeadCommandValidator()
    {
        RuleFor(request => request.LeadId).GreaterThan(0);
        RuleFor(request => request.AssignedUserId).GreaterThan(0);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class AssignLeadCommandHandler : IRequestHandler<AssignLeadCommand, LeadResponse>
{
    private static readonly IReadOnlyCollection<string> AllowedAssigneeRoles =
    [
        RoleNames.CustomerSupportExecutive,
        RoleNames.OperationsExecutive,
        RoleNames.OperationsManager,
        RoleNames.Admin,
        RoleNames.SuperAdmin
    ];

    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly IGapPhaseARepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public AssignLeadCommandHandler(
        IGapPhaseARepository repository,
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        GapPhaseAFeatureFlagService featureFlagService)
    {
        _repository = repository;
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _featureFlagService = featureFlagService;
    }

    public async Task<LeadResponse> Handle(AssignLeadCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.lead.enabled", cancellationToken);

        var lead = await _repository.GetLeadByIdForUpdateAsync(request.LeadId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested lead could not be found.", 404);

        var assignedUser = await _userRepository.GetByIdWithRolesAsync(request.AssignedUserId, cancellationToken)
            ?? throw new AppException(ErrorCodes.LeadInvalidAssignee, "The selected assignee could not be found.", 404);

        if (!assignedUser.IsActive || !assignedUser.UserRoles.Any(userRole => userRole.Role is not null && AllowedAssigneeRoles.Contains(userRole.Role.RoleName)))
        {
            throw new AppException(ErrorCodes.LeadInvalidAssignee, "The selected assignee is not eligible for lead ownership.", 409);
        }

        if (lead.AssignedUserId == request.AssignedUserId)
        {
            throw new AppException(ErrorCodes.DuplicateAssignment, "The lead is already assigned to the selected user.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var actor = LeadManagementSupport.ResolveActor(_currentUserContext, "LeadAssignment");
        var remarks = request.Remarks?.Trim() ?? $"Lead assigned to {assignedUser.FullName}.";

        await _repository.AddLeadAssignmentAsync(
            new Domain.Entities.LeadAssignment
            {
                Lead = lead,
                AssignedUserId = assignedUser.UserId,
                PreviousAssignedUserId = lead.AssignedUserId,
                Remarks = remarks,
                AssignedDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        lead.AssignedUserId = assignedUser.UserId;
        lead.AssignedUser = assignedUser;
        lead.UpdatedBy = actor;
        lead.LastUpdated = now;

        await LeadManagementSupport.WriteLeadAuditAsync(
            _auditLogRepository,
            _currentUserContext,
            "AssignLead",
            lead.LeadNumber,
            assignedUser.FullName,
            actor,
            now,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeadManagementSupport.MapLead(lead);
    }
}

public sealed record UpdateLeadStatusCommand(
    long LeadId,
    string LeadStatus,
    string? Remarks,
    string? LostReason) : IRequest<LeadResponse>;

public sealed class UpdateLeadStatusCommandValidator : AbstractValidator<UpdateLeadStatusCommand>
{
    public UpdateLeadStatusCommandValidator()
    {
        RuleFor(request => request.LeadId).GreaterThan(0);
        RuleFor(request => request.LeadStatus).NotEmpty().Must(BeValidLeadStatus).WithMessage("Lead status is invalid.");
        RuleFor(request => request.Remarks).MaximumLength(512);
        RuleFor(request => request.LostReason).MaximumLength(256);
    }

    private static bool BeValidLeadStatus(string leadStatus)
    {
        return LeadManagementSupport.TryParseLeadStatus(leadStatus, out _);
    }
}

public sealed class UpdateLeadStatusCommandHandler : IRequestHandler<UpdateLeadStatusCommand, LeadResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly IGapPhaseARepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly GapPhaseAWorkflowService _workflowService;

    public UpdateLeadStatusCommandHandler(
        IGapPhaseARepository repository,
        GapPhaseAWorkflowService workflowService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        GapPhaseAFeatureFlagService featureFlagService)
    {
        _repository = repository;
        _workflowService = workflowService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _featureFlagService = featureFlagService;
    }

    public async Task<LeadResponse> Handle(UpdateLeadStatusCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.lead.enabled", cancellationToken);

        var lead = await _repository.GetLeadByIdForUpdateAsync(request.LeadId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested lead could not be found.", 404);

        LeadManagementSupport.TryParseLeadStatus(request.LeadStatus, out var targetStatus);

        if (targetStatus == LeadStatus.Converted)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "Use the conversion endpoints to move a lead into converted status.", 409);
        }

        if (targetStatus == LeadStatus.Qualified && !lead.AssignedUserId.HasValue)
        {
            throw new AppException(ErrorCodes.LeadAssignmentRequired, "Lead assignment is required before qualification.", 409);
        }

        if (targetStatus == LeadStatus.Lost && string.IsNullOrWhiteSpace(request.LostReason))
        {
            throw new AppException(ErrorCodes.ValidationFailure, "Lost reason is required when a lead is marked as lost.", 400);
        }

        if (targetStatus == LeadStatus.Closed && !lead.ConvertedBookingId.HasValue && !lead.ConvertedServiceRequestId.HasValue)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "Only converted leads can be closed.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var actor = LeadManagementSupport.ResolveActor(_currentUserContext, "LeadWorkflow");
        var remarks = request.Remarks?.Trim();

        if (targetStatus == LeadStatus.Lost)
        {
            lead.LostReason = request.LostReason!.Trim();
            remarks ??= $"Lead marked lost. Reason: {lead.LostReason}";
        }
        else
        {
            remarks ??= $"Lead status changed to {LeadManagementSupport.ToDisplayLeadStatus(targetStatus)}.";
        }

        await _workflowService.ChangeLeadStatusAsync(lead, targetStatus, remarks, cancellationToken);
        await LeadManagementSupport.WriteLeadAuditAsync(
            _auditLogRepository,
            _currentUserContext,
            "UpdateLeadStatus",
            lead.LeadNumber,
            LeadManagementSupport.ToDisplayLeadStatus(targetStatus),
            actor,
            now,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeadManagementSupport.MapLead(lead);
    }
}

public sealed record AddLeadNoteCommand(
    long LeadId,
    string NoteText,
    bool IsInternal) : IRequest<LeadResponse>;

public sealed class AddLeadNoteCommandValidator : AbstractValidator<AddLeadNoteCommand>
{
    public AddLeadNoteCommandValidator()
    {
        RuleFor(request => request.LeadId).GreaterThan(0);
        RuleFor(request => request.NoteText).NotEmpty().MaximumLength(1024);
    }
}

public sealed class AddLeadNoteCommandHandler : IRequestHandler<AddLeadNoteCommand, LeadResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAFeatureFlagService _featureFlagService;
    private readonly IGapPhaseARepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddLeadNoteCommandHandler(
        IGapPhaseARepository repository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        GapPhaseAFeatureFlagService featureFlagService)
    {
        _repository = repository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _featureFlagService = featureFlagService;
    }

    public async Task<LeadResponse> Handle(AddLeadNoteCommand request, CancellationToken cancellationToken)
    {
        await _featureFlagService.EnsureEnabledAsync("gap.phaseA.lead.enabled", cancellationToken);

        var lead = await _repository.GetLeadByIdForUpdateAsync(request.LeadId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested lead could not be found.", 404);

        var now = _currentDateTime.UtcNow;
        var actor = LeadManagementSupport.ResolveActor(_currentUserContext, "LeadNote");

        var note = new Domain.Entities.LeadNote
        {
            Lead = lead,
            NoteText = request.NoteText.Trim(),
            IsInternal = request.IsInternal,
            NoteDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        await _repository.AddLeadNoteAsync(note, cancellationToken);
        lead.Notes.Add(note);
        lead.UpdatedBy = actor;
        lead.LastUpdated = now;

        await LeadManagementSupport.WriteLeadAuditAsync(
            _auditLogRepository,
            _currentUserContext,
            "AddLeadNote",
            lead.LeadNumber,
            note.NoteText,
            actor,
            now,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeadManagementSupport.MapLead(lead);
    }
}

public sealed record GetLeadListQuery(
    string? SearchTerm,
    string? LeadStatus,
    string? SourceChannel,
    DateOnly? CreatedFrom,
    DateOnly? CreatedTo,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<LeadListItemResponse>>;

public sealed class GetLeadListQueryValidator : AbstractValidator<GetLeadListQuery>
{
    public GetLeadListQueryValidator()
    {
        RuleFor(request => request.PageNumber).GreaterThan(0);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 100);
        RuleFor(request => request.SearchTerm).MaximumLength(128);
        RuleFor(request => request.LeadStatus)
            .Must(status => string.IsNullOrWhiteSpace(status) || LeadManagementSupport.TryParseLeadStatus(status, out _))
            .WithMessage("Lead status filter is invalid.");
        RuleFor(request => request.SourceChannel)
            .Must(source => string.IsNullOrWhiteSpace(source) || LeadManagementSupport.TryParseLeadSourceChannel(source, out _))
            .WithMessage("Lead source filter is invalid.");
        RuleFor(request => request)
            .Must(request => !request.CreatedFrom.HasValue || !request.CreatedTo.HasValue || request.CreatedFrom.Value <= request.CreatedTo.Value)
            .WithMessage("Created-from date cannot be later than created-to date.");
    }
}

public sealed class GetLeadListQueryHandler : IRequestHandler<GetLeadListQuery, PagedResult<LeadListItemResponse>>
{
    private readonly IGapPhaseARepository _repository;

    public GetLeadListQueryHandler(IGapPhaseARepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<LeadListItemResponse>> Handle(GetLeadListQuery request, CancellationToken cancellationToken)
    {
        LeadStatus? leadStatus = null;
        LeadSourceChannel? sourceChannel = null;

        if (!string.IsNullOrWhiteSpace(request.LeadStatus))
        {
            LeadManagementSupport.TryParseLeadStatus(request.LeadStatus, out var parsedStatus);
            leadStatus = parsedStatus;
        }

        if (!string.IsNullOrWhiteSpace(request.SourceChannel))
        {
            LeadManagementSupport.TryParseLeadSourceChannel(request.SourceChannel, out var parsedSource);
            sourceChannel = parsedSource;
        }

        var leads = await _repository.SearchLeadsAsync(
            request.SearchTerm,
            leadStatus,
            sourceChannel,
            request.CreatedFrom,
            request.CreatedTo,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var totalCount = await _repository.CountLeadsAsync(
            request.SearchTerm,
            leadStatus,
            sourceChannel,
            request.CreatedFrom,
            request.CreatedTo,
            cancellationToken);

        return new PagedResult<LeadListItemResponse>(
            leads.Select(LeadManagementSupport.MapLeadListItem).ToArray(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}

public sealed record GetLeadDetailQuery(long LeadId) : IRequest<LeadDetailResponse>;

public sealed class GetLeadDetailQueryValidator : AbstractValidator<GetLeadDetailQuery>
{
    public GetLeadDetailQueryValidator()
    {
        RuleFor(request => request.LeadId).GreaterThan(0);
    }
}

public sealed class GetLeadDetailQueryHandler : IRequestHandler<GetLeadDetailQuery, LeadDetailResponse>
{
    private readonly IGapPhaseARepository _repository;

    public GetLeadDetailQueryHandler(IGapPhaseARepository repository)
    {
        _repository = repository;
    }

    public async Task<LeadDetailResponse> Handle(GetLeadDetailQuery request, CancellationToken cancellationToken)
    {
        var lead = await _repository.GetLeadByIdAsync(request.LeadId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested lead could not be found.", 404);

        return LeadManagementSupport.MapLeadDetail(lead);
    }
}

public sealed record GetLeadAnalyticsQuery(
    DateOnly? FromDate,
    DateOnly? ToDate) : IRequest<LeadAnalyticsResponse>;

public sealed class GetLeadAnalyticsQueryValidator : AbstractValidator<GetLeadAnalyticsQuery>
{
    public GetLeadAnalyticsQueryValidator()
    {
        RuleFor(request => request)
            .Must(request => !request.FromDate.HasValue || !request.ToDate.HasValue || request.FromDate.Value <= request.ToDate.Value)
            .WithMessage("Analytics from-date cannot be later than to-date.");
    }
}

public sealed class GetLeadAnalyticsQueryHandler : IRequestHandler<GetLeadAnalyticsQuery, LeadAnalyticsResponse>
{
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IGapPhaseARepository _repository;

    public GetLeadAnalyticsQueryHandler(
        IGapPhaseARepository repository,
        ICurrentDateTime currentDateTime)
    {
        _repository = repository;
        _currentDateTime = currentDateTime;
    }

    public async Task<LeadAnalyticsResponse> Handle(GetLeadAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var toDate = request.ToDate ?? DateOnly.FromDateTime(_currentDateTime.UtcNow);
        var fromDate = request.FromDate ?? toDate.AddDays(-29);
        var fromUtc = fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtcExclusive = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var leads = await _repository.ListLeadsCreatedBetweenAsync(fromUtc, toUtcExclusive, cancellationToken);

        var convertedLeads = leads.Count(
            lead =>
                lead.ConvertedBookingId.HasValue ||
                lead.ConvertedServiceRequestId.HasValue ||
                lead.LeadStatus is LeadStatus.Converted or LeadStatus.Closed);

        var leadsBySource = leads
            .GroupBy(lead => lead.SourceChannel)
            .OrderBy(group => group.Key)
            .Select(
                group => new LeadSourceAnalyticsResponse(
                    LeadManagementSupport.ToDisplayLeadSourceChannel(group.Key),
                    group.Count(),
                    group.Count(
                        lead =>
                            lead.ConvertedBookingId.HasValue ||
                            lead.ConvertedServiceRequestId.HasValue ||
                            lead.LeadStatus is LeadStatus.Converted or LeadStatus.Closed)))
            .ToArray();

        var dailyLeadCount = leads
            .GroupBy(lead => DateOnly.FromDateTime(lead.DateCreated))
            .OrderBy(group => group.Key)
            .Select(group => new LeadDailyCountResponse(group.Key, group.Count()))
            .ToArray();

        return new LeadAnalyticsResponse(
            fromDate,
            toDate,
            leads.Count,
            leads.Count(lead => lead.LeadStatus == LeadStatus.Contacted),
            leads.Count(lead => lead.LeadStatus == LeadStatus.Qualified),
            leads.Count(lead => lead.LeadStatus == LeadStatus.Converted),
            leads.Count(lead => lead.LeadStatus == LeadStatus.Lost),
            leads.Count(lead => lead.LeadStatus == LeadStatus.Closed),
            leads.Count == 0 ? 0.00m : Math.Round((decimal)convertedLeads / leads.Count * 100.00m, 2),
            leadsBySource,
            dailyLeadCount);
    }
}
