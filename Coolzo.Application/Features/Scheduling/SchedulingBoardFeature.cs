using System.Globalization;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.OperationsDashboard;
using Coolzo.Application.Features.Technician.Management;
using Coolzo.Contracts.Requests.Operations;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;
using BookingEntity = Coolzo.Domain.Entities.Booking;
using ServiceRequestEntity = Coolzo.Domain.Entities.ServiceRequest;
using TechnicianEntity = Coolzo.Domain.Entities.Technician;

namespace Coolzo.Application.Features.Scheduling;

public sealed record GetSchedulingBoardQuery(
    DateOnly DateFrom,
    DateOnly DateTo,
    long? TechnicianId) : IRequest<SchedulingBoardResponse>;

public sealed class GetSchedulingBoardQueryHandler : IRequestHandler<GetSchedulingBoardQuery, SchedulingBoardResponse>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ISchedulingRepository _schedulingRepository;
    private readonly ITechnicianRepository _technicianRepository;

    public GetSchedulingBoardQueryHandler(
        ISchedulingRepository schedulingRepository,
        ITechnicianRepository technicianRepository,
        IAdminConfigurationRepository adminConfigurationRepository)
    {
        _schedulingRepository = schedulingRepository;
        _technicianRepository = technicianRepository;
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<SchedulingBoardResponse> Handle(GetSchedulingBoardQuery request, CancellationToken cancellationToken)
    {
        SchedulingSupport.ValidateDateRange(request.DateFrom, request.DateTo);

        var referenceDate = request.DateFrom > DateOnly.FromDateTime(DateTime.UtcNow)
            ? request.DateFrom
            : DateOnly.FromDateTime(DateTime.UtcNow);
        var technicians = await SchedulingSupport.GetSchedulingTechniciansAsync(_technicianRepository, cancellationToken);
        var storedShifts = await _schedulingRepository.GetTechnicianShiftsAsync(request.TechnicianId, asNoTracking: true, cancellationToken);
        var businessHours = await _adminConfigurationRepository.GetBusinessHoursAsync(cancellationToken);
        var serviceRequests = await _schedulingRepository.GetBoardServiceRequestsAsync(request.DateFrom, request.DateTo, cancellationToken);
        var slots = await _schedulingRepository.GetSlotsAsync(zoneId: null, request.DateFrom, request.DateTo, cancellationToken);

        var technicianFilter = request.TechnicianId;
        var filteredTechnicians = technicianFilter.HasValue
            ? technicians.Where(technician => technician.TechnicianId == technicianFilter.Value).ToArray()
            : technicians.ToArray();
        var shiftsByTechnician = SchedulingSupport.BuildWeeklyShiftLookup(filteredTechnicians, storedShifts, businessHours);
        var boardJobs = serviceRequests
            .Where(SchedulingSupport.ShouldRenderOnBoard)
            .Where(serviceRequest => !technicianFilter.HasValue || SchedulingSupport.GetActiveAssignment(serviceRequest)?.TechnicianId == technicianFilter.Value)
            .ToArray();

        return new SchedulingBoardResponse(
            request.DateFrom,
            request.DateTo,
            DateTime.UtcNow,
            SchedulingSupport.BuildTimeSlots(slots, boardJobs),
            filteredTechnicians.Select(technician => SchedulingSupport.ToSchedulingTechnician(technician, referenceDate, shiftsByTechnician)).ToArray(),
            boardJobs
                .Where(serviceRequest => SchedulingSupport.GetActiveAssignment(serviceRequest) is not null)
                .Select(SchedulingSupport.ToBoardJob)
                .ToArray(),
            boardJobs
                .Where(serviceRequest => SchedulingSupport.GetActiveAssignment(serviceRequest) is null)
                .Select(SchedulingSupport.ToBoardJob)
                .ToArray());
    }
}

public sealed record GetSchedulingConflictsQuery(
    long ServiceRequestId,
    long TechnicianId,
    long SlotAvailabilityId) : IRequest<IReadOnlyCollection<SchedulingConflictResponse>>;

public sealed class GetSchedulingConflictsQueryHandler : IRequestHandler<GetSchedulingConflictsQuery, IReadOnlyCollection<SchedulingConflictResponse>>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly ISchedulingRepository _schedulingRepository;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly ITechnicianRepository _technicianRepository;

    public GetSchedulingConflictsQueryHandler(
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository,
        IBookingLookupRepository bookingLookupRepository,
        ISchedulingRepository schedulingRepository,
        IAdminConfigurationRepository adminConfigurationRepository)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _technicianRepository = technicianRepository;
        _bookingLookupRepository = bookingLookupRepository;
        _schedulingRepository = schedulingRepository;
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<IReadOnlyCollection<SchedulingConflictResponse>> Handle(GetSchedulingConflictsQuery request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _serviceRequestRepository.GetByIdAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);
        var technician = await _technicianRepository.GetManagementDetailAsync(request.TechnicianId, asNoTracking: true, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested technician could not be found.", 404);
        var slot = await _bookingLookupRepository.GetSlotAvailabilityByIdAsync(request.SlotAvailabilityId, cancellationToken)
            ?? throw new AppException(ErrorCodes.SlotUnavailable, "The requested slot could not be found.", 409);
        var businessHours = await _adminConfigurationRepository.GetBusinessHoursAsync(cancellationToken);
        var storedShifts = await _schedulingRepository.GetTechnicianShiftsAsync(technician.TechnicianId, asNoTracking: true, cancellationToken);

        return await SchedulingSupport.BuildConflictsAsync(
            serviceRequest,
            technician,
            slot,
            storedShifts,
            businessHours,
            _serviceRequestRepository,
            _technicianRepository,
            cancellationToken);
    }
}

public sealed record GetSchedulingSlotsQuery(
    long ZoneId,
    DateOnly SlotDate) : IRequest<IReadOnlyCollection<SchedulingSlotResponse>>;

public sealed class GetSchedulingSlotsQueryHandler : IRequestHandler<GetSchedulingSlotsQuery, IReadOnlyCollection<SchedulingSlotResponse>>
{
    private readonly ISchedulingRepository _schedulingRepository;

    public GetSchedulingSlotsQueryHandler(ISchedulingRepository schedulingRepository)
    {
        _schedulingRepository = schedulingRepository;
    }

    public async Task<IReadOnlyCollection<SchedulingSlotResponse>> Handle(GetSchedulingSlotsQuery request, CancellationToken cancellationToken)
    {
        var slots = await _schedulingRepository.GetSlotsAsync(request.ZoneId, request.SlotDate, request.SlotDate, cancellationToken);
        return slots.Select(SchedulingSupport.ToSchedulingSlot).ToArray();
    }
}

public sealed record GetTechnicianShiftsQuery(long? TechnicianId) : IRequest<IReadOnlyCollection<SchedulingShiftResponse>>;

public sealed class GetTechnicianShiftsQueryHandler : IRequestHandler<GetTechnicianShiftsQuery, IReadOnlyCollection<SchedulingShiftResponse>>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ISchedulingRepository _schedulingRepository;
    private readonly ITechnicianRepository _technicianRepository;

    public GetTechnicianShiftsQueryHandler(
        ISchedulingRepository schedulingRepository,
        ITechnicianRepository technicianRepository,
        IAdminConfigurationRepository adminConfigurationRepository)
    {
        _schedulingRepository = schedulingRepository;
        _technicianRepository = technicianRepository;
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<IReadOnlyCollection<SchedulingShiftResponse>> Handle(GetTechnicianShiftsQuery request, CancellationToken cancellationToken)
    {
        var technicians = await SchedulingSupport.GetSchedulingTechniciansAsync(_technicianRepository, cancellationToken);
        var filteredTechnicians = request.TechnicianId.HasValue
            ? technicians.Where(technician => technician.TechnicianId == request.TechnicianId.Value).ToArray()
            : technicians.ToArray();
        var storedShifts = await _schedulingRepository.GetTechnicianShiftsAsync(request.TechnicianId, asNoTracking: true, cancellationToken);
        var businessHours = await _adminConfigurationRepository.GetBusinessHoursAsync(cancellationToken);
        var shiftsByTechnician = SchedulingSupport.BuildWeeklyShiftLookup(filteredTechnicians, storedShifts, businessHours);

        return filteredTechnicians
            .Select(technician => SchedulingSupport.ToSchedulingShift(technician, shiftsByTechnician))
            .ToArray();
    }
}

public sealed record GetSchedulingAmcAutoQuery(
    DateOnly? DateFrom,
    DateOnly? DateTo) : IRequest<IReadOnlyCollection<SchedulingAmcAutoVisitResponse>>;

public sealed class GetSchedulingAmcAutoQueryHandler : IRequestHandler<GetSchedulingAmcAutoQuery, IReadOnlyCollection<SchedulingAmcAutoVisitResponse>>
{
    private readonly ISchedulingRepository _schedulingRepository;

    public GetSchedulingAmcAutoQueryHandler(ISchedulingRepository schedulingRepository)
    {
        _schedulingRepository = schedulingRepository;
    }

    public async Task<IReadOnlyCollection<SchedulingAmcAutoVisitResponse>> Handle(GetSchedulingAmcAutoQuery request, CancellationToken cancellationToken)
    {
        var dateFrom = request.DateFrom ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var dateTo = request.DateTo ?? dateFrom.AddDays(6);
        SchedulingSupport.ValidateDateRange(dateFrom, dateTo);

        var visits = await _schedulingRepository.GetAmcVisitsAsync(dateFrom, dateTo, unlinkedOnly: true, cancellationToken);

        return visits.Select(SchedulingSupport.ToSchedulingAmcAutoVisit).ToArray();
    }
}

