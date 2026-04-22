using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IFieldWorkflowRepository
{
    Task<JobReport?> GetJobReportByIdempotencyKeyAsync(string idempotencyKey, bool asNoTracking, CancellationToken cancellationToken);

    Task<JobReport?> GetLatestJobReportAsync(long serviceRequestId, bool asNoTracking, CancellationToken cancellationToken);

    Task AddJobReportAsync(JobReport jobReport, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<JobPhoto>> GetJobPhotosAsync(long serviceRequestId, bool asNoTracking, CancellationToken cancellationToken);

    Task AddJobPhotoAsync(JobPhoto jobPhoto, CancellationToken cancellationToken);

    Task<CustomerSignature?> GetLatestCustomerSignatureAsync(long serviceRequestId, bool asNoTracking, CancellationToken cancellationToken);

    Task AddCustomerSignatureAsync(CustomerSignature customerSignature, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PartsRequest>> GetPartsRequestsAsync(long serviceRequestId, bool asNoTracking, CancellationToken cancellationToken);

    Task AddPartsRequestAsync(PartsRequest partsRequest, CancellationToken cancellationToken);
}
