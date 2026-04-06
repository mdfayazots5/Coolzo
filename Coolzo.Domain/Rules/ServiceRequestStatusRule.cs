using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Rules;

public static class ServiceRequestStatusRule
{
    public static bool CanTransition(ServiceRequestStatus currentStatus, ServiceRequestStatus targetStatus)
    {
        return currentStatus switch
        {
            ServiceRequestStatus.New => targetStatus == ServiceRequestStatus.Assigned ||
                targetStatus == ServiceRequestStatus.Cancelled,
            ServiceRequestStatus.Assigned => targetStatus == ServiceRequestStatus.EnRoute ||
                targetStatus == ServiceRequestStatus.NoShow ||
                targetStatus == ServiceRequestStatus.Rescheduled ||
                targetStatus == ServiceRequestStatus.Cancelled,
            ServiceRequestStatus.EnRoute => targetStatus == ServiceRequestStatus.Reached ||
                targetStatus == ServiceRequestStatus.NoShow ||
                targetStatus == ServiceRequestStatus.Rescheduled,
            ServiceRequestStatus.Reached => targetStatus == ServiceRequestStatus.WorkStarted ||
                targetStatus == ServiceRequestStatus.CustomerAbsent ||
                targetStatus == ServiceRequestStatus.NoShow ||
                targetStatus == ServiceRequestStatus.Rescheduled,
            ServiceRequestStatus.WorkStarted => targetStatus == ServiceRequestStatus.WorkInProgress ||
                targetStatus == ServiceRequestStatus.Cancelled,
            ServiceRequestStatus.WorkInProgress => targetStatus == ServiceRequestStatus.WorkCompletedPendingSubmission ||
                targetStatus == ServiceRequestStatus.Cancelled,
            ServiceRequestStatus.WorkCompletedPendingSubmission => targetStatus == ServiceRequestStatus.SubmittedForClosure,
            ServiceRequestStatus.NoShow => targetStatus == ServiceRequestStatus.Assigned ||
                targetStatus == ServiceRequestStatus.Rescheduled,
            ServiceRequestStatus.CustomerAbsent => targetStatus == ServiceRequestStatus.Rescheduled ||
                targetStatus == ServiceRequestStatus.Cancelled,
            ServiceRequestStatus.Rescheduled => targetStatus == ServiceRequestStatus.Assigned,
            _ => false
        };
    }
}
