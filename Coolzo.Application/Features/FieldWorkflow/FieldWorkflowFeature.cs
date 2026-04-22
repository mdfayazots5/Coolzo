using System.Text.Json;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Coolzo.Application.Common.Services;
using Coolzo.Application.Features.Billing;
using Coolzo.Application.Features.TechnicianJob;
using Coolzo.Contracts.Requests.Billing;
using Coolzo.Contracts.Requests.FieldExecution;
using Coolzo.Contracts.Requests.FieldWorkflow;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Contracts.Responses.FieldWorkflow;
using Coolzo.Contracts.Responses.Technician;
using Coolzo.Contracts.Responses.TechnicianJobs;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;
using BookingEntity = Coolzo.Domain.Entities.Booking;
using ServiceRequestEntity = Coolzo.Domain.Entities.ServiceRequest;

namespace Coolzo.Application.Features.FieldWorkflow;

public sealed record GetFieldMyJobsQuery() : IRequest<IReadOnlyCollection<TechnicianJobListItemResponse>>;

public sealed class GetFieldMyJobsQueryHandler : IRequestHandler<GetFieldMyJobsQuery, IReadOnlyCollection<TechnicianJobListItemResponse>>
{
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly TechnicianJobLifecycleResolver _technicianJobLifecycleResolver;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;

    public GetFieldMyJobsQueryHandler(
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianJobAccessService technicianJobAccessService,
        TechnicianJobLifecycleResolver technicianJobLifecycleResolver,
        ICurrentDateTime currentDateTime)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _technicianJobAccessService = technicianJobAccessService;
        _technicianJobLifecycleResolver = technicianJobLifecycleResolver;
        _currentDateTime = currentDateTime;
    }

    public async Task<IReadOnlyCollection<TechnicianJobListItemResponse>> Handle(GetFieldMyJobsQuery request, CancellationToken cancellationToken)
    {
        var technician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);
        var slotDate = DateOnly.FromDateTime(_currentDateTime.UtcNow);
        var jobs = await _serviceRequestRepository.SearchAssignedJobsAsync(
            technician.TechnicianId,
            currentStatus: null,
            slotDate,
            pageNumber: 1,
            pageSize: 100,
            cancellationToken);

        var items = await Task.WhenAll(
            jobs.Select(async job =>
            {
                var (lifecycleType, lifecycleLabel) = await _technicianJobLifecycleResolver.ResolveAsync(job.ServiceRequestId, cancellationToken);
                return TechnicianJobResponseMapper.ToListItem(job, lifecycleType, lifecycleLabel);
            }));

        return items;
    }
}

public sealed record GetFieldJobHistoryQuery() : IRequest<IReadOnlyCollection<TechnicianJobListItemResponse>>;

public sealed class GetFieldJobHistoryQueryHandler : IRequestHandler<GetFieldJobHistoryQuery, IReadOnlyCollection<TechnicianJobListItemResponse>>
{
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly TechnicianJobLifecycleResolver _technicianJobLifecycleResolver;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;

    public GetFieldJobHistoryQueryHandler(
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianJobAccessService technicianJobAccessService,
        TechnicianJobLifecycleResolver technicianJobLifecycleResolver)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _technicianJobAccessService = technicianJobAccessService;
        _technicianJobLifecycleResolver = technicianJobLifecycleResolver;
    }

    public async Task<IReadOnlyCollection<TechnicianJobListItemResponse>> Handle(GetFieldJobHistoryQuery request, CancellationToken cancellationToken)
    {
        var technician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);
        var jobs = await _serviceRequestRepository.SearchAssignedJobsAsync(
            technician.TechnicianId,
            ServiceRequestStatus.SubmittedForClosure,
            slotDate: null,
            pageNumber: 1,
            pageSize: 200,
            cancellationToken);

        var items = await Task.WhenAll(
            jobs.Select(async job =>
            {
                var (lifecycleType, lifecycleLabel) = await _technicianJobLifecycleResolver.ResolveAsync(job.ServiceRequestId, cancellationToken);
                return TechnicianJobResponseMapper.ToListItem(job, lifecycleType, lifecycleLabel);
            }));

        return items;
    }
}

public sealed record GetFieldJobDetailQuery(long ServiceRequestId) : IRequest<FieldJobDetailResponse>;

public sealed class GetFieldJobDetailQueryHandler : IRequestHandler<GetFieldJobDetailQuery, FieldJobDetailResponse>
{
    private readonly IBillingRepository _billingRepository;
    private readonly IFieldLookupRepository _fieldLookupRepository;
    private readonly IFieldWorkflowRepository _fieldWorkflowRepository;
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly TechnicianJobLifecycleResolver _technicianJobLifecycleResolver;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;

    public GetFieldJobDetailQueryHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        IFieldLookupRepository fieldLookupRepository,
        ISupportTicketRepository supportTicketRepository,
        TechnicianJobLifecycleResolver technicianJobLifecycleResolver,
        IFieldWorkflowRepository fieldWorkflowRepository,
        IBillingRepository billingRepository)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _fieldLookupRepository = fieldLookupRepository;
        _supportTicketRepository = supportTicketRepository;
        _technicianJobLifecycleResolver = technicianJobLifecycleResolver;
        _fieldWorkflowRepository = fieldWorkflowRepository;
        _billingRepository = billingRepository;
    }

    public Task<FieldJobDetailResponse> Handle(GetFieldJobDetailQuery request, CancellationToken cancellationToken)
    {
        return FieldWorkflowSupport.BuildFieldJobDetailAsync(
            request.ServiceRequestId,
            _technicianJobAccessService,
            _fieldLookupRepository,
            _supportTicketRepository,
            _technicianJobLifecycleResolver,
            _fieldWorkflowRepository,
            _billingRepository,
            cancellationToken);
    }
}

public sealed record DepartFieldJobCommand(
    long ServiceRequestId,
    double? Latitude,
    double? Longitude,
    string? Remarks) : IRequest<FieldJobDetailResponse>;

public sealed class DepartFieldJobCommandValidator : AbstractValidator<DepartFieldJobCommand>
{
    public DepartFieldJobCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class DepartFieldJobCommandHandler : IRequestHandler<DepartFieldJobCommand, FieldJobDetailResponse>
{
    private readonly IBillingRepository _billingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFieldLookupRepository _fieldLookupRepository;
    private readonly IFieldWorkflowRepository _fieldWorkflowRepository;
    private readonly ITechnicianFieldExecutionService _fieldExecutionService;
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly TechnicianJobLifecycleResolver _technicianJobLifecycleResolver;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;

    public DepartFieldJobCommandHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        ITechnicianFieldExecutionService fieldExecutionService,
        ITechnicianRepository technicianRepository,
        IFieldLookupRepository fieldLookupRepository,
        ISupportTicketRepository supportTicketRepository,
        TechnicianJobLifecycleResolver technicianJobLifecycleResolver,
        IFieldWorkflowRepository fieldWorkflowRepository,
        IBillingRepository billingRepository,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _fieldExecutionService = fieldExecutionService;
        _technicianRepository = technicianRepository;
        _fieldLookupRepository = fieldLookupRepository;
        _supportTicketRepository = supportTicketRepository;
        _technicianJobLifecycleResolver = technicianJobLifecycleResolver;
        _fieldWorkflowRepository = fieldWorkflowRepository;
        _billingRepository = billingRepository;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<FieldJobDetailResponse> Handle(DepartFieldJobCommand request, CancellationToken cancellationToken)
    {
        var technician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);
        var now = _currentDateTime.UtcNow;
        var actor = FieldWorkflowSupport.ResolveActor(_currentUserContext, "FieldWorkflow");

        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            await _technicianRepository.AddGpsLogAsync(
                new TechnicianGpsLog
                {
                    TechnicianId = technician.TechnicianId,
                    ServiceRequestId = request.ServiceRequestId,
                    TrackedOnUtc = now,
                    Latitude = (decimal)request.Latitude.Value,
                    Longitude = (decimal)request.Longitude.Value,
                    TrackingSource = "FieldDepart",
                    LocationText = request.Remarks?.Trim() ?? string.Empty,
                    CreatedBy = actor,
                    DateCreated = now,
                    IPAddress = _currentUserContext.IPAddress
                },
                cancellationToken);
        }

        await _fieldExecutionService.AdvanceStatusAsync(
            request.ServiceRequestId,
            ServiceRequestStatus.EnRoute,
            request.Remarks,
            workSummary: null,
            auditActionName: "DepartFieldJob",
            cancellationToken);

        return await FieldWorkflowSupport.BuildFieldJobDetailAsync(
            request.ServiceRequestId,
            _technicianJobAccessService,
            _fieldLookupRepository,
            _supportTicketRepository,
            _technicianJobLifecycleResolver,
            _fieldWorkflowRepository,
            _billingRepository,
            cancellationToken);
    }
}

public sealed record ArriveFieldJobCommand(
    long ServiceRequestId,
    double? Latitude,
    double? Longitude,
    string? Remarks,
    string? OverrideReason) : IRequest<FieldArrivalValidationResponse>;

public sealed class ArriveFieldJobCommandValidator : AbstractValidator<ArriveFieldJobCommand>
{
    public ArriveFieldJobCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.Latitude).NotNull();
        RuleFor(request => request.Longitude).NotNull();
        RuleFor(request => request.Remarks).MaximumLength(512);
        RuleFor(request => request.OverrideReason).MaximumLength(512);
    }
}

