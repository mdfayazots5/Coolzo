using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Coolzo.Domain.Entities;
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

    public Task<Technician?> GetByUserIdAsync(long userId, CancellationToken cancellationToken)
    {
        return _dbContext.Technicians
            .AsNoTracking()
            .Include(entity => entity.BaseZone)
            .FirstOrDefaultAsync(
                entity => entity.UserId == userId && entity.IsActive && !entity.IsDeleted,
                cancellationToken);
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
}
