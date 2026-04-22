using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.Technician.Management;
using Coolzo.Application.Features.TechnicianJob;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using MediatR;
using DomainServiceRequest = Coolzo.Domain.Entities.ServiceRequest;
using DomainTechnician = Coolzo.Domain.Entities.Technician;

namespace Coolzo.Application.Features.OperationsDashboard;

public sealed record GetOperationsDashboardQuery() : IRequest<OperationsDashboardResponse>;

public sealed class GetOperationsDashboardQueryHandler : IRequestHandler<GetOperationsDashboardQuery, OperationsDashboardResponse>
{
    private readonly IGapPhaseARepository _gapPhaseARepository;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly ITechnicianRepository _technicianRepository;

    public GetOperationsDashboardQueryHandler(
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository,
        IGapPhaseARepository gapPhaseARepository)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _technicianRepository = technicianRepository;
        _gapPhaseARepository = gapPhaseARepository;
    }

    public async Task<OperationsDashboardResponse> Handle(GetOperationsDashboardQuery request, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(utcNow);
        var serviceRequests = await OperationsDashboardLiveSupport.GetAllServiceRequestsAsync(_serviceRequestRepository, cancellationToken);
        var technicians = await OperationsDashboardLiveSupport.GetActiveTechniciansAsync(_technicianRepository, cancellationToken);
        var alerts = await OperationsDashboardLiveSupport.GetDashboardAlertsAsync(_gapPhaseARepository, utcNow, cancellationToken);

        var todayRequests = serviceRequests
            .Where(serviceRequest => OperationsDashboardLiveSupport.GetOperationalDate(serviceRequest) == today)
            .ToArray();
        var todayPendingQueueCount = todayRequests.Count(OperationsDashboardLiveSupport.IsPendingQueueItem);
        var breachedServiceRequestIds = OperationsDashboardLiveSupport.GetAlertRelatedServiceRequestIds(
            alerts.Where(alert => OperationsDashboardLiveSupport.IsBreachedAlert(alert, utcNow)));
        var atRiskServiceRequestIds = OperationsDashboardLiveSupport.GetAlertRelatedServiceRequestIds(
            alerts.Where(alert => OperationsDashboardLiveSupport.IsAtRiskAlert(alert, utcNow)));
        var todayServiceRequestIds = todayRequests.Select(serviceRequest => serviceRequest.ServiceRequestId).ToHashSet();

        var breachedCount = breachedServiceRequestIds.Count(serviceRequestId => todayServiceRequestIds.Contains(serviceRequestId));
        var atRiskCount = atRiskServiceRequestIds.Count(serviceRequestId => todayServiceRequestIds.Contains(serviceRequestId));

        return new OperationsDashboardResponse(
            todayRequests.Length,
            todayPendingQueueCount,
            todayRequests.Count(serviceRequest => serviceRequest.CurrentStatus == ServiceRequestStatus.Assigned),
            todayRequests.Count(serviceRequest => OperationsDashboardLiveSupport.IsInProgressStatus(serviceRequest.CurrentStatus)),
            todayRequests.Count(serviceRequest => OperationsDashboardLiveSupport.IsCompletedOperationalStatus(serviceRequest.CurrentStatus)),
            technicians.Count,
            atRiskCount,
            breachedCount,
            OperationsDashboardLiveSupport.CalculateSlaCompliancePercent(todayRequests.Length, breachedCount),
            utcNow);
    }
}

public sealed record GetOperationsPendingQueueQuery() : IRequest<IReadOnlyCollection<OperationsPendingQueueItemResponse>>;

public sealed class GetOperationsPendingQueueQueryHandler : IRequestHandler<GetOperationsPendingQueueQuery, IReadOnlyCollection<OperationsPendingQueueItemResponse>>
{
    private readonly IServiceRequestRepository _serviceRequestRepository;

    public GetOperationsPendingQueueQueryHandler(IServiceRequestRepository serviceRequestRepository)
    {
        _serviceRequestRepository = serviceRequestRepository;
    }

