using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.FieldExecution;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.FieldExecution.Commands.SaveJobExecutionNote;

public sealed class SaveJobExecutionNoteCommandHandler : IRequestHandler<SaveJobExecutionNoteCommand, JobExecutionNoteResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IJobCardFactory _jobCardFactory;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public SaveJobExecutionNoteCommandHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        IJobCardFactory jobCardFactory,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _jobCardFactory = jobCardFactory;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<JobExecutionNoteResponse> Handle(SaveJobExecutionNoteCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestForUpdateAsync(request.ServiceRequestId, cancellationToken);
        var jobCard = _jobCardFactory.EnsureCreated(serviceRequest);
        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
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
                ActionName = "SaveJobExecutionNote",
                EntityName = "JobCard",
                EntityId = jobCard.JobCardNumber,
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
