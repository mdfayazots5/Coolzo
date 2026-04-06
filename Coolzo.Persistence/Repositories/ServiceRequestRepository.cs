using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class ServiceRequestRepository : IServiceRequestRepository
{
    private readonly CoolzoDbContext _dbContext;

    public ServiceRequestRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken)
    {
        return _dbContext.ServiceRequests.AddAsync(serviceRequest, cancellationToken).AsTask();
    }

    public Task<bool> ServiceRequestNumberExistsAsync(string serviceRequestNumber, CancellationToken cancellationToken)
    {
        return _dbContext.ServiceRequests.AnyAsync(
            entity => entity.ServiceRequestNumber == serviceRequestNumber && !entity.IsDeleted,
            cancellationToken);
    }

    public Task<ServiceRequest?> GetByIdAsync(long serviceRequestId, CancellationToken cancellationToken)
    {
        return BuildServiceRequestQuery(asNoTracking: true)
            .FirstOrDefaultAsync(entity => entity.ServiceRequestId == serviceRequestId, cancellationToken);
    }

    public Task<ServiceRequest?> GetByIdForUpdateAsync(long serviceRequestId, CancellationToken cancellationToken)
    {
        return BuildServiceRequestQuery(asNoTracking: false)
            .FirstOrDefaultAsync(entity => entity.ServiceRequestId == serviceRequestId, cancellationToken);
    }

    public Task<ServiceRequest?> GetByJobCardIdAsync(long jobCardId, CancellationToken cancellationToken)
    {
        return BuildServiceRequestQuery(asNoTracking: true)
            .FirstOrDefaultAsync(
                entity => entity.JobCard != null && entity.JobCard.JobCardId == jobCardId,
                cancellationToken);
    }

    public Task<ServiceRequest?> GetByJobCardIdForUpdateAsync(long jobCardId, CancellationToken cancellationToken)
    {
        return BuildServiceRequestQuery(asNoTracking: false)
            .FirstOrDefaultAsync(
                entity => entity.JobCard != null && entity.JobCard.JobCardId == jobCardId,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<ServiceRequest>> SearchAssignedJobsAsync(
        long technicianId,
        ServiceRequestStatus? currentStatus,
        DateOnly? slotDate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;

        return await BuildAssignedJobQuery(technicianId, currentStatus, slotDate)
            .OrderBy(entity => entity.CurrentStatus)
            .ThenBy(entity => entity.Booking != null && entity.Booking.SlotAvailability != null
                ? entity.Booking.SlotAvailability.SlotDate
                : DateOnly.FromDateTime(entity.ServiceRequestDateUtc))
            .ThenBy(entity => entity.ServiceRequestDateUtc)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountAssignedJobsAsync(
        long technicianId,
        ServiceRequestStatus? currentStatus,
        DateOnly? slotDate,
        CancellationToken cancellationToken)
    {
        return BuildAssignedJobQuery(technicianId, currentStatus, slotDate).CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ServiceRequest>> SearchAsync(
        long? bookingId,
        long? serviceId,
        ServiceRequestStatus? currentStatus,
        DateOnly? slotDate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;
        var query = ApplyFilters(bookingId, serviceId, currentStatus, slotDate);

        return await query
            .OrderByDescending(entity => entity.ServiceRequestDateUtc)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountSearchAsync(
        long? bookingId,
        long? serviceId,
        ServiceRequestStatus? currentStatus,
        DateOnly? slotDate,
        CancellationToken cancellationToken)
    {
        return ApplyFilters(bookingId, serviceId, currentStatus, slotDate).CountAsync(cancellationToken);
    }

    public async Task<OperationsDashboardSummaryView> GetDashboardSummaryAsync(CancellationToken cancellationToken)
    {
        var totalBookings = await _dbContext.Bookings.CountAsync(entity => !entity.IsDeleted, cancellationToken);
        var totalServiceRequests = await _dbContext.ServiceRequests.CountAsync(entity => !entity.IsDeleted, cancellationToken);
        var assignedServiceRequests = await _dbContext.ServiceRequestAssignments.CountAsync(
            entity => entity.IsActiveAssignment && !entity.IsDeleted,
            cancellationToken);
        var serviceRequests = await _dbContext.ServiceRequests
            .AsNoTracking()
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.SlotAvailability)
            .Include(entity => entity.Assignments)
                .ThenInclude(assignment => assignment.Technician)
            .Where(entity => !entity.IsDeleted)
            .ToArrayAsync(cancellationToken);
        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeStatuses = new[]
        {
            ServiceRequestStatus.EnRoute,
            ServiceRequestStatus.Reached,
            ServiceRequestStatus.WorkStarted,
            ServiceRequestStatus.WorkInProgress,
            ServiceRequestStatus.WorkCompletedPendingSubmission
        };
        var activeAssignments = serviceRequests
            .SelectMany(serviceRequest => serviceRequest.Assignments
                .Where(assignment => assignment.IsActiveAssignment && !assignment.IsDeleted)
                .Select(assignment => new
                {
                    ServiceRequest = serviceRequest,
                    Assignment = assignment
                }))
            .ToArray();
        var technicianMonitoring = activeAssignments
            .Where(entry => entry.Assignment.Technician != null)
            .GroupBy(entry => entry.Assignment.TechnicianId)
            .Select(group =>
            {
                var technician = group.First().Assignment.Technician!;
                var todayAssignedJobsCount = group.Count(entry =>
                    (entry.ServiceRequest.Booking?.SlotAvailability?.SlotDate ?? DateOnly.FromDateTime(entry.ServiceRequest.ServiceRequestDateUtc)) == currentDate);
                var activeJobs = group
                    .Where(entry => activeStatuses.Contains(entry.ServiceRequest.CurrentStatus))
                    .OrderBy(entry => entry.ServiceRequest.CurrentStatus)
                    .ThenBy(entry => entry.ServiceRequest.ServiceRequestDateUtc)
                    .ToArray();
                var currentActiveJob = activeJobs.FirstOrDefault()?.ServiceRequest;

                return new TechnicianMonitoringView(
                    technician.TechnicianId,
                    technician.TechnicianCode,
                    technician.TechnicianName,
                    todayAssignedJobsCount,
                    activeJobs.Length,
                    currentActiveJob?.JobCard?.JobCardNumber ?? currentActiveJob?.ServiceRequestNumber,
                    currentActiveJob?.CurrentStatus.ToString());
            })
            .OrderByDescending(entity => entity.ActiveJobsCount)
            .ThenByDescending(entity => entity.TodayAssignedJobsCount)
            .ThenBy(entity => entity.TechnicianName)
            .ToArray();

        return new OperationsDashboardSummaryView(
            totalBookings,
            totalServiceRequests,
            assignedServiceRequests,
            totalServiceRequests - assignedServiceRequests,
            serviceRequests.Count(entity => entity.CurrentStatus == ServiceRequestStatus.EnRoute),
            serviceRequests.Count(entity => entity.CurrentStatus == ServiceRequestStatus.Reached),
            serviceRequests.Count(entity => entity.CurrentStatus == ServiceRequestStatus.WorkStarted),
            serviceRequests.Count(entity => entity.CurrentStatus == ServiceRequestStatus.WorkInProgress),
            serviceRequests.Count(entity => entity.CurrentStatus == ServiceRequestStatus.SubmittedForClosure),
            technicianMonitoring.Count(entity => entity.ActiveJobsCount > 0),
            technicianMonitoring);
    }

    private IQueryable<ServiceRequest> ApplyFilters(
        long? bookingId,
        long? serviceId,
        ServiceRequestStatus? currentStatus,
        DateOnly? slotDate)
    {
        var query = BuildServiceRequestQuery(asNoTracking: true);

        if (bookingId.HasValue)
        {
            query = query.Where(entity => entity.BookingId == bookingId.Value);
        }

        if (serviceId.HasValue)
        {
            query = query.Where(entity => entity.Booking != null && entity.Booking.BookingLines.Any(line => line.ServiceId == serviceId.Value));
        }

        if (currentStatus.HasValue)
        {
            query = query.Where(entity => entity.CurrentStatus == currentStatus.Value);
        }

        if (slotDate.HasValue)
        {
            query = query.Where(
                entity => entity.Booking != null &&
                    entity.Booking.SlotAvailability != null &&
                    entity.Booking.SlotAvailability.SlotDate == slotDate.Value);
        }

        return query;
    }

    private IQueryable<ServiceRequest> BuildServiceRequestQuery(bool asNoTracking)
    {
        IQueryable<ServiceRequest> query = _dbContext.ServiceRequests
            .AsSplitQuery()
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.Customer)
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.CustomerAddress)
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.SlotAvailability)
                    .ThenInclude(slotAvailability => slotAvailability!.SlotConfiguration)
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.BookingLines)
                    .ThenInclude(line => line.Service)
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.BookingLines)
                    .ThenInclude(line => line.AcType)
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.BookingLines)
                    .ThenInclude(line => line.Tonnage)
            .Include(entity => entity.Booking)
                .ThenInclude(booking => booking!.BookingLines)
                    .ThenInclude(line => line.Brand)
            .Include(entity => entity.StatusHistories)
            .Include(entity => entity.Assignments)
                .ThenInclude(assignment => assignment.Technician)
                    .ThenInclude(technician => technician!.BaseZone)
            .Include(entity => entity.JobCard)
                .ThenInclude(jobCard => jobCard!.Quotations)
                    .ThenInclude(quotation => quotation.InvoiceHeader)
            .Include(entity => entity.JobCard)
                .ThenInclude(jobCard => jobCard!.JobDiagnosis)
                    .ThenInclude(diagnosis => diagnosis!.ComplaintIssueMaster)
            .Include(entity => entity.JobCard)
                .ThenInclude(jobCard => jobCard!.JobDiagnosis)
                    .ThenInclude(diagnosis => diagnosis!.DiagnosisResultMaster)
            .Include(entity => entity.JobCard)
                .ThenInclude(jobCard => jobCard!.ChecklistResponses)
                    .ThenInclude(response => response.ServiceChecklistMaster)
            .Include(entity => entity.JobCard)
                .ThenInclude(jobCard => jobCard!.Attachments)
            .Include(entity => entity.JobCard)
                .ThenInclude(jobCard => jobCard!.ExecutionNotes)
            .Include(entity => entity.JobCard)
                .ThenInclude(jobCard => jobCard!.ExecutionTimelines)
            .Include(entity => entity.AssignmentLogs)
                .ThenInclude(log => log.PreviousTechnician)
            .Include(entity => entity.AssignmentLogs)
                .ThenInclude(log => log.CurrentTechnician)
            .Where(entity => !entity.IsDeleted);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    private IQueryable<ServiceRequest> BuildAssignedJobQuery(
        long technicianId,
        ServiceRequestStatus? currentStatus,
        DateOnly? slotDate)
    {
        var query = BuildServiceRequestQuery(asNoTracking: true)
            .Where(entity => entity.Assignments.Any(
                assignment => assignment.TechnicianId == technicianId &&
                    assignment.IsActiveAssignment &&
                    !assignment.IsDeleted));

        if (currentStatus.HasValue)
        {
            query = query.Where(entity => entity.CurrentStatus == currentStatus.Value);
        }

        if (slotDate.HasValue)
        {
            query = query.Where(
                entity => entity.Booking != null &&
                    entity.Booking.SlotAvailability != null &&
                    entity.Booking.SlotAvailability.SlotDate == slotDate.Value);
        }

        return query;
    }
}