    public async Task<IReadOnlyCollection<OperationsPendingQueueItemResponse>> Handle(GetOperationsPendingQueueQuery request, CancellationToken cancellationToken)
    {
        var serviceRequests = await OperationsDashboardLiveSupport.GetAllServiceRequestsAsync(_serviceRequestRepository, cancellationToken);

        return serviceRequests
            .Where(OperationsDashboardLiveSupport.IsPendingQueueItem)
            .OrderBy(serviceRequest => OperationsDashboardLiveSupport.GetPriorityRank(serviceRequest))
            .ThenBy(serviceRequest => OperationsDashboardLiveSupport.GetOperationalDate(serviceRequest))
            .ThenBy(serviceRequest => serviceRequest.ServiceRequestDateUtc)
            .Select(OperationsDashboardLiveSupport.ToPendingQueueItem)
            .ToArray();
    }
}

public sealed record GetOperationsTechnicianStatusQuery() : IRequest<IReadOnlyCollection<OperationsTechnicianStatusItemResponse>>;

public sealed class GetOperationsTechnicianStatusQueryHandler : IRequestHandler<GetOperationsTechnicianStatusQuery, IReadOnlyCollection<OperationsTechnicianStatusItemResponse>>
{
    private readonly ITechnicianRepository _technicianRepository;

    public GetOperationsTechnicianStatusQueryHandler(ITechnicianRepository technicianRepository)
    {
        _technicianRepository = technicianRepository;
    }

    public async Task<IReadOnlyCollection<OperationsTechnicianStatusItemResponse>> Handle(GetOperationsTechnicianStatusQuery request, CancellationToken cancellationToken)
    {
        var technicians = await OperationsDashboardLiveSupport.GetActiveTechniciansAsync(_technicianRepository, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return OperationsDashboardLiveSupport.BuildTechnicianStatusItems(technicians, today);
    }
}

public sealed record GetOperationsSlaAlertsQuery() : IRequest<IReadOnlyCollection<OperationsSlaAlertItemResponse>>;

public sealed class GetOperationsSlaAlertsQueryHandler : IRequestHandler<GetOperationsSlaAlertsQuery, IReadOnlyCollection<OperationsSlaAlertItemResponse>>
{
    private readonly IGapPhaseARepository _gapPhaseARepository;
    private readonly IServiceRequestRepository _serviceRequestRepository;

    public GetOperationsSlaAlertsQueryHandler(
        IServiceRequestRepository serviceRequestRepository,
        IGapPhaseARepository gapPhaseARepository)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _gapPhaseARepository = gapPhaseARepository;
    }

    public async Task<IReadOnlyCollection<OperationsSlaAlertItemResponse>> Handle(GetOperationsSlaAlertsQuery request, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var serviceRequests = await OperationsDashboardLiveSupport.GetAllServiceRequestsAsync(_serviceRequestRepository, cancellationToken);
        var serviceRequestLookup = serviceRequests.ToDictionary(entity => entity.ServiceRequestId);
        var alerts = await OperationsDashboardLiveSupport.GetDashboardAlertsAsync(_gapPhaseARepository, utcNow, cancellationToken);

        return alerts
            .OrderBy(alert => OperationsDashboardLiveSupport.IsBreachedAlert(alert, utcNow) ? 0 : 1)
            .ThenBy(alert => alert.SlaDueDateUtc ?? DateTime.MaxValue)
            .ThenByDescending(alert => alert.Severity)
            .Select(alert => OperationsDashboardLiveSupport.ToSlaAlertItem(alert, serviceRequestLookup, utcNow))
            .ToArray();
    }
}

public sealed record GetOperationsZoneWorkloadQuery() : IRequest<IReadOnlyCollection<OperationsZoneWorkloadItemResponse>>;

public sealed class GetOperationsZoneWorkloadQueryHandler : IRequestHandler<GetOperationsZoneWorkloadQuery, IReadOnlyCollection<OperationsZoneWorkloadItemResponse>>
{
    private readonly IGapPhaseARepository _gapPhaseARepository;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly ITechnicianRepository _technicianRepository;

    public GetOperationsZoneWorkloadQueryHandler(
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository,
        IGapPhaseARepository gapPhaseARepository)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _technicianRepository = technicianRepository;
        _gapPhaseARepository = gapPhaseARepository;
    }

    public async Task<IReadOnlyCollection<OperationsZoneWorkloadItemResponse>> Handle(GetOperationsZoneWorkloadQuery request, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(utcNow);
        var serviceRequests = await OperationsDashboardLiveSupport.GetAllServiceRequestsAsync(_serviceRequestRepository, cancellationToken);
        var technicians = await OperationsDashboardLiveSupport.GetActiveTechniciansAsync(_technicianRepository, cancellationToken);
        var technicianStatuses = OperationsDashboardLiveSupport.BuildTechnicianStatusItems(technicians, today);
        var todayRequests = serviceRequests
            .Where(serviceRequest => OperationsDashboardLiveSupport.GetOperationalDate(serviceRequest) == today)
            .ToArray();
        var breachedServiceRequestIds = OperationsDashboardLiveSupport.GetAlertRelatedServiceRequestIds(
            (await OperationsDashboardLiveSupport.GetDashboardAlertsAsync(_gapPhaseARepository, utcNow, cancellationToken))
            .Where(alert => OperationsDashboardLiveSupport.IsBreachedAlert(alert, utcNow)));

        return OperationsDashboardLiveSupport.BuildZoneWorkload(todayRequests, technicianStatuses, breachedServiceRequestIds);
    }
}