public sealed class ArriveFieldJobCommandHandler : IRequestHandler<ArriveFieldJobCommand, FieldArrivalValidationResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBillingRepository _billingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFieldLookupRepository _fieldLookupRepository;
    private readonly IFieldWorkflowRepository _fieldWorkflowRepository;
    private readonly ITechnicianFieldExecutionService _fieldExecutionService;
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly TechnicianJobLifecycleResolver _technicianJobLifecycleResolver;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;

    public ArriveFieldJobCommandHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        ITechnicianFieldExecutionService fieldExecutionService,
        ITechnicianRepository technicianRepository,
        IAuditLogRepository auditLogRepository,
        IFieldLookupRepository fieldLookupRepository,
        ISupportTicketRepository supportTicketRepository,
        TechnicianJobLifecycleResolver technicianJobLifecycleResolver,
        IFieldWorkflowRepository fieldWorkflowRepository,
        IBillingRepository billingRepository,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _fieldExecutionService = fieldExecutionService;
        _technicianRepository = technicianRepository;
        _auditLogRepository = auditLogRepository;
        _fieldLookupRepository = fieldLookupRepository;
        _supportTicketRepository = supportTicketRepository;
        _technicianJobLifecycleResolver = technicianJobLifecycleResolver;
        _fieldWorkflowRepository = fieldWorkflowRepository;
        _billingRepository = billingRepository;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<FieldArrivalValidationResponse> Handle(ArriveFieldJobCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestForUpdateAsync(request.ServiceRequestId, cancellationToken);
        var technician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);
        var distanceMeters = 0d;
        var arrivalLatitude = request.Latitude!.Value;
        var arrivalLongitude = request.Longitude!.Value;

        if (serviceRequest.Booking?.CustomerAddress?.Latitude is double customerLatitude &&
            serviceRequest.Booking.CustomerAddress.Longitude is double customerLongitude)
        {
            distanceMeters = FieldWorkflowSupport.CalculateDistanceMeters(
                arrivalLatitude,
                arrivalLongitude,
                customerLatitude,
                customerLongitude);

            if (distanceMeters > 150d && string.IsNullOrWhiteSpace(request.OverrideReason))
            {
                var currentDetail = await FieldWorkflowSupport.BuildFieldJobDetailAsync(
                    request.ServiceRequestId,
                    _technicianJobAccessService,
                    _fieldLookupRepository,
                    _supportTicketRepository,
                    _technicianJobLifecycleResolver,
                    _fieldWorkflowRepository,
                    _billingRepository,
                    cancellationToken);

                return new FieldArrivalValidationResponse(
                    true,
                    distanceMeters,
                    $"Arrival location is {distanceMeters:0}m away from the service address. Override reason is required.",
                    currentDetail);
            }
        }

        var now = _currentDateTime.UtcNow;
        var actor = FieldWorkflowSupport.ResolveActor(_currentUserContext, "FieldWorkflow");
        await _technicianRepository.AddGpsLogAsync(
            new TechnicianGpsLog
            {
                TechnicianId = technician.TechnicianId,
                ServiceRequestId = request.ServiceRequestId,
                TrackedOnUtc = now,
                Latitude = (decimal)arrivalLatitude,
                Longitude = (decimal)arrivalLongitude,
                TrackingSource = string.IsNullOrWhiteSpace(request.OverrideReason) ? "FieldArrive" : "FieldArriveOverride",
                LocationText = string.IsNullOrWhiteSpace(request.OverrideReason)
                    ? request.Remarks?.Trim() ?? string.Empty
                    : request.OverrideReason.Trim(),
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.OverrideReason))
        {
            await _auditLogRepository.AddAsync(
                FieldWorkflowSupport.CreateAuditLog(
                    _currentUserContext,
                    now,
                    "OverrideFieldArrivalCheckIn",
                    nameof(ServiceRequest),
                    serviceRequest.ServiceRequestNumber,
                    request.OverrideReason.Trim()),
                cancellationToken);
        }

        await _fieldExecutionService.AdvanceStatusAsync(
            request.ServiceRequestId,
            ServiceRequestStatus.Reached,
            request.Remarks,
            workSummary: null,
            auditActionName: "ArriveFieldJob",
            cancellationToken);

        var detail = await FieldWorkflowSupport.BuildFieldJobDetailAsync(
            request.ServiceRequestId,
            _technicianJobAccessService,
            _fieldLookupRepository,
            _supportTicketRepository,
            _technicianJobLifecycleResolver,
            _fieldWorkflowRepository,
            _billingRepository,
            cancellationToken);

        return new FieldArrivalValidationResponse(false, distanceMeters, "Field arrival recorded successfully.", detail);
    }
}

public sealed record StartFieldJobWorkCommand(
    long ServiceRequestId,
    string? Remarks) : IRequest<FieldJobDetailResponse>;

public sealed class StartFieldJobWorkCommandValidator : AbstractValidator<StartFieldJobWorkCommand>
{
    public StartFieldJobWorkCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class StartFieldJobWorkCommandHandler : IRequestHandler<StartFieldJobWorkCommand, FieldJobDetailResponse>
{
    private readonly IBillingRepository _billingRepository;
    private readonly IFieldLookupRepository _fieldLookupRepository;
    private readonly IFieldWorkflowRepository _fieldWorkflowRepository;
    private readonly ITechnicianFieldExecutionService _fieldExecutionService;
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly TechnicianJobLifecycleResolver _technicianJobLifecycleResolver;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;

    public StartFieldJobWorkCommandHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        ITechnicianFieldExecutionService fieldExecutionService,
        IFieldLookupRepository fieldLookupRepository,
        ISupportTicketRepository supportTicketRepository,
        TechnicianJobLifecycleResolver technicianJobLifecycleResolver,
        IFieldWorkflowRepository fieldWorkflowRepository,
        IBillingRepository billingRepository)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _fieldExecutionService = fieldExecutionService;
        _fieldLookupRepository = fieldLookupRepository;
        _supportTicketRepository = supportTicketRepository;
        _technicianJobLifecycleResolver = technicianJobLifecycleResolver;
        _fieldWorkflowRepository = fieldWorkflowRepository;
        _billingRepository = billingRepository;
    }

    public async Task<FieldJobDetailResponse> Handle(StartFieldJobWorkCommand request, CancellationToken cancellationToken)
    {
        await _fieldExecutionService.AdvanceStatusAsync(
            request.ServiceRequestId,
            ServiceRequestStatus.WorkStarted,
            request.Remarks,
            workSummary: null,
            auditActionName: "StartFieldJobWork",
            cancellationToken);

        return await FieldWorkflowSupport.BuildFieldJobDetailAsync(
            request.ServiceRequestId,
            _technicianJobAccessService,
            _fieldLookupRepository,
            _supportTicketRepository,
            _technicianJobLifecycleResolver,
            _fieldWorkflowRepository,
            _billingRepository,
            cancellationToken);
    }
}

public sealed record SaveFieldJobProgressCommand(
    long ServiceRequestId,
    IReadOnlyCollection<SaveJobChecklistResponseItemRequest> Items,
    string? Remarks) : IRequest<FieldJobDetailResponse>;

public sealed class SaveFieldJobProgressCommandValidator : AbstractValidator<SaveFieldJobProgressCommand>
{
    public SaveFieldJobProgressCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.Items).NotEmpty();
        RuleFor(request => request.Remarks).MaximumLength(512);
        RuleForEach(request => request.Items).SetValidator(new FieldWorkflowChecklistItemValidator());
    }
}

public sealed class SaveFieldJobProgressCommandHandler : IRequestHandler<SaveFieldJobProgressCommand, FieldJobDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBillingRepository _billingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFieldLookupRepository _fieldLookupRepository;
    private readonly IFieldWorkflowRepository _fieldWorkflowRepository;
    private readonly IJobCardFactory _jobCardFactory;
    private readonly ITechnicianFieldExecutionService _fieldExecutionService;
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly TechnicianJobLifecycleResolver _technicianJobLifecycleResolver;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public SaveFieldJobProgressCommandHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        IJobCardFactory jobCardFactory,
        IFieldLookupRepository fieldLookupRepository,
        IAuditLogRepository auditLogRepository,
        ITechnicianFieldExecutionService fieldExecutionService,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        ISupportTicketRepository supportTicketRepository,
        TechnicianJobLifecycleResolver technicianJobLifecycleResolver,
        IFieldWorkflowRepository fieldWorkflowRepository,
        IBillingRepository billingRepository)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _jobCardFactory = jobCardFactory;
        _fieldLookupRepository = fieldLookupRepository;
        _auditLogRepository = auditLogRepository;
        _fieldExecutionService = fieldExecutionService;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _supportTicketRepository = supportTicketRepository;
        _technicianJobLifecycleResolver = technicianJobLifecycleResolver;
        _fieldWorkflowRepository = fieldWorkflowRepository;
        _billingRepository = billingRepository;
    }

    public async Task<FieldJobDetailResponse> Handle(SaveFieldJobProgressCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestForUpdateAsync(request.ServiceRequestId, cancellationToken);

        if (serviceRequest.CurrentStatus is not ServiceRequestStatus.WorkStarted and not ServiceRequestStatus.WorkInProgress)
        {
            throw new AppException(
                ErrorCodes.InvalidStatusTransition,
                "Field progress can be recorded only after work has started.",
                409);
        }

        var serviceId = serviceRequest.Booking?.BookingLines
            .OrderBy(line => line.BookingLineId)
            .Select(line => line.ServiceId)
            .FirstOrDefault() ?? 0;
        var checklistMasters = serviceId > 0
            ? await _fieldLookupRepository.GetChecklistByServiceIdAsync(serviceId, cancellationToken)
            : Array.Empty<ServiceChecklistMaster>();
        var masterLookup = checklistMasters.ToDictionary(master => master.ServiceChecklistMasterId);
        var jobCard = _jobCardFactory.EnsureCreated(serviceRequest);
        var now = _currentDateTime.UtcNow;
        var actor = FieldWorkflowSupport.ResolveActor(_currentUserContext, "FieldWorkflow");

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
                jobCard.ChecklistResponses.Add(new JobChecklistResponse
                {
                    ServiceChecklistMasterId = item.ServiceChecklistMasterId,
                    IsChecked = item.IsChecked,
                    ResponseRemarks = item.ResponseRemarks?.Trim() ?? string.Empty,
                    ResponseDateUtc = now,
                    CreatedBy = actor,
                    DateCreated = now,
                    IPAddress = _currentUserContext.IPAddress
                });

                continue;
            }

            response.IsChecked = item.IsChecked;
            response.ResponseRemarks = item.ResponseRemarks?.Trim() ?? string.Empty;
            response.ResponseDateUtc = now;
            response.UpdatedBy = actor;
            response.LastUpdated = now;
            response.IPAddress = _currentUserContext.IPAddress;
        }

        jobCard.ExecutionTimelines.Add(new JobExecutionTimeline
        {
            Status = serviceRequest.CurrentStatus,
            EventType = "ChecklistSaved",
            EventTitle = "Checklist Updated",
            Remarks = string.IsNullOrWhiteSpace(request.Remarks)
                ? $"Checklist responses updated for {request.Items.Count} item(s)."
                : request.Remarks.Trim(),
            EventDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });

        await _auditLogRepository.AddAsync(
            FieldWorkflowSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "SaveFieldJobProgress",
                nameof(JobCard),
                jobCard.JobCardNumber,
                request.Items.Count.ToString()),
            cancellationToken);

        if (serviceRequest.CurrentStatus == ServiceRequestStatus.WorkStarted)
        {
            await _fieldExecutionService.AdvanceStatusAsync(
                request.ServiceRequestId,
                ServiceRequestStatus.WorkInProgress,
                request.Remarks,
                workSummary: null,
                auditActionName: "SaveFieldJobProgress",
                cancellationToken);
        }
        else
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return await FieldWorkflowSupport.BuildFieldJobDetailAsync(
            request.ServiceRequestId,
            _technicianJobAccessService,
            _fieldLookupRepository,
            _supportTicketRepository,
            _technicianJobLifecycleResolver,
            _fieldWorkflowRepository,
            _billingRepository,
            cancellationToken);
    }
}

public sealed record CreateFieldPartsRequestCommand(
    long ServiceRequestId,
    string Urgency,
    IReadOnlyCollection<FieldPartsRequestItemRequest> Items,
    string? Notes) : IRequest<FieldPartsRequestResponse>;

