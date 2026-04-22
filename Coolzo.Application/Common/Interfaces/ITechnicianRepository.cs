using Coolzo.Application.Common.Models;
using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface ITechnicianRepository
{
    Task AddAsync(Technician technician, CancellationToken cancellationToken);

    Task<bool> MobileExistsAsync(string mobileNumber, long? excludedTechnicianId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Technician>> SearchAsync(string? searchTerm, bool activeOnly, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Technician>> SearchManagementAsync(
        string? searchTerm,
        bool activeOnly,
        string? zoneName,
        string? skillName,
        string? availability,
        decimal? minimumRating,
        CancellationToken cancellationToken);

    Task<Technician?> GetByIdAsync(long technicianId, CancellationToken cancellationToken);

    Task<Technician?> GetByIdForUpdateAsync(long technicianId, CancellationToken cancellationToken);

    Task<Technician?> GetManagementDetailAsync(long technicianId, bool asNoTracking, CancellationToken cancellationToken);

    Task<Technician?> GetByUserIdAsync(long userId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Zone>> GetZonesByIdsAsync(IReadOnlyCollection<long> zoneIds, CancellationToken cancellationToken);

    Task<TechnicianAvailability?> GetAvailabilityEntryForUpdateAsync(long technicianId, DateOnly availableDate, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TechnicianAvailabilitySnapshot>> GetAvailabilityByServiceRequestIdAsync(long serviceRequestId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TechnicianAttendance>> GetAttendanceAsync(long technicianId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken);

    Task<TechnicianAttendance?> GetAttendanceByIdAsync(long technicianId, long technicianAttendanceId, bool asNoTracking, CancellationToken cancellationToken);

    Task<TechnicianAttendance?> GetAttendanceByDateAsync(long technicianId, DateOnly attendanceDate, bool asNoTracking, CancellationToken cancellationToken);

    Task AddAttendanceAsync(TechnicianAttendance technicianAttendance, CancellationToken cancellationToken);

    Task AddGpsLogAsync(TechnicianGpsLog technicianGpsLog, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TechnicianGpsLog>> GetGpsLogsAsync(long technicianId, DateOnly trackingDate, CancellationToken cancellationToken);

    Task<TechnicianPerformanceSummary?> GetPerformanceSummaryAsync(long technicianId, DateOnly summaryDate, bool asNoTracking, CancellationToken cancellationToken);

    Task AddPerformanceSummaryAsync(TechnicianPerformanceSummary performanceSummary, CancellationToken cancellationToken);

    Task<TechnicianPerformanceMetricsSnapshot> BuildPerformanceMetricsAsync(long technicianId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken);

    Task<decimal> GetTeamAverageSlaComplianceAsync(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken);
}
