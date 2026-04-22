using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface ISchedulingRepository
{
    Task<IReadOnlyCollection<ServiceRequest>> GetBoardServiceRequestsAsync(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SlotAvailability>> GetSlotsAsync(long? zoneId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TechnicianShift>> GetTechnicianShiftsAsync(long? technicianId, bool asNoTracking, CancellationToken cancellationToken);

    Task<TechnicianShift?> GetTechnicianShiftForUpdateAsync(long technicianId, int dayOfWeekNumber, CancellationToken cancellationToken);

    Task AddTechnicianShiftAsync(TechnicianShift technicianShift, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<AmcVisitSchedule>> GetAmcVisitsAsync(DateOnly fromDate, DateOnly toDate, bool unlinkedOnly, CancellationToken cancellationToken);

    Task<AmcVisitSchedule?> GetAmcVisitForUpdateAsync(long amcVisitScheduleId, CancellationToken cancellationToken);
}
