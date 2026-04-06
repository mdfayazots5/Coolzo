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

public sealed record CreateInstallationProposalCommand(
    long InstallationId,
    string? ProposalRemarks,
    IReadOnlyCollection<Coolzo.Contracts.Requests.GapPhaseC.InstallationProposalLineRequest> Lines) : IRequest<InstallationSummaryResponse>;

public sealed class CreateInstallationProposalCommandValidator : AbstractValidator<CreateInstallationProposalCommand>
{
    public CreateInstallationProposalCommandValidator()
    {
        RuleFor(request => request.InstallationId).GreaterThan(0);
        RuleFor(request => request.ProposalRemarks).MaximumLength(512);
        RuleFor(request => request.Lines).NotEmpty();
    }
}

public sealed class CreateInstallationProposalCommandHandler : IRequestHandler<CreateInstallationProposalCommand, InstallationSummaryResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IInstallationLifecycleReferenceGenerator _installationLifecycleReferenceGenerator;
    private readonly IInstallationLifecycleRepository _installationLifecycleRepository;
    private readonly InstallationLifecycleAccessService _accessService;
    private readonly InstallationLifecycleWorkflowService _workflowService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInstallationProposalCommandHandler(
        IInstallationLifecycleRepository installationLifecycleRepository,
        IInstallationLifecycleReferenceGenerator installationLifecycleReferenceGenerator,
        InstallationLifecycleAccessService accessService,
        InstallationLifecycleWorkflowService workflowService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _installationLifecycleRepository = installationLifecycleRepository;
        _installationLifecycleReferenceGenerator = installationLifecycleReferenceGenerator;
        _accessService = accessService;
        _workflowService = workflowService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<InstallationSummaryResponse> Handle(CreateInstallationProposalCommand request, CancellationToken cancellationToken)
    {
        if (!_accessService.HasManagementAccess())
        {
            throw new AppException(ErrorCodes.Forbidden, "Only operations users can generate installation proposals.", 403);
        }

        var installation = await _installationLifecycleRepository.GetInstallationByIdForUpdateAsync(request.InstallationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The installation could not be found.", 404);

        if (installation.InstallationStatus is not InstallationLifecycleStatus.SurveyCompleted and not InstallationLifecycleStatus.ProposalRejected)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "A completed survey is required before generating the proposal.", 409);
        }

        var actorName = InstallationLifecycleSupport.ResolveActorName(_currentUserContext, "InstallationProposal");
        var actorRole = InstallationLifecycleSupport.ResolveActorRole(_currentUserContext);
        var now = _currentDateTime.UtcNow;

        var lines = request.Lines
            .Where(line => !string.IsNullOrWhiteSpace(line.LineDescription))
            .Select(line => new InstallationProposalLine
            {
                LineDescription = line.LineDescription.Trim(),
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineTotal = line.Quantity * line.UnitPrice,
                Remarks = line.Remarks?.Trim() ?? string.Empty,
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            })
            .ToArray();

        var subTotal = lines.Sum(line => line.LineTotal);
        var proposal = new InstallationProposal
        {
            ProposalNumber = await GenerateProposalNumberAsync(cancellationToken),
            ProposalStatus = InstallationProposalStatus.PendingApproval,
            SubTotalAmount = subTotal,
            TaxAmount = 0.00m,
            TotalAmount = subTotal,
            ProposalRemarks = request.ProposalRemarks?.Trim() ?? string.Empty,
            GeneratedDateUtc = now,
            CreatedBy = actorName,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        foreach (var line in lines)
        {
            proposal.Lines.Add(line);
        }

        installation.ApprovalStatus = InstallationApprovalStatus.Pending;
        installation.Proposals.Add(proposal);

        _workflowService.EnsureTransition(
            installation,
            InstallationLifecycleStatus.ProposalGenerated,
            "Installation proposal generated.",
            actorName,
            actorRole,
            _currentUserContext.IPAddress,
            now);

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreateInstallationProposal",
                EntityName = nameof(InstallationLead),
                EntityId = installation.InstallationNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = proposal.ProposalNumber,
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InstallationLifecycleSupport.MapSummary(installation);
    }

    private async Task<string> GenerateProposalNumberAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var proposalNumber = _installationLifecycleReferenceGenerator.GenerateInstallationProposalNumber();

            if (!await _installationLifecycleRepository.ProposalNumberExistsAsync(proposalNumber, cancellationToken))
            {
                return proposalNumber;
            }
        }
    }
}

public sealed record ApproveInstallationProposalCommand(long InstallationId, string? CustomerRemarks) : IRequest<InstallationSummaryResponse>;

public sealed class ApproveInstallationProposalCommandValidator : AbstractValidator<ApproveInstallationProposalCommand>
{
    public ApproveInstallationProposalCommandValidator()
    {
        RuleFor(request => request.InstallationId).GreaterThan(0);
        RuleFor(request => request.CustomerRemarks).MaximumLength(512);
    }
}