public sealed record GetOperationsDaySummaryQuery() : IRequest<OperationsDaySummaryResponse>;

public sealed class GetOperationsDaySummaryQueryHandler : IRequestHandler<GetOperationsDaySummaryQuery, OperationsDaySummaryResponse>
{
    private readonly IGapPhaseARepository _gapPhaseARepository;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly ITechnicianRepository _technicianRepository;

    public GetOperationsDaySummaryQueryHandler(
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository,
        IGapPhaseARepository gapPhaseARepository)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _technicianRepository = technicianRepository;
        _gapPhaseARepository = gapPhaseARepository;
    }

    public async Task<OperationsDaySummaryResponse> Handle(GetOperationsDaySummaryQuery request, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(utcNow);
        var serviceRequests = await OperationsDashboardLiveSupport.GetAllServiceRequestsAsync(_serviceRequestRepository, cancellationToken);
        var technicians = await OperationsDashboardLiveSupport.GetActiveTechniciansAsync(_technicianRepository, cancellationToken);
        var technicianStatuses = OperationsDashboardLiveSupport.BuildTechnicianStatusItems(technicians, today);
        var alerts = await OperationsDashboardLiveSupport.GetDashboardAlertsAsync(_gapPhaseARepository, utcNow, cancellationToken);

        var todayRequests = serviceRequests
            .Where(serviceRequest => OperationsDashboardLiveSupport.GetOperationalDate(serviceRequest) == today)
            .ToArray();
        var pendingQueue = todayRequests.Where(OperationsDashboardLiveSupport.IsPendingQueueItem).ToArray();
        var breachedServiceRequestIds = OperationsDashboardLiveSupport.GetAlertRelatedServiceRequestIds(
            alerts.Where(alert => OperationsDashboardLiveSupport.IsBreachedAlert(alert, utcNow)));
        var atRiskServiceRequestIds = OperationsDashboardLiveSupport.GetAlertRelatedServiceRequestIds(
            alerts.Where(alert => OperationsDashboardLiveSupport.IsAtRiskAlert(alert, utcNow)));
        var todayServiceRequestIds = todayRequests.Select(serviceRequest => serviceRequest.ServiceRequestId).ToHashSet();
        var breachedCount = breachedServiceRequestIds.Count(serviceRequestId => todayServiceRequestIds.Contains(serviceRequestId));
        var atRiskCount = atRiskServiceRequestIds.Count(serviceRequestId => todayServiceRequestIds.Contains(serviceRequestId));

        return new OperationsDaySummaryResponse(
            today,
            todayRequests.Length,
            pendingQueue.Length,
            todayRequests.Count(serviceRequest => serviceRequest.CurrentStatus == ServiceRequestStatus.Assigned),
            todayRequests.Count(serviceRequest => OperationsDashboardLiveSupport.IsInProgressStatus(serviceRequest.CurrentStatus)),
            todayRequests.Count(serviceRequest => OperationsDashboardLiveSupport.IsCompletedOperationalStatus(serviceRequest.CurrentStatus)),
            todayRequests.Count(serviceRequest => serviceRequest.CurrentStatus == ServiceRequestStatus.SubmittedForClosure),
            todayRequests.Count(serviceRequest => !OperationsDashboardLiveSupport.IsCompletedOperationalStatus(serviceRequest.CurrentStatus) &&
                serviceRequest.CurrentStatus != ServiceRequestStatus.Cancelled),
            todayRequests.Count(serviceRequest => OperationsDashboardLiveSupport.ResolvePriority(serviceRequest) == "emergency"),
            breachedCount + atRiskCount,
            atRiskCount,
            breachedCount,
            technicians.Count,
            OperationsDashboardLiveSupport.CalculateSlaCompliancePercent(todayRequests.Length, breachedCount),
            OperationsDashboardLiveSupport.BuildZoneWorkload(todayRequests, technicianStatuses, breachedServiceRequestIds),
            utcNow);
    }
}

