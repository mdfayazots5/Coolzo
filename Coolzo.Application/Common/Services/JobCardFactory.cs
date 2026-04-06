using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Models;

namespace Coolzo.Application.Common.Services;

public sealed class JobCardFactory : IJobCardFactory
{
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IJobCardNumberGenerator _jobCardNumberGenerator;

    public JobCardFactory(
        IJobCardNumberGenerator jobCardNumberGenerator,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _jobCardNumberGenerator = jobCardNumberGenerator;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public JobCard EnsureCreated(ServiceRequest serviceRequest)
    {
        if (serviceRequest.JobCard is not null)
        {
            return serviceRequest.JobCard;
        }

        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
        var ipAddress = _currentUserContext.IPAddress;
        var jobCard = new JobCard
        {
            JobCardNumber = _jobCardNumberGenerator.GenerateNumber(),
            ServiceRequestId = serviceRequest.ServiceRequestId,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        };

        foreach (var statusHistory in serviceRequest.StatusHistories
            .Where(history => !history.IsDeleted && history.Status != ServiceRequestStatus.New)
            .OrderBy(history => history.StatusDateUtc))
        {
            jobCard.ExecutionTimelines.Add(new JobExecutionTimeline
            {
                Status = statusHistory.Status,
                EventType = "StatusChanged",
                EventTitle = statusHistory.Status.ToString(),
                Remarks = statusHistory.Remarks,
                EventDateUtc = statusHistory.StatusDateUtc,
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = ipAddress
            });
        }

        serviceRequest.JobCard = jobCard;
        return jobCard;
    }
}