public sealed class CreateFieldPartsRequestCommandValidator : AbstractValidator<CreateFieldPartsRequestCommand>
{
    public CreateFieldPartsRequestCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.Urgency)
            .NotEmpty()
            .Must(FieldWorkflowSupport.BeValidPartsRequestUrgency)
            .WithMessage("Parts request urgency is invalid.");
        RuleFor(request => request.Items).NotEmpty();
        RuleFor(request => request.Notes).MaximumLength(1024);
        RuleForEach(request => request.Items).SetValidator(new FieldPartsRequestItemValidator());
    }
}

public sealed class CreateFieldPartsRequestCommandHandler : IRequestHandler<CreateFieldPartsRequestCommand, FieldPartsRequestResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFieldWorkflowRepository _fieldWorkflowRepository;
    private readonly IJobCardFactory _jobCardFactory;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFieldPartsRequestCommandHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        IFieldWorkflowRepository fieldWorkflowRepository,
        IJobCardFactory jobCardFactory,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _fieldWorkflowRepository = fieldWorkflowRepository;
        _jobCardFactory = jobCardFactory;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<FieldPartsRequestResponse> Handle(CreateFieldPartsRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestForUpdateAsync(request.ServiceRequestId, cancellationToken);
        var technician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);
        var urgency = Enum.Parse<PartsRequestUrgency>(request.Urgency, true);
        var jobCard = _jobCardFactory.EnsureCreated(serviceRequest);
        var now = _currentDateTime.UtcNow;
        var actor = FieldWorkflowSupport.ResolveActor(_currentUserContext, "FieldWorkflow");

        var partsRequest = new PartsRequest
        {
            ServiceRequestId = serviceRequest.ServiceRequestId,
            JobCard = jobCard,
            TechnicianId = technician.TechnicianId,
            Urgency = urgency,
            CurrentStatus = PartsRequestStatus.Pending,
            Notes = request.Notes?.Trim() ?? string.Empty,
            SubmittedAtUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        foreach (var item in request.Items)
        {
            partsRequest.Items.Add(new PartsRequestItem
            {
                PartCode = item.PartCode.Trim(),
                PartName = item.PartName.Trim(),
                QuantityRequested = item.QuantityRequested,
                QuantityApproved = 0.00m,
                ItemRemarks = item.Remarks?.Trim() ?? string.Empty,
                CurrentStatus = PartsRequestStatus.Pending,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });
        }

        jobCard.ExecutionTimelines.Add(new JobExecutionTimeline
        {
            Status = serviceRequest.CurrentStatus,
            EventType = "PartsRequestSubmitted",
            EventTitle = "Parts Requested",
            Remarks = string.IsNullOrWhiteSpace(partsRequest.Notes)
                ? $"{request.Items.Count} part item(s) requested."
                : partsRequest.Notes,
            EventDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });

        await _fieldWorkflowRepository.AddPartsRequestAsync(partsRequest, cancellationToken);
        await _auditLogRepository.AddAsync(
            FieldWorkflowSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "CreateFieldPartsRequest",
                nameof(PartsRequest),
                serviceRequest.ServiceRequestNumber,
                request.Items.Count.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FieldWorkflowSupport.MapPartsRequest(partsRequest);
    }
}

public sealed record CreateFieldEstimateCommand(
    long ServiceRequestId,
    IReadOnlyCollection<QuotationLineRequest> Lines,
    decimal DiscountAmount,
    decimal TaxPercentage,
    string? Remarks) : IRequest<QuotationDetailResponse>;

public sealed class CreateFieldEstimateCommandValidator : AbstractValidator<CreateFieldEstimateCommand>
{
    public CreateFieldEstimateCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.Lines).NotEmpty();
        RuleFor(request => request.DiscountAmount).GreaterThanOrEqualTo(0.00m);
        RuleFor(request => request.TaxPercentage).GreaterThanOrEqualTo(0.00m).LessThanOrEqualTo(100.00m);
        RuleFor(request => request.Remarks).MaximumLength(512);
        RuleForEach(request => request.Lines).SetValidator(new FieldQuotationLineValidator());
    }
}

public sealed class CreateFieldEstimateCommandHandler : IRequestHandler<CreateFieldEstimateCommand, QuotationDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBillingCalculationService _billingCalculationService;
    private readonly IBillingRepository _billingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IJobCardFactory _jobCardFactory;
    private readonly IQuotationNumberGenerator _quotationNumberGenerator;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFieldEstimateCommandHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        IJobCardFactory jobCardFactory,
        IBillingRepository billingRepository,
        IBillingCalculationService billingCalculationService,
        IQuotationNumberGenerator quotationNumberGenerator,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _jobCardFactory = jobCardFactory;
        _billingRepository = billingRepository;
        _billingCalculationService = billingCalculationService;
        _quotationNumberGenerator = quotationNumberGenerator;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<QuotationDetailResponse> Handle(CreateFieldEstimateCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestForUpdateAsync(request.ServiceRequestId, cancellationToken);
        var booking = serviceRequest.Booking
            ?? throw new AppException(ErrorCodes.NotFound, "The booking linked to this field job could not be found.", 404);
        var jobCard = _jobCardFactory.EnsureCreated(serviceRequest);
        var existingQuotation = jobCard.JobCardId > 0
            ? await _billingRepository.GetQuotationByJobCardIdAsync(jobCard.JobCardId, cancellationToken)
            : null;
        var now = _currentDateTime.UtcNow;
        var quotationHeader = await FieldWorkflowSupport.CreateOrUpdateFieldQuotationAsync(
            existingQuotation,
            serviceRequest,
            jobCard,
            booking,
            request.Lines,
            request.DiscountAmount,
            request.TaxPercentage,
            request.Remarks,
            _billingRepository,
            _billingCalculationService,
            _quotationNumberGenerator,
            _auditLogRepository,
            _currentDateTime,
            _currentUserContext,
            cancellationToken);

        await _auditLogRepository.AddAsync(
            FieldWorkflowSupport.CreateAuditLog(
                _currentUserContext,
                now,
                existingQuotation is null ? "CreateFieldEstimate" : "ResubmitFieldEstimate",
                nameof(QuotationHeader),
                quotationHeader.QuotationNumber,
                serviceRequest.ServiceRequestNumber),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BillingResponseMapper.ToQuotationDetail(quotationHeader);
    }
}

public sealed record SubmitFieldJobReportCommand(
    long ServiceRequestId,
    string EquipmentCondition,
    IReadOnlyCollection<string> IssuesIdentified,
    string ActionTaken,
    string? Recommendation,
    string? Observations,
    string? IdempotencyKey) : IRequest<FieldJobReportResponse>;

public sealed class SubmitFieldJobReportCommandValidator : AbstractValidator<SubmitFieldJobReportCommand>
{
    public SubmitFieldJobReportCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.EquipmentCondition).NotEmpty().MaximumLength(64);
        RuleFor(request => request.IssuesIdentified).NotEmpty();
        RuleForEach(request => request.IssuesIdentified).NotEmpty().MaximumLength(256);
        RuleFor(request => request.ActionTaken).NotEmpty();
        RuleFor(request => request.Recommendation).MaximumLength(4000);
        RuleFor(request => request.Observations).MaximumLength(4000);
        RuleFor(request => request.IdempotencyKey).MaximumLength(128);
    }
}

public sealed class SubmitFieldJobReportCommandHandler : IRequestHandler<SubmitFieldJobReportCommand, FieldJobReportResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFieldWorkflowRepository _fieldWorkflowRepository;
    private readonly IJobCardFactory _jobCardFactory;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitFieldJobReportCommandHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        IJobCardFactory jobCardFactory,
        IFieldWorkflowRepository fieldWorkflowRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _jobCardFactory = jobCardFactory;
        _fieldWorkflowRepository = fieldWorkflowRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<FieldJobReportResponse> Handle(SubmitFieldJobReportCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existingReport = await _fieldWorkflowRepository.GetJobReportByIdempotencyKeyAsync(
                request.IdempotencyKey.Trim(),
                asNoTracking: true,
                cancellationToken);

            if (existingReport is not null)
            {
                return FieldWorkflowSupport.MapJobReport(existingReport);
            }
        }

        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestForUpdateAsync(request.ServiceRequestId, cancellationToken);
        var technician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);
        var photos = await _fieldWorkflowRepository.GetJobPhotosAsync(request.ServiceRequestId, asNoTracking: false, cancellationToken);

        if (photos.Count < 2)
        {
            throw new AppException(
                ErrorCodes.ValidationFailure,
                "At least two job photos are required before submitting the field report.",
                400);
        }

        var signature = await _fieldWorkflowRepository.GetLatestCustomerSignatureAsync(
            request.ServiceRequestId,
            asNoTracking: false,
            cancellationToken);
        var jobCard = _jobCardFactory.EnsureCreated(serviceRequest);
        var now = _currentDateTime.UtcNow;
        var actor = FieldWorkflowSupport.ResolveActor(_currentUserContext, "FieldWorkflow");
        var report = new JobReport
        {
            ServiceRequestId = serviceRequest.ServiceRequestId,
            JobCard = jobCard,
            TechnicianId = technician.TechnicianId,
            IdempotencyKey = request.IdempotencyKey?.Trim() ?? string.Empty,
            EquipmentCondition = request.EquipmentCondition.Trim(),
            IssuesIdentifiedJson = JsonSerializer.Serialize(request.IssuesIdentified.Select(issue => issue.Trim()).ToArray()),
            ActionTaken = request.ActionTaken.Trim(),
            Recommendation = request.Recommendation?.Trim() ?? string.Empty,
            Observations = request.Observations?.Trim() ?? string.Empty,
            SubmittedAtUtc = now,
            IsQualityReviewed = false,
            QualityScore = 0.00m,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        foreach (var photo in photos.Where(photo => photo.JobReportId is null))
        {
            photo.JobReport = report;
        }

        if (signature is not null && signature.JobReportId is null)
        {
            signature.JobReport = report;
        }

        jobCard.ExecutionTimelines.Add(new JobExecutionTimeline
        {
            Status = serviceRequest.CurrentStatus,
            EventType = "JobReportSubmitted",
            EventTitle = "Field Report Submitted",
            Remarks = report.ActionTaken,
            EventDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });

        await _fieldWorkflowRepository.AddJobReportAsync(report, cancellationToken);
        await _auditLogRepository.AddAsync(
            FieldWorkflowSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "SubmitFieldJobReport",
                nameof(JobReport),
                serviceRequest.ServiceRequestNumber,
                report.ActionTaken),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FieldWorkflowSupport.MapJobReport(report);
    }
}

public sealed record UploadFieldJobPhotoCommand(
    long ServiceRequestId,
    string PhotoType,
    string FileName,
    string ContentType,
    string Base64Content,
    string? Remarks) : IRequest<FieldJobPhotoResponse>;