public sealed record GetSchedulingDaySheetQuery(
    DateOnly ScheduleDate,
    long? TechnicianId) : IRequest<SchedulingDaySheetResponse>;

public sealed class GetSchedulingDaySheetQueryHandler : IRequestHandler<GetSchedulingDaySheetQuery, SchedulingDaySheetResponse>
{
    private readonly ISchedulingRepository _schedulingRepository;
    private readonly ITechnicianRepository _technicianRepository;

    public GetSchedulingDaySheetQueryHandler(
        ISchedulingRepository schedulingRepository,
        ITechnicianRepository technicianRepository)
    {
        _schedulingRepository = schedulingRepository;
        _technicianRepository = technicianRepository;
    }

    public async Task<SchedulingDaySheetResponse> Handle(GetSchedulingDaySheetQuery request, CancellationToken cancellationToken)
    {
        var technicians = await SchedulingSupport.GetSchedulingTechniciansAsync(_technicianRepository, cancellationToken);
        var filteredTechnicians = request.TechnicianId.HasValue
            ? technicians.Where(technician => technician.TechnicianId == request.TechnicianId.Value).ToArray()
            : technicians.ToArray();
        var serviceRequests = await _schedulingRepository.GetBoardServiceRequestsAsync(request.ScheduleDate, request.ScheduleDate, cancellationToken);
        var technicianLookup = filteredTechnicians.ToDictionary(technician => technician.TechnicianId);

        var technicianSheets = serviceRequests
            .Where(SchedulingSupport.ShouldRenderOnBoard)
            .Select(serviceRequest => new
            {
                ServiceRequest = serviceRequest,
                Assignment = SchedulingSupport.GetActiveAssignment(serviceRequest)
            })
            .Where(entry => entry.Assignment is not null && technicianLookup.ContainsKey(entry.Assignment.TechnicianId))
            .GroupBy(entry => entry.Assignment!.TechnicianId)
            .Select(group =>
            {
                var technician = technicianLookup[group.Key];
                return new SchedulingDaySheetTechnicianResponse(
                    technician.TechnicianId,
                    technician.TechnicianCode,
                    technician.TechnicianName,
                    technician.BaseZone?.ZoneName,
                    group
                        .OrderBy(entry => SchedulingSupport.GetSlotStartTime(entry.ServiceRequest))
                        .ThenBy(entry => entry.ServiceRequest.ServiceRequestNumber)
                        .Select(entry => SchedulingSupport.ToDaySheetItem(entry.ServiceRequest))
                        .ToArray());
            })
            .OrderBy(item => item.TechnicianName)
            .ToArray();

        return new SchedulingDaySheetResponse(
            request.ScheduleDate,
            DateTime.UtcNow,
            technicianSheets);
    }
}

public sealed record ScheduleAssignServiceRequestCommand(
    long ServiceRequestId,
    long TechnicianId,
    long SlotAvailabilityId,
    string? Remarks) : IRequest<SchedulingBoardJobResponse>;

public sealed class ScheduleAssignServiceRequestCommandHandler : IRequestHandler<ScheduleAssignServiceRequestCommand, SchedulingBoardJobResponse>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger<ScheduleAssignServiceRequestCommandHandler> _logger;
    private readonly ISchedulingRepository _schedulingRepository;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly ITechnicianRepository _technicianRepository;

    public ScheduleAssignServiceRequestCommandHandler(
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository,
        IBookingLookupRepository bookingLookupRepository,
        ISchedulingRepository schedulingRepository,
        IAdminConfigurationRepository adminConfigurationRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<ScheduleAssignServiceRequestCommandHandler> logger)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _technicianRepository = technicianRepository;
        _bookingLookupRepository = bookingLookupRepository;
        _schedulingRepository = schedulingRepository;
        _adminConfigurationRepository = adminConfigurationRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<SchedulingBoardJobResponse> Handle(ScheduleAssignServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _serviceRequestRepository.GetByIdForUpdateAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);
        var activeAssignment = SchedulingSupport.GetActiveAssignment(serviceRequest);
        if (activeAssignment is not null)
        {
            throw new AppException(ErrorCodes.DuplicateAssignment, "A technician is already assigned to this service request.", 409);
        }

        SchedulingSupport.EnsureSchedulingEditable(serviceRequest);

        var technician = await _technicianRepository.GetByIdForUpdateAsync(request.TechnicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The selected technician could not be found.", 404);
        var slot = await _bookingLookupRepository.GetSlotAvailabilityByIdAsync(request.SlotAvailabilityId, cancellationToken)
            ?? throw new AppException(ErrorCodes.SlotUnavailable, "The selected slot could not be found.", 409);

        await SchedulingSupport.EnsureSlotAndConflictsAreValidAsync(
            serviceRequest,
            technician,
            slot,
            _schedulingRepository,
            _adminConfigurationRepository,
            _serviceRequestRepository,
            _technicianRepository,
            cancellationToken);
        await SchedulingSupport.EnsureTechnicianCapacityAsync(
            serviceRequest,
            technician,
            slot,
            _serviceRequestRepository,
            _technicianRepository,
            ignoreCurrentAssignment: null,
            provisionalAssignments: 0,
            cancellationToken);

        var now = _currentDateTime.UtcNow;
        var actor = SchedulingSupport.ResolveActor(_currentUserContext, "SchedulingAssign");
        var remarks = string.IsNullOrWhiteSpace(request.Remarks)
            ? $"Service request scheduled to {technician.TechnicianName}."
            : request.Remarks.Trim();

        SchedulingSupport.ApplySlotChange(serviceRequest.Booking!, slot, remarks, actor, now, _currentUserContext.IPAddress);

        serviceRequest.CurrentStatus = ServiceRequestStatus.Assigned;
        serviceRequest.UpdatedBy = actor;
        serviceRequest.LastUpdated = now;
        serviceRequest.IPAddress = _currentUserContext.IPAddress;
        serviceRequest.Assignments.Add(new ServiceRequestAssignment
        {
            TechnicianId = technician.TechnicianId,
            AssignedDateUtc = now,
            AssignmentRemarks = remarks,
            IsAutoAssigned = false,
            IsActiveAssignment = true,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });
        serviceRequest.AssignmentLogs.Add(new AssignmentLog
        {
            CurrentTechnicianId = technician.TechnicianId,
            ActionName = "Assigned",
            Remarks = remarks,
            ActionDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });
        serviceRequest.StatusHistories.Add(new ServiceRequestStatusHistory
        {
            Status = ServiceRequestStatus.Assigned,
            Remarks = remarks,
            StatusDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = _currentUserContext.IPAddress
        });

        await SchedulingSupport.UpdateAvailabilityAsync(
            technician,
            slot.SlotDate,
            1,
            actor,
            now,
            _currentUserContext.IPAddress,
            _technicianRepository,
            cancellationToken);
        await _auditLogRepository.AddAsync(
            TechnicianManagementSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "ScheduleAssignServiceRequest",
                "ServiceRequest",
                serviceRequest.ServiceRequestNumber,
                $"{technician.TechnicianCode}:{slot.SlotAvailabilityId}"),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Scheduling board assigned technician {TechnicianId} to service request {ServiceRequestId} on slot {SlotAvailabilityId}.",
            technician.TechnicianId,
            serviceRequest.ServiceRequestId,
            slot.SlotAvailabilityId);

        return SchedulingSupport.ToBoardJob(serviceRequest);
    }
}

public sealed record ScheduleReassignServiceRequestCommand(
    long ServiceRequestId,
    long TechnicianId,
    long SlotAvailabilityId,
    string? Remarks) : IRequest<SchedulingBoardJobResponse>;