public sealed record GetOperationsLiveMapQuery() : IRequest<OperationsLiveMapResponse>;

public sealed class GetOperationsLiveMapQueryHandler : IRequestHandler<GetOperationsLiveMapQuery, OperationsLiveMapResponse>
{
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly ITechnicianRepository _technicianRepository;

    public GetOperationsLiveMapQueryHandler(
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _technicianRepository = technicianRepository;
    }

    public async Task<OperationsLiveMapResponse> Handle(GetOperationsLiveMapQuery request, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(utcNow);
        var serviceRequests = await OperationsDashboardLiveSupport.GetAllServiceRequestsAsync(_serviceRequestRepository, cancellationToken);
        var technicians = await OperationsDashboardLiveSupport.GetActiveTechniciansAsync(_technicianRepository, cancellationToken);

        var technicianGpsTasks = technicians
            .Select(async technician =>
            {
                var logs = await _technicianRepository.GetGpsLogsAsync(technician.TechnicianId, today, cancellationToken);
                return new
                {
                    Technician = technician,
                    Logs = logs
                };
            });

        var technicianGpsResults = await Task.WhenAll(technicianGpsTasks);

        var technicianPins = technicianGpsResults
            .Where(result => result.Logs.Count > 0)
            .Select(result => OperationsDashboardLiveSupport.ToTechnicianMapPin(result.Technician, result.Logs, today))
            .Where(pin => pin is not null)
            .Select(pin => pin!)
            .OrderBy(pin => pin.TechnicianName)
            .ToArray();

        var serviceRequestPins = serviceRequests
            .Where(serviceRequest => OperationsDashboardLiveSupport.ShouldRenderOnLiveMap(serviceRequest, today))
            .Select(OperationsDashboardLiveSupport.ToServiceRequestMapPin)
            .Where(pin => pin is not null)
            .Select(pin => pin!)
            .OrderBy(pin => OperationsDashboardLiveSupport.GetPriorityRank(pin.Priority))
            .ThenBy(pin => pin.ZoneName)
            .ThenBy(pin => pin.ServiceRequestNumber)
            .ToArray();

        return new OperationsLiveMapResponse(
            utcNow,
            technicianPins,
            serviceRequestPins);
    }
}

internal static class OperationsDashboardLiveSupport
{
    private static readonly TimeSpan AlertLeadWindow = TimeSpan.FromHours(1);

    public static async Task<IReadOnlyCollection<DomainServiceRequest>> GetAllServiceRequestsAsync(
        IServiceRequestRepository serviceRequestRepository,
        CancellationToken cancellationToken)
    {
        var totalCount = await serviceRequestRepository.CountSearchAsync(
            bookingId: null,
            serviceId: null,
            currentStatus: null,
            slotDate: null,
            cancellationToken: cancellationToken);

        if (totalCount <= 0)
        {
            return Array.Empty<DomainServiceRequest>();
        }

        return await serviceRequestRepository.SearchAsync(
            bookingId: null,
            serviceId: null,
            currentStatus: null,
            slotDate: null,
            pageNumber: 1,
            pageSize: totalCount,
            cancellationToken: cancellationToken);
    }

    public static Task<IReadOnlyCollection<DomainTechnician>> GetActiveTechniciansAsync(
        ITechnicianRepository technicianRepository,
        CancellationToken cancellationToken)
    {
        return technicianRepository.SearchManagementAsync(
            searchTerm: null,
            activeOnly: true,
            zoneName: null,
            skillName: null,
            availability: null,
            minimumRating: null,
            cancellationToken: cancellationToken);
    }

    public static async Task<IReadOnlyCollection<SystemAlert>> GetDashboardAlertsAsync(
        IGapPhaseARepository gapPhaseARepository,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        return (await gapPhaseARepository.GetOpenAlertsAsync("ServiceRequest", cancellationToken))
            .Where(alert => alert.SlaDueDateUtc.HasValue && alert.SlaDueDateUtc.Value <= utcNow.Add(AlertLeadWindow))
            .ToArray();
    }

