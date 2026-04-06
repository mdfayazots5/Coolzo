using Coolzo.Application.Common.Models;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;

namespace Coolzo.Application.Common.Interfaces;

public interface IServiceRequestRepository
{
    Task AddAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken);

    Task<bool> ServiceRequestNumberExistsAsync(string serviceRequestNumber, CancellationToken cancellationToken);

    Task<ServiceRequest?> GetByIdAsync(long serviceRequestId, CancellationToken cancellationToken);

    Task<ServiceRequest?> GetByIdForUpdateAsync(long serviceRequestId, CancellationToken cancellationToken);

    Task<ServiceRequest?> GetByJobCardIdAsync(long jobCardId, CancellationToken cancellationToken);

    Task<ServiceRequest?> GetByJobCardIdForUpdateAsync(long jobCardId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ServiceRequest>> SearchAssignedJobsAsync(
        long technicianId,
        ServiceRequestStatus? currentStatus,
        DateOnly? slotDate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountAssignedJobsAsync(
        long technicianId,
        ServiceRequestStatus? currentStatus,
        DateOnly? slotDate,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ServiceRequest>> SearchAsync(
        long? bookingId,
        long? serviceId,
        ServiceRequestStatus? currentStatus,
        DateOnly? slotDate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountSearchAsync(
        long? bookingId,
        long? serviceId,
        ServiceRequestStatus? currentStatus,
        DateOnly? slotDate,
        CancellationToken cancellationToken);

    Task<OperationsDashboardSummaryView> GetDashboardSummaryAsync(CancellationToken cancellationToken);
}