public sealed class ScheduleReassignServiceRequestCommandHandler : IRequestHandler<ScheduleReassignServiceRequestCommand, SchedulingBoardJobResponse>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger<ScheduleReassignServiceRequestCommandHandler> _logger;
    private readonly ISchedulingRepository _schedulingRepository;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly ITechnicianRepository _technicianRepository;

    public ScheduleReassignServiceRequestCommandHandler(
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository,
        IBookingLookupRepository bookingLookupRepository,
        ISchedulingRepository schedulingRepository,
        IAdminConfigurationRepository adminConfigurationRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<ScheduleReassignServiceRequestCommandHandler> logger)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _technicianRepository = technicianRepository;
        _bookingLookupRepository = bookingLookupRepository;
        _schedulingRepository = schedulingRepository;
        _adminConfigurationRepository = adminConfigurationRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<SchedulingBoardJobResponse> Handle(ScheduleReassignServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _serviceRequestRepository.GetByIdForUpdateAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);
        var activeAssignment = SchedulingSupport.GetActiveAssignment(serviceRequest)
            ?? throw new AppException(ErrorCodes.Conflict, "No active technician assignment was found for this service request.", 409);

        SchedulingSupport.EnsureSchedulingEditable(serviceRequest);

        var currentTechnician = await _technicianRepository.GetByIdForUpdateAsync(activeAssignment.TechnicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The currently assigned technician could not be found.", 404);
        var nextTechnician = activeAssignment.TechnicianId == request.TechnicianId
            ? currentTechnician
            : await _technicianRepository.GetByIdForUpdateAsync(request.TechnicianId, cancellationToken)
                ?? throw new AppException(ErrorCodes.NotFound, "The selected technician could not be found.", 404);
        var slot = await _bookingLookupRepository.GetSlotAvailabilityByIdAsync(request.SlotAvailabilityId, cancellationToken)
            ?? throw new AppException(ErrorCodes.SlotUnavailable, "The selected slot could not be found.", 409);

        await SchedulingSupport.EnsureSlotAndConflictsAreValidAsync(
            serviceRequest,
            nextTechnician,
            slot,
            _schedulingRepository,
            _adminConfigurationRepository,
            _serviceRequestRepository,
            _technicianRepository,
            cancellationToken);
        await SchedulingSupport.EnsureTechnicianCapacityAsync(
            serviceRequest,
            nextTechnician,
            slot,
            _serviceRequestRepository,
            _technicianRepository,
            ignoreCurrentAssignment: activeAssignment,
            provisionalAssignments: 0,
            cancellationToken);

        var now = _currentDateTime.UtcNow;
        var actor = SchedulingSupport.ResolveActor(_currentUserContext, "SchedulingReassign");
        var previousSlotDate = serviceRequest.Booking?.SlotAvailability?.SlotDate;
        var previousTechnicianId = currentTechnician.TechnicianId;
        var remarks = string.IsNullOrWhiteSpace(request.Remarks)
            ? previousTechnicianId == nextTechnician.TechnicianId
                ? $"Service request rescheduled for {nextTechnician.TechnicianName}."
                : $"Service request reassigned from {currentTechnician.TechnicianName} to {nextTechnician.TechnicianName}."
            : request.Remarks.Trim();

        SchedulingSupport.ApplySlotChange(serviceRequest.Booking!, slot, remarks, actor, now, _currentUserContext.IPAddress);
        serviceRequest.CurrentStatus = ServiceRequestStatus.Assigned;
        serviceRequest.UpdatedBy = actor;
        serviceRequest.LastUpdated = now;
        serviceRequest.IPAddress = _currentUserContext.IPAddress;

        if (previousTechnicianId == nextTechnician.TechnicianId)
        {
            serviceRequest.AssignmentLogs.Add(new AssignmentLog
            {
                CurrentTechnicianId = nextTechnician.TechnicianId,
                ActionName = "Rescheduled",
                Remarks = remarks,
                ActionDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });
            serviceRequest.StatusHistories.Add(new ServiceRequestStatusHistory
            {
                Status = ServiceRequestStatus.Assigned,
                Remarks = remarks,
                StatusDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });
        }
        else
        {
            activeAssignment.IsActiveAssignment = false;
            activeAssignment.UnassignedDateUtc = now;
            activeAssignment.UnassignmentRemarks = remarks;
            activeAssignment.UpdatedBy = actor;
            activeAssignment.LastUpdated = now;
            activeAssignment.IPAddress = _currentUserContext.IPAddress;

            serviceRequest.Assignments.Add(new ServiceRequestAssignment
            {
                TechnicianId = nextTechnician.TechnicianId,
                AssignedDateUtc = now,
                AssignmentRemarks = remarks,
                IsAutoAssigned = false,
                IsActiveAssignment = true,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });
            serviceRequest.AssignmentLogs.Add(new AssignmentLog
            {
                PreviousTechnicianId = currentTechnician.TechnicianId,
                CurrentTechnicianId = nextTechnician.TechnicianId,
                ActionName = "Reassigned",
                Remarks = remarks,
                ActionDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });

            await SchedulingSupport.UpdateAvailabilityAsync(
                currentTechnician,
                previousSlotDate ?? slot.SlotDate,
                -1,
                actor,
                now,
                _currentUserContext.IPAddress,
                _technicianRepository,
                cancellationToken);
            await SchedulingSupport.UpdateAvailabilityAsync(
                nextTechnician,
                slot.SlotDate,
                1,
                actor,
                now,
                _currentUserContext.IPAddress,
                _technicianRepository,
                cancellationToken);
        }

        if (previousTechnicianId == nextTechnician.TechnicianId &&
            previousSlotDate.HasValue &&
            previousSlotDate.Value != slot.SlotDate)
        {
            await SchedulingSupport.UpdateAvailabilityAsync(
                nextTechnician,
                previousSlotDate.Value,
                -1,
                actor,
                now,
                _currentUserContext.IPAddress,
                _technicianRepository,
                cancellationToken);
            await SchedulingSupport.UpdateAvailabilityAsync(
                nextTechnician,
                slot.SlotDate,
                1,
                actor,
                now,
                _currentUserContext.IPAddress,
                _technicianRepository,
                cancellationToken);
        }

        await _auditLogRepository.AddAsync(
            TechnicianManagementSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "ScheduleReassignServiceRequest",
                "ServiceRequest",
                serviceRequest.ServiceRequestNumber,
                $"{currentTechnician.TechnicianCode}->{nextTechnician.TechnicianCode}:{slot.SlotAvailabilityId}"),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Scheduling board reassigned service request {ServiceRequestId} to technician {TechnicianId} on slot {SlotAvailabilityId}.",
            serviceRequest.ServiceRequestId,
            nextTechnician.TechnicianId,
            slot.SlotAvailabilityId);

        return SchedulingSupport.ToBoardJob(serviceRequest);
    }
}

public sealed record UpdateScheduleSlotCommand(
    long SlotAvailabilityId,
    bool IsBlocked,
    int? AvailableCapacity) : IRequest<SchedulingSlotResponse>;