    public static DateOnly GetOperationalDate(DomainServiceRequest serviceRequest)
    {
        return serviceRequest.Booking?.SlotAvailability?.SlotDate ?? DateOnly.FromDateTime(serviceRequest.ServiceRequestDateUtc);
    }

    public static string ResolvePriority(DomainServiceRequest serviceRequest)
    {
        if (serviceRequest.Booking?.IsEmergency == true)
        {
            return "emergency";
        }

        var issueNotes = serviceRequest.Booking?.BookingLines
            .OrderBy(line => line.BookingLineId)
            .Select(line => line.IssueNotes)
            .FirstOrDefault() ?? string.Empty;

        if (issueNotes.Contains("[Priority: Emergency]", StringComparison.OrdinalIgnoreCase))
        {
            return "emergency";
        }

        if (issueNotes.Contains("[Priority: Urgent]", StringComparison.OrdinalIgnoreCase))
        {
            return "urgent";
        }

        return "normal";
    }

    public static int GetPriorityRank(DomainServiceRequest serviceRequest)
    {
        return GetPriorityRank(ResolvePriority(serviceRequest));
    }

    public static int GetPriorityRank(string priority)
    {
        return priority.ToLowerInvariant() switch
        {
            "emergency" => 0,
            "urgent" => 1,
            _ => 2
        };
    }

    public static bool IsPendingQueueItem(DomainServiceRequest serviceRequest)
    {
        if (TechnicianJobResponseMapper.GetActiveAssignment(serviceRequest) is not null)
        {
            return false;
        }

        return serviceRequest.CurrentStatus is
            ServiceRequestStatus.New or
            ServiceRequestStatus.Assigned or
            ServiceRequestStatus.Rescheduled or
            ServiceRequestStatus.NoShow or
            ServiceRequestStatus.CustomerAbsent;
    }

    public static bool IsInProgressStatus(ServiceRequestStatus currentStatus)
    {
        return currentStatus is
            ServiceRequestStatus.EnRoute or
            ServiceRequestStatus.Reached or
            ServiceRequestStatus.WorkStarted or
            ServiceRequestStatus.WorkInProgress;
    }

    public static bool IsCompletedOperationalStatus(ServiceRequestStatus currentStatus)
    {
        return currentStatus is
            ServiceRequestStatus.WorkCompletedPendingSubmission or
            ServiceRequestStatus.SubmittedForClosure;
    }

    public static bool IsBreachedAlert(SystemAlert alert, DateTime utcNow)
    {
        return alert.SlaDueDateUtc.HasValue && alert.SlaDueDateUtc.Value <= utcNow;
    }

    public static bool IsAtRiskAlert(SystemAlert alert, DateTime utcNow)
    {
        return alert.SlaDueDateUtc.HasValue &&
            alert.SlaDueDateUtc.Value > utcNow &&
            alert.SlaDueDateUtc.Value <= utcNow.Add(AlertLeadWindow);
    }

    public static HashSet<long> GetAlertRelatedServiceRequestIds(IEnumerable<SystemAlert> alerts)
    {
        return alerts
            .Select(TryParseServiceRequestId)
            .Where(serviceRequestId => serviceRequestId.HasValue)
            .Select(serviceRequestId => serviceRequestId!.Value)
            .ToHashSet();
    }

    public static decimal CalculateSlaCompliancePercent(int totalCount, int breachedCount)
    {
        if (totalCount <= 0)
        {
            return 100m;
        }

        return Math.Round(Math.Max(0m, (totalCount - breachedCount) * 100m / totalCount), 2);
    }

    public static OperationsPendingQueueItemResponse ToPendingQueueItem(DomainServiceRequest serviceRequest)
    {
        var booking = serviceRequest.Booking;

        return new OperationsPendingQueueItemResponse(
            serviceRequest.ServiceRequestId,
            serviceRequest.ServiceRequestNumber,
            booking?.CustomerNameSnapshot ?? string.Empty,
            booking?.MobileNumberSnapshot ?? string.Empty,
            booking?.ZoneNameSnapshot ?? "Unassigned",
            ResolveServiceName(serviceRequest),
            TechnicianJobResponseMapper.BuildAddressSummary(booking),
            GetOperationalDate(serviceRequest),
            booking?.SlotAvailability?.SlotConfiguration?.SlotLabel ?? "Preferred Slot",
            ResolvePriority(serviceRequest),
            serviceRequest.CurrentStatus.ToString(),
            booking?.EstimatedPrice ?? 0m,
            serviceRequest.ServiceRequestDateUtc);
    }

