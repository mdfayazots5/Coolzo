using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.AssignSupportTicket;

public sealed class AssignSupportTicketCommandHandler : IRequestHandler<AssignSupportTicketCommand, SupportTicketDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<AssignSupportTicketCommandHandler> _logger;
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly SupportTicketAccessService _supportTicketAccessService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public AssignSupportTicketCommandHandler(
        ISupportTicketRepository supportTicketRepository,
        IUserRepository userRepository,
        SupportTicketAccessService supportTicketAccessService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<AssignSupportTicketCommandHandler> logger)
    {
        _supportTicketRepository = supportTicketRepository;
        _userRepository = userRepository;
        _supportTicketAccessService = supportTicketAccessService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<SupportTicketDetailResponse> Handle(AssignSupportTicketCommand request, CancellationToken cancellationToken)
    {
        _supportTicketAccessService.EnsureCanManage();

        var supportTicket = await _supportTicketAccessService.GetTicketForUpdateAsync(request.SupportTicketId, cancellationToken);
        var assignedUser = await _userRepository.GetByIdWithRolesAsync(request.AssignedUserId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested support ticket owner could not be found.", 404);

        if (!assignedUser.IsActive || !assignedUser.UserRoles.Any(userRole =>
                userRole.Role != null &&
                !string.Equals(userRole.Role.RoleName, RoleNames.Customer, StringComparison.OrdinalIgnoreCase)))
        {
            throw new AppException(ErrorCodes.InactiveUser, "The requested support ticket owner is not an active internal user.", 409);
        }

        var activeAssignment = supportTicket.Assignments
            .FirstOrDefault(assignment => assignment.IsActiveAssignment && !assignment.IsDeleted);

        if (activeAssignment?.AssignedUserId == assignedUser.UserId)
        {
            throw new AppException(ErrorCodes.DuplicateAssignment, "The requested support ticket owner is already assigned.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
        var ipAddress = _currentUserContext.IPAddress;

        if (activeAssignment is not null)
        {
            activeAssignment.IsActiveAssignment = false;
            activeAssignment.UpdatedBy = userName;
            activeAssignment.LastUpdated = now;
        }

        supportTicket.Assignments.Add(new SupportTicketAssignment
        {
            AssignedUserId = assignedUser.UserId,
            AssignedDateUtc = now,
            AssignmentRemarks = string.IsNullOrWhiteSpace(request.Remarks) ? "Support ticket assigned." : request.Remarks.Trim(),
            IsActiveAssignment = true,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        });

        supportTicket.UpdatedBy = userName;
        supportTicket.LastUpdated = now;

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "AssignSupportTicket",
                EntityName = "SupportTicket",
                EntityId = supportTicket.TicketNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = assignedUser.FullName,
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = ipAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Support ticket {SupportTicketId} assigned to user {AssignedUserId}.", supportTicket.SupportTicketId, assignedUser.UserId);

        var updatedTicket = await _supportTicketRepository.GetByIdAsync(supportTicket.SupportTicketId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The updated support ticket could not be loaded.", 404);

        return SupportTicketResponseMapper.ToDetail(updatedTicket, true, true, false, true);
    }
}
