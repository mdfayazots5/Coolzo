using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.AddSupportTicketReply;

public sealed class AddSupportTicketReplyCommandHandler : IRequestHandler<AddSupportTicketReplyCommand, SupportTicketReplyResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<AddSupportTicketReplyCommandHandler> _logger;
    private readonly SupportTicketAccessService _supportTicketAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public AddSupportTicketReplyCommandHandler(
        SupportTicketAccessService supportTicketAccessService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<AddSupportTicketReplyCommandHandler> logger)
    {
        _supportTicketAccessService = supportTicketAccessService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<SupportTicketReplyResponse> Handle(AddSupportTicketReplyCommand request, CancellationToken cancellationToken)
    {
        var supportTicket = await _supportTicketAccessService.GetTicketForUpdateAsync(request.SupportTicketId, cancellationToken);
        var canManage = _supportTicketAccessService.CanManage();
        var isCustomer = _supportTicketAccessService.IsCustomer() && !canManage;

        if (isCustomer && request.IsInternalOnly)
        {
            throw new AppException(ErrorCodes.SupportAccessDenied, "Customers cannot add internal-only replies.", 403);
        }

        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
        var reply = new SupportTicketReply
        {
            ReplyText = request.ReplyText.Trim(),
            IsInternalOnly = request.IsInternalOnly,
            IsFromCustomer = isCustomer,
            ReplyDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        supportTicket.Replies.Add(reply);
        supportTicket.UpdatedBy = userName;
        supportTicket.LastUpdated = now;

        if (isCustomer && supportTicket.CurrentStatus == SupportTicketStatus.WaitingForCustomer)
        {
            supportTicket.CurrentStatus = SupportTicketStatus.CustomerResponded;
            supportTicket.StatusHistories.Add(new SupportTicketStatusHistory
            {
                SupportTicketStatus = SupportTicketStatus.CustomerResponded,
                Remarks = "Customer replied to the support ticket.",
                StatusDateUtc = now,
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });
        }

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "AddSupportTicketReply",
                EntityName = "SupportTicket",
                EntityId = supportTicket.TicketNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = reply.ReplyText,
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reply added to support ticket {SupportTicketId}.", supportTicket.SupportTicketId);

        return SupportTicketResponseMapper.ToReplyResponses(supportTicket, canManage).Last();
    }
}