    public static IReadOnlyCollection<OperationsTechnicianStatusItemResponse> BuildTechnicianStatusItems(
        IReadOnlyCollection<DomainTechnician> technicians,
        DateOnly today)
    {
        return technicians
            .Select(technician =>
            {
                var item = TechnicianManagementSupport.ToListItem(technician, today);

                return new OperationsTechnicianStatusItemResponse(
                    item.TechnicianId,
                    item.TechnicianCode,
                    item.TechnicianName,
                    item.MobileNumber,
                    item.EmailAddress,
                    item.AvailabilityStatus,
                    item.CurrentServiceRequestNumber,
                    item.BaseZoneName,
                    item.Zones,
                    item.Skills.Select(skill => skill.SkillName).ToArray(),
                    item.AverageRating,
                    item.TodayJobCount,
                    item.NextFreeSlot);
            })
            .OrderBy(item => GetTechnicianAvailabilityRank(item.AvailabilityStatus))
            .ThenBy(item => item.TechnicianName)
            .ToArray();
    }

    public static OperationsSlaAlertItemResponse ToSlaAlertItem(
        SystemAlert alert,
        IReadOnlyDictionary<long, DomainServiceRequest> serviceRequestLookup,
        DateTime utcNow)
    {
        var serviceRequestId = TryParseServiceRequestId(alert);
        var serviceRequest = serviceRequestId.HasValue && serviceRequestLookup.TryGetValue(serviceRequestId.Value, out var matchedServiceRequest)
            ? matchedServiceRequest
            : null;
        var activeAssignment = serviceRequest is null ? null : TechnicianJobResponseMapper.GetActiveAssignment(serviceRequest);
        var minutesFromDue = alert.SlaDueDateUtc.HasValue
            ? (int?)Math.Round((utcNow - alert.SlaDueDateUtc.Value).TotalMinutes, MidpointRounding.AwayFromZero)
            : null;

        return new OperationsSlaAlertItemResponse(
            alert.SystemAlertId,
            serviceRequestId,
            serviceRequest?.ServiceRequestNumber,
            serviceRequest?.Booking?.CustomerNameSnapshot ?? string.Empty,
            serviceRequest?.Booking?.ZoneNameSnapshot ?? "Unassigned",
            serviceRequest is null ? string.Empty : ResolveServiceName(serviceRequest),
            serviceRequest is null ? "normal" : ResolvePriority(serviceRequest),
            alert.AlertType,
            IsBreachedAlert(alert, utcNow) ? "breached" : "at-risk",
            alert.Severity.ToString().ToLowerInvariant(),
            alert.SlaDueDateUtc,
            minutesFromDue,
            alert.EscalationLevel,
            alert.AlertMessage,
            activeAssignment?.Technician?.TechnicianName);
    }