public sealed class UploadFieldJobPhotoCommandValidator : AbstractValidator<UploadFieldJobPhotoCommand>
{
    public UploadFieldJobPhotoCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.PhotoType)
            .NotEmpty()
            .Must(FieldWorkflowSupport.BeValidPhotoType)
            .WithMessage("Photo type is invalid.");
        RuleFor(request => request.FileName).NotEmpty().MaximumLength(256);
        RuleFor(request => request.ContentType).NotEmpty().MaximumLength(128);
        RuleFor(request => request.Base64Content).NotEmpty();
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class UploadFieldJobPhotoCommandHandler : IRequestHandler<UploadFieldJobPhotoCommand, FieldJobPhotoResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFieldWorkflowRepository _fieldWorkflowRepository;
    private readonly IJobAttachmentStorageService _jobAttachmentStorageService;
    private readonly IJobCardFactory _jobCardFactory;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public UploadFieldJobPhotoCommandHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        IJobCardFactory jobCardFactory,
        IJobAttachmentStorageService jobAttachmentStorageService,
        IFieldWorkflowRepository fieldWorkflowRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _jobCardFactory = jobCardFactory;
        _jobAttachmentStorageService = jobAttachmentStorageService;
        _fieldWorkflowRepository = fieldWorkflowRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<FieldJobPhotoResponse> Handle(UploadFieldJobPhotoCommand request, CancellationToken cancellationToken)
    {
        FieldWorkflowSupport.EnsureSupportedImageContentType(request.ContentType);
        var fileBytes = FieldWorkflowSupport.DecodeBase64Content(
            request.Base64Content,
            "The field photo content is not valid base64.");
        FieldWorkflowSupport.EnsureSupportedUploadSize(fileBytes, "Field photo size must not exceed 5 MB.");

        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestForUpdateAsync(request.ServiceRequestId, cancellationToken);
        var technician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);
        var photoType = Enum.Parse<JobPhotoType>(request.PhotoType, true);
        var jobCard = _jobCardFactory.EnsureCreated(serviceRequest);
        var latestReport = await _fieldWorkflowRepository.GetLatestJobReportAsync(request.ServiceRequestId, asNoTracking: false, cancellationToken);
        var storedFile = await _jobAttachmentStorageService.SaveAsync(request.FileName, request.ContentType, fileBytes, cancellationToken);
        var now = _currentDateTime.UtcNow;
        var actor = FieldWorkflowSupport.ResolveActor(_currentUserContext, "FieldWorkflow");

        var photo = new JobPhoto
        {
            ServiceRequestId = serviceRequest.ServiceRequestId,
            JobCard = jobCard,
            TechnicianId = technician.TechnicianId,
            JobReportId = latestReport?.JobReportId,
            PhotoType = photoType,
            FileName = request.FileName.Trim(),
            ContentType = request.ContentType.Trim(),
            StorageUrl = storedFile.RelativePath,
            UploadedBy = actor,
            UploadedAtUtc = now,
            PhotoRemarks = request.Remarks?.Trim() ?? string.Empty,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        jobCard.ExecutionTimelines.Add(new JobExecutionTimeline
        {
            Status = serviceRequest.CurrentStatus,
            EventType = "FieldPhotoUploaded",
            EventTitle = photoType.ToString(),
            Remarks = string.IsNullOrWhiteSpace(photo.PhotoRemarks) ? photo.FileName : photo.PhotoRemarks,
            EventDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });

        await _fieldWorkflowRepository.AddJobPhotoAsync(photo, cancellationToken);
        await _auditLogRepository.AddAsync(
            FieldWorkflowSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "UploadFieldJobPhoto",
                nameof(JobPhoto),
                serviceRequest.ServiceRequestNumber,
                photoType.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FieldWorkflowSupport.MapJobPhoto(photo);
    }
}

public sealed record SaveFieldJobSignatureCommand(
    long ServiceRequestId,
    string CustomerName,
    string SignatureBase64,
    string? Remarks) : IRequest<FieldCustomerSignatureResponse>;

public sealed class SaveFieldJobSignatureCommandValidator : AbstractValidator<SaveFieldJobSignatureCommand>
{
    public SaveFieldJobSignatureCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.CustomerName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.SignatureBase64).NotEmpty();
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class SaveFieldJobSignatureCommandHandler : IRequestHandler<SaveFieldJobSignatureCommand, FieldCustomerSignatureResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFieldWorkflowRepository _fieldWorkflowRepository;
    private readonly IJobCardFactory _jobCardFactory;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public SaveFieldJobSignatureCommandHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        IJobCardFactory jobCardFactory,
        IFieldWorkflowRepository fieldWorkflowRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _jobCardFactory = jobCardFactory;
        _fieldWorkflowRepository = fieldWorkflowRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<FieldCustomerSignatureResponse> Handle(SaveFieldJobSignatureCommand request, CancellationToken cancellationToken)
    {
        FieldWorkflowSupport.DecodeBase64Content(
            request.SignatureBase64,
            "The customer signature content is not valid base64.");

        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestForUpdateAsync(request.ServiceRequestId, cancellationToken);
        var technician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);
        var jobCard = _jobCardFactory.EnsureCreated(serviceRequest);
        var latestReport = await _fieldWorkflowRepository.GetLatestJobReportAsync(request.ServiceRequestId, asNoTracking: false, cancellationToken);
        var now = _currentDateTime.UtcNow;
        var actor = FieldWorkflowSupport.ResolveActor(_currentUserContext, "FieldWorkflow");

        var signature = new CustomerSignature
        {
            ServiceRequestId = serviceRequest.ServiceRequestId,
            JobCard = jobCard,
            TechnicianId = technician.TechnicianId,
            JobReportId = latestReport?.JobReportId,
            CustomerName = request.CustomerName.Trim(),
            SignatureDataUrl = FieldWorkflowSupport.NormalizeSignatureDataUrl(request.SignatureBase64),
            SignedAtUtc = now,
            CapturedBy = actor,
            SignatureRemarks = request.Remarks?.Trim() ?? string.Empty,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        jobCard.ExecutionTimelines.Add(new JobExecutionTimeline
        {
            Status = serviceRequest.CurrentStatus,
            EventType = "CustomerSignatureCaptured",
            EventTitle = "Customer Signature Captured",
            Remarks = signature.CustomerName,
            EventDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });

        await _fieldWorkflowRepository.AddCustomerSignatureAsync(signature, cancellationToken);
        await _auditLogRepository.AddAsync(
            FieldWorkflowSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "SaveFieldJobSignature",
                nameof(CustomerSignature),
                serviceRequest.ServiceRequestNumber,
                signature.CustomerName),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FieldWorkflowSupport.MapCustomerSignature(signature);
    }
}

public sealed record CollectFieldJobPaymentCommand(
    long ServiceRequestId,
    decimal PaidAmount,
    string PaymentMethod,
    string? ReferenceNumber,
    string? Remarks,
    string? IdempotencyKey,
    string? GatewayTransactionId,
    string? Signature,
    decimal? ExpectedInvoiceAmount) : IRequest<PaymentTransactionResponse>;

public sealed class CollectFieldJobPaymentCommandValidator : AbstractValidator<CollectFieldJobPaymentCommand>
{
    public CollectFieldJobPaymentCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.PaidAmount).GreaterThan(0.00m);
        RuleFor(request => request.PaymentMethod)
            .NotEmpty()
            .Must(FieldWorkflowSupport.BeValidPaymentMethod)
            .WithMessage("Payment method is invalid.");
        RuleFor(request => request.ReferenceNumber).MaximumLength(128);
        RuleFor(request => request.Remarks).MaximumLength(512);
        RuleFor(request => request.IdempotencyKey).MaximumLength(128);
        RuleFor(request => request.GatewayTransactionId).MaximumLength(128);
        RuleFor(request => request.Signature).MaximumLength(256);
        RuleFor(request => request.ExpectedInvoiceAmount).GreaterThan(0.00m).When(request => request.ExpectedInvoiceAmount.HasValue);
    }
}

