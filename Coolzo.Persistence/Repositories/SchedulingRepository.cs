using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class SchedulingRepository : ISchedulingRepository
{
    private readonly CoolzoDbContext _dbContext;

    public SchedulingRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ServiceRequest>> GetBoardServiceRequestsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ServiceRequests
            .AsNoTracking()
            .AsSplitQuery()
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.Customer)
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.CustomerAddress)
                    .ThenInclude(address => address!.Zone)
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.SlotAvailability)
                    .ThenInclude(slot => slot!.SlotConfiguration)
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.SlotAvailability)
                    .ThenInclude(slot => slot!.Zone)
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.BookingLines)
                    .ThenInclude(line => line.Service)
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.BookingLines)
                    .ThenInclude(line => line.AcType)
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.BookingLines)
                    .ThenInclude(line => line.Brand)
            .Include(entity => entity.Assignments.Where(assignment => !assignment.IsDeleted))
                .ThenInclude(assignment => assignment.Technician)
            .Where(entity =>
                !entity.IsDeleted &&
                entity.Booking != null &&
                entity.Booking.SlotAvailability != null &&
                entity.Booking.SlotAvailability.SlotDate >= fromDate &&
                entity.Booking.SlotAvailability.SlotDate <= toDate)
            .OrderBy(entity => entity.Booking!.SlotAvailability!.SlotDate)
            .ThenBy(entity => entity.Booking!.SlotAvailability!.SlotConfiguration!.StartTime)
            .ThenBy(entity => entity.ServiceRequestNumber)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<SlotAvailability>> GetSlotsAsync(
        long? zoneId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.SlotAvailabilities
            .AsNoTracking()
            .Include(entity => entity.SlotConfiguration)
            .Include(entity => entity.Zone)
            .Where(entity =>
                !entity.IsDeleted &&
                entity.SlotDate >= fromDate &&
                entity.SlotDate <= toDate &&
                entity.SlotConfiguration != null &&
                !entity.SlotConfiguration.IsDeleted);

        if (zoneId.HasValue)
        {
            query = query.Where(entity => entity.ZoneId == zoneId.Value);
        }

        return await query
            .OrderBy(entity => entity.SlotDate)
            .ThenBy(entity => entity.Zone!.ZoneName)
            .ThenBy(entity => entity.SlotConfiguration!.StartTime)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TechnicianShift>> GetTechnicianShiftsAsync(
        long? technicianId,
        bool asNoTracking,
        CancellationToken cancellationToken)
    {
        IQueryable<TechnicianShift> query = asNoTracking
            ? _dbContext.TechnicianShifts.AsNoTracking()
            : _dbContext.TechnicianShifts.AsQueryable();

        query = query
            .Include(entity => entity.Technician)
            .Where(entity => !entity.IsDeleted);

        if (technicianId.HasValue)
        {
            query = query.Where(entity => entity.TechnicianId == technicianId.Value);
        }

        return await query
            .OrderBy(entity => entity.TechnicianId)
            .ThenBy(entity => entity.DayOfWeekNumber)
            .ToArrayAsync(cancellationToken);
    }

    public Task<TechnicianShift?> GetTechnicianShiftForUpdateAsync(long technicianId, int dayOfWeekNumber, CancellationToken cancellationToken)
    {
        return _dbContext.TechnicianShifts
            .Include(entity => entity.Technician)
            .FirstOrDefaultAsync(
                entity => entity.TechnicianId == technicianId &&
                    entity.DayOfWeekNumber == dayOfWeekNumber &&
                    !entity.IsDeleted,
                cancellationToken);
    }

    public Task AddTechnicianShiftAsync(TechnicianShift technicianShift, CancellationToken cancellationToken)
    {
        return _dbContext.TechnicianShifts.AddAsync(technicianShift, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<AmcVisitSchedule>> GetAmcVisitsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        bool unlinkedOnly,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.AmcVisitSchedules
            .AsNoTracking()
            .AsSplitQuery()
            .Include(entity => entity.CustomerAmc)
                .ThenInclude(customerAmc => customerAmc!.Customer)
            .Include(entity => entity.CustomerAmc)
                .ThenInclude(customerAmc => customerAmc!.AmcPlan)
            .Include(entity => entity.CustomerAmc)
                .ThenInclude(customerAmc => customerAmc!.JobCard)
                    .ThenInclude(jobCard => jobCard!.ServiceRequest)
                        .ThenInclude(serviceRequest => serviceRequest!.Booking)
                            .ThenInclude(booking => booking!.CustomerAddress)
                                .ThenInclude(address => address!.Zone)
            .Include(entity => entity.CustomerAmc)
                .ThenInclude(customerAmc => customerAmc!.JobCard)
                    .ThenInclude(jobCard => jobCard!.ServiceRequest)
                        .ThenInclude(serviceRequest => serviceRequest!.Booking)
                            .ThenInclude(booking => booking!.BookingLines)
                                .ThenInclude(line => line.Service)
            .Include(entity => entity.CustomerAmc)
                .ThenInclude(customerAmc => customerAmc!.JobCard)
                    .ThenInclude(jobCard => jobCard!.ServiceRequest)
                        .ThenInclude(serviceRequest => serviceRequest!.Booking)
                            .ThenInclude(booking => booking!.BookingLines)
                                .ThenInclude(line => line.AcType)
            .Include(entity => entity.CustomerAmc)
                .ThenInclude(customerAmc => customerAmc!.JobCard)
                    .ThenInclude(jobCard => jobCard!.ServiceRequest)
                        .ThenInclude(serviceRequest => serviceRequest!.Booking)
                            .ThenInclude(booking => booking!.BookingLines)
                                .ThenInclude(line => line.Brand)
            .Include(entity => entity.ServiceRequest)
            .Where(entity =>
                !entity.IsDeleted &&
                entity.ScheduledDate >= fromDate &&
                entity.ScheduledDate <= toDate);

        if (unlinkedOnly)
        {
            query = query.Where(entity => !entity.ServiceRequestId.HasValue);
        }

        return await query
            .OrderBy(entity => entity.ScheduledDate)
            .ThenBy(entity => entity.VisitNumber)
            .ToArrayAsync(cancellationToken);
    }

    public Task<AmcVisitSchedule?> GetAmcVisitForUpdateAsync(long amcVisitScheduleId, CancellationToken cancellationToken)
    {
        return _dbContext.AmcVisitSchedules
            .AsSplitQuery()
            .Include(entity => entity.CustomerAmc)
                .ThenInclude(customerAmc => customerAmc!.Customer)
            .Include(entity => entity.CustomerAmc)
                .ThenInclude(customerAmc => customerAmc!.AmcPlan)
            .Include(entity => entity.CustomerAmc)
                .ThenInclude(customerAmc => customerAmc!.JobCard)
                    .ThenInclude(jobCard => jobCard!.ServiceRequest)
                        .ThenInclude(serviceRequest => serviceRequest!.Booking)
                            .ThenInclude(booking => booking!.CustomerAddress)
            .Include(entity => entity.CustomerAmc)
                .ThenInclude(customerAmc => customerAmc!.JobCard)
                    .ThenInclude(jobCard => jobCard!.ServiceRequest)
                        .ThenInclude(serviceRequest => serviceRequest!.Booking)
                            .ThenInclude(booking => booking!.BookingLines)
                                .ThenInclude(line => line.Service)
            .Include(entity => entity.CustomerAmc)
                .ThenInclude(customerAmc => customerAmc!.JobCard)
                    .ThenInclude(jobCard => jobCard!.ServiceRequest)
                        .ThenInclude(serviceRequest => serviceRequest!.Booking)
                            .ThenInclude(booking => booking!.BookingLines)
                                .ThenInclude(line => line.AcType)
            .Include(entity => entity.CustomerAmc)
                .ThenInclude(customerAmc => customerAmc!.JobCard)
                    .ThenInclude(jobCard => jobCard!.ServiceRequest)
                        .ThenInclude(serviceRequest => serviceRequest!.Booking)
                            .ThenInclude(booking => booking!.BookingLines)
                                .ThenInclude(line => line.Brand)
            .FirstOrDefaultAsync(
                entity => entity.AmcVisitScheduleId == amcVisitScheduleId && !entity.IsDeleted,
                cancellationToken);
    }
}
