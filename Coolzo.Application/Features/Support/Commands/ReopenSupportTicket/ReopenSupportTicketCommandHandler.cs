using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.ReopenSupportTicket;

public sealed class ReopenSupportTicketCommandHandler : IRequestHandler<ReopenSupportTicketCommand, SupportTicketDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<ReopenSupportTicketCommandHandler> _logger;
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly SupportTicketAccessService _supportTicketAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public ReopenSupportTicketCommandHandler(
        ISupportTicketRepository supportTicketRepository,
        SupportTicketAccessService supportTicketAccessService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<ReopenSupportTicketCommandHandler> logger)
    {
        _supportTicketRepository = supportTicketRepository;
        _supportTicketAccessService = supportTicketAccessService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<SupportTicketDetailResponse> Handle(ReopenSupportTicketCommand request, CancellationToken cancellationToken)
    {
        var supportTicket = await _supportTicketAccessService.GetTicketForUpdateAsync(request.SupportTicketId, cancellationToken);

        if (supportTicket.CurrentStatus != SupportTicketStatus.Closed)
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "Only closed support tickets can be reopened.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
        var remarks = string.IsNullOrWhiteSpace(request.Remarks) ? "Support ticket reopened." : request.Remarks.Trim();

        supportTicket.CurrentStatus = SupportTicketStatus.Reopened;
        supportTicket.UpdatedBy = userName;
        supportTicket.LastUpdated = now;
        supportTicket.StatusHistories.Add(new SupportTicketStatusHistory
        {
            SupportTicketStatus = SupportTicketStatus.Reopened,
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
                ActionName = "ReopenSupportTicket",
                EntityName = "SupportTicket",
                EntityId = supportTicket.TicketNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = SupportTicketStatus.Reopened.ToString(),
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Support ticket {SupportTicketId} reopened.", supportTicket.SupportTicketId);

        var updatedTicket = await _supportTicketRepository.GetByIdAsync(supportTicket.SupportTicketId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The updated support ticket could not be loaded.", 404);

        var canManage = _supportTicketAccessService.CanManage();

        return SupportTicketResponseMapper.ToDetail(updatedTicket, canManage, canManage, false, canManage);
    }
}
