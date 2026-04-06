using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.TechnicianJob;
using Coolzo.Contracts.Responses.FieldExecution;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Diagnosis.Commands.SaveJobDiagnosis;

public sealed class SaveJobDiagnosisCommandHandler : IRequestHandler<SaveJobDiagnosisCommand, JobDiagnosisSummaryResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFieldLookupRepository _fieldLookupRepository;
    private readonly IJobCardFactory _jobCardFactory;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public SaveJobDiagnosisCommandHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        IJobCardFactory jobCardFactory,
        IFieldLookupRepository fieldLookupRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _jobCardFactory = jobCardFactory;
        _fieldLookupRepository = fieldLookupRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<JobDiagnosisSummaryResponse> Handle(SaveJobDiagnosisCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestForUpdateAsync(request.ServiceRequestId, cancellationToken);
        var jobCard = _jobCardFactory.EnsureCreated(serviceRequest);
        var issues = await _fieldLookupRepository.SearchComplaintIssuesAsync(null, cancellationToken);
        var results = await _fieldLookupRepository.SearchDiagnosisResultsAsync(null, cancellationToken);

        if (request.ComplaintIssueMasterId.HasValue &&
            issues.All(issue => issue.ComplaintIssueMasterId != request.ComplaintIssueMasterId.Value))
        {
            throw new AppException(ErrorCodes.NotFound, "The selected complaint issue could not be found.", 404);
        }

        if (request.DiagnosisResultMasterId.HasValue &&
            results.All(result => result.DiagnosisResultMasterId != request.DiagnosisResultMasterId.Value))
        {
            throw new AppException(ErrorCodes.NotFound, "The selected diagnosis result could not be found.", 404);
        }

        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
        var ipAddress = _currentUserContext.IPAddress;
        var diagnosis = jobCard.JobDiagnosis;

        if (diagnosis is null)
        {
            diagnosis = new JobDiagnosis
            {
                DiagnosisDateUtc = now,
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = ipAddress
            };

            jobCard.JobDiagnosis = diagnosis;
        }

        diagnosis.ComplaintIssueMasterId = request.ComplaintIssueMasterId;
        diagnosis.DiagnosisResultMasterId = request.DiagnosisResultMasterId;
        diagnosis.DiagnosisRemarks = request.DiagnosisRemarks?.Trim() ?? string.Empty;
        diagnosis.DiagnosisDateUtc = now;
        diagnosis.UpdatedBy = userName;
        diagnosis.LastUpdated = now;
        diagnosis.IPAddress = ipAddress;

        jobCard.ExecutionTimelines.Add(new JobExecutionTimeline
        {
            Status = serviceRequest.CurrentStatus,
            EventType = "DiagnosisSaved",
            EventTitle = "Diagnosis Updated",
            Remarks = diagnosis.DiagnosisRemarks,
            EventDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        });

        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "SaveJobDiagnosis",
                EntityName = "JobCard",
                EntityId = jobCard.JobCardNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = diagnosis.DiagnosisRemarks,
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = ipAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var refreshedServiceRequest = await _technicianJobAccessService.GetOwnedServiceRequestAsync(request.ServiceRequestId, cancellationToken);
        return TechnicianJobResponseMapper.ToDiagnosisSummary(refreshedServiceRequest.JobCard);
    }
}
