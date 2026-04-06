using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Domain.Rules;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.ChangeSupportTicketStatus;

public sealed class ChangeSupportTicketStatusCommandHandler : IRequestHandler<ChangeSupportTicketStatusCommand, SupportTicketDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<ChangeSupportTicketStatusCommandHandler> _logger;
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly SupportTicketAccessService _supportTicketAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeSupportTicketStatusCommandHandler(
        ISupportTicketRepository supportTicketRepository,
        SupportTicketAccessService supportTicketAccessService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<ChangeSupportTicketStatusCommandHandler> logger)
    {
        _supportTicketRepository = supportTicketRepository;
        _supportTicketAccessService = supportTicketAccessService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<SupportTicketDetailResponse> Handle(ChangeSupportTicketStatusCommand request, CancellationToken cancellationToken)
    {
        _supportTicketAccessService.EnsureCanManage();

        var supportTicket = await _supportTicketAccessService.GetTicketForUpdateAsync(request.SupportTicketId, cancellationToken);

        if (!Enum.TryParse<SupportTicketStatus>(request.Status, true, out var targetStatus))
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "The requested support ticket status is invalid.", 400);
        }

        if (targetStatus is SupportTicketStatus.Escalated or SupportTicketStatus.Closed or SupportTicketStatus.Reopened)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "Use the dedicated escalation, close, or reopen action for this status change.", 409);
        }

        if (targetStatus == supportTicket.CurrentStatus || !SupportTicketStatusRule.CanTransition(supportTicket.CurrentStatus, targetStatus))
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "The requested support ticket status transition is not allowed.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
        var remarks = string.IsNullOrWhiteSpace(request.Remarks)
            ? $"Support ticket moved to {targetStatus}."
            : request.Remarks.Trim();

        supportTicket.CurrentStatus = targetStatus;
        supportTicket.UpdatedBy = userName;
        supportTicket.LastUpdated = now;
        supportTicket.StatusHistories.Add(new SupportTicketStatusHistory
        {
            SupportTicketStatus = targetStatus,
            Remarks = remarks,
            StatusDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "ChangeSupportTicketStatus",
                EntityName = "SupportTicket",
                EntityId = supportTicket.TicketNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = targetStatus.ToString(),
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Support ticket {SupportTicketId} moved to status {Status}.", supportTicket.SupportTicketId, targetStatus);

        var updatedTicket = await _supportTicketRepository.GetByIdAsync(supportTicket.SupportTicketId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The updated support ticket could not be loaded.", 404);

        return SupportTicketResponseMapper.ToDetail(updatedTicket, true, true, false, true);
    }
}
