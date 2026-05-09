using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.FlagFeedback;

public sealed class FlagFeedbackCommandHandler : IRequestHandler<FlagFeedbackCommand, SupportFeedbackResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FlagFeedbackCommandHandler(
        ICustomerAppRepository customerAppRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _customerAppRepository = customerAppRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<SupportFeedbackResponse> Handle(FlagFeedbackCommand request, CancellationToken cancellationToken)
    {
        var feedback = await _customerAppRepository.GetFeedbackByIdForUpdateAsync(request.CustomerReviewId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested feedback could not be found.", 404);

        var now = _currentDateTime.UtcNow;
        var reason = request.Reason.Trim();

        feedback.IsPublished = false;
        feedback.DisplayOnWeb = false;
        feedback.Tag = FeedbackResponseMapper.BuildFlagTag(reason);
        feedback.UpdatedBy = _currentUserContext.UserName;
        feedback.LastUpdated = now;

        await WriteAuditAsync("FlagFeedback", feedback.CustomerReviewId.ToString(), reason, now, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FeedbackResponseMapper.ToResponse(feedback);
    }

    private Task WriteAuditAsync(string actionName, string entityId, string newValues, DateTime now, CancellationToken cancellationToken)
    {
        return _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = actionName,
                EntityName = "CustomerReview",
                EntityId = entityId,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = newValues,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
    }
}
