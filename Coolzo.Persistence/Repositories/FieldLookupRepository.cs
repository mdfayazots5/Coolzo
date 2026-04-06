using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class FieldLookupRepository : IFieldLookupRepository
{
    private readonly CoolzoDbContext _dbContext;

    public FieldLookupRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ServiceChecklistMaster>> GetChecklistByServiceIdAsync(long serviceId, CancellationToken cancellationToken)
    {
        return await _dbContext.ServiceChecklistMasters
            .AsNoTracking()
            .Where(entity => entity.ServiceId == serviceId && entity.IsPublished && !entity.IsDeleted)
            .OrderBy(entity => entity.SortOrder)
            .ThenBy(entity => entity.ChecklistTitle)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ComplaintIssueMaster>> SearchComplaintIssuesAsync(string? searchTerm, CancellationToken cancellationToken)
    {
        var query = _dbContext.ComplaintIssueMasters
            .AsNoTracking()
            .Where(entity => entity.IsPublished && !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(entity => entity.IssueName.Contains(searchTerm));
        }

        return await query
            .OrderBy(entity => entity.ServiceId)
            .ThenBy(entity => entity.IssueName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<DiagnosisResultMaster>> SearchDiagnosisResultsAsync(string? searchTerm, CancellationToken cancellationToken)
    {
        var query = _dbContext.DiagnosisResultMasters
            .AsNoTracking()
            .Where(entity => entity.IsPublished && !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(entity => entity.ResultName.Contains(searchTerm));
        }

        return await query
            .OrderBy(entity => entity.ResultName)
            .ToArrayAsync(cancellationToken);
    }
}