public sealed class CollectFieldJobPaymentCommandHandler : IRequestHandler<CollectFieldJobPaymentCommand, PaymentTransactionResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBillingCalculationService _billingCalculationService;
    private readonly IBillingRepository _billingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GapPhaseAValidationService _gapPhaseAValidationService;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
    private readonly IJobCardFactory _jobCardFactory;
    private readonly IQuotationNumberGenerator _quotationNumberGenerator;
    private readonly IReceiptNumberGenerator _receiptNumberGenerator;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public CollectFieldJobPaymentCommandHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        IJobCardFactory jobCardFactory,
        IBillingRepository billingRepository,
        IBillingCalculationService billingCalculationService,
        IQuotationNumberGenerator quotationNumberGenerator,
        IInvoiceNumberGenerator invoiceNumberGenerator,
        IReceiptNumberGenerator receiptNumberGenerator,
        GapPhaseAValidationService gapPhaseAValidationService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _jobCardFactory = jobCardFactory;
        _billingRepository = billingRepository;
        _billingCalculationService = billingCalculationService;
        _quotationNumberGenerator = quotationNumberGenerator;
        _invoiceNumberGenerator = invoiceNumberGenerator;
        _receiptNumberGenerator = receiptNumberGenerator;
        _gapPhaseAValidationService = gapPhaseAValidationService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<PaymentTransactionResponse> Handle(CollectFieldJobPaymentCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existingPayment = await _billingRepository.GetPaymentByIdempotencyKeyAsync(request.IdempotencyKey.Trim(), cancellationToken);

            if (existingPayment is not null)
            {
                return BillingResponseMapper.ToPaymentTransactionResponse(existingPayment);
            }
        }

        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestForUpdateAsync(request.ServiceRequestId, cancellationToken);
        _jobCardFactory.EnsureCreated(serviceRequest);
        var invoiceHeader = await FieldWorkflowSupport.EnsureInvoiceReadyAsync(
            serviceRequest,
            _billingRepository,
            _billingCalculationService,
            _quotationNumberGenerator,
            _invoiceNumberGenerator,
            _auditLogRepository,
            _currentDateTime,
            _currentUserContext,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.ReferenceNumber) &&
            await _billingRepository.PaymentReferenceExistsAsync(invoiceHeader.InvoiceHeaderId, request.ReferenceNumber.Trim(), cancellationToken))
        {
            throw new AppException(ErrorCodes.DuplicatePaymentDetected, "The payment reference has already been processed.", 409);
        }

        if (!string.IsNullOrWhiteSpace(request.GatewayTransactionId) &&
            await _billingRepository.GatewayTransactionExistsAsync(request.GatewayTransactionId.Trim(), cancellationToken))
        {
            throw new AppException(ErrorCodes.DuplicatePaymentDetected, "The gateway transaction has already been processed.", 409);
        }

        if (request.ExpectedInvoiceAmount.HasValue)
        {
            _gapPhaseAValidationService.ValidatePaymentAmount(request.ExpectedInvoiceAmount.Value, invoiceHeader.GrandTotalAmount);
        }

        if (request.PaidAmount > invoiceHeader.BalanceAmount)
        {
            throw new AppException(ErrorCodes.PaymentAmountExceedsBalance, "The payment amount exceeds the outstanding balance.", 409);
        }

        var now = _currentDateTime.UtcNow;
        var actor = FieldWorkflowSupport.ResolveActor(_currentUserContext, "FieldWorkflow");
        var nextPaidAmount = invoiceHeader.PaidAmount + request.PaidAmount;
        var nextStatus = _billingCalculationService.ResolvePaymentStatus(invoiceHeader.GrandTotalAmount, nextPaidAmount);
        var nextBalanceAmount = _billingCalculationService.CalculateBalanceAmount(invoiceHeader.GrandTotalAmount, nextPaidAmount);
        var paymentMethod = Enum.Parse<PaymentMethod>(request.PaymentMethod, true);
        var paymentTransaction = new PaymentTransaction
        {
            InvoiceHeader = invoiceHeader,
            PaymentMethod = paymentMethod,
            ReferenceNumber = request.ReferenceNumber?.Trim() ?? string.Empty,
            IdempotencyKey = request.IdempotencyKey?.Trim() ?? string.Empty,
            GatewayTransactionId = request.GatewayTransactionId?.Trim() ?? string.Empty,
            GatewaySignature = request.Signature?.Trim() ?? string.Empty,
            PaidAmount = request.PaidAmount,
            PaymentDateUtc = now,
            TransactionRemarks = request.Remarks?.Trim() ?? string.Empty,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        paymentTransaction.PaymentReceipt = new PaymentReceipt
        {
            InvoiceHeader = invoiceHeader,
            ReceiptNumber = await FieldWorkflowSupport.GenerateUniqueReceiptNumberAsync(_receiptNumberGenerator, _billingRepository, cancellationToken),
            ReceiptDateUtc = now,
            ReceivedAmount = request.PaidAmount,
            BalanceAmount = nextBalanceAmount,
            ReceiptRemarks = request.Remarks?.Trim() ?? string.Empty,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        invoiceHeader.PaidAmount = nextPaidAmount;
        invoiceHeader.BalanceAmount = nextBalanceAmount;
        invoiceHeader.CurrentStatus = nextStatus;
        invoiceHeader.LastPaymentDateUtc = now;
        invoiceHeader.UpdatedBy = actor;
        invoiceHeader.LastUpdated = now;
        invoiceHeader.PaymentTransactions.Add(paymentTransaction);
        invoiceHeader.BillingStatusHistories.Add(new BillingStatusHistory
        {
            QuotationHeader = invoiceHeader.QuotationHeader,
            InvoiceHeader = invoiceHeader,
            EntityType = BillingEntityType.Invoice,
            StatusName = nextStatus.ToString(),
            Remarks = "Invoice payment status updated after field payment collection.",
            StatusDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });
        paymentTransaction.BillingStatusHistories.Add(new BillingStatusHistory
        {
            QuotationHeader = invoiceHeader.QuotationHeader,
            InvoiceHeader = invoiceHeader,
            PaymentTransaction = paymentTransaction,
            EntityType = BillingEntityType.Payment,
            StatusName = "Collected",
            Remarks = string.IsNullOrWhiteSpace(paymentTransaction.ReferenceNumber)
                ? $"Field payment collected via {paymentMethod}."
                : $"Field payment collected via {paymentMethod} with reference {paymentTransaction.ReferenceNumber}.",
            StatusDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });

        serviceRequest.JobCard?.ExecutionTimelines.Add(new JobExecutionTimeline
        {
            Status = serviceRequest.CurrentStatus,
            EventType = "PaymentCollected",
            EventTitle = "Field Payment Recorded",
            Remarks = $"{request.PaidAmount:0.00} collected via {paymentMethod}.",
            EventDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });

        await _billingRepository.AddPaymentTransactionAsync(paymentTransaction, cancellationToken);
        await _auditLogRepository.AddAsync(
            FieldWorkflowSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "CollectFieldJobPayment",
                nameof(PaymentTransaction),
                invoiceHeader.InvoiceNumber,
                $"{request.PaidAmount:0.00}:{paymentMethod}"),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BillingResponseMapper.ToPaymentTransactionResponse(paymentTransaction);
    }
}

public sealed record CompleteFieldJobCommand(
    long ServiceRequestId,
    string? Remarks) : IRequest<FieldJobDetailResponse>;

public sealed class CompleteFieldJobCommandValidator : AbstractValidator<CompleteFieldJobCommand>
{
    public CompleteFieldJobCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

public sealed class CompleteFieldJobCommandHandler : IRequestHandler<CompleteFieldJobCommand, FieldJobDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBillingCalculationService _billingCalculationService;
    private readonly IBillingRepository _billingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFieldLookupRepository _fieldLookupRepository;
    private readonly IFieldWorkflowRepository _fieldWorkflowRepository;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
    private readonly ITechnicianFieldExecutionService _fieldExecutionService;
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly TechnicianJobLifecycleResolver _technicianJobLifecycleResolver;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;
    private readonly IQuotationNumberGenerator _quotationNumberGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteFieldJobCommandHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        ITechnicianFieldExecutionService fieldExecutionService,
        IFieldWorkflowRepository fieldWorkflowRepository,
        IFieldLookupRepository fieldLookupRepository,
        ISupportTicketRepository supportTicketRepository,
        TechnicianJobLifecycleResolver technicianJobLifecycleResolver,
        IBillingRepository billingRepository,
        IBillingCalculationService billingCalculationService,
        IQuotationNumberGenerator quotationNumberGenerator,
        IInvoiceNumberGenerator invoiceNumberGenerator,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _fieldExecutionService = fieldExecutionService;
        _fieldWorkflowRepository = fieldWorkflowRepository;
        _fieldLookupRepository = fieldLookupRepository;
        _supportTicketRepository = supportTicketRepository;
        _technicianJobLifecycleResolver = technicianJobLifecycleResolver;
        _billingRepository = billingRepository;
        _billingCalculationService = billingCalculationService;
        _quotationNumberGenerator = quotationNumberGenerator;
        _invoiceNumberGenerator = invoiceNumberGenerator;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<FieldJobDetailResponse> Handle(CompleteFieldJobCommand request, CancellationToken cancellationToken)
    {
        var artifacts = await FieldWorkflowSupport.EnsureCompletionArtifactsAsync(
            request.ServiceRequestId,
            _fieldWorkflowRepository,
            cancellationToken);
        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestForUpdateAsync(request.ServiceRequestId, cancellationToken);
        var workSummary = !string.IsNullOrWhiteSpace(artifacts.Report.ActionTaken)
            ? artifacts.Report.ActionTaken
            : request.Remarks;

        if (serviceRequest.CurrentStatus == ServiceRequestStatus.WorkInProgress)
        {
            await _fieldExecutionService.AdvanceStatusAsync(
                request.ServiceRequestId,
                ServiceRequestStatus.WorkCompletedPendingSubmission,
                request.Remarks,
                workSummary,
                auditActionName: "MarkFieldWorkCompleted",
                cancellationToken);

            serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestForUpdateAsync(request.ServiceRequestId, cancellationToken);
        }

        if (serviceRequest.CurrentStatus != ServiceRequestStatus.WorkCompletedPendingSubmission &&
            serviceRequest.CurrentStatus != ServiceRequestStatus.SubmittedForClosure)
        {
            throw new AppException(
                ErrorCodes.InvalidStatusTransition,
                "The field job cannot be completed from the current status.",
                409);
        }

        await FieldWorkflowSupport.EnsureInvoiceReadyAsync(
            serviceRequest,
            _billingRepository,
            _billingCalculationService,
            _quotationNumberGenerator,
            _invoiceNumberGenerator,
            _auditLogRepository,
            _currentDateTime,
            _currentUserContext,
            cancellationToken);

        if (serviceRequest.CurrentStatus == ServiceRequestStatus.WorkCompletedPendingSubmission)
        {
            await _fieldExecutionService.AdvanceStatusAsync(
                request.ServiceRequestId,
                ServiceRequestStatus.SubmittedForClosure,
                request.Remarks,
                workSummary,
                auditActionName: "CompleteFieldJob",
                cancellationToken);
        }
        else
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return await FieldWorkflowSupport.BuildFieldJobDetailAsync(
            request.ServiceRequestId,
            _technicianJobAccessService,
            _fieldLookupRepository,
            _supportTicketRepository,
            _technicianJobLifecycleResolver,
            _fieldWorkflowRepository,
            _billingRepository,
            cancellationToken);
    }
}

public sealed record CheckInFieldAttendanceCommand(
    string? LocationText,
    double? Latitude,
    double? Longitude) : IRequest<TechnicianAttendanceResponse>;

public sealed class CheckInFieldAttendanceCommandValidator : AbstractValidator<CheckInFieldAttendanceCommand>
{
    public CheckInFieldAttendanceCommandValidator()
    {
        RuleFor(request => request.LocationText).MaximumLength(256);
    }
}

public sealed class CheckInFieldAttendanceCommandHandler : IRequestHandler<CheckInFieldAttendanceCommand, TechnicianAttendanceResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CheckInFieldAttendanceCommandHandler(
        ITechnicianRepository technicianRepository,
        ITechnicianJobAccessService technicianJobAccessService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianRepository = technicianRepository;
        _technicianJobAccessService = technicianJobAccessService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<TechnicianAttendanceResponse> Handle(CheckInFieldAttendanceCommand request, CancellationToken cancellationToken)
    {
        var technician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);
        var now = _currentDateTime.UtcNow;
        var attendanceDate = DateOnly.FromDateTime(now);
        var existingAttendance = await _technicianRepository.GetAttendanceByDateAsync(
            technician.TechnicianId,
            attendanceDate,
            asNoTracking: false,
            cancellationToken);

        if (existingAttendance is not null &&
            existingAttendance.CheckInOnUtc.HasValue &&
            !existingAttendance.CheckOutOnUtc.HasValue)
        {
            throw new AppException(ErrorCodes.Conflict, "The technician is already checked in for today.", 409);
        }

        if (existingAttendance is not null && existingAttendance.CheckOutOnUtc.HasValue)
        {
            throw new AppException(ErrorCodes.Conflict, "The technician has already completed attendance for today.", 409);
        }

        if (existingAttendance is not null &&
            existingAttendance.AttendanceStatus.StartsWith("Leave", StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException(ErrorCodes.Conflict, "The technician cannot check in while a leave attendance record is active.", 409);
        }

        var actor = FieldWorkflowSupport.ResolveActor(_currentUserContext, "FieldWorkflow");
        var attendance = existingAttendance ?? new TechnicianAttendance
        {
            TechnicianId = technician.TechnicianId,
            AttendanceDate = attendanceDate,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        };

        attendance.CheckInOnUtc = now;
        attendance.CheckOutOnUtc = null;
        attendance.AttendanceStatus = "CheckedIn";
        attendance.LocationText = request.LocationText?.Trim() ?? string.Empty;
        attendance.LeaveReason = string.Empty;
        attendance.UpdatedBy = actor;
        attendance.LastUpdated = now;
        attendance.IPAddress = _currentUserContext.IPAddress;

        if (existingAttendance is null)
        {
            await _technicianRepository.AddAttendanceAsync(attendance, cancellationToken);
        }

        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            await _technicianRepository.AddGpsLogAsync(
                new TechnicianGpsLog
                {
                    TechnicianId = technician.TechnicianId,
                    TrackedOnUtc = now,
                    Latitude = (decimal)request.Latitude.Value,
                    Longitude = (decimal)request.Longitude.Value,
                    TrackingSource = "AttendanceCheckIn",
                    LocationText = attendance.LocationText,
                    CreatedBy = actor,
                    DateCreated = now,
                    IPAddress = _currentUserContext.IPAddress
                },
                cancellationToken);
        }

        await _auditLogRepository.AddAsync(
            FieldWorkflowSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "CheckInFieldAttendance",
                nameof(TechnicianAttendance),
                technician.TechnicianCode,
                attendanceDate.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FieldWorkflowSupport.MapAttendance(attendance);
    }
}

