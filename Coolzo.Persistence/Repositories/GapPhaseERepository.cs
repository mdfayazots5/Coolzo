using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class GapPhaseERepository : IGapPhaseERepository
{
    private readonly CoolzoDbContext _dbContext;

    public GapPhaseERepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> TechnicianMobileExistsAsync(string mobileNumber, long? excludedTechnicianId, CancellationToken cancellationToken)
    {
        return _dbContext.Technicians.AnyAsync(
            entity => !entity.IsDeleted &&
                entity.MobileNumber == mobileNumber &&
                (!excludedTechnicianId.HasValue || entity.TechnicianId != excludedTechnicianId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<Technician>> SearchTechniciansAsync(string? searchTerm, int? branchId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Technicians
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted);

        if (branchId.HasValue)
        {
            query = query.Where(entity => entity.BranchId == branchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(
                entity => entity.TechnicianCode.Contains(searchTerm) ||
                    entity.TechnicianName.Contains(searchTerm) ||
                    entity.MobileNumber.Contains(searchTerm) ||
                    entity.EmailAddress.Contains(searchTerm));
        }

        return await query
            .OrderBy(entity => entity.TechnicianName)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Technician?> GetTechnicianAsync(long technicianId, bool asNoTracking, CancellationToken cancellationToken)
    {
        var query = asNoTracking ? _dbContext.Technicians.AsNoTracking() : _dbContext.Technicians.AsQueryable();

        return query.FirstOrDefaultAsync(entity => entity.TechnicianId == technicianId && !entity.IsDeleted, cancellationToken);
    }

    public Task AddTechnicianDocumentAsync(TechnicianDocument technicianDocument, CancellationToken cancellationToken)
    {
        return _dbContext.TechnicianDocuments.AddAsync(technicianDocument, cancellationToken).AsTask();
    }

    public Task<TechnicianDocument?> GetTechnicianDocumentAsync(long technicianId, long technicianDocumentId, bool asNoTracking, CancellationToken cancellationToken)
    {
        var query = asNoTracking ? _dbContext.TechnicianDocuments.AsNoTracking() : _dbContext.TechnicianDocuments.AsQueryable();

        return query.FirstOrDefaultAsync(
            entity => entity.TechnicianId == technicianId &&
                entity.TechnicianDocumentId == technicianDocumentId &&
                !entity.IsDeleted,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<TechnicianDocument>> GetTechnicianDocumentsAsync(long technicianId, CancellationToken cancellationToken)
    {
        return await _dbContext.TechnicianDocuments
            .AsNoTracking()
            .Where(entity => entity.TechnicianId == technicianId && !entity.IsDeleted)
            .OrderBy(entity => entity.DocumentType)
            .ThenBy(entity => entity.TechnicianDocumentId)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddSkillAssessmentAsync(SkillAssessment skillAssessment, CancellationToken cancellationToken)
    {
        return _dbContext.SkillAssessments.AddAsync(skillAssessment, cancellationToken).AsTask();
    }

    public Task<SkillAssessment?> GetSkillAssessmentAsync(long technicianId, long skillAssessmentId, bool asNoTracking, CancellationToken cancellationToken)
    {
        var query = asNoTracking ? _dbContext.SkillAssessments.AsNoTracking() : _dbContext.SkillAssessments.AsQueryable();

        return query.FirstOrDefaultAsync(
            entity => entity.TechnicianId == technicianId &&
                entity.SkillAssessmentId == skillAssessmentId &&
                !entity.IsDeleted,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<SkillAssessment>> GetSkillAssessmentsAsync(long technicianId, CancellationToken cancellationToken)
    {
        return await _dbContext.SkillAssessments
            .AsNoTracking()
            .Where(entity => entity.TechnicianId == technicianId && !entity.IsDeleted)
            .OrderByDescending(entity => entity.DateCreated)
            .ThenByDescending(entity => entity.SkillAssessmentId)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddTrainingRecordAsync(TrainingRecord trainingRecord, CancellationToken cancellationToken)
    {
        return _dbContext.TrainingRecords.AddAsync(trainingRecord, cancellationToken).AsTask();
    }

    public Task<TrainingRecord?> GetTrainingRecordAsync(long technicianId, long trainingRecordId, bool asNoTracking, CancellationToken cancellationToken)
    {
        var query = asNoTracking ? _dbContext.TrainingRecords.AsNoTracking() : _dbContext.TrainingRecords.AsQueryable();

        return query.FirstOrDefaultAsync(
            entity => entity.TechnicianId == technicianId &&
                entity.TrainingRecordId == trainingRecordId &&
                !entity.IsDeleted,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<TrainingRecord>> GetTrainingRecordsAsync(long technicianId, CancellationToken cancellationToken)
    {
        return await _dbContext.TrainingRecords
            .AsNoTracking()
            .Where(entity => entity.TechnicianId == technicianId && !entity.IsDeleted)
            .OrderByDescending(entity => entity.DateCreated)
            .ThenByDescending(entity => entity.TrainingRecordId)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddTechnicianActivationLogAsync(TechnicianActivationLog technicianActivationLog, CancellationToken cancellationToken)
    {
        return _dbContext.TechnicianActivationLogs.AddAsync(technicianActivationLog, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<TechnicianActivationLog>> GetTechnicianActivationLogsAsync(long technicianId, CancellationToken cancellationToken)
    {
        return await _dbContext.TechnicianActivationLogs
            .AsNoTracking()
            .Where(entity => entity.TechnicianId == technicianId && !entity.IsDeleted)
            .OrderByDescending(entity => entity.ActivatedOnUtc)
            .ThenByDescending(entity => entity.TechnicianActivationLogId)
            .ToArrayAsync(cancellationToken);
    }

    public Task<bool> HelperCodeExistsAsync(string helperCode, long? excludedHelperProfileId, CancellationToken cancellationToken)
    {
        return _dbContext.HelperProfiles.AnyAsync(
            entity => !entity.IsDeleted &&
                entity.HelperCode == helperCode &&
                (!excludedHelperProfileId.HasValue || entity.HelperProfileId != excludedHelperProfileId.Value),
            cancellationToken);
    }

    public Task AddHelperProfileAsync(HelperProfile helperProfile, CancellationToken cancellationToken)
    {
        return _dbContext.HelperProfiles.AddAsync(helperProfile, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<HelperProfile>> SearchHelpersAsync(string? searchTerm, int? branchId, CancellationToken cancellationToken)
    {
        var query = _dbContext.HelperProfiles
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted);

        if (branchId.HasValue)
        {
            query = query.Where(entity => entity.BranchId == branchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(
                entity => entity.HelperCode.Contains(searchTerm) ||
                    entity.HelperName.Contains(searchTerm) ||
                    entity.MobileNo.Contains(searchTerm));
        }

        return await query
            .OrderBy(entity => entity.HelperName)
            .ToArrayAsync(cancellationToken);
    }

    public Task<HelperProfile?> GetHelperProfileAsync(long helperProfileId, bool asNoTracking, CancellationToken cancellationToken)
    {
        var query = asNoTracking ? _dbContext.HelperProfiles.AsNoTracking() : _dbContext.HelperProfiles.AsQueryable();

        return query.FirstOrDefaultAsync(entity => entity.HelperProfileId == helperProfileId && !entity.IsDeleted, cancellationToken);
    }

    public Task<HelperProfile?> GetHelperProfileByUserIdAsync(long userId, CancellationToken cancellationToken)
    {
        return _dbContext.HelperProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.UserId == userId && !entity.IsDeleted, cancellationToken);
    }

    public Task AddHelperAssignmentAsync(HelperAssignment helperAssignment, CancellationToken cancellationToken)
    {
        return _dbContext.HelperAssignments.AddAsync(helperAssignment, cancellationToken).AsTask();
    }

    public Task<HelperAssignment?> GetActiveHelperAssignmentAsync(long helperProfileId, bool asNoTracking, CancellationToken cancellationToken)
    {
        var query = BuildHelperAssignmentQuery(asNoTracking);

        return query.FirstOrDefaultAsync(
            entity => entity.HelperProfileId == helperProfileId &&
                !entity.IsDeleted &&
                entity.AssignmentStatus == "Assigned",
            cancellationToken);
    }

    public Task<HelperAssignment?> GetHelperAssignmentByIdAsync(long helperProfileId, long helperAssignmentId, bool asNoTracking, CancellationToken cancellationToken)
    {
        var query = BuildHelperAssignmentQuery(asNoTracking);

        return query.FirstOrDefaultAsync(
            entity => entity.HelperProfileId == helperProfileId &&
                entity.HelperAssignmentId == helperAssignmentId &&
                !entity.IsDeleted,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<HelperAssignment>> GetHelperAssignmentsAsync(long helperProfileId, CancellationToken cancellationToken)
    {
        return await BuildHelperAssignmentQuery(asNoTracking: true)
            .Where(entity => entity.HelperProfileId == helperProfileId && !entity.IsDeleted)
            .OrderByDescending(entity => entity.AssignedOnUtc)
            .ThenByDescending(entity => entity.HelperAssignmentId)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddHelperAttendanceAsync(HelperAttendance helperAttendance, CancellationToken cancellationToken)
    {
        return _dbContext.HelperAttendances.AddAsync(helperAttendance, cancellationToken).AsTask();
    }

    public Task<HelperAttendance?> GetOpenHelperAttendanceAsync(long helperProfileId, CancellationToken cancellationToken)
    {
        return _dbContext.HelperAttendances
            .FirstOrDefaultAsync(
                entity => entity.HelperProfileId == helperProfileId &&
                    !entity.IsDeleted &&
                    entity.CheckInOnUtc.HasValue &&
                    !entity.CheckOutOnUtc.HasValue,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<HelperAttendance>> GetHelperAttendancesAsync(long helperProfileId, CancellationToken cancellationToken)
    {
        return await _dbContext.HelperAttendances
            .AsNoTracking()
            .Where(entity => entity.HelperProfileId == helperProfileId && !entity.IsDeleted)
            .OrderByDescending(entity => entity.AttendanceDate)
            .ThenByDescending(entity => entity.HelperAttendanceId)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<HelperTaskChecklist>> GetHelperTaskChecklistsAsync(long? serviceTypeId, CancellationToken cancellationToken)
    {
        var query = _dbContext.HelperTaskChecklists
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted && entity.IsPublished);

        if (serviceTypeId.HasValue)
        {
            query = query.Where(entity => entity.ServiceTypeId == null || entity.ServiceTypeId == serviceTypeId.Value);
        }
        else
        {
            query = query.Where(entity => entity.ServiceTypeId == null);
        }

        return await query
            .OrderBy(entity => entity.SortOrder)
            .ThenBy(entity => entity.HelperTaskChecklistId)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddHelperTaskResponseAsync(HelperTaskResponse helperTaskResponse, CancellationToken cancellationToken)
    {
        return _dbContext.HelperTaskResponses.AddAsync(helperTaskResponse, cancellationToken).AsTask();
    }

    public Task<HelperTaskResponse?> GetHelperTaskResponseAsync(long helperAssignmentId, long helperTaskChecklistId, bool asNoTracking, CancellationToken cancellationToken)
    {
        var query = asNoTracking ? _dbContext.HelperTaskResponses.AsNoTracking() : _dbContext.HelperTaskResponses.AsQueryable();

        return query.FirstOrDefaultAsync(
            entity => entity.HelperAssignmentId == helperAssignmentId &&
                entity.HelperTaskChecklistId == helperTaskChecklistId &&
                !entity.IsDeleted,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<HelperTaskResponse>> GetHelperTaskResponsesAsync(long helperAssignmentId, CancellationToken cancellationToken)
    {
        return await _dbContext.HelperTaskResponses
            .AsNoTracking()
            .Where(entity => entity.HelperAssignmentId == helperAssignmentId && !entity.IsDeleted)
            .OrderBy(entity => entity.HelperTaskChecklistId)
            .ToArrayAsync(cancellationToken);
    }

    public Task<ServiceRequest?> GetServiceRequestAsync(long serviceRequestId, bool asNoTracking, CancellationToken cancellationToken)
    {
        var query = asNoTracking ? _dbContext.ServiceRequests.AsNoTracking() : _dbContext.ServiceRequests.AsQueryable();

        return query
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.BookingLines)
                    .ThenInclude(line => line.Service)
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.Customer)
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.CustomerAddress)
            .Include(entity => entity.JobCard)
            .FirstOrDefaultAsync(entity => entity.ServiceRequestId == serviceRequestId && !entity.IsDeleted, cancellationToken);
    }

    public Task<JobCard?> GetJobCardAsync(long jobCardId, bool asNoTracking, CancellationToken cancellationToken)
    {
        var query = asNoTracking ? _dbContext.JobCards.AsNoTracking() : _dbContext.JobCards.AsQueryable();

        return query.FirstOrDefaultAsync(entity => entity.JobCardId == jobCardId && !entity.IsDeleted, cancellationToken);
    }

    private IQueryable<HelperAssignment> BuildHelperAssignmentQuery(bool asNoTracking)
    {
        var query = asNoTracking ? _dbContext.HelperAssignments.AsNoTracking() : _dbContext.HelperAssignments.AsQueryable();

        return query
            .Include(entity => entity.Technician)
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.Booking)
                    .ThenInclude(booking => booking!.Customer)
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.Booking)
                    .ThenInclude(booking => booking!.CustomerAddress)
            .Include(entity => entity.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest!.Booking)
                    .ThenInclude(booking => booking!.BookingLines)
                        .ThenInclude(line => line.Service)
            .Include(entity => entity.JobCard);
    }
}