public sealed class UpdateScheduleSlotCommandHandler : IRequestHandler<UpdateScheduleSlotCommand, SchedulingSlotResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateScheduleSlotCommandHandler(
        IBookingLookupRepository bookingLookupRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _bookingLookupRepository = bookingLookupRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<SchedulingSlotResponse> Handle(UpdateScheduleSlotCommand request, CancellationToken cancellationToken)
    {
        var slot = await _bookingLookupRepository.GetSlotAvailabilityByIdAsync(request.SlotAvailabilityId, cancellationToken)
            ?? throw new AppException(ErrorCodes.SlotUnavailable, "The selected slot could not be found.", 409);

        if (request.AvailableCapacity.HasValue && request.AvailableCapacity.Value < slot.ReservedCapacity)
        {
            throw new AppException(ErrorCodes.ValidationFailure, "Available capacity cannot be lower than reserved capacity.", 400);
        }

        slot.IsBlocked = request.IsBlocked;
        if (request.AvailableCapacity.HasValue)
        {
            slot.AvailableCapacity = request.AvailableCapacity.Value;
        }

        slot.UpdatedBy = SchedulingSupport.ResolveActor(_currentUserContext, "ScheduleSlotUpdate");
        slot.LastUpdated = _currentDateTime.UtcNow;
        slot.IPAddress = _currentUserContext.IPAddress;

        await _auditLogRepository.AddAsync(
            TechnicianManagementSupport.CreateAuditLog(
                _currentUserContext,
                _currentDateTime.UtcNow,
                "UpdateScheduleSlot",
                "SlotAvailability",
                slot.SlotAvailabilityId.ToString(CultureInfo.InvariantCulture),
                $"{slot.IsBlocked}:{slot.AvailableCapacity}"),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return SchedulingSupport.ToSchedulingSlot(slot);
    }
}

public sealed record UpdateTechnicianShiftsCommand(
    long TechnicianId,
    IReadOnlyCollection<ScheduleShiftDayRequest> Days) : IRequest<SchedulingShiftResponse>;

public sealed class UpdateTechnicianShiftsCommandHandler : IRequestHandler<UpdateTechnicianShiftsCommand, SchedulingShiftResponse>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ISchedulingRepository _schedulingRepository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTechnicianShiftsCommandHandler(
        ISchedulingRepository schedulingRepository,
        ITechnicianRepository technicianRepository,
        IAdminConfigurationRepository adminConfigurationRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _schedulingRepository = schedulingRepository;
        _technicianRepository = technicianRepository;
        _adminConfigurationRepository = adminConfigurationRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<SchedulingShiftResponse> Handle(UpdateTechnicianShiftsCommand request, CancellationToken cancellationToken)
    {
        var technician = await _technicianRepository.GetByIdForUpdateAsync(request.TechnicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The selected technician could not be found.", 404);
        var actor = SchedulingSupport.ResolveActor(_currentUserContext, "TechnicianShiftUpdate");
        var now = _currentDateTime.UtcNow;

        foreach (var day in request.Days
                     .GroupBy(item => item.DayOfWeekNumber)
                     .Select(group => group.Last())
                     .OrderBy(item => item.DayOfWeekNumber))
        {
            SchedulingSupport.ValidateShiftRequest(day);
            var existing = await _schedulingRepository.GetTechnicianShiftForUpdateAsync(technician.TechnicianId, day.DayOfWeekNumber, cancellationToken);
            if (existing is null)
            {
                await _schedulingRepository.AddTechnicianShiftAsync(
                    new TechnicianShift
                    {
                        TechnicianId = technician.TechnicianId,
                        DayOfWeekNumber = day.DayOfWeekNumber,
                        ShiftStartTimeLocal = SchedulingSupport.ParseNullableTime(day.ShiftStartTime),
                        ShiftEndTimeLocal = SchedulingSupport.ParseNullableTime(day.ShiftEndTime),
                        BreakStartTimeLocal = SchedulingSupport.ParseNullableTime(day.BreakStartTime),
                        BreakEndTimeLocal = SchedulingSupport.ParseNullableTime(day.BreakEndTime),
                        IsOffDuty = day.IsOffDuty,
                        CreatedBy = actor,
                        DateCreated = now,
                        IPAddress = _currentUserContext.IPAddress
                    },
                    cancellationToken);

                continue;
            }

            existing.ShiftStartTimeLocal = SchedulingSupport.ParseNullableTime(day.ShiftStartTime);
            existing.ShiftEndTimeLocal = SchedulingSupport.ParseNullableTime(day.ShiftEndTime);
            existing.BreakStartTimeLocal = SchedulingSupport.ParseNullableTime(day.BreakStartTime);
            existing.BreakEndTimeLocal = SchedulingSupport.ParseNullableTime(day.BreakEndTime);
            existing.IsOffDuty = day.IsOffDuty;
            existing.UpdatedBy = actor;
            existing.LastUpdated = now;
            existing.IPAddress = _currentUserContext.IPAddress;
        }

        await _auditLogRepository.AddAsync(
            TechnicianManagementSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "UpdateTechnicianShifts",
                "Technician",
                technician.TechnicianCode,
                request.Days.Count.ToString(CultureInfo.InvariantCulture)),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var businessHours = await _adminConfigurationRepository.GetBusinessHoursAsync(cancellationToken);
        var storedShifts = await _schedulingRepository.GetTechnicianShiftsAsync(technician.TechnicianId, asNoTracking: true, cancellationToken);
        var detailedTechnician = await _technicianRepository.GetManagementDetailAsync(technician.TechnicianId, asNoTracking: true, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The updated technician could not be reloaded.", 404);
        var shiftsByTechnician = SchedulingSupport.BuildWeeklyShiftLookup(new[] { detailedTechnician }, storedShifts, businessHours);

        return SchedulingSupport.ToSchedulingShift(detailedTechnician, shiftsByTechnician);
    }
}

public sealed record ScheduleAmcBulkAssignCommand(
    long TechnicianId,
    IReadOnlyCollection<ScheduleAmcBulkAssignVisitRequest> Visits,
    string? Remarks) : IRequest<IReadOnlyCollection<SchedulingBoardJobResponse>>;

public sealed class ScheduleAmcBulkAssignCommandHandler : IRequestHandler<ScheduleAmcBulkAssignCommand, IReadOnlyCollection<SchedulingBoardJobResponse>>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly IBookingReferenceGenerator _bookingReferenceGenerator;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger<ScheduleAmcBulkAssignCommandHandler> _logger;
    private readonly ISchedulingRepository _schedulingRepository;
    private readonly IServiceRequestNumberGenerator _serviceRequestNumberGenerator;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly ITechnicianRepository _technicianRepository;

    public ScheduleAmcBulkAssignCommandHandler(
        ISchedulingRepository schedulingRepository,
        ITechnicianRepository technicianRepository,
        IServiceRequestRepository serviceRequestRepository,
        IBookingRepository bookingRepository,
        IBookingLookupRepository bookingLookupRepository,
        IAdminConfigurationRepository adminConfigurationRepository,
        IBookingReferenceGenerator bookingReferenceGenerator,
        IServiceRequestNumberGenerator serviceRequestNumberGenerator,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<ScheduleAmcBulkAssignCommandHandler> logger)
    {
        _schedulingRepository = schedulingRepository;
        _technicianRepository = technicianRepository;
        _serviceRequestRepository = serviceRequestRepository;
        _bookingRepository = bookingRepository;
        _bookingLookupRepository = bookingLookupRepository;
        _adminConfigurationRepository = adminConfigurationRepository;
        _bookingReferenceGenerator = bookingReferenceGenerator;
        _serviceRequestNumberGenerator = serviceRequestNumberGenerator;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<SchedulingBoardJobResponse>> Handle(ScheduleAmcBulkAssignCommand request, CancellationToken cancellationToken)
    {
        if (request.Visits.Count == 0)
        {
            return Array.Empty<SchedulingBoardJobResponse>();
        }

        var technician = await _technicianRepository.GetByIdForUpdateAsync(request.TechnicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The selected technician could not be found.", 404);
        if (!technician.IsActive)
        {
            throw new AppException(ErrorCodes.TechnicianInactive, "The selected technician is inactive.", 409);
        }

        var actor = SchedulingSupport.ResolveActor(_currentUserContext, "AmcBulkAssign");
        var now = _currentDateTime.UtcNow;
        var businessHours = await _adminConfigurationRepository.GetBusinessHoursAsync(cancellationToken);
        var storedShifts = await _schedulingRepository.GetTechnicianShiftsAsync(technician.TechnicianId, asNoTracking: true, cancellationToken);
        var createdJobs = new List<SchedulingBoardJobResponse>(request.Visits.Count);
        var provisionalAssignmentsByDate = new Dictionary<DateOnly, int>();

        foreach (var visitRequest in request.Visits
                     .GroupBy(item => item.AmcVisitScheduleId)
                     .Select(group => group.Last()))
        {
            var visit = await _schedulingRepository.GetAmcVisitForUpdateAsync(visitRequest.AmcVisitScheduleId, cancellationToken)
                ?? throw new AppException(ErrorCodes.NotFound, "The requested AMC visit could not be found.", 404);
            if (visit.ServiceRequestId.HasValue)
            {
                throw new AppException(ErrorCodes.DuplicateValue, "One or more AMC visits are already linked to service requests.", 409);
            }

            var slot = await _bookingLookupRepository.GetSlotAvailabilityByIdAsync(visitRequest.SlotAvailabilityId, cancellationToken)
                ?? throw new AppException(ErrorCodes.SlotUnavailable, "The selected AMC slot could not be found.", 409);
            if (slot.SlotDate != visit.ScheduledDate)
            {
                throw new AppException(ErrorCodes.ValidationFailure, "AMC visit slot date must match the scheduled visit date.", 400);
            }

            var sourceBooking = visit.CustomerAmc?.JobCard?.ServiceRequest?.Booking
                ?? throw new AppException(ErrorCodes.Conflict, "The AMC visit is missing source booking details.", 409);
            var customer = visit.CustomerAmc?.Customer
                ?? throw new AppException(ErrorCodes.NotFound, "The AMC visit customer could not be resolved.", 404);
            var customerAddress = sourceBooking.CustomerAddress
                ?? throw new AppException(ErrorCodes.Conflict, "The AMC source booking is missing customer address details.", 409);
            var sourceLine = sourceBooking.BookingLines
                .Where(line => !line.IsDeleted)
                .OrderBy(line => line.BookingLineId)
                .FirstOrDefault()
                ?? throw new AppException(ErrorCodes.Conflict, "The AMC source booking is missing service line details.", 409);

            SchedulingSupport.ValidateSlotSelection(sourceBooking, slot, allowCurrentSlotReuse: false);

            var bookingReference = await GenerateUniqueBookingReferenceAsync(cancellationToken);
            slot.ReservedCapacity += 1;

            var booking = new BookingEntity
            {
                BookingReference = bookingReference,
                Customer = customer,
                CustomerAddress = customerAddress,
                ZoneId = slot.ZoneId,
                SlotAvailability = slot,
                SlotAvailabilityId = slot.SlotAvailabilityId,
                BookingDateUtc = now,
                BookingStatus = BookingStatus.Confirmed,
                SourceChannel = sourceBooking.SourceChannel,
                IsGuestBooking = false,
                IsEmergency = false,
                EmergencySurchargeAmount = 0m,
                CustomerNameSnapshot = customer.CustomerName,
                MobileNumberSnapshot = customer.MobileNumber,
                EmailAddressSnapshot = customer.EmailAddress,
                AddressLine1Snapshot = customerAddress.AddressLine1,
                AddressLine2Snapshot = customerAddress.AddressLine2,
                LandmarkSnapshot = customerAddress.Landmark,
                CityNameSnapshot = customerAddress.CityName,
                PincodeSnapshot = customerAddress.Pincode,
                ZoneNameSnapshot = slot.Zone?.ZoneName ?? sourceBooking.ZoneNameSnapshot,
                ServiceNameSnapshot = sourceBooking.ServiceNameSnapshot,
                EstimatedPrice = 0m,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            };
            booking.BookingLines.Add(new BookingLine
            {
                ServiceId = sourceLine.ServiceId,
                AcTypeId = sourceLine.AcTypeId,
                TonnageId = sourceLine.TonnageId,
                BrandId = sourceLine.BrandId,
                Quantity = sourceLine.Quantity,
                ModelName = sourceLine.ModelName,
                IssueNotes = $"AMC visit {visit.VisitNumber} auto-scheduled from subscription {visit.CustomerAmc?.AmcPlan?.PlanName ?? visit.CustomerAmcId.ToString(CultureInfo.InvariantCulture)}.",
                UnitPrice = 0m,
                LineTotal = 0m,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });
            booking.BookingStatusHistories.Add(new BookingStatusHistory
            {
                BookingStatus = BookingStatus.Confirmed,
                Remarks = $"AMC visit {visit.VisitNumber} converted to scheduled job.",
                StatusDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });
            await _bookingRepository.AddBookingAsync(booking, cancellationToken);

            var serviceRequest = new ServiceRequestEntity
            {
                Booking = booking,
                ServiceRequestNumber = await GenerateUniqueServiceRequestNumberAsync(cancellationToken),
                CurrentStatus = ServiceRequestStatus.Assigned,
                ServiceRequestDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            };
            serviceRequest.StatusHistories.Add(new ServiceRequestStatusHistory
            {
                Status = ServiceRequestStatus.New,
                Remarks = $"Service request created from AMC visit {visit.VisitNumber}.",
                StatusDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });
            serviceRequest.StatusHistories.Add(new ServiceRequestStatusHistory
            {
                Status = ServiceRequestStatus.Assigned,
                Remarks = string.IsNullOrWhiteSpace(request.Remarks)
                    ? $"AMC visit assigned to {technician.TechnicianName}."
                    : request.Remarks.Trim(),
                StatusDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });
            serviceRequest.Assignments.Add(new ServiceRequestAssignment
            {
                TechnicianId = technician.TechnicianId,
                AssignedDateUtc = now,
                AssignmentRemarks = string.IsNullOrWhiteSpace(request.Remarks)
                    ? $"AMC bulk assignment for visit {visit.VisitNumber}."
                    : request.Remarks.Trim(),
                IsAutoAssigned = false,
                IsActiveAssignment = true,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });
            serviceRequest.AssignmentLogs.Add(new AssignmentLog
            {
                CurrentTechnicianId = technician.TechnicianId,
                ActionName = "AmcBulkAssigned",
                Remarks = string.IsNullOrWhiteSpace(request.Remarks)
                    ? $"AMC visit {visit.VisitNumber} scheduled from review board."
                    : request.Remarks.Trim(),
                ActionDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });
            await _serviceRequestRepository.AddAsync(serviceRequest, cancellationToken);

            visit.ServiceRequest = serviceRequest;
            visit.VisitRemarks = string.IsNullOrWhiteSpace(request.Remarks)
                ? $"AMC visit {visit.VisitNumber} assigned to {technician.TechnicianName}."
                : request.Remarks.Trim();
            visit.UpdatedBy = actor;
            visit.LastUpdated = now;
            visit.IPAddress = _currentUserContext.IPAddress;

            var conflicts = await SchedulingSupport.BuildConflictsAsync(
                serviceRequest,
                technician,
                slot,
                storedShifts,
                businessHours,
                _serviceRequestRepository,
                _technicianRepository,
                cancellationToken);
            SchedulingSupport.ThrowIfBlockingConflictsExist(conflicts);

            await SchedulingSupport.EnsureTechnicianCapacityAsync(
                serviceRequest,
                technician,
                slot,
                _serviceRequestRepository,
                _technicianRepository,
                ignoreCurrentAssignment: null,
                provisionalAssignments: provisionalAssignmentsByDate.TryGetValue(slot.SlotDate, out var provisionalAssignments) ? provisionalAssignments : 0,
                cancellationToken);
            await SchedulingSupport.UpdateAvailabilityAsync(
                technician,
                slot.SlotDate,
                1,
                actor,
                now,
                _currentUserContext.IPAddress,
                _technicianRepository,
                cancellationToken);
            provisionalAssignmentsByDate[slot.SlotDate] = provisionalAssignmentsByDate.TryGetValue(slot.SlotDate, out var existingAssignments)
                ? existingAssignments + 1
                : 1;

            createdJobs.Add(SchedulingSupport.ToBoardJob(serviceRequest));
        }

        await _auditLogRepository.AddAsync(
            TechnicianManagementSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "ScheduleAmcBulkAssign",
                "AmcVisitSchedule",
                request.TechnicianId.ToString(CultureInfo.InvariantCulture),
                request.Visits.Count.ToString(CultureInfo.InvariantCulture)),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "AMC bulk assignment created {JobCount} scheduled jobs for technician {TechnicianId}.",
            createdJobs.Count,
            technician.TechnicianId);

        return createdJobs;
    }

    private async Task<string> GenerateUniqueBookingReferenceAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var bookingReference = _bookingReferenceGenerator.GenerateReference();
            if (!await _bookingRepository.BookingReferenceExistsAsync(bookingReference, cancellationToken))
            {
                return bookingReference;
            }
        }
    }

    private async Task<string> GenerateUniqueServiceRequestNumberAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var serviceRequestNumber = _serviceRequestNumberGenerator.GenerateNumber();
            if (!await _serviceRequestRepository.ServiceRequestNumberExistsAsync(serviceRequestNumber, cancellationToken))
            {
                return serviceRequestNumber;
            }
        }
    }
}

