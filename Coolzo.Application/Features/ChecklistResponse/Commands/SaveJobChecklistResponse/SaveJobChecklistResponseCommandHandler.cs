using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.TechnicianJob;
using Coolzo.Contracts.Responses.FieldExecution;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.ChecklistResponse.Commands.SaveJobChecklistResponse;

public sealed class SaveJobChecklistResponseCommandHandler : IRequestHandler<SaveJobChecklistResponseCommand, IReadOnlyCollection<JobChecklistItemResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFieldLookupRepository _fieldLookupRepository;
    private readonly IJobCardFactory _jobCardFactory;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public SaveJobChecklistResponseCommandHandler(
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

    public async Task<IReadOnlyCollection<JobChecklistItemResponse>> Handle(SaveJobChecklistResponseCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestForUpdateAsync(request.ServiceRequestId, cancellationToken);
        var serviceId = serviceRequest.Booking?.BookingLines
            .OrderBy(line => line.BookingLineId)
            .Select(line => line.ServiceId)
            .FirstOrDefault() ?? 0;
        var checklistMasters = serviceId > 0
            ? await _fieldLookupRepository.GetChecklistByServiceIdAsync(serviceId, cancellationToken)
            : Array.Empty<Coolzo.Domain.Entities.ServiceChecklistMaster>();

        if (checklistMasters.Count == 0)
        {
            return Array.Empty<JobChecklistItemResponse>();
        }

        var masterLookup = checklistMasters.ToDictionary(master => master.ServiceChecklistMasterId);
        var jobCard = _jobCardFactory.EnsureCreated(serviceRequest);
        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
        var ipAddress = _currentUserContext.IPAddress;

        foreach (var item in request.Items)
        {
            if (!masterLookup.TryGetValue(item.ServiceChecklistMasterId, out _))
            {
                throw new AppException(ErrorCodes.NotFound, "One or more checklist items could not be found.", 404);
            }

            var response = jobCard.ChecklistResponses
                .FirstOrDefault(existing => existing.ServiceChecklistMasterId == item.ServiceChecklistMasterId && !existing.IsDeleted);

            if (response is null)
            {
                jobCard.ChecklistResponses.Add(new Coolzo.Domain.Entities.JobChecklistResponse
                {
                    ServiceChecklistMasterId = item.ServiceChecklistMasterId,
                    IsChecked = item.IsChecked,
                    ResponseRemarks = item.ResponseRemarks?.Trim() ?? string.Empty,
                    ResponseDateUtc = now,
                    CreatedBy = userName,
                    DateCreated = now,
                    IPAddress = ipAddress
                });

                continue;
            }

            response.IsChecked = item.IsChecked;
            response.ResponseRemarks = item.ResponseRemarks?.Trim() ?? string.Empty;
            response.ResponseDateUtc = now;
            response.UpdatedBy = userName;
            response.LastUpdated = now;
            response.IPAddress = ipAddress;
        }

        jobCard.ExecutionTimelines.Add(new Coolzo.Domain.Entities.JobExecutionTimeline
        {
            Status = serviceRequest.CurrentStatus,
            EventType = "ChecklistSaved",
            EventTitle = "Checklist Updated",
            Remarks = $"Checklist responses updated for {request.Items.Count} item(s).",
            EventDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        });

        await _auditLogRepository.AddAsync(
            new Coolzo.Domain.Entities.AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "SaveJobChecklistResponse",
                EntityName = "JobCard",
                EntityId = jobCard.JobCardNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = request.Items.Count.ToString(),
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = ipAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var refreshedServiceRequest = await _technicianJobAccessService.GetOwnedServiceRequestAsync(request.ServiceRequestId, cancellationToken);
        return TechnicianJobResponseMapper.ToChecklistItems(refreshedServiceRequest, checklistMasters);
    }
}
