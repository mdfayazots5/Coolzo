using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.CloseSupportTicket;

public sealed class CloseSupportTicketCommandHandler : IRequestHandler<CloseSupportTicketCommand, SupportTicketDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CloseSupportTicketCommandHandler> _logger;
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly SupportTicketAccessService _supportTicketAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public CloseSupportTicketCommandHandler(
        ISupportTicketRepository supportTicketRepository,
        SupportTicketAccessService supportTicketAccessService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<CloseSupportTicketCommandHandler> logger)
    {
        _supportTicketRepository = supportTicketRepository;
        _supportTicketAccessService = supportTicketAccessService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<SupportTicketDetailResponse> Handle(CloseSupportTicketCommand request, CancellationToken cancellationToken)
    {
        var supportTicket = await _supportTicketAccessService.GetTicketForUpdateAsync(request.SupportTicketId, cancellationToken);

        if (supportTicket.CurrentStatus != SupportTicketStatus.Resolved)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "Only resolved support tickets can be closed.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
        var remarks = string.IsNullOrWhiteSpace(request.Remarks) ? "Support ticket closed." : request.Remarks.Trim();

        supportTicket.CurrentStatus = SupportTicketStatus.Closed;
        supportTicket.UpdatedBy = userName;
        supportTicket.LastUpdated = now;
        supportTicket.StatusHistories.Add(new SupportTicketStatusHistory
        {
            SupportTicketStatus = SupportTicketStatus.Closed,
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
                ActionName = "CloseSupportTicket",
                EntityName = "SupportTicket",
                EntityId = supportTicket.TicketNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = SupportTicketStatus.Closed.ToString(),
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Support ticket {SupportTicketId} closed.", supportTicket.SupportTicketId);

        var updatedTicket = await _supportTicketRepository.GetByIdAsync(supportTicket.SupportTicketId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The updated support ticket could not be loaded.", 404);

        var canManage = _supportTicketAccessService.CanManage();

        return SupportTicketResponseMapper.ToDetail(updatedTicket, canManage, canManage, false, canManage);
    }
}