public sealed record CheckOutFieldAttendanceCommand(
    string? LocationText,
    double? Latitude,
    double? Longitude) : IRequest<TechnicianAttendanceResponse>;

public sealed class CheckOutFieldAttendanceCommandValidator : AbstractValidator<CheckOutFieldAttendanceCommand>
{
    public CheckOutFieldAttendanceCommandValidator()
    {
        RuleFor(request => request.LocationText).MaximumLength(256);
    }
}

public sealed class CheckOutFieldAttendanceCommandHandler : IRequestHandler<CheckOutFieldAttendanceCommand, TechnicianAttendanceResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CheckOutFieldAttendanceCommandHandler(
        ITechnicianRepository technicianRepository,
        ITechnicianJobAccessService technicianJobAccessService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianRepository = technicianRepository;
        _technicianJobAccessService = technicianJobAccessService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<TechnicianAttendanceResponse> Handle(CheckOutFieldAttendanceCommand request, CancellationToken cancellationToken)
    {
        var technician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);
        var now = _currentDateTime.UtcNow;
        var attendanceDate = DateOnly.FromDateTime(now);
        var attendance = await _technicianRepository.GetAttendanceByDateAsync(
            technician.TechnicianId,
            attendanceDate,
            asNoTracking: false,
            cancellationToken)
            ?? throw new AppException(ErrorCodes.Conflict, "The technician cannot check out before checking in.", 409);

        if (!attendance.CheckInOnUtc.HasValue)
        {
            throw new AppException(ErrorCodes.Conflict, "The technician cannot check out before checking in.", 409);
        }

        if (attendance.CheckOutOnUtc.HasValue)
        {
            throw new AppException(ErrorCodes.Conflict, "The technician is already checked out for today.", 409);
        }

        var actor = FieldWorkflowSupport.ResolveActor(_currentUserContext, "FieldWorkflow");
        attendance.CheckOutOnUtc = now;
        attendance.AttendanceStatus = "CheckedOut";
        attendance.LocationText = request.LocationText?.Trim() ?? attendance.LocationText;
        attendance.UpdatedBy = actor;
        attendance.LastUpdated = now;
        attendance.IPAddress = _currentUserContext.IPAddress;

        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            await _technicianRepository.AddGpsLogAsync(
                new TechnicianGpsLog
                {
                    TechnicianId = technician.TechnicianId,
                    TrackedOnUtc = now,
                    Latitude = (decimal)request.Latitude.Value,
                    Longitude = (decimal)request.Longitude.Value,
                    TrackingSource = "AttendanceCheckOut",
                    LocationText = attendance.LocationText,
                    CreatedBy = actor,
                    DateCreated = now,
                    IPAddress = _currentUserContext.IPAddress
                },
                cancellationToken);
        }

        await _auditLogRepository.AddAsync(
            FieldWorkflowSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "CheckOutFieldAttendance",
                nameof(TechnicianAttendance),
                technician.TechnicianCode,
                attendanceDate.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FieldWorkflowSupport.MapAttendance(attendance);
    }
}

internal static class FieldWorkflowSupport
{
    private static readonly HashSet<string> AllowedImageContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private const long MaxUploadBytes = 5 * 1024 * 1024;

    public static async Task<FieldJobDetailResponse> BuildFieldJobDetailAsync(
        long serviceRequestId,
        ITechnicianJobAccessService technicianJobAccessService,
        IFieldLookupRepository fieldLookupRepository,
        ISupportTicketRepository supportTicketRepository,
        TechnicianJobLifecycleResolver technicianJobLifecycleResolver,
        IFieldWorkflowRepository fieldWorkflowRepository,
        IBillingRepository billingRepository,
        CancellationToken cancellationToken)
    {
        var serviceRequest = await technicianJobAccessService.GetOwnedServiceRequestAsync(serviceRequestId, cancellationToken);
        var serviceId = serviceRequest.Booking?.BookingLines
            .OrderBy(line => line.BookingLineId)
            .Select(line => line.ServiceId)
            .FirstOrDefault() ?? 0;
        var checklistMasters = serviceId > 0
            ? await fieldLookupRepository.GetChecklistByServiceIdAsync(serviceId, cancellationToken)
            : Array.Empty<ServiceChecklistMaster>();
        var (lifecycleType, lifecycleLabel) = await technicianJobLifecycleResolver.ResolveAsync(serviceRequestId, cancellationToken);
        var supportJobAlert = await supportTicketRepository.GetJobAlertAsync(
            serviceRequest.ServiceRequestId,
            serviceRequest.BookingId,
            serviceRequest.JobCard?.JobCardId,
            cancellationToken);
        var latestReport = await fieldWorkflowRepository.GetLatestJobReportAsync(serviceRequestId, asNoTracking: true, cancellationToken);
        var photos = await fieldWorkflowRepository.GetJobPhotosAsync(serviceRequestId, asNoTracking: true, cancellationToken);
        var signature = await fieldWorkflowRepository.GetLatestCustomerSignatureAsync(serviceRequestId, asNoTracking: true, cancellationToken);
        var partsRequests = await fieldWorkflowRepository.GetPartsRequestsAsync(serviceRequestId, asNoTracking: true, cancellationToken);

        QuotationDetailResponse? quotation = null;
        InvoiceDetailResponse? invoice = null;
        IReadOnlyCollection<PaymentTransactionResponse> payments = Array.Empty<PaymentTransactionResponse>();

        if (serviceRequest.JobCard?.JobCardId > 0)
        {
            var quotationHeader = await billingRepository.GetQuotationByJobCardIdAsync(serviceRequest.JobCard.JobCardId, cancellationToken);

            if (quotationHeader is not null)
            {
                quotation = BillingResponseMapper.ToQuotationDetail(quotationHeader);
                var invoiceHeader = await billingRepository.GetInvoiceByQuotationIdAsync(quotationHeader.QuotationHeaderId, cancellationToken);

                if (invoiceHeader is not null)
                {
                    invoice = BillingResponseMapper.ToInvoiceDetail(invoiceHeader);
                    payments = (await billingRepository.GetPaymentTransactionsByInvoiceIdAsync(invoiceHeader.InvoiceHeaderId, cancellationToken))
                        .Select(BillingResponseMapper.ToPaymentTransactionResponse)
                        .ToArray();
                }
            }
        }

        return new FieldJobDetailResponse(
            TechnicianJobResponseMapper.ToDetail(serviceRequest, checklistMasters, lifecycleType, lifecycleLabel, supportJobAlert),
            serviceRequest.Booking?.CustomerAddress?.Latitude,
            serviceRequest.Booking?.CustomerAddress?.Longitude,
            latestReport is null ? null : MapJobReport(latestReport),
            photos.Select(MapJobPhoto).ToArray(),
            signature is null ? null : MapCustomerSignature(signature),
            partsRequests.Select(MapPartsRequest).ToArray(),
            quotation,
            invoice,
            payments);
    }

    public static async Task<(JobReport Report, IReadOnlyCollection<JobPhoto> Photos, CustomerSignature Signature)> EnsureCompletionArtifactsAsync(
        long serviceRequestId,
        IFieldWorkflowRepository fieldWorkflowRepository,
        CancellationToken cancellationToken)
    {
        var report = await fieldWorkflowRepository.GetLatestJobReportAsync(serviceRequestId, asNoTracking: true, cancellationToken)
            ?? throw new AppException(
                ErrorCodes.ValidationFailure,
                "A field job report is required before completion.",
                400);
        var photos = await fieldWorkflowRepository.GetJobPhotosAsync(serviceRequestId, asNoTracking: true, cancellationToken);

        if (photos.Count < 2)
        {
            throw new AppException(
                ErrorCodes.ValidationFailure,
                "At least two field job photos are required before completion.",
                400);
        }

        var signature = await fieldWorkflowRepository.GetLatestCustomerSignatureAsync(serviceRequestId, asNoTracking: true, cancellationToken)
            ?? throw new AppException(
                ErrorCodes.ValidationFailure,
                "A customer signature is required before completion.",
                400);

        return (report, photos, signature);
    }

    public static async Task<InvoiceHeader> EnsureInvoiceReadyAsync(
        ServiceRequestEntity serviceRequest,
        IBillingRepository billingRepository,
        IBillingCalculationService billingCalculationService,
        IQuotationNumberGenerator quotationNumberGenerator,
        IInvoiceNumberGenerator invoiceNumberGenerator,
        IAuditLogRepository auditLogRepository,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        CancellationToken cancellationToken)
    {
        var booking = serviceRequest.Booking
            ?? throw new AppException(ErrorCodes.NotFound, "The booking linked to this field job could not be found.", 404);
        var jobCard = serviceRequest.JobCard
            ?? throw new AppException(ErrorCodes.NotFound, "A job card could not be resolved for this field job.", 404);
        QuotationHeader? trackedQuotation = null;

        if (jobCard.JobCardId > 0)
        {
            var quotationSnapshot = await billingRepository.GetQuotationByJobCardIdAsync(jobCard.JobCardId, cancellationToken);

            if (quotationSnapshot is not null)
            {
                var existingInvoice = await billingRepository.GetInvoiceByQuotationIdAsync(quotationSnapshot.QuotationHeaderId, cancellationToken);

                if (existingInvoice is not null)
                {
                    return await billingRepository.GetInvoiceByIdForUpdateAsync(existingInvoice.InvoiceHeaderId, cancellationToken)
                        ?? throw new AppException(ErrorCodes.NotFound, "The generated invoice could not be reloaded.", 404);
                }

                trackedQuotation = await billingRepository.GetQuotationByIdForUpdateAsync(quotationSnapshot.QuotationHeaderId, cancellationToken)
                    ?? throw new AppException(ErrorCodes.NotFound, "The field quotation could not be loaded for invoice generation.", 404);
            }
        }

        trackedQuotation ??= await CreateAutoApprovedQuotationAsync(
            serviceRequest,
            jobCard,
            booking,
            billingRepository,
            billingCalculationService,
            quotationNumberGenerator,
            auditLogRepository,
            currentDateTime,
            currentUserContext,
            cancellationToken);

        if (trackedQuotation.CurrentStatus == QuotationStatus.PendingCustomerApproval)
        {
            throw new AppException(
                ErrorCodes.QuotationApprovalRequired,
                "The field estimate must be approved before invoice generation.",
                409);
        }

        if (trackedQuotation.CurrentStatus == QuotationStatus.Rejected)
        {
            throw new AppException(
                ErrorCodes.Conflict,
                "The field estimate was rejected and must be resubmitted before completion.",
                409);
        }

        return await CreateInvoiceFromQuotationAsync(
            trackedQuotation,
            billingRepository,
            billingCalculationService,
            invoiceNumberGenerator,
            auditLogRepository,
            currentDateTime,
            currentUserContext,
            cancellationToken);
    }