public sealed class ApproveInstallationProposalCommandHandler : IRequestHandler<ApproveInstallationProposalCommand, InstallationSummaryResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IInstallationLifecycleRepository _installationLifecycleRepository;
    private readonly InstallationLifecycleAccessService _accessService;
    private readonly InstallationLifecycleWorkflowService _workflowService;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveInstallationProposalCommandHandler(
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

    public async Task<InstallationSummaryResponse> Handle(ApproveInstallationProposalCommand request, CancellationToken cancellationToken)
    {
        var installation = await _installationLifecycleRepository.GetInstallationByIdForUpdateAsync(request.InstallationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The installation could not be found.", 404);

        await _accessService.EnsureProposalDecisionAccessAsync(installation, cancellationToken);

        var proposal = installation.Proposals
            .Where(item => !item.IsDeleted)
            .OrderByDescending(item => item.GeneratedDateUtc)
            .FirstOrDefault()
            ?? throw new AppException(ErrorCodes.NotFound, "No installation proposal was found for approval.", 404);

        if (proposal.ProposalStatus != InstallationProposalStatus.PendingApproval)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "The latest proposal is not awaiting approval.", 409);
        }

        var actorName = InstallationLifecycleSupport.ResolveActorName(_currentUserContext, "CustomerApproval");
        var actorRole = InstallationLifecycleSupport.ResolveActorRole(_currentUserContext);
        var now = _currentDateTime.UtcNow;

        proposal.ProposalStatus = InstallationProposalStatus.Approved;
        proposal.CustomerRemarks = request.CustomerRemarks?.Trim() ?? string.Empty;
        proposal.DecisionDateUtc = now;
        proposal.LastUpdated = now;
        proposal.UpdatedBy = actorName;
        installation.ApprovalStatus = InstallationApprovalStatus.Approved;
        installation.ProposalApprovedDateUtc = now;

        _workflowService.EnsureTransition(
            installation,
            InstallationLifecycleStatus.ProposalApproved,
            "Installation proposal approved.",
            actorName,
            actorRole,
            _currentUserContext.IPAddress,
            now);

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "ApproveInstallationProposal",
                EntityName = nameof(InstallationLead),
                EntityId = installation.InstallationNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = proposal.ProposalNumber,
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InstallationLifecycleSupport.MapSummary(installation);
    }
}

public sealed record RejectInstallationProposalCommand(long InstallationId, string? CustomerRemarks) : IRequest<InstallationSummaryResponse>;

public sealed class RejectInstallationProposalCommandValidator : AbstractValidator<RejectInstallationProposalCommand>
{
    public RejectInstallationProposalCommandValidator()
    {
        RuleFor(request => request.InstallationId).GreaterThan(0);
        RuleFor(request => request.CustomerRemarks).MaximumLength(512);
    }
}

public sealed class RejectInstallationProposalCommandHandler : IRequestHandler<RejectInstallationProposalCommand, InstallationSummaryResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IInstallationLifecycleRepository _installationLifecycleRepository;
    private readonly InstallationLifecycleAccessService _accessService;
    private readonly InstallationLifecycleWorkflowService _workflowService;
    private readonly IUnitOfWork _unitOfWork;

    public RejectInstallationProposalCommandHandler(
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

    public async Task<InstallationSummaryResponse> Handle(RejectInstallationProposalCommand request, CancellationToken cancellationToken)
    {
        var installation = await _installationLifecycleRepository.GetInstallationByIdForUpdateAsync(request.InstallationId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The installation could not be found.", 404);

        await _accessService.EnsureProposalDecisionAccessAsync(installation, cancellationToken);

        var proposal = installation.Proposals
            .Where(item => !item.IsDeleted)
            .OrderByDescending(item => item.GeneratedDateUtc)
            .FirstOrDefault()
            ?? throw new AppException(ErrorCodes.NotFound, "No installation proposal was found for rejection.", 404);

        if (proposal.ProposalStatus != InstallationProposalStatus.PendingApproval)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "The latest proposal is not awaiting approval.", 409);
        }

        var actorName = InstallationLifecycleSupport.ResolveActorName(_currentUserContext, "CustomerApproval");
        var actorRole = InstallationLifecycleSupport.ResolveActorRole(_currentUserContext);
        var now = _currentDateTime.UtcNow;

        proposal.ProposalStatus = InstallationProposalStatus.Rejected;
        proposal.CustomerRemarks = request.CustomerRemarks?.Trim() ?? string.Empty;
        proposal.DecisionDateUtc = now;
        proposal.LastUpdated = now;
        proposal.UpdatedBy = actorName;
        installation.ApprovalStatus = InstallationApprovalStatus.Rejected;

        _workflowService.EnsureTransition(
            installation,
            InstallationLifecycleStatus.ProposalRejected,
            "Installation proposal rejected.",
            actorName,
            actorRole,
            _currentUserContext.IPAddress,
            now);

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "RejectInstallationProposal",
                EntityName = nameof(InstallationLead),
                EntityId = installation.InstallationNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = proposal.ProposalNumber,
                CreatedBy = actorName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InstallationLifecycleSupport.MapSummary(installation);
    }
}
