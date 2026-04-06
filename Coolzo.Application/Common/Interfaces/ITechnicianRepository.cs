using Coolzo.Application.Common.Models;
using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface ITechnicianRepository
{
    Task AddAsync(Technician technician, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Technician>> SearchAsync(string? searchTerm, bool activeOnly, CancellationToken cancellationToken);

    Task<Technician?> GetByIdAsync(long technicianId, CancellationToken cancellationToken);

    Task<Technician?> GetByIdForUpdateAsync(long technicianId, CancellationToken cancellationToken);

    Task<Technician?> GetByUserIdAsync(long userId, CancellationToken cancellationToken);

    Task<TechnicianAvailability?> GetAvailabilityEntryForUpdateAsync(long technicianId, DateOnly availableDate, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TechnicianAvailabilitySnapshot>> GetAvailabilityByServiceRequestIdAsync(long serviceRequestId, CancellationToken cancellationToken);
}