    public static IReadOnlyCollection<OperationsZoneWorkloadItemResponse> BuildZoneWorkload(
        IReadOnlyCollection<DomainServiceRequest> todayRequests,
        IReadOnlyCollection<OperationsTechnicianStatusItemResponse> technicianStatuses,
        IReadOnlySet<long> breachedServiceRequestIds)
    {
        var zoneNames = todayRequests
            .Select(ResolveZoneName)
            .Concat(technicianStatuses.SelectMany(status => status.Zones))
            .Where(zoneName => !string.IsNullOrWhiteSpace(zoneName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(zoneName => zoneName)
            .ToArray();

        return zoneNames
            .Select(zoneName =>
            {
                var zoneRequests = todayRequests
                    .Where(serviceRequest => string.Equals(ResolveZoneName(serviceRequest), zoneName, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                var zoneTechnicians = technicianStatuses
                    .Where(technician => technician.Zones.Any(zone => string.Equals(zone, zoneName, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();

                return new OperationsZoneWorkloadItemResponse(
                    zoneName,
                    zoneRequests.Length,
                    zoneRequests.Count(IsPendingQueueItem),
                    zoneRequests.Count(serviceRequest => serviceRequest.CurrentStatus == ServiceRequestStatus.Assigned),
                    zoneRequests.Count(serviceRequest => IsInProgressStatus(serviceRequest.CurrentStatus)),
                    zoneRequests.Count(serviceRequest => IsCompletedOperationalStatus(serviceRequest.CurrentStatus)),
                    zoneRequests.Count(serviceRequest => ResolvePriority(serviceRequest) == "emergency"),
                    zoneRequests.Count(serviceRequest => breachedServiceRequestIds.Contains(serviceRequest.ServiceRequestId)),
                    zoneTechnicians.Length);
            })
            .OrderByDescending(item => item.TotalCount)
            .ThenBy(item => item.ZoneName)
            .ToArray();
    }

    public static OperationsLiveMapTechnicianPinResponse? ToTechnicianMapPin(
        DomainTechnician technician,
        IReadOnlyCollection<TechnicianGpsLog> gpsLogs,
        DateOnly today)
    {
        var latestLog = gpsLogs
            .OrderByDescending(log => log.TrackedOnUtc)
            .FirstOrDefault();

        if (latestLog is null)
        {
            return null;
        }

        var item = TechnicianManagementSupport.ToListItem(technician, today);
        var breadcrumbs = gpsLogs
            .OrderBy(log => log.TrackedOnUtc)
            .TakeLast(6)
            .Select(log => new OperationsMapCoordinateResponse(
                (double)log.Latitude,
                (double)log.Longitude,
                log.TrackedOnUtc))
            .ToArray();

        return new OperationsLiveMapTechnicianPinResponse(
            item.TechnicianId,
            item.TechnicianCode,
            item.TechnicianName,
            item.AvailabilityStatus,
            item.CurrentServiceRequestNumber,
            item.BaseZoneName,
            (double)latestLog.Latitude,
            (double)latestLog.Longitude,
            latestLog.TrackedOnUtc,
            breadcrumbs);
    }

    public static OperationsLiveMapServiceRequestPinResponse? ToServiceRequestMapPin(DomainServiceRequest serviceRequest)
    {
        var latitude = serviceRequest.Booking?.CustomerAddress?.Latitude;
        var longitude = serviceRequest.Booking?.CustomerAddress?.Longitude;

        if (!latitude.HasValue || !longitude.HasValue)
        {
            return null;
        }

        return new OperationsLiveMapServiceRequestPinResponse(
            serviceRequest.ServiceRequestId,
            serviceRequest.ServiceRequestNumber,
            serviceRequest.Booking?.CustomerNameSnapshot ?? string.Empty,
            ResolveServiceName(serviceRequest),
            serviceRequest.CurrentStatus.ToString(),
            ResolvePriority(serviceRequest),
            ResolveZoneName(serviceRequest),
            TechnicianJobResponseMapper.BuildAddressSummary(serviceRequest.Booking),
            TechnicianJobResponseMapper.GetActiveAssignment(serviceRequest)?.Technician?.TechnicianName,
            latitude.Value,
            longitude.Value);
    }

    public static bool ShouldRenderOnLiveMap(DomainServiceRequest serviceRequest, DateOnly today)
    {
        var latitude = serviceRequest.Booking?.CustomerAddress?.Latitude;
        var longitude = serviceRequest.Booking?.CustomerAddress?.Longitude;

        if (!latitude.HasValue || !longitude.HasValue)
        {
            return false;
        }

        return GetOperationalDate(serviceRequest) == today ||
            ResolvePriority(serviceRequest) == "emergency" ||
            IsPendingQueueItem(serviceRequest) ||
            IsInProgressStatus(serviceRequest.CurrentStatus);
    }

    private static long? TryParseServiceRequestId(SystemAlert alert)
    {
        return long.TryParse(alert.RelatedEntityId, out var serviceRequestId) ? serviceRequestId : null;
    }

    private static string ResolveServiceName(DomainServiceRequest serviceRequest)
    {
        return serviceRequest.Booking?.BookingLines
            .OrderBy(line => line.BookingLineId)
            .Select(line => line.Service?.ServiceName)
            .FirstOrDefault(serviceName => !string.IsNullOrWhiteSpace(serviceName))
            ?? serviceRequest.Booking?.ServiceNameSnapshot
            ?? string.Empty;
    }

    private static string ResolveZoneName(DomainServiceRequest serviceRequest)
    {
        return string.IsNullOrWhiteSpace(serviceRequest.Booking?.ZoneNameSnapshot)
            ? "Unassigned"
            : serviceRequest.Booking.ZoneNameSnapshot;
    }

    private static int GetTechnicianAvailabilityRank(string availabilityStatus)
    {
        return availabilityStatus.ToLowerInvariant() switch
        {
            "available" => 0,
            "on-job" => 1,
            "off-duty" => 2,
            "on-leave" => 3,
            _ => 4
        };
    }
}
