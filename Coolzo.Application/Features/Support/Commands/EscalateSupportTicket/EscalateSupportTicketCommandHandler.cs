using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Domain.Rules;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.EscalateSupportTicket;

public sealed class EscalateSupportTicketCommandHandler : IRequestHandler<EscalateSupportTicketCommand, SupportTicketDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<EscalateSupportTicketCommandHandler> _logger;
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly SupportTicketAccessService _supportTicketAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public EscalateSupportTicketCommandHandler(
        ISupportTicketRepository supportTicketRepository,
        SupportTicketAccessService supportTicketAccessService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<EscalateSupportTicketCommandHandler> logger)
    {
        _supportTicketRepository = supportTicketRepository;
        _supportTicketAccessService = supportTicketAccessService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<SupportTicketDetailResponse> Handle(EscalateSupportTicketCommand request, CancellationToken cancellationToken)
    {
        _supportTicketAccessService.EnsureCanManage();

        var supportTicket = await _supportTicketAccessService.GetTicketForUpdateAsync(request.SupportTicketId, cancellationToken);

        if (!SupportTicketStatusRule.CanTransition(supportTicket.CurrentStatus, SupportTicketStatus.Escalated))
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "The requested support ticket cannot be escalated from its current status.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
        var ipAddress = _currentUserContext.IPAddress;

        supportTicket.CurrentStatus = SupportTicketStatus.Escalated;
        supportTicket.UpdatedBy = userName;
        supportTicket.LastUpdated = now;
        supportTicket.Escalations.Add(new SupportTicketEscalation
        {
            EscalationTarget = request.EscalationTarget.Trim(),
            EscalationRemarks = request.EscalationRemarks.Trim(),
            EscalatedDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        });
        supportTicket.StatusHistories.Add(new SupportTicketStatusHistory
        {
            SupportTicketStatus = SupportTicketStatus.Escalated,
            Remarks = request.EscalationRemarks.Trim(),
            StatusDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        });

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "EscalateSupportTicket",
                EntityName = "SupportTicket",
                EntityId = supportTicket.TicketNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = request.EscalationTarget.Trim(),
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = ipAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Support ticket {SupportTicketId} escalated to {EscalationTarget}.", supportTicket.SupportTicketId, request.EscalationTarget);

        var updatedTicket = await _supportTicketRepository.GetByIdAsync(supportTicket.SupportTicketId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The updated support ticket could not be loaded.", 404);

        return SupportTicketResponseMapper.ToDetail(updatedTicket, true, true, false, true);
    }
}