internal static class SchedulingSupport
{
    private static readonly string[] BlockingConflictTypes = ["overlap", "shift"];

    public static string ResolveActor(ICurrentUserContext currentUserContext, string fallback)
    {
        return string.IsNullOrWhiteSpace(currentUserContext.UserName) ? fallback : currentUserContext.UserName;
    }

    public static void ValidateDateRange(DateOnly fromDate, DateOnly toDate)
    {
        if (fromDate > toDate)
        {
            throw new AppException(ErrorCodes.ValidationFailure, "The schedule date range is invalid.", 400);
        }
    }

    public static async Task<IReadOnlyCollection<TechnicianEntity>> GetSchedulingTechniciansAsync(
        ITechnicianRepository technicianRepository,
        CancellationToken cancellationToken)
    {
        return await technicianRepository.SearchManagementAsync(
            searchTerm: null,
            activeOnly: true,
            zoneName: null,
            skillName: null,
            availability: null,
            minimumRating: null,
            cancellationToken: cancellationToken);
    }

    public static Dictionary<long, IReadOnlyCollection<SchedulingShiftDayResponse>> BuildWeeklyShiftLookup(
        IReadOnlyCollection<TechnicianEntity> technicians,
        IReadOnlyCollection<TechnicianShift> storedShifts,
        IReadOnlyCollection<BusinessHourConfiguration> businessHours)
    {
        var shiftLookup = storedShifts
            .Where(shift => shift.TechnicianId > 0)
            .GroupBy(shift => shift.TechnicianId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyCollection<SchedulingShiftDayResponse>)BuildShiftDays(group.ToArray(), businessHours));

        foreach (var technician in technicians)
        {
            if (!shiftLookup.ContainsKey(technician.TechnicianId))
            {
                shiftLookup[technician.TechnicianId] = BuildShiftDays(Array.Empty<TechnicianShift>(), businessHours);
            }
        }

