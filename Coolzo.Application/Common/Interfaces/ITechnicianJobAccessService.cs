using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface ITechnicianJobAccessService
{
    Task<Technician> GetCurrentTechnicianAsync(CancellationToken cancellationToken);

    Task<ServiceRequest> GetOwnedServiceRequestAsync(long serviceRequestId, CancellationToken cancellationToken);

    Task<ServiceRequest> GetOwnedServiceRequestForUpdateAsync(long serviceRequestId, CancellationToken cancellationToken);
}
