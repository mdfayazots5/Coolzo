using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IFieldLookupRepository
{
    Task<IReadOnlyCollection<ServiceChecklistMaster>> GetChecklistByServiceIdAsync(long serviceId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ComplaintIssueMaster>> SearchComplaintIssuesAsync(string? searchTerm, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DiagnosisResultMaster>> SearchDiagnosisResultsAsync(string? searchTerm, CancellationToken cancellationToken);
}
