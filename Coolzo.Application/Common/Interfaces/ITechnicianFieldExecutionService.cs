using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;

namespace Coolzo.Application.Common.Interfaces;

public interface ITechnicianFieldExecutionService
{
    Task<ServiceRequest> AdvanceStatusAsync(
        long serviceRequestId,
        ServiceRequestStatus targetStatus,
        string? remarks,
        string? workSummary,
        string auditActionName,
        CancellationToken cancellationToken);
}
