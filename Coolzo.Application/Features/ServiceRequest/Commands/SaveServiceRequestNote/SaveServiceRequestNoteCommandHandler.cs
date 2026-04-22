using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.FieldExecution;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.ServiceRequest.Commands.SaveServiceRequestNote;

public sealed class SaveServiceRequestNoteCommandHandler : IRequestHandler<SaveServiceRequestNoteCommand, JobExecutionNoteResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IJobCardFactory _jobCardFactory;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveServiceRequestNoteCommandHandler(
        IServiceRequestRepository serviceRequestRepository,
        IJobCardFactory jobCardFactory,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _jobCardFactory = jobCardFactory;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<JobExecutionNoteResponse> Handle(SaveServiceRequestNoteCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _serviceRequestRepository.GetByIdForUpdateAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);

        var jobCard = _jobCardFactory.EnsureCreated(serviceRequest);
        var now = _currentDateTime.UtcNow;
        var userName = string.IsNullOrWhiteSpace(_currentUserContext.UserName) ? "ServiceRequestNote" : _currentUserContext.UserName;
        var ipAddress = _currentUserContext.IPAddress;
        var note = new Coolzo.Domain.Entities.JobExecutionNote
        {
            NoteText = request.NoteText.Trim(),
            IsCustomerVisible = request.IsCustomerVisible,
            NoteDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        };

        jobCard.ExecutionNotes.Add(note);
        jobCard.ExecutionTimelines.Add(new Coolzo.Domain.Entities.JobExecutionTimeline
        {
            Status = serviceRequest.CurrentStatus,
            EventType = "NoteSaved",
            EventTitle = request.IsCustomerVisible ? "Customer Visible Note" : "Internal Note",
            Remarks = note.NoteText,
            EventDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        });

        await _auditLogRepository.AddAsync(
            new Coolzo.Domain.Entities.AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "SaveServiceRequestNote",
                EntityName = "ServiceRequest",
                EntityId = serviceRequest.ServiceRequestNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = note.NoteText,
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = ipAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new JobExecutionNoteResponse(
            note.JobExecutionNoteId,
            note.NoteText,
            note.IsCustomerVisible,
            note.CreatedBy,
            note.NoteDateUtc);
    }
}
