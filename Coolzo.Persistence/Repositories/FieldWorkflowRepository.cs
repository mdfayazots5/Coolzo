using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class FieldWorkflowRepository : IFieldWorkflowRepository
{
    private readonly CoolzoDbContext _dbContext;

    public FieldWorkflowRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<JobReport?> GetJobReportByIdempotencyKeyAsync(string idempotencyKey, bool asNoTracking, CancellationToken cancellationToken)
    {
        IQueryable<JobReport> query = asNoTracking
            ? _dbContext.JobReports.AsNoTracking()
            : _dbContext.JobReports.AsQueryable();

        return query
            .Include(entity => entity.Photos.Where(photo => !photo.IsDeleted))
            .Include(entity => entity.Signatures.Where(signature => !signature.IsDeleted))
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.IdempotencyKey == idempotencyKey,
                cancellationToken);
    }

    public Task<JobReport?> GetLatestJobReportAsync(long serviceRequestId, bool asNoTracking, CancellationToken cancellationToken)
    {
        IQueryable<JobReport> query = asNoTracking
            ? _dbContext.JobReports.AsNoTracking()
            : _dbContext.JobReports.AsQueryable();

        return query
            .Include(entity => entity.Photos.Where(photo => !photo.IsDeleted))
            .Include(entity => entity.Signatures.Where(signature => !signature.IsDeleted))
            .OrderByDescending(entity => entity.SubmittedAtUtc)
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ServiceRequestId == serviceRequestId,
                cancellationToken);
    }

    public Task AddJobReportAsync(JobReport jobReport, CancellationToken cancellationToken)
    {
        return _dbContext.JobReports.AddAsync(jobReport, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<JobPhoto>> GetJobPhotosAsync(long serviceRequestId, bool asNoTracking, CancellationToken cancellationToken)
    {
        IQueryable<JobPhoto> query = asNoTracking
            ? _dbContext.JobPhotos.AsNoTracking()
            : _dbContext.JobPhotos.AsQueryable();

        return await query
            .Where(entity => !entity.IsDeleted && entity.ServiceRequestId == serviceRequestId)
            .OrderByDescending(entity => entity.UploadedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddJobPhotoAsync(JobPhoto jobPhoto, CancellationToken cancellationToken)
    {
        return _dbContext.JobPhotos.AddAsync(jobPhoto, cancellationToken).AsTask();
    }

    public Task<CustomerSignature?> GetLatestCustomerSignatureAsync(long serviceRequestId, bool asNoTracking, CancellationToken cancellationToken)
    {
        IQueryable<CustomerSignature> query = asNoTracking
            ? _dbContext.CustomerSignatures.AsNoTracking()
            : _dbContext.CustomerSignatures.AsQueryable();

        return query
            .OrderByDescending(entity => entity.SignedAtUtc)
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ServiceRequestId == serviceRequestId,
                cancellationToken);
    }

    public Task AddCustomerSignatureAsync(CustomerSignature customerSignature, CancellationToken cancellationToken)
    {
        return _dbContext.CustomerSignatures.AddAsync(customerSignature, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<PartsRequest>> GetPartsRequestsAsync(long serviceRequestId, bool asNoTracking, CancellationToken cancellationToken)
    {
        IQueryable<PartsRequest> query = asNoTracking
            ? _dbContext.PartsRequests.AsNoTracking()
            : _dbContext.PartsRequests.AsQueryable();

        return await query
            .Include(entity => entity.Items.Where(item => !item.IsDeleted))
            .Where(entity => !entity.IsDeleted && entity.ServiceRequestId == serviceRequestId)
            .OrderByDescending(entity => entity.SubmittedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddPartsRequestAsync(PartsRequest partsRequest, CancellationToken cancellationToken)
    {
        return _dbContext.PartsRequests.AddAsync(partsRequest, cancellationToken).AsTask();
    }
}