    public static async Task<QuotationHeader> CreateOrUpdateFieldQuotationAsync(
        QuotationHeader? existingQuotation,
        ServiceRequestEntity serviceRequest,
        JobCard jobCard,
        BookingEntity booking,
        IReadOnlyCollection<QuotationLineRequest> lines,
        decimal discountAmount,
        decimal taxPercentage,
        string? remarks,
        IBillingRepository billingRepository,
        IBillingCalculationService billingCalculationService,
        IQuotationNumberGenerator quotationNumberGenerator,
        IAuditLogRepository auditLogRepository,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        CancellationToken cancellationToken)
    {
        var calculation = billingCalculationService.CalculateInvoice(
            lines.Select(line => new BillingLineCalculationInput(line.Quantity, line.UnitPrice)).ToArray(),
            discountAmount,
            taxPercentage,
            0.00m);
        var quotationHeader = existingQuotation is null
            ? await CreateNewFieldQuotationAsync(
                serviceRequest,
                jobCard,
                booking,
                calculation,
                remarks,
                billingRepository,
                quotationNumberGenerator,
                auditLogRepository,
                currentDateTime,
                currentUserContext,
                cancellationToken)
            : await ReuseRejectedFieldQuotationAsync(
                existingQuotation,
                calculation,
                remarks,
                billingRepository,
                currentDateTime,
                currentUserContext,
                cancellationToken);
        var now = currentDateTime.UtcNow;
        var actor = ResolveActor(currentUserContext, "FieldWorkflow");

        foreach (var line in quotationHeader.Lines.Where(line => !line.IsDeleted))
        {
            line.IsDeleted = true;
            line.DeletedBy = actor;
            line.DateDeleted = now;
        }

        foreach (var line in lines)
        {
            quotationHeader.Lines.Add(new QuotationLine
            {
                LineType = Enum.Parse<QuotationLineType>(line.LineType, true),
                LineDescription = line.LineDescription.Trim(),
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineAmount = billingCalculationService.CalculateLineAmount(line.Quantity, line.UnitPrice),
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = currentUserContext.IPAddress
            });
        }

        quotationHeader.BillingStatusHistories.Add(new BillingStatusHistory
        {
            EntityType = BillingEntityType.Quotation,
            StatusName = quotationHeader.CurrentStatus.ToString(),
            Remarks = string.IsNullOrWhiteSpace(remarks)
                ? "Field estimate submitted for customer approval."
                : remarks.Trim(),
            StatusDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = currentUserContext.IPAddress
        });

        return quotationHeader;
    }

