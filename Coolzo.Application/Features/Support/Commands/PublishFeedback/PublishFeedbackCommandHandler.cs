using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.PublishFeedback;

public sealed class PublishFeedbackCommandHandler : IRequestHandler<PublishFeedbackCommand, SupportFeedbackResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ICustomerAppRepository _customerAppRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PublishFeedbackCommandHandler(
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

    public async Task<SupportFeedbackResponse> Handle(PublishFeedbackCommand request, CancellationToken cancellationToken)
    {
        var feedback = await _customerAppRepository.GetFeedbackByIdForUpdateAsync(request.CustomerReviewId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested feedback could not be found.", 404);

        var now = _currentDateTime.UtcNow;

        feedback.IsPublished = request.Publish;
        feedback.DisplayOnWeb = request.Publish;
        feedback.Tag = request.Publish ? null : "unpublished";
        feedback.UpdatedBy = _currentUserContext.UserName;
        feedback.LastUpdated = now;

        if (request.Publish)
        {
            feedback.DatePublished = now;
            feedback.PublishedBy = _currentUserContext.UserName;
        }

        await WriteAuditAsync("PublishFeedback", feedback.CustomerReviewId.ToString(), request.Publish ? "published" : "unpublished", now, cancellationToken);
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
