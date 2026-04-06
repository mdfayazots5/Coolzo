namespace Coolzo.Domain.Enums;

public enum ServiceRequestStatus
{
    New = 1,
    Assigned = 2,
    EnRoute = 3,
    Reached = 4,
    WorkStarted = 5,
    WorkInProgress = 6,
    WorkCompletedPendingSubmission = 7,
    SubmittedForClosure = 8,
    Cancelled = 9,
    Rescheduled = 10,
    NoShow = 11,
    CustomerAbsent = 12
}