    public static async Task<string> GenerateUniqueReceiptNumberAsync(
        IReceiptNumberGenerator receiptNumberGenerator,
        IBillingRepository billingRepository,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            var receiptNumber = receiptNumberGenerator.GenerateNumber();

            if (!await billingRepository.ReceiptNumberExistsAsync(receiptNumber, cancellationToken))
            {
                return receiptNumber;
            }
        }
    }

    public static TechnicianAttendanceResponse MapAttendance(TechnicianAttendance attendance)
    {
        return new TechnicianAttendanceResponse(
            attendance.TechnicianAttendanceId,
            attendance.AttendanceDate,
            attendance.AttendanceStatus,
            attendance.CheckInOnUtc,
            attendance.CheckOutOnUtc,
            attendance.LocationText,
            attendance.LeaveReason,
            attendance.ReviewedByUserId,
            attendance.ReviewedOnUtc);
    }

    public static FieldJobReportResponse MapJobReport(JobReport jobReport)
    {
        return new FieldJobReportResponse(
            jobReport.JobReportId,
            jobReport.ServiceRequestId,
            jobReport.JobCardId,
            jobReport.TechnicianId,
            DeserializeIssues(jobReport.IssuesIdentifiedJson),
            jobReport.EquipmentCondition,
            jobReport.ActionTaken,
            jobReport.Recommendation,
            jobReport.Observations,
            jobReport.SubmittedAtUtc,
            jobReport.IsQualityReviewed,
            jobReport.QualityScore);
    }

    public static FieldJobPhotoResponse MapJobPhoto(JobPhoto jobPhoto)
    {
        return new FieldJobPhotoResponse(
            jobPhoto.JobPhotoId,
            jobPhoto.ServiceRequestId,
            jobPhoto.JobCardId,
            jobPhoto.PhotoType.ToString(),
            jobPhoto.FileName,
            jobPhoto.ContentType,
            jobPhoto.StorageUrl,
            jobPhoto.UploadedBy,
            jobPhoto.UploadedAtUtc,
            jobPhoto.PhotoRemarks);
    }

    public static FieldCustomerSignatureResponse MapCustomerSignature(CustomerSignature signature)
    {
        return new FieldCustomerSignatureResponse(
            signature.CustomerSignatureId,
            signature.ServiceRequestId,
            signature.JobCardId,
            signature.CustomerName,
            signature.SignatureDataUrl,
            signature.SignedAtUtc,
            signature.CapturedBy,
            signature.SignatureRemarks);
    }

    public static FieldPartsRequestResponse MapPartsRequest(PartsRequest partsRequest)
    {
        return new FieldPartsRequestResponse(
            partsRequest.PartsRequestId,
            partsRequest.ServiceRequestId,
            partsRequest.JobCardId,
            partsRequest.TechnicianId,
            partsRequest.Urgency.ToString(),
            partsRequest.CurrentStatus.ToString(),
            partsRequest.Notes,
            partsRequest.SubmittedAtUtc,
            partsRequest.ProcessedAtUtc,
            partsRequest.Items
                .Where(item => !item.IsDeleted)
                .OrderBy(item => item.PartsRequestItemId)
                .Select(item => new FieldPartsRequestItemResponse(
                    item.PartsRequestItemId,
                    item.PartCode,
                    item.PartName,
                    item.QuantityRequested,
                    item.QuantityApproved,
                    item.CurrentStatus.ToString(),
                    item.ItemRemarks))
                .ToArray());
    }

    public static AuditLog CreateAuditLog(
        ICurrentUserContext currentUserContext,
        DateTime now,
        string actionName,
        string entityName,
        string entityId,
        string newValues)
    {
        return new AuditLog
        {
            UserId = currentUserContext.UserId,
            ActionName = actionName,
            EntityName = entityName,
            EntityId = entityId,
            TraceId = currentUserContext.TraceId,
            StatusName = "Success",
            NewValues = newValues,
            CreatedBy = ResolveActor(currentUserContext, actionName),
            DateCreated = now,
            IPAddress = currentUserContext.IPAddress
        };
    }

    public static string ResolveActor(ICurrentUserContext currentUserContext, string fallback)
    {
        return string.IsNullOrWhiteSpace(currentUserContext.UserName) ? fallback : currentUserContext.UserName;
    }

    public static bool BeValidPartsRequestUrgency(string urgency)
    {
        return Enum.TryParse<PartsRequestUrgency>(urgency, true, out _);
    }

    public static bool BeValidPhotoType(string photoType)
    {
        return Enum.TryParse<JobPhotoType>(photoType, true, out _);
    }

    public static bool BeValidPaymentMethod(string paymentMethod)
    {
        return Enum.TryParse<PaymentMethod>(paymentMethod, true, out _);
    }

    public static void EnsureSupportedImageContentType(string contentType)
    {
        if (!AllowedImageContentTypes.Contains(contentType))
        {
            throw new AppException(
                ErrorCodes.InvalidAttachmentContent,
                "Only JPEG, PNG, and WEBP field images are supported.",
                400);
        }
    }

    public static byte[] DecodeBase64Content(string base64Content, string invalidMessage)
    {
        try
        {
            return Convert.FromBase64String(NormalizeBase64Payload(base64Content));
        }
        catch (FormatException)
        {
            throw new AppException(ErrorCodes.InvalidAttachmentContent, invalidMessage, 400);
        }
    }

    public static void EnsureSupportedUploadSize(byte[] fileBytes, string sizeMessage)
    {
        if (fileBytes.LongLength > MaxUploadBytes)
        {
            throw new AppException(ErrorCodes.AttachmentTooLarge, sizeMessage, 409);
        }
    }

    public static string NormalizeSignatureDataUrl(string signatureBase64)
    {
        var trimmedSignature = signatureBase64.Trim();
        return trimmedSignature.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            ? trimmedSignature
            : $"data:image/png;base64,{NormalizeBase64Payload(trimmedSignature)}";
    }

    public static double CalculateDistanceMeters(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        const double earthRadiusMeters = 6371000d;
        var latitudeDelta = ToRadians(latitude2 - latitude1);
        var longitudeDelta = ToRadians(longitude2 - longitude1);
        var firstLatitude = ToRadians(latitude1);
        var secondLatitude = ToRadians(latitude2);
        var haversine = Math.Sin(latitudeDelta / 2d) * Math.Sin(latitudeDelta / 2d) +
            Math.Cos(firstLatitude) * Math.Cos(secondLatitude) *
            Math.Sin(longitudeDelta / 2d) * Math.Sin(longitudeDelta / 2d);
        var arc = 2d * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1d - haversine));
        return earthRadiusMeters * arc;
    }

    private static async Task<QuotationHeader> CreateAutoApprovedQuotationAsync(
        ServiceRequestEntity serviceRequest,
        JobCard jobCard,
        BookingEntity booking,
        IBillingRepository billingRepository,
        IBillingCalculationService billingCalculationService,
        IQuotationNumberGenerator quotationNumberGenerator,
        IAuditLogRepository auditLogRepository,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        CancellationToken cancellationToken)
    {
        var now = currentDateTime.UtcNow;
        var actor = ResolveActor(currentUserContext, "FieldWorkflow");
        var serviceAmount = Math.Max(booking.EstimatedPrice, 0.00m);
        var calculation = billingCalculationService.CalculateInvoice(
            new[] { new BillingLineCalculationInput(1.00m, serviceAmount) },
            0.00m,
            0.00m,
            0.00m);
        var quotationNumber = await GenerateUniqueQuotationNumberAsync(quotationNumberGenerator, billingRepository, cancellationToken);
        var quotationHeader = new QuotationHeader
        {
            QuotationNumber = quotationNumber,
            JobCard = jobCard,
            CustomerId = booking.CustomerId,
            CurrentStatus = QuotationStatus.Approved,
            QuotationDateUtc = now,
            SubTotalAmount = calculation.SubTotalAmount,
            DiscountAmount = calculation.DiscountAmount,
            TaxPercentage = calculation.TaxPercentage,
            TaxAmount = calculation.TaxAmount,
            GrandTotalAmount = calculation.GrandTotalAmount,
            CustomerDecisionRemarks = "Auto-approved during field completion.",
            ApprovedDateUtc = now,
            Comments = "Auto-generated field quotation.",
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = currentUserContext.IPAddress
        };

        quotationHeader.Lines.Add(new QuotationLine
        {
            LineType = QuotationLineType.Service,
            LineDescription = string.IsNullOrWhiteSpace(booking.ServiceNameSnapshot)
                ? "Field Service"
                : booking.ServiceNameSnapshot,
            Quantity = 1.00m,
            UnitPrice = serviceAmount,
            LineAmount = billingCalculationService.CalculateLineAmount(1.00m, serviceAmount),
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = currentUserContext.IPAddress
        });
        quotationHeader.BillingStatusHistories.Add(new BillingStatusHistory
        {
            EntityType = BillingEntityType.Quotation,
            StatusName = QuotationStatus.Approved.ToString(),
            Remarks = "Auto-approved quotation generated from field workflow.",
            StatusDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = currentUserContext.IPAddress
        });

        await billingRepository.AddQuotationAsync(quotationHeader, cancellationToken);
        await auditLogRepository.AddAsync(
            CreateAuditLog(
                currentUserContext,
                now,
                "AutoCreateFieldQuotation",
                nameof(QuotationHeader),
                quotationNumber,
                serviceRequest.ServiceRequestNumber),
            cancellationToken);

        return quotationHeader;
    }

    private static async Task<QuotationHeader> CreateNewFieldQuotationAsync(
        ServiceRequestEntity serviceRequest,
        JobCard jobCard,
        BookingEntity booking,
        BillingCalculationResult calculation,
        string? remarks,
        IBillingRepository billingRepository,
        IQuotationNumberGenerator quotationNumberGenerator,
        IAuditLogRepository auditLogRepository,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        CancellationToken cancellationToken)
    {
        var now = currentDateTime.UtcNow;
        var quotationNumber = await GenerateUniqueQuotationNumberAsync(quotationNumberGenerator, billingRepository, cancellationToken);
        var quotationHeader = new QuotationHeader
        {
            QuotationNumber = quotationNumber,
            JobCard = jobCard,
            CustomerId = booking.CustomerId,
            CurrentStatus = QuotationStatus.PendingCustomerApproval,
            QuotationDateUtc = now,
            SubTotalAmount = calculation.SubTotalAmount,
            DiscountAmount = calculation.DiscountAmount,
            TaxPercentage = calculation.TaxPercentage,
            TaxAmount = calculation.TaxAmount,
            GrandTotalAmount = calculation.GrandTotalAmount,
            Comments = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim(),
            CreatedBy = ResolveActor(currentUserContext, "FieldWorkflow"),
            DateCreated = now,
            IPAddress = currentUserContext.IPAddress
        };

        await billingRepository.AddQuotationAsync(quotationHeader, cancellationToken);
        await auditLogRepository.AddAsync(
            CreateAuditLog(
                currentUserContext,
                now,
                "CreateFieldQuotationDraft",
                nameof(QuotationHeader),
                quotationNumber,
                serviceRequest.ServiceRequestNumber),
            cancellationToken);

        return quotationHeader;
    }

    private static async Task<QuotationHeader> ReuseRejectedFieldQuotationAsync(
        QuotationHeader existingQuotation,
        BillingCalculationResult calculation,
        string? remarks,
        IBillingRepository billingRepository,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        CancellationToken cancellationToken)
    {
        if (existingQuotation.CurrentStatus is QuotationStatus.PendingCustomerApproval or QuotationStatus.Approved or QuotationStatus.Invoiced)
        {
            throw new AppException(ErrorCodes.QuotationAlreadyExists, "A field estimate is already active for this job.", 409);
        }

        var quotationHeader = await billingRepository.GetQuotationByIdForUpdateAsync(existingQuotation.QuotationHeaderId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The rejected quotation could not be loaded for resubmission.", 404);
        var now = currentDateTime.UtcNow;
        var actor = ResolveActor(currentUserContext, "FieldWorkflow");

        quotationHeader.CurrentStatus = QuotationStatus.PendingCustomerApproval;
        quotationHeader.QuotationDateUtc = now;
        quotationHeader.SubTotalAmount = calculation.SubTotalAmount;
        quotationHeader.DiscountAmount = calculation.DiscountAmount;
        quotationHeader.TaxPercentage = calculation.TaxPercentage;
        quotationHeader.TaxAmount = calculation.TaxAmount;
        quotationHeader.GrandTotalAmount = calculation.GrandTotalAmount;
        quotationHeader.CustomerDecisionRemarks = string.Empty;
        quotationHeader.ApprovedDateUtc = null;
        quotationHeader.RejectedDateUtc = null;
        quotationHeader.Comments = string.IsNullOrWhiteSpace(remarks) ? quotationHeader.Comments : remarks.Trim();
        quotationHeader.UpdatedBy = actor;
        quotationHeader.LastUpdated = now;

        return quotationHeader;
    }

    private static async Task<InvoiceHeader> CreateInvoiceFromQuotationAsync(
        QuotationHeader quotationHeader,
        IBillingRepository billingRepository,
        IBillingCalculationService billingCalculationService,
        IInvoiceNumberGenerator invoiceNumberGenerator,
        IAuditLogRepository auditLogRepository,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        CancellationToken cancellationToken)
    {
        var activeLines = quotationHeader.Lines.Where(line => !line.IsDeleted).ToArray();

        if (activeLines.Length == 0)
        {
            throw new AppException(
                ErrorCodes.ValidationFailure,
                "The field estimate does not contain any active lines for invoice generation.",
                400);
        }

        var now = currentDateTime.UtcNow;
        var actor = ResolveActor(currentUserContext, "FieldWorkflow");
        var calculation = billingCalculationService.CalculateInvoice(
            activeLines.Select(line => new BillingLineCalculationInput(line.Quantity, line.UnitPrice)).ToArray(),
            quotationHeader.DiscountAmount,
            quotationHeader.TaxPercentage,
            0.00m);
        var invoiceNumber = await GenerateUniqueInvoiceNumberAsync(invoiceNumberGenerator, billingRepository, cancellationToken);
        var invoiceHeader = new InvoiceHeader
        {
            InvoiceNumber = invoiceNumber,
            QuotationHeader = quotationHeader,
            CustomerId = quotationHeader.CustomerId,
            CurrentStatus = calculation.InvoicePaymentStatus,
            InvoiceDateUtc = now,
            SubTotalAmount = calculation.SubTotalAmount,
            DiscountAmount = calculation.DiscountAmount,
            TaxPercentage = calculation.TaxPercentage,
            TaxAmount = calculation.TaxAmount,
            GrandTotalAmount = calculation.GrandTotalAmount,
            PaidAmount = calculation.PaidAmount,
            BalanceAmount = calculation.BalanceAmount,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = currentUserContext.IPAddress
        };

        foreach (var line in activeLines)
        {
            invoiceHeader.Lines.Add(new InvoiceLine
            {
                QuotationLine = line,
                LineType = line.LineType,
                LineDescription = line.LineDescription,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineAmount = line.LineAmount,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = currentUserContext.IPAddress
            });
        }

        quotationHeader.CurrentStatus = QuotationStatus.Invoiced;
        quotationHeader.UpdatedBy = actor;
        quotationHeader.LastUpdated = now;
        quotationHeader.InvoiceHeader = invoiceHeader;
        quotationHeader.BillingStatusHistories.Add(new BillingStatusHistory
        {
            InvoiceHeader = invoiceHeader,
            EntityType = BillingEntityType.Quotation,
            StatusName = QuotationStatus.Invoiced.ToString(),
            Remarks = "Quotation converted to invoice from field workflow.",
            StatusDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = currentUserContext.IPAddress
        });
        invoiceHeader.BillingStatusHistories.Add(new BillingStatusHistory
        {
            QuotationHeader = quotationHeader,
            EntityType = BillingEntityType.Invoice,
            StatusName = invoiceHeader.CurrentStatus.ToString(),
            Remarks = "Invoice generated from field workflow.",
            StatusDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = currentUserContext.IPAddress
        });

        await billingRepository.AddInvoiceAsync(invoiceHeader, cancellationToken);
        await auditLogRepository.AddAsync(
            CreateAuditLog(
                currentUserContext,
                now,
                "GenerateFieldInvoice",
                nameof(InvoiceHeader),
                invoiceNumber,
                quotationHeader.QuotationNumber),
            cancellationToken);

        return invoiceHeader;
    }

    private static async Task<string> GenerateUniqueQuotationNumberAsync(
        IQuotationNumberGenerator quotationNumberGenerator,
        IBillingRepository billingRepository,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            var quotationNumber = quotationNumberGenerator.GenerateNumber();

            if (!await billingRepository.QuotationNumberExistsAsync(quotationNumber, cancellationToken))
            {
                return quotationNumber;
            }
        }
    }

    private static async Task<string> GenerateUniqueInvoiceNumberAsync(
        IInvoiceNumberGenerator invoiceNumberGenerator,
        IBillingRepository billingRepository,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            var invoiceNumber = invoiceNumberGenerator.GenerateNumber();

            if (!await billingRepository.InvoiceNumberExistsAsync(invoiceNumber, cancellationToken))
            {
                return invoiceNumber;
            }
        }
    }

    private static IReadOnlyCollection<string> DeserializeIssues(string issuesIdentifiedJson)
    {
        if (string.IsNullOrWhiteSpace(issuesIdentifiedJson))
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(issuesIdentifiedJson) ?? Array.Empty<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    private static string NormalizeBase64Payload(string payload)
    {
        var trimmedPayload = payload.Trim();
        var separatorIndex = trimmedPayload.IndexOf(',');

        return separatorIndex >= 0
            ? trimmedPayload[(separatorIndex + 1)..]
            : trimmedPayload;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180d);
    }
}

internal sealed class FieldWorkflowChecklistItemValidator : AbstractValidator<SaveJobChecklistResponseItemRequest>
{
    public FieldWorkflowChecklistItemValidator()
    {
        RuleFor(request => request.ServiceChecklistMasterId).GreaterThan(0);
        RuleFor(request => request.ResponseRemarks).MaximumLength(512);
    }
}

internal sealed class FieldPartsRequestItemValidator : AbstractValidator<FieldPartsRequestItemRequest>
{
    public FieldPartsRequestItemValidator()
    {
        RuleFor(request => request.PartCode).NotEmpty().MaximumLength(64);
        RuleFor(request => request.PartName).NotEmpty().MaximumLength(256);
        RuleFor(request => request.QuantityRequested).GreaterThan(0.00m);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}

internal sealed class FieldQuotationLineValidator : AbstractValidator<QuotationLineRequest>
{
    public FieldQuotationLineValidator()
    {
        RuleFor(request => request.LineType)
            .NotEmpty()
            .Must(lineType => Enum.TryParse<QuotationLineType>(lineType, true, out _))
            .WithMessage("Quotation line type is invalid.");
        RuleFor(request => request.LineDescription).NotEmpty().MaximumLength(256);
        RuleFor(request => request.Quantity).GreaterThan(0.00m);
        RuleFor(request => request.UnitPrice).GreaterThanOrEqualTo(0.00m);
    }
}
