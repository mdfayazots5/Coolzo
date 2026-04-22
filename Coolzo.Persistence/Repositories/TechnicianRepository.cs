using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class TechnicianRepository : ITechnicianRepository
{
    private readonly CoolzoDbContext _dbContext;

    public TechnicianRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Technician technician, CancellationToken cancellationToken)
    {
        return _dbContext.Technicians.AddAsync(technician, cancellationToken).AsTask();
    }

    public Task<bool> MobileExistsAsync(string mobileNumber, long? excludedTechnicianId, CancellationToken cancellationToken)
    {
        var normalizedMobile = mobileNumber.Trim();

        return _dbContext.Technicians.AnyAsync(
            entity => !entity.IsDeleted &&
                entity.MobileNumber == normalizedMobile &&
                (!excludedTechnicianId.HasValue || entity.TechnicianId != excludedTechnicianId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<Technician>> SearchAsync(string? searchTerm, bool activeOnly, CancellationToken cancellationToken)
    {
        var query = _dbContext.Technicians
            .AsNoTracking()
            .Include(entity => entity.BaseZone)
            .Include(entity => entity.ServiceRequestAssignments)
            .Where(entity => !entity.IsDeleted);

        if (activeOnly)
        {
            query = query.Where(entity => entity.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(
                entity => entity.TechnicianCode.Contains(searchTerm) ||
                    entity.TechnicianName.Contains(searchTerm) ||
                    entity.MobileNumber.Contains(searchTerm));
        }

        return await query
            .OrderBy(entity => entity.TechnicianName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Technician>> SearchManagementAsync(
        string? searchTerm,
        bool activeOnly,
        string? zoneName,
        string? skillName,
        string? availability,
        decimal? minimumRating,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Technicians
            .AsNoTracking()
            .AsSplitQuery()
            .Include(entity => entity.BaseZone)
            .Include(entity => entity.Skills.Where(skill => !skill.IsDeleted))
            .Include(entity => entity.Zones.Where(zone => !zone.IsDeleted))
                .ThenInclude(zone => zone.Zone)
            .Include(entity => entity.TechnicianAvailabilities.Where(availabilityEntry => !availabilityEntry.IsDeleted))
            .Include(entity => entity.Attendances.Where(attendance => !attendance.IsDeleted))
            .Include(entity => entity.PerformanceSummaries.Where(summary => !summary.IsDeleted))
            .Include(entity => entity.ServiceRequestAssignments.Where(assignment => !assignment.IsDeleted))
                .ThenInclude(assignment => assignment.ServiceRequest)
            .Where(entity => !entity.IsDeleted);

        if (activeOnly)
        {
            query = query.Where(entity => entity.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearch = searchTerm.Trim();
            query = query.Where(
                entity => entity.TechnicianCode.Contains(normalizedSearch) ||
                    entity.TechnicianName.Contains(normalizedSearch) ||
                    entity.MobileNumber.Contains(normalizedSearch) ||
                    entity.EmailAddress.Contains(normalizedSearch));
        }

        var technicians = await query
            .OrderBy(entity => entity.TechnicianName)
            .ToArrayAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return technicians
            .Where(entity => MatchesZone(entity, zoneName))
            .Where(entity => MatchesSkill(entity, skillName))
            .Where(entity => MatchesAvailability(entity, availability, today))
            .Where(entity => !minimumRating.HasValue || GetLatestAverageRating(entity) >= minimumRating.Value)
            .ToArray();
    }

    public Task<Technician?> GetByIdAsync(long technicianId, CancellationToken cancellationToken)
    {
        return _dbContext.Technicians
            .AsNoTracking()
            .Include(entity => entity.BaseZone)
            .FirstOrDefaultAsync(entity => entity.TechnicianId == technicianId && !entity.IsDeleted, cancellationToken);
    }

    public Task<Technician?> GetByIdForUpdateAsync(long technicianId, CancellationToken cancellationToken)
    {
        return _dbContext.Technicians
            .Include(entity => entity.BaseZone)
            .FirstOrDefaultAsync(entity => entity.TechnicianId == technicianId && !entity.IsDeleted, cancellationToken);
    }

    public async Task<Technician?> GetManagementDetailAsync(long technicianId, bool asNoTracking, CancellationToken cancellationToken)
    {
        var query = asNoTracking
            ? _dbContext.Technicians.AsNoTracking()
            : _dbContext.Technicians.AsQueryable();

        return await query
            .AsSplitQuery()
            .Include(entity => entity.BaseZone)
            .Include(entity => entity.Skills.Where(skill => !skill.IsDeleted))
            .Include(entity => entity.Zones.Where(zone => !zone.IsDeleted))
                .ThenInclude(zone => zone.Zone)
            .Include(entity => entity.TechnicianAvailabilities.Where(availability => !availability.IsDeleted))
            .Include(entity => entity.Attendances.Where(attendance => !attendance.IsDeleted))
            .Include(entity => entity.GpsLogs.Where(log => !log.IsDeleted))
            .Include(entity => entity.PerformanceSummaries.Where(summary => !summary.IsDeleted))
            .Include(entity => entity.ServiceRequestAssignments.Where(assignment => !assignment.IsDeleted))
                .ThenInclude(assignment => assignment.ServiceRequest)
            .FirstOrDefaultAsync(entity => entity.TechnicianId == technicianId && !entity.IsDeleted, cancellationToken);
    }

    public Task<Technician?> GetByUserIdAsync(long userId, CancellationToken cancellationToken)
    {
        return _dbContext.Technicians
            .AsNoTracking()
            .Include(entity => entity.BaseZone)
            .FirstOrDefaultAsync(
                entity => entity.UserId == userId && entity.IsActive && !entity.IsDeleted,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Zone>> GetZonesByIdsAsync(IReadOnlyCollection<long> zoneIds, CancellationToken cancellationToken)
    {
        if (zoneIds.Count == 0)
        {
            return Array.Empty<Zone>();
        }

        return await _dbContext.Zones
            .AsNoTracking()
            .Where(entity => zoneIds.Contains(entity.ZoneId) && !entity.IsDeleted)
            .OrderBy(entity => entity.ZoneName)
            .ToArrayAsync(cancellationToken);
    }

    public Task<TechnicianAvailability?> GetAvailabilityEntryForUpdateAsync(long technicianId, DateOnly availableDate, CancellationToken cancellationToken)
    {
        return _dbContext.TechnicianAvailabilities
            .FirstOrDefaultAsync(
                entity => entity.TechnicianId == technicianId &&
                    entity.AvailableDate == availableDate &&
                    !entity.IsDeleted,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<TechnicianAvailabilitySnapshot>> GetAvailabilityByServiceRequestIdAsync(long serviceRequestId, CancellationToken cancellationToken)
    {
        var serviceRequestInfo = await _dbContext.ServiceRequests
            .AsNoTracking()
            .Where(entity => entity.ServiceRequestId == serviceRequestId && !entity.IsDeleted)
            .Select(entity => new
            {
                SlotDate = entity.Booking != null && entity.Booking.SlotAvailability != null
                    ? entity.Booking.SlotAvailability.SlotDate
                    : DateOnly.FromDateTime(entity.ServiceRequestDateUtc),
                ServiceId = entity.Booking != null
                    ? entity.Booking.BookingLines.OrderBy(line => line.BookingLineId).Select(line => (long?)line.ServiceId).FirstOrDefault()
                    : null,
                AcTypeId = entity.Booking != null
                    ? entity.Booking.BookingLines.OrderBy(line => line.BookingLineId).Select(line => (long?)line.AcTypeId).FirstOrDefault()
                    : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (serviceRequestInfo is null || !serviceRequestInfo.ServiceId.HasValue)
        {
            return Array.Empty<TechnicianAvailabilitySnapshot>();
        }

        var technicians = await _dbContext.Technicians
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted)
            .Select(entity => new
            {
                entity.TechnicianId,
                entity.TechnicianCode,
                entity.TechnicianName,
                entity.MobileNumber,
                entity.EmailAddress,
                BaseZoneName = entity.BaseZone != null ? entity.BaseZone.ZoneName : null,
                entity.IsActive,
                entity.MaxDailyAssignments,
                AvailableSlotCount = entity.TechnicianAvailabilities
                    .Where(availability => !availability.IsDeleted && availability.AvailableDate == serviceRequestInfo.SlotDate)
                    .Select(availability => (int?)availability.AvailableSlotCount)
                    .FirstOrDefault(),
                AvailabilityEnabled = entity.TechnicianAvailabilities
                    .Where(availability => !availability.IsDeleted && availability.AvailableDate == serviceRequestInfo.SlotDate)
                    .Select(availability => (bool?)availability.IsAvailable)
                    .FirstOrDefault(),
                ActiveWorkload = entity.ServiceRequestAssignments.Count(
                    assignment => assignment.IsActiveAssignment &&
                        !assignment.IsDeleted &&
                        assignment.ServiceRequestId != serviceRequestId &&
                        assignment.ServiceRequest != null &&
                        !assignment.ServiceRequest.IsDeleted &&
                        assignment.ServiceRequest.Booking != null &&
                        assignment.ServiceRequest.Booking.SlotAvailability != null &&
                        assignment.ServiceRequest.Booking.SlotAvailability.SlotDate == serviceRequestInfo.SlotDate),
                HasSkillMatch = !entity.SkillMappings.Any(skill => !skill.IsDeleted) ||
                    entity.SkillMappings.Any(
                        skill => !skill.IsDeleted &&
                            skill.ServiceId == serviceRequestInfo.ServiceId.Value &&
                            (!skill.AcTypeId.HasValue || skill.AcTypeId == serviceRequestInfo.AcTypeId))
            })
            .OrderBy(entity => entity.TechnicianName)
            .ToArrayAsync(cancellationToken);

        return technicians
            .Select(entity =>
            {
                var availableSlotCount = entity.AvailableSlotCount ?? entity.MaxDailyAssignments;
                var remainingCapacity = Math.Max(availableSlotCount - entity.ActiveWorkload, 0);
                var isAvailable = entity.IsActive &&
                    (entity.AvailabilityEnabled ?? true) &&
                    remainingCapacity > 0;

                return new TechnicianAvailabilitySnapshot(
                    entity.TechnicianId,
                    entity.TechnicianCode,
                    entity.TechnicianName,
                    entity.MobileNumber,
                    entity.EmailAddress,
                    entity.BaseZoneName,
                    serviceRequestInfo.SlotDate,
                    availableSlotCount,
                    entity.ActiveWorkload,
                    remainingCapacity,
                    isAvailable,
                    entity.HasSkillMatch);
            })
            .ToArray();
    }

    public async Task<IReadOnlyCollection<TechnicianAttendance>> GetAttendanceAsync(long technicianId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
    {
        return await _dbContext.TechnicianAttendances
            .AsNoTracking()
            .Where(entity => entity.TechnicianId == technicianId &&
                !entity.IsDeleted &&
                entity.AttendanceDate >= fromDate &&
                entity.AttendanceDate <= toDate)
            .OrderBy(entity => entity.AttendanceDate)
            .ToArrayAsync(cancellationToken);
    }

    public Task<TechnicianAttendance?> GetAttendanceByIdAsync(long technicianId, long technicianAttendanceId, bool asNoTracking, CancellationToken cancellationToken)
    {
        var query = asNoTracking
            ? _dbContext.TechnicianAttendances.AsNoTracking()
            : _dbContext.TechnicianAttendances.AsQueryable();

        return query.FirstOrDefaultAsync(
            entity => entity.TechnicianId == technicianId &&
                entity.TechnicianAttendanceId == technicianAttendanceId &&
                !entity.IsDeleted,
            cancellationToken);
    }

    public Task<TechnicianAttendance?> GetAttendanceByDateAsync(long technicianId, DateOnly attendanceDate, bool asNoTracking, CancellationToken cancellationToken)
    {
        var query = asNoTracking
            ? _dbContext.TechnicianAttendances.AsNoTracking()
            : _dbContext.TechnicianAttendances.AsQueryable();

        return query.FirstOrDefaultAsync(
            entity => entity.TechnicianId == technicianId &&
                entity.AttendanceDate == attendanceDate &&
                !entity.IsDeleted,
            cancellationToken);
    }

    public Task AddAttendanceAsync(TechnicianAttendance technicianAttendance, CancellationToken cancellationToken)
    {
        return _dbContext.TechnicianAttendances.AddAsync(technicianAttendance, cancellationToken).AsTask();
    }

    public Task AddGpsLogAsync(TechnicianGpsLog technicianGpsLog, CancellationToken cancellationToken)
    {
        return _dbContext.TechnicianGpsLogs.AddAsync(technicianGpsLog, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<TechnicianGpsLog>> GetGpsLogsAsync(long technicianId, DateOnly trackingDate, CancellationToken cancellationToken)
    {
        var fromUtc = trackingDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = trackingDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        return await _dbContext.TechnicianGpsLogs
            .AsNoTracking()
            .Where(entity => entity.TechnicianId == technicianId &&
                !entity.IsDeleted &&
                entity.TrackedOnUtc >= fromUtc &&
                entity.TrackedOnUtc < toUtc)
            .OrderBy(entity => entity.TrackedOnUtc)
            .ToArrayAsync(cancellationToken);
    }

    public Task<TechnicianPerformanceSummary?> GetPerformanceSummaryAsync(long technicianId, DateOnly summaryDate, bool asNoTracking, CancellationToken cancellationToken)
    {
        var query = asNoTracking
            ? _dbContext.TechnicianPerformanceSummaries.AsNoTracking()
            : _dbContext.TechnicianPerformanceSummaries.AsQueryable();

        return query.FirstOrDefaultAsync(
            entity => entity.TechnicianId == technicianId &&
                entity.SummaryDate == summaryDate &&
                !entity.IsDeleted,
            cancellationToken);
    }

    public Task AddPerformanceSummaryAsync(TechnicianPerformanceSummary performanceSummary, CancellationToken cancellationToken)
    {
        return _dbContext.TechnicianPerformanceSummaries.AddAsync(performanceSummary, cancellationToken).AsTask();
    }

    public async Task<TechnicianPerformanceMetricsSnapshot> BuildPerformanceMetricsAsync(long technicianId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
    {
        var fromUtc = fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var assignments = await _dbContext.ServiceRequestAssignments
            .AsNoTracking()
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.JobCard)
            .Where(entity => entity.TechnicianId == technicianId &&
                !entity.IsDeleted &&
                entity.AssignedDateUtc >= fromUtc &&
                entity.AssignedDateUtc < toUtc)
            .OrderBy(entity => entity.AssignedDateUtc)
            .ToArrayAsync(cancellationToken);

        var totalJobs = assignments.Length;
        var completedJobs = assignments.Count(IsCompletedAssignment);

        var revisitServiceRequestIds = assignments
            .Select(entity => entity.ServiceRequestId)
            .Distinct()
            .ToArray();

        var revisitCount = revisitServiceRequestIds.Length == 0
            ? 0
            : await _dbContext.RevisitRequests
                .AsNoTracking()
                .CountAsync(
                    entity => !entity.IsDeleted &&
                        revisitServiceRequestIds.Contains(entity.OriginalServiceRequestId),
                    cancellationToken);

        var revenueGenerated = await _dbContext.TechnicianEarnings
            .AsNoTracking()
            .Where(entity => entity.TechnicianId == technicianId &&
                !entity.IsDeleted &&
                entity.CalculatedDateUtc >= fromUtc &&
                entity.CalculatedDateUtc < toUtc)
            .Select(entity => (decimal?)entity.EarningAmount)
            .SumAsync(cancellationToken) ?? 0m;

        var existingAverageRating = await _dbContext.TechnicianPerformanceSummaries
            .AsNoTracking()
            .Where(entity => entity.TechnicianId == technicianId && !entity.IsDeleted)
            .OrderByDescending(entity => entity.SummaryDate)
            .Select(entity => (decimal?)entity.AverageRating)
            .FirstOrDefaultAsync(cancellationToken) ?? 0m;

        var trendPoints = Enumerable
            .Range(0, Math.Max(toDate.DayNumber - fromDate.DayNumber + 1, 1))
            .Select(offset => fromDate.AddDays(offset))
            .Select(metricDate =>
            {
                var dailyAssignments = assignments
                    .Where(entity => DateOnly.FromDateTime(entity.AssignedDateUtc) == metricDate)
                    .ToArray();
                var dailyCompletedJobs = dailyAssignments.Count(IsCompletedAssignment);
                var dailySla = dailyAssignments.Length == 0
                    ? 0m
                    : Math.Round(dailyCompletedJobs * 100m / dailyAssignments.Length, 2);

                return new TechnicianPerformanceTrendPointMetric(
                    metricDate,
                    dailyAssignments.Length,
                    dailyCompletedJobs,
                    dailySla);
            })
            .ToArray();

        return new TechnicianPerformanceMetricsSnapshot(
            existingAverageRating,
            totalJobs,
            completedJobs,
            totalJobs == 0 ? 0m : Math.Round(completedJobs * 100m / totalJobs, 2),
            completedJobs == 0 ? 0m : Math.Round(revisitCount * 100m / completedJobs, 2),
            Math.Round(revenueGenerated, 2),
            trendPoints);
    }

    public async Task<decimal> GetTeamAverageSlaComplianceAsync(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
    {
        var fromUtc = fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var assignments = await _dbContext.ServiceRequestAssignments
            .AsNoTracking()
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.JobCard)
            .Where(entity => !entity.IsDeleted &&
                entity.AssignedDateUtc >= fromUtc &&
                entity.AssignedDateUtc < toUtc)
            .ToArrayAsync(cancellationToken);

        var technicianRates = assignments
            .GroupBy(entity => entity.TechnicianId)
            .Select(group =>
            {
                var totalAssignments = group.Count();
                var completedAssignments = group.Count(IsCompletedAssignment);

                return totalAssignments == 0
                    ? 0m
                    : completedAssignments * 100m / totalAssignments;
            })
            .ToArray();

        return technicianRates.Length == 0
            ? 0m
            : Math.Round(technicianRates.Average(), 2);
    }

    private static bool MatchesZone(Technician technician, string? zoneName)
    {
        if (string.IsNullOrWhiteSpace(zoneName))
        {
            return true;
        }

        var normalizedZone = zoneName.Trim();
        var assignedZones = technician.Zones
            .Where(entity => !entity.IsDeleted && entity.Zone is not null)
            .Select(entity => entity.Zone!.ZoneName);

        return assignedZones.Any(zone => zone.Contains(normalizedZone, StringComparison.OrdinalIgnoreCase)) ||
            (technician.BaseZone?.ZoneName?.Contains(normalizedZone, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private static bool MatchesSkill(Technician technician, string? skillName)
    {
        if (string.IsNullOrWhiteSpace(skillName))
        {
            return true;
        }

        var normalizedSkill = skillName.Trim();

        return technician.Skills.Any(
            entity => !entity.IsDeleted &&
                entity.SkillName.Contains(normalizedSkill, StringComparison.OrdinalIgnoreCase));
    }

    private static bool MatchesAvailability(Technician technician, string? availability, DateOnly today)
    {
        if (string.IsNullOrWhiteSpace(availability))
        {
            return true;
        }

        var normalizedAvailability = availability.Trim().ToLowerInvariant();
        var resolvedStatus = ResolveAvailabilityStatus(technician, today);

        return resolvedStatus == normalizedAvailability;
    }

    private static string ResolveAvailabilityStatus(Technician technician, DateOnly today)
    {
        if (!technician.IsActive)
        {
            return "off-duty";
        }

        var attendance = technician.Attendances
            .Where(entity => !entity.IsDeleted && entity.AttendanceDate == today)
            .OrderByDescending(entity => entity.TechnicianAttendanceId)
            .FirstOrDefault();

        if (attendance is not null &&
            (attendance.AttendanceStatus.Equals("LeaveRequested", StringComparison.OrdinalIgnoreCase) ||
             attendance.AttendanceStatus.Equals("LeaveApproved", StringComparison.OrdinalIgnoreCase) ||
             attendance.AttendanceStatus.Equals("OnLeave", StringComparison.OrdinalIgnoreCase)))
        {
            return "on-leave";
        }

        if (technician.ServiceRequestAssignments.Any(entity => entity.IsActiveAssignment && !entity.IsDeleted))
        {
            return "on-job";
        }

        var availabilityEntry = technician.TechnicianAvailabilities
            .Where(entity => !entity.IsDeleted && entity.AvailableDate == today)
            .OrderByDescending(entity => entity.TechnicianAvailabilityId)
            .FirstOrDefault();

        if (availabilityEntry is not null && !availabilityEntry.IsAvailable)
        {
            return "off-duty";
        }

        return "available";
    }

    private static decimal GetLatestAverageRating(Technician technician)
    {
        return technician.PerformanceSummaries
            .Where(entity => !entity.IsDeleted)
            .OrderByDescending(entity => entity.SummaryDate)
            .ThenByDescending(entity => entity.TechnicianPerformanceSummaryId)
            .Select(entity => entity.AverageRating)
            .FirstOrDefault();
    }

    private static bool IsCompletedAssignment(ServiceRequestAssignment assignment)
    {
        var serviceRequest = assignment.ServiceRequest;
        var jobCard = serviceRequest?.JobCard;

        return (jobCard?.WorkCompletedDateUtc).HasValue ||
            (jobCard?.SubmittedForClosureDateUtc).HasValue ||
            serviceRequest?.CurrentStatus is ServiceRequestStatus.WorkCompletedPendingSubmission or ServiceRequestStatus.SubmittedForClosure;
    }
}