        return shiftLookup;
    }

    public static SchedulingTechnicianResponse ToSchedulingTechnician(
        TechnicianEntity technician,
        DateOnly referenceDate,
        IReadOnlyDictionary<long, IReadOnlyCollection<SchedulingShiftDayResponse>> shiftsByTechnician)
    {
        var baseItem = TechnicianManagementSupport.ToListItem(technician, referenceDate);

        return new SchedulingTechnicianResponse(
            baseItem.TechnicianId,
            baseItem.TechnicianCode,
            baseItem.TechnicianName,
            baseItem.AvailabilityStatus,
            baseItem.BaseZoneName,
            baseItem.Zones,
            baseItem.Skills.Select(skill => skill.SkillName).ToArray(),
            baseItem.AverageRating,
            baseItem.TodayJobCount,
            baseItem.NextFreeSlot,
            shiftsByTechnician.TryGetValue(technician.TechnicianId, out var days) ? days : Array.Empty<SchedulingShiftDayResponse>());
    }

    public static SchedulingShiftResponse ToSchedulingShift(
        TechnicianEntity technician,
        IReadOnlyDictionary<long, IReadOnlyCollection<SchedulingShiftDayResponse>> shiftsByTechnician)
    {
        return new SchedulingShiftResponse(
            technician.TechnicianId,
            technician.TechnicianCode,
            technician.TechnicianName,
            shiftsByTechnician.TryGetValue(technician.TechnicianId, out var days) ? days : Array.Empty<SchedulingShiftDayResponse>());
    }

    public static IReadOnlyCollection<SchedulingTimeSlotResponse> BuildTimeSlots(
        IReadOnlyCollection<SlotAvailability> slots,
        IReadOnlyCollection<ServiceRequestEntity> boardJobs)
    {
        var slotResponses = slots
            .Where(slot => slot.SlotConfiguration is not null)
            .GroupBy(slot => new
            {
                slot.SlotConfiguration!.StartTime,
                slot.SlotConfiguration.EndTime,
                slot.SlotConfiguration.SlotLabel
            })
            .Select(group => new SchedulingTimeSlotResponse(
                group.Key.StartTime.ToString("HH\\:mm", CultureInfo.InvariantCulture),
                group.Key.SlotLabel,
                group.Key.StartTime.ToString("HH\\:mm", CultureInfo.InvariantCulture),
                group.Key.EndTime.ToString("HH\\:mm", CultureInfo.InvariantCulture)))
            .OrderBy(slot => slot.StartTime)
            .ToArray();

        if (slotResponses.Length > 0)
        {
            return slotResponses;
        }

        return boardJobs
            .Select(serviceRequest =>
            {
                var slot = serviceRequest.Booking?.SlotAvailability;
                var config = slot?.SlotConfiguration;
                if (slot is null || config is null)
                {
                    return null;
                }

                return new SchedulingTimeSlotResponse(
                    config.StartTime.ToString("HH\\:mm", CultureInfo.InvariantCulture),
                    config.SlotLabel,
                    config.StartTime.ToString("HH\\:mm", CultureInfo.InvariantCulture),
                    config.EndTime.ToString("HH\\:mm", CultureInfo.InvariantCulture));
            })
            .Where(item => item is not null)
            .DistinctBy(item => item!.SlotKey)
            .Select(item => item!)
            .OrderBy(item => item.StartTime)
            .ToArray();
    }

    public static bool ShouldRenderOnBoard(ServiceRequestEntity serviceRequest)
    {
        return serviceRequest.CurrentStatus != ServiceRequestStatus.Cancelled &&
            serviceRequest.Booking?.SlotAvailability?.SlotConfiguration is not null;
    }

    public static ServiceRequestAssignment? GetActiveAssignment(ServiceRequestEntity serviceRequest)
    {
        return serviceRequest.Assignments
            .Where(assignment => assignment.IsActiveAssignment && !assignment.IsDeleted)
            .OrderByDescending(assignment => assignment.AssignedDateUtc)
            .FirstOrDefault();
    }

    public static SchedulingBoardJobResponse ToBoardJob(ServiceRequestEntity serviceRequest)
    {
        var booking = serviceRequest.Booking
            ?? throw new AppException(ErrorCodes.Conflict, "The service request is missing booking details.", 409);
        var slot = booking.SlotAvailability
            ?? throw new AppException(ErrorCodes.Conflict, "The service request is missing slot details.", 409);
        var config = slot.SlotConfiguration
            ?? throw new AppException(ErrorCodes.Conflict, "The service request is missing slot configuration details.", 409);
        var line = booking.BookingLines
            .Where(item => !item.IsDeleted)
            .OrderBy(item => item.BookingLineId)
            .FirstOrDefault();
        var assignment = GetActiveAssignment(serviceRequest);

        return new SchedulingBoardJobResponse(
            serviceRequest.ServiceRequestId,
            serviceRequest.ServiceRequestNumber,
            booking.BookingId,
            booking.ZoneId,
            slot.Zone?.ZoneName ?? booking.ZoneNameSnapshot,
            booking.Customer?.CustomerName ?? booking.CustomerNameSnapshot,
            booking.Customer?.MobileNumber ?? booking.MobileNumberSnapshot,
            BuildAddressSummary(booking),
            line?.Service?.ServiceName ?? booking.ServiceNameSnapshot,
            line?.AcType?.AcTypeName,
            line?.Brand?.BrandName,
            OperationsDashboardLiveSupport.ResolvePriority(serviceRequest),
            MapStatus(serviceRequest.CurrentStatus),
            slot.SlotAvailabilityId,
            slot.SlotDate,
            config.SlotLabel,
            config.StartTime.ToString("HH\\:mm", CultureInfo.InvariantCulture),
            config.EndTime.ToString("HH\\:mm", CultureInfo.InvariantCulture),
            (int)Math.Max((config.EndTime - config.StartTime).TotalMinutes, 0),
            assignment?.TechnicianId,
            assignment?.Technician?.TechnicianName,
            booking.EstimatedPrice);
    }

    public static SchedulingSlotResponse ToSchedulingSlot(SlotAvailability slot)
    {
        return new SchedulingSlotResponse(
            slot.SlotAvailabilityId,
            slot.ZoneId,
            slot.Zone?.ZoneName ?? string.Empty,
            slot.SlotDate,
            slot.SlotConfiguration?.SlotLabel ?? "Preferred Slot",
            slot.SlotConfiguration?.StartTime.ToString("HH\\:mm", CultureInfo.InvariantCulture) ?? string.Empty,
            slot.SlotConfiguration?.EndTime.ToString("HH\\:mm", CultureInfo.InvariantCulture) ?? string.Empty,
            slot.AvailableCapacity,
            slot.ReservedCapacity,
            slot.IsBlocked,
            !slot.IsBlocked && slot.ReservedCapacity < slot.AvailableCapacity);
    }

    public static SchedulingAmcAutoVisitResponse ToSchedulingAmcAutoVisit(AmcVisitSchedule visit)
    {
        var sourceBooking = visit.CustomerAmc?.JobCard?.ServiceRequest?.Booking;
        var sourceLine = sourceBooking?.BookingLines
            .Where(line => !line.IsDeleted)
            .OrderBy(line => line.BookingLineId)
            .FirstOrDefault();
        var customerAddress = sourceBooking?.CustomerAddress
            ?? throw new AppException(ErrorCodes.Conflict, "The AMC visit is missing source booking address details.", 409);

        return new SchedulingAmcAutoVisitResponse(
            visit.AmcVisitScheduleId,
            visit.CustomerAmcId,
            visit.VisitNumber,
            visit.ScheduledDate,
            visit.CurrentStatus.ToString(),
            visit.CustomerAmc?.CustomerId ?? 0,
            visit.CustomerAmc?.Customer?.CustomerName ?? string.Empty,
            visit.CustomerAmc?.Customer?.MobileNumber ?? string.Empty,
            customerAddress.CustomerAddressId,
            customerAddress.ZoneId,
            customerAddress.Zone?.ZoneName ?? sourceBooking?.ZoneNameSnapshot ?? string.Empty,
            BuildAddressSummary(sourceBooking),
            sourceLine?.ServiceId ?? 0,
            sourceLine?.Service?.ServiceName ?? sourceBooking?.ServiceNameSnapshot ?? string.Empty,
            sourceLine?.AcType?.AcTypeName,
            sourceLine?.Brand?.BrandName,
            visit.CustomerAmc?.JobCard?.JobCardNumber ?? string.Empty,
            visit.CustomerAmc?.JobCard?.ServiceRequest?.ServiceRequestNumber,
            visit.CustomerAmc?.AmcPlan?.PlanName ?? string.Empty,
            visit.ServiceRequestId,
            visit.ServiceRequest?.ServiceRequestNumber);
    }

    public static SchedulingDaySheetItemResponse ToDaySheetItem(ServiceRequestEntity serviceRequest)
    {
        var booking = serviceRequest.Booking!;
        var slot = booking.SlotAvailability!;
        var config = slot.SlotConfiguration!;
        var line = booking.BookingLines
            .Where(item => !item.IsDeleted)
            .OrderBy(item => item.BookingLineId)
            .FirstOrDefault();

        return new SchedulingDaySheetItemResponse(
            serviceRequest.ServiceRequestId,
            serviceRequest.ServiceRequestNumber,
            booking.Customer?.CustomerName ?? booking.CustomerNameSnapshot,
            booking.Customer?.MobileNumber ?? booking.MobileNumberSnapshot,
            BuildAddressSummary(booking),
            line?.Service?.ServiceName ?? booking.ServiceNameSnapshot,
            config.SlotLabel,
            config.StartTime.ToString("HH\\:mm", CultureInfo.InvariantCulture),
            config.EndTime.ToString("HH\\:mm", CultureInfo.InvariantCulture),
            MapStatus(serviceRequest.CurrentStatus),
            OperationsDashboardLiveSupport.ResolvePriority(serviceRequest),
            slot.Zone?.ZoneName ?? booking.ZoneNameSnapshot);
    }

    public static DateTime GetSlotStartTime(ServiceRequestEntity serviceRequest)
    {
        var slot = serviceRequest.Booking?.SlotAvailability;
        var startTime = slot?.SlotConfiguration?.StartTime ?? TimeOnly.MinValue;
        var slotDate = slot?.SlotDate ?? DateOnly.FromDateTime(serviceRequest.ServiceRequestDateUtc);
        return slotDate.ToDateTime(startTime, DateTimeKind.Utc);
    }

    public static async Task<IReadOnlyCollection<SchedulingConflictResponse>> BuildConflictsAsync(
        ServiceRequestEntity serviceRequest,
        TechnicianEntity technician,
        SlotAvailability slot,
        IReadOnlyCollection<TechnicianShift> storedShifts,
        IReadOnlyCollection<BusinessHourConfiguration> businessHours,
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository,
        CancellationToken cancellationToken)
    {
        var conflicts = new List<SchedulingConflictResponse>();
        var booking = serviceRequest.Booking
            ?? throw new AppException(ErrorCodes.Conflict, "The service request is missing booking details.", 409);
        var slotConfig = slot.SlotConfiguration
            ?? throw new AppException(ErrorCodes.Conflict, "The selected slot is missing slot configuration details.", 409);
        var targetStart = slot.SlotDate.ToDateTime(slotConfig.StartTime, DateTimeKind.Utc);
        var targetEnd = slot.SlotDate.ToDateTime(slotConfig.EndTime, DateTimeKind.Utc);
        var shiftsByTechnician = BuildWeeklyShiftLookup(new[] { technician }, storedShifts, businessHours);
        var weeklyShifts = shiftsByTechnician.TryGetValue(technician.TechnicianId, out var shiftDays)
            ? shiftDays
            : Array.Empty<SchedulingShiftDayResponse>();
        var shift = weeklyShifts.FirstOrDefault(day => day.DayOfWeekNumber == (int)targetStart.DayOfWeek);
        if (shift is null || shift.IsOffDuty)
        {
            conflicts.Add(new SchedulingConflictResponse(
                "shift",
                "error",
                $"{technician.TechnicianName} is off-duty on the selected day.",
                "Choose another working day or assign a different technician.",
                null,
                null));
        }
        else
        {
            var shiftStart = slot.SlotDate.ToDateTime(ParseNullableTime(shift.ShiftStartTime) ?? TimeOnly.MinValue, DateTimeKind.Utc);
            var shiftEnd = slot.SlotDate.ToDateTime(ParseNullableTime(shift.ShiftEndTime) ?? TimeOnly.MaxValue, DateTimeKind.Utc);
            if (targetStart < shiftStart || targetEnd > shiftEnd)
            {
                conflicts.Add(new SchedulingConflictResponse(
                    "shift",
                    "error",
                    $"The selected slot falls outside {technician.TechnicianName}'s shift hours.",
                    "Pick a slot inside the technician shift window.",
                    null,
                    null));
            }

            var breakStart = ParseNullableTime(shift.BreakStartTime);
            var breakEnd = ParseNullableTime(shift.BreakEndTime);
            if (breakStart.HasValue && breakEnd.HasValue)
            {
                var breakStartUtc = slot.SlotDate.ToDateTime(breakStart.Value, DateTimeKind.Utc);
                var breakEndUtc = slot.SlotDate.ToDateTime(breakEnd.Value, DateTimeKind.Utc);
                if (targetStart < breakEndUtc && targetEnd > breakStartUtc)
                {
                    conflicts.Add(new SchedulingConflictResponse(
                        "shift",
                        "error",
                        $"The selected slot overlaps {technician.TechnicianName}'s break window.",
                        "Choose a non-break slot or update the roster.",
                        null,
                        null));
                }
            }
        }

        var availabilitySnapshots = await technicianRepository.GetAvailabilityByServiceRequestIdAsync(serviceRequest.ServiceRequestId, cancellationToken);
        var technicianSnapshot = availabilitySnapshots.FirstOrDefault(snapshot => snapshot.TechnicianId == technician.TechnicianId);
        if (technicianSnapshot is not null && !technicianSnapshot.IsSkillMatched)
        {
            conflicts.Add(new SchedulingConflictResponse(
                "skill",
                "warning",
                $"{technician.TechnicianName} is not marked as a full skill match for this service request.",
                "Use a certified technician or accept the skill-gap override.",
                null,
                null));
        }

        var zoneIds = technician.Zones
            .Where(zone => !zone.IsDeleted)
            .Select(zone => zone.ZoneId)
            .ToHashSet();
        if (zoneIds.Count > 0 && !zoneIds.Contains(booking.ZoneId))
        {
            conflicts.Add(new SchedulingConflictResponse(
                "zone",
                "warning",
                $"{technician.TechnicianName} is not assigned to the job zone.",
                "Consider a technician mapped to the service zone.",
                null,
                null));
        }

        var assignedCount = await serviceRequestRepository.CountAssignedJobsAsync(
            technician.TechnicianId,
            currentStatus: null,
            slotDate: slot.SlotDate,
            cancellationToken: cancellationToken);
        if (assignedCount > 0)
        {
            var assignedJobs = await serviceRequestRepository.SearchAssignedJobsAsync(
                technician.TechnicianId,
                currentStatus: null,
                slotDate: slot.SlotDate,
                pageNumber: 1,
                pageSize: assignedCount,
                cancellationToken: cancellationToken);
            var comparableJobs = assignedJobs
                .Where(job => job.ServiceRequestId != serviceRequest.ServiceRequestId)
                .Select(job => new
                {
                    Job = job,
                    Start = GetSlotStartTime(job),
                    End = GetSlotEndTime(job)
                })
                .OrderBy(item => item.Start)
                .ToArray();

            foreach (var comparable in comparableJobs)
            {
                if (targetStart < comparable.End && targetEnd > comparable.Start)
                {
                    conflicts.Add(new SchedulingConflictResponse(
                        "overlap",
                        "error",
                        $"Overlaps with {comparable.Job.ServiceRequestNumber}.",
                        "Select a different slot or move the conflicting job first.",
                        comparable.Job.ServiceRequestId,
                        comparable.Job.ServiceRequestNumber));
                }
            }

            var previousJob = comparableJobs
                .Where(item => item.End <= targetStart)
                .OrderByDescending(item => item.End)
                .FirstOrDefault();
            var nextJob = comparableJobs
                .Where(item => item.Start >= targetEnd)
                .OrderBy(item => item.Start)
                .FirstOrDefault();

            if (previousJob is not null)
            {
                var gapMinutes = (targetStart - previousJob.End).TotalMinutes;
                var travelMinutes = EstimateTravelMinutes(previousJob.Job.Booking, booking);
                if (gapMinutes < travelMinutes)
                {
                    conflicts.Add(new SchedulingConflictResponse(
                        "travel",
                        "warning",
                        $"Travel buffer after {previousJob.Job.ServiceRequestNumber} is only {Math.Max((int)Math.Round(gapMinutes), 0)} minutes.",
                        $"Allow at least {travelMinutes} minutes between jobs.",
                        previousJob.Job.ServiceRequestId,
                        previousJob.Job.ServiceRequestNumber));
                }
            }

            if (nextJob is not null)
            {
                var gapMinutes = (nextJob.Start - targetEnd).TotalMinutes;
                var travelMinutes = EstimateTravelMinutes(booking, nextJob.Job.Booking);
                if (gapMinutes < travelMinutes)
                {
                    conflicts.Add(new SchedulingConflictResponse(
                        "travel",
                        "warning",
                        $"Travel buffer before {nextJob.Job.ServiceRequestNumber} is only {Math.Max((int)Math.Round(gapMinutes), 0)} minutes.",
                        $"Allow at least {travelMinutes} minutes between jobs.",
                        nextJob.Job.ServiceRequestId,
                        nextJob.Job.ServiceRequestNumber));
                }
            }
        }

        return conflicts;
    }

    public static async Task EnsureSlotAndConflictsAreValidAsync(
        ServiceRequestEntity serviceRequest,
        TechnicianEntity technician,
        SlotAvailability slot,
        ISchedulingRepository schedulingRepository,
        IAdminConfigurationRepository adminConfigurationRepository,
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository,
        CancellationToken cancellationToken)
    {
        ValidateSlotSelection(serviceRequest.Booking!, slot, allowCurrentSlotReuse: true);
        var businessHours = await adminConfigurationRepository.GetBusinessHoursAsync(cancellationToken);
        var storedShifts = await schedulingRepository.GetTechnicianShiftsAsync(technician.TechnicianId, asNoTracking: true, cancellationToken);
        var conflicts = await BuildConflictsAsync(
            serviceRequest,
            technician,
            slot,
            storedShifts,
            businessHours,
            serviceRequestRepository,
            technicianRepository,
            cancellationToken);
        ThrowIfBlockingConflictsExist(conflicts);
    }

    public static void ThrowIfBlockingConflictsExist(IReadOnlyCollection<SchedulingConflictResponse> conflicts)
    {
        var blocking = conflicts.FirstOrDefault(conflict =>
            BlockingConflictTypes.Contains(conflict.ConflictType, StringComparer.OrdinalIgnoreCase) &&
            conflict.Severity.Equals("error", StringComparison.OrdinalIgnoreCase));
        if (blocking is not null)
        {
            throw new AppException(ErrorCodes.Conflict, blocking.Message, 409);
        }
    }

    public static async Task EnsureTechnicianCapacityAsync(
        ServiceRequestEntity serviceRequest,
        TechnicianEntity technician,
        SlotAvailability slot,
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository,
        ServiceRequestAssignment? ignoreCurrentAssignment,
        int provisionalAssignments,
        CancellationToken cancellationToken)
    {
        if (!technician.IsActive)
        {
            throw new AppException(ErrorCodes.TechnicianInactive, "The selected technician is inactive.", 409);
        }

        var availabilityEntry = await technicianRepository.GetAvailabilityEntryForUpdateAsync(technician.TechnicianId, slot.SlotDate, cancellationToken);
        var availableSlotCount = availabilityEntry?.AvailableSlotCount ?? technician.MaxDailyAssignments;
        var assignedCount = await serviceRequestRepository.CountAssignedJobsAsync(
            technician.TechnicianId,
            currentStatus: null,
            slotDate: slot.SlotDate,
            cancellationToken: cancellationToken);

        if (ignoreCurrentAssignment is not null &&
            ignoreCurrentAssignment.TechnicianId == technician.TechnicianId &&
            serviceRequest.Booking?.SlotAvailability?.SlotDate == slot.SlotDate &&
            assignedCount > 0)
        {
            assignedCount -= 1;
        }

        assignedCount += Math.Max(provisionalAssignments, 0);

        if (assignedCount >= availableSlotCount)
        {
            throw new AppException(
                ErrorCodes.TechnicianUnavailable,
                "The selected technician has no remaining capacity for the chosen date.",
                409);
        }
    }

    public static async Task UpdateAvailabilityAsync(
        TechnicianEntity technician,
        DateOnly slotDate,
        int delta,
        string actor,
        DateTime now,
        string ipAddress,
        ITechnicianRepository technicianRepository,
        CancellationToken cancellationToken)
    {
        var availabilityEntry = await technicianRepository.GetAvailabilityEntryForUpdateAsync(technician.TechnicianId, slotDate, cancellationToken);
        if (availabilityEntry is null)
        {
            technician.TechnicianAvailabilities.Add(new TechnicianAvailability
            {
                AvailableDate = slotDate,
                AvailableSlotCount = technician.MaxDailyAssignments,
                BookedAssignmentCount = Math.Max(delta, 0),
                IsAvailable = true,
                AvailabilityRemarks = "Auto-created from scheduling board.",
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = ipAddress
            });

            return;
        }

        availabilityEntry.BookedAssignmentCount = Math.Max(availabilityEntry.BookedAssignmentCount + delta, 0);
        availabilityEntry.UpdatedBy = actor;
        availabilityEntry.LastUpdated = now;
        availabilityEntry.IPAddress = ipAddress;
    }

    public static void EnsureSchedulingEditable(ServiceRequestEntity serviceRequest)
    {
        if (serviceRequest.CurrentStatus is ServiceRequestStatus.EnRoute or
            ServiceRequestStatus.Reached or
            ServiceRequestStatus.WorkStarted or
            ServiceRequestStatus.WorkInProgress or
            ServiceRequestStatus.WorkCompletedPendingSubmission or
            ServiceRequestStatus.SubmittedForClosure or
            ServiceRequestStatus.Cancelled)
        {
            throw new AppException(
                ErrorCodes.InvalidStatusTransition,
                "Scheduling changes are not allowed once the job has progressed beyond the assigned state.",
                409);
        }
    }

    public static void ApplySlotChange(
        BookingEntity booking,
        SlotAvailability nextSlot,
        string remarks,
        string actor,
        DateTime now,
        string ipAddress)
    {
        if (booking.SlotAvailabilityId != nextSlot.SlotAvailabilityId)
        {
            if (booking.SlotAvailability is not null && booking.SlotAvailability.ReservedCapacity > 0)
            {
                booking.SlotAvailability.ReservedCapacity -= 1;
            }

            nextSlot.ReservedCapacity += 1;
            booking.SlotAvailability = nextSlot;
            booking.SlotAvailabilityId = nextSlot.SlotAvailabilityId;
            booking.ZoneId = nextSlot.ZoneId;
            booking.ZoneNameSnapshot = nextSlot.Zone?.ZoneName ?? booking.ZoneNameSnapshot;
            booking.BookingStatusHistories.Add(new BookingStatusHistory
            {
                BookingStatus = BookingStatus.Confirmed,
                Remarks = remarks,
                StatusDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = ipAddress
            });
        }

        booking.UpdatedBy = actor;
        booking.LastUpdated = now;
        booking.IPAddress = ipAddress;
    }

    public static void ValidateSlotSelection(BookingEntity booking, SlotAvailability slot, bool allowCurrentSlotReuse)
    {
        if (booking.ZoneId != slot.ZoneId)
        {
            throw new AppException(ErrorCodes.ValidationFailure, "The selected slot belongs to a different zone.", 400);
        }

        if (slot.IsBlocked)
        {
            throw new AppException(ErrorCodes.SlotUnavailable, "The selected slot is blocked.", 409);
        }

        var isCurrentSlot = booking.SlotAvailabilityId == slot.SlotAvailabilityId;
        if ((!allowCurrentSlotReuse || !isCurrentSlot) &&
            slot.ReservedCapacity >= slot.AvailableCapacity)
        {
            throw new AppException(ErrorCodes.SlotUnavailable, "The selected slot has no remaining capacity.", 409);
        }
    }

    public static void ValidateShiftRequest(ScheduleShiftDayRequest request)
    {
        if (request.DayOfWeekNumber < 0 || request.DayOfWeekNumber > 6)
        {
            throw new AppException(ErrorCodes.ValidationFailure, "Shift day-of-week is invalid.", 400);
        }

        if (request.IsOffDuty)
        {
            return;
        }

        var shiftStart = ParseNullableTime(request.ShiftStartTime)
            ?? throw new AppException(ErrorCodes.ValidationFailure, "Shift start time is required for working days.", 400);
        var shiftEnd = ParseNullableTime(request.ShiftEndTime)
            ?? throw new AppException(ErrorCodes.ValidationFailure, "Shift end time is required for working days.", 400);
        if (shiftStart >= shiftEnd)
        {
            throw new AppException(ErrorCodes.ValidationFailure, "Shift end time must be later than shift start time.", 400);
        }

        var breakStart = ParseNullableTime(request.BreakStartTime);
        var breakEnd = ParseNullableTime(request.BreakEndTime);
        if (breakStart.HasValue != breakEnd.HasValue)
        {
            throw new AppException(ErrorCodes.ValidationFailure, "Break start and end times must be provided together.", 400);
        }

        if (breakStart.HasValue && breakEnd.HasValue)
        {
            if (breakStart.Value >= breakEnd.Value)
            {
                throw new AppException(ErrorCodes.ValidationFailure, "Break end time must be later than break start time.", 400);
            }

            if (breakStart.Value < shiftStart || breakEnd.Value > shiftEnd)
            {
                throw new AppException(ErrorCodes.ValidationFailure, "Break window must remain inside the shift window.", 400);
            }
        }
    }

    public static TimeOnly? ParseNullableTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (TimeOnly.TryParseExact(value.Trim(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        throw new AppException(ErrorCodes.ValidationFailure, $"Invalid time value '{value}'.", 400);
    }

    private static IReadOnlyCollection<SchedulingShiftDayResponse> BuildShiftDays(
        IReadOnlyCollection<TechnicianShift> technicianShifts,
        IReadOnlyCollection<BusinessHourConfiguration> businessHours)
    {
        var shiftLookup = technicianShifts.ToDictionary(shift => shift.DayOfWeekNumber);
        var businessHourLookup = businessHours.ToDictionary(hour => hour.DayOfWeekNumber);
        var dayNames = CultureInfo.InvariantCulture.DateTimeFormat.DayNames;
        var days = new List<SchedulingShiftDayResponse>(7);

        for (var day = 0; day < 7; day++)
        {
            if (shiftLookup.TryGetValue(day, out var shift))
            {
                days.Add(new SchedulingShiftDayResponse(
                    day,
                    dayNames[day],
                    shift.IsOffDuty,
                    shift.ShiftStartTimeLocal?.ToString("HH\\:mm", CultureInfo.InvariantCulture),
                    shift.ShiftEndTimeLocal?.ToString("HH\\:mm", CultureInfo.InvariantCulture),
                    shift.BreakStartTimeLocal?.ToString("HH\\:mm", CultureInfo.InvariantCulture),
                    shift.BreakEndTimeLocal?.ToString("HH\\:mm", CultureInfo.InvariantCulture)));
                continue;
            }

            if (businessHourLookup.TryGetValue(day, out var businessHour))
            {
                days.Add(new SchedulingShiftDayResponse(
                    day,
                    dayNames[day],
                    businessHour.IsClosed,
                    businessHour.StartTimeLocal?.ToString("HH\\:mm", CultureInfo.InvariantCulture),
                    businessHour.EndTimeLocal?.ToString("HH\\:mm", CultureInfo.InvariantCulture),
                    null,
                    null));
                continue;
            }

            days.Add(new SchedulingShiftDayResponse(day, dayNames[day], true, null, null, null, null));
        }

        return days;
    }

    private static string BuildAddressSummary(BookingEntity? booking)
    {
        if (booking is null)
        {
            return string.Empty;
        }

        return string.Join(
            ", ",
            new[]
            {
                booking.CustomerAddress?.AddressLine1 ?? booking.AddressLine1Snapshot,
                booking.CustomerAddress?.AddressLine2 ?? booking.AddressLine2Snapshot,
                booking.CustomerAddress?.Landmark ?? booking.LandmarkSnapshot,
                booking.CustomerAddress?.CityName ?? booking.CityNameSnapshot,
                booking.CustomerAddress?.Pincode ?? booking.PincodeSnapshot
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static DateTime GetSlotEndTime(ServiceRequestEntity serviceRequest)
    {
        var slot = serviceRequest.Booking?.SlotAvailability;
        var endTime = slot?.SlotConfiguration?.EndTime ?? TimeOnly.MinValue;
        var slotDate = slot?.SlotDate ?? DateOnly.FromDateTime(serviceRequest.ServiceRequestDateUtc);
        return slotDate.ToDateTime(endTime, DateTimeKind.Utc);
    }

    private static int EstimateTravelMinutes(BookingEntity? fromBooking, BookingEntity? toBooking)
    {
        var fromAddress = fromBooking?.CustomerAddress;
        var toAddress = toBooking?.CustomerAddress;
        if (fromAddress?.Latitude is double fromLat &&
            fromAddress.Longitude is double fromLng &&
            toAddress?.Latitude is double toLat &&
            toAddress.Longitude is double toLng)
        {
            var distanceKm = CalculateDistanceKm(fromLat, fromLng, toLat, toLng);
            return Math.Max((int)Math.Ceiling((distanceKm / 30d) * 60d) + 10, 15);
        }

        return fromBooking?.ZoneId == toBooking?.ZoneId ? 15 : 30;
    }

    private static double CalculateDistanceKm(double fromLat, double fromLng, double toLat, double toLng)
    {
        const double earthRadiusKm = 6371d;
        var dLat = DegreesToRadians(toLat - fromLat);
        var dLng = DegreesToRadians(toLng - fromLng);
        var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                Math.Cos(DegreesToRadians(fromLat)) *
                Math.Cos(DegreesToRadians(toLat)) *
                Math.Pow(Math.Sin(dLng / 2), 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180d;
    }

    private static string MapStatus(ServiceRequestStatus status)
    {
        return status switch
        {
            ServiceRequestStatus.New => "pending",
            ServiceRequestStatus.Assigned => "assigned",
            ServiceRequestStatus.EnRoute => "en-route",
            ServiceRequestStatus.Reached => "arrived",
            ServiceRequestStatus.WorkStarted => "in-progress",
            ServiceRequestStatus.WorkInProgress => "in-progress",
            ServiceRequestStatus.WorkCompletedPendingSubmission => "completed",
            ServiceRequestStatus.SubmittedForClosure => "closed",
            ServiceRequestStatus.Rescheduled => "pending",
            ServiceRequestStatus.NoShow => "pending",
            ServiceRequestStatus.CustomerAbsent => "pending",
            _ => "cancelled"
        };
    }
}
