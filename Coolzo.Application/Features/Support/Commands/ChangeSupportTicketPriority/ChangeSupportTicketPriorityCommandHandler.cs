using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.ChangeSupportTicketPriority;

public sealed class ChangeSupportTicketPriorityCommandHandler : IRequestHandler<ChangeSupportTicketPriorityCommand, SupportTicketDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<ChangeSupportTicketPriorityCommandHandler> _logger;
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly SupportTicketAccessService _supportTicketAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeSupportTicketPriorityCommandHandler(
        ISupportTicketRepository supportTicketRepository,
        SupportTicketAccessService supportTicketAccessService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<ChangeSupportTicketPriorityCommandHandler> logger)
    {
        _supportTicketRepository = supportTicketRepository;
        _supportTicketAccessService = supportTicketAccessService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<SupportTicketDetailResponse> Handle(ChangeSupportTicketPriorityCommand request, CancellationToken cancellationToken)
    {
        _supportTicketAccessService.EnsureCanManage();

        var supportTicket = await _supportTicketAccessService.GetTicketForUpdateAsync(request.SupportTicketId, cancellationToken);
        _ = await _supportTicketRepository.GetPriorityByIdAsync(request.SupportTicketPriorityId, cancellationToken)
            ?? throw new AppException(ErrorCodes.InvalidMasterSelection, "The requested support ticket priority is invalid.", 400);

        if (supportTicket.SupportTicketPriorityId == request.SupportTicketPriorityId)
        {
            throw new AppException(ErrorCodes.Conflict, "The requested support ticket priority is already active.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;

        supportTicket.SupportTicketPriorityId = request.SupportTicketPriorityId;
        supportTicket.UpdatedBy = userName;
        supportTicket.LastUpdated = now;

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "ChangeSupportTicketPriority",
                EntityName = "SupportTicket",
                EntityId = supportTicket.TicketNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = request.SupportTicketPriorityId.ToString(),
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Support ticket {SupportTicketId} priority changed to {PriorityId}.", supportTicket.SupportTicketId, request.SupportTicketPriorityId);

        var updatedTicket = await _supportTicketRepository.GetByIdAsync(supportTicket.SupportTicketId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The updated support ticket could not be loaded.", 404);

        return SupportTicketResponseMapper.ToDetail(updatedTicket, true, true, false, true);
    }
}
